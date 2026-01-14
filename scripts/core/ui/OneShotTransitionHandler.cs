using System;
using System.Threading;
using System.Threading.Tasks;

namespace GFrameworkGodotTemplate.scripts.core.ui;

/// <summary>
/// 一次性 UI 切换处理器，支持 Lambda 表达式快速定义逻辑
/// </summary>
public sealed class OneShotTransitionHandler : IUiTransitionHandler
{
    private readonly Func<UiTransitionEvent, UITransitionPhases, bool> _shouldHandle;
    private readonly Func<UiTransitionEvent, CancellationToken, Task> _handle;
    private readonly Action? _onExecuted;
    private bool _executed;

    /// <summary>
    /// 创建一次性 UI 切换处理器
    /// </summary>
    /// <param name="priority">优先级</param>
    /// <param name="phases">适用的阶段</param>
    /// <param name="shouldHandle">判断是否应该处理当前事件</param>
    /// <param name="handle">处理逻辑</param>
    /// <param name="onExecuted">执行完成后的回调（可选）</param>
    public OneShotTransitionHandler(
        int priority,
        UITransitionPhases phases,
        Func<UiTransitionEvent, UITransitionPhases, bool>? shouldHandle,
        Func<UiTransitionEvent, CancellationToken, Task> handle,
        Action? onExecuted = null
    )
    {
        Priority = priority;
        Phases = phases;
        _shouldHandle = shouldHandle ?? ((@event, phase) => true);
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        _onExecuted = onExecuted;
    }

    /// <summary>
    /// 优先级
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// 适用的阶段
    /// </summary>
    public UITransitionPhases Phases { get; }

    /// <summary>
    /// 判断是否应该处理当前事件
    /// </summary>
    public bool ShouldHandle(UiTransitionEvent @event, UITransitionPhases phases)
    {
        if (_executed)
            return false;

        return _shouldHandle(@event, phases);
    }

    /// <summary>
    /// 处理 UI 切换事件
    /// </summary>
    public async Task HandleAsync(UiTransitionEvent @event, CancellationToken cancellationToken)
    {
        if (_executed)
            return;

        await _handle(@event, cancellationToken);

        _executed = true;
        _onExecuted?.Invoke();
    }

    /// <summary>
    /// 是否已经执行过
    /// </summary>
    public bool IsExecuted => _executed;
}
