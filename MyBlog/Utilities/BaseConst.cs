
namespace MyBlog.Utilities
{
    public static class BaseConst
    {
        public static class ResponseStatusCode
        {
            public static int NOT_FOUND = 1;
            public static int SUCCESS = 0;
            public static int UNAUTHORIZE = 2;
            public static int SYSTEM_ERROR = 3;
        }

        public static class Role
        {
            public static string ADMIN = "admin";
            public static string USER = "user";
            public static string CENSOR = "censor";
            public static string MANAGER = "manage";
        }
    }
}
