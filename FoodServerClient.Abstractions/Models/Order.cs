namespace FoodServerClient.Abstractions.Models;

public sealed class Order
{
    public Guid OrderId { get; init; }
    public IReadOnlyList<OrderItem> Items { get; init; } = [];
}