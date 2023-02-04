using EasyUnitech.NetApi.Models;
namespace EasyUnitech.NetApi.Interfaces;

public interface IUserService
{
    Task<User> GetUserAsync();
}
