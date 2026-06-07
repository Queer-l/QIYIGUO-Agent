using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//
// @param 无
// @return 无
//
// @summary UserData 负责在程序启动时读取用户 API 配置和对话记录，并在程序关闭时保存为 JSON 文件。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class UserData : MonoBehaviour
{
    [SerializeField] private AISettings aiSettings;
    [SerializeField] private MessageData messageData;
    [SerializeField] private string fileName = "user-data.json";
    [SerializeField] private bool saveApiKey = true;
    [SerializeField] private bool enableDebugLog = true;

    private string cachedApiKey;

    //
    // @param 无
    // @return string 用户数据 JSON 文件的完整路径。
    //
    // @summary 拼接 Unity 持久化目录和配置文件名。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private string SavePath => Path.Combine(Application.persistentDataPath, fileName);

    //
    // @param 无
    // @return 无
    //
    // @summary 自动查找依赖组件并订阅 API Key 修改事件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void Awake()
    {
        if (aiSettings == null)
        {
            aiSettings = FindObjectOfType<AISettings>();
        }

        if (messageData == null)
        {
            messageData = FindObjectOfType<MessageData>();
        }

        AIEventBus.AuthTokenChangeRequested += CacheApiKey;
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 程序启动时读取 JSON 文件并恢复 API 配置和对话记录。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void Start()
    {
        LoadData();
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 组件销毁时取消订阅 API Key 修改事件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnDestroy()
    {
        AIEventBus.AuthTokenChangeRequested -= CacheApiKey;
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 程序关闭时保存 API 配置和对话记录到 JSON 文件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnApplicationQuit()
    {
        SaveData();
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 从 JSON 文件读取用户数据，并恢复到 AISettings 和 MessageData。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void LoadData()
    {
        if (!File.Exists(SavePath))
        {
            LogDebug("未找到用户数据文件：" + SavePath);
            return;
        }

        string json = File.ReadAllText(SavePath);
        UserDataFile dataFile = JsonUtility.FromJson<UserDataFile>(json);
        if (dataFile == null)
        {
            LogDebug("用户数据读取失败：JSON 内容为空或格式错误。");
            return;
        }

        if (aiSettings != null)
        {
            aiSettings.SetBaseUrl(dataFile.baseUrl);

            if (saveApiKey && !string.IsNullOrWhiteSpace(dataFile.apiKey))
            {
                cachedApiKey = dataFile.apiKey;
                AIEventBus.RequestAuthTokenChange(dataFile.apiKey);
            }
        }

        if (messageData != null)
        {
            messageData.LoadMessages(dataFile.userMessages, dataFile.aiMessages);
            DialogueEventBus.PublishDialogueHistoryChanged();
        }

        LogDebug("用户数据读取完成：" + SavePath);
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 将当前 API 配置和 MessageData 中的对话记录保存到 JSON 文件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SaveData()
    {
        UserDataFile dataFile = new UserDataFile();

        if (aiSettings != null)
        {
            dataFile.baseUrl = aiSettings.BaseUrl;
            dataFile.apiKey = saveApiKey ? GetApiKeyForSave() : string.Empty;
        }

        if (messageData != null)
        {
            dataFile.userMessages = new List<UserMessageInfo>(messageData.UserMessages);
            dataFile.aiMessages = new List<AIMessageInfo>(messageData.AIMessages);
        }

        string json = JsonUtility.ToJson(dataFile, true);
        Directory.CreateDirectory(Application.persistentDataPath);
        File.WriteAllText(SavePath, json);
        LogDebug("用户数据保存完成：" + SavePath);
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 给退出按钮绑定使用，先保存用户数据，然后退出程序或停止编辑器播放。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SaveDataAndQuit()
    {
        SaveData();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 清空当前对话历史记录并立即保存到 JSON 文件，保留 API 配置。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void ClearDialogueHistory()
    {
        if (messageData != null)
        {
            messageData.Clear();
            SaveData();
            DialogueEventBus.PublishDialogueHistoryChanged();
            LogDebug("历史对话已清空并保存。");
        }
        else
        {
            LogDebug("清空历史对话失败：未绑定 MessageData。");
        }
    }

    //
    // @param apiKey 用户输入的新 API Key。
    // @return 无
    //
    // @summary 缓存用户通过配置界面提交的 API Key，便于后续保存。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void CacheApiKey(string apiKey)
    {
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            cachedApiKey = apiKey.Trim();
        }
    }

    //
    // @param 无
    // @return string 当前需要保存到 JSON 文件的 API Key。
    //
    // @summary 优先返回配置界面提交或文件读取到的 API Key，没有缓存时再读取 AISettings 当前 Token。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private string GetApiKeyForSave()
    {
        if (!string.IsNullOrWhiteSpace(cachedApiKey))
        {
            return cachedApiKey;
        }

        if (aiSettings != null)
        {
            return aiSettings.AuthToken;
        }

        return string.Empty;
    }

    //
    // @param message 调试日志内容。
    // @return 无
    //
    // @summary 在开启调试日志时输出用户数据读写流程状态。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log("[UserData] " + message);
        }
    }
}

//
// @param 无
// @return 无
//
// @summary UserDataFile 表示保存到 JSON 文件中的 API 配置和对话记录结构。
// @checked: false. not reviewed by human, 07/06/2026.
//
[Serializable]
public class UserDataFile
{
    public string apiKey;
    public string baseUrl;
    public List<UserMessageInfo> userMessages = new List<UserMessageInfo>();
    public List<AIMessageInfo> aiMessages = new List<AIMessageInfo>();
}
