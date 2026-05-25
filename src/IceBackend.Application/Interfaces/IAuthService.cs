using System;
using System.Threading.Tasks;
using IceBackend.Domain.Entities;

namespace IceBackend.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Player> RegisterIceAccountAsync(string username, string password);
        Task<(Player Player, string SessionToken)?> LoginIceAccountAsync(string username, string password);
        Task<(Player Player, string SessionToken)> RegisterAndLoginAsync(string username, string password);
    }
}
