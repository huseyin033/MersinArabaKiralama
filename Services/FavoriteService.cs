using MersinAracKiralama.Data;
using MersinAracKiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace MersinAracKiralama.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly ApplicationDbContext _context;
        public FavoriteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Favorite>> GetUserFavoritesAsync(string userId)
        {
            return await _context.Set<Favorite>()
                .Include(f => f.Car)
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

        public async Task AddAsync(Favorite fav)
        {
            _context.Add(fav);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(int id)
        {
            var fav = await _context.Set<Favorite>().FindAsync(id);
            if (fav != null)
            {
                _context.Remove(fav);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Favorite?> FindAsync(int carId, string userId)
        {
            return await _context.Set<Favorite>()
                .FirstOrDefaultAsync(f => f.CarId == carId && f.UserId == userId);
        }
    }
}
