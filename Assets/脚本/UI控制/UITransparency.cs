using UnityEngine;
using UnityEngine.UI;

//
// @param 无
// @return 无
//
// @summary UITransparency 控制背景图和窗体整体透明度，用于实现半透明 UI 背景和半透明窗口。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class UITransparency : MonoBehaviour
{
    [SerializeField] private Graphic backgroundGraphic;
    [SerializeField] private CanvasGroup windowCanvasGroup;
    [SerializeField, Range(0f, 1f)] private float backgroundAlpha = 0.45f;
    [SerializeField, Range(0f, 1f)] private float windowAlpha = 0.9f;
    [SerializeField] private bool interactableWhenTransparent = true;

    //
    // @param 无
    // @return 无
    //
    // @summary 组件启动时应用背景和窗体透明度设置。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void Awake()
    {
        ApplyTransparency();
    }

    //
    // @param 无
    // @return 无
    //
    // @summary Inspector 参数变化时实时刷新透明度，方便在编辑器中预览效果。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnValidate()
    {
        ApplyTransparency();
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 把当前透明度参数应用到背景 Graphic 和窗体 CanvasGroup。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void ApplyTransparency()
    {
        ApplyBackgroundAlpha();
        ApplyWindowAlpha();
    }

    //
    // @param alpha 新的背景透明度，范围 0 到 1。
    // @return 无
    //
    // @summary 设置背景透明度并立即刷新背景显示。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SetBackgroundAlpha(float alpha)
    {
        backgroundAlpha = Mathf.Clamp01(alpha);
        ApplyBackgroundAlpha();
    }

    //
    // @param alpha 新的窗体透明度，范围 0 到 1。
    // @return 无
    //
    // @summary 设置窗体整体透明度并立即刷新窗体显示。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SetWindowAlpha(float alpha)
    {
        windowAlpha = Mathf.Clamp01(alpha);
        ApplyWindowAlpha();
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 修改背景 Graphic 的颜色透明度。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void ApplyBackgroundAlpha()
    {
        if (backgroundGraphic == null)
        {
            return;
        }

        Color color = backgroundGraphic.color;
        color.a = backgroundAlpha;
        backgroundGraphic.color = color;
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 修改窗体 CanvasGroup 的整体透明度和交互状态。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void ApplyWindowAlpha()
    {
        if (windowCanvasGroup == null)
        {
            return;
        }

        windowCanvasGroup.alpha = windowAlpha;
        windowCanvasGroup.interactable = interactableWhenTransparent;
        windowCanvasGroup.blocksRaycasts = interactableWhenTransparent;
    }
}
