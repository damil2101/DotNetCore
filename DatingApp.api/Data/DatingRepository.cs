using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.api.Helpers;
using DatingApp.api.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.api.Data
{
    public class DatingRepository : IDatingRepository
    {
        public DatingRepository(DataContext context)
        {
            _context = context;
        }

        private DataContext _context { get; }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u=>u.LikerId == userId && u.LikeeId == recipientId);
        }

        public async Task<Photo> GetMainPhoto(int userId)
        {
            return await _context.Photos.Where(u=>u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            Photo photo = await _context.Photos.FirstOrDefaultAsync(photo=>photo.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            User user = await _context.Users.FirstOrDefaultAsync(x=>x.Id == id);
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users =  _context.Users.OrderByDescending(u=>u.LastActive).AsQueryable();
            
            //Do not return the currently logged in user and return users of the opposite gender
            users = users.Where(users=>users.Id != userParams.UserId).Where(u=>u.Gender == userParams.Gender);

            if(userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId,userParams.Likers);
                users = users.Where(u=>userLikers.Contains(u.Id));
            }

            if(userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId,userParams.Likers);
                users = users.Where(u=>userLikees.Contains(u.Id));
            }

            if(userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDOB = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDOB = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u=>u.DOB >= minDOB && u.DOB <= maxDOB);
            }

            if(!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {   
                    case "created":
                        users = users.OrderByDescending(u=>u.Created);
                        break;    
                    default:
                        users = users.OrderByDescending(u=>u.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users,userParams.PageNumber,userParams.PageSize);
        }
        
        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            User user = await _context.Users.FirstOrDefaultAsync(u=>u.Id == id);

            if(likers)
            {
                return user.Likers.Where(u=>u.LikeeId == id).Select(x=>x.LikerId);
            }
            else
            {
                return user.Likees.Where(u=>u.LikerId == id).Select(x=>x.LikeeId);
            }
        }
        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m=>m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages
                                        .AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(u=>u.RecipientId == messageParams.UserId && u.RecipientDeleted == false);
                    break;

                case "Outbox":
                    messages = messages.Where(u=>u.SenderId == messageParams.UserId && u.SenderDeleted == false);
                    break;    
                default:
                    messages = messages.Where(u=>u.RecipientId == messageParams.UserId && u.IsRead == false && u.RecipientDeleted == false);
                    break;
            }

            messages = messages.OrderByDescending(d=>d.MessageSent);
            return await PagedList<Message>.CreateAsync(messages,messageParams.PageNumber,messageParams.PageSize);                           
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
                                        .Where(m=>m.RecipientId == userId && m.RecipientDeleted == false && m.SenderId == recipientId 
                                            || m.RecipientId == recipientId && m.SenderId == userId && m.SenderDeleted == false) 
                                        .OrderByDescending(m=>m.MessageSent)
                                        .ToListAsync();
            return messages;                            
            

                                        
        }
    }
}