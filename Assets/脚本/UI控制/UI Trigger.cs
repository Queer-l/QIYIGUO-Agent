using UnityEngine;

//
// @param 无
// @return 无
//
// @summary UITrigger 对外提供 UI 面板打开和关闭函数，供按钮或其他脚本调用。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class UITrigger : MonoBehaviour
{
    [SerializeField] private GameObject targetUI;

    //
    // @param 无
    // @return 无
    //
    // @summary 将目标 UI 对象设置为激活状态，用于打开面板。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void OpenUI()
    {
        if (targetUI != null)
        {
            targetUI.SetActive(true);
        }
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 将目标 UI 对象设置为非激活状态，用于关闭面板。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void CloseUI()
    {
        if (targetUI != null)
        {
            targetUI.SetActive(false);
        }
    }
}
