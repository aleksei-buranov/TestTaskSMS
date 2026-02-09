using Sms.Test;
using Order = Sms.Test.Order;

namespace FoodServerClient.Grpc.Mapping;

internal static class OrderMapper
{
    public static Order ToProto(Abstractions.Models.Order order)
    {
        var proto = new Order { Id = order.OrderId.ToString() };

        proto.OrderItems.AddRange(order.Items.Select(i => new OrderItem
        {
            Id = i.Id,
            Quantity = i.Quantity
        }));

        return proto;
    }
}