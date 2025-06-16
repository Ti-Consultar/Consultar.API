using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._2_Dto_s.Classification;


namespace _2___Application._2_Dto_s.DRE
{
   public class DREResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Sequential { get; set; }
        public ClassificationResponse Classification { get; set; }
        public AccountPlanResponseSimple AccountPlan { get; set; }
    }

    public class AccountPlanResponseWithDREs
    {
        public int Id { get; set; }
        public List<ClassificationWithDREsResponse> Classifications { get; set; }
    }

    public class ClassificationWithDREsResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public List<DREResponseSimple> DREs { get; set; }
    }

    public class DREResponseSimple
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Sequential { get; set; }
    }


}
