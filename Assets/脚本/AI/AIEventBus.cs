using System;

//
// @param 无
// @return 无
//
// @summary AIEventBus 提供 AI 配置相关的全局事件入口，用于解耦 UI 控制脚本和 AI 配置脚本。
// @checked: false. not reviewed by human, 07/06/2026.
//
public static class AIEventBus
{
    //
    // @param string 请求更新的 AI 鉴权 Token。
    // @return 无
    //
    // @summary 当外部界面或系统请求修改 AI 鉴权 Token 时触发。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public static event Action<string> AuthTokenChangeRequested;

    //
    // @param 无
    // @return 无
    //
    // @summary 当外部界面或系统请求重新获取当前 AI 配置时触发。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public static event Action ConfigRefreshRequested;

    //
    // @param string 当前 AI 模型名称。
    // @param string 当前 AI 接口基础地址。
    // @return 无
    //
    // @summary 当 AI 配置被初始化或修改后触发，用于通知 UI 刷新显示内容。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public static event Action<string, string> ConfigChanged;

    //
    // @param newAuthToken 请求写入的新 AI 鉴权 Token。
    // @return 无
    //
    // @summary 发布 AI 鉴权 Token 修改请求，已订阅的 AISettings 会接收并处理该请求。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public static void RequestAuthTokenChange(string newAuthToken)
    {
        AuthTokenChangeRequested?.Invoke(newAuthToken);
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 发布配置刷新请求，已订阅的 AISettings 会广播当前配置。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public static void RequestConfigRefresh()
    {
        ConfigRefreshRequested?.Invoke();
    }

    //
    // @param model 当前 AI 模型名称。
    // @param baseUrl 当前 AI 接口基础地址。
    // @return 无
    //
    // @summary 发布当前 AI 配置，供配置界面或其他系统刷新显示。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public static void PublishConfigChanged(string model, string baseUrl)
    {
        ConfigChanged?.Invoke(model, baseUrl);
    }
}
