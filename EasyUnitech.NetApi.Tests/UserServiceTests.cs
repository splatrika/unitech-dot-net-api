using System;
using EasyUnitech.NetApi.Constants;
using EasyUnitech.NetApi.Interfaces;
using EasyUnitech.NetApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
namespace EasyUnitech.NetApi.Tests.Unit;

public class UserServiceTests
{
	public const string FakeFirstName = "Иван";
	public const string FakeLastName = "Иванов";
	public const string FakePatronymic = "Иванович";
	public const string FakeBirthDay = "11.12.2000";
	public DateTime FakeBirthDateTime = new DateTime(2000, 12, 11);
	public const string FakePhoto = "/static/photos/ivan@12.png";

    public const string FakeUserPage = @$"
<html>
	<head>
		<title>Fake user page</title>
	</head>
	<body>
		<div class=""userpage_block_wrap"">
			<div class=""userpage_block_title"">
				Личные данные
				<div class=""fl_right"">
					(Это вы)
				</div>	
			</div>
			<div class=""info""><strong>Фамилия:</strong> {FakeLastName}</div>
			<div class=""info""><strong>Имя:</strong> {FakeFirstName}</div>
			<div class=""info""><strong>Отчество:</strong> {FakePatronymic}</div>
			<div class=""info"">
				<strong>Дата рождения:</strong> {FakeBirthDay}		
			</div>
			<div class=""clearer""></div>	
		</div>
		<div class=""userpage_block_wrap"">
			<div class=""userpage_block_title"">
				Контакты
			</div>
			<div class=""info""><strong>Адрес электронной почты:</strong> example@example.example</div>
		</div>
		<div class=""user_rating"">			
			<div class=""users_avatar_wrap"" onclick=""showModalImg('/files/upload/pages/image/stock_people.png')"" style=""background: url({FakePhoto}) top left no-repeat;"">
				<h2 id=""user_page_rating_total"" data-toggle=""tooltip"" title="""" data-original-title=""Итоговый рейтинг по курсу в университете"">54.6%</h2>				
				<svg viewBox=""0 0 36 36"" class=""rating-chart""><path class=""circle"" stroke=""#ffdf00"" stroke-dasharray=""54.6, 100"" d=""M18 2 a 15.9155 15.9155 0 0 0 0 31.831 a 15.9155 15.9155 0 0 0 0 -31.831""></path></svg>
			</div>
		</div>
	</body>
</html>";


	[Fact]
	public async void GetUser()
	{
		var httpServiceMock = new Mock<IHttpService>();
		httpServiceMock.Setup(x => x.GetAsync($"{HttpConstants.Host}/user"))
			.Returns(Task.FromResult(FakeUserPage));

		var loggerMock = new Mock<ILogger<UserService>>();

		var service = new UserService(httpServiceMock.Object, loggerMock.Object);
		var user = await service.GetUserAsync();

		Assert.Equal(FakeFirstName, user.FirstName);
		Assert.Equal(FakeLastName, user.LastName);
		Assert.Equal(FakePatronymic, user.Patronymic);
		Assert.Equal(FakeBirthDateTime, user.Bithday);
		Assert.Equal(FakePhoto, user.Photo);
    }
}

