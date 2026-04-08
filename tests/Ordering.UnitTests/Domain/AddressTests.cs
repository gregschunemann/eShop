namespace eShop.Ordering.UnitTests.Domain;

using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

[TestClass]
public class AddressTests
{
    [TestMethod]
    public void Two_addresses_with_same_values_are_equal()
    {
        var a = new Address("123 Main St", "Springfield", "IL", "US", "62701");
        var b = new Address("123 Main St", "Springfield", "IL", "US", "62701");

        Assert.AreEqual(a, b);
    }

    [TestMethod]
    public void Two_addresses_with_different_street_are_not_equal()
    {
        var a = new Address("123 Main St", "Springfield", "IL", "US", "62701");
        var b = new Address("456 Elm St", "Springfield", "IL", "US", "62701");

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void Two_addresses_with_different_city_are_not_equal()
    {
        var a = new Address("123 Main St", "Springfield", "IL", "US", "62701");
        var b = new Address("123 Main St", "Shelbyville", "IL", "US", "62701");

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void Two_addresses_with_different_state_are_not_equal()
    {
        var a = new Address("123 Main St", "Springfield", "IL", "US", "62701");
        var b = new Address("123 Main St", "Springfield", "MO", "US", "62701");

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void Two_addresses_with_different_country_are_not_equal()
    {
        var a = new Address("123 Main St", "Springfield", "IL", "US", "62701");
        var b = new Address("123 Main St", "Springfield", "IL", "CA", "62701");

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void Two_addresses_with_different_zipcode_are_not_equal()
    {
        var a = new Address("123 Main St", "Springfield", "IL", "US", "62701");
        var b = new Address("123 Main St", "Springfield", "IL", "US", "99999");

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void Address_GetCopy_returns_equal_instance()
    {
        var original = new Address("123 Main St", "Springfield", "IL", "US", "62701");

        var copy = original.GetCopy() as Address;

        Assert.IsNotNull(copy);
        Assert.AreEqual(original, copy);
        Assert.AreNotSame(original, copy);
    }

    [TestMethod]
    public void Address_GetHashCode_same_for_equal_addresses()
    {
        var a = new Address("123 Main St", "Springfield", "IL", "US", "62701");
        var b = new Address("123 Main St", "Springfield", "IL", "US", "62701");

        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }
}
