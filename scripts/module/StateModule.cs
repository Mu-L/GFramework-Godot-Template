using GFramework.Core.Abstractions.architecture;
using GFramework.Game.architecture;
using GFrameworkGodotTemplate.scripts.core.state;
using GFrameworkGodotTemplate.scripts.core.state.impls;

namespace GFrameworkGodotTemplate.scripts.module;

/// <summary>
/// 状态模块类，负责安装和注册游戏状态系统
/// </summary>
public class StateModule : AbstractModule
{
    /// <summary>
    /// 安装模块方法，向架构中注册游戏状态机和相关状态
    /// </summary>
    /// <param name="architecture">游戏架构实例，用于注册状态系统</param>
    public override void Install(IArchitecture architecture)
    {
        var gameStateMachine = new GameStateMachine();
        
        gameStateMachine.RegisterState(new MainMenuState());
        gameStateMachine.RegisterState(new PlayingState());
        gameStateMachine.RegisterState(new PausedState());
        gameStateMachine.RegisterState(new GameOverState());
        
        architecture.RegisterSystem(gameStateMachine);
        
    }
}
