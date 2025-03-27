using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4_InfraData._1_Repositories
{
    public class UserRepository
    {
        private readonly CoreServiceDbContext _context;

        public UserRepository(CoreServiceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<UserModel> Get(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email && x.Password == password);
            return user;
        }
        public async Task AddUser(UserModel model)
        {
            await _context.Users.AddAsync(model);
            await _context.SaveChangesAsync();
        }

        public async Task ResetPassword(UserModel model)
        {
            _context.Users.Update(model);
            await _context.SaveChangesAsync();
        }


        public async Task<UserModel> GetByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            return user;
        }

        public async Task<UserModel> GetByUserId(int userId)
        {
            return await _context.Users
                .Include(u => u.CompanyUsers)
                    .ThenInclude(cu => cu.Company)
                        .ThenInclude(c => c.SubCompanies) 
                .FirstOrDefaultAsync(u => u.Id == userId);
        }



    }
}
