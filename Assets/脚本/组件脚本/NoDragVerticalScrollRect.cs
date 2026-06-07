using UnityEngine.EventSystems;
using UnityEngine.UI;

//
// @param 无
// @return 无
//
// @summary NoDragVerticalScrollRect 禁止 ScrollRect 的鼠标或触摸拖拽，同时保留滚轮、滚动条和代码控制滚动。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class NoDragVerticalScrollRect : ScrollRect
{
    //
    // @param eventData 拖拽开始事件数据。
    // @return 无
    //
    // @summary 拦截拖拽开始事件，不调用父类逻辑，从而禁止用户拖拽滚动区域。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public override void OnBeginDrag(PointerEventData eventData)
    {
    }

    //
    // @param eventData 拖拽过程事件数据。
    // @return 无
    //
    // @summary 拦截拖拽过程事件，不改变 ScrollRect 的滚动位置。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public override void OnDrag(PointerEventData eventData)
    {
    }

    //
    // @param eventData 拖拽结束事件数据。
    // @return 无
    //
    // @summary 拦截拖拽结束事件，防止 ScrollRect 进入拖拽收尾逻辑。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public override void OnEndDrag(PointerEventData eventData)
    {
    }
}
