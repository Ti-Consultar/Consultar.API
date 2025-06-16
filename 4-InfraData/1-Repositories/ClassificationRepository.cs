using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4_InfraData._1_Repositories
{
    public class ClassificationRepository : GenericRepository<ClassificationModel>
    {
        private readonly CoreServiceDbContext _context;
        public ClassificationRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

    }
}