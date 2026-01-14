using System.Collections.Generic;
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
    private readonly Stack<IUiPage> _stack = new();
    /// <summary>
    /// 初始化方法，在页面初始化时获取UI工厂实例
    /// </summary>
    protected override void OnInit()
    {
        _factory = this.GetUtility<IUiFactory>()!;
        _log.Debug("UiRouter initialized. Factory={0}", _factory.GetType().Name);
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
        _log.Debug(
            "Push UI Page: key={0}, policy={1}, stackBefore={2}",
            uiKey, policy, _stack.Count
        );

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

        var top = _stack.Pop();
        _log.Debug(
            "Pop UI Page: {0}, policy={1}, stackAfterPop={2}",
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
        _log.Debug(
            "Replace UI Stack with page: key={0}, popPolicy={1}, pushPolicy={2}",
            uiKey, popPolicy, pushPolicy
        );

        while (_stack.Count > 0)
            Pop(popPolicy);

        // 推入新的页面到栈中
        Push(uiKey, param, pushPolicy);
    }


    /// <summary>
    /// 清空所有页面栈中的页面
    /// </summary>
    public void Clear()
    {
        _log.Debug("Clear UI Stack, stackCount={0}", _stack.Count);

        while (_stack.Count > 0)
            Pop();
    }
}