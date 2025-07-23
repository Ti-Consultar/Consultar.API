using _3_Domain._1_Entities;
using _3_Domain._2_Enum_s;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4_InfraData._1_Repositories
{
    public class AccountPlanClassificationRepository : GenericRepository<AccountPlanClassification>
    {
        private readonly CoreServiceDbContext _context;
        public AccountPlanClassificationRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<AccountPlanClassification>> GetByTypeClassification(int accountPlanId, ETypeClassification typeClassification)
        {
            var model = await _context.AccountPlanClassification
                .Where(c => c.TypeClassification == typeClassification && c.AccountPlanId == accountPlanId)
                .Include(a => a.AccountPlan)
                .OrderBy(c => c.TypeOrder)
                .Select(c => new AccountPlanClassification
                {
                    Id = c.Id,
                    AccountPlanId = c.AccountPlanId,
                    Name = c.Name,
                    TypeOrder = c.TypeOrder,
                    TypeClassification = c.TypeClassification
                })
                .ToListAsync();

            return model;
        }
        public async Task<List<AccountPlanClassification>> GetAllAsync(int accountPlanId)
        {
            var model = await _context.AccountPlanClassification
                .Where(c => c.AccountPlanId == accountPlanId)
                .OrderBy(c => c.TypeOrder)
                .Select(c => new AccountPlanClassification
                {
                    Id = c.Id,
                    AccountPlanId =c.AccountPlanId,
                    Name = c.Name,
                    TypeOrder = c.TypeOrder,
                    TypeClassification = c.TypeClassification
                })
                .ToListAsync();

            return model;
        }

        public async Task<List<AccountPlanClassification>> GetAllBytypeClassificationAsync(int accountPlanId, int typeClassification)
        {
            var model = await _context.AccountPlanClassification
                .Where(c => c.AccountPlanId == accountPlanId && (int)c.TypeClassification == typeClassification)
                .OrderBy(c => c.TypeOrder)
                .ToListAsync();

            return model;
        }

        public async Task<AccountPlanClassification> GetByAccountIdAndId(int accountPlanId, int id)
        {
            var model = await _context.AccountPlanClassification
                .Where(c => c.AccountPlanId == accountPlanId && c.Id == id)
                .OrderBy(c => c.TypeOrder)
                .Select(c => new AccountPlanClassification
                {
                    Id = c.Id,
                    AccountPlanId = c.AccountPlanId,
                    Name = c.Name,
                    TypeOrder = c.TypeOrder,
                    TypeClassification = c.TypeClassification
                })
                .FirstOrDefaultAsync();

            return model;
        }
        public async Task<List<AccountPlanClassification>> GetAllAfterTypeOrderAsync(int accountPlanId, int typeClassification, int typeOrder)
        {
            return await _context.AccountPlanClassification
                .Where(c =>
                    c.AccountPlanId == accountPlanId &&
                    (int)c.TypeClassification == typeClassification &&
                    c.TypeOrder >= typeOrder)
                .OrderBy(c => c.TypeOrder)
                .ToListAsync();
        }

        


        public async Task<List<AccountPlanClassification>> GetItemsToDecrementOrderAsync(int accountPlanId, ETypeClassification typeClassification, int oldOrder, int newOrder)
        {
            return await _context.AccountPlanClassification
                .Where(c => c.AccountPlanId == accountPlanId
                            && c.TypeClassification == typeClassification
                            && c.TypeOrder <= newOrder
                            && c.TypeOrder > oldOrder)
                .ToListAsync();
        }

        public async Task CreateBond(List<BalanceteDataAccountPlanClassification> models)
        {
            await _context.BalanceteDataAccountPlanClassification.AddRangeAsync(models);
            await _context.SaveChangesAsync();
        }
        public async Task<List<BalanceteDataAccountPlanClassification>> GetBond(int accountPlanId, int typeClassification)
        {
            var model = await _context.BalanceteDataAccountPlanClassification
                .Include(a => a.AccountPlanClassification)
                    .ThenInclude(apc => apc.TotalizerClassification)
                .Include(a => a.AccountPlanClassification)
                    .ThenInclude(apc => apc.AccountPlan)
                .Where(c =>
                    c.AccountPlanClassification.AccountPlanId == accountPlanId &&
                    (int)c.AccountPlanClassification.TypeClassification == typeClassification
                )
                .OrderBy(c => c.AccountPlanClassification.TypeOrder)
                .ToListAsync();

            return model;
        }

        public async Task<List<BalanceteDataAccountPlanClassification>> GetBondAtivo(int accountPlanId)
        {
            var model = await _context.BalanceteDataAccountPlanClassification
                .Include(a => a.AccountPlanClassification)
                    .ThenInclude(apc => apc.AccountPlan)
                .Where(c =>
                    c.AccountPlanClassification.AccountPlanId == accountPlanId &&
                    (int)c.AccountPlanClassification.TypeClassification == 1 
                   
                )
                .OrderBy(c => c.AccountPlanClassification.TypeOrder)
                .ToListAsync();

            return model;
        }

        public async Task<bool> ExistsAccountPlanClassification(int accountPlanId)
        {
            return await _context.AccountPlanClassification
                .AnyAsync(a => a.AccountPlanId == accountPlanId);
        }


        public async Task<List<BalanceteDataAccountPlanClassification>> GetBondMonth(int accountPlanId, int balanceteId,int typeClassification)
        {
            var model = await _context.BalanceteDataAccountPlanClassification
                .Include(a => a.AccountPlanClassification)
                    .ThenInclude(apc => apc.AccountPlan)
                .Where(c =>
                    c.AccountPlanClassification.AccountPlanId == accountPlanId &&
                    (int)c.AccountPlanClassification.TypeClassification == typeClassification
                )
                .OrderBy(c => c.AccountPlanClassification.TypeOrder)
                .ToListAsync();

            return model;
        }
    }
}