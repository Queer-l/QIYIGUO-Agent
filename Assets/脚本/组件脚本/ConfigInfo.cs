using TMPro;
using UnityEngine;
using UnityEngine.UI;

//
// @param 无
// @return 无
//
// @summary ConfigInfo 绑定配置界面的文本、输入框和按钮，通过事件总线显示并请求修改当前 AI 接口配置。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class ConfigInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text modelText;
    [SerializeField] private TMP_Text urlText;
    [SerializeField] private TMP_InputField apiInputField;
    [SerializeField] private Button applyButton;

    //
    // @param 无
    // @return 无
    //
    // @summary 在组件初始化时绑定应用按钮的点击事件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void Awake()
    {
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(ApplyApiUrl);
        }
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 组件启用时监听 AI 配置变化事件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnEnable()
    {
        AIEventBus.ConfigChanged += UpdateConfigInfo;
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 在场景开始运行时刷新配置界面显示。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void Start()
    {
        RefreshInfo();
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 组件禁用时取消监听 AI 配置变化事件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnDisable()
    {
        AIEventBus.ConfigChanged -= UpdateConfigInfo;
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 在组件销毁时解绑按钮点击事件，避免重复监听或空引用调用。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnDestroy()
    {
        if (applyButton != null)
        {
            applyButton.onClick.RemoveListener(ApplyApiUrl);
        }
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 通过事件总线请求刷新当前 AI 配置。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void RefreshInfo()
    {
        AIEventBus.RequestConfigRefresh();
    }

    //
    // @param model 当前 AI 模型名称。
    // @param baseUrl 当前 AI 接口基础地址。
    // @return 无
    //
    // @summary 接收事件总线广播的 AI 配置并刷新文本与输入框显示。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void UpdateConfigInfo(string model, string baseUrl)
    {
        if (modelText != null)
        {
            modelText.text = model;
        }

        if (urlText != null)
        {
            urlText.text = baseUrl;
        }

        if (apiInputField != null)
        {
            apiInputField.text = string.Empty;
        }
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 读取输入框中的 API Key 并通过事件总线发布鉴权 Token 修改请求。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void ApplyApiUrl()
    {
        if (apiInputField == null)
        {
            return;
        }

        AIEventBus.RequestAuthTokenChange(apiInputField.text);
    }
}
