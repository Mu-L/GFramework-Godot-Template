using GFramework.Core.Abstractions.state;
using GFramework.Core.extensions;
using GFramework.SourceGenerators.Abstractions.logging;
using GFramework.SourceGenerators.Abstractions.rule;
using GFrameworkGodotTemplate.scripts.command.game;
using GFrameworkGodotTemplate.scripts.core.controller;
using GFrameworkGodotTemplate.scripts.core.state.impls;
using Godot;

namespace GFrameworkGodotTemplate.global;

/// <summary>
/// 全局输入控制器类，继承自 GameInputController。
/// 负责处理游戏中的全局输入事件，包括暂停和恢复游戏的功能。
/// </summary>
[ContextAware]
[Log]
public partial class GlobalInputController : GameInputController
{
    /// <summary>
    /// 状态机系统实例，用于管理游戏状态。
    /// </summary>
    private IStateMachineSystem _stateMachineSystem = null!;

    /// <summary>
    /// 获取一个布尔值，指示是否允许在游戏暂停时处理输入。
    /// </summary>
    protected override bool AllowWhenPaused => true;

    /// <summary>
    /// 初始化方法，在节点准备就绪时调用。
    /// 获取并初始化状态机系统实例。
    /// </summary>
    public override void _Ready()
    {
        _stateMachineSystem = this.GetSystem<IStateMachineSystem>()!;
    }

    /// <summary>
    /// 处理未被其他处理器捕获的输入事件。
    /// 在 Playing 状态下，按下 ESC 键将触发暂停游戏操作。
    /// </summary>
    /// <param name="event">输入事件对象。</param>
    protected override void HandleUnhandled(InputEvent @event)
    {
        // 检查是否按下了取消操作（通常是 ESC 键）
        if (!@event.IsActionPressed("ui_cancel"))
            return;

        // 根据当前状态执行相应操作
        switch (_stateMachineSystem.Current)
        {
            case PlayingState:
                _log.Debug("暂停游戏");
                this.SendCommand(new PauseGameWithOpenPauseMenuCommand());
                break;
        }
    }

    /// <summary>
    /// 处理在游戏暂停状态下接收到的输入事件。
    /// 在 Paused 状态下，按下 ESC 键将触发恢复游戏操作。
    /// </summary>
    /// <param name="event">输入事件对象。</param>
    protected override void HandleWhenPaused(InputEvent @event)
    {
        // 检查是否按下了取消操作（通常是 ESC 键）
        if (!@event.IsActionPressed("ui_cancel"))
            return;

        // 根据当前状态执行相应操作
        switch (_stateMachineSystem.Current)
        {
            case PausedState:
                _log.Debug("恢复游戏");
                this.SendCommand(new ResumeGameWithClosePauseMenuCommand());
                break;
        }
    }
}