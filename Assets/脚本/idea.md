用unity做的ai助手


任务1
完成ConfigInfo 脚本，绑定两个文本组件，显示当前的AI的model和url，同时绑定一个输入框，和一个按钮，点击按钮后将输入框里的api更改到AI Setting里

任务2
气泡预制体制作完毕
要求
1，实现脚本 AI message，挂载到了ui上，成员组件tmp，将ai返回的对话消息显示在这上面（不需要气泡，直接显示文本），文本在滚动条的content下，这个脚本也需要根据AI模型给的文本长度动态调整content的高度（向下拓高）。
对话功能通过messagecontroller实现，对话文本数据保存到Messagedata里面，ai与用户信息保存到不同的数据结构里

任务3
在api配置好后ai模型回应前的对话框显示“思考中”

任务4
完成历史对话ui脚本（现在已经有了预制体了，这个脚本只需要读取历史对话并在content生成气泡即可（限制显示上限20个对话））

任务5
AgentImagerController形象控制
成员组件 image
要求提供切换差分的函数，
平时情况 使用 待机（微笑）

发送请求后 返回前 使用 思考

请求体和返回体 新增要求 返回表情枚举值的其中一个，通知AgentImagerController更换形象差分