using _2___Application._2_Dto_s.Classification;
using _2___Application._2_Dto_s.TotalizerClassification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Classification
{
    public class ClassificationResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TypeClassification { get; set; }
        public int TypeOrder { get; set; }
    }
    public class ClassificationtesteResponse
    {
        public TotalizerClassificationTemplateResponse TotalizerClassification { get; set; }
        public List<TotalClassificationtesteResponse> Classifications { get; set; }

    }

    public class TotalClassificationtesteResponse
    {
        public ClassificationResponse Classifications { get; set; }

    }
}

    
