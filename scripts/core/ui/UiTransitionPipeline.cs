using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GFramework.SourceGenerators.Abstractions.logging;

namespace GFrameworkGodotTemplate.scripts.core.ui;

/// <summary>
/// UI切换处理器管道，负责管理和执行UI切换扩展点
/// </summary>
[Log]
public partial class UiTransitionPipeline
{
    private readonly List<IUiTransitionHandler> _handlers = new();
    private readonly Dictionary<IUiTransitionHandler, UiTransitionHandlerOptions> _options = new();

    /// <summary>
    /// 注册UI切换处理器
    /// </summary>
    /// <param name="handler">处理器实例</param>
    /// <param name="options">执行选项</param>
    public void RegisterHandler(IUiTransitionHandler handler, UiTransitionHandlerOptions? options = null)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        if (_handlers.Contains(handler))
        {
            _log.Debug("Handler already registered: {0}", handler.GetType().Name);
            return;
        }

        _handlers.Add(handler);
        _options[handler] = options ?? new UiTransitionHandlerOptions();
        _log.Debug(
            "Handler registered: {0}, Priority={1}, Phases={2}, TimeoutMs={3}",
            handler.GetType().Name,
            handler.Priority,
            handler.Phases,
            _options[handler].TimeoutMs
        );
    }

    /// <summary>
    /// 注销UI切换处理器
    /// </summary>
    /// <param name="handler">处理器实例</param>
    public void UnregisterHandler(IUiTransitionHandler handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        if (_handlers.Remove(handler))
        {
            _options.Remove(handler);
            _log.Debug("Handler unregistered: {0}", handler.GetType().Name);
        }
    }

    /// <summary>
    /// 执行指定阶段的所有Handler
    /// </summary>
    /// <param name="event">UI切换事件</param>
    /// <param name="phases">执行阶段</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    public async Task ExecuteAsync(
        UiTransitionEvent @event,
        UITransitionPhases phases,
        CancellationToken cancellationToken = default
    )
    {
        @event.Set("Phases", phases.ToString());

        _log.Debug(
            "Execute pipeline: Phases={0}, From={1}, To={2}, Type={3}, HandlerCount={4}",
            phases,
            @event.FromUiKey,
            @event.ToUiKey,
            @event.TransitionType,
            _handlers.Count
        );

        var sortedHandlers = _handlers
            .Where(h => h.Phases.HasFlag(phases) && h.ShouldHandle(@event, phases))
            .OrderBy(h => h.Priority)
            .ToList();

        if (sortedHandlers.Count == 0)
        {
            _log.Debug("No handlers to execute for phases: {0}", phases);
            return;
        }

        _log.Debug(
            "Executing {0} handlers for phases {1}",
            sortedHandlers.Count,
            phases
        );

        foreach (var handler in sortedHandlers)
        {
            var options = _options[handler];

            try
            {
                _log.Debug(
                    "Executing handler: {0}, Priority={1}",
                    handler.GetType().Name,
                    handler.Priority
                );

                using var timeoutCts = options.TimeoutMs > 0
                    ? new CancellationTokenSource(options.TimeoutMs)
                    : null;

                using var linkedCts = timeoutCts != null && cancellationToken.CanBeCanceled
                    ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token)
                    : null;

                await handler.HandleAsync(
                    @event,
                    linkedCts?.Token ?? cancellationToken
                ).ConfigureAwait(false);

                _log.Debug("Handler completed: {0}", handler.GetType().Name);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _log.Error(
                    "Handler timeout: {0}, TimeoutMs={1}",
                    handler.GetType().Name,
                    options.TimeoutMs
                );

                if (!options.ContinueOnError)
                {
                    _log.Error("Stopping pipeline due to timeout and ContinueOnError=false");
                    throw;
                }
            }
            catch (OperationCanceledException)
            {
                _log.Debug("Handler cancelled: {0}", handler.GetType().Name);
                throw;
            }
            catch (Exception ex)
            {
                _log.Error("Handler failed: {0}, Error: {1}", handler.GetType().Name, ex.Message);

                if (!options.ContinueOnError)
                {
                    _log.Error("Stopping pipeline due to error and ContinueOnError=false");
                    throw;
                }
            }
        }

        _log.Debug("Pipeline execution completed for phases: {0}", phases);
    }
}
