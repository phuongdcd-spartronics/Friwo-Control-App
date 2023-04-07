using FriwoControl.Models;

namespace FriwoControl
{
    public static class UserSession
    {
        public static User User { get; set; }

        public static bool HasRole(string role)
        {
            return User.Roles.Contains(role);
        }
    }
}
