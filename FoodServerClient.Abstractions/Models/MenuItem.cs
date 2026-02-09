namespace FoodServerClient.Abstractions.Models;

public sealed class MenuItem
{
    public string Id { get; init; } = "";
    public string Article { get; init; } = "";
    public string Name { get; init; } = "";
    public double? Price { get; init; }
    public bool IsWeighted { get; init; }
    public string FullPath { get; init; } = "";
    public IReadOnlyList<string> Barcodes { get; init; } = [];
}