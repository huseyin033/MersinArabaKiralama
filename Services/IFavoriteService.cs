using MersinAracKiralama.Models;

namespace MersinAracKiralama.Services
{
    public interface IFavoriteService
    {
        Task<List<Favorite>> GetUserFavoritesAsync(string userId);
        Task AddAsync(Favorite fav);
        Task RemoveAsync(int id);
        Task<Favorite?> FindAsync(int carId, string userId);
    }
}
