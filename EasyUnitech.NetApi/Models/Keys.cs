namespace EasyUnitech.NetApi.Models;

public class Keys
{
	public string CSRF { get; }
	public string SessCommon { get; }


    public Keys(string cSRF, string sessCommon)
    {
        CSRF = cSRF;
        SessCommon = sessCommon;
    }


    public Keys()
    {
        CSRF = "";
        SessCommon = "";
    }
}
