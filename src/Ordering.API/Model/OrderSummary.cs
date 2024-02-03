using eShop.Ordering.Data;

namespace eShop.Ordering.API.Model;

public class OrderSummary
{
    public int OrderNumber { get; init; }

    public DateTime Date { get; init; }

    public required string Status { get; init; }

    public decimal Total { get; init; }

    public static OrderSummary FromOrder(Order order)
    {
        return new OrderSummary
        {
            OrderNumber = order.Id,
            Date = order.OrderDate,
            Status = order.OrderStatus.ToString(),
            Total = order.GetTotal()
        };
    }
}
