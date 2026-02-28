namespace NotikaEmail_Identity.Models
{
    public class AssignRoleViewModel
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool RoleExists { get; set; }


    }
}
