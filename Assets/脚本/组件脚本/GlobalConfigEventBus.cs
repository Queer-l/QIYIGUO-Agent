using System;

//
// @param 无
// @return 无
//
// @summary GlobalConfigEventBus 提供全局配置事件，用于解耦 UI 按钮和实际配置执行脚本。
// @checked: false. not reviewed by human, 07/06/2026.
//
public static class GlobalConfigEventBus
{
    //
    // @param int 请求设置的窗口分辨率宽度。
    // @param int 请求设置的窗口分辨率高度。
    // @return 无
    //
    // @summary 当外部 UI 请求修改窗口分辨率时触发。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public static event Action<int, int> WindowResolutionChangeRequested;

    //
    // @param width 请求设置的窗口分辨率宽度。
    // @param height 请求设置的窗口分辨率高度。
    // @return 无
    //
    // @summary 发布窗口分辨率修改请求，由 UIResolutionController 监听并应用。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public static void RequestWindowResolutionChange(int width, int height)
    {
        WindowResolutionChangeRequested?.Invoke(width, height);
    }
}
