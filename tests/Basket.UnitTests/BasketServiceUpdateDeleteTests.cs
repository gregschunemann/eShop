using System.Security.Claims;
using eShop.Basket.API.Repositories;
using eShop.Basket.API.Grpc;
using eShop.Basket.API.Model;
using eShop.Basket.UnitTests.Helpers;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using BasketItem = eShop.Basket.API.Model.BasketItem;

namespace eShop.Basket.UnitTests;

[TestClass]
public class BasketServiceUpdateDeleteTests
{
    public TestContext TestContext { get; set; }

    private static ServerCallContext CreateAuthenticatedContext(string userId, CancellationToken cancellationToken = default)
    {
        var context = TestServerCallContext.Create(cancellationToken: cancellationToken);
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId)]));
        context.SetUserState("__HttpContext", httpContext);
        return context;
    }

    private static ServerCallContext CreateUnauthenticatedContext(CancellationToken cancellationToken = default)
    {
        var context = TestServerCallContext.Create(cancellationToken: cancellationToken);
        context.SetUserState("__HttpContext", new DefaultHttpContext());
        return context;
    }

    [TestMethod]
    public async Task UpdateBasket_with_valid_user_returns_updated_basket()
    {
        var mockRepository = Substitute.For<IBasketRepository>();
        var userId = "user-123";
        var updatedBasket = new CustomerBasket
        {
            BuyerId = userId,
            Items = [new BasketItem { ProductId = 10, Quantity = 3 }]
        };
        mockRepository.UpdateBasketAsync(Arg.Any<CustomerBasket>()).Returns(Task.FromResult(updatedBasket));

        var service = new BasketService(mockRepository, NullLogger<BasketService>.Instance);
        var request = new UpdateBasketRequest();
        request.Items.Add(new eShop.Basket.API.Grpc.BasketItem { ProductId = 10, Quantity = 3 });

        var response = await service.UpdateBasket(request, CreateAuthenticatedContext(userId, TestContext.CancellationToken));

        Assert.IsInstanceOfType<CustomerBasketResponse>(response);
        Assert.HasCount(1, response.Items);
        Assert.AreEqual(10, response.Items[0].ProductId);
        Assert.AreEqual(3, response.Items[0].Quantity);
    }

    [TestMethod]
    public async Task UpdateBasket_without_user_throws_unauthenticated()
    {
        var mockRepository = Substitute.For<IBasketRepository>();
        var service = new BasketService(mockRepository, NullLogger<BasketService>.Instance);

        var ex = await Assert.ThrowsExactlyAsync<RpcException>(() =>
            service.UpdateBasket(new UpdateBasketRequest(), CreateUnauthenticatedContext(TestContext.CancellationToken)));

        Assert.AreEqual(StatusCode.Unauthenticated, ex.Status.StatusCode);
    }

    [TestMethod]
    public async Task UpdateBasket_when_repository_returns_null_throws_not_found()
    {
        var mockRepository = Substitute.For<IBasketRepository>();
        mockRepository.UpdateBasketAsync(Arg.Any<CustomerBasket>()).Returns(Task.FromResult<CustomerBasket>(null));

        var service = new BasketService(mockRepository, NullLogger<BasketService>.Instance);
        var request = new UpdateBasketRequest();

        var ex = await Assert.ThrowsExactlyAsync<RpcException>(() =>
            service.UpdateBasket(request, CreateAuthenticatedContext("user-1", TestContext.CancellationToken)));

        Assert.AreEqual(StatusCode.NotFound, ex.Status.StatusCode);
    }

    [TestMethod]
    public async Task DeleteBasket_with_valid_user_returns_empty_response()
    {
        var mockRepository = Substitute.For<IBasketRepository>();
        mockRepository.DeleteBasketAsync(Arg.Any<string>()).Returns(Task.FromResult(true));

        var service = new BasketService(mockRepository, NullLogger<BasketService>.Instance);

        var response = await service.DeleteBasket(new DeleteBasketRequest(), CreateAuthenticatedContext("user-1", TestContext.CancellationToken));

        Assert.IsInstanceOfType<DeleteBasketResponse>(response);
        await mockRepository.Received(1).DeleteBasketAsync("user-1");
    }

    [TestMethod]
    public async Task DeleteBasket_without_user_throws_unauthenticated()
    {
        var mockRepository = Substitute.For<IBasketRepository>();
        var service = new BasketService(mockRepository, NullLogger<BasketService>.Instance);

        var ex = await Assert.ThrowsExactlyAsync<RpcException>(() =>
            service.DeleteBasket(new DeleteBasketRequest(), CreateUnauthenticatedContext(TestContext.CancellationToken)));

        Assert.AreEqual(StatusCode.Unauthenticated, ex.Status.StatusCode);
    }
}
