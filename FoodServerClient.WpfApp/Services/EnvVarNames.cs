namespace FoodServerClient.WpfApp.Services;

internal static class EnvVarNames
{
    public static string CommentName(string baseName)
    {
        return $"{baseName}_COMMENT";
    }
}