using Microsoft.AspNetCore.Http;

namespace _2___Application._2_Dto_s.AccountPlan
{
    public class AccountPlanAccountResponse
    {
        public int Id { get; set; }
        public int AccountPlanId { get; set; }
        public string CostCenter { get; set; }
        public string Name { get; set; }
        public int? AccountPlanClassificationId { get; set; }
        public string ClassificationStatus { get; set; }
        public string Origin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AccountPlanAccountListItemResponse
    {
        public int Id { get; set; }
        public int AccountPlanId { get; set; }
        public string CostCenter { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? AccountPlanClassificationId { get; set; }
        public string? AccountPlanClassificationName { get; set; }
        public string? AccountPlanClassificationType { get; set; }
        public string ClassificationStatus { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AccountPlanAccountListResponse
    {
        public int AccountPlanId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SourceMode { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public bool HasPendingClassifications { get; set; }
        public int PendingClassificationsCount { get; set; }
        public List<AccountPlanAccountListItemResponse> Accounts { get; set; } = new();
    }

    public class CreateAccountPlanAccountDto
    {
        public string CostCenter { get; set; }
        public string Name { get; set; }
    }

    public class ImportAccountPlanAccountsDto
    {
        public IFormFile File { get; set; }
    }

    public class ImportAccountPlanAccountsResponse
    {
        public string Message { get; set; }
        public int ImportedAccountsCount { get; set; }
        public int NewAccountsCount { get; set; }
        public int UpdatedAccountsCount { get; set; }
        public int RemovedAccountsCount { get; set; }
        public string SourceMode { get; set; }
        public List<AccountPlanAccountResponse> NewAccounts { get; set; } = new();
        public List<AccountPlanAccountResponse> RemovedAccounts { get; set; } = new();
    }

    public class UpdateAccountPlanSourceModeDto
    {
        public int SourceMode { get; set; }
    }
}
