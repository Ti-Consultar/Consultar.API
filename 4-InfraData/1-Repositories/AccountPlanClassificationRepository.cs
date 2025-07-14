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
        public async Task<List<ClassificationModel>> GetByTypeClassification(ETypeClassification typeClassification)
        {
            var model = await _context.AccountPlanClassification
                .Where(c => c.TypeClassification == typeClassification)
                .OrderBy(c => c.TypeOrder)
                .Select(c => new ClassificationModel
                {
                    Id = c.Id,
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



    }
}