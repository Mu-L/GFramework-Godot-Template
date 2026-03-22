using GFramework.Core.Abstractions.Localization;
using Godot;

namespace GFrameworkGodotTemplate.scripts.core.localization;

/// <summary>
/// 本地化 RichTextLabel 组件
/// 自动根据语言变化更新富文本内容，支持 BBCode
/// </summary>
[GlobalClass]
[ContextAware]
public partial class LocalizationRichTextLabel : RichTextLabel
{
    private readonly Dictionary<string, object> _variables = new(StringComparer.OrdinalIgnoreCase);
    private ILocalizationManager _locManager = null!;
    private ILocalizationString? _locString;
    private bool _subscribed;

    /// <summary>
    /// 本地化表名
    /// </summary>
    [Export]
    public string LocalizationTable { get; set; } = "common";

    /// <summary>
    /// 本地化键名
    /// </summary>
    [Export]
    public string LocalizationKey { get; set; } = string.Empty;

    /// <summary>
    /// 是否在 Ready 时自动更新文本
    /// </summary>
    [Export]
    public bool AutoUpdate { get; set; } = true;

    /// <summary>
    /// 是否启用 BBCode
    /// </summary>
    [Export]
    public bool EnableBbCode { get; set; } = true;

    public override void _Ready()
    {
        // 设置 BBCode 启用状态
        BbcodeEnabled = EnableBbCode;
        // 从架构中获取本地化管理器
        _locManager = this.GetSystem<ILocalizationManager>()!;

        if (!_subscribed)
        {
            // 订阅语言变化事件
            _locManager.SubscribeToLanguageChange(OnLanguageChanged);
            _subscribed = true;
        }

        // 如果启用自动更新，尝试初始化
        if (AutoUpdate)
        {
            UpdateText();
        }
    }

    public override void _ExitTree()
    {
        // 取消订阅
        UnsubscribeFromLanguageChange();
    }

    /// <summary>
    /// 设置变量
    /// </summary>
    /// <param name="name">变量名</param>
    /// <param name="value">变量值</param>
    public void SetVariable(string name, object value)
    {
        _variables[name] = value;
        UpdateText();
    }

    /// <summary>
    /// 批量设置变量
    /// </summary>
    /// <param name="variables">变量字典</param>
    public void SetVariables(IReadOnlyDictionary<string, object> variables)
    {
        foreach (var (name, value) in variables)
        {
            _variables[name] = value;
        }

        UpdateText();
    }

    /// <summary>
    /// 清除所有变量
    /// </summary>
    public void ClearVariables()
    {
        _variables.Clear();
        UpdateText();
    }

    /// <summary>
    /// 更新文本
    /// </summary>
    public void UpdateText()
    {
        if (string.IsNullOrEmpty(LocalizationKey))
        {
            return;
        }

        // 获取本地化字符串
        _locString = _locManager.GetString(LocalizationTable, LocalizationKey);

        // 应用变量
        foreach (var (name, value) in _variables)
        {
            _locString.WithVariable(name, value);
        }

        // 格式化并设置文本
        var formattedText = _locString.Format();

        // 使用 BBCode
        Text = formattedText;
    }

    /// <summary>
    /// 取消订阅语言变化事件
    /// </summary>
    private void UnsubscribeFromLanguageChange()
    {
        if (!_subscribed) return;
        _locManager.UnsubscribeFromLanguageChange(OnLanguageChanged);
        _subscribed = false;
    }

    /// <summary>
    /// 语言变化回调
    /// </summary>
    private void OnLanguageChanged(string language)
    {
        UpdateText();
    }
}