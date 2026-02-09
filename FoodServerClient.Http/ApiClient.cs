using System.Text;
using FoodServerClient.Abstractions;
using FoodServerClient.Abstractions.Models;
using FoodServerClient.Http.Mapping;
using FoodServerClient.Http.Transport;
using Newtonsoft.Json;

namespace FoodServerClient.Http;

public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerSettings _json;

    public ApiClient(HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _json = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };
    }

    public async Task<IReadOnlyList<MenuItem>> GetMenuAsync(bool withPrice = true, CancellationToken ct = default)
    {
        var req = new ApiRequest<GetMenuParams>(
            "GetMenu",
            new GetMenuParams { WithPrice = withPrice }
        );

        var resp = await PostAsync<GetMenuParams, GetMenuData>(req, ct).ConfigureAwait(false);

        var dtos = resp.Data?.MenuItems ?? [];
        return MenuItemMapper.ToDomain(dtos);
    }

    public async Task SendOrderAsync(Order order, CancellationToken ct = default)
    {
        if (order == null) throw new ArgumentNullException(nameof(order));
        if (order.OrderId == Guid.Empty) throw new ArgumentException("OrderId must be non-empty.", nameof(order));
        if (order.Items == null || order.Items.Count == 0)
            throw new ArgumentException("Order must contain at least one item.", nameof(order));

        var p = OrderMapper.ToSendOrderParams(order);
        var req = new ApiRequest<SendOrderParams>("SendOrder", p);

        await PostAsync<SendOrderParams, object>(req, ct).ConfigureAwait(false);
    }

    private async Task<ApiResponse<TData>> PostAsync<TParams, TData>(ApiRequest<TParams> request, CancellationToken ct)
    {
        var json = JsonConvert.SerializeObject(request, _json);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpResp = await _http.PostAsync("", content, ct).ConfigureAwait(false);

        httpResp.EnsureSuccessStatusCode();

        var body = await httpResp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        ApiResponse<TData>? apiResp;
        try
        {
            apiResp = JsonConvert.DeserializeObject<ApiResponse<TData>>(body);
        }
        catch (Exception ex)
        {
            throw new ApiErrorException(request.Command, $"Invalid JSON response. {ex.Message}");
        }

        if (apiResp == null)
            throw new ApiErrorException(request.Command, "Empty response.");

        if (!apiResp.Success)
            throw new ApiErrorException(apiResp.Command, apiResp.ErrorMessage);

        return apiResp;
    }
}