using Godot;

namespace GFrameworkGodotTemplate.scripts.ui.component;

/// <summary>
///     兼容旧场景的通用停靠动画面板。
/// </summary>
public partial class AnimatedDockPanel : BaseAnimatedDockPanel
{
    public AnimatedDockPanel()
    {
        ExpandedButtonText = "◀";
        CollapsedButtonText = "▶";
    }

    /// <summary>
    ///     停靠方向。
    /// </summary>
    [Export]
    public DockEdge Edge { get; set; } = DockEdge.Left;

    protected override bool IsHorizontalDock => Edge is DockEdge.Left or DockEdge.Right;

    protected override Vector2 GetExpandedPanelPosition(Vector2 panelSize)
    {
        return Edge switch
        {
            DockEdge.Right => new Vector2(Size.X - VisualPadding - panelSize.X, VisualPadding),
            DockEdge.Bottom => new Vector2(VisualPadding, Size.Y - VisualPadding - panelSize.Y),
            _ => new Vector2(VisualPadding, VisualPadding)
        };
    }

    protected override Vector2 GetCollapsedTranslation(float panelOffset)
    {
        return Edge switch
        {
            DockEdge.Left => new Vector2(-panelOffset, 0f),
            DockEdge.Right => new Vector2(panelOffset, 0f),
            DockEdge.Top => new Vector2(0f, -panelOffset),
            DockEdge.Bottom => new Vector2(0f, panelOffset),
            _ => Vector2.Zero
        };
    }

    protected override Vector2 GetTogglePosition(
        Vector2 panelPosition,
        Vector2 panelSize,
        Vector2 toggleSize,
        float gap,
        float toggleCrossPosition
    )
    {
        return Edge switch
        {
            DockEdge.Left => new Vector2(panelPosition.X + panelSize.X + gap, toggleCrossPosition),
            DockEdge.Right => new Vector2(panelPosition.X - toggleSize.X - gap, toggleCrossPosition),
            DockEdge.Top => new Vector2(toggleCrossPosition, panelPosition.Y + panelSize.Y + gap),
            DockEdge.Bottom => new Vector2(toggleCrossPosition, panelPosition.Y - toggleSize.Y - gap),
            _ => panelPosition
        };
    }
}