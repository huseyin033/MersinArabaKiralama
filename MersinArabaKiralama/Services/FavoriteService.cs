using MersinArabaKiralama.Data;
using MersinArabaKiralama.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MersinArabaKiralama.Services
{
    public class FavoriteService(ApplicationDbContext context) : IFavoriteService
    {
        public async Task<List<Favorite>> GetUserFavoritesAsync(string userId)
        {
            return await context.Favorites
                                 .Where(f => f.UserId == userId)
                                 .Include(f => f.Car) // Arabayı da dahil et
                                 .ToListAsync();
        }

        public async Task<Favorite?> FindAsync(int carId, string userId)
        {
            return await context.Favorites.FirstOrDefaultAsync(f => f.CarId == carId && f.UserId == userId);
        }

        public async Task AddAsync(Favorite favorite)
        {
            context.Favorites.Add(favorite);
            await context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Favorite favorite)
        {
            context.Favorites.Remove(favorite);
            await context.SaveChangesAsync();
        }
    }
}