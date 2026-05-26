namespace sberbank.Model
{
    public class User
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int? ClientId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public bool IsAdmin
        {
            get { return RoleName == "Admin" || RoleId == 1; }
        }
    }
}
