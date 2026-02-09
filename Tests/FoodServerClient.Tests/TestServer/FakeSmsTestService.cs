using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Sms.Test;

namespace FoodServerClient.Tests.TestServer;

internal sealed class FakeSmsTestService : SmsTestService.SmsTestServiceBase
{
    public bool ForceError { get; set; }

    public override Task<GetMenuResponse> GetMenu(BoolValue request, ServerCallContext context)
    {
        if (ForceError)
            return Task.FromResult(new GetMenuResponse
            {
                Success = false,
                ErrorMessage = "Bad request payload"
            });

        var resp = new GetMenuResponse { Success = true, ErrorMessage = "" };
        resp.MenuItems.Add(new MenuItem
        {
            Id = "5979224",
            Article = "A1004292",
            Name = "Каша гречневая",
            Price = request.Value ? 50 : 0,
            IsWeighted = false,
            FullPath = "ПРОИЗВОДСТВО\\Гарниры"
        });
        resp.MenuItems[0].Barcodes.Add("57890975627974236429");

        return Task.FromResult(resp);
    }

    public override Task<SendOrderResponse> SendOrder(Order request, ServerCallContext context)
    {
        if (ForceError)
            return Task.FromResult(new SendOrderResponse
            {
                Success = false,
                ErrorMessage = "Order rejected"
            });

        // можно добавить проверки запроса
        if (string.IsNullOrWhiteSpace(request.Id) || request.OrderItems.Count == 0)
            return Task.FromResult(new SendOrderResponse
            {
                Success = false,
                ErrorMessage = "Invalid order"
            });

        return Task.FromResult(new SendOrderResponse { Success = true, ErrorMessage = "" });
    }
}