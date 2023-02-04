namespace EasyUnitech.NetApi.Interfaces;

public interface IHttpService
{
    Task<string> GetAsync(string uri);
}
