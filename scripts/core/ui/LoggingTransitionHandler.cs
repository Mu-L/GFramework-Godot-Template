using System.Threading;
using System.Threading.Tasks;
using GFramework.SourceGenerators.Abstractions.logging;

namespace GFrameworkGodotTemplate.scripts.core.ui;

/// <summary>
/// 日志UI切换处理器，用于记录UI切换的详细信息
/// </summary>
[Log]
public sealed partial class LoggingTransitionHandler : UiTransitionHandlerBase
{
    public override int Priority => 999;
    public override UITransitionPhases Phases => UITransitionPhases.All;

    public override Task HandleAsync(UiTransitionEvent @event, CancellationToken cancellationToken)
    {
        _log.Info(
            "UI Transition: Phases={0}, Type={1}, From={2}, To={3}, Policy={4}",
            @event.Get<string>("Phases", "Unknown"),
            @event.TransitionType,
            @event.FromUiKey,
            @event.ToUiKey,
            @event.Policy
        );

        return Task.CompletedTask;
    }
}
