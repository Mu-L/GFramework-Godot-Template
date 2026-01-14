using System;
using System.Threading;
using System.Threading.Tasks;

namespace GFrameworkGodotTemplate.scripts.core.ui;

/// <summary>
/// 一次性 UI 切换处理器构建器，提供流式 API
/// </summary>
public sealed class OneShotHandlerBuilder
{
    private readonly IUiRouter _router;
    private int _priority = 100;
    private UITransitionPhases _phases = UITransitionPhases.BeforeChange;
    private Func<UiTransitionEvent, UITransitionPhases, bool>? _shouldHandle;
    private Func<UiTransitionEvent, CancellationToken, Task>? _handle;
    private Action? _onExecuted;

    internal OneShotHandlerBuilder(IUiRouter router)
    {
        _router = router ?? throw new ArgumentNullException(nameof(router));
    }

    /// <summary>
    /// 设置优先级
    /// </summary>
    public OneShotHandlerBuilder WithPriority(int priority)
    {
        _priority = priority;
        return this;
    }

    /// <summary>
    /// 设置适用的阶段
    /// </summary>
    public OneShotHandlerBuilder WithPhases(UITransitionPhases phases)
    {
        _phases = phases;
        return this;
    }

    /// <summary>
    /// 设置判断逻辑
    /// </summary>
    public OneShotHandlerBuilder When(
        Func<UiTransitionEvent, UITransitionPhases, bool> shouldHandle)
    {
        _shouldHandle = shouldHandle;
        return this;
    }

    /// <summary>
    /// 设置异步处理逻辑
    /// </summary>
    public OneShotHandlerBuilder HandleAsync(
        Func<UiTransitionEvent, CancellationToken, Task> handle)
    {
        _handle = handle;
        return this;
    }

    /// <summary>
    /// 设置同步处理逻辑
    /// </summary>
    public OneShotHandlerBuilder Handle(Action<UiTransitionEvent> handle)
    {
        _handle = (@event, ct) =>
        {
            handle(@event);
            return Task.CompletedTask;
        };
        return this;
    }

    /// <summary>
    /// 设置执行完成后的回调
    /// </summary>
    public OneShotHandlerBuilder OnExecuted(Action onExecuted)
    {
        _onExecuted = onExecuted;
        return this;
    }

    /// <summary>
    /// 构建并注册一次性 Handler
    /// </summary>
    public OneShotTransitionHandler Register()
    {
        if (_handle == null)
            throw new InvalidOperationException("处理逻辑未设置，请先调用 HandleAsync 或 Handle 方法");

        var handler = new OneShotTransitionHandler(
            _priority,
            _phases,
            _shouldHandle,
            _handle,
            _onExecuted
        );

        _router.RegisterHandler(handler);
        return handler;
    }
}
