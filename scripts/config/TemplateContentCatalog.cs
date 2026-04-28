using Godot;

namespace GFrameworkGodotTemplate.scripts.config;

/// <summary>
///     Loads and exposes static template content configuration.
/// </summary>
public sealed class TemplateContentCatalog : ITemplateContentCatalog
{
    private readonly TemplateConfigHost _configHost;
    private MenuTextTable _menuTextTable = null!;

    public TemplateContentCatalog()
    {
        _configHost = new TemplateConfigHost();
        RefreshReloadableTables(_configHost.Registry);
    }

    public MenuTextConfig GetMenuText()
    {
        return ResolveByLanguage(_menuTextTable);
    }

    public string GetCurrentLanguageId()
    {
        var locale = TranslationServer.GetLocale();
        if (string.IsNullOrWhiteSpace(locale)) return "en";

        var normalized = locale.Replace("_", "-", StringComparison.Ordinal).ToLowerInvariant();
        return normalized.StartsWith("zh", StringComparison.Ordinal) ? "zh-cn" : "en";
    }

    public void Reload()
    {
        _configHost.Reload();
        RefreshReloadableTables(_configHost.Registry);
    }

    private void RefreshReloadableTables(IConfigRegistry registry)
    {
        _menuTextTable = registry.GetMenuTextTable();
    }

    private TConfig ResolveByLanguage<TConfig>(IConfigTable<string, TConfig> table)
    {
        var languageId = GetCurrentLanguageId();
        if (table.TryGet(languageId, out var config) && config is not null) return config;

        return table.Get("en");
    }
}
