using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            User user = await _context.Users.Include(x=>x.Photos).FirstOrDefaultAsync(x=>x.Id == id);
            return user;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            return await _context.Users.Include(u=>u.Photos).ToListAsync();
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}