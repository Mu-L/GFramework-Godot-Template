using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GFramework.Core.extensions;
using GFramework.Core.system;
using GFramework.SourceGenerators.Abstractions.logging;

namespace GFrameworkGodotTemplate.scripts.core.ui;

/// <summary>
/// UI路由类，提供页面栈管理功能
/// </summary>
[Log]
public partial class UiRouter : AbstractSystem, IUiRouter
{
    private IUiRoot _uiRoot = null!;

    /// <summary>
    /// UI工厂实例，用于创建UI相关的对象
    /// </summary>
    private IUiFactory _factory= null!;

    /// <summary>
    /// 页面栈，用于管理UI页面的显示顺序
    /// </summary>
    private readonly Stack<IPageBehavior> _stack = new();

    /// <summary>
    /// UI切换处理器管道
    /// </summary>
    private readonly UiTransitionPipeline _pipeline = new();

    /// <summary>
    /// 初始化方法，在页面初始化时获取UI工厂实例
    /// </summary>
    protected override void OnInit()
    {
        _factory = this.GetUtility<IUiFactory>()!;
        _log.Debug("UiRouter initialized. Factory={0}", _factory.GetType().Name);
        RegisterDefaultHandlers();
    }

    /// <summary>
    /// 注册默认的UI切换处理器
    /// </summary>
    private void RegisterDefaultHandlers()
    {
        _log.Debug("Registering default transition handlers");

        RegisterHandler(new LoggingTransitionHandler());
        RegisterHandler(new AudioTransitionHandler());
    }

    /// <summary>
    /// 获取当前栈顶UI的key
    /// </summary>
    /// <returns>当前UI key，如果栈为空则返回空字符串</returns>
    private string GetCurrentUiKey()
    {
        if (_stack.Count == 0)
            return string.Empty;
        var page = _stack.Peek();
        return page.View.GetType().Name;
    }

    /// <summary>
    /// 注册UI切换处理器
    /// </summary>
    /// <param name="handler">处理器实例</param>
    /// <param name="options">执行选项</param>
    public void RegisterHandler(IUiTransitionHandler handler, UiTransitionHandlerOptions? options = null)
    {
        _pipeline.RegisterHandler(handler, options);
    }

    /// <summary>
    /// 注销UI切换处理器
    /// </summary>
    /// <param name="handler">处理器实例</param>
    public void UnregisterHandler(IUiTransitionHandler handler)
    {
        _pipeline.UnregisterHandler(handler);
    }

    /// <summary>
    /// 创建UI切换事件
    /// </summary>
    private UiTransitionEvent CreateEvent(
        string toUiKey,
        UiTransitionType type,
        UiTransitionPolicy? policy = null,
        IUiPageEnterParam? param = null
    )
    {
        return new UiTransitionEvent
        {
            FromUiKey = GetCurrentUiKey(),
            ToUiKey = toUiKey,
            TransitionType = type,
            Policy = policy ?? UiTransitionPolicy.Exclusive,
            EnterParam = param
        };
    }

    /// <summary>
    /// 执行UI切换前的Handler（阻塞）
    /// </summary>
    private void BeforeChange(UiTransitionEvent @event)
    {
        _log.Debug("BeforeChange phases started: {0}", @event.TransitionType);
        _pipeline.ExecuteAsync(@event, UITransitionPhases.BeforeChange).GetAwaiter().GetResult();
        _log.Debug("BeforeChange phases completed: {0}", @event.TransitionType);
    }

    /// <summary>
    /// 执行UI切换后的Handler（不阻塞）
    /// </summary>
    private void AfterChange(UiTransitionEvent @event)
    {
        _log.Debug("AfterChange phases started: {0}", @event.TransitionType);
        _ = Task.Run(async () =>
        {
            try
            {
                await _pipeline.ExecuteAsync(@event, UITransitionPhases.AfterChange).ConfigureAwait(false);
                _log.Debug("AfterChange phases completed: {0}", @event.TransitionType);
            }
            catch (Exception ex)
            {
                _log.Error("AfterChange phases failed: {0}, Error: {1}", @event.TransitionType, ex.Message);
            }
        });
    }

    /// <summary>
    /// 绑定UI根节点
    /// </summary>
    public void BindRoot(IUiRoot root)
    {
        _uiRoot = root;
        _log.Debug("Bind UI Root: {0}", root.GetType().Name);
    }

    /// <summary>
    /// 执行Push的核心逻辑（不触发Pipeline）
    /// </summary>
    private void DoPushInternal(string uiKey, IUiPageEnterParam? param, UiTransitionPolicy policy)
    {
        if (_stack.Count > 0)
        {
            var current = _stack.Peek();
            _log.Debug("Pause current page: {0}", current.GetType().Name);
            current.OnPause();

            if (policy == UiTransitionPolicy.Exclusive)
            {
                _log.Debug("Hide current page (Exclusive): {0}", current.GetType().Name);
                current.OnHide();
            }
        }

        var page = _factory.Create(uiKey);
        _log.Debug("Create UI Page instance: {0}", page.GetType().Name);

        _uiRoot.AddUiPage(page);
        _stack.Push(page);

        _log.Debug(
            "Enter & Show page: {0}, stackAfter={1}",
            page.GetType().Name, _stack.Count
        );

        page.OnEnter(param);
        page.OnShow();
    }

    /// <summary>
    /// 执行Pop的核心逻辑（不触发Pipeline）
    /// </summary>
    private void DoPopInternal(UiPopPolicy policy)
    {
        if (_stack.Count == 0)
            return;

        var top = _stack.Pop();
        _log.Debug(
            "Pop UI Page internal: {0}, policy={1}, stackAfterPop={2}",
            top.GetType().Name, policy, _stack.Count
        );

        top.OnExit();

        if (policy == UiPopPolicy.Destroy)
        {
            _log.Debug("Destroy UI Page: {0}", top.GetType().Name);
            _uiRoot.RemoveUiPage(top);
        }
        else
        {
            _log.Debug("Hide UI Page: {0}", top.GetType().Name);
            top.OnHide();
        }

        if (_stack.Count > 0)
        {
            var next = _stack.Peek();
            _log.Debug("Resume & Show page: {0}", next.GetType().Name);
            next.OnResume();
            next.OnShow();
        }
        else
        {
            _log.Debug("UI stack is now empty");
        }
    }

    /// <summary>
    /// 执行Clear的核心逻辑（不触发Pipeline）
    /// </summary>
    private void DoClearInternal(UiPopPolicy policy)
    {
        _log.Debug("Clear UI Stack internal, count={0}", _stack.Count);
        while (_stack.Count > 0)
            DoPopInternal(policy);
    }

    /// <summary>
    /// 将指定UI页面压入栈顶并显示
    /// </summary>
    /// <param name="uiKey">UI页面标识符</param>
    /// <param name="param">页面进入参数，可为空</param>
    /// <param name="policy">页面切换策略</param>
    public void Push(
        string uiKey,
        IUiPageEnterParam? param = null,
        UiTransitionPolicy policy = UiTransitionPolicy.Exclusive
    )
    {
        var @event = CreateEvent(uiKey, UiTransitionType.Push, policy, param);

        _log.Debug(
            "Push UI Page: key={0}, policy={1}, stackBefore={2}",
            uiKey, policy, _stack.Count
        );

        BeforeChange(@event);

        DoPushInternal(uiKey, param, policy);

        AfterChange(@event);
    }


    /// <summary>
    /// 弹出栈顶页面并根据策略处理页面
    /// </summary>
    /// <param name="policy">弹出策略，默认为销毁策略</param>
    public void Pop(UiPopPolicy policy = UiPopPolicy.Destroy)
    {
        if (_stack.Count == 0)
        {
            _log.Debug("Pop ignored: stack is empty");
            return;
        }

        var nextUiKey = _stack.Count > 1
            ? _stack.ElementAt(1).View.GetType().Name
            : string.Empty;
        var @event = CreateEvent(nextUiKey, UiTransitionType.Pop);

        BeforeChange(@event);

        DoPopInternal(policy);

        AfterChange(@event);
    }

    /// <summary>
    /// 替换当前所有页面为新页面
    /// </summary>
    /// <param name="uiKey">新UI页面标识符</param>
    /// <param name="param">页面进入参数，可为空</param>
    /// <param name="popPolicy">弹出页面时的销毁策略，默认为销毁</param>
    /// <param name="pushPolicy">推入页面时的过渡策略，默认为独占</param>
    public void Replace(
        string uiKey,
        IUiPageEnterParam? param = null,
        UiPopPolicy popPolicy = UiPopPolicy.Destroy,
        UiTransitionPolicy pushPolicy = UiTransitionPolicy.Exclusive
    )
    {
        var @event = CreateEvent(uiKey, UiTransitionType.Replace, pushPolicy, param);

        _log.Debug(
            "Replace UI Stack with page: key={0}, popPolicy={1}, pushPolicy={2}",
            uiKey, popPolicy, pushPolicy
        );

        BeforeChange(@event);

        // 使用内部方法，避免触发额外的Pipeline
        DoClearInternal(popPolicy);
        DoPushInternal(uiKey, param, pushPolicy);

        AfterChange(@event);
    }

    /// <summary>
    /// 清空所有页面栈中的页面
    /// </summary>
    public void Clear()
    {
        var @event = CreateEvent(string.Empty, UiTransitionType.Clear);

        _log.Debug("Clear UI Stack, stackCount={0}", _stack.Count);

        BeforeChange(@event);

        // 使用内部方法，避免触发额外的Pipeline
        DoClearInternal(UiPopPolicy.Destroy);

        AfterChange(@event);
    }
}