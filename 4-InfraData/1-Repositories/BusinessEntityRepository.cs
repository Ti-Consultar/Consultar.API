using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _4_InfraData._1_Repositories
{
    public class BusinessEntityRepository
    {
        private readonly CoreServiceDbContext _context;

        public BusinessEntityRepository(CoreServiceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(BusinessEntity entity)
        {
            await _context.BusinessEntity.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BusinessEntity entity)
        {
            _context.BusinessEntity.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.BusinessEntity.FindAsync(id);
            if (entity != null)
            {
                _context.BusinessEntity.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<BusinessEntity> GetByIdAsync(int id)
        {
            return await _context.BusinessEntity
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<BusinessEntity>> GetAllAsync()
        {
            return await _context.BusinessEntity
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
