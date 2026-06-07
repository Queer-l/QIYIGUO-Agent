using System;

//
// @param 无
// @return 无
//
// @summary DialogueEventBus 提供对话系统事件，用于通知历史记录变化并解耦数据脚本和历史 UI。
// @checked: false. not reviewed by human, 07/06/2026.
//
public static class DialogueEventBus
{
    //
    // @param 无
    // @return 无
    //
    // @summary 当对话历史记录被读取、清空或修改后触发。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public static event Action DialogueHistoryChanged;

    //
    // @param 无
    // @return 无
    //
    // @summary 发布对话历史变化事件，通知历史 UI 刷新。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public static void PublishDialogueHistoryChanged()
    {
        DialogueHistoryChanged?.Invoke();
    }
}
