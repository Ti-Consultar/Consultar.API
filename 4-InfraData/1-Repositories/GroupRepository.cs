using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using _4_InfraData._3_Utils;
using _4_InfraData._6_Dto_sSQL;
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

        public async Task<List<CompanyUserModel>> GetUsersByGroupId(int id)
        {
            return await _context.CompanyUsers
            .Where(g => g.GroupId == id && g.CompanyId == null && g.SubCompanyId == null)
            .Include(g => g.User)
            .Include(a => a.Permission)
            .ToListAsync();

        }
        public async Task<List<CompanyUserModel>> GetUsersByCompanyId(int groupId, int companyId)
        {
            return await _context.CompanyUsers
            .Where(g => g.GroupId == groupId && g.CompanyId == companyId && g.SubCompanyId == null)
            .Include(g => g.User)
            .Include(a => a.Permission)
            .ToListAsync();

        }

        public async Task<List<CompanyUserModel>> GetUsersBySubCompanyId(int groupId, int companyId, int subCompanyId)
        {
            return await _context.CompanyUsers
            .Where(g => g.GroupId == groupId && g.CompanyId == companyId && g.SubCompanyId == subCompanyId)
            .Include(g => g.User)
            .Include(a => a.Permission)
            .ToListAsync();

        }
        public async Task<GroupModel> GetByIdByCompanies(int id)
        {
            var group = await _context.Groups
                .Where(g => g.Id == id && !g.Deleted)
                .Include(g => g.BusinessEntity)
                .Include(g => g.Companies)
                    .ThenInclude(c => c.BusinessEntity)
                .Include(g => g.Companies)
                    .ThenInclude(c => c.CompanyUsers)
                        .ThenInclude(cu => cu.Permission)
                .FirstOrDefaultAsync();

            if (group == null)
                return null;

            // Remove as Companies deletadas
            group.Companies = group.Companies.Where(c => !c.Deleted).ToList();

            return group;
        }



        public async Task<List<GroupCompanyDeletedDto>> GetByIdByCompaniesDeleted(int groupId)
        {
            var sql = @"
            SELECT 
                g.Id AS GroupId,
                g.Name AS GroupName,
                c.Id AS CompanyId,
                be.NomeFantasia AS CompanyName,
                be.Cnpj AS CompanyCnpj,
                cu.UserId,
                p.Id AS PermissionId,
                p.Name AS PermissionName
            FROM Groups g
            INNER JOIN CompanyUsers cu ON cu.GroupId = g.Id
            INNER JOIN Companies c ON c.Id = cu.CompanyId
            INNER JOIN BusinessEntity be ON be.Id = c.BusinessEntityId
            LEFT JOIN Permissions p ON p.Id = cu.PermissionId
            WHERE g.Id = {0}
              AND c.Deleted = 1";

            return await _context.Set<GroupCompanyDeletedDto>()
                .FromSqlRaw(sql, groupId)
                .ToListAsync();
        }

        public async Task<List<GroupSubCompanyDeletedDto>> GetByIdBySubCompaniesDeleted(int companyId)
        {
            var sql = @"
                        SELECT 
                            g.Id AS GroupId,
                            g.Name AS GroupName,
                            c.Id AS CompanyId,
                            c.Name AS CompanyName,
                            sc.Id AS SubCompanyId,
                            be.NomeFantasia AS SubCompanyName,
                            be.Cnpj AS SubCompanyCnpj,
                            cu.UserId,
                            p.Id AS PermissionId,
                            p.Name AS PermissionName
                        FROM Companies c
                        INNER JOIN Groups g ON g.Id = c.GroupId
                        INNER JOIN CompanyUsers cu ON cu.CompanyId = c.Id
                        INNER JOIN SubCompanies sc ON sc.CompanyId = c.Id
                        INNER JOIN BusinessEntity be ON be.Id = sc.BusinessEntityId
                        LEFT JOIN Permissions p ON p.Id = cu.PermissionId
                        WHERE c.Id = {0}
                          AND sc.Deleted = 1";

            return await _context.Set<GroupSubCompanyDeletedDto>()
                .FromSqlRaw(sql, companyId)
                .ToListAsync();
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

        public async Task<List<CompanyModel>> GetCompaniesByUserIdAndGroupId(int userId, int groupId)
        {
            var companies = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId && cu.Company != null && cu.GroupId == groupId) // Filtro para usuários vinculados a uma empresa do grupo
                .Include(cu => cu.Permission) // Inclui a permissão diretamente
                .Include(cu => cu.Company) // Inclui a Company vinculada ao usuário
                    .ThenInclude(c => c.BusinessEntity) // Inclui o BusinessEntity da Company
                .Include(cu => cu.Company) // Inclui novamente para garantir os CompanyUsers
                    .ThenInclude(c => c.CompanyUsers
                        .Where(cu => cu.UserId == userId) // Garante que o usuário está vinculado à empresa
                    )
                    .ThenInclude(cu => cu.Permission) // Inclui a permissão do usuário na empresa
                .Select(cu => cu.Company) // Seleciona diretamente as Companies
                .Distinct() // Remove duplicatas
                .ToListAsync();

            return companies;
        }
        public async Task<List<CompanyModel>> GetDeletedCompaniesByUserIdAndGroupId(int userId, int groupId)
        {
            var companies = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId && cu.Company != null && cu.GroupId == groupId) // Filtro para usuários vinculados a uma empresa do grupo
                .Include(cu => cu.Permission) // Inclui a permissão diretamente
                .Include(cu => cu.Company) // Inclui a Company vinculada ao usuário
                    .ThenInclude(c => c.BusinessEntity) // Inclui o BusinessEntity da Company
                .Include(cu => cu.Company) // Inclui novamente para garantir os CompanyUsers
                    .ThenInclude(c => c.CompanyUsers
                        .Where(cu => cu.UserId == userId) // Garante que o usuário está vinculado à empresa
                    )
                    .ThenInclude(cu => cu.Permission) // Inclui a permissão do usuário na empresa
                .Select(cu => cu.Company) // Seleciona diretamente as Companies
                .Where(c => c.Deleted) // Filtra apenas as empresas deletadas
                .Distinct() // Remove duplicatas
                .ToListAsync();

            return companies;
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
            var group = await _context.Groups
                .Include(g => g.Companies)
                    .ThenInclude(c => c.SubCompanies)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                throw new Exception("Grupo não encontrado.");

            group.Deleted = true;

            if (group.Companies != null)
            {
                foreach (var company in group.Companies)
                {
                    company.Deleted = true;

                    if (company.SubCompanies != null)
                    {
                        foreach (var subCompany in company.SubCompanies)
                        {
                            subCompany.Deleted = true;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task Restore(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Companies)
                    .ThenInclude(c => c.SubCompanies)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                throw new Exception("Grupo não encontrado.");

            group.Deleted = false;

            if (group.Companies != null)
            {
                foreach (var company in group.Companies)
                {
                    company.Deleted = false;

                    if (company.SubCompanies != null)
                    {
                        foreach (var subCompany in company.SubCompanies)
                        {
                            subCompany.Deleted = false;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
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
