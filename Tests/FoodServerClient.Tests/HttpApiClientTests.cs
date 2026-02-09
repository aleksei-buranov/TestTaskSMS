using FoodServerClient.Abstractions;
using FoodServerClient.Abstractions.Models;
using FoodServerClient.Http;
using FoodServerClient.Tests.Handlers;
using Newtonsoft.Json.Linq;

namespace FoodServerClient.Tests
{
    public sealed class HttpApiClientTests
    {
        [Fact]
        public async Task GetMenuAsync_WhenSuccessTrue_ReturnsMappedMenuItems()
        {
            var handler = new FakeHttpMessageHandler(async (req, ct) =>
            {
                var body = await req.Content?.ReadAsStringAsync(ct)!;
                var j = JObject.Parse(body);

                Assert.Equal("GetMenu", (string?)j["Command"]);
                Assert.True((bool?)j["CommandParameters"]?["WithPrice"]!);

                var responseJson = """
                                   {
                                     "Command": "GetMenu",
                                     "Success": true,
                                     "ErrorMessage": "",
                                     "Data": {
                                       "MenuItems": [
                                         {
                                           "Id": "5979224",
                                           "Article": "A1004292",
                                           "Name": "Каша гречневая",
                                           "Price": 50,
                                           "IsWeighted": false,
                                           "FullPath": "ПРОИЗВОДСТВО\\\\Гарниры",
                                           "Barcodes": ["57890975627974236429"]
                                         },
                                         {
                                           "Id": "9084246",
                                           "Article": "A1004293",
                                           "Name": "Конфеты Коровка",
                                           "Price": 300,
                                           "IsWeighted": true,
                                           "FullPath": "ДЕСЕРТЫ\\\\Развес",
                                           "Barcodes": []
                                         }
                                       ]
                                     }
                                   }
                                   """;

                return FakeHttpMessageHandler.Json200(responseJson);
            });

            var http = new HttpClient(handler) { BaseAddress = new Uri("https://fake.local/") };
            var client = new ApiClient(http);


            var menu = await client.GetMenuAsync(true);


            Assert.Equal(2, menu.Count);

            var first = menu[0];
            Assert.Equal("5979224", first.Id);
            Assert.Equal("A1004292", first.Article);
            Assert.Equal("Каша гречневая", first.Name);
            Assert.Equal(50d, first.Price);
            Assert.False(first.IsWeighted);
            Assert.Single(first.Barcodes);

            var second = menu[1];
            Assert.True(second.IsWeighted);
            Assert.Empty(second.Barcodes);
        }

        [Fact]
        public async Task SendOrderAsync_SendsQuantityAsStringWithDotInvariant()
        {

            var handler = new FakeHttpMessageHandler(async (req, ct) =>
            {
                var body = await req.Content!.ReadAsStringAsync(ct);
                var j = JObject.Parse(body);

                Assert.Equal("SendOrder", (string?)j["Command"]);

                var items = (JArray)j["CommandParameters"]!["MenuItems"]!;
                var q1 = (string?)items[0]["Quantity"];
                var q2 = (string?)items[1]["Quantity"];

                Assert.Equal("1", q1);
                Assert.Equal("0.408", q2);

                var responseJson = """
                                   {
                                     "Command": "SendOrder",
                                     "Success": true,
                                     "ErrorMessage": ""
                                   }
                                   """;
                return FakeHttpMessageHandler.Json200(responseJson);
            });

            var http = new HttpClient(handler) { BaseAddress = new Uri("https://fake.local/") };
            var client = new ApiClient(http);

            var order = new Order
            {
                OrderId = Guid.Parse("62137983-1117-4D10-87C1-EF40A4348250"),
                Items = new[]
                {
                    new OrderItem { Id = "5979224", Quantity = 1d },
                    new OrderItem { Id = "9084246", Quantity = 0.408d }
                }
            };

            await client.SendOrderAsync(order);
        }

        [Fact]
        public async Task GetMenuAsync_WhenSuccessFalse_ThrowsApiErrorExceptionWithMessage()
        {

            var handler = new FakeHttpMessageHandler((req, ct) =>
            {
                var responseJson = """
                                   {
                                     "Command": "GetMenu",
                                     "Success": false,
                                     "ErrorMessage": "Bad request payload"
                                   }
                                   """;
                return Task.FromResult(FakeHttpMessageHandler.Json200(responseJson));
            });

            var http = new HttpClient(handler) { BaseAddress = new Uri("https://fake.local/") };
            var client = new ApiClient(http);

            var ex = await Assert.ThrowsAsync<ApiErrorException>(() => client.GetMenuAsync());
            Assert.Equal("GetMenu", ex.Command);
            Assert.Contains("Bad request payload", ex.Message);
        }

        [Fact]
        public async Task GetMenuAsync_WhenInvalidJson_ThrowsApiErrorException()
        {

            var handler = new FakeHttpMessageHandler((req, ct) =>
                Task.FromResult(FakeHttpMessageHandler.Json200("{ not-json }")));

            var http = new HttpClient(handler) { BaseAddress = new Uri("https://fake.local/") };
            var client = new ApiClient(http);

            await Assert.ThrowsAsync<ApiErrorException>(() => client.GetMenuAsync());
        }
    }
}
