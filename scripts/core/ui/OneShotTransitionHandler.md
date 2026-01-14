# 一次性 UI 切换处理器使用指南

## 概述

`OneShotTransitionHandler` 是一种特殊类型的 UI 切换处理器，它只执行一次，执行完成后自动从 Pipeline 中注销。适用于"某个特定 UI 切换时执行一次性逻辑"的场景。

## 核心特性

✅ **一次性执行**：执行完自动注销，不会再次执行  
✅ **Lambda 支持**：支持 Lambda 表达式快速定义逻辑  
✅ **Pipeline 集成**：完全集成到 Pipeline，支持优先级、超时、错误处理  
✅ **条件过滤**：支持 `ShouldHandle` 进行细粒度控制  
✅ **自动生命周期**：无需手动管理，执行完自动清理  

## 使用方式

### 方式1：简洁 API（推荐）

#### 基础用法
```csharp
// 注册一次性 Handler，只在首次匹配时执行
uiRouter.RegisterOneShot(
    handleAsync: async (@event, ct) => {
        Console.WriteLine("一次性逻辑执行");
        await Task.Delay(100, ct);
    }
);
```

#### 带条件过滤
```csharp
// 只在切换到 MainMenu 时执行一次
uiRouter.RegisterOneShot(
    shouldHandle: (@event, phase) => @event.ToUiKey == "MainMenu",
    handleAsync: async (@event, ct) => {
        Console.WriteLine("首次进入 MainMenu");
        await Task.CompletedTask;
    }
);
```

#### 设置优先级和阶段
```csharp
uiRouter.RegisterOneShot(
    priority: 50,
    phases: UITransitionPhases.BeforeChange,
    shouldHandle: (@event, phase) => @event.ToUiKey == "Settings",
    handleAsync: async (@event, ct) => {
        Console.WriteLine("在切换前执行一次性逻辑");
        await Task.CompletedTask;
    }
);
```

#### 执行完成回调
```csharp
uiRouter.RegisterOneShot(
    handleAsync: async (@event, ct) => {
        Console.WriteLine("执行一次性逻辑");
        await Task.CompletedTask;
    },
    onExecuted: () => {
        Console.WriteLine("Handler 已自动注销");
    }
);
```

### 方式2：流式 API（适合复杂配置）

```csharp
uiRouter.CreateOneShot()
    .WithPriority(50)
    .WithPhases(UITransitionPhases.BeforeChange)
    .When((@event, phase) => @event.ToUiKey == "MainMenu")
    .Handle(@event => {
        Console.WriteLine("一次性逻辑执行");
    })
    .OnExecuted(() => {
        Console.WriteLine("Handler 已自动注销");
    })
    .Register();
```

### 方式3：同步处理逻辑

```csharp
uiRouter.RegisterOneShot(
    shouldHandle: (@event, phase) => @event.ToUiKey == "MainMenu",
    handle: @event => {
        Console.WriteLine("同步执行一次性逻辑");
    }
);
```

## 使用场景

### 场景1：首次进入某个 UI 时显示教程
```csharp
// 只在首次进入 Settings UI 时显示教程
uiRouter.RegisterOneShot(
    shouldHandle: (@event, phase) => @event.ToUiKey == "Settings",
    handleAsync: async (@event, ct) => {
        await ShowTutorialAsync("Settings", ct);
    }
);
```

### 场景2：UI 切换时播放一次性音效
```csharp
// 只在从 MainMenu 到 Game 时播放特殊音效
uiRouter.RegisterOneShot(
    priority: 200,
    shouldHandle: (@event, phase) =>
        @event.FromUiKey == "MainMenu" && @event.ToUiKey == "Game",
    handle: @event => {
        PlaySpecialSound("game_start");
    }
);
```

### 场景3：加载特定 UI 时预加载数据
```csharp
// 首次进入 Store UI 时预加载商品数据
uiRouter.RegisterOneShot(
    priority: 50,
    phases: UITransitionPhases.BeforeChange,
    shouldHandle: (@event, phase) => @event.ToUiKey == "Store",
    handleAsync: async (@event, ct) => {
        await PreloadStoreItemsAsync(ct);
    }
);
```

### 场景4：UI 切换完成后执行一次性逻辑
```csharp
// 只在首次加载 MainMenu 后显示欢迎消息
uiRouter.RegisterOneShot(
    priority: 999,
    phases: UITransitionPhases.AfterChange,
    shouldHandle: (@event, phase) => @event.ToUiKey == "MainMenu",
    handle: @event => {
        ShowWelcomeMessage();
    }
);
```

### 场景5：条件性的一次性操作
```csharp
// 只在特定条件下执行一次性操作
bool shouldShowPromo = CheckPromoCondition();

if (shouldShowPromo)
{
    uiRouter.RegisterOneShot(
        shouldHandle: (@event, phase) => @event.ToUiKey == "MainMenu",
        handle: @event => {
            ShowPromotionPopup();
        }
    );
}
```

## API 参考

### RegisterOneShot 扩展方法

#### 异步版本
```csharp
public static OneShotTransitionHandler RegisterOneShot(
    this IUiRouter router,
    int priority = 100,
    UITransitionPhases phases = UITransitionPhases.BeforeChange,
    Func<UiTransitionEvent, UITransitionPhases, bool>? shouldHandle = null,
    Func<UiTransitionEvent, CancellationToken, Task>? handle = null,
    Action? onExecuted = null
)
```

**参数**：
- `router`: UI 路由器实例
- `priority`: 优先级，默认 100
- `phases`: 适用的阶段，默认 BeforeChange
- `shouldHandle`: 判断是否应该处理当前事件（可选），默认返回 true
- `handle`: 处理逻辑（必填）
- `onExecuted`: 执行完成后的回调（可选）

**返回值**：创建的 `OneShotTransitionHandler` 实例，可用于后续手动注销

#### 同步版本
```csharp
public static OneShotTransitionHandler RegisterOneShot(
    this IUiRouter router,
    int priority,
    UITransitionPhases phases,
    Func<UiTransitionEvent, UITransitionPhases, bool>? shouldHandle,
    Action<UiTransitionEvent> handle,
    Action? onExecuted = null
)
```

**参数**：
- `handle`: 同步处理逻辑（必填）

**返回值**：创建的 `OneShotTransitionHandler` 实例

### CreateOneShot 扩展方法

```csharp
public static OneShotHandlerBuilder CreateOneShot(this IUiRouter router)
```

**返回值**：`OneShotHandlerBuilder` 构建器实例

#### OneShotHandlerBuilder 方法

- `WithPriority(int priority)`: 设置优先级
- `WithPhases(UITransitionPhases phases)`: 设置适用的阶段
- `When(Func<UiTransitionEvent, UITransitionPhases, bool> shouldHandle)`: 设置判断逻辑
- `HandleAsync(Func<UiTransitionEvent, CancellationToken, Task> handle)`: 设置异步处理逻辑
- `Handle(Action<UiTransitionEvent> handle)`: 设置同步处理逻辑
- `OnExecuted(Action onExecuted)`: 设置执行完成回调
- `Register()`: 构建并注册一次性 Handler

## 高级用法

### 手动注销

虽然一次性 Handler 会在执行后自动注销，但你也可以提前手动注销：

```csharp
var handler = uiRouter.RegisterOneShot(
    shouldHandle: (@event, phase) => @event.ToUiKey == "MainMenu",
    handle: @event => {
        Console.WriteLine("可能不会执行的逻辑");
    }
);

// 提前注销
if (someCondition)
{
    uiRouter.UnregisterHandler(handler);
}
```

### 检查是否已执行

```csharp
var handler = uiRouter.RegisterOneShot(
    handle: @event => {
        Console.WriteLine("执行一次性逻辑");
    }
);

// 检查是否已执行
if (handler.IsExecuted)
{
    Console.WriteLine("Handler 已执行过");
}
```

### 多个一次性 Handler

可以注册多个一次性 Handler，它们会独立执行：

```csharp
// 每个 UI 都有自己的一次性 Handler
foreach (var uiKey in new[] { "Settings", "Store", "Inventory" })
{
    uiRouter.RegisterOneShot(
        shouldHandle: (@event, phase) => @event.ToUiKey == uiKey,
        handle: @event => {
            Console.WriteLine($"首次进入 {uiKey}");
        }
    );
}
```

### 与持久化 Handler 混用

一次性 Handler 可以和持久化 Handler 一起使用：

```csharp
// 持久化 Handler：每次都执行
uiRouter.RegisterHandler(new LoggingTransitionHandler());

// 一次性 Handler：只执行一次
uiRouter.RegisterOneShot(
    shouldHandle: (@event, phase) => @event.ToUiKey == "MainMenu",
    handle: @event => {
        Console.WriteLine("只执行一次");
    }
);
```

## 注意事项

### 1. 执行时机
- 一次性 Handler 在 Pipeline 中按照优先级执行
- 如果 `ShouldHandle` 返回 false，不会执行，也不会标记为已执行
- 只有真正执行过 `HandleAsync` 后才会标记为已执行

### 2. 并发安全
- 一次性 Handler 的执行状态是线程安全的
- 即使多个 UI 切换同时发生，也只会执行一次

### 3. 注销时机
- 执行完 `HandleAsync` 后自动注销
- `ShouldHandle` 返回 false 不会导致注销
- 可以手动注销，无论是否已执行

### 4. 与 Handler 列表的关系
- 一次性 Handler 注册后会加入 Handler 列表
- 执行完成后会自动从列表中移除
- 不需要手动调用 `UnregisterHandler`

## 示例代码

### 完整示例：新手引导
```csharp
public class TutorialController
{
    private readonly IUiRouter _router;

    public TutorialController(IUiRouter router)
    {
        _router = router;
    }

    public void SetupOneShotTutorials()
    {
        // 首次进入 MainMenu：显示欢迎教程
        _router.RegisterOneShot(
            priority: 999,
            phases: UITransitionPhases.AfterChange,
            shouldHandle: (@event, phase) => @event.ToUiKey == "MainMenu",
            handle: @event => {
                ShowWelcomeTutorial();
            }
        );

        // 首次进入 Settings：显示设置教程
        _router.RegisterOneShot(
            shouldHandle: (@event, phase) => @event.ToUiKey == "Settings",
            handle: @event => {
                ShowSettingsTutorial();
            }
        );

        // 首次进入 Store：加载商品数据并显示教程
        _router.CreateOneShot()
            .WithPriority(50)
            .WithPhases(UITransitionPhases.BeforeChange)
            .When((@event, phase) => @event.ToUiKey == "Store")
            .HandleAsync(async (@event, ct) => {
                await PreloadStoreDataAsync(ct);
            })
            .OnExecuted(() => {
                ShowStoreTutorial();
            })
            .Register();
    }

    private void ShowWelcomeTutorial()
    {
        Console.WriteLine("欢迎来到游戏！");
    }

    private void ShowSettingsTutorial()
    {
        Console.WriteLine("这里是设置界面");
    }

    private void ShowStoreTutorial()
    {
        Console.WriteLine("这里是商店");
    }

    private async Task PreloadStoreDataAsync(CancellationToken ct)
    {
        Console.WriteLine("预加载商店数据...");
        await Task.Delay(100, ct);
    }
}
```

## 最佳实践

### 1. 明确使用场景
- ✅ 首次进入某个 UI 的教程
- ✅ 一次性数据预加载
- ✅ 特定条件下的临时行为
- ❌ 需要重复执行的逻辑（使用持久化 Handler）
- ❌ 复杂的状态管理（考虑使用状态机）

### 2. 合理设置优先级
- 优先级 0-49：需要在 UI 切换前完成的基础操作
- 优先级 50-99：数据预加载等准备操作
- 优先级 100-199：动画、音效等视觉/听觉反馈
- 优先级 200-999：日志、统计等非关键操作

### 3. 充分利用条件过滤
- 使用 `ShouldHandle` 精确控制执行时机
- 避免不必要的一次性 Handler 执行
- 提高代码可读性和维护性

### 4. 选择合适的 API
- 简单场景：使用简洁 API（`RegisterOneShot`）
- 复杂配置：使用流式 API（`CreateOneShot`）
- 同步操作：使用同步版本

## 总结

一次性 UI 切换处理器为"特定 UI 切换时执行一次性逻辑"提供了优雅的解决方案。通过 Lambda 表达式和流式 API，可以快速定义和注册一次性逻辑，同时享受完整的 Pipeline 功能。

关键优势：
- ✅ 语法简洁，类似闭包
- ✅ 自动管理生命周期
- ✅ 完全集成 Pipeline
- ✅ 支持条件过滤和优先级
- ✅ 适合各种一次性场景
