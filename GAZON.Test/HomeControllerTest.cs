using GAZON.Controllers;
using GAZON.Database;
using GAZON.Database.Models;
using GAZON.Models;
using GAZON.Test.Extention;
using Moq;

namespace GAZON.Test;

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class HomeControllerTests
{
    private Mock<GazonContext> GetMockedContext()
    {
        var context = new Mock<GazonContext>();

        var user = new User { Id = 1, Login = "test_user" };
        var usersDbSet = new List<User> { user }.AsQueryable().BuildMockDbSet();
        var itemsDbSet = new List<Item>().AsQueryable().BuildMockDbSet();

        context.Setup(c => c.Users).Returns(usersDbSet.Object);
        context.Setup(c => c.Items).Returns(itemsDbSet.Object);

        return context;
    }

    private ClaimsPrincipal GetMockedUser()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "test_user")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithItemList()
    {
        // Arrange
        var context = GetMockedContext();
        var controller = new HomeController(context.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = GetMockedUser() }
            }
        };

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsAssignableFrom<List<ItemViewModel>>(viewResult.Model);
    }

    [Fact]
    public async Task Search_ReturnsViewResult_WithFilteredItemList()
    {
        // Arrange
        var context = GetMockedContext();
        var controller = new HomeController(context.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = GetMockedUser() }
            }
        };

        // Act
        var result = await controller.Search("test", "item");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.IsAssignableFrom<List<ItemViewModel>>(viewResult.Model);
    }
}
