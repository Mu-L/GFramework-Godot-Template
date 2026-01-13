using System;
using GFrameworkGodotTemplate.scripts.model;

namespace GFrameworkGodotTemplate.scripts.core.state;

/// <summary>
/// 游戏状态机实现类，用于管理游戏流程状态
/// </summary>
public class GameStateMachine : AbstractStateMachine
{
    /// <summary>
    /// 切换到指定游戏状态
    /// </summary>
    /// <param name="gameState">要切换到的游戏状态枚举</param>
    public void ChangeState(GameState gameState)
    {
        var stateKey = gameState.ToString();
        if (States.TryGetValue(stateKey, out var state))
        {
            ChangeState(state);
        }
    }
    
    /// <summary>
    /// 获取当前游戏状态枚举
    /// </summary>
    /// <returns>当前游戏状态枚举，如果当前状态为null则返回null</returns>
    public GameState? GetCurrentGameState()
    {
        if (CurrentState == null)
            return null;
        
        var stateKey = GetStateKey(CurrentState);
        return Enum.TryParse<GameState>(stateKey, out var gameState) ? gameState : null;
    }
}
