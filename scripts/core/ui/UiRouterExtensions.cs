using System;
using System.Threading;
using System.Threading.Tasks;

namespace GFrameworkGodotTemplate.scripts.core.ui;

/// <summary>
/// UiRouter 扩展方法，提供一次性 Handler 注册的便捷 API
/// </summary>
public static class UiRouterExtensions
{
    /// <summary>
    /// 注册一次性 UI 切换处理器（默认 BeforeChange 阶段）
    /// </summary>
    /// <param name="router">UI 路由器实例</param>
    /// <param name="priority">优先级，默认为 100</param>
    /// <param name="phases">适用的阶段，默认为 BeforeChange</param>
    /// <param name="shouldHandle">判断是否应该处理当前事件（可选）</param>
    /// <param name="handle">处理逻辑</param>
    /// <param name="onExecuted">执行完成后的回调（可选）</param>
    /// <returns>创建的一次性 Handler，可用于后续手动注销</returns>
    public static OneShotTransitionHandler RegisterOneShot(
        this IUiRouter router,
        int priority = 100,
        UITransitionPhases phases = UITransitionPhases.BeforeChange,
        Func<UiTransitionEvent, UITransitionPhases, bool>? shouldHandle = null,
        Func<UiTransitionEvent, CancellationToken, Task>? handle = null,
        Action? onExecuted = null
    )
    {
        if (handle == null)
            throw new ArgumentNullException(nameof(handle));

        var handler = new OneShotTransitionHandler(
            priority,
            phases,
            shouldHandle,
            handle,
            onExecuted
        );

        router.RegisterHandler(handler);
        return handler;
    }

    /// <summary>
    /// 注册一次性 UI 切换处理器（同步版本）
    /// </summary>
    /// <param name="router">UI 路由器实例</param>
    /// <param name="priority">优先级，默认为 100</param>
    /// <param name="phases">适用的阶段，默认为 BeforeChange</param>
    /// <param name="shouldHandle">判断是否应该处理当前事件（可选）</param>
    /// <param name="handle">处理逻辑</param>
    /// <param name="onExecuted">执行完成后的回调（可选）</param>
    /// <returns>创建的一次性 Handler，可用于后续手动注销</returns>
    public static OneShotTransitionHandler RegisterOneShot(
        this IUiRouter router,
        int priority,
        UITransitionPhases phases,
        Func<UiTransitionEvent, UITransitionPhases, bool>? shouldHandle,
        Action<UiTransitionEvent> handle,
        Action? onExecuted = null
    )
    {
        if (handle == null)
            throw new ArgumentNullException(nameof(handle));

        var handler = new OneShotTransitionHandler(
            priority,
            phases,
            shouldHandle,
            (@event, ct) =>
            {
                handle(@event);
                return Task.CompletedTask;
            },
            onExecuted
        );

        router.RegisterHandler(handler);
        return handler;
    }

    /// <summary>
    /// 创建一次性 Handler 构建器，提供流式 API
    /// </summary>
    /// <param name="router">UI 路由器实例</param>
    /// <returns>构建器实例</returns>
    public static OneShotHandlerBuilder CreateOneShot(this IUiRouter router)
    {
        return new OneShotHandlerBuilder(router);
    }
}
