using TMPro;
using UnityEngine;
using UnityEngine.UI;

//
// @param 无
// @return 无
//
// @summary AIMessage 负责把 AI 返回文本显示到 TMP 组件，并根据文本高度调整自身和滚动 Content 的高度。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class AIMessage : MonoBehaviour
{
    [SerializeField] private TMP_Text tmp;
    [SerializeField] private RectTransform contentRectTransform;
    [SerializeField] private float bottomPadding = 30f;

    //
    // @param 无
    // @return 无
    //
    // @summary 自动获取 TMP 文本组件和父级 Content 节点引用。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void Awake()
    {
        if (tmp == null)
        {
            tmp = GetComponent<TMP_Text>();
        }

        if (contentRectTransform == null && transform.parent != null)
        {
            contentRectTransform = transform.parent as RectTransform;
        }
    }

    //
    // @param message AI 返回的完整文本内容。
    // @return 无
    //
    // @summary 将 AI 返回文本显示到 TMP 组件，并刷新文本与 Content 的高度。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SetMessage(string message)
    {
        if (tmp == null)
        {
            return;
        }

        tmp.text = message;
        ResizeToText();
    }

    //
    // @param messageDelta AI 流式返回的一段追加文本。
    // @return 无
    //
    // @summary 将 AI 新返回的片段追加到 TMP 文本末尾，并刷新文本与 Content 的高度。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void AppendMessage(string messageDelta)
    {
        if (tmp == null)
        {
            return;
        }

        tmp.text += messageDelta;
        ResizeToText();
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 根据 TMP 的首选高度调整文本 RectTransform 和父级 Content 的高度。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void ResizeToText()
    {
        if (tmp == null)
        {
            return;
        }

        RectTransform textRectTransform = tmp.rectTransform;
        float textWidth = textRectTransform.rect.width;
        if (textWidth <= 0f && contentRectTransform != null)
        {
            textWidth = contentRectTransform.rect.width;
        }

        float preferredHeight = tmp.GetPreferredValues(tmp.text, textWidth, 0f).y;

        textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);

        RectTransform selfRectTransform = transform as RectTransform;
        if (selfRectTransform != null && selfRectTransform != textRectTransform)
        {
            selfRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
        }

        if (contentRectTransform != null)
        {
            float contentHeight = Mathf.Abs(textRectTransform.anchoredPosition.y) + preferredHeight + bottomPadding;
            contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(contentRectTransform.rect.height, contentHeight));
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform);
        }
    }
}
