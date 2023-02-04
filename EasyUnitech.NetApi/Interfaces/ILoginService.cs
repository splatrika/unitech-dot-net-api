using EasyUnitech.NetApi.Models;
namespace EasyUnitech.NetApi.Interfaces;

public interface ILoginService
{
    Task<bool> TryLoginAsync(string login, string password, Action<Keys> callback);
    Task<bool> ValidateAsync(Keys keys);
    Task LogoutAsync(Keys keys);
}

