namespace eShop.Basket.API.Model;

public class CustomerBasket
{
    public string BuyerId { get; set; }

    public List<BasketItem> Items { get; set; } = [];

    public CustomerBasket() { }
    //add comment 2
    public CustomerBasket(string customerId)
    {
        BuyerId = customerId;
    }
}
