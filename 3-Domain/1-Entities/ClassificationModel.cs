using _3_Domain._2_Enum_s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
   public class ClassificationModel
    {
        public ClassificationModel()
        {
            
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int TypeOrder { get; set; }
        public ETypeClassification TypeClassification { get; set; }
        public int TotalizerClassificationTemplateId { get; set; }
        public TotalizerClassificationTemplate TotalizerClassificationTemplate { get; set; }

    }
}
