namespace EasyUnitech.NetApi.Interfaces;

public interface IAuthorizedHttpClient
{
    Task<string> GetAsync(string uri);
    Task<string> PostAsync(string uri, Dictionary<string, string> formData);
}
