using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using System;

namespace _4_InfraData._1_Repositories
{
    public class AccountPlansRepository : GenericRepository<AccountPlansModel>
    {
        private readonly CoreServiceDbContext _context;
        public AccountPlansRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> ExistsAccountPlanAsync(int groupId, int? companyId, int? subCompanyId)
        {
            return await _context.AccountPlans
                .AnyAsync(x =>
                    x.GroupId == groupId &&
                    x.CompanyId == companyId &&
                    (
                        (subCompanyId == null && x.SubCompanyId == null) ||
                        (subCompanyId != null && x.SubCompanyId == subCompanyId)
                    )
                );
        }
        public async Task<AccountPlansModel> GetByCompanyOrGroupId(int companyId, int groupId)
        {
            return await _context.AccountPlans
                .FirstOrDefaultAsync(x => x.CompanyId == companyId ||
                                         (x.CompanyId == null && x.GroupId == groupId));
        }
        public async Task<AccountPlansModel> GetBySubCompanyOrCompanyOrGroupId(int subCompanyId, int companyId, int groupId)
        {
            return await _context.AccountPlans
                .FirstOrDefaultAsync(x =>
                    x.SubCompanyId == subCompanyId ||
                    x.CompanyId == companyId ||
                    x.GroupId == groupId);
        }

        public async Task<AccountPlansModel> GetByGroupId(int groupId)
        {
            return await _context.AccountPlans
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.CompanyId == null);
        }
        public async Task<bool> ExistsAccountPlanByIdAsync(int id)
        {
            return await _context.AccountPlans
                .AnyAsync(x =>
                    x.Id == id
                );
        }
        public async Task<List<AccountPlansModel>> GetByFilters(int groupId, int? companyId, int? subCompanyId)
        {
            return await _context.AccountPlans
                .Include(x => x.Group)
                .Include(x => x.Company)
                .Include(x => x.SubCompany)
                .Where(x =>
                    x.GroupId == groupId &&
                    (
                        // Caso 1: Só GroupId (CompanyId e SubCompanyId são nulos no banco)
                        (companyId == null && subCompanyId == null && x.CompanyId == null && x.SubCompanyId == null)

                        // Caso 2: GroupId + CompanyId (SubCompanyId é nulo no banco)
                        || (companyId != null && subCompanyId == null && x.CompanyId == companyId && x.SubCompanyId == null)

                        // Caso 3: GroupId + CompanyId + SubCompanyId (registro completo)
                        || (companyId != null && subCompanyId != null && x.CompanyId == companyId && x.SubCompanyId == subCompanyId)
                    )
                )
                .ToListAsync();
        }
        public async Task<List<AccountPlansModel>> GetById(int id)
        {
            return await _context.AccountPlans
                .Include(x => x.Group)
                .Include(x => x.Company)
                .Include(x => x.SubCompany)
                .Where(x => x.Id == id)
                .ToListAsync();
        }
       
        public async Task<AccountPlansModel> GetByaccountPlanId(int id)
        {
            return await _context.AccountPlans
                .Include(x => x.Group)
                .Include(x => x.Company)
                .Include(x => x.SubCompany)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

    }
}
