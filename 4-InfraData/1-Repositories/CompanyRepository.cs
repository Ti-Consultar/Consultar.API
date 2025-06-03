using _3_Domain._1_Entities;
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

        #region Company
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
            var company = await _context.Companies
                .Include(c => c.SubCompanies) // Inclui as SubCompanies
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
                throw new Exception("Empresa não encontrada.");

            // Marca a Company como deletada
            company.Deleted = true;

            // Marca todas as SubCompanies como deletadas
            if (company.SubCompanies != null)
            {
                foreach (var subCompany in company.SubCompanies)
                {
                    subCompany.Deleted = true;
                }
            }

            _context.Companies.Update(company);
            await _context.SaveChangesAsync();
        }
        public async Task RestoreCompany(int id)
        {
            var company = await _context.Companies
                .Include(c => c.SubCompanies) // Traz também as SubCompanies
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
                throw new Exception("Empresa não encontrada.");

            // Marca a Company como restaurada
            company.Deleted = false;

            // Marca todas as SubCompanies como restauradas
            if (company.SubCompanies != null)
            {
                foreach (var subCompany in company.SubCompanies)
                {
                    subCompany.Deleted = false;
                }
            }

            _context.Companies.Update(company);
            await _context.SaveChangesAsync();
        }
        public async Task<List<CompanyModel>> GetCompaniesByUserId(int userId, int groupId)
        {
            var companies = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId && cu.GroupId == groupId)
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.CompanyUsers)
                        .ThenInclude(cu => cu.Permission)
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.SubCompanies
                        .Where(sc => !sc.Deleted && sc.CompanyUsers.Any(cu => cu.UserId == userId))) // Filtra subempresas ativas e do usuário
                .Select(cu => cu.Company)
                .Where(c => !c.Deleted) // Filtra companies ativas
                .Distinct()
                .ToListAsync();

            return companies;
        }
        public async Task<List<CompanyModel>> GetDeletedCompaniesByUserId(int userId, int groupId)
        {
            var companies = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId && cu.GroupId == groupId)
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.CompanyUsers)
                        .ThenInclude(cu => cu.Permission)
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.SubCompanies
                        .Where(sc => sc.Deleted && sc.CompanyUsers.Any(cu => cu.UserId == userId))) // SubCompanies DELETADAS
                .Select(cu => cu.Company)
                .Where(c => c.Deleted) // Companies DELETADAS
                .Distinct()
                .ToListAsync();

            return companies;
        }
        public async Task<CompanyModel> GetCompanyByUserId(int id, int userId, int groupId)
        {
            var company = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId && cu.GroupId == groupId && cu.CompanyId == id)
                      .Include(cu => cu.Company)
                    .ThenInclude(c => c.BusinessEntity)
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.CompanyUsers)
                        .ThenInclude(cu => cu.Permission)
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.SubCompanies
                        .Where(sc => !sc.Deleted && sc.CompanyUsers.Any(cu => cu.UserId == userId))) // Filtra SubCompanies ativas
                .Select(cu => cu.Company)
                .Where(c => !c.Deleted) // Filtra Companies ativas
                .Distinct()
                .FirstOrDefaultAsync();

            return company;
        }
        public async Task<CompanyModel> GetDeletedCompanyByUserId(int id, int userId, int groupId)
        {
            var company = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId && cu.GroupId == groupId && cu.Id == id)
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.CompanyUsers)
                        .ThenInclude(cu => cu.Permission)
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.SubCompanies
                        .Where(sc => sc.Deleted && sc.CompanyUsers.Any(cu => cu.UserId == userId))) // Filtra SubCompanies deletadas
                .Select(cu => cu.Company)
                .Where(c => c.Deleted) // Filtra Companies deletadas
                .Distinct()
                .FirstOrDefaultAsync();

            return company;
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
                .Where(c => !c.Deleted) // Filtra Companies ativas
                .Skip(skip)  // Pula os primeiros 'skip' registros
                .Take(take)  // Limita os resultados a 'take' registros
                .ToListAsync();

            return companies;
        }
        public async Task<List<CompanyModel>> GetDeletedCompaniesByUserIdPaginated(int userId, int skip, int take)
        {
            var companies = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId)
                .Include(cu => cu.Permission)
                .Include(cu => cu.Company)
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.SubCompanies) // Inclui as subempresas
                .Select(cu => cu.Company)
                .Where(c => c.Deleted) // Filtra Companies deletadas
                .Skip(skip)  // Pula os primeiros 'skip' registros
                .Take(take)  // Limita os resultados a 'take' registros
                .ToListAsync();

            return companies;
        }
        public async Task<CompanyModel?> GetById(int? id)
        {
            var company = await _context.Companies
                .Where(c => c.Id == id) // Filtrando diretamente em Companies
                .Include(c => c.Group) // Inclui as subempresas
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

        public async Task<List<CompanyModel>> GetByUser(int userId)
        {
            var companies = await _context.CompanyUsers
                .AsNoTracking()
                .Where(cu => cu.UserId == userId && cu.CompanyId != null)
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.SubCompanies)
                        .ThenInclude(sc => sc.CompanyUsers) // Inclui os usuários nas SubCompanies
                            .ThenInclude(cu => cu.Permission) // Inclui a Permission das SubCompanies
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.Group)
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.CompanyUsers) // Inclui os usuários na Company
                        .ThenInclude(cu => cu.Permission) // Inclui a Permission dos usuários na Company
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.BusinessEntity) // Inclui dados do CNPJ
                .Select(cu => cu.Company)
                .ToListAsync();

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
        #endregion

        #region Company Users
        public async Task<CompanyUserModel> GetCompanyUser(int userId,int groupId, int? companyId, int? subCompanyId)
        {
            if(companyId is null  && subCompanyId is null)
            {
                var group = await _context.CompanyUsers
                .Where(cu => cu.GroupId == groupId && cu.UserId == userId)
                .FirstOrDefaultAsync();

                return group;
            }
            if(companyId != null && subCompanyId is null)
            {
                var model = await _context.CompanyUsers
               .Where(cu => cu.GroupId == groupId && cu.CompanyId == companyId && cu.UserId == userId)
               .FirstOrDefaultAsync();

                return model;
            }
            if(subCompanyId != null)
            {
                var model = await _context.CompanyUsers
              .Where(cu => cu.GroupId == groupId && cu.CompanyId == companyId && cu.SubCompanyId == subCompanyId && cu.UserId == userId)
              .FirstOrDefaultAsync();
                return model;
            }
            return null;
            
        }
        public async Task DeleteCompanyUser(int userId, int groupId, int? companyId, int? subCompanyId)
        {
            // Monta a query base
            var query = _context.CompanyUsers
                .Where(cu => cu.UserId == userId && cu.GroupId == groupId);

            // Refinamentos conforme parâmetros
            if (companyId.HasValue && !subCompanyId.HasValue)
            {
                query = query.Where(cu => cu.CompanyId == companyId.Value);
            }
            else if (subCompanyId.HasValue)
            {
                query = query.Where(cu =>
                    cu.CompanyId == companyId.Value &&
                    cu.SubCompanyId == subCompanyId.Value);
            }
            var toRemove = await query.ToListAsync();
            if (toRemove.Any())
            {
                _context.CompanyUsers.RemoveRange(toRemove);
                await _context.SaveChangesAsync();
            }
        }
        public async Task AddUserToCompany(int userId, int? companyId)
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
                    GroupId = company.Group.Id
                };

                await _context.CompanyUsers.AddAsync(companyUser);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Empresa não encontrada", nameof(companyId));
            }
        }
        public async Task AddUserToGroup(int userId, int groupId, int permissionId)
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(c => c.Id == groupId);

            if (group != null)
            {
                var companyUser = new CompanyUserModel
                {
                    UserId = userId,
                    PermissionId = permissionId,
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
        public async Task AddUserToCompany(int userId, int companyId,int groupId, int permissionId)
        {
            var companyUser = new CompanyUserModel
            {
                UserId = userId,
                CompanyId = companyId,
                PermissionId = permissionId,
                GroupId = groupId
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
        #endregion

        #region Exists 
        public async Task<bool> ExistsEditCompanyUser(int userId, int companyId, int groupId)
        {
            return await _context.CompanyUsers
                .AnyAsync(cu => cu.UserId == userId && cu.CompanyId == companyId && cu.GroupId == groupId && cu.PermissionId == 1);
        }
        public async Task<bool> ExistsCompanyUser(int userId, int? companyId, int groupId)
        {
            return await _context.CompanyUsers
                .AnyAsync(cu => cu.UserId == userId && cu.CompanyId == companyId && cu.GroupId == groupId);
        }
        public async Task<bool> ExistsGroupUser(int userId, int groupId)
        {
            return await _context.CompanyUsers
                .AnyAsync(cu => cu.UserId == userId && cu.GroupId == groupId);
        }
        public async Task<bool> ExistsCompanyUser(int userId, int companyId)
        {
            return await _context.CompanyUsers
                .AnyAsync(cu => cu.UserId == userId && cu.CompanyId == companyId);
        }
        #endregion

        #region SubCompany
        public async Task RestoreSubCompany(int companyId, int subcompanyId)
        {
            var subCompany = await _context.SubCompanies
                .FirstOrDefaultAsync(a => a.CompanyId == companyId && a.Id == subcompanyId);

            subCompany.Deleted = false;

            _context.SubCompanies.Update(subCompany);
            await _context.SaveChangesAsync();
        }
        public async Task<List<SubCompanyModel>> GetSubCompaniesByUserId(int userId)
        {
            var subCompanies = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId && cu.Company != null)
                .Include(cu => cu.Permission) // Inclui a permissão diretamente
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.SubCompanies
                        .Where(sc => !sc.Deleted) // Filtra subempresas ativas (Deleted == false)
                    )
                    .ThenInclude(sc => sc.BusinessEntity) // Inclui o BusinessEntity da SubCompany
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.SubCompanies)
                        .ThenInclude(sc => sc.CompanyUsers
                            .Where(cu => cu.UserId == userId) // Garante que o usuário está vinculado à subempresa
                        )
                        .ThenInclude(cu => cu.Permission) // Inclui a permissão da subempresa
                .SelectMany(cu => cu.Company.SubCompanies
                    .Where(sc => !sc.Deleted && sc.CompanyUsers.Any(cu => cu.UserId == userId))) // Garante a associação do usuário e SubCompany ativa
                .Distinct()
                .ToListAsync();

            return subCompanies;
        }
        public async Task<SubCompanyModel> GetSubCompanyByUserId(int id, int userId)
        {
            var companyUser = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId && cu.SubCompanyId == id)
                .Include(cu => cu.Permission)  // Inclui a permissão do usuário
                .Include(cu => cu.Company)     // Inclui a empresa associada
                    .ThenInclude(c => c.SubCompanies)  // Inclui as subempresas
                        .ThenInclude(sc => sc.BusinessEntity)  // Inclui a BusinessEntity da SubCompany
                .FirstOrDefaultAsync();

            // Verifica se encontrou o usuário e a subempresa está vinculada
            if (companyUser?.Company?.SubCompanies == null)
                return null;

            // Busca a SubCompany específica pelo ID
            var subCompany = companyUser.Company.SubCompanies
                .FirstOrDefault(sc => sc.Id == id && !sc.Deleted);

            return subCompany;
        }
        public async Task<SubCompanyModel> GetSubCompanyId(int userId, int companyId, int id)
        {
            var companyUser = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId && cu.SubCompanyId == id && cu.CompanyId == companyId)
                .Include(cu => cu.Permission)  // Inclui a permissão do usuário
                .Include(cu => cu.Company)     // Inclui a empresa associada
                    .ThenInclude(c => c.SubCompanies)  // Inclui as subempresas
                        .ThenInclude(sc => sc.BusinessEntity)  // Inclui a BusinessEntity da SubCompany
                .FirstOrDefaultAsync();

            // Verifica se encontrou o usuário e a subempresa está vinculada
            if (companyUser?.Company?.SubCompanies == null)
                return null;

            // Busca a SubCompany específica pelo ID
            var subCompany = companyUser.Company.SubCompanies
                .FirstOrDefault(sc => sc.Id == id && !sc.Deleted);

            return subCompany;
        }
        public async Task<List<SubCompanyModel>> GetSubCompaniesDeletedByUserId(int userId)
        {
            var subCompanies = await _context.CompanyUsers
                .Where(cu => cu.UserId == userId && cu.Company != null)
                .Include(cu => cu.Permission) // Inclui a permissão diretamente
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.SubCompanies
                        .Where(sc => sc.Deleted) // Filtra subempresas inativas
                    )
                    .ThenInclude(sc => sc.BusinessEntity) // Inclui o BusinessEntity da SubCompany
                .Include(cu => cu.Company)
                    .ThenInclude(c => c.SubCompanies)
                        .ThenInclude(sc => sc.CompanyUsers
                            .Where(cu => cu.UserId == userId) // Garante que o usuário está vinculado à subempresa
                        )
                        .ThenInclude(cu => cu.Permission) // Inclui a permissão da subempresa
                .SelectMany(cu => cu.Company.SubCompanies
                    .Where(sc => sc.CompanyUsers.Any(cu => cu.UserId == userId) && sc.Deleted)) // Garante a associação e o status de deletado
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
        public async Task<bool> ExistsEditSubCompanyUser(int userId, int companyId, int subCompanyId)
        {
            return await _context.CompanyUsers
                .AnyAsync(cu => cu.UserId == userId && cu.CompanyId == companyId && cu.SubCompanyId == subCompanyId && cu.PermissionId == 1);
        }
        public async Task<bool> ExistsSubCompanyUser(int userId, int? companyId, int subCompanyId)
        {
            return await _context.CompanyUsers
                .AnyAsync(cu => cu.UserId == userId && cu.CompanyId == companyId && cu.SubCompanyId == subCompanyId);
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
        public async Task<SubCompanyModel> GetSubCompanyById(int id)
        {
            var subCompanies = await _context.SubCompanies
                .Where(cu => cu.Id == id)
                .FirstOrDefaultAsync();

            return subCompanies;
        }
        public async Task DeleteSubCompany(int companyId, int subcompanyId)
        {
            var subCompany = await _context.SubCompanies
                .FirstOrDefaultAsync(a => a.CompanyId == companyId && a.Id == subcompanyId);

            subCompany.Deleted = true;

            _context.SubCompanies.Update(subCompany);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}
