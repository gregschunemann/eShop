using System.ComponentModel.DataAnnotations;
using System.Linq;
using eShop.Basket.API.Model;

namespace eShop.Basket.UnitTests;

[TestClass]
public class BasketItemValidationTests
{
    [TestMethod]
    public void Validate_with_positive_quantity_returns_no_errors()
    {
        var item = new BasketItem { Quantity = 1, Id = "x", ProductId = 1 };
        var context = new ValidationContext(item);

        var results = item.Validate(context).ToList();

        Assert.IsEmpty(results);
    }

    [TestMethod]
    public void Validate_with_zero_quantity_returns_error()
    {
        var item = new BasketItem { Quantity = 0, Id = "x", ProductId = 1 };
        var context = new ValidationContext(item);

        var results = item.Validate(context).ToList();

        Assert.HasCount(1, results);
        CollectionAssert.Contains(results[0].MemberNames.ToList(), "Quantity");
    }

    [TestMethod]
    public void Validate_with_negative_quantity_returns_error()
    {
        var item = new BasketItem { Quantity = -5, Id = "x", ProductId = 1 };
        var context = new ValidationContext(item);

        var results = item.Validate(context).ToList();

        Assert.HasCount(1, results);
        CollectionAssert.Contains(results[0].MemberNames.ToList(), "Quantity");
    }

    [TestMethod]
    public void Validate_with_large_quantity_returns_no_errors()
    {
        var item = new BasketItem { Quantity = 999, Id = "x", ProductId = 1 };
        var context = new ValidationContext(item);

        var results = item.Validate(context).ToList();

        Assert.IsEmpty(results);
    }
}
