using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GAZON.Controllers;
using GAZON.Database;
using GAZON.Database.Models;
using GAZON.Models.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
	using GAZON.Controllers;
	using GAZON.Models;
	using Microsoft.AspNetCore.Mvc;
	using Moq;
	using Xunit;

	namespace GAZON.Tests.Controllers
	{
		public class AuthControllerTests
		{
			[Fact]
			public async Task Register_ReturnsViewResult_WhenModelStateIsValid()
			{
				// Arrange
				var contextMock = new Mock<GazonContext>();
				var hashServiceMock = new Mock<IHashService>();
				var controller = new AuthController(contextMock.Object, hashServiceMock.Object);
				var model = new User { Login = "testuser", Password = "testpassword" };

				// Act
				var result = await controller.Register(model);

				// Assert
				var viewResult = Assert.IsType<OkObjectResult>(result);
			}
		}
	}

