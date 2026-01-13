using GFramework.Core.Abstractions.controller;
using GFramework.Core.extensions;
using GFramework.SourceGenerators.Abstractions.logging;
using GFramework.SourceGenerators.Abstractions.rule;
using GFrameworkGodotTemplate.scripts.core.constants;
using GFrameworkGodotTemplate.scripts.core.ui;
using Godot;

namespace GFrameworkGodotTemplate.scenes.tests.ui.main_menu;

[ContextAware]
[Log]
public partial class TestMainMenu : ControlUiPage, IController, IUiPage
{
    private Button Page1Button => GetNode<Button>("%Page1Button");
    private Button Page2Button => GetNode<Button>("%Page2Button");
    private Button Page3Button => GetNode<Button>("%Page3Button");

    /// <summary>
    /// 节点准备就绪时的回调方法
    /// 在节点添加到场景树后调用
    /// </summary>
    public override void _Ready()
    {
        var _uiRouter = ContextAwareExtensions.GetSystem<IUiRouter>(this)!;
        Page1Button.Pressed += () => { _uiRouter.Push(UiKeys.Page1); };
        Page2Button.Pressed += () => { _uiRouter.Push(UiKeys.Page2); };
        Page3Button.Pressed += () => { _uiRouter.Push(UiKeys.Page3); };
    }

    public void OnEnter(IUiPageEnterParam? param)
    {
        _log.Info("测试主菜单 OnEnter");
    }
}