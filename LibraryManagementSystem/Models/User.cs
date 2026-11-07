namespace LibraryManagementSystem.Models
{
    // Defines the types of users in the system
    public enum UserRole
    {
        Admin,
        RegularUser
    }

    // Represents a user in the library system
    public class User
    {
        // Unique identifier for each user
        public int Id { get; set; }

        // User's login name
        public string Username { get; set; } = string.Empty;

        // Stores the hashed password
        public string PasswordHash { get; set; } = string.Empty;

        // Defines the user's role
        public UserRole Role { get; set; }
    }
}