using Microsoft.EntityFrameworkCore.Storage;
using MyBlog.Models;
using MyBlog.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyBlog.Service
{
    public interface INotifySvc
    {
        Task<Notify> CreateNotify(Notify model);
        Task<List<Notify>> GetListNotifyByUserId(string userId);
        Task<PaginatedList<Notify>> GetListPagingNotifyByUserId(string userId, string sortBy, string sortOrder, int page, int pageSize);
        Task<List<Notify>> CreateNotifyByRole(string roleId, int type, string title, string content, string link);
        Task<List<Notify>> CreateNotifyByGroup(List<string> userId, int type, string title, string content, string link);
        Task<Notify> CreateNotifyByUserId(string userId, int type, string title, string content, string link);
        Task<Notify> SeenNotify(Guid id);
        Task<List<Notify>> SeenAllNotify(string userId);
     }
}
