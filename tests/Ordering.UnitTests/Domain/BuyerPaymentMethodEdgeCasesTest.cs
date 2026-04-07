namespace eShop.Ordering.UnitTests.Domain;

[TestClass]
public class BuyerPaymentMethodEdgeCasesTest
{
    [TestMethod]
    public void Create_buyer_with_empty_name_throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new Buyer(Guid.NewGuid().ToString(), string.Empty));
    }

    [TestMethod]
    public void Create_buyer_with_whitespace_name_throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new Buyer(Guid.NewGuid().ToString(), "   "));
    }

    [TestMethod]
    public void VerifyOrAddPaymentMethod_returns_existing_payment_when_already_added()
    {
        var buyer = new Buyer(Guid.NewGuid().ToString(), "testUser");
        var cardTypeId = 1;
        var cardNumber = "4111111111111111";
        var expiration = DateTime.UtcNow.AddYears(1);

        var first = buyer.VerifyOrAddPaymentMethod(cardTypeId, "alias", cardNumber, "123", "Name", expiration, orderId: 1);
        buyer.ClearDomainEvents();
        var second = buyer.VerifyOrAddPaymentMethod(cardTypeId, "alias", cardNumber, "123", "Name", expiration, orderId: 2);

        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void VerifyOrAddPaymentMethod_raises_domain_event_for_existing_payment()
    {
        var buyer = new Buyer(Guid.NewGuid().ToString(), "testUser");
        var expiration = DateTime.UtcNow.AddYears(1);

        buyer.VerifyOrAddPaymentMethod(1, "alias", "4111", "123", "Name", expiration, orderId: 1);
        buyer.ClearDomainEvents();

        buyer.VerifyOrAddPaymentMethod(1, "alias", "4111", "123", "Name", expiration, orderId: 2);

        Assert.HasCount(1, buyer.DomainEvents);
    }

    [TestMethod]
    public void VerifyOrAddPaymentMethod_adds_new_payment_for_different_card()
    {
        var buyer = new Buyer(Guid.NewGuid().ToString(), "testUser");
        var expiration = DateTime.UtcNow.AddYears(1);

        buyer.VerifyOrAddPaymentMethod(1, "alias1", "4111", "123", "Name", expiration, orderId: 1);
        buyer.VerifyOrAddPaymentMethod(1, "alias2", "9999", "456", "Name", expiration, orderId: 2);

        Assert.HasCount(2, buyer.PaymentMethods);
    }

    [TestMethod]
    public void Create_payment_method_with_empty_cardNumber_throws()
    {
        Assert.ThrowsExactly<OrderingDomainException>(() =>
            new PaymentMethod(1, "alias", string.Empty, "123", "Name", DateTime.UtcNow.AddYears(1)));
    }

    [TestMethod]
    public void Create_payment_method_with_empty_securityNumber_throws()
    {
        Assert.ThrowsExactly<OrderingDomainException>(() =>
            new PaymentMethod(1, "alias", "4111", string.Empty, "Name", DateTime.UtcNow.AddYears(1)));
    }

    [TestMethod]
    public void Create_payment_method_with_empty_cardHolderName_throws()
    {
        Assert.ThrowsExactly<OrderingDomainException>(() =>
            new PaymentMethod(1, "alias", "4111", "123", string.Empty, DateTime.UtcNow.AddYears(1)));
    }

    [TestMethod]
    public void IsEqualTo_returns_false_for_different_cardTypeId()
    {
        var expiration = DateTime.UtcNow.AddYears(1);
        var payment = new PaymentMethod(1, "alias", "4111", "123", "Name", expiration);

        Assert.IsFalse(payment.IsEqualTo(2, "4111", expiration));
    }

    [TestMethod]
    public void IsEqualTo_returns_false_for_different_cardNumber()
    {
        var expiration = DateTime.UtcNow.AddYears(1);
        var payment = new PaymentMethod(1, "alias", "4111", "123", "Name", expiration);

        Assert.IsFalse(payment.IsEqualTo(1, "9999", expiration));
    }

    [TestMethod]
    public void IsEqualTo_returns_false_for_different_expiration()
    {
        var expiration = DateTime.UtcNow.AddYears(1);
        var payment = new PaymentMethod(1, "alias", "4111", "123", "Name", expiration);

        Assert.IsFalse(payment.IsEqualTo(1, "4111", expiration.AddDays(1)));
    }
}
