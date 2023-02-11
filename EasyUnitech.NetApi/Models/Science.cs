namespace EasyUnitech.NetApi.Models;

public class Science
{
    public int UnitechId { get; }
	public string Name { get; }

    public Science(int unitechId, string name)
    {
        UnitechId = unitechId;
        Name = name;
    }
}

