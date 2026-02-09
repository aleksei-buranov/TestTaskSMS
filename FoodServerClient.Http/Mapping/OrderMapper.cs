using System.Globalization;
using FoodServerClient.Abstractions.Models;
using FoodServerClient.Http.Transport;

namespace FoodServerClient.Http.Mapping;

internal static class OrderMapper
{
    public static SendOrderParams ToSendOrderParams(Order order)
    {
        return new SendOrderParams
        {
            OrderId = order.OrderId.ToString(),
            MenuItems = order.Items.Select(i => new SendOrderItemDto
            {
                Id = i.Id,
                Quantity = i.Quantity.ToString(CultureInfo.InvariantCulture)
            }).ToList()
        };
    }
}