using _3_Domain._1_Entities;
using _3_Domain._2_Enum_s;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace _4_InfraData._1_Repositories
{
    public class InvitationRepository
    {
        private readonly CoreServiceDbContext _context;

        public InvitationRepository(CoreServiceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<InvitationToCompany>> GetAll()
        {
            return await _context.InvitationToCompany.ToListAsync();
        }
        public async Task<InvitationToCompany> GetExistingInvitation(int userId, int groupId, int? companyId = null, int? subCompanyId = null)
        {
            var predicate = BuildInvitationPredicate(userId, groupId, companyId, subCompanyId);

            return await _context.InvitationToCompany
                .AsNoTracking()
                .Where(predicate)
                .FirstOrDefaultAsync();
        }

        private Expression<Func<InvitationToCompany, bool>> BuildInvitationPredicate(
    int userId, int groupId, int? companyId, int? subCompanyId)
        {
            return i =>
                i.UserId == userId &&
                i.GroupId == groupId &&
                (companyId == null || i.CompanyId == companyId) &&
                (subCompanyId == null || i.SubCompanyId == subCompanyId);
        }

        public async Task<InvitationToCompany> GetById(int id)
        {
            return await _context.InvitationToCompany
                .Where(i => i.Id == id)
                .Include(i => i.Group)
                .Include(i => i.Company)
                .Include(i => i.SubCompany)
                .Include(i => i.User)
                .Include(i => i.InvitedBy)
                .Include(i => i.Permission)
                .FirstOrDefaultAsync();
        }

        public async Task<List<InvitationToCompany>> GetByUserId(int userId)
        {
            return await _context.InvitationToCompany
                .Where(i => i.UserId == userId)
                .Include(a => a.Group)
                .Include(a => a.Company)
                .Include(a => a.SubCompany)
                .Include(a => a.User)
                .Include(a => a.Permission)
                .Include(a => a.InvitedBy)
                .ToListAsync();
        }

        public async Task<InvitationToCompany> GetByUserId(int invitationId, int userId)
        {
            return await _context.InvitationToCompany
                .Where(i => i.Id == invitationId && i.UserId == userId)
                .Include(a => a.Group)
                .Include(a => a.Company)
                .Include(a => a.SubCompany)
                .Include(a => a.User)
                .Include(a => a.Permission)
                .Include(a => a.InvitedBy)
                .FirstOrDefaultAsync();
        }

        public async Task<List<InvitationToCompany>> GetInvitationsByInvitedById(int userId)
        {
            return await _context.InvitationToCompany
                .Where(i => i.InvitedById == userId && i.Status == InvitationStatus.Pending)
                .Include(a => a.Group)
                .Include(a => a.Company)
                .Include(a => a.SubCompany)
                .Include(a => a.User)
                .Include(a => a.Permission)
                .Include(a => a.InvitedBy)
                .ToListAsync();
        }

        public async Task Add(InvitationToCompany invitation)
        {
            try
            {
                await _context.InvitationToCompany.AddAsync(invitation);
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

        public async Task Update(InvitationToCompany invitation)
        {
            _context.InvitationToCompany.Update(invitation);
            await _context.SaveChangesAsync();
        }


        public async Task DeleteCompanyUser(int id)
        {
            // cria um stub só com o Id
            var stub = new InvitationToCompany { Id = id };

            // anexa ao contexto (sem buscar no banco)
            _context.InvitationToCompany.Attach(stub);

            // marca pra remoção
            _context.InvitationToCompany.Remove(stub);

            await _context.SaveChangesAsync();
        }



        public async Task Delete(int id)
        {
            var invitation = await GetById(id);
            if (invitation != null)
            {
                _context.InvitationToCompany.Remove(invitation);
                await _context.SaveChangesAsync();
            }
        }
    }
}
