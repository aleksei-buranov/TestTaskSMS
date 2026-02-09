namespace FoodServerClient.Abstractions.Models;

public sealed class OrderItem
{
    public string Id { get; init; } = "";
    public double Quantity { get; init; }
}