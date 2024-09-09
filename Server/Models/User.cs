namespace Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }

        public ICollection<Emprunt> Emprunts { get; set; }
    }

    public enum Role
    {
        Admin = 1,
        User = 0,
    }
}