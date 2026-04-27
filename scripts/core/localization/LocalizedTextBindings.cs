using System.Reflection;
using GFrameworkGodotTemplate.scripts.config;
using Godot;
using Godot.Collections;

namespace GFrameworkGodotTemplate.scripts.core.localization;

/// <summary>
///     Applies a group of localized text bindings to node properties and refreshes them on language changes.
/// </summary>
[GlobalClass]
[ContextAware]
public partial class LocalizedTextBindings : Node
{
    [GetUtility] private ITemplateContentCatalog? _contentCatalog;

    [GetSystem] private ILocalizationManager? _localizationManager;

    [Export]
    public Array<LocalizedTextBinding> Bindings { get; set; } = [];

    [Export]
    public bool AutoApplyOnReady { get; set; } = true;

    public override void _Ready()
    {
        __InjectContextBindings_Generated();
        _localizationManager?.SubscribeToLanguageChange(OnLanguageChanged);
        this.RegisterEvent<SettingsAppliedEvent<ISettingsSection>>(OnSettingsApplied);

        if (AutoApplyOnReady) ApplyAll();
    }

    public override void _ExitTree()
    {
        this.UnRegisterEvent<SettingsAppliedEvent<ISettingsSection>>(OnSettingsApplied);
        _localizationManager?.UnsubscribeFromLanguageChange(OnLanguageChanged);
    }

    public void ApplyAll()
    {
        if (_contentCatalog is null) return;

        var root = GetParent();
        if (root is null) return;

        foreach (var binding in Bindings)
        {
            if (binding is null || string.IsNullOrWhiteSpace(binding.Key)) continue;

            var target = string.IsNullOrWhiteSpace(binding.TargetPath)
                ? root
                : root.GetNodeOrNull(binding.TargetPath);
            if (target is null)
            {
                GD.PushWarning($"{nameof(LocalizedTextBindings)} target node was not found: {binding.TargetPath}");
                continue;
            }

            if (!TryResolveText(binding, out var text))
            {
                GD.PushWarning($"{nameof(LocalizedTextBindings)} text key was not found: {binding.Key}");
                continue;
            }

            ApplyValue(target, binding.TargetProperty, text);
        }
    }

    private void OnLanguageChanged(string _)
    {
        ApplyAll();
    }

    private void OnSettingsApplied(SettingsAppliedEvent<ISettingsSection> @event)
    {
        if (!@event.Success ||
            @event.Settings is not IResetApplyAbleSettings settings ||
            settings.DataType != typeof(LocalizationSettings))
            return;

        ApplyAll();
    }

    private bool TryResolveText(LocalizedTextBinding binding, out string text)
    {
        text = string.Empty;
        if (_contentCatalog is null) return false;

        object sourceObject = binding.Source switch
        {
            LocalizedTextCatalogSource.Menu => _contentCatalog.GetMenuText(),
            _ => _contentCatalog.GetMenuText()
        };

        var property = sourceObject.GetType().GetProperty(
            binding.Key,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (property?.PropertyType != typeof(string)) return false;

        text = property.GetValue(sourceObject) as string ?? string.Empty;
        return true;
    }

    private static void ApplyValue(Node target, string propertyName, string text)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (property is not null &&
            property.CanWrite &&
            property.PropertyType == typeof(string))
        {
            property.SetValue(target, text);
            return;
        }

        target.Set(propertyName, text);
    }
}
