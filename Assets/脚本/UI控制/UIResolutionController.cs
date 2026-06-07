using UnityEngine;
//
// @param 无
// @return 无
//
// @summary UIResolutionController 监听全局分辨率修改事件，并把事件中的宽高应用到游戏窗口分辨率。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class UIResolutionController : MonoBehaviour
{
    [SerializeField] private bool fullScreen;
    [SerializeField] private bool enableDebugLog = true;

    //
    // @param 无
    // @return 无
    //
    // @summary 组件启用时监听窗口分辨率修改事件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnEnable()
    {
        GlobalConfigEventBus.WindowResolutionChangeRequested += ApplyWindowResolution;
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 组件禁用时取消监听窗口分辨率修改事件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnDisable()
    {
        GlobalConfigEventBus.WindowResolutionChangeRequested -= ApplyWindowResolution;
    }

    //
    // @param width 窗口分辨率宽度。
    // @param height 窗口分辨率高度。
    // @return 无
    //
    // @summary 接收全局分辨率修改事件，并调用 Screen.SetResolution 修改游戏窗口大小。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void ApplyWindowResolution(int width, int height)
    {
        int safeWidth = Mathf.Max(1, width);
        int safeHeight = Mathf.Max(1, height);
        Screen.SetResolution(safeWidth, safeHeight, fullScreen);
        LogDebug("窗口分辨率已设置：" + safeWidth + "x" + safeHeight + "，全屏：" + fullScreen);
    }

    //
    // @param message 调试日志内容。
    // @return 无
    //
    // @summary 在开启调试日志时输出 UI 分辨率应用状态。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log("[UIResolutionController] " + message);
        }
    }
}
