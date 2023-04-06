using System.Collections.Generic;

namespace MyBlog.Utilities
{
    public class MenuList
    {
        public class MenuItem
        {
            public string Action { get; set; }
            public string Controller { get; set; }
            public string Name { get; set; }
            public string CurrentPage { get; set; }
            public string Role { get; set; }
            public string Icon { get; set; }
        }

        public static MenuItem[] Menu()
        {
            return new MenuItem[]
            {
                    new MenuItem
                    {
                        Action = "Create",
                        Controller = "Post",
                        Name = "Đăng bài",
                        CurrentPage = "CreatePost",
                        Role = "User, Manager, Admin"
                    },
                    new MenuItem
                    {
                        Action = "MyItems",
                        Controller = "Account",
                        Name = "Bài viết của tôi",
                        CurrentPage = "MyItems",
                        Role = "User, Manager, Admin"
                    },
                    new MenuItem
                    {
                        Action = "Create",
                        Controller = "Document",
                        Name = "Đăng tài liệu",
                        CurrentPage = "CreateDocument",
                        Role = "User, Manager, Admin"
                    },
                    new MenuItem
                    {
                        Action = "MyDocuments",
                        Controller = "Account",
                        Name = "Tài liệu của tôi",
                        CurrentPage = "MyDocuments",
                        Role = "User, Manager, Admin"
                    },
                    new MenuItem
                    {
                        Action = "MyCollections",
                        Controller = "Collections",
                        Name = "Bộ sưu tập",
                        CurrentPage = "MyCollections",
                        Role = "User"
                    },
                    new MenuItem
                    {
                        Action = "Setting",
                        Controller = "Account",
                        Name = "Thông tin tài khoản",
                        CurrentPage = "AccountSetting",
                        Role = "User, Manager, Admin"
                    },
                    new MenuItem
                    {
                        Action = "Manage",
                        Controller = "Categories",
                        Name = "Quản lí thể loại",
                        CurrentPage = "CategoriesManage",
                        Role = "Manager, Admin"
                    },
                    new MenuItem
                    {
                        Action = "Index",
                        Controller = "User",
                        Name = "Quản lí người dùng",
                        CurrentPage = "UserManage",
                        Role = "Admin"
                    },
                    new MenuItem
                    {
                        Action = "Index",
                        Controller = "Censor",
                        Name = "Quản lí nhân viên kiểm duyệt",
                        CurrentPage = "CensorManage",
                        Role = "Admin"
                    },
                    new MenuItem
                    {
                        Action = "Index",
                        Controller = "CensorShip",
                        Name = "Kiểm duyệt bài viết",
                        CurrentPage = "CensorShip",
                        Role = "Admin"
                    },
                    new MenuItem
                    {
                        Action = "Index",
                        Controller = "Statistic",
                        Name = "Thống kê báo cáo",
                        CurrentPage = "Statistic",
                        Role = "Admin"
                    }
            }; 
        }

        public static MenuItem[] AdminMenu()
        {
            return new MenuItem[]
            {
                    new MenuItem
                    {
                        Action = "Index",
                        Controller = "Statistic",
                        Name = "Thống kê báo cáo",
                        CurrentPage = "Statistic",
                        Role = "Admin, Manage, Censor"
                    },
                    new MenuItem
                    {
                        Action = "Manage",
                        Controller = "Categories",
                        Name = "Quản lí thể loại",
                        CurrentPage = "CategoriesManage",
                        Role = "Manager, Admin"
                    },
                    new MenuItem
                    {
                        Action = "Index",
                        Controller = "User",
                        Name = "Quản lí người dùng",
                        CurrentPage = "UserManage",
                        Role = "Admin"
                    },
                    new MenuItem
                    {
                        Action = "Index",
                        Controller = "Censor",
                        Name = "Nhân viên kiểm duyệt",
                        CurrentPage = "CensorManage",
                        Role = "Admin"
                    },
                    new MenuItem
                    {
                        Action = "Index",
                        Controller = "CensorShip",
                        Name = "Kiểm duyệt bài viết",
                        CurrentPage = "CensorShip",
                        Role = "Admin, Manage, Censor"
                    },
                    new MenuItem
                    {
                        Action = "Index",
                        Controller = "Report",
                        Name = "Xử lý báo cáo",
                        CurrentPage = "ReportManager",
                        Role = "Admin, Manage"
                    },

            };
        }


    }
}
