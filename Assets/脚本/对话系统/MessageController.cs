using System;
using TMPro;
using UnityEngine;

//
// @param 无
// @return 无
//
// @summary MessageController 负责读取用户输入、保存对话数据，并把 AI 回复交给固定的 AIMessage 文本组件显示。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class MessageController : MonoBehaviour
{
    [SerializeField] private MessageData messageData;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private AIMessage aiMessage;
    [SerializeField] private bool enableDebugLog = true;

    //
    // @param string 用户提交的消息文本。
    // @return 无
    //
    // @summary 当用户消息提交成功后触发，外部 AI 请求脚本可监听此事件开始请求模型回复。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public event Action<string> UserMessageSubmitted;

    //
    // @param 无
    // @return 无
    //
    // @summary 初始化消息数据组件引用。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void Awake()
    {
        if (messageData == null)
        {
            messageData = GetComponent<MessageData>();
        }

        LogDebug("MessageController 初始化完成。");
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 读取输入框内容并作为用户消息发送，可直接绑定到发送按钮 OnClick。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SendInputMessage()
    {
        if (inputField == null)
        {
            LogDebug("发送失败：未绑定输入框。");
            return;
        }

        LogDebug("点击发送按钮，准备读取输入框内容。");
        SendUserMessage(inputField.text);
    }

    //
    // @param content 用户输入的消息内容。
    // @return 无
    //
    // @summary 保存用户消息、清空输入框，并通知外部开始 AI 对话。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SendUserMessage(string content)
    {
        string message = (content ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(message))
        {
            LogDebug("发送取消：输入内容为空。");
            return;
        }

        if (messageData != null)
        {
            messageData.AddUserMessage(message);
            LogDebug("用户消息已保存：" + message);
        }
        else
        {
            LogDebug("用户消息未保存：未绑定 MessageData。");
        }

        if (inputField != null)
        {
            inputField.text = string.Empty;
            inputField.ActivateInputField();
        }

        LogDebug("用户消息事件已触发，等待 AIChatClient 请求 AI。");
        UserMessageSubmitted?.Invoke(message);
    }

    //
    // @param content AI 返回的消息内容。
    // @return 无
    //
    // @summary 保存 AI 消息，并把 AI 回复显示到固定的 AIMessage 文本组件上。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void ShowAIMessage(string content)
    {
        string message = (content ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(message))
        {
            LogDebug("AI 回复显示取消：回复内容为空。");
            return;
        }

        if (messageData != null)
        {
            messageData.AddAIMessage(message);
            LogDebug("AI 消息已保存。长度：" + message.Length);
        }
        else
        {
            LogDebug("AI 消息未保存：未绑定 MessageData。");
        }

        if (aiMessage != null)
        {
            aiMessage.SetMessage(message);
            LogDebug("AI 回复已显示到 AIMessage。");
        }
        else
        {
            LogDebug("AI 回复显示失败：未绑定 AIMessage。");
        }
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 在 AI 正式回复前显示临时的“思考中”状态，此状态不会保存到 MessageData。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void ShowAIThinking()
    {
        if (aiMessage != null)
        {
            aiMessage.SetMessage("思考中");
            LogDebug("AI 临时状态已显示：思考中。");
        }
        else
        {
            LogDebug("AI 临时状态显示失败：未绑定 AIMessage。");
        }
    }

    //
    // @param message 调试日志内容。
    // @return 无
    //
    // @summary 在开启调试日志时输出 MessageController 的发送流程状态。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log("[MessageController] " + message);
        }
    }
}
