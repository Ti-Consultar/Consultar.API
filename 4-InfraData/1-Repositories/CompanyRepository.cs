﻿using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace _4_InfraData._1_Repositories
{
    public class CompanyRepository
    {
        private readonly CoreServiceDbContext _context;

        public CompanyRepository(CoreServiceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }


        public async Task AddCompany(CompanyModel companyModel)
        {

            await _context.Companies.AddAsync(companyModel);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateCompany(CompanyModel companyModel)
        {
            _context.Companies.Update(companyModel);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteSubCompany(int companyId, int subcompanyId)
        {
            var subCompany = await _context.SubCompanies
                .FirstOrDefaultAsync(a => a.CompanyId == companyId && a.Id == subcompanyId);

            _context.SubCompanies.Remove(subCompany);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SubCompanyModel>> GetSubCompaniesByUserId(int userId)
        {
            var subCompanies = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId)
                .Include(cu => cu.Permission)
                .Include(cu => cu.Company)
                .ThenInclude(c => c.SubCompanies)
                .SelectMany(cu => cu.Company.SubCompanies)
                 .Distinct()
                .ToListAsync();

            return subCompanies;
        }


        public async Task AddSubCompany(int companyId, SubCompanyModel subCompanyModel)
        {
            subCompanyModel.CompanyId = companyId;
            await _context.SubCompanies.AddAsync(subCompanyModel);
            await _context.SaveChangesAsync();

        }

        public async Task UpdateSubCompany(SubCompanyModel subCompanyModel)
        {
            _context.SubCompanies.Update(subCompanyModel);
            await _context.SaveChangesAsync();
        }

        // Método para associar um usuário a uma subempresa
        public async Task AddUserToSubCompany(int userId, int subCompanyId)
        {
            var subCompany = await _context.SubCompanies
                .FirstOrDefaultAsync(sc => sc.Id == subCompanyId);

            if (subCompany != null)
            {
                var companyUser = new CompanyUserModel
                {
                    UserId = userId,
                    CompanyId = subCompany.CompanyId
                };

                await _context.CompanyUsers.AddAsync(companyUser);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Subempresa não encontrada", nameof(subCompanyId));
            }
        }

        // Método para buscar todas as empresas associadas a um usuário
        public async Task<List<CompanyModel>> GetCompaniesByUserId(int userId, int groupId)
        {
            var companies = await _context.CompanyUsers
     .Where(cu => cu.UserId == userId && cu.GroupId == groupId)
     .Include(cu => cu.Company)
         .ThenInclude(c => c.CompanyUsers)
         .ThenInclude(cu => cu.Permission)
     .Include(cu => cu.Company)
         .ThenInclude(c => c.SubCompanies
             .Where(sc => sc.CompanyUsers.Any(cu => cu.UserId == userId))) // Filtra subempresas no banco
     .Select(cu => cu.Company)
     .Distinct()
     .ToListAsync();

            return companies;
        }

        public async Task<CompanyModel> GetCompanyByUserId(int id, int userId, int groupId)
        {
            var companies = await _context.CompanyUsers
     .Where(cu => cu.UserId == userId && cu.GroupId == groupId && cu.Id == id)
     .Include(cu => cu.Company)
         .ThenInclude(c => c.CompanyUsers)
         .ThenInclude(cu => cu.Permission)
     .Include(cu => cu.Company)
         .ThenInclude(c => c.SubCompanies
             .Where(sc => sc.CompanyUsers.Any(cu => cu.UserId == userId))) // Filtra subempresas no banco
     .Select(cu => cu.Company)
     .Distinct()
     .FirstOrDefaultAsync();

            return companies;
        }
        public async Task<bool> ExistsEditCompanyUser(int userId, int companyId, int groupId)
        {
            return await _context.CompanyUsers
                .AnyAsync(cu => cu.UserId == userId && cu.CompanyId == companyId && cu.GroupId == groupId && cu.PermissionId == 1);
        }
        public async Task<bool> ExistsEditSubCompanyUser(int userId, int companyId, int subCompanyId)
        {
            return await _context.CompanyUsers
                .AnyAsync(cu => cu.UserId == userId && cu.CompanyId == companyId && cu.SubCompanyId == subCompanyId && cu.PermissionId == 1);
        }
        public async Task<bool> ExistsCompanyUser(int userId, int companyId, int groupId)
        {
            return await _context.CompanyUsers
                .AnyAsync(cu => cu.UserId == userId && cu.CompanyId == companyId && cu.GroupId == groupId);
        }

        public async Task<bool> ExistsCompanyUser(int userId, int companyId)
        {
            return await _context.CompanyUsers
                .AnyAsync(cu => cu.UserId == userId && cu.CompanyId == companyId);
        }
        public async Task<bool> ExistsSubCompanyUser(int userId, int companyId, int subCompanyId)
        {
            return await _context.CompanyUsers
                .AnyAsync(cu => cu.UserId == userId && cu.CompanyId == companyId && cu.SubCompanyId == subCompanyId);
        }

        public async Task<List<CompanyModel>> GetCompaniesByUserIdPaginated(int userId, int skip, int take)
        {
            var companies = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId)
                .Include(cu => cu.Permission)
                .Include(cu => cu.Company)
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.SubCompanies) // Inclui as subempresas
                .Select(cu => cu.Company)
                .Skip(skip)  // Pula os primeiros 'skip' registros
                .Take(take)  // Limita os resultados a 'take' registros
                .ToListAsync();

            return companies;
        }


        public async Task<CompanyModel?> GetById(int id)
        {
            var company = await _context.Companies
                .Where(c => c.Id == id) // Filtrando diretamente em Companies
                .Include(c => c.SubCompanies) // Inclui as subempresas
                .Include(c => c.CompanyUsers) // Inclui a relação com usuários
                    .ThenInclude(cu => cu.User) // Inclui os usuários
                .FirstOrDefaultAsync();

            return company;
        }


        public async Task<CompanyModel> GetByUserId(int userId)
        {
            var companies = await _context.CompanyUsers
                .AsNoTracking() // Evita rastreamento duplicado
                .Where(cu => cu.UserId == userId)
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.SubCompanies)
                .Include(cu => cu.User)
                .Select(cu => cu.Company)
                .FirstOrDefaultAsync();

            return companies;
        }

        public async Task<CompanyModel> GetSubCompanieByUserId(int subcompanyId)
        {
            var companies = await _context.CompanyUsers
                .Where(cu => cu.SubCompanyId == subcompanyId)
                .Include(cu => cu.Company)
                .ThenInclude(cu => cu.SubCompanies)
                .Include(cu => cu.User)
                .Select(cu => cu.Company)
                .FirstOrDefaultAsync();
            return companies;
        }

        public async Task<CompanyModel> GetCompanyById(int userId, int groupId, int companyId)
        {
            return await _context.Companies
                .Include(c => c.BusinessEntity)
                .Include(c => c.CompanyUsers)
                    .ThenInclude(cu => cu.Permission) // Isso aqui é o segredo pra trazer a Permission junto
                .Where(c =>
                    c.Id == companyId &&
                    c.GroupId == groupId &&
                    c.CompanyUsers.Any(cu => cu.UserId == userId && cu.GroupId == groupId))
                .FirstOrDefaultAsync();
        }



        public async Task<SubCompanyModel> GetSubCompanyById(int id)
        {
            var subCompanies = await _context.SubCompanies
                .Where(cu => cu.Id == id)
                .FirstOrDefaultAsync();

            return subCompanies;
        }

        public async Task AddUserToCompany(int userId, int? companyId, int groupId)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company != null)
            {
                var companyUser = new CompanyUserModel
                {
                    UserId = userId,
                    CompanyId = companyId,
                    PermissionId = 1,
                    GroupId = groupId
                };

                await _context.CompanyUsers.AddAsync(companyUser);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Empresa não encontrada", nameof(companyId));
            }
        }
        public async Task AddUserToGroup(int userId, int groupId)
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(c => c.Id == groupId);

            if (group != null)
            {
                var companyUser = new CompanyUserModel
                {
                    UserId = userId,
                    PermissionId = 1,
                    GroupId = groupId
                };

                await _context.CompanyUsers.AddAsync(companyUser);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Grupo não encontrado", nameof(groupId));
            }
        }
        public async Task AddUserToCompany(int userId, int companyId, int permissionId)
        {
            var companyUser = new CompanyUserModel
            {
                UserId = userId,
                CompanyId = companyId,
                PermissionId = permissionId
            };

            await _context.CompanyUsers.AddAsync(companyUser);
            await _context.SaveChangesAsync();

        }
        public async Task AddUserToCompanyOrSubCompany(int userId,int groupId ,int? companyId, int? subCompanyId, int permissionId)
        {
            try
            {
                var existingEntity = await _context.CompanyUsers
                    .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.CompanyId == companyId && cu.SubCompanyId == subCompanyId && cu.GroupId == groupId);

                if (existingEntity != null)
                {
                    _context.Entry(existingEntity).State = EntityState.Detached;
                    existingEntity.PermissionId = permissionId;
                    _context.CompanyUsers.Update(existingEntity);
                }
                else
                {
                    var companyUser = new CompanyUserModel
                    {
                        UserId = userId,
                        CompanyId = companyId,
                        SubCompanyId = subCompanyId,
                        PermissionId = permissionId,
                        GroupId = groupId
                    };

                    await _context.CompanyUsers.AddAsync(companyUser);
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? "Sem exceção interna";
                throw new Exception($"Erro ao salvar no banco. Detalhes: {innerExceptionMessage}", ex);
            }
        }

        public async Task<GroupModel> GetGroupWithCompaniesAndSubCompanies(int userId, int groupId)
        {
            var group = await _context.Groups
                .Where(g => g.Id == groupId)
                .Include(g => g.Companies
                    .Where(c => c.CompanyUsers.Any(cu => cu.UserId == userId)))
                    .ThenInclude(c => c.CompanyUsers)
                        .ThenInclude(cu => cu.Permission)
                .Include(g => g.Companies)
                    .ThenInclude(c => c.SubCompanies
                        .Where(sc => sc.CompanyUsers.Any(cu => cu.UserId == userId)))
                        .ThenInclude(sc => sc.CompanyUsers)
                            .ThenInclude(cu => cu.Permission)
                .FirstOrDefaultAsync();

            return group;
        }

        public async Task<CompanyUserModel> GetUserGroupPermission(int userId, int groupId)
        {
            return await _context.CompanyUsers
                .Include(cu => cu.Permission)
                .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.GroupId == groupId);
        }

    }
}
