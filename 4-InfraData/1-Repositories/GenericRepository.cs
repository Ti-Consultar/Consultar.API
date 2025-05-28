using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _4_InfraData._1_Repositories
{
    public class GenericRepository<T> where T : class
    {
        private readonly CoreServiceDbContext _context;

        public GenericRepository(CoreServiceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Set<T>().AnyAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public async Task Update(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task DeletePermanently(int id)
        {
            var entity = await _context.Set<T>().FirstOrDefaultAsync(a => EF.Property<int>(a, "Id") == id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task Delete(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity != null)
            {
                var deletedProperty = entity.GetType().GetProperty("Deleted");
                if (deletedProperty != null)
                {
                    deletedProperty.SetValue(entity, true);
                    _context.Set<T>().Update(entity);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task Restore(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity != null)
            {
                var deletedProperty = entity.GetType().GetProperty("Deleted");
                if (deletedProperty != null)
                {
                    deletedProperty.SetValue(entity, false);
                    _context.Set<T>().Update(entity);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<T> GetById(int id)
        {
            return await _context.Set<T>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>()
                //.Where(e => EF.Property<bool>(e, "Deleted") == false)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<T>> GetAllDeletedAsync()
        {
            return await _context.Set<T>()
                //.Where(e => EF.Property<bool>(e, "Deleted") == true)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
