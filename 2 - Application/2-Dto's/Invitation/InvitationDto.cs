namespace _2___Application._2_Dto_s.Invitation
{
    public class InvitationDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int CompanyId { get; set; }
        public int? SubCompanyId { get; set; }
        public int UserId { get; set; }
        public int InvitedByUserId { get; set; }
        public int PermissionId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

   
}
