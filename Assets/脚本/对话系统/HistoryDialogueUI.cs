using System.Collections;
using TMPro;
using UnityEngine;

//
// @param 无
// @return 无
//
// @summary HistoryDialogueUI 读取 MessageData 中的历史对话记录，并在 Content 下生成用户和 AI 气泡。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class HistoryDialogueUI : MonoBehaviour
{
    [SerializeField] private MessageData messageData;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject userBubblePrefab;
    [SerializeField] private GameObject aiBubblePrefab;
    [SerializeField] private int maxDialogueCount = 20;
    [SerializeField] private bool refreshOnEnable = true;
    [SerializeField] private bool clearContentBeforeGenerate = true;
    [SerializeField] private bool enableDebugLog = true;

    //
    // @param 无
    // @return 无
    //
    // @summary 自动查找 MessageData，并在启用时延迟刷新历史对话 UI。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnEnable()
    {
        DialogueEventBus.DialogueHistoryChanged += RefreshHistory;

        if (messageData == null)
        {
            messageData = FindObjectOfType<MessageData>();
        }

        if (refreshOnEnable)
        {
            StartCoroutine(RefreshNextFrame());
        }
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 组件禁用时取消监听对话历史变化事件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnDisable()
    {
        DialogueEventBus.DialogueHistoryChanged -= RefreshHistory;
    }

    //
    // @param 无
    // @return IEnumerator Unity 协程迭代器。
    //
    // @summary 延迟一帧刷新历史对话，确保 UserData 有机会先读取 JSON 记录。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private IEnumerator RefreshNextFrame()
    {
        yield return null;
        RefreshHistory();
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 清空旧气泡并按时间顺序生成最多 20 组历史对话气泡。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void RefreshHistory()
    {
        if (messageData == null || content == null)
        {
            LogDebug("刷新失败：MessageData 或 Content 未绑定。");
            return;
        }

        if (clearContentBeforeGenerate)
        {
            ClearContent();
        }

        int userCount = messageData.UserMessages.Count;
        int aiCount = messageData.AIMessages.Count;
        int dialogueCount = Mathf.Max(userCount, aiCount);
        int safeMaxCount = Mathf.Max(0, maxDialogueCount);
        int startIndex = Mathf.Max(0, dialogueCount - safeMaxCount);

        for (int i = startIndex; i < dialogueCount; i++)
        {
            if (i < userCount)
            {
                CreateBubble(userBubblePrefab, messageData.UserMessages[i].content);
            }

            if (i < aiCount)
            {
                CreateBubble(aiBubblePrefab, messageData.AIMessages[i].content);
            }
        }

        LogDebug("历史对话刷新完成，显示组数：" + Mathf.Min(dialogueCount, safeMaxCount));
    }

    //
    // @param bubblePrefab 要生成的气泡预制体。
    // @param message 气泡内显示的文本内容。
    // @return GameObject 新生成的气泡对象，生成失败时返回 null。
    //
    // @summary 在 Content 下生成一个气泡，并把文本写入气泡内部第一个 TMP_Text。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private GameObject CreateBubble(GameObject bubblePrefab, string message)
    {
        if (bubblePrefab == null || content == null)
        {
            return null;
        }

        GameObject bubble = Instantiate(bubblePrefab, content);
        TMP_Text bubbleText = bubble.GetComponentInChildren<TMP_Text>();

        if (bubbleText != null)
        {
            bubbleText.text = message;
        }
        else
        {
            LogDebug("气泡生成成功，但预制体中没有 TMP_Text。");
        }

        return bubble;
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 删除 Content 下已有子对象，为重新生成历史气泡做准备。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void ClearContent()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    //
    // @param message 调试日志内容。
    // @return 无
    //
    // @summary 在开启调试日志时输出历史对话 UI 刷新状态。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log("[HistoryDialogueUI] " + message);
        }
    }
}
