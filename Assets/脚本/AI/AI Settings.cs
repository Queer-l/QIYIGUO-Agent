using System;
using UnityEngine;

//
// @param 无
// @return 无
//
// @summary AISettings 保存 AI 接口地址、模型名称和鉴权环境变量名，并通过事件总线接收修改请求和广播当前配置。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class AISettings : MonoBehaviour
{
    [Header("Anthropic Compatible API")]
    [SerializeField] private string baseUrl = "https://api.deepseek.com/anthropic";
    [SerializeField] private string model = "deepseek-v4-pro[1m]";
    [SerializeField] private string defaultHaikuModel = "deepseek-v4-pro[1m]";
    [SerializeField] private string defaultSonnetModel = "deepseek-v4-pro[1m]";
    [SerializeField] private string defaultOpusModel = "deepseek-v4-pro[1m]";

    [Header("AI Role")]
    [SerializeField] private AIRoleSO aiRole;

    [Header("Environment Variable Names")]
    [SerializeField] private string authTokenEnvironmentKey = "ANTHROPIC_AUTH_TOKEN";

    private string runtimeAuthToken;

    //
    // @param 无
    // @return 无
    //
    // @summary 组件启用时订阅 AI 鉴权 Token 修改事件和配置刷新事件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnEnable()
    {
        AIEventBus.AuthTokenChangeRequested += SetAuthToken;
        AIEventBus.ConfigRefreshRequested += PublishConfig;
        PublishConfig();
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 组件禁用时取消订阅 AI 配置相关事件，避免重复订阅。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnDisable()
    {
        AIEventBus.AuthTokenChangeRequested -= SetAuthToken;
        AIEventBus.ConfigRefreshRequested -= PublishConfig;
    }

    //
    // @param 无
    // @return string 当前 AI 接口基础地址。
    //
    // @summary 向外部脚本提供当前 AI 服务的基础请求地址。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public string BaseUrl => baseUrl;

    //
    // @param 无
    // @return string 当前默认使用的 AI 模型名称。
    //
    // @summary 向外部脚本提供当前聊天请求使用的默认模型名称。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public string Model => model;

    //
    // @param 无
    // @return string 当前 Haiku 档位映射的模型名称。
    //
    // @summary 向外部脚本提供 Haiku 档位对应的模型配置。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public string DefaultHaikuModel => defaultHaikuModel;

    //
    // @param 无
    // @return string 当前 Sonnet 档位映射的模型名称。
    //
    // @summary 向外部脚本提供 Sonnet 档位对应的模型配置。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public string DefaultSonnetModel => defaultSonnetModel;

    //
    // @param 无
    // @return string 当前 Opus 档位映射的模型名称。
    //
    // @summary 向外部脚本提供 Opus 档位对应的模型配置。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public string DefaultOpusModel => defaultOpusModel;

    //
    // @param 无
    // @return AIRoleSO 当前 AI 角色配置。
    //
    // @summary 向外部脚本提供当前统一配置的 AI 角色 ScriptableObject。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public AIRoleSO AIRole => aiRole;

    //
    // @param 无
    // @return string 当前 AI 角色生成的 system prompt，未配置角色时返回空字符串。
    //
    // @summary 向请求脚本提供当前 AI 角色人格设定文本。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public string BuildRoleSystemPrompt()
    {
        if (aiRole == null)
        {
            return string.Empty;
        }

        return aiRole.BuildSystemPrompt();
    }

    //
    // @param expression 需要读取的表情枚举。
    // @return Sprite 当前 AI 角色配置中的表情精灵图，未配置时返回 null。
    //
    // @summary 向形象控制脚本提供当前角色的表情差分精灵图。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public Sprite GetExpressionSprite(AIRoleExpression expression)
    {
        if (aiRole == null)
        {
            return null;
        }

        return aiRole.GetExpressionSprite(expression);
    }

    //
    // @param newBaseUrl 新的 AI 接口基础地址。
    // @return 无
    //
    // @summary 校验并保存新的 AI 接口基础地址，空字符串不会覆盖已有配置。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SetBaseUrl(string newBaseUrl)
    {
        if (!string.IsNullOrWhiteSpace(newBaseUrl))
        {
            baseUrl = newBaseUrl.Trim();
            PublishConfig();
        }
    }

    //
    // @param newAuthToken 新的 AI 鉴权 Token。
    // @return 无
    //
    // @summary 校验并保存运行时 AI 鉴权 Token，空字符串不会覆盖已有配置。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SetAuthToken(string newAuthToken)
    {
        if (!string.IsNullOrWhiteSpace(newAuthToken))
        {
            runtimeAuthToken = newAuthToken.Trim();
            PublishConfig();
        }
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 通过事件总线广播当前 AI 模型名称和接口地址。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void PublishConfig()
    {
        AIEventBus.PublishConfigChanged(model, baseUrl);
    }

    //
    // @param 无
    // @return string 当前进程环境变量中的鉴权 Token，未设置时返回 null。
    //
    // @summary 从配置的环境变量名读取 AI 服务鉴权 Token，避免把密钥写入 Unity 项目文件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public string AuthToken
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(runtimeAuthToken))
            {
                return runtimeAuthToken;
            }

            return Environment.GetEnvironmentVariable(authTokenEnvironmentKey);
        }
    }

    //
    // @param 无
    // @return bool 已配置鉴权 Token 时返回 true，否则返回 false。
    //
    // @summary 判断当前运行环境中是否存在可用的 AI 服务鉴权 Token。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public bool HasAuthToken => !string.IsNullOrWhiteSpace(AuthToken);
}
