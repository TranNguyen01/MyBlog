using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace MyBlog.Models.ViewModels
{
    public class ViewUserListModel
    {
        public int ITEMS_PERPAGE { get; set; } = 10;
        public int TotalPage { get; set; }

        public int currentPage { get; set; }

        public ICollection<User> Users { get; set; }

        public ICollection<IdentityRoleClaim<string>> RoleClaims { get; set; }
    }
}
