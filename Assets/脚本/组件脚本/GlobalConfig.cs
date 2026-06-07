using UnityEngine;

//
// @param 无
// @return 无
//
// @summary GlobalConfig 负责发布全局配置修改请求，当前用于向 UI 控制器发送窗口分辨率修改事件。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class GlobalConfig : MonoBehaviour
{
    [SerializeField] private ResolutionPreset[] resolutionPresets =
    {
        new ResolutionPreset("HD", 1280, 720),
        new ResolutionPreset("Full HD", 1920, 1080),
        new ResolutionPreset("2K", 2560, 1440)
    };
    [SerializeField] private int defaultPresetIndex = 1;
    [SerializeField] private int defaultWidth = 1920;
    [SerializeField] private int defaultHeight = 1080;
    [SerializeField] private int buttonWidth = 1920;
    [SerializeField] private int buttonHeight = 1080;
    [SerializeField] private bool applyDefaultOnStart = true;
    [SerializeField] private bool enableDebugLog = true;

    //
    // @param 无
    // @return 无
    //
    // @summary 程序启动时按需发布默认窗口分辨率修改事件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void Start()
    {
        if (applyDefaultOnStart)
        {
            if (IsValidPresetIndex(defaultPresetIndex))
            {
                SetCanvasResolutionByPresetIndex(defaultPresetIndex);
            }
            else
            {
                SetCanvasResolution(defaultWidth, defaultHeight);
            }
        }
    }

    //
    // @param width 窗口分辨率宽度。
    // @param height 窗口分辨率高度。
    // @return 无
    //
    // @summary 发布窗口分辨率修改事件，供 UIResolutionController 监听并应用。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SetCanvasResolution(int width, int height)
    {
        int safeWidth = Mathf.Max(1, width);
        int safeHeight = Mathf.Max(1, height);
        GlobalConfigEventBus.RequestWindowResolutionChange(safeWidth, safeHeight);
        LogDebug("已发布窗口分辨率修改事件：" + safeWidth + "x" + safeHeight);
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 使用 Inspector 中配置的 buttonWidth 和 buttonHeight 发布分辨率修改事件，方便按钮直接绑定。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SetCanvasResolutionByButtonValue()
    {
        SetCanvasResolution(buttonWidth, buttonHeight);
    }

    //
    // @param presetIndex 分辨率预设数组索引。
    // @return 无
    //
    // @summary 根据 Inspector 中配置的分辨率预设索引发布 Canvas 分辨率修改事件，适合按钮传 int 调用。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SetCanvasResolutionByPresetIndex(int presetIndex)
    {
        if (!IsValidPresetIndex(presetIndex))
        {
            LogDebug("分辨率预设索引无效：" + presetIndex);
            return;
        }

        ResolutionPreset preset = resolutionPresets[presetIndex];
        SetCanvasResolution(preset.width, preset.height);
        LogDebug("已使用分辨率预设：" + preset.name);
    }

    //
    // @param width 按钮预设分辨率宽度。
    // @return 无
    //
    // @summary 设置按钮预设宽度，供外部 UI 输入控件修改。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SetButtonWidth(int width)
    {
        buttonWidth = Mathf.Max(1, width);
    }

    //
    // @param height 按钮预设分辨率高度。
    // @return 无
    //
    // @summary 设置按钮预设高度，供外部 UI 输入控件修改。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SetButtonHeight(int height)
    {
        buttonHeight = Mathf.Max(1, height);
    }

    //
    // @param message 调试日志内容。
    // @return 无
    //
    // @summary 在开启调试日志时输出全局配置事件发布状态。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log("[GlobalConfig] " + message);
        }
    }

    //
    // @param presetIndex 分辨率预设数组索引。
    // @return bool 索引可用时返回 true，否则返回 false。
    //
    // @summary 判断给定索引是否能从分辨率预设数组中读取有效配置。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private bool IsValidPresetIndex(int presetIndex)
    {
        return resolutionPresets != null && presetIndex >= 0 && presetIndex < resolutionPresets.Length;
    }
}

//
// @param 无
// @return 无
//
// @summary ResolutionPreset 保存一个 Canvas 参考分辨率预设。
// @checked: false. not reviewed by human, 07/06/2026.
//
[System.Serializable]
public class ResolutionPreset
{
    public string name;
    public int width;
    public int height;

    //
    // @param name 分辨率预设名称。
    // @param width 分辨率宽度。
    // @param height 分辨率高度。
    // @return 无
    //
    // @summary 初始化一个 Canvas 参考分辨率预设。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public ResolutionPreset(string name, int width, int height)
    {
        this.name = name;
        this.width = width;
        this.height = height;
    }
}
