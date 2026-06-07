using System.Collections.Generic;
using UnityEngine;

//
// @param 无
// @return 无
//
// @summary AIContextBuilder 读取 MessageData 中最近 20 轮对话，并转换成发送给 AI 模型的上下文消息列表。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class AIContextBuilder : MonoBehaviour
{
    [SerializeField] private MessageData messageData;
    [SerializeField] private int maxDialogueCount = 20;
    [SerializeField] private bool enableDebugLog = true;

    //
    // @param 无
    // @return 无
    //
    // @summary 自动查找 MessageData 组件引用。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void Awake()
    {
        if (messageData == null)
        {
            messageData = FindObjectOfType<MessageData>();
        }
    }

    //
    // @param currentUserMessage 当前用户提交的消息内容，用于 MessageData 不存在时兜底。
    // @return List<AnthropicRequestMessage> 发送给 AI 模型的上下文消息列表。
    //
    // @summary 按 user 和 assistant 顺序读取最近 20 轮对话，组成 Anthropic Messages 请求需要的上下文。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public List<AnthropicRequestMessage> BuildMessages(string currentUserMessage)
    {
        List<AnthropicRequestMessage> messages = new List<AnthropicRequestMessage>();

        if (messageData == null)
        {
            AddMessage(messages, "user", currentUserMessage);
            LogDebug("未找到 MessageData，仅发送当前用户消息。");
            return messages;
        }

        int userCount = messageData.UserMessages.Count;
        int aiCount = messageData.AIMessages.Count;
        int dialogueCount = Mathf.Max(userCount, aiCount);
        int safeMaxCount = Mathf.Max(1, maxDialogueCount);
        int startIndex = Mathf.Max(0, dialogueCount - safeMaxCount);

        for (int i = startIndex; i < dialogueCount; i++)
        {
            if (i < userCount)
            {
                AddMessage(messages, "user", messageData.UserMessages[i].content);
            }

            if (i < aiCount)
            {
                AddMessage(messages, "assistant", messageData.AIMessages[i].content);
            }
        }

        if (messages.Count == 0)
        {
            AddMessage(messages, "user", currentUserMessage);
        }

        LogDebug("上下文构建完成，消息条数：" + messages.Count);
        return messages;
    }

    //
    // @param messages 要追加消息的上下文列表。
    // @param role 消息角色，通常是 user 或 assistant。
    // @param content 消息文本内容。
    // @return 无
    //
    // @summary 当消息内容不为空时，把一条上下文消息追加到列表。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void AddMessage(List<AnthropicRequestMessage> messages, string role, string content)
    {
        string safeContent = (content ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(safeContent))
        {
            return;
        }

        messages.Add(new AnthropicRequestMessage
        {
            role = role,
            content = safeContent
        });
    }

    //
    // @param message 调试日志内容。
    // @return 无
    //
    // @summary 在开启调试日志时输出上下文构建状态。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log("[AIContextBuilder] " + message);
        }
    }
}
