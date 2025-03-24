using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _4_InfraData._1_Repositories
{
    public class CompanyRepository
    {
        private readonly CoreServiceDbContext _context;

        public CompanyRepository(CoreServiceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Método para adicionar uma nova empresa
        public async Task AddCompany(CompanyModel companyModel)
        {
            if (companyModel == null)
                throw new ArgumentNullException(nameof(companyModel), "Company model cannot be null");

            await _context.Companies.AddAsync(companyModel);
            await _context.SaveChangesAsync();
        }

        // Método para buscar todas as subempresas associadas a um usuário específico
        public async Task<List<SubCompanyModel>> GetSubCompaniesByUserId(int userId)
        {
            var subCompanies = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId)
                .Include(cu => cu.Company)
                .ThenInclude(c => c.SubCompanies)
                .SelectMany(cu => cu.Company.SubCompanies)
                .ToListAsync();

            return subCompanies;
        }

        // Método para adicionar uma subempresa a uma empresa existente
        public async Task AddSubCompany(int companyId, SubCompanyModel subCompanyModel)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company != null)
            {
                subCompanyModel.CompanyId = companyId;
                await _context.SubCompanies.AddAsync(subCompanyModel);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Empresa não encontrada", nameof(companyId));
            }
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
        public async Task<List<CompanyModel>> GetCompaniesByUserId(int userId)
        {
            var companies = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId)
                .Include(cu => cu.Company)
                .Select(cu => cu.Company)
                .ToListAsync();

            return companies;
        }

        public async Task<List<CompanyModel>> GetById(int id)
        {
            var companies = await _context.CompanyUsers
                .Where(cu => cu.CompanyId == id)
                .Include(cu => cu.Company)
                .Select(cu => cu.Company)
                .ToListAsync();

            return companies;
        }

        // Método para adicionar um usuário à uma empresa
        public async Task AddUserToCompany(int userId, int companyId)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company != null)
            {
                var companyUser = new CompanyUserModel
                {
                    UserId = userId,
                    CompanyId = companyId
                };

                await _context.CompanyUsers.AddAsync(companyUser);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Empresa não encontrada", nameof(companyId));
            }
        }
    }
}
