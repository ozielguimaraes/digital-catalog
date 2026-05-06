namespace MeuCatalogo.Features.Financeiro.Icons;

public static class IconCatalog
{
    private static readonly IReadOnlyDictionary<string, string> Map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["food"] = "🍽",
        ["home"] = "🏠",
        ["car"] = "🚗",
        ["health"] = "🩺",
        ["education"] = "🎓",
        ["entertainment"] = "🎮",
        ["travel"] = "✈",
        ["shopping"] = "🛍",
        ["cart"] = "🛒",
        ["bills"] = "📄",
        ["water"] = "💧",
        ["electricity"] = "💡",
        ["internet"] = "🌐",
        ["phone"] = "📱",
        ["transport"] = "🚌",
        ["fitness"] = "🏋",
        ["pet"] = "🐾",
        ["gift"] = "🎁",
        ["work"] = "💼",
        ["salary"] = "💰",
        ["investment"] = "📈",
        ["bonus"] = "🏅",
        ["donation"] = "❤",
        ["tax"] = "🧾",
        ["fuel"] = "⛽",
        ["clothing"] = "👕",
        ["beauty"] = "💄",
        ["bank"] = "🏦",
        ["card"] = "💳",
        ["wallet"] = "👛",
        ["tag"] = "🏷"
    };

    public static string GetGlyph(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Map["tag"];
        return Map.TryGetValue(name.Trim(), out var glyph) ? glyph : Map["tag"];
    }

    public static IEnumerable<string> AvailableNames => Map.Keys;
}
