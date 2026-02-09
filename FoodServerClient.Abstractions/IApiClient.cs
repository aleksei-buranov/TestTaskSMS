using FoodServerClient.Abstractions.Models;

namespace FoodServerClient.Abstractions;

public interface IApiClient
{
    Task<IReadOnlyList<MenuItem>> GetMenuAsync(bool withPrice = true, CancellationToken ct = default);
    Task SendOrderAsync(Order order, CancellationToken ct = default);
}