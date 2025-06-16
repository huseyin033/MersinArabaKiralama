using MersinArabaKiralama.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MersinArabaKiralama.Services
{
    public interface IFavoriteService
    {
        Task<List<Favorite>> GetUserFavoritesAsync(string userId);
        Task<Favorite?> FindAsync(int carId, string userId);
        Task AddAsync(Favorite favorite);
        Task RemoveAsync(Favorite favorite);
    }
}