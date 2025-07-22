using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4_InfraData._1_Repositories
{
    public class BalancoReclassificadoRepository : GenericRepository<BalancoReclassificadoModel>
    {
        private readonly CoreServiceDbContext _context;
        public BalancoReclassificadoRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<BalancoReclassificadoModel>> GetByAccountPlanId(int accountPlanId)
        {
            return await _context.BalancoReclassificado
                .Where(x => x.AccountPlanId == accountPlanId)
                .ToListAsync();
        }

    }
}
