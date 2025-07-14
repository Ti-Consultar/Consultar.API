using _3_Domain._1_Entities;
using _3_Domain._2_Enum_s;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
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
        public async Task<List<ClassificationModel>> GetByTypeClassification(ETypeClassification typeClassification)
        {
            var model = await _context.Classification
                .Where(c => c.TypeClassification == typeClassification)
                .OrderBy(c => c.TypeOrder)
                .Select(c => new ClassificationModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    TypeOrder = c.TypeOrder,
                    TypeClassification = c.TypeClassification
                })
                .ToListAsync();

            return model;
        }
      


    }
}