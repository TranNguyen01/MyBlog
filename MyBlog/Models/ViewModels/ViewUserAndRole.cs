using System.Collections.Generic;

namespace MyBlog.Models.ViewModels
{
    public class ViewUserAndRole : User
    {
        public ICollection<string> Roles { get; set; }
    }
}
