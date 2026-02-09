using FoodServerClient.Abstractions;
using FoodServerClient.Grpc.Mapping;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Sms.Test;
using MenuItem = FoodServerClient.Abstractions.Models.MenuItem;
using Order = FoodServerClient.Abstractions.Models.Order;

namespace FoodServerClient.Grpc;

public sealed class ApiClient : IApiClient
{
    private readonly SmsTestService.SmsTestServiceClient _client;

    public ApiClient(GrpcChannel channel)
    {
        if (channel is null) throw new ArgumentNullException(nameof(channel));
        _client = new SmsTestService.SmsTestServiceClient(channel);
    }

    public async Task<IReadOnlyList<MenuItem>> GetMenuAsync(bool withPrice, CancellationToken ct = default)
    {
        GetMenuResponse resp;
        try
        {
            resp = await _client.GetMenuAsync(new BoolValue { Value = withPrice }, cancellationToken: ct);
        }
        catch (RpcException ex)
        {
            throw new HttpRequestException($"gRPC transport error: {ex.Status}", ex);
        }

        if (!resp.Success)
            throw new ApiErrorException("GetMenu", resp.ErrorMessage);

        return MenuItemMapper.ToDomain(resp.MenuItems);
    }

    public async Task SendOrderAsync(Order order, CancellationToken ct = default)
    {
        if (order is null) throw new ArgumentNullException(nameof(order));
        if (order.OrderId == Guid.Empty) throw new ArgumentException("OrderId must be non-empty.", nameof(order));
        if (order.Items is null || order.Items.Count == 0)
            throw new ArgumentException("Order must contain at least one item.", nameof(order));

        var proto = OrderMapper.ToProto(order);

        SendOrderResponse resp;
        try
        {
            resp = await _client.SendOrderAsync(proto, cancellationToken: ct);
        }
        catch (RpcException ex)
        {
            throw new HttpRequestException($"gRPC transport error: {ex.Status}", ex);
        }

        if (!resp.Success)
            throw new ApiErrorException("SendOrder", resp.ErrorMessage);
    }
}