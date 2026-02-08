using GFramework.Core.Abstractions.controller;
using GFramework.Core.Abstractions.coroutine;
using GFramework.Core.coroutine.extensions;
using GFramework.Core.coroutine.instructions;
using GFramework.SourceGenerators.Abstractions.logging;
using GFramework.SourceGenerators.Abstractions.rule;
using Godot;

namespace GFrameworkGodotTemplate.global;

/// <summary>
/// 场景过渡管理器，负责处理场景之间的过渡动画。
/// 实现了 IController 接口，并继承自 Godot 的 Node 类。
/// 提供异步过渡功能，支持捕获当前场景、执行过渡动画以及切换场景。
/// </summary>
[ContextAware]
[Log]
public partial class SceneTransitionManager : Node, IController
{
    private static readonly string Progress = "progress";

    /// <summary>
    /// 过渡效果使用的着色器材质。
    /// </summary>
    private ShaderMaterial _material = null!;

    /// <summary>
    /// 获取或设置场景过渡管理器的单例实例。
    /// </summary>
    public static SceneTransitionManager? Instance { get; private set; }

    /// <summary>
    /// 获取用于显示过渡效果的 ColorRect 节点。
    /// </summary>
    private ColorRect SceneTransitionRect => GetNode<ColorRect>("%SceneTransitionRect");

    /// <summary>
    /// 获取或设置是否正在进行场景过渡。
    /// </summary>
    public bool IsTransitioning { get; private set; }

    /// <summary>
    /// 节点准备就绪时的回调方法。
    /// 在节点添加到场景树后调用，初始化单例实例、材质和场景路由器。
    /// </summary>
    public override void _Ready()
    {
        Instance = this;
        _material = (ShaderMaterial)SceneTransitionRect.Material;
        SceneTransitionRect.Visible = false;
    }


    /// <summary>
    /// 执行场景过渡的协程逻辑，包括捕获当前画面、执行过渡动画、切换场景以及捕获新画面。
    /// </summary>
    /// <param name="onSwitch">在场景切换时执行的协程逻辑。</param>
    /// <param name="duration">整个过渡过程的总持续时间（单位：秒），默认值为 0.6 秒。</param>
    /// <returns>返回一个可枚举的协程指令，用于控制过渡流程的执行。</returns>
    public IEnumerator<IYieldInstruction> PlayTransitionCoroutine(IEnumerator<IYieldInstruction> onSwitch,
        float duration = 0.6f)
    {
        IsTransitioning = true;

        // 1. 截图当前画面
        var captureFromInstruction = CaptureScreenshot().AsCoroutineInstruction();
        yield return captureFromInstruction;
        var fromTexture = captureFromInstruction.Result;

        _log.Debug($"捕获旧画面: {fromTexture.GetWidth()}x{fromTexture.GetHeight()}");

        // 2. 执行场景切换
        yield return new WaitForCoroutine(onSwitch);

        // 3. 等待新场景渲染
        yield return new WaitOneFrame();
        yield return new WaitOneFrame();

        // 4. 截图新画面
        var captureToInstruction = CaptureScreenshot().AsCoroutineInstruction();
        yield return captureToInstruction;
        var toTexture = captureToInstruction.Result;

        _log.Debug($"捕获新画面: {toTexture.GetWidth()}x{toTexture.GetHeight()}");

        // 5. 设置纹理
        _material.SetShaderParameter("from_tex", fromTexture);
        _material.SetShaderParameter("to_tex", toTexture);
        _material.SetShaderParameter(Progress, 0.0f);

        // 6. 显示过渡层
        SceneTransitionRect.Visible = true;

        // 等待一帧
        yield return new WaitOneFrame();

        // 7. 使用协程版本的进度更新
        yield return new WaitForCoroutine(TweenProgressCoroutine(0f, 1f, duration));

        // 8. 清理
        SceneTransitionRect.Visible = false;
        _material.SetShaderParameter(Progress, 0.0f);

        fromTexture.Dispose();
        toTexture.Dispose();

        IsTransitioning = false;
    }

    /// <summary>
    /// 捕获整个屏幕的截图（包括所有 UI 层和场景）
    /// </summary>
    private async Task<ImageTexture> CaptureScreenshot()
    {
        // 临时隐藏过渡层，避免截图包含它
        var wasVisible = SceneTransitionRect.Visible;
        SceneTransitionRect.Visible = false;

        // 等待渲染完成
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);

        // 获取视口的纹理
        var viewport = GetViewport();
        var image = viewport.GetTexture().GetImage();

        // 恢复可见性
        SceneTransitionRect.Visible = wasVisible;

        // 转换为 ImageTexture
        var texture = ImageTexture.CreateFromImage(image);

        return texture;
    }


    /// <summary>
    /// 协程版本的进度插值
    /// </summary>
    private IEnumerator<IYieldInstruction> TweenProgressCoroutine(float from, float to, float duration)
    {
        _material.SetShaderParameter(Progress, from);

        var tween = CreateTween();
        tween.TweenMethod(
            Callable.From<float>(v =>
            {
                _material.SetShaderParameter(Progress, v);
                _log.Debug($"Progress: {v}"); // 调试用
            }),
            from,
            to,
            duration
        );
        yield return new WaitForTask(ToSignal(tween, Tween.SignalName.Finished).AsTask());
    }
}