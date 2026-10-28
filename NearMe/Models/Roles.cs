namespace NearMe.Models
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Provider = "Provider";

        public static readonly string[] All = { Admin, User, Provider };
    }
}