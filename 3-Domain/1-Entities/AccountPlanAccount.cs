using _3_Domain._2_Enum_s;

namespace _3_Domain._1_Entities
{
    public class AccountPlanAccount
    {
        public int Id { get; set; }
        public int AccountPlanId { get; set; }
        public AccountPlansModel AccountPlan { get; set; }
        public string CostCenter { get; set; }
        public string Name { get; set; }
        public int? AccountPlanClassificationId { get; set; }
        public AccountPlanClassification? AccountPlanClassification { get; set; }
        public EAccountPlanAccountStatus Status { get; set; }
        public EAccountPlanAccountOrigin Origin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
