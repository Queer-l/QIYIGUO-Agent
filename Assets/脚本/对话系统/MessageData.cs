using System;
using System.Collections.Generic;
using UnityEngine;

//
// @param 无
// @return 无
//
// @summary MessageData 保存对话系统中的用户消息和 AI 消息，并为 MessageController 提供增删查数据接口。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class MessageData : MonoBehaviour
{
    [SerializeField] private List<UserMessageInfo> userMessages = new List<UserMessageInfo>();
    [SerializeField] private List<AIMessageInfo> aiMessages = new List<AIMessageInfo>();

    //
    // @param 无
    // @return IReadOnlyList<UserMessageInfo> 当前保存的用户消息只读列表。
    //
    // @summary 向外部脚本提供用户消息历史记录。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public IReadOnlyList<UserMessageInfo> UserMessages => userMessages;

    //
    // @param 无
    // @return IReadOnlyList<AIMessageInfo> 当前保存的 AI 消息只读列表。
    //
    // @summary 向外部脚本提供 AI 消息历史记录。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public IReadOnlyList<AIMessageInfo> AIMessages => aiMessages;

    //
    // @param content 用户输入的消息内容。
    // @return UserMessageInfo 新保存的用户消息数据。
    //
    // @summary 创建用户消息数据并保存到用户消息列表。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public UserMessageInfo AddUserMessage(string content)
    {
        UserMessageInfo message = new UserMessageInfo(content, DateTime.Now.ToString("O"));
        userMessages.Add(message);
        return message;
    }

    //
    // @param content AI 返回的消息内容。
    // @return AIMessageInfo 新保存的 AI 消息数据。
    //
    // @summary 创建 AI 消息数据并保存到 AI 消息列表。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public AIMessageInfo AddAIMessage(string content)
    {
        AIMessageInfo message = new AIMessageInfo(content, DateTime.Now.ToString("O"));
        aiMessages.Add(message);
        return message;
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 清空当前保存的用户消息和 AI 消息历史记录。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void Clear()
    {
        userMessages.Clear();
        aiMessages.Clear();
    }

    //
    // @param savedUserMessages 读取到的用户消息列表。
    // @param savedAIMessages 读取到的 AI 消息列表。
    // @return 无
    //
    // @summary 使用 JSON 文件中读取到的对话记录覆盖当前内存中的消息数据。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void LoadMessages(List<UserMessageInfo> savedUserMessages, List<AIMessageInfo> savedAIMessages)
    {
        userMessages.Clear();
        aiMessages.Clear();

        if (savedUserMessages != null)
        {
            userMessages.AddRange(savedUserMessages);
        }

        if (savedAIMessages != null)
        {
            aiMessages.AddRange(savedAIMessages);
        }
    }
}

//
// @param 无
// @return 无
//
// @summary UserMessageInfo 保存单条用户消息的文本内容和创建时间。
// @checked: false. not reviewed by human, 07/06/2026.
//
[Serializable]
public class UserMessageInfo
{
    public string content;
    public string createdAt;

    //
    // @param 无
    // @return 无
    //
    // @summary 为 Unity JsonUtility 反序列化保留的默认构造函数。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public UserMessageInfo()
    {
    }

    //
    // @param content 用户消息文本内容。
    // @param createdAt 用户消息创建时间。
    // @return 无
    //
    // @summary 初始化用户消息数据。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public UserMessageInfo(string content, string createdAt)
    {
        this.content = content;
        this.createdAt = createdAt;
    }
}

//
// @param 无
// @return 无
//
// @summary AIMessageInfo 保存单条 AI 消息的文本内容和创建时间。
// @checked: false. not reviewed by human, 07/06/2026.
//
[Serializable]
public class AIMessageInfo
{
    public string content;
    public string createdAt;

    //
    // @param 无
    // @return 无
    //
    // @summary 为 Unity JsonUtility 反序列化保留的默认构造函数。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public AIMessageInfo()
    {
    }

    //
    // @param content AI 消息文本内容。
    // @param createdAt AI 消息创建时间。
    // @return 无
    //
    // @summary 初始化 AI 消息数据。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public AIMessageInfo(string content, string createdAt)
    {
        this.content = content;
        this.createdAt = createdAt;
    }
}
