namespace EasyUnitech.NetApi.Models;

public class User
{
	public string FirstName { get; }
	public string LastName { get; }
	public string Patronymic { get; }
	public DateTime Bithday { get; }
	public string Photo { get; }

    public User(
        string firstName,
        string lastName,
        string patronymic,
        DateTime bithday,
        string photo)
    {
        FirstName = firstName;
        LastName = lastName;
        Patronymic = patronymic;
        Bithday = bithday;
        Photo = photo;
    }
}

