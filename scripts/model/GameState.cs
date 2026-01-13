namespace GFrameworkGodotTemplate.scripts.model;

/// <summary>
/// 游戏状态枚举，定义游戏中所有可能的流程状态
/// </summary>
public enum GameState
{
    /// <summary>
    /// 主菜单状态，显示游戏主界面
    /// </summary>
    MainMenu,
    
    /// <summary>
    /// 游戏进行中状态，玩家可操作游戏
    /// </summary>
    Playing,
    
    /// <summary>
    /// 暂停状态，游戏逻辑暂停但UI仍显示
    /// </summary>
    Paused,
    
    /// <summary>
    /// 游戏结束状态，显示结算界面
    /// </summary>
    GameOver
}
