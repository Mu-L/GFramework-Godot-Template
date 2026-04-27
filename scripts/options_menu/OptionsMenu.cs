using GFrameworkGodotTemplate.scripts.core.ui;
using GFrameworkGodotTemplate.scripts.config;
using GFrameworkGodotTemplate.scripts.cqrs.audio.command;
using GFrameworkGodotTemplate.scripts.cqrs.audio.command.input;
using GFrameworkGodotTemplate.scripts.cqrs.graphics.command;
using GFrameworkGodotTemplate.scripts.cqrs.graphics.command.input;
using GFrameworkGodotTemplate.scripts.cqrs.setting.command;
using GFrameworkGodotTemplate.scripts.cqrs.setting.command.input;
using GFrameworkGodotTemplate.scripts.cqrs.setting.query;
using GFrameworkGodotTemplate.scripts.enums.ui;
using Godot;
using VolumeContainer = GFrameworkGodotTemplate.scripts.ui.component.VolumeContainer;

namespace GFrameworkGodotTemplate.scripts.options_menu;

/// <summary>
///     选项设置界面控制器
///     负责处理游戏设置界面的UI逻辑，包括音量控制、分辨率和全屏模式设置
/// </summary>
[ContextAware]
[Log]
public partial class OptionsMenu : Control, IController, IUiPageBehaviorProvider, ISimpleUiPage
{
    private const string ChineseLanguageValue = "简体中文";
    private const string EnglishLanguageValue = "English";

    // 语言选项
    private readonly string[] _languages =
    [
        ChineseLanguageValue,
        EnglishLanguageValue
    ];

    // 分辨率选项
    private readonly Vector2I[] _resolutions =
    [
        new(1920, 1080),
        new(1366, 768),
        new(1280, 720),
        new(1024, 768)
    ];

    /// <summary>
    ///     背景音乐音量控制容器
    /// </summary>
    [GetNode] private VolumeContainer _bgmVolumeContainer = null!;

    [GetUtility] private ITemplateContentCatalog _contentCatalog = null!;

    /// <summary>
    ///     全屏模式选择按钮
    /// </summary>
    [GetNode] private OptionButton _fullscreenOptionButton = null!;

    private bool _initializing;

    /// <summary>
    ///     语言选择按钮
    /// </summary>
    [GetNode] private OptionButton _languageOptionButton = null!;

    /// <summary>
    ///     主音量控制容器
    /// </summary>
    [GetNode] private VolumeContainer _masterVolumeContainer = null!;

    /// <summary>
    ///     页面行为实例的私有字段
    /// </summary>
    private IUiPageBehavior? _page;

    /// <summary>
    ///     分辨率选择按钮
    /// </summary>
    [GetNode] private OptionButton _resolutionOptionButton = null!;

    /// <summary>
    ///     音效音量控制容器
    /// </summary>
    [GetNode] private VolumeContainer _sfxVolumeContainer = null!;

    private IUiRouter _uiRouter = null!;

    /// <summary>
    ///     Ui Key的字符串形式
    /// </summary>
    public static string UiKeyStr => nameof(UiKey.OptionsMenu);

    /// <summary>
    ///     获取页面行为实例，如果不存在则创建新的CanvasItemUiPageBehavior实例
    /// </summary>
    /// <returns>返回IUiPageBehavior类型的页面行为实例</returns>
    public IUiPageBehavior GetPage()
    {
        _page ??= UiPageBehaviorFactory.Create<Control>(this, UiKeyStr, UiLayer.Modal);
        return _page;
    }

    /// <summary>
    ///     检查当前UI是否在路由栈顶，如果不在则将页面推入路由栈
    /// </summary>
    private void CallDeferredInit()
    {
        CallDeferredInitCoroutine().RunCoroutine(Segment.ProcessIgnorePause);
    }

    /// <summary>
    ///     节点准备就绪时的回调方法
    ///     在节点添加到场景树后调用
    /// </summary>
    public override void _Ready()
    {
        __InjectGetNodes_Generated();
        __InjectContextBindings_Generated();
        InitCoroutine().RunCoroutine();
    }

    private IEnumerator<IYieldInstruction> InitCoroutine()
    {
        GetNode<Button>("%Back").Pressed += OnBackPressed;
        SetupEventHandlers();
        // 获取UI路由器实例
        _uiRouter = this.GetSystem<IUiRouter>()!;
        // 延迟调用初始化方法
        CallDeferred(nameof(CallDeferredInit));
        yield return new Delay(0);
    }

    /// <summary>
    ///     处理未处理的输入事件，用于 ESC 关闭设置窗口
    /// </summary>
    public override void _Input(InputEvent @event)
    {
        if (!@event.IsActionPressed("ui_cancel")) return;

        OnBackPressed();
        AcceptEvent();
    }

    /// <summary>
    ///     当按下返回键时的处理方法
    /// </summary>
    private void OnBackPressed()
    {
        SaveCommandCoroutine().RunCoroutine(Segment.ProcessIgnorePause);
    }

    /// <summary>
    ///     开始保存设置协程
    /// </summary>
    private void StartSaving()
    {
        SaveCommandCoroutine().RunCoroutine(Segment.ProcessIgnorePause);
    }


    /// <summary>
    ///     异步初始化用户界面
    ///     设置音量控制组件和分辨率选项的初始值
    /// </summary>
    private async Task InitializeUiAsync()
    {
        _initializing = true;
        var view = await this.SendQueryAsync(new GetCurrentSettingsQuery()).ConfigureAwait(true);
        var text = _contentCatalog.GetMenuText();
        var audioSettings = view.Audio;
        _masterVolumeContainer.Initialize(text.OptionsMasterVolume, audioSettings.MasterVolume);
        _bgmVolumeContainer.Initialize(text.OptionsBgmVolume, audioSettings.BgmVolume);
        _sfxVolumeContainer.Initialize(text.OptionsSfxVolume, audioSettings.SfxVolume);

        var graphicsSettings = view.Graphics;
        _resolutionOptionButton.Disabled = graphicsSettings.Fullscreen;

        // 初始化全屏选项
        PopulateFullscreenOptions(text, graphicsSettings.Fullscreen);
        // 初始化分辨率选项
        _resolutionOptionButton.Clear();
        for (var i = 0; i < _resolutions.Length; i++)
        {
            var r = _resolutions[i];
            _resolutionOptionButton.AddItem($"{r.X}x{r.Y}");

            if (r.X == graphicsSettings.ResolutionWidth && r.Y == graphicsSettings.ResolutionHeight)
                _resolutionOptionButton.Selected = i; // ⭐ 正确方式
        }

        var localizationSettings = view.Localization;
        PopulateLanguageOptions(text, localizationSettings.Language);
        ApplyMenuTexts(text);
        _initializing = false;
    }

    /// <summary>
    ///     设置事件处理器
    ///     为音量控制、分辨率和全屏模式选择器绑定事件处理逻辑
    /// </summary>
    private void SetupEventHandlers()
    {
        var signalName = VolumeContainer.SignalName.VolumeChanged;
        _masterVolumeContainer
            .Signal(signalName)
            .To(Callable.From<float>(v =>
                this.RunCommandCoroutine(
                    new ChangeMasterVolumeCommand(new ChangeMasterVolumeCommandInput { Volume = v }))))
            .End();
        _bgmVolumeContainer
            .Signal(signalName)
            .To(Callable.From<float>(v =>
                this.RunCommandCoroutine(
                    new ChangeBgmVolumeCommand(
                        new ChangeBgmVolumeCommandInput { Volume = v }))))
            .End();
        _sfxVolumeContainer
            .Signal(signalName)
            .To(Callable.From<float>(v =>
                this.RunCommandCoroutine(
                    new ChangeSfxVolumeCommand(
                        new ChangeSfxVolumeCommandInput { Volume = v }))))
            .End();
        _resolutionOptionButton.ItemSelected += async index => await OnResolutionChanged(index).ConfigureAwait(true);
        _fullscreenOptionButton.ItemSelected += async index => await OnFullscreenChanged(index).ConfigureAwait(true);
        _languageOptionButton.ItemSelected += async index => await OnLanguageChanged(index).ConfigureAwait(true);
    }

    /// <summary>
    ///     语言改变事件
    /// </summary>
    /// <param name="index">选择的语言索引</param>
    private async Task OnLanguageChanged(long index)
    {
        if (_initializing) return;

        // 根据索引获取对应的语言
        var language = index == 0 ? ChineseLanguageValue : EnglishLanguageValue;

        // 发送更改语言命令
        await this.SendCommandAsync(new ChangeLanguageCommand(new ChangeLanguageCommandInput
            { Language = language })).ConfigureAwait(true);

        RefreshLocalizedTexts(language);
        _log.Debug($"语言更改为: {language}");
    }

    /// <summary>
    ///     分辨率改变事件
    /// </summary>
    /// <param name="index">选择的分辨率索引</param>
    private async Task OnResolutionChanged(long index)
    {
        if (_initializing) return;
        var resolution = _resolutions[index];
        await this.SendCommandAsync(new ChangeResolutionCommand(new ChangeResolutionCommandInput
            { Width = resolution.X, Height = resolution.Y })).ConfigureAwait(true);
        _log.Debug($"分辨率更改为: {resolution.X}x{resolution.Y}");
    }

    /// <summary>
    ///     全屏模式改变事件
    /// </summary>
    /// <param name="index">选择的全屏模式索引</param>
    private async Task OnFullscreenChanged(long index)
    {
        if (_initializing) return;
        var fullscreen = index == 0;
        await this.SendCommandAsync(new ToggleFullscreenCommand(new ToggleFullscreenCommandInput
            { Fullscreen = fullscreen })).ConfigureAwait(true);
        // ⭐ 禁用 / 启用分辨率选择
        _resolutionOptionButton.Disabled = fullscreen;
        _log.Debug($"全屏模式切换为: {fullscreen}");
    }

    /// <summary>
    ///     初始化协程，用于设置界面的初始化流程
    /// </summary>
    /// <returns>返回一个IYieldInstruction类型的IEnumerator，用于协程执行</returns>
    private IEnumerator<IYieldInstruction> CallDeferredInitCoroutine()
    {
        Hide();
        var settings = this.GetModel<ISettingsModel>()!;
        var eventBus = this.GetService<IEventBus>()!;
        if (!settings.IsInitialized)
            // 等待设置初始化事件完成
            yield return new WaitForEvent<SettingsInitializedEvent>(eventBus);

        yield return InitializeUiAsync().AsCoroutineInstruction();
        Show();
    }

    /// <summary>
    ///     保存命令协程，用于处理设置保存操作
    /// </summary>
    /// <returns>返回一个IYieldInstruction类型的IEnumerator，用于协程执行</returns>
    private IEnumerator<IYieldInstruction> SaveCommandCoroutine()
    {
        return this.SendCommandCoroutine(
                new SaveSettingsCommand(),
                e => _log.Error("保存失败！", e)
            )
            .Then(() =>
            {
                _log.Info("设置已保存");
                var handle = GetPage().Handle;
                if (handle.HasValue)
                    _uiRouter.Hide(handle.Value, UiLayer.Modal, true);
                else
                    _log.Warn("页面句柄为空，无法隐藏页面");
            });
    }

    private void RefreshLocalizedTexts(string selectedLanguage)
    {
        _initializing = true;
        var text = _contentCatalog.GetMenuText();
        ApplyMenuTexts(text);
        PopulateFullscreenOptions(text, _fullscreenOptionButton.Selected == 0);
        PopulateLanguageOptions(text, selectedLanguage);
        _initializing = false;
    }

    private void ApplyMenuTexts(MenuTextConfig text)
    {
        GetNode<Label>("Panel/MarginContainer/HBoxContainer/MarginContainer/HBoxContainer/Title").Text =
            text.OptionsTitle;
        GetNode<Label>("Panel/MarginContainer/HBoxContainer/MarginContainer/HBoxContainer/Audio/Title").Text =
            text.OptionsAudioTitle;
        GetNode<Label>("Panel/MarginContainer/HBoxContainer/MarginContainer/HBoxContainer/Graphics/Title").Text =
            text.OptionsGraphicsTitle;
        GetNode<Label>("Panel/MarginContainer/HBoxContainer/MarginContainer/HBoxContainer/Localization/Title").Text =
            text.OptionsLocalizationTitle;
        GetNode<Label>(
                "Panel/MarginContainer/HBoxContainer/MarginContainer/HBoxContainer/Graphics/MarginContainer/FullscreenContainer/FullscreenLabel")
            .Text = text.OptionsFullscreenLabel;
        GetNode<Label>(
                "Panel/MarginContainer/HBoxContainer/MarginContainer/HBoxContainer/Graphics/MarginContainer2/ResolutionContainer/ResolutionLabel")
            .Text = text.OptionsResolutionLabel;
        GetNode<Label>(
                "Panel/MarginContainer/HBoxContainer/MarginContainer/HBoxContainer/Localization/MarginContainer/LanguageContainer/LanguageLabel")
            .Text = text.OptionsLanguageLabel;
        GetNode<Button>("%Back").Text = text.OptionsBack;

        _masterVolumeContainer.SetTitle(text.OptionsMasterVolume);
        _bgmVolumeContainer.SetTitle(text.OptionsBgmVolume);
        _sfxVolumeContainer.SetTitle(text.OptionsSfxVolume);
    }

    private void PopulateFullscreenOptions(MenuTextConfig text, bool fullscreen)
    {
        _fullscreenOptionButton.Clear();
        _fullscreenOptionButton.AddItem(text.OptionsFullscreen);
        _fullscreenOptionButton.AddItem(text.OptionsWindowed);
        _fullscreenOptionButton.Selected = fullscreen ? 0 : 1;
    }

    private void PopulateLanguageOptions(MenuTextConfig text, string selectedLanguage)
    {
        _languageOptionButton.Clear();
        _languageOptionButton.AddItem(text.OptionsLanguageZh);
        _languageOptionButton.AddItem(text.OptionsLanguageEn);
        _languageOptionButton.Selected =
            string.Equals(selectedLanguage, ChineseLanguageValue, StringComparison.Ordinal) ? 0 : 1;
    }
}
