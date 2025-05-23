﻿using _3_Domain._1_Entities;
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
        public async Task<bool> CnpjExists(string cnpj)
        {
            return await _context.BusinessEntity.AnyAsync(be => be.Cnpj == cnpj);
        }
        public async Task Update(BusinessEntity entity)
        {
            _context.BusinessEntity.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var entity = await _context.BusinessEntity.FindAsync(id);
            if (entity != null)
            {
                entity.Deleted = true; // Marcar como deletado

                _context.BusinessEntity.Update(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task Restore(int id)
        {
            var entity = await _context.BusinessEntity.FindAsync(id);
            if (entity != null)
            {
                entity.Deleted = false; 

                _context.BusinessEntity.Update(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<BusinessEntity> GetById(int id)
        {
            return await _context.BusinessEntity
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<BusinessEntity>> GetAllAsync()
        {
            return await _context.BusinessEntity
                .Where(a => a.Deleted == false)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<BusinessEntity>> GetAllDeletedAsync()
        {
            return await _context.BusinessEntity
                .Where(a => a.Deleted == true)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
