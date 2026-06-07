using System.Text;
using UnityEngine;

//
// @param 无
// @return 无
//
// @summary AIRoleSO 保存 AI 助手的人格背景、说话风格和能力边界，并生成请求模型使用的 system prompt。
// @checked: false. not reviewed by human, 07/06/2026.
//
[CreateAssetMenu(fileName = "AI Role", menuName = "AI/AI Role")]
public class AIRoleSO : ScriptableObject
{
    [Header("Role Text")]
    [SerializeField] private string roleName = "奇异果助手";
    [SerializeField] private string identity = "一个住在用户桌面里的 Unity AI 宠物助手。";
    [SerializeField] private string personality = "温和、认真、机灵，带一点活泼，但不过度卖萌。";
    [SerializeField] private string speakingStyle = "优先使用中文，语气自然，句子不要太长，复杂问题拆成简单步骤。";
    [SerializeField] private string relationship = "像陪用户学习、写代码和整理灵感的小伙伴。";
    [SerializeField] private string abilityBoundary = "不知道的信息要说明不确定；涉及代码时给出可执行建议。";
    [SerializeField] private string forbiddenBehavior = "不要声称自己拥有真实身体、真实记忆或现实行动能力，不要编造事实。";
    [SerializeField, TextArea(6, 20)] private string customSystemPrompt;
    [SerializeField] private bool useCustomSystemPrompt;

    [Header("Expression Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite thinkingSprite;
    [SerializeField] private Sprite confusedSprite;
    [SerializeField] private Sprite apologySprite;

    //
    // @param 无
    // @return string AI 角色名称。
    //
    // @summary 向外部脚本提供当前角色名称。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public string RoleName => roleName;

    //
    // @param 无
    // @return Sprite 待机表情精灵图。
    //
    // @summary 向外部脚本提供待机状态使用的表情精灵图。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public Sprite IdleSprite => idleSprite;

    //
    // @param 无
    // @return Sprite 思考表情精灵图。
    //
    // @summary 向外部脚本提供思考状态使用的表情精灵图。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public Sprite ThinkingSprite => thinkingSprite;

    //
    // @param 无
    // @return Sprite 疑惑表情精灵图。
    //
    // @summary 向外部脚本提供疑惑状态使用的表情精灵图。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public Sprite ConfusedSprite => confusedSprite;

    //
    // @param 无
    // @return Sprite 抱歉表情精灵图。
    //
    // @summary 向外部脚本提供抱歉状态使用的表情精灵图。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public Sprite ApologySprite => apologySprite;

    //
    // @param 无
    // @return string 生成后的 system prompt。
    //
    // @summary 根据角色字段或自定义提示词生成发送给 AI 模型的人格设定。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public string BuildSystemPrompt()
    {
        if (useCustomSystemPrompt && !string.IsNullOrWhiteSpace(customSystemPrompt))
        {
            return customSystemPrompt.Trim();
        }

        StringBuilder builder = new StringBuilder();
        AppendLine(builder, "名字", roleName);
        AppendLine(builder, "身份", identity);
        AppendLine(builder, "性格", personality);
        AppendLine(builder, "说话风格", speakingStyle);
        AppendLine(builder, "和用户的关系", relationship);
        AppendLine(builder, "能力边界", abilityBoundary);
        AppendLine(builder, "禁止行为", forbiddenBehavior);
        return builder.ToString().Trim();
    }

    //
    // @param expression 需要获取的 AI 表情类型。
    // @return Sprite 对应表情的精灵图，未配置时返回 null。
    //
    // @summary 根据表情枚举返回 AIRoleSO 中配置的表情差分精灵图。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public Sprite GetExpressionSprite(AIRoleExpression expression)
    {
        switch (expression)
        {
            case AIRoleExpression.Idle:
                return idleSprite;
            case AIRoleExpression.Thinking:
                return thinkingSprite;
            case AIRoleExpression.Confused:
                return confusedSprite;
            case AIRoleExpression.Apology:
                return apologySprite;
            default:
                return idleSprite;
        }
    }

    //
    // @param builder 用于拼接提示词的 StringBuilder。
    // @param title 当前段落标题。
    // @param content 当前段落内容。
    // @return 无
    //
    // @summary 当内容不为空时把一段角色设定追加到 system prompt。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void AppendLine(StringBuilder builder, string title, string content)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            builder.Append(title);
            builder.Append("：");
            builder.AppendLine(content.Trim());
        }
    }
}

//
// @param 无
// @return 无
//
// @summary AIRoleExpression 定义 AI 角色当前可使用的表情状态。
// @checked: false. not reviewed by human, 07/06/2026.
//
public enum AIRoleExpression
{
    Idle,
    Thinking,
    Confused,
    Apology
}
