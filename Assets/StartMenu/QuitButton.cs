using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitButton : MonoBehaviour
{
    public void QuitGame()
    {
        #if UNITY_EDITOR
            // 在 Unity 编辑器中，停止播放模式
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // 在构建的游戏版本中，真正退出程序
            Application.Quit();
        #endif
    }
}
