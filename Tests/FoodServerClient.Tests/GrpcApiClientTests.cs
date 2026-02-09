using FoodServerClient.Abstractions;
using FoodServerClient.Grpc;
using FoodServerClient.Tests.TestServer;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Order = FoodServerClient.Abstractions.Models.Order;
using OrderItem = FoodServerClient.Abstractions.Models.OrderItem;

namespace FoodServerClient.Tests
{
    public sealed class GrpcApiClientTests
    {
        private static (ApiClient client, FakeSmsTestService service) CreateClient()
        {
            var fake = new FakeSmsTestService();

            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddGrpc();
                    services.AddSingleton(fake);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseGrpcWeb();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGrpcService<FakeSmsTestService>()
                            .EnableGrpcWeb();
                    });
                });

            var testServer = new Microsoft.AspNetCore.TestHost.TestServer(builder);


            var innerHandler = testServer.CreateHandler();
            var grpcWebHandler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, innerHandler);

            var httpClient = new HttpClient(grpcWebHandler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
            {
                HttpClient = httpClient
            });

            var apiClient = new ApiClient(channel);

            return (apiClient, fake);
        }

        [Fact]
        public async Task GetMenuAsync_SuccessTrue_ReturnsMappedItems()
        {
            var (client, _) = CreateClient();

            var menu = await client.GetMenuAsync(withPrice: true);

            Assert.Single(menu);
            Assert.Equal("5979224", menu[0].Id);
            Assert.Equal("A1004292", menu[0].Article);
            Assert.Equal("Каша гречневая", menu[0].Name);
            Assert.Equal(50, menu[0].Price);
            Assert.False(menu[0].IsWeighted);
            Assert.Single(menu[0].Barcodes);
        }

        [Fact]
        public async Task GetMenuAsync_SuccessFalse_ThrowsApiErrorException()
        {
            var (client, service) = CreateClient();
            service.ForceError = true;

            var ex = await Assert.ThrowsAsync<ApiErrorException>(() => client.GetMenuAsync(true));
            Assert.Contains("Bad request payload", ex.Message);
        }

        [Fact]
        public async Task SendOrderAsync_SuccessTrue_DoesNotThrow()
        {
            var (client, _) = CreateClient();

            var order = new Order
            {
                OrderId = Guid.Parse("62137983-1117-4D10-87C1-EF40A4348250"),
                Items = new[]
                {
                    new OrderItem { Id = "5979224", Quantity = 1 },
                    new OrderItem { Id = "9084246", Quantity = 0.408 }
                }
            };

            await client.SendOrderAsync(order);
        }

        [Fact]
        public async Task SendOrderAsync_SuccessFalse_ThrowsApiErrorException()
        {
            var (client, service) = CreateClient();
            service.ForceError = true;

            var order = new Order
            {
                OrderId = Guid.Parse("62137983-1117-4D10-87C1-EF40A4348250"),
                Items = new[] { new OrderItem { Id = "5979224", Quantity = 1 } }
            };

            var ex = await Assert.ThrowsAsync<ApiErrorException>(() =>
                client.SendOrderAsync(order));
            Assert.Contains("Order rejected", ex.Message);
        }
    }
}
