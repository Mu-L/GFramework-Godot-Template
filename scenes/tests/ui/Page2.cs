using GFramework.Core.Abstractions.controller;
using GFramework.SourceGenerators.Abstractions.logging;
using GFramework.SourceGenerators.Abstractions.rule;
using GFrameworkGodotTemplate.scripts.core.ui;
using Godot;

[ContextAware]
[Log]
public partial class Page2 : Control,IController,IUiPageProvider
{
	private ControlUiPageBehavior? _page;
	public IUiPage GetPage()
	{
		_page ??= new ControlUiPageBehavior(this);
		return _page;
	}
	/// <summary>
	/// 节点准备就绪时的回调方法
	/// 在节点添加到场景树后调用
	/// </summary>
	public override void _Ready()
	{
		
	}

	public void OnEnter(IUiPageEnterParam? param)
	{
		_log.Info("Page2 OnEnter");
	}
	
}
