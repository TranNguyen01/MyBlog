using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Controllers;
using MyBlog.Models;
using MyBlog.Utilities;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Service
{
    public class NotifySvc : INotifySvc
    {
        private readonly AppDbContext _DbContext;

        private readonly ILogger<NotifySvc> _Logger;
        public NotifySvc(AppDbContext dbContext, ILogger<NotifySvc> logger)
        {
            _DbContext = dbContext;
            _Logger = logger;
        }


        public async Task<Notify> CreateNotify(Notify model)
        {
            _DbContext.Notify.Add(model);
            await _DbContext.SaveChangesAsync();
            return model;
        }

        public async Task<List<Notify>> CreateNotifyByGroup(List<string> userIds, int type, string title, string content, string link)
        {
            var now = DateTime.Now;
            List<Notify> notifies = new List<Notify>();
            foreach(string userId in userIds)
            {
                Notify notify = new Notify { 
                    UserId = userId, 
                    Type = type, Title = title, 
                    Content = content, Link = link, 
                    CreatedAt = now, 
                    UpdatedAt = now 
                };
                notifies.Add(notify);
               
            }
            await _DbContext.Notify.AddRangeAsync(notifies);
            await _DbContext.SaveChangesAsync();
            return notifies;
        }

        public async Task<List<Notify>> CreateNotifyByRole(string roleId, int type, string title, string content, string link)
        {
            var query = from user in _DbContext.Users
                        join userRole in _DbContext.UserRoles on user.Id equals userRole.UserId
                        join role in _DbContext.Roles on userRole.RoleId equals role.Id
                        where role.Id == roleId
                        select user;
            List<User> users = await query.ToListAsync();
            List<Notify> notifies = new List<Notify>();
            DateTime now = DateTime.Now;
            foreach(var user in users)
            {
                Notify newNotify = new Notify
                {
                    UserId = user.Id,
                    Type = type,
                    Title = title,
                    Content = content,
                    Link = link,
                    CreatedAt = now,
                    UpdatedAt = now,
                };
                notifies.Add(newNotify);
            }
            await _DbContext.AddRangeAsync(notifies);
            await _DbContext.SaveChangesAsync();
            return notifies;
        }

        public async Task<Notify> CreateNotifyByUserId(string userId, int type, string title, string content, string link)
        {
            DateTime now = DateTime.Now;
            Notify notify = new Notify
            {
                UserId = userId,
                Type = type,
                Title = title,
                Content = content,
                Link = link,
                CreatedAt = now,
                UpdatedAt = now,
            };
            _DbContext.Notify.Add(notify);
            await _DbContext.SaveChangesAsync();
            return notify;
        }

        public async Task<List<Notify>> GetListNotifyByUserId(string userId, string sortBy, string sortOrder, int page, int pageSize)
        {
            List<Notify> notifies = await _DbContext.Notify.Where(n => n.UserId == userId).ToListAsync();
            return notifies;
        }

        public Task<List<Notify>> GetListNotifyByUserId(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<PaginatedList<Notify>> GetListPagingNotifyByUserId(string userId, string sortBy, string sortOrder, int page, int pageSize)
        {
            var query = _DbContext.Notify.Where(n => n.UserId == userId);
            query = sortBy switch
            {
                "createdAt" => sortOrder == "asc" ? query.OrderBy(n => n.CreatedAt) : query.OrderByDescending(n => n.CreatedAt),
                "updatedAt" => sortOrder == "asc" ? query.OrderBy(n => n.UpdatedAt) : query.OrderByDescending(n => n.UpdatedAt),
                "title" => sortOrder == "asc" ? query.OrderBy(n => n.Title) : query.OrderByDescending(n => n.Title),
                "content" => sortOrder == "asc" ? query.OrderBy(n => n.Content) : query.OrderByDescending(n => n.Content),
                _ => sortOrder == "asc" ? query.OrderBy(n => n.Id) : query.OrderByDescending(n => n.Id)
            };
            return await PaginatedList<Notify>.CreatePaginatedList(query, page, pageSize);
        }

        public async Task<List<Notify>> SeenAllNotify(string userId)
        {
            List<Notify> notifies = await _DbContext.Notify.Where(n => n.UserId == userId && n.Seen == false).ToListAsync();
            DateTime now = DateTime.Now;
            foreach(var notify in notifies)
            {
                notify.Seen = true;
                notify.UpdatedAt = now;
                _DbContext.Entry(notify).State = EntityState.Modified;
            }
            await _DbContext.SaveChangesAsync();
            return notifies;
        }

        public async Task<Notify> SeenNotify(Guid id)
        {
            Notify notify = await _DbContext.Notify.FindAsync(id);
            if (notify == null)
                return null;
            notify.Seen = true;
            notify.UpdatedAt = DateTime.Now;
            _DbContext.Entry(notify).State = EntityState.Modified;
            await _DbContext.SaveChangesAsync();
            return notify;
        }
    }
}
