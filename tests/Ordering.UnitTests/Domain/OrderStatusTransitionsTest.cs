namespace eShop.Ordering.UnitTests.Domain;

using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

[TestClass]
public class OrderStatusTransitionsTest
{
    private static Order CreateSubmittedOrder()
    {
        var address = new Address("street", "city", "state", "country", "zipcode");
        return new Order("userId", "userName", address, cardTypeId: 1, cardNumber: "1234",
            cardSecurityNumber: "123", cardHolderName: "Name", cardExpiration: DateTime.UtcNow.AddYears(1));
    }

    [TestMethod]
    public void SetAwaitingValidationStatus_from_Submitted_succeeds()
    {
        var order = CreateSubmittedOrder();
        order.ClearDomainEvents();

        order.SetAwaitingValidationStatus();

        Assert.AreEqual(OrderStatus.AwaitingValidation, order.OrderStatus);
        Assert.HasCount(1, order.DomainEvents);
        Assert.IsInstanceOfType<OrderStatusChangedToAwaitingValidationDomainEvent>(order.DomainEvents.Single());
    }

    [TestMethod]
    public void SetAwaitingValidationStatus_from_non_Submitted_is_no_op()
    {
        var order = CreateSubmittedOrder();
        order.SetAwaitingValidationStatus();
        order.ClearDomainEvents();

        order.SetAwaitingValidationStatus();

        Assert.AreEqual(OrderStatus.AwaitingValidation, order.OrderStatus);
        Assert.IsEmpty(order.DomainEvents);
    }

    [TestMethod]
    public void SetStockConfirmedStatus_from_AwaitingValidation_succeeds()
    {
        var order = CreateSubmittedOrder();
        order.SetAwaitingValidationStatus();
        order.ClearDomainEvents();

        order.SetStockConfirmedStatus();

        Assert.AreEqual(OrderStatus.StockConfirmed, order.OrderStatus);
        Assert.HasCount(1, order.DomainEvents);
        Assert.IsInstanceOfType<OrderStatusChangedToStockConfirmedDomainEvent>(order.DomainEvents.Single());
    }

    [TestMethod]
    public void SetStockConfirmedStatus_from_non_AwaitingValidation_is_no_op()
    {
        var order = CreateSubmittedOrder();
        order.ClearDomainEvents();

        order.SetStockConfirmedStatus();

        Assert.AreEqual(OrderStatus.Submitted, order.OrderStatus);
        Assert.IsEmpty(order.DomainEvents);
    }

    [TestMethod]
    public void SetPaidStatus_from_StockConfirmed_succeeds()
    {
        var order = CreateSubmittedOrder();
        order.SetAwaitingValidationStatus();
        order.SetStockConfirmedStatus();
        order.ClearDomainEvents();

        order.SetPaidStatus();

        Assert.AreEqual(OrderStatus.Paid, order.OrderStatus);
        Assert.HasCount(1, order.DomainEvents);
        Assert.IsInstanceOfType<OrderStatusChangedToPaidDomainEvent>(order.DomainEvents.Single());
    }

    [TestMethod]
    public void SetPaidStatus_from_non_StockConfirmed_is_no_op()
    {
        var order = CreateSubmittedOrder();
        order.SetAwaitingValidationStatus();
        order.ClearDomainEvents();

        order.SetPaidStatus();

        Assert.AreEqual(OrderStatus.AwaitingValidation, order.OrderStatus);
        Assert.IsEmpty(order.DomainEvents);
    }

    [TestMethod]
    public void SetShippedStatus_from_Paid_succeeds()
    {
        var order = CreateSubmittedOrder();
        order.SetAwaitingValidationStatus();
        order.SetStockConfirmedStatus();
        order.SetPaidStatus();
        order.ClearDomainEvents();

        order.SetShippedStatus();

        Assert.AreEqual(OrderStatus.Shipped, order.OrderStatus);
        Assert.HasCount(1, order.DomainEvents);
        Assert.IsInstanceOfType<OrderShippedDomainEvent>(order.DomainEvents.Single());
    }

    [TestMethod]
    public void SetShippedStatus_from_non_Paid_throws()
    {
        var order = CreateSubmittedOrder();

        Assert.ThrowsExactly<OrderingDomainException>(() => order.SetShippedStatus());
    }

    [TestMethod]
    public void SetCancelledStatus_from_Submitted_succeeds()
    {
        var order = CreateSubmittedOrder();
        order.ClearDomainEvents();

        order.SetCancelledStatus();

        Assert.AreEqual(OrderStatus.Cancelled, order.OrderStatus);
        Assert.HasCount(1, order.DomainEvents);
        Assert.IsInstanceOfType<OrderCancelledDomainEvent>(order.DomainEvents.Single());
    }

    [TestMethod]
    public void SetCancelledStatus_from_AwaitingValidation_succeeds()
    {
        var order = CreateSubmittedOrder();
        order.SetAwaitingValidationStatus();
        order.ClearDomainEvents();

        order.SetCancelledStatus();

        Assert.AreEqual(OrderStatus.Cancelled, order.OrderStatus);
        Assert.HasCount(1, order.DomainEvents);
    }

    [TestMethod]
    public void SetCancelledStatus_from_Paid_throws()
    {
        var order = CreateSubmittedOrder();
        order.SetAwaitingValidationStatus();
        order.SetStockConfirmedStatus();
        order.SetPaidStatus();

        Assert.ThrowsExactly<OrderingDomainException>(() => order.SetCancelledStatus());
    }

    [TestMethod]
    public void SetCancelledStatus_from_Shipped_throws()
    {
        var order = CreateSubmittedOrder();
        order.SetAwaitingValidationStatus();
        order.SetStockConfirmedStatus();
        order.SetPaidStatus();
        order.SetShippedStatus();

        Assert.ThrowsExactly<OrderingDomainException>(() => order.SetCancelledStatus());
    }

    [TestMethod]
    public void SetCancelledStatusWhenStockIsRejected_from_AwaitingValidation_cancels_and_sets_description()
    {
        var order = CreateSubmittedOrder();
        order.AddOrderItem(42, "Widget", 10m, 0m, string.Empty);
        order.SetAwaitingValidationStatus();

        order.SetCancelledStatusWhenStockIsRejected(new[] { 42 });

        Assert.AreEqual(OrderStatus.Cancelled, order.OrderStatus);
        Assert.Contains("Widget", order.Description);
    }

    [TestMethod]
    public void SetCancelledStatusWhenStockIsRejected_from_non_AwaitingValidation_is_no_op()
    {
        var order = CreateSubmittedOrder();

        order.SetCancelledStatusWhenStockIsRejected(new[] { 1 });

        Assert.AreEqual(OrderStatus.Submitted, order.OrderStatus);
    }

    [TestMethod]
    public void SetPaymentMethodVerified_updates_buyerId_and_paymentId()
    {
        var order = CreateSubmittedOrder();

        order.SetPaymentMethodVerified(buyerId: 7, paymentId: 99);

        Assert.AreEqual(7, order.BuyerId);
        Assert.AreEqual(99, order.PaymentId);
    }

    [TestMethod]
    public void AddOrderItem_duplicate_productId_accumulates_units()
    {
        var address = new Address("street", "city", "state", "country", "zipcode");
        var order = new OrderBuilder(address)
            .AddOne(1, "Widget", 5m, 0m, string.Empty, 3)
            .AddOne(1, "Widget", 5m, 0m, string.Empty, 2)
            .Build();

        Assert.HasCount(1, order.OrderItems);
        Assert.AreEqual(5, order.OrderItems.First().Units);
        Assert.AreEqual(25m, order.GetTotal());
    }

    [TestMethod]
    public void AddOrderItem_duplicate_productId_applies_higher_discount()
    {
        var address = new Address("street", "city", "state", "country", "zipcode");
        var order = new OrderBuilder(address)
            .AddOne(1, "Widget", 50m, 5m, string.Empty, 3)
            .AddOne(1, "Widget", 50m, 10m, string.Empty, 1)
            .Build();

        Assert.AreEqual(10m, order.OrderItems.First().Discount);
    }

    [TestMethod]
    public void AddOrderItem_duplicate_productId_keeps_existing_discount_when_new_is_lower()
    {
        var address = new Address("street", "city", "state", "country", "zipcode");
        var order = new OrderBuilder(address)
            .AddOne(1, "Widget", 50m, 10m, string.Empty, 3)
            .AddOne(1, "Widget", 50m, 5m, string.Empty, 1)
            .Build();

        Assert.AreEqual(10m, order.OrderItems.First().Discount);
    }

    [TestMethod]
    public void NewDraft_creates_order_as_transient()
    {
        var order = Order.NewDraft();

        Assert.IsNotNull(order);
        Assert.IsTrue(order.IsTransient());
    }
}
