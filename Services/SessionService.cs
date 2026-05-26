using sberbank.Model;

namespace sberbank.Services
{
    public static class SessionService
    {
        public static User CurrentUser { get; private set; }

        public static void SignIn(User user)
        {
            CurrentUser = user;
        }

        public static void SignOut()
        {
            CurrentUser = null;
        }
    }
}
