using System.Threading;
using System.Threading.Tasks;
using GFramework.SourceGenerators.Abstractions.logging;

namespace GFrameworkGodotTemplate.scripts.core.ui;

/// <summary>
/// 示例UI切换处理器：延迟处理器
/// 用于演示如何在BeforeChange阶段延迟UI切换
/// </summary>
[Log]
public sealed partial class DelayTransitionHandler : UiTransitionHandlerBase
{
    public override int Priority => 50;
    public override UITransitionPhases Phases => UITransitionPhases.BeforeChange;

    public override bool ShouldHandle(UiTransitionEvent @event, UITransitionPhases phases)
    {
        return @event.TransitionType == UiTransitionType.Push;
    }

    public override async Task HandleAsync(UiTransitionEvent @event, CancellationToken cancellationToken)
    {
        _log.Info("Delaying UI transition by 500ms...");
        await Task.Delay(500, cancellationToken).ConfigureAwait(false);
        _log.Info("Delay completed, proceeding with UI transition");
    }
}
