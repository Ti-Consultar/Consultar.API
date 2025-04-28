using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using _4_InfraData._3_Utils;
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
                .Where(a => a.Deleted == false)
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
                .FirstOrDefaultAsync();
        }

        public async Task<GroupModel> GetByIdByCompanies(int id)
        {
            return await _context.Groups
                .Where(g => g.Id == id && !g.Deleted)
                .Include(g => g.BusinessEntity)
                .Include(g => g.Companies.Where(c => !c.Deleted)) // só Companies não deletadas
                    .ThenInclude(c => c.BusinessEntity)
                .Include(g => g.Companies.Where(c => !c.Deleted)) // precisa repetir para manter o filtro
                    .ThenInclude(c => c.CompanyUsers)
                        .ThenInclude(cu => cu.Permission)
                .FirstOrDefaultAsync();
        }
        public async Task<GroupModel> GetByIdByCompaniesDeleted(int id)
        {
            return await _context.Groups
                .Where(g => g.Id == id)
                .Include(g => g.BusinessEntity)
                .Include(g => g.Companies.Where(c => c.Deleted)) // Apenas Companies deletadas
                    .ThenInclude(c => c.BusinessEntity)
                .Include(g => g.Companies.Where(c => c.Deleted)) // Repetir para manter o filtro
                    .ThenInclude(c => c.CompanyUsers)
                        .ThenInclude(cu => cu.Permission)
                .FirstOrDefaultAsync();
        }

        public async Task<PaginatedResult<CompanyModel>> GetCompaniesByUserIdPaginatedAsync(int userId, int groupId, int skip, int take)
        {
            var group = await _context.Groups
                .Where(g => g.Id == groupId && !g.Deleted)
                .Include(g => g.Companies.Where(c => !c.Deleted)) // Só companies ativas
                    .ThenInclude(c => c.BusinessEntity)
                .Include(g => g.Companies.Where(c => !c.Deleted)) // Precisa repetir o filtro
                    .ThenInclude(c => c.CompanyUsers)
                .FirstOrDefaultAsync();

            if (group == null || group.Companies == null)
            {
                return new PaginatedResult<CompanyModel>
                {
                    TotalCount = 0,
                    Items = new List<CompanyModel>()
                };
            }

            var companiesQuery = group.Companies
                .Where(c => c.CompanyUsers != null && c.CompanyUsers.Any(cu => cu.UserId == userId))
                .AsQueryable();

            var totalCount = companiesQuery.Count();
            var companies = companiesQuery
                .Skip(skip)
                .Take(take)
                .ToList();

            return new PaginatedResult<CompanyModel>
            {
                TotalCount = totalCount,
                Items = companies
            };
        }



        public async Task<GroupModel> GetByCompanyId(int companyId)
        {
            return await _context.Groups
                .Where(a => a.Deleted == false)
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
                group.Deleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task Restore(int id)
        {
            var group = await GetById(id);
            if (group != null)
            {
                group.Deleted = false;
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

            // Busca todos os grupos ativos com includes
            var groups = await _context.Groups
                .Where(g => groupIds.Contains(g.Id) && !g.Deleted) // Só grupos ativos
                .Include(g => g.BusinessEntity)
                .Include(g => g.Companies.Where(c => !c.Deleted)) // Só Companies ativas
                    .ThenInclude(c => c.CompanyUsers)
                        .ThenInclude(cu => cu.Permission)
                .Include(g => g.Companies.Where(c => !c.Deleted)) // Precisa repetir pra continuar o filtro
                    .ThenInclude(c => c.SubCompanies.Where(sc => !sc.Deleted)) // Só SubCompanies ativas
                        .ThenInclude(sc => sc.CompanyUsers)
                            .ThenInclude(cu => cu.Permission)
                .ToListAsync();

            return groups;
        }

        public async Task<List<GroupModel>> GetGroupsDeletedByUserId(int userId)
        {
            // Busca todos os GroupIds distintos onde o usuário está vinculado em CompanyUsers
            var groupIds = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId)
                .Select(cu => cu.GroupId)
                .Distinct()
                .ToListAsync();

            // Busca todos os grupos ativos com includes
            var groups = await _context.Groups
                .Where(g => groupIds.Contains(g.Id) && g.Deleted) // Só grupos ativos
                .Include(g => g.BusinessEntity)
                .Include(g => g.Companies.Where(c => c.Deleted)) // Só Companies ativas
                    .ThenInclude(c => c.CompanyUsers)
                        .ThenInclude(cu => cu.Permission)
                .Include(g => g.Companies.Where(c => c.Deleted)) // Precisa repetir pra continuar o filtro
                    .ThenInclude(c => c.SubCompanies.Where(sc => sc.Deleted)) // Só SubCompanies ativas
                        .ThenInclude(sc => sc.CompanyUsers)
                            .ThenInclude(cu => cu.Permission)
                .ToListAsync();

            return groups;
        }

        public async Task<GroupModel?> GetGroupWithCompaniesById(int groupId, int userId)
        {
            var isUserInGroup = await _context.CompanyUsers
                .AnyAsync(cu => cu.UserId == userId && cu.GroupId == groupId);

            if (!isUserInGroup)
                return null;

            var group = await _context.Groups
                .Where(g => g.Id == groupId && !g.Deleted) // Filtra grupo ativo
                .Include(g => g.BusinessEntity)
                .Include(g => g.CompanyUsers)
                    .ThenInclude(cu => cu.Permission)
                .Include(g => g.Companies.Where(c => !c.Deleted)) // Só Companies ativas
                    .ThenInclude(c => c.CompanyUsers)
                        .ThenInclude(cu => cu.Permission)
                .Include(g => g.Companies.Where(c => !c.Deleted)) // Repetição para pegar BusinessEntity das Companies
                    .ThenInclude(c => c.BusinessEntity)
                .Include(g => g.Companies.Where(c => !c.Deleted)) // Repetição para SubCompanies
                    .ThenInclude(c => c.SubCompanies.Where(sc => !sc.Deleted)) // Só SubCompanies ativas
                        .ThenInclude(sc => sc.CompanyUsers)
                            .ThenInclude(cu => cu.Permission)
                .FirstOrDefaultAsync();

            return group;
        }


        public async Task<bool> UserHasManagerPermissionInGroup(int userId, int groupId)
        {
            return await _context.CompanyUsers
                .AnyAsync(cu => cu.UserId == userId && cu.GroupId == groupId && cu.PermissionId == 1);
        }





    }
}
