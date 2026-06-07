using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

//
// @param 无
// @return 无
//
// @summary WindowSettings 独立控制 Windows 桌面窗口：隐藏标题栏和边框、背景透明、窗口置顶，并在分辨率变化后自动恢复样式。
// @checked: false. not reviewed by human, 07/06/2026.
//
public class WindowSettings : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Window")]
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private bool forceWindowedMode = true;
    [SerializeField] private bool borderless = true;
    [SerializeField] private bool transparent = true;
    [SerializeField] private bool alwaysOnTop = true;
    [SerializeField] private bool clickThrough = false;
    [SerializeField] private bool draggable = true;

    [Header("Transparency")]
    [SerializeField] private bool useDwmFrameExtension = true;
    [SerializeField] private bool useColorKeyTransparency = false;
    [SerializeField] private Color transparentClearColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color transparentColorKey = new Color(1f, 0f, 1f, 1f);
    [SerializeField] private bool previewColorKeyInEditor = false;
    [SerializeField] private Color editorPreviewBackgroundColor = new Color(0f, 0f, 0f, 0f);

    [Header("Timing")]
    [SerializeField] private float startApplyDelay = 0.25f;
    [SerializeField] private int reapplyFramesAfterResolutionChange = 30;
    [SerializeField] private int secondRefreshFrames = 10;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    [Header("Transparent Area Click Through")]
    [SerializeField] private bool transparentAreaClickThrough = true;
    [SerializeField] private float hitTestRefreshInterval = 0.02f;
    [SerializeField] private Camera uiEventCamera;
    [SerializeField] private RectTransform[] panels = new RectTransform[0];

    private IntPtr windowHandle = IntPtr.Zero;
    private Coroutine applyCoroutine;
    private Coroutine reapplyCoroutine;
    private bool cursorOverSolidArea = true;
    private bool mousePassThroughApplied;
    private float nextHitTestTime;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    private const int GWL_WNDPROC = -4;
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;

    private const long WS_CAPTION = 0x00C00000L;
    private const long WS_THICKFRAME = 0x00040000L;
    private const long WS_MINIMIZEBOX = 0x00020000L;
    private const long WS_MAXIMIZEBOX = 0x00010000L;
    private const long WS_SYSMENU = 0x00080000L;
    private const long WS_POPUP = 0x80000000L;
    private const long WS_VISIBLE = 0x10000000L;

    private const long WS_EX_LAYERED = 0x00080000L;
    private const long WS_EX_TRANSPARENT = 0x00000020L;

    private const uint LWA_COLORKEY = 0x00000001;
    private const uint LWA_ALPHA = 0x00000002;

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_FRAMECHANGED = 0x0020;
    private const uint SWP_SHOWWINDOW = 0x0040;

    private const uint WM_NCLBUTTONDOWN = 0x00A1;
    private const uint WM_NCHITTEST = 0x0084;
    private const int HTCAPTION = 2;
    private const int HTTRANSPARENT = -1;

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    private delegate IntPtr WindowProcedure(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private IntPtr originalWindowProcedure = IntPtr.Zero;
    private IntPtr procedureWindowHandle = IntPtr.Zero;
    private WindowProcedure windowProcedure;

    [StructLayout(LayoutKind.Sequential)]
    private struct WinPoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Margins
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentProcessId();

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins margins);

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr previousWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out WinPoint point);

    [DllImport("user32.dll")]
    private static extern bool ScreenToClient(IntPtr hWnd, ref WinPoint point);
#endif

    //
    // @param 无
    // @return 无
    //
    // @summary 在第一帧前初始化相机引用，并把透明窗口强制切到普通窗口模式。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void Awake()
    {
        ResolveTargetCamera();
        ApplyWindowedMode();
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 初始化相机引用并按需延迟应用窗口设置。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void Start()
    {
        ResolveTargetCamera();

        if (applyOnStart)
        {
            ScheduleApply(startApplyDelay);
        }
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 监听分辨率变化事件，分辨率切换后重新应用窗口样式。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnEnable()
    {
        GlobalConfigEventBus.WindowResolutionChangeRequested += OnWindowResolutionChanged;
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 取消监听分辨率变化事件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnDisable()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        SetMousePassThrough(false);
        RestoreWindowProcedure();
#endif
        GlobalConfigEventBus.WindowResolutionChangeRequested -= OnWindowResolutionChanged;
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 每帧更新鼠标是否位于可点击 UI 区域，用于透明区域点击穿透判断。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void Update()
    {
        UpdateTransparentAreaHitState();
    }

    //
    // @param width 新窗口宽度。
    // @param height 新窗口高度。
    // @return 无
    //
    // @summary 窗口分辨率变化后清除旧句柄，并延迟重新应用透明无边框样式。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void OnWindowResolutionChanged(int width, int height)
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        RestoreWindowProcedure();
#endif
        windowHandle = IntPtr.Zero;
        ApplyCameraColorKey();

        if (reapplyCoroutine != null)
        {
            StopCoroutine(reapplyCoroutine);
        }

        reapplyCoroutine = StartCoroutine(ReapplyAfterResolutionChange());
    }

    //
    // @param delay 延迟秒数。
    // @return 无
    //
    // @summary 安排一次延迟应用窗口设置，避免窗口刚创建时句柄还未稳定。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void ScheduleApply(float delay)
    {
        if (applyCoroutine != null)
        {
            StopCoroutine(applyCoroutine);
        }

        applyCoroutine = StartCoroutine(ApplyAfterDelay(delay));
    }

    //
    // @param delay 延迟秒数。
    // @return IEnumerator Unity 协程迭代器。
    //
    // @summary 等待指定时间后应用窗口设置。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private IEnumerator ApplyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ApplyWindowSettings();
        applyCoroutine = null;
    }

    //
    // @param 无
    // @return IEnumerator Unity 协程迭代器。
    //
    // @summary 分辨率变化后等待 Unity 重建窗口，再重复应用窗口设置以保证样式稳定。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private IEnumerator ReapplyAfterResolutionChange()
    {
        for (int i = 0; i < reapplyFramesAfterResolutionChange; i++)
        {
            yield return new WaitForEndOfFrame();
        }

        ApplyWindowSettings();

        for (int i = 0; i < secondRefreshFrames; i++)
        {
            yield return new WaitForEndOfFrame();
        }

        ApplyWindowSettings();
        reapplyCoroutine = null;
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 应用透明背景、无边框、置顶和点击穿透设置。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void ApplyWindowSettings()
    {
        ApplyWindowedMode();
        ApplyCameraColorKey();
        ApplyNativeWindowSettings();
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 自动查找主相机，减少 Inspector 漏绑导致的透明背景失效。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void ResolveTargetCamera()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 将 Unity 运行窗口切换为 Windowed，避免全屏窗口模式阻止桌面透明。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void ApplyWindowedMode()
    {
        if (!forceWindowedMode)
        {
            return;
        }

        if (Screen.fullScreenMode == FullScreenMode.Windowed && !Screen.fullScreen)
        {
            return;
        }

        int safeWidth = Mathf.Max(1, Screen.width);
        int safeHeight = Mathf.Max(1, Screen.height);
        Screen.fullScreenMode = FullScreenMode.Windowed;
        Screen.fullScreen = false;
        Screen.SetResolution(safeWidth, safeHeight, FullScreenMode.Windowed);
        LogDebug("已强制切换为窗口模式：" + safeWidth + "x" + safeHeight);
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 将相机背景设置为透明色键颜色，让窗口背景区域可以被系统抠除。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void ApplyCameraColorKey()
    {
        if (!transparent || targetCamera == null)
        {
            return;
        }

        targetCamera.clearFlags = CameraClearFlags.SolidColor;

        if (ShouldUseNativeTransparency())
        {
            targetCamera.backgroundColor = useColorKeyTransparency ? transparentColorKey : transparentClearColor;
            LogDebug(useColorKeyTransparency
                ? "相机背景色键已设置：" + ColorUtility.ToHtmlStringRGB(transparentColorKey)
                : "相机透明清屏色已设置。");
            return;
        }

        if (previewColorKeyInEditor)
        {
            targetCamera.backgroundColor = transparentColorKey;
            LogDebug("Editor 色键预览已开启，当前显示洋红色仅用于打包透明测试。");
            return;
        }

        targetCamera.backgroundColor = editorPreviewBackgroundColor;
        LogDebug("当前环境不支持原生窗口透明，已使用 Editor 预览背景色。");
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 在 Windows 构建中应用原生窗口样式。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void ApplyNativeWindowSettings()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        IntPtr hWnd = FindUnityWindow();
        if (hWnd == IntPtr.Zero)
        {
            LogDebug("窗口句柄获取失败。");
            return;
        }

        windowHandle = hWnd;

        if (borderless)
        {
            long style = GetWindowLong(hWnd, GWL_STYLE);
            style &= ~WS_CAPTION;
            style &= ~WS_THICKFRAME;
            style &= ~WS_MINIMIZEBOX;
            style &= ~WS_MAXIMIZEBOX;
            style &= ~WS_SYSMENU;
            style |= WS_POPUP | WS_VISIBLE;
            SetWindowLong(hWnd, GWL_STYLE, style);
        }

        bool shouldUseLayeredWindow = transparent && useColorKeyTransparency || clickThrough;
        long exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
        if (shouldUseLayeredWindow)
        {
            exStyle |= WS_EX_LAYERED;
        }
        else
        {
            exStyle &= ~WS_EX_LAYERED;
        }

        if (clickThrough)
        {
            exStyle |= WS_EX_TRANSPARENT;
        }
        else
        {
            exStyle &= ~WS_EX_TRANSPARENT;
        }

        SetWindowLong(hWnd, GWL_EXSTYLE, exStyle);
        mousePassThroughApplied = (exStyle & WS_EX_TRANSPARENT) != 0;

        if (transparent)
        {
            ApplyDwmFrameExtension(hWnd);

            if (useColorKeyTransparency)
            {
                SetLayeredWindowAttributes(hWnd, ColorToColorRef(transparentColorKey), 0, LWA_COLORKEY);
                LogDebug("已应用色键透明：" + ColorUtility.ToHtmlStringRGB(transparentColorKey));
            }
            else if (clickThrough)
            {
                SetLayeredWindowAttributes(hWnd, 0, 255, LWA_ALPHA);
                LogDebug("点击穿透模式已保持窗口 Alpha。");
            }
        }

        IntPtr zOrder = alwaysOnTop ? HWND_TOPMOST : HWND_NOTOPMOST;
        SetWindowPos(hWnd, zOrder, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
        InstallWindowProcedure(hWnd);
        LogDebug("窗口原生设置已应用。");
#else
        LogDebug("窗口透明/无边框/置顶仅在 Windows 打包后生效。无边框配置：" + borderless + "，DWM配置：" + useDwmFrameExtension);
#endif
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 开始拖动无边框窗口，可绑定到 UI 拖拽区域的 PointerDown 事件。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void StartWindowDrag()
    {
        if (!draggable)
        {
            return;
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (windowHandle == IntPtr.Zero)
        {
            windowHandle = FindUnityWindow();
        }

        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        ReleaseCapture();
        SendMessage(windowHandle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
#else
        LogDebug("窗口拖拽仅在 Windows 打包后生效。");
#endif
    }

    //
    // @param enable 为 true 时窗口置顶。
    // @return 无
    //
    // @summary 动态设置窗口置顶状态。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SetAlwaysOnTop(bool enable)
    {
        alwaysOnTop = enable;
        ApplyNativeWindowSettings();
    }

    //
    // @param enable 为 true 时整窗点击穿透。
    // @return 无
    //
    // @summary 动态设置窗口点击穿透状态。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SetClickThrough(bool enable)
    {
        clickThrough = enable;
        ApplyNativeWindowSettings();
    }

    //
    // @param enable 为 true 时只让透明背景区域点击穿透。
    // @return 无
    //
    // @summary 动态设置透明区域点击穿透状态，保留 UI 元素的正常点击能力。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    public void SetTransparentAreaClickThrough(bool enable)
    {
        transparentAreaClickThrough = enable;
        cursorOverSolidArea = true;
        ApplyNativeWindowSettings();
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 根据鼠标位置缓存当前是否位于指定 Panel 或其子物体区域。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void UpdateTransparentAreaHitState()
    {
        if (!transparentAreaClickThrough || clickThrough || !transparent)
        {
            cursorOverSolidArea = true;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!clickThrough)
            {
                SetMousePassThrough(false);
            }
#endif
            return;
        }

        if (Time.unscaledTime < nextHitTestTime)
        {
            return;
        }

        nextHitTestTime = Time.unscaledTime + Mathf.Max(0.01f, hitTestRefreshInterval);
        Vector2 screenPosition = GetCurrentPointerScreenPosition();
        bool isOverSolidArea = IsPointerOverSolidArea(screenPosition);
        cursorOverSolidArea = isOverSolidArea;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        SetMousePassThrough(!isOverSolidArea);
#endif
    }

    //
    // @param 无
    // @return Vector2 当前鼠标在 Unity 窗口客户区内的屏幕坐标。
    //
    // @summary 获取当前鼠标坐标，Windows 构建中直接读取系统鼠标位置以便失焦后仍能恢复 Panel 点击能力。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private Vector2 GetCurrentPointerScreenPosition()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (windowHandle == IntPtr.Zero)
        {
            windowHandle = FindUnityWindow();
        }

        if (TryGetClientMousePosition(windowHandle, out Vector2 screenPosition))
        {
            return screenPosition;
        }
#endif
        return Input.mousePosition;
    }

    //
    // @param screenPosition 鼠标屏幕坐标。
    // @return bool 鼠标位于指定 Panel 或其子物体区域时返回 true。
    //
    // @summary 只使用 WindowSettings 配置的 Panel 白名单判断窗口当前位置是否应该接收点击，其他区域默认穿透。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private bool IsPointerOverSolidArea(Vector2 screenPosition)
    {
        if (panels == null)
        {
            return false;
        }

        for (int i = 0; i < panels.Length; i++)
        {
            RectTransform panel = panels[i];
            if (IsPointerOverRectTransformOrChildren(panel, screenPosition))
            {
                return true;
            }
        }

        return false;
    }

    //
    // @param rectTransform 待检测的 Panel 或子物体 RectTransform。
    // @param screenPosition 鼠标屏幕坐标。
    // @return bool 鼠标位于当前 RectTransform 或任意子 RectTransform 范围内时返回 true。
    //
    // @summary 递归检测 Panel 白名单和其子物体，只有这些区域会阻挡透明区域点击穿透。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private bool IsPointerOverRectTransformOrChildren(RectTransform rectTransform, Vector2 screenPosition)
    {
        if (rectTransform == null || !rectTransform.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition, uiEventCamera))
        {
            return true;
        }

        for (int i = 0; i < rectTransform.childCount; i++)
        {
            RectTransform childRectTransform = rectTransform.GetChild(i) as RectTransform;
            if (IsPointerOverRectTransformOrChildren(childRectTransform, screenPosition))
            {
                return true;
            }
        }

        return false;
    }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    //
    // @param hWnd Windows 窗口句柄。
    // @return 无
    //
    // @summary 扩展 DWM 客户区透明效果，辅助色键透明在 Unity 独立窗口中生效。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void ApplyDwmFrameExtension(IntPtr hWnd)
    {
        if (!useDwmFrameExtension)
        {
            return;
        }

        Margins margins = new Margins
        {
            Left = -1,
            Right = -1,
            Top = -1,
            Bottom = -1
        };

        int result = DwmExtendFrameIntoClientArea(hWnd, ref margins);
        LogDebug("DWM 客户区透明扩展结果：" + result);
    }

    //
    // @param hWnd Windows 窗口句柄。
    // @return 无
    //
    // @summary 安装窗口消息回调，在透明区域返回 HTTRANSPARENT 让点击落到下层窗口。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void InstallWindowProcedure(IntPtr hWnd)
    {
        if (!transparentAreaClickThrough || hWnd == IntPtr.Zero)
        {
            return;
        }

        if (procedureWindowHandle == hWnd && originalWindowProcedure != IntPtr.Zero)
        {
            return;
        }

        RestoreWindowProcedure();

        if (windowProcedure == null)
        {
            windowProcedure = HandleWindowMessage;
        }

        IntPtr procedurePointer = Marshal.GetFunctionPointerForDelegate(windowProcedure);
        originalWindowProcedure = SetWindowLongPointer(hWnd, GWL_WNDPROC, procedurePointer);
        procedureWindowHandle = hWnd;

        LogDebug(originalWindowProcedure == IntPtr.Zero ? "窗口消息回调安装失败。" : "透明区域点击穿透已启用。");
    }

    //
    // @param 无
    // @return 无
    //
    // @summary 还原原始窗口消息回调，避免组件禁用或窗口重建后保留旧委托。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void RestoreWindowProcedure()
    {
        if (procedureWindowHandle == IntPtr.Zero || originalWindowProcedure == IntPtr.Zero)
        {
            procedureWindowHandle = IntPtr.Zero;
            originalWindowProcedure = IntPtr.Zero;
            return;
        }

        SetWindowLongPointer(procedureWindowHandle, GWL_WNDPROC, originalWindowProcedure);
        procedureWindowHandle = IntPtr.Zero;
        originalWindowProcedure = IntPtr.Zero;
    }

    //
    // @param hWnd Windows 窗口句柄。
    // @param msg Windows 消息 ID。
    // @param wParam 消息参数。
    // @param lParam 消息参数。
    // @return IntPtr 消息处理结果。
    //
    // @summary 处理窗口命中测试，透明区域返回 HTTRANSPARENT，其他消息交给原始窗口过程。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private IntPtr HandleWindowMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_NCHITTEST && transparentAreaClickThrough && transparent && !clickThrough && ShouldPassThroughMouseMessage(hWnd))
        {
            return new IntPtr(HTTRANSPARENT);
        }

        return originalWindowProcedure == IntPtr.Zero
            ? IntPtr.Zero
            : CallWindowProc(originalWindowProcedure, hWnd, msg, wParam, lParam);
    }

    //
    // @param hWnd Windows 窗口句柄。
    // @return bool 当前鼠标不在 Panel 白名单区域时返回 true。
    //
    // @summary 在窗口命中测试消息中直接计算鼠标位置，避免依赖 Unity 输入缓存导致穿透状态滞后。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private bool ShouldPassThroughMouseMessage(IntPtr hWnd)
    {
        if (!TryGetClientMousePosition(hWnd, out Vector2 screenPosition))
        {
            return !cursorOverSolidArea;
        }

        cursorOverSolidArea = IsPointerOverSolidArea(screenPosition);
        return !cursorOverSolidArea;
    }

    //
    // @param enable 为 true 时窗口鼠标事件穿透到下层应用。
    // @return 无
    //
    // @summary 动态切换 WS_EX_TRANSPARENT，确保 Panel 外区域可以真正点到下层窗口。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void SetMousePassThrough(bool enable)
    {
        if (clickThrough)
        {
            enable = true;
        }

        if (mousePassThroughApplied == enable)
        {
            return;
        }

        if (windowHandle == IntPtr.Zero)
        {
            windowHandle = FindUnityWindow();
        }

        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        long exStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
        if (enable)
        {
            exStyle |= WS_EX_LAYERED;
            exStyle |= WS_EX_TRANSPARENT;
        }
        else
        {
            exStyle &= ~WS_EX_TRANSPARENT;

            if (!useColorKeyTransparency && !clickThrough)
            {
                exStyle &= ~WS_EX_LAYERED;
            }
        }

        SetWindowLong(windowHandle, GWL_EXSTYLE, exStyle);

        if (enable)
        {
            SetLayeredWindowAttributes(windowHandle, 0, 255, LWA_ALPHA);
        }
        else if (transparent)
        {
            ApplyDwmFrameExtension(windowHandle);
        }

        SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
        mousePassThroughApplied = enable;
        LogDebug(enable ? "鼠标已切换为 Panel 外穿透。" : "鼠标已恢复为 Panel 内接收。");
    }

    //
    // @param hWnd Windows 窗口句柄。
    // @param screenPosition 输出 Unity 客户区鼠标坐标。
    // @return bool 成功获取坐标时返回 true。
    //
    // @summary 从 Windows 系统鼠标位置转换为 Unity 客户区坐标，支持窗口失焦后继续判断 Panel 区域。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private static bool TryGetClientMousePosition(IntPtr hWnd, out Vector2 screenPosition)
    {
        screenPosition = Vector2.zero;

        if (hWnd == IntPtr.Zero || !GetCursorPos(out WinPoint point))
        {
            return false;
        }

        if (!ScreenToClient(hWnd, ref point))
        {
            return false;
        }

        screenPosition = new Vector2(point.X, Screen.height - point.Y);
        return true;
    }

    //
    // @param 无
    // @return IntPtr Unity 当前主窗口句柄。
    //
    // @summary 按当前进程查找 Unity 主窗口，避免修改到其他程序窗口。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private IntPtr FindUnityWindow()
    {
        uint currentProcessId = GetCurrentProcessId();

        if (windowHandle != IntPtr.Zero && IsWindowBelongsToProcess(windowHandle, currentProcessId))
        {
            return windowHandle;
        }

        IntPtr activeWindow = GetActiveWindow();
        if (activeWindow != IntPtr.Zero && IsWindowBelongsToProcess(activeWindow, currentProcessId))
        {
            return activeWindow;
        }

        IntPtr foundWindow = IntPtr.Zero;
        EnumWindows((hWnd, lParam) =>
        {
            if (!IsWindowVisible(hWnd) || !IsWindowBelongsToProcess(hWnd, currentProcessId))
            {
                return true;
            }

            StringBuilder titleBuilder = new StringBuilder(256);
            GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);
            if (titleBuilder.Length > 0)
            {
                foundWindow = hWnd;
                return false;
            }

            return true;
        }, IntPtr.Zero);

        if (foundWindow != IntPtr.Zero)
        {
            return foundWindow;
        }

        IntPtr foregroundWindow = GetForegroundWindow();
        if (foregroundWindow != IntPtr.Zero && IsWindowBelongsToProcess(foregroundWindow, currentProcessId))
        {
            return foregroundWindow;
        }

        return IntPtr.Zero;
    }

    //
    // @param hWnd 待验证的窗口句柄。
    // @param processId 当前进程 ID。
    // @return bool 窗口属于当前进程时返回 true。
    //
    // @summary 防止窗口 API 修改到其他进程窗口。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private static bool IsWindowBelongsToProcess(IntPtr hWnd, uint processId)
    {
        GetWindowThreadProcessId(hWnd, out uint windowProcessId);
        return windowProcessId == processId;
    }

    //
    // @param hWnd Windows 窗口句柄。
    // @param index 窗口样式索引。
    // @return long 窗口样式值。
    //
    // @summary 根据进程位数读取窗口样式。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private static long GetWindowLong(IntPtr hWnd, int index)
    {
        return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, index).ToInt64() : GetWindowLongPtr32(hWnd, index).ToInt64();
    }

    //
    // @param hWnd Windows 窗口句柄。
    // @param index 窗口样式索引。
    // @param value 新窗口样式值。
    // @return 无
    //
    // @summary 根据进程位数写入窗口样式。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private static void SetWindowLong(IntPtr hWnd, int index, long value)
    {
        if (IntPtr.Size == 8)
        {
            SetWindowLongPtr64(hWnd, index, new IntPtr(value));
        }
        else
        {
            SetWindowLongPtr32(hWnd, index, new IntPtr(value));
        }
    }

    //
    // @param hWnd Windows 窗口句柄。
    // @param index 窗口属性索引。
    // @param value 新窗口属性指针。
    // @return IntPtr 原窗口属性指针。
    //
    // @summary 根据进程位数写入窗口指针属性，例如窗口消息回调。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private static IntPtr SetWindowLongPointer(IntPtr hWnd, int index, IntPtr value)
    {
        return IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, index, value) : SetWindowLongPtr32(hWnd, index, value);
    }

    //
    // @param color Unity 颜色。
    // @return uint Windows COLORREF 颜色值。
    //
    // @summary 把 Unity Color 转换为 Windows 色键格式。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private static uint ColorToColorRef(Color color)
    {
        uint r = (uint)Mathf.RoundToInt(Mathf.Clamp01(color.r) * 255f);
        uint g = (uint)Mathf.RoundToInt(Mathf.Clamp01(color.g) * 255f);
        uint b = (uint)Mathf.RoundToInt(Mathf.Clamp01(color.b) * 255f);
        return r | (g << 8) | (b << 16);
    }
#endif

    //
    // @param message 调试日志内容。
    // @return 无
    //
    // @summary 在开启调试日志时输出窗口设置状态。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log("[WindowSettings] " + message);
        }
    }

    //
    // @param 无
    // @return bool 当前运行环境支持原生窗口透明时返回 true。
    //
    // @summary 判断是否应启用 Windows 原生透明色键，避免 Unity Editor 中直接显示洋红色背景。
    // @checked: false. not reviewed by human, 07/06/2026.
    //
    private static bool ShouldUseNativeTransparency()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        return true;
#else
        return false;
#endif
    }
}
