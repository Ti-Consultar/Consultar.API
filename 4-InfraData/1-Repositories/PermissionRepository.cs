using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _4_InfraData._1_Repositories
{
    public class PermissionRepository
    {
        private readonly CoreServiceDbContext _context;

        public PermissionRepository(CoreServiceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }


        public async Task<List<PermissionModel>> GetAll()
        {
            var model = await _context.Permissions
                .ToListAsync();

            return model;
        }
        public async Task<PermissionModel> GetById(int id)
        {
            var model = await _context.Permissions
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();

            return model;
        }
    }
}
