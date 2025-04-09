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
                 .Include(g => g.BusinessEntity)
                 .Include(g => g.Companies)
                   .ThenInclude(c => c.SubCompanies)
                 .ToListAsync();

        }

        public async Task<GroupModel> GetById(int id)
        {
            return await _context.Groups
                .Where(g => g.Id == id)
                .Include(g => g.BusinessEntity)
                .Include(g => g.Companies)
                    .ThenInclude(c => c.SubCompanies)
                .FirstOrDefaultAsync();
        }



        public async Task<GroupModel> GetByCompanyId(int companyId)
        {
            return await _context.Groups
                //    .Where(g => g.Companies.Any(c => c.Id == companyId))
                //  .Include(g => g.Companies)
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
        public async Task<List<GroupModel>> GetGroupsByUserId(int userId)
        {
            // Busca todos os GroupIds distintos onde o usuário está vinculado em CompanyUsers
            var groupIds = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId)
                .Select(cu => cu.GroupId)
                .Distinct()
                .ToListAsync();

            // Busca todos os grupos com os includes, baseado nos IDs únicos encontrados
            var groups = await _context.Groups
                .Where(g => groupIds.Contains(g.Id))
                .Include(g => g.BusinessEntity)
                .Include(g => g.Companies)
                    .ThenInclude(c => c.CompanyUsers)
                        .ThenInclude(cu => cu.Permission)
                .Include(g => g.Companies)
                    .ThenInclude(c => c.SubCompanies)
                        .ThenInclude(sc => sc.CompanyUsers)
                            .ThenInclude(cu => cu.Permission)
                .ToListAsync();

            return groups;
        }






    }
}
