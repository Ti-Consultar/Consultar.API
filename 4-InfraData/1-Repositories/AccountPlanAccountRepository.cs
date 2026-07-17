using _3_Domain._1_Entities;
using _3_Domain._2_Enum_s;
using _4_InfraData._1_Context;
using _4_InfraData._3_Utils;
using Microsoft.EntityFrameworkCore;

namespace _4_InfraData._1_Repositories
{
    public class AccountPlanAccountRepository : GenericRepository<AccountPlanAccount>
    {
        private readonly CoreServiceDbContext _context;

        public AccountPlanAccountRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<AccountPlanAccount>> GetByAccountPlanIdAsync(int accountPlanId)
        {
            return await _context.AccountPlanAccount
                .AsNoTracking()
                .Where(x => x.AccountPlanId == accountPlanId)
                .OrderBy(x => x.CostCenter)
                .ToListAsync();
        }

        public async Task<PaginatedResult<AccountPlanAccount>> GetPaginatedByAccountPlanIdAsync(
            int accountPlanId,
            int skip,
            int take,
            string? search = null)
        {
            var query = _context.AccountPlanAccount
                .AsNoTracking()
                .Include(x => x.AccountPlanClassification)
                .Where(x => x.AccountPlanId == accountPlanId);

            var normalizedSearch = search?.Trim();
            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                query = query.Where(x =>
                    x.CostCenter.Contains(normalizedSearch) ||
                    x.Name.Contains(normalizedSearch));
            }

            var totalCount = await query.CountAsync();
            var accounts = await query
                .OrderBy(x => x.CostCenter)
                .ThenBy(x => x.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return new PaginatedResult<AccountPlanAccount>
            {
                TotalCount = totalCount,
                Items = accounts
            };
        }

        public async Task<AccountPlanAccount?> GetByAccountPlanAndCostCenterAsync(int accountPlanId, string costCenter)
        {
            var normalizedCostCenter = NormalizeCostCenter(costCenter);

            return await _context.AccountPlanAccount
                .FirstOrDefaultAsync(x => x.AccountPlanId == accountPlanId &&
                                          x.CostCenter == normalizedCostCenter);
        }

        public async Task<List<AccountPlanAccount>> GetPendingByAccountPlanIdAsync(int accountPlanId)
        {
            return await _context.AccountPlanAccount
                .AsNoTracking()
                .Where(x => x.AccountPlanId == accountPlanId &&
                            x.Status == EAccountPlanAccountStatus.PendingClassification)
                .OrderBy(x => x.CostCenter)
                .ToListAsync();
        }

        public async Task<int> CountPendingByAccountPlanIdAsync(int accountPlanId)
        {
            return await _context.AccountPlanAccount
                .CountAsync(x => x.AccountPlanId == accountPlanId &&
                                 x.Status == EAccountPlanAccountStatus.PendingClassification);
        }

        public async Task<bool> AnyByAccountPlanIdAsync(int accountPlanId)
        {
            return await _context.AccountPlanAccount
                .AnyAsync(x => x.AccountPlanId == accountPlanId);
        }

        public async Task<bool> DeleteAccountsAndClassificationsAsync(int accountPlanId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var accountPlanExists = await _context.AccountPlans
                    .AnyAsync(x => x.Id == accountPlanId);

                if (!accountPlanExists)
                    return false;

                // Exclui a tabela importada de conta e descrição.
                await _context.AccountPlanAccount
                    .Where(x => x.AccountPlanId == accountPlanId)
                    .ExecuteDeleteAsync();

                // Exclui vínculos e classificações antigas desse catálogo.
                await _context.BalanceteDataAccountPlanClassification
                    .Where(x => x.AccountPlanClassification.AccountPlanId == accountPlanId)
                    .ExecuteDeleteAsync();

                await _context.AccountPlanClassification
                    .Where(x => x.AccountPlanId == accountPlanId)
                    .ExecuteDeleteAsync();

                // Estruturas auxiliares são recriadas junto com as classificações.
                await _context.TotalizerClassification
                    .Where(x => x.AccountPlanId == accountPlanId)
                    .ExecuteDeleteAsync();

                await _context.BalancoReclassificado
                    .Where(x => x.AccountPlanId == accountPlanId)
                    .ExecuteDeleteAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<AccountPlanAccount>> UpsertFromBalanceteDataAsync(
            int accountPlanId,
            IEnumerable<BalanceteDataModel> balanceteData)
        {
            var importedAccounts = balanceteData
                .Where(x => !string.IsNullOrWhiteSpace(x.CostCenter))
                .GroupBy(x => NormalizeCostCenter(x.CostCenter))
                .Select(g => new
                {
                    CostCenter = g.Key,
                    Name = g.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name))?.Name?.Trim() ?? string.Empty
                })
                .ToList();

            if (!importedAccounts.Any())
                return new List<AccountPlanAccount>();

            var costCenters = importedAccounts.Select(x => x.CostCenter).ToList();
            var existing = await _context.AccountPlanAccount
                .Where(x => x.AccountPlanId == accountPlanId && costCenters.Contains(x.CostCenter))
                .ToListAsync();

            var existingByCostCenter = existing.ToDictionary(x => x.CostCenter, StringComparer.OrdinalIgnoreCase);
            var newAccounts = new List<AccountPlanAccount>();

            foreach (var importedAccount in importedAccounts)
            {
                if (existingByCostCenter.TryGetValue(importedAccount.CostCenter, out var account))
                {
                    if (!string.IsNullOrWhiteSpace(importedAccount.Name) && account.Name != importedAccount.Name)
                    {
                        account.Name = importedAccount.Name;
                        account.UpdatedAt = DateTime.UtcNow;
                    }

                    continue;
                }

                newAccounts.Add(new AccountPlanAccount
                {
                    AccountPlanId = accountPlanId,
                    CostCenter = importedAccount.CostCenter,
                    Name = importedAccount.Name,
                    Status = EAccountPlanAccountStatus.PendingClassification,
                    Origin = EAccountPlanAccountOrigin.BalanceteImport,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (newAccounts.Any())
                await _context.AccountPlanAccount.AddRangeAsync(newAccounts);

            await _context.SaveChangesAsync();
            return newAccounts;
        }

        public async Task<(List<AccountPlanAccount> NewAccounts, int UpdatedAccountsCount)> UpsertOfficialAccountsAsync(
            int accountPlanId,
            IEnumerable<AccountPlanAccount> accounts)
        {
            var result = await SynchronizeOfficialAccountsAsync(
                accountPlanId,
                accounts,
                removeMissingAccounts: false);

            return (result.NewAccounts, result.UpdatedAccountsCount);
        }

        public async Task<AccountPlanAccountSynchronizationResult> ReplaceOfficialAccountsAsync(
            int accountPlanId,
            IEnumerable<AccountPlanAccount> accounts)
        {
            return await SynchronizeOfficialAccountsAsync(
                accountPlanId,
                accounts,
                removeMissingAccounts: true);
        }

        private async Task<AccountPlanAccountSynchronizationResult> SynchronizeOfficialAccountsAsync(
            int accountPlanId,
            IEnumerable<AccountPlanAccount> accounts,
            bool removeMissingAccounts)
        {
            var importedAccounts = accounts
                .Where(x => !string.IsNullOrWhiteSpace(x.CostCenter))
                .GroupBy(x => NormalizeCostCenter(x.CostCenter))
                .Select(g => new
                {
                    CostCenter = g.Key,
                    Name = g.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name))?.Name?.Trim() ?? string.Empty,
                    Origin = g.First().Origin
                })
                .ToList();

            if (!importedAccounts.Any())
                return new AccountPlanAccountSynchronizationResult();

            var existing = await _context.AccountPlanAccount
                .Where(x => x.AccountPlanId == accountPlanId)
                .ToListAsync();

            var existingByCostCenter = existing.ToDictionary(x => x.CostCenter, StringComparer.OrdinalIgnoreCase);
            var importedCostCenters = importedAccounts
                .Select(x => x.CostCenter)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var newAccounts = new List<AccountPlanAccount>();
            var updatedAccountsCount = 0;

            foreach (var importedAccount in importedAccounts)
            {
                if (existingByCostCenter.TryGetValue(importedAccount.CostCenter, out var account))
                {
                    var changed = false;

                    if (!string.IsNullOrWhiteSpace(importedAccount.Name) && account.Name != importedAccount.Name)
                    {
                        account.Name = importedAccount.Name;
                        changed = true;
                    }

                    if (account.Origin != importedAccount.Origin)
                    {
                        account.Origin = importedAccount.Origin;
                        changed = true;
                    }

                    if (changed)
                    {
                        account.UpdatedAt = DateTime.UtcNow;
                        updatedAccountsCount++;
                    }

                    continue;
                }

                newAccounts.Add(new AccountPlanAccount
                {
                    AccountPlanId = accountPlanId,
                    CostCenter = importedAccount.CostCenter,
                    Name = importedAccount.Name,
                    Status = EAccountPlanAccountStatus.PendingClassification,
                    Origin = importedAccount.Origin,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (newAccounts.Any())
                await _context.AccountPlanAccount.AddRangeAsync(newAccounts);

            var removedAccounts = removeMissingAccounts
                ? existing.Where(x => !importedCostCenters.Contains(x.CostCenter)).ToList()
                : new List<AccountPlanAccount>();

            if (removedAccounts.Any())
                _context.AccountPlanAccount.RemoveRange(removedAccounts);

            await _context.SaveChangesAsync();
            return new AccountPlanAccountSynchronizationResult
            {
                NewAccounts = newAccounts,
                RemovedAccounts = removedAccounts,
                UpdatedAccountsCount = updatedAccountsCount
            };
        }

        public async Task EnsureFromBalanceteDataAsync(int accountPlanId)
        {
            var hasAccounts = await _context.AccountPlanAccount
                .AnyAsync(x => x.AccountPlanId == accountPlanId);

            if (hasAccounts)
                return;

            var existingBalanceteData = await _context.BalanceteData
                .Include(x => x.Balancete)
                .Where(x => x.Balancete.AccountPlansId == accountPlanId)
                .ToListAsync();

            await UpsertFromBalanceteDataAsync(accountPlanId, existingBalanceteData);
            await SyncClassificationsFromBondsAsync(accountPlanId);
        }

        public async Task SyncClassificationsFromBondsAsync(int accountPlanId)
        {
            var accounts = await _context.AccountPlanAccount
                .Where(x => x.AccountPlanId == accountPlanId)
                .ToListAsync();

            if (!accounts.Any())
                return;

            var bonds = await _context.BalanceteDataAccountPlanClassification
                .Include(x => x.AccountPlanClassification)
                .Where(x => x.AccountPlanClassification.AccountPlanId == accountPlanId)
                .ToListAsync();

            var bondByCostCenter = bonds
                .GroupBy(x => NormalizeCostCenter(x.CostCenter))
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var account in accounts)
            {
                if (bondByCostCenter.TryGetValue(account.CostCenter, out var bond))
                {
                    account.AccountPlanClassificationId = bond.AccountPlanClassificationId;
                    account.Status = EAccountPlanAccountStatus.Classified;
                }
                else
                {
                    account.AccountPlanClassificationId = null;
                    account.Status = EAccountPlanAccountStatus.PendingClassification;
                }

                account.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SyncClassificationsFromBondListAsync(
            int accountPlanId,
            IEnumerable<BalanceteDataAccountPlanClassification> bonds)
        {
            var accounts = await _context.AccountPlanAccount
                .Where(x => x.AccountPlanId == accountPlanId)
                .ToListAsync();

            var bondByCostCenter = bonds
                .GroupBy(x => NormalizeCostCenter(x.CostCenter))
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var account in accounts)
            {
                if (bondByCostCenter.TryGetValue(account.CostCenter, out var bond))
                {
                    account.AccountPlanClassificationId = bond.AccountPlanClassificationId;
                    account.Status = EAccountPlanAccountStatus.Classified;
                }
                else
                {
                    account.AccountPlanClassificationId = null;
                    account.Status = EAccountPlanAccountStatus.PendingClassification;
                }

                account.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        private static string NormalizeCostCenter(string costCenter)
        {
            return costCenter.Trim();
        }
    }

    public class AccountPlanAccountSynchronizationResult
    {
        public List<AccountPlanAccount> NewAccounts { get; set; } = new();
        public List<AccountPlanAccount> RemovedAccounts { get; set; } = new();
        public int UpdatedAccountsCount { get; set; }
    }
}
