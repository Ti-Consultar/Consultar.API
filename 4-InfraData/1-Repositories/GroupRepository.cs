using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace _4_InfraData._1_Repositories
{
    public class GroupRepository
    {
        private readonly CoreServiceDbContext _context;

        public GroupRepository(CoreServiceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<GroupModel>> GetAll()
        {
            return await _context.Groups
              //  .Include(g => g.Companies)
                .ToListAsync();
        }

        public async Task<GroupModel> GetById(int id)
        {
            return await _context.Groups
                .Where(g => g.Id == id)
                .Include(g => g.Companies)
                .FirstOrDefaultAsync();
        }

        public async Task<GroupModel> GetByCompanyId(int companyId)
        {
            return await _context.Groups
                .Where(g => g.Companies.Any(c => c.Id == companyId))
                .Include(g => g.Companies)
                .FirstOrDefaultAsync();
        }

        public async Task Add(GroupModel group)
        {
            try
            {
                await _context.Groups.AddAsync(group);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar no banco: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");

                throw;
            }
        }

        public async Task Update(GroupModel group)
        {
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var group = await GetById(id);
            if (group != null)
            {
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
            }
        }
    }
}
