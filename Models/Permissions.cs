using static AngularAdminPannel.Models.Permissions;

namespace AngularAdminPannel.Models
{
    public class Permissions
    {
        public static class Users
        {
            public const string View = "Users.View";
            public const string Create = "Users.Create";
            public const string Edit = "Users.Edit";
            public const string Delete = "Users.Delete";
        }

        public static class Roles
        {
            public const string View = "Roles.View";
            public const string Create = "Roles.Create";
            public const string Edit = "Roles.Edit";
            public const string Delete = "Roles.Delete";
        }

        public static class CMS
        {
            public const string View = "CMS.View";
            public const string Create = "CMS.Create";
            public const string Edit = "CMS.Edit";
            public const string Delete = "CMS.Delete";
        }

        public static class EmailTemplate
        {
            public const string View = "EmailTemplate.View";
            public const string Create = "EmailTemplate.Create";
            public const string Edit = "EmailTemplate.Edit";
            public const string Delete = "EmailTemplate.Delete";
        }

        public static class Configration
        {
            public const string View = "Configration.View";
            public const string Create = "Configration.Create";
            public const string Edit = "Configration.Edit";
            public const string Delete = "Configration.Delete";
        }
        public static class FAQ
        {
            public const string View = "FAQ.View";
            public const string Create = "FAQ.Create";
            public const string Edit = "FAQ.Edit";
            public const string Delete = "FAQ.Delete";
        }
        public static class AuditLog
        {
            public const string View = "AuditLog.View";
            
        }

        public static List<string> GetAll()
        {
            return new List<string>
        {
            Users.View, Users.Create, Users.Edit, Users.Delete,
            Roles.View, Roles.Create, Roles.Edit, Roles.Delete,
            CMS.View, CMS.Create, CMS.Edit, CMS.Delete,
            EmailTemplate.View, EmailTemplate.Create, EmailTemplate.Edit, EmailTemplate.Delete,
            Configration.View, Configration.Create, Configration.Edit, Configration.Delete,
            FAQ.View, FAQ.Create, FAQ.Edit, FAQ.Delete,
            AuditLog.View


        };
        }
    }
}
