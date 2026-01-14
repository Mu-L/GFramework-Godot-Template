using System;
using System.Threading;
using System.Threading.Tasks;
using GFramework.SourceGenerators.Abstractions.logging;

namespace GFrameworkGodotTemplate.scripts.core.ui;

/// <summary>
/// 音频UI切换处理器，用于播放UI切换音效和背景音乐
/// </summary>
[Log]
public sealed partial class AudioTransitionHandler : UiTransitionHandlerBase
{
    public override int Priority => 200;
    public override UITransitionPhases Phases => UITransitionPhases.All;

    public override bool ShouldHandle(UiTransitionEvent @event, UITransitionPhases phases)
    {
        if (phases == UITransitionPhases.BeforeChange)
        {
            return @event.TransitionType != UiTransitionType.Pop;
        }
        return true;
    }

    public override async Task HandleAsync(UiTransitionEvent @event, CancellationToken cancellationToken)
    {
        if (@event.TryGet<string>("Phases", out var phase))
        {
            if (string.Equals(phase, "BeforeChange", StringComparison.Ordinal))
            {
                await HandleBeforeChangeAsync(@event, cancellationToken).ConfigureAwait(false);
            }
            else if (string.Equals(phase, "AfterChange", StringComparison.Ordinal))
            {
                await HandleAfterChangeAsync(@event, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private Task HandleBeforeChangeAsync(UiTransitionEvent @event, CancellationToken cancellationToken)
    {
        _log.Debug("Audio: Playing UI switch sound from {0} to {1}", @event.FromUiKey, @event.ToUiKey);
        return Task.CompletedTask;
    }

    private Task HandleAfterChangeAsync(UiTransitionEvent @event, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(@event.ToUiKey))
        {
            _log.Debug("Audio: Playing BGM for UI: {0}", @event.ToUiKey);
        }
        return Task.CompletedTask;
    }
}
