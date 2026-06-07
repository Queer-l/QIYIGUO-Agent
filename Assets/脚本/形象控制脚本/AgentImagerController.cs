using UnityEngine;
using UnityEngine.UI;

//
// @param 无
// @return 无
//
// @summary AgentImagerController 控制 AI 助手形象 Image，根据 AIRoleSO 中配置的表情差分切换精灵图。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class AgentImagerController : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private AISettings aiSettings;
    [SerializeField] private AIRoleExpression defaultExpression = AIRoleExpression.Idle;
    [SerializeField] private bool enableDebugLog = true;

    //
    // @param 无
    // @return 无
    //
    // @summary 自动获取 Image 组件，并切换到默认待机表情。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void Awake()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if (aiSettings == null)
        {
            aiSettings = FindObjectOfType<AISettings>();
        }

        SetExpression(defaultExpression);
    }

    //
    // @param expression 需要切换到的表情枚举。
    // @return 无
    //
    // @summary 根据表情枚举从 AIRoleSO 读取精灵图并应用到 Image。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SetExpression(AIRoleExpression expression)
    {
        if (image == null || aiSettings == null)
        {
            LogDebug("切换表情失败：Image 或 AISettings 未绑定。");
            return;
        }

        Sprite expressionSprite = aiSettings.GetExpressionSprite(expression);
        if (expressionSprite == null)
        {
            LogDebug("切换表情失败：未配置精灵图 " + expression);
            return;
        }

        image.sprite = expressionSprite;
        LogDebug("已切换表情：" + expression);
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 切换到待机表情。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void ShowIdle()
    {
        SetExpression(AIRoleExpression.Idle);
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 切换到思考表情。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void ShowThinking()
    {
        SetExpression(AIRoleExpression.Thinking);
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 切换到疑惑表情。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void ShowConfused()
    {
        SetExpression(AIRoleExpression.Confused);
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 切换到抱歉表情。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void ShowApology()
    {
        SetExpression(AIRoleExpression.Apology);
    }

    //
    // @param message 调试日志内容。
    // @return 无
    //
    // @summary 在开启调试日志时输出形象差分切换状态。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log("[AgentImagerController] " + message);
        }
    }
}
