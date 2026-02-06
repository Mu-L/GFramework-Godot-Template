using GFramework.Core.Abstractions.controller;
using Godot;

namespace GFrameworkGodotTemplate.scripts.core.controller;

/// <summary>
/// 游戏输入控制器抽象基类，继承自Node并实现IController接口
/// 负责处理游戏中的未处理输入事件
/// </summary>
public abstract partial class GameInputController
    : Node, IController
{
    /// <summary>
    /// 获取当前场景树的引用
    /// </summary>
    protected SceneTree Tree => GetTree();

    /// <summary>
    /// 是否允许在暂停状态下接收输入
    /// 默认 false：Gameplay Controller 不允许
    /// </summary>
    protected virtual bool AllowWhenPaused => false;

    /// <summary>
    /// 系统级输入处理方法，在_pause状态下也会被调用
    /// 该方法会检查是否允许在暂停时处理输入，并根据条件调用HandleWhenPaused
    /// </summary>
    /// <param name="event">输入事件对象</param>
    public override void _Input(InputEvent @event)
    {
        // 如果游戏未暂停，直接返回
        if (!Tree.Paused)
            return;

        // 如果不允许在暂停时处理输入，直接返回
        if (!AllowWhenPaused)
            return;

        // 如果输入被阻塞，直接返回
        if (IsBlocked())
            return;

        // 处理暂停状态下的输入
        HandleWhenPaused(@event);
    }

    /// <summary>
    /// 正常 Gameplay 输入处理方法，在非暂停状态下被调用
    /// 该方法会检查输入是否被阻塞，并根据条件调用HandleUnhandled
    /// </summary>
    /// <param name="event">输入事件对象</param>
    public override void _UnhandledInput(InputEvent @event)
    {
        // 如果游戏处于暂停状态，直接返回
        if (Tree.Paused)
            return;

        // 如果输入被阻塞，直接返回
        if (IsBlocked())
            return;

        // 处理正常状态下的未处理输入
        HandleUnhandled(@event);
    }

    /// <summary>
    /// 暂停状态下的输入处理逻辑（默认不处理）
    /// 子类可以重写此方法以实现特定的暂停输入处理逻辑
    /// </summary>
    /// <param name="event">输入事件对象</param>
    protected virtual void HandleWhenPaused(InputEvent @event)
    {
        // 默认情况下不执行任何操作
    }

    /// <summary>
    /// 正常未处理输入的处理逻辑（必须由子类实现）
    /// 子类需要重写此方法以定义具体的输入处理行为
    /// </summary>
    /// <param name="event">输入事件对象</param>
    protected abstract void HandleUnhandled(InputEvent @event);

    /// <summary>
    /// 检查输入是否被阻塞
    /// 默认返回false，表示输入未被阻塞
    /// 子类可以重写此方法以实现自定义的阻塞逻辑
    /// </summary>
    /// <returns>如果输入被阻塞则返回true，否则返回false</returns>
    protected virtual bool IsBlocked() => false;
}