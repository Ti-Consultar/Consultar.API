using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
                .Where(i => i.InvitedById == userId)
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
