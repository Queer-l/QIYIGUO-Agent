using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

//
// @param 无
// @return 无
//
// @summary AIChatClient 监听 MessageController 的用户消息事件，请求 Anthropic 兼容 AI 接口并把回复显示回聊天界面。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class AIChatClient : MonoBehaviour
{
    [SerializeField] private AISettings aiSettings;
    [SerializeField] private AIContextBuilder contextBuilder;
    [SerializeField] private AgentImagerController agentImagerController;
    [SerializeField] private MessageController messageController;
    [SerializeField] private int maxTokens = 1024;
    [SerializeField] private string anthropicVersion = "2023-06-01";
    [SerializeField] private bool enableDebugLog = true;

    //
    // @param 无
    // @return 无
    //
    // @summary 自动查找缺失引用并订阅用户消息提交事件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnEnable()
    {
        if (aiSettings == null)
        {
            aiSettings = FindObjectOfType<AISettings>();
        }

        if (messageController == null)
        {
            messageController = FindObjectOfType<MessageController>();
        }

        if (contextBuilder == null)
        {
            contextBuilder = FindObjectOfType<AIContextBuilder>();
        }

        if (agentImagerController == null)
        {
            agentImagerController = FindObjectOfType<AgentImagerController>();
        }

        if (messageController != null)
        {
            messageController.UserMessageSubmitted += RequestAIReply;
            LogDebug("已订阅用户消息事件。");
        }
        else
        {
            LogDebug("订阅失败：未找到 MessageController。");
        }
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 组件禁用时取消订阅用户消息提交事件，避免重复请求。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnDisable()
    {
        if (messageController != null)
        {
            messageController.UserMessageSubmitted -= RequestAIReply;
            LogDebug("已取消订阅用户消息事件。");
        }
    }

    //
    // @param userMessage 用户提交给 AI 的消息文本。
    // @return 无
    //
    // @summary 启动协程，把用户消息发送到 AI 服务。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void RequestAIReply(string userMessage)
    {
        LogDebug("收到用户消息，开始 AI 请求。消息长度：" + (userMessage ?? string.Empty).Length);
        StartCoroutine(RequestAIReplyRoutine(userMessage));
    }

    //
    // @param userMessage 用户提交给 AI 的消息文本。
    // @return IEnumerator Unity 协程迭代器。
    //
    // @summary 构建 Anthropic Messages 请求，发送到 AI 接口并处理响应文本。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private IEnumerator RequestAIReplyRoutine(string userMessage)
    {
        if (aiSettings == null)
        {
            LogDebug("请求终止：未绑定 AISettings。");
            ShowAIError("未绑定 AISettings。");
            yield break;
        }

        if (!aiSettings.HasAuthToken)
        {
            LogDebug("请求终止：未配置 API Key。");
            ShowAIError("未配置 API Key。");
            yield break;
        }

        if (messageController != null)
        {
            messageController.ShowAIThinking();
        }

        if (agentImagerController != null)
        {
            agentImagerController.ShowThinking();
        }

        string requestUrl = BuildMessagesUrl(aiSettings.BaseUrl);
        string requestJson = BuildRequestJson(userMessage);
        byte[] requestBody = Encoding.UTF8.GetBytes(requestJson);

        LogDebug("请求地址：" + requestUrl);
        LogDebug("请求模型：" + aiSettings.Model);

        using (UnityWebRequest request = new UnityWebRequest(requestUrl, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(requestBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("x-api-key", aiSettings.AuthToken);
            request.SetRequestHeader("Authorization", "Bearer " + aiSettings.AuthToken);
            request.SetRequestHeader("anthropic-version", anthropicVersion);

            yield return request.SendWebRequest();

            LogDebug("请求完成，结果：" + request.result + "，状态码：" + request.responseCode);

            if (request.result != UnityWebRequest.Result.Success)
            {
                LogDebug("请求失败：" + request.error);
                ShowAIError("AI 请求失败：" + request.error);
                yield break;
            }

            AIReplyResult replyResult = ParseReplyResult(request.downloadHandler.text);
            if (string.IsNullOrWhiteSpace(replyResult.reply))
            {
                LogDebug("解析失败或回复为空，原始响应：" + request.downloadHandler.text);
                ShowAIError("AI 返回内容为空。");
                yield break;
            }

            if (messageController != null)
            {
                LogDebug("AI 回复解析成功，长度：" + replyResult.reply.Length + "，表情：" + replyResult.expression);
                messageController.ShowAIMessage(replyResult.reply);
            }

            if (agentImagerController != null)
            {
                agentImagerController.SetExpression(replyResult.expression);
            }
            else
            {
                LogDebug("AI 回复无法显示：未绑定 MessageController。");
            }
        }
    }

    //
    // @param baseUrl AI 接口基础地址。
    // @return string Anthropic Messages 完整请求地址。
    //
    // @summary 根据 AISettings 中的基础地址拼接 messages 接口路径。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private string BuildMessagesUrl(string baseUrl)
    {
        string normalizedBaseUrl = (baseUrl ?? string.Empty).TrimEnd('/');
        if (normalizedBaseUrl.EndsWith("/v1/messages", StringComparison.OrdinalIgnoreCase))
        {
            return normalizedBaseUrl;
        }

        return normalizedBaseUrl + "/v1/messages";
    }

    //
    // @param userMessage 用户提交给 AI 的消息文本。
    // @return string 序列化后的 JSON 请求体。
    //
    // @summary 构建 Anthropic Messages API 所需的请求 JSON。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private string BuildRequestJson(string userMessage)
    {
        AnthropicMessageRequest request = new AnthropicMessageRequest
        {
            model = aiSettings.Model,
            system = BuildSystemPrompt(),
            max_tokens = maxTokens,
            messages = contextBuilder != null ? contextBuilder.BuildMessages(userMessage) : BuildSingleUserMessage(userMessage)
        };

        return JsonUtility.ToJson(request);
    }

    //
    // @param userMessage 用户提交给 AI 的消息文本。
    // @return List<AnthropicRequestMessage> 只包含当前用户消息的请求列表。
    //
    // @summary 当未绑定 AIContextBuilder 时，构建只包含当前输入的兜底请求消息。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private List<AnthropicRequestMessage> BuildSingleUserMessage(string userMessage)
    {
        return new List<AnthropicRequestMessage>
        {
            new AnthropicRequestMessage
            {
                role = "user",
                content = userMessage
            }
        };
    }

    //
    // @param 无
    // @return string 包含角色设定和返回格式要求的 system prompt。
    //
    // @summary 构建发送给 AI 的系统提示词，要求模型返回 reply 和 expression 字段。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private string BuildSystemPrompt()
    {
        string rolePrompt = aiSettings != null ? aiSettings.BuildRoleSystemPrompt() : string.Empty;
        string expressionPrompt =
            "返回格式要求：你必须只返回一个 JSON 对象，不要添加 Markdown，不要添加额外解释。" +
            "JSON 必须包含 reply 和 expression 两个字段。" +
            "reply 是回复给用户的自然语言文本。" +
            "expression 必须是以下枚举值之一：Idle, Thinking, Confused, Apology。" +
            "普通回答使用 Idle；需要推理或正在分析时使用 Thinking；不确定或需要澄清时使用 Confused；道歉、无法完成或出错时使用 Apology。";

        if (string.IsNullOrWhiteSpace(rolePrompt))
        {
            return expressionPrompt;
        }

        return rolePrompt + "\n\n" + expressionPrompt;
    }

    //
    // @param responseJson AI 服务返回的 JSON 字符串。
    // @return string 解析出的 AI 文本回复。
    //
    // @summary 从 Anthropic Messages 响应中读取第一段 text 类型内容。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private string ParseReplyText(string responseJson)
    {
        AnthropicMessageResponse response = JsonUtility.FromJson<AnthropicMessageResponse>(responseJson);
        if (response == null || response.content == null)
        {
            return string.Empty;
        }

        for (int i = 0; i < response.content.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(response.content[i].text))
            {
                return response.content[i].text;
            }
        }

        return string.Empty;
    }

    //
    // @param responseJson AI 服务返回的 JSON 字符串。
    // @return AIReplyResult 解析后的回复文本和表情枚举。
    //
    // @summary 从 AI 响应文本中解析 reply 和 expression，无法解析时把原始文本作为回复并使用待机表情。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private AIReplyResult ParseReplyResult(string responseJson)
    {
        string rawText = ParseReplyText(responseJson);
        AIReplyResult result = new AIReplyResult
        {
            reply = rawText,
            expression = InferExpressionFromText(rawText)
        };

        string jsonObject = ExtractJsonObject(rawText);
        if (string.IsNullOrWhiteSpace(jsonObject))
        {
            return result;
        }

        try
        {
            AIReplyPayload payload = JsonUtility.FromJson<AIReplyPayload>(jsonObject);
            if (payload != null && !string.IsNullOrWhiteSpace(payload.reply))
            {
                result.reply = payload.reply;
                result.expression = ParseExpression(payload.expression, payload.reply);
            }
        }
        catch (Exception exception)
        {
            LogDebug("AI 回复 JSON 解析失败：" + exception.Message);
        }

        return result;
    }

    //
    // @param text AI 返回的原始文本。
    // @return string 文本中的第一个 JSON 对象字符串，未找到时返回空字符串。
    //
    // @summary 从模型返回文本中截取 JSON 对象，兼容模型偶尔包裹额外文本的情况。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private string ExtractJsonObject(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        int startIndex = text.IndexOf('{');
        int endIndex = text.LastIndexOf('}');
        if (startIndex < 0 || endIndex <= startIndex)
        {
            return string.Empty;
        }

        return text.Substring(startIndex, endIndex - startIndex + 1);
    }

    //
    // @param expressionText 模型返回的表情字符串。
    // @param replyText 模型返回的回复文本。
    // @return AIRoleExpression 解析后的表情枚举。
    //
    // @summary 优先解析模型返回的表情枚举，解析失败时根据回复文本推断表情。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private AIRoleExpression ParseExpression(string expressionText, string replyText)
    {
        string safeExpression = (expressionText ?? string.Empty).Trim();
        if (Enum.TryParse(safeExpression, true, out AIRoleExpression parsedExpression))
        {
            return parsedExpression;
        }

        switch (safeExpression)
        {
            case "待机":
            case "微笑":
                return AIRoleExpression.Idle;
            case "思考":
                return AIRoleExpression.Thinking;
            case "疑惑":
                return AIRoleExpression.Confused;
            case "抱歉":
            case "道歉":
                return AIRoleExpression.Apology;
            default:
                return InferExpressionFromText(replyText);
        }
    }

    //
    // @param replyText 模型返回的回复文本。
    // @return AIRoleExpression 根据文本内容推断出的表情枚举。
    //
    // @summary 当模型没有返回表情枚举时，根据回复文字中的关键词选择待机、思考、疑惑或抱歉表情。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private AIRoleExpression InferExpressionFromText(string replyText)
    {
        string text = (replyText ?? string.Empty).ToLowerInvariant();

        if (ContainsAny(text, "抱歉", "对不起", "不好意思", "sorry", "无法", "不能", "失败", "错误", "异常"))
        {
            return AIRoleExpression.Apology;
        }

        if (ContainsAny(text, "不确定", "不知道", "不清楚", "可能", "也许", "能否", "请确认", "?", "？"))
        {
            return AIRoleExpression.Confused;
        }

        if (ContainsAny(text, "分析", "思考", "推理", "判断", "步骤", "首先", "其次", "因此", "因为"))
        {
            return AIRoleExpression.Thinking;
        }

        return AIRoleExpression.Idle;
    }

    //
    // @param text 要检查的文本。
    // @param keywords 需要匹配的关键词数组。
    // @return bool 文本包含任一关键词时返回 true，否则返回 false。
    //
    // @summary 判断回复文本中是否包含指定关键词。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private bool ContainsAny(string text, params string[] keywords)
    {
        for (int i = 0; i < keywords.Length; i++)
        {
            if (text.Contains(keywords[i]))
            {
                return true;
            }
        }

        return false;
    }

    //
    // @param errorMessage 错误提示文本。
    // @return 无
    //
    // @summary 将 AI 请求错误显示到聊天界面的 AI 文本区域。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void ShowAIError(string errorMessage)
    {
        if (agentImagerController != null)
        {
            agentImagerController.ShowApology();
        }

        if (messageController != null)
        {
            messageController.ShowAIMessage(errorMessage);
        }
        else
        {
            LogDebug("错误无法显示到 UI：" + errorMessage);
        }
    }

    //
    // @param message 调试日志内容。
    // @return 无
    //
    // @summary 在开启调试日志时输出 AIChatClient 的请求流程状态。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log("[AIChatClient] " + message);
        }
    }
}

//
// @param 无
// @return 无
//
// @summary AnthropicMessageRequest 表示 Anthropic Messages API 的请求体结构。
// @checked: false. not reviewed by human, 07/06/2026.
//
[Serializable]
public class AnthropicMessageRequest
{
    public string model;
    public string system;
    public int max_tokens;
    public List<AnthropicRequestMessage> messages;
}

//
// @param 无
// @return 无
//
// @summary AnthropicRequestMessage 表示发送给 AI 的单条用户或助手消息。
// @checked: false. not reviewed by human, 07/06/2026.
//
[Serializable]
public class AnthropicRequestMessage
{
    public string role;
    public string content;
}

//
// @param 无
// @return 无
//
// @summary AnthropicMessageResponse 表示 Anthropic Messages API 的响应体结构。
// @checked: false. not reviewed by human, 07/06/2026.
//
[Serializable]
public class AnthropicMessageResponse
{
    public List<AnthropicResponseContent> content;
}

//
// @param 无
// @return 无
//
// @summary AnthropicResponseContent 表示 AI 响应中的一段内容。
// @checked: false. not reviewed by human, 07/06/2026.
//
[Serializable]
public class AnthropicResponseContent
{
    public string type;
    public string text;
}

//
// @param 无
// @return 无
//
// @summary AIReplyPayload 表示模型在 content.text 中返回的业务 JSON 结构。
// @checked: false. not reviewed by human, 07/06/2026.
//
[Serializable]
public class AIReplyPayload
{
    public string reply;
    public string expression;
}

//
// @param 无
// @return 无
//
// @summary AIReplyResult 表示解析后的 AI 回复文本和角色表情枚举。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class AIReplyResult
{
    public string reply;
    public AIRoleExpression expression;
}
