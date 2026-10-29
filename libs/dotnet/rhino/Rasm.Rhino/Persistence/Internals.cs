using System.Drawing;
using System.Runtime.CompilerServices;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Runtime;
using Rhino.UI.Theme;

namespace Rasm.Rhino.Persistence;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Accessors {
    // --- [OWNERS]
    private const string TabPanelSettings = "Rhino.UI.Internal.TabPanels.TabPanelSettings, Rhino.UI";
    private const string IToolbarSettings = "Rhino.UI.Internal.TabPanels.IToolbarSettings, Rhino.UI";
    private const string ToolbarButtonSettings = "Rhino.UI.Internal.TabPanels.ToolbarButtonSettings, Rhino.UI";
    private const string ToolbarIntSetting = "Rhino.UI.Internal.TabPanels.ToolbarIntSetting, Rhino.UI";
    private const string TabPanelDockSites = "Rhino.UI.Internal.TabPanels.TabPanelDockSites, Rhino.UI";
    private const string ThemeSettings = "Rhino.UI.ThemeSettings, Rhino.UI";
    private const string RuntimeSettings = "Rhino.UI.Runtime.Settings, Rhino.UI";

    // --- [SCOPE]
    internal static IO<T> Scoped<T>(
        Func<T> call, [CallerArgumentExpression(nameof(call))] string member = "", [CallerFilePath] string file = "", [CallerMemberName] string caller = "", [CallerLineNumber] int line = 0) =>
        (from risky in use(() => new RiskyAction(member, file, caller, line))
         from value in IO.lift(call)
         select value).Bracket();

    internal static IO<Unit> Scoped(
        Action call, [CallerArgumentExpression(nameof(call))] string member = "", [CallerFilePath] string file = "", [CallerMemberName] string caller = "", [CallerLineNumber] int line = 0) =>
        (from risky in use(() => new RiskyAction(member, file, caller, line))
         from value in IO.lift(call)
         select value).Bracket();

    // --- [TAB_PANELS]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_TabIconSize")]
    internal static extern int TabIconSize([UnsafeAccessorType(TabPanelSettings)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "set_TabIconSize")]
    internal static extern void TabIconSize([UnsafeAccessorType(TabPanelSettings)] object? owner, int value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_ToolBarImageSize")]
    internal static extern int ToolBarImageSize([UnsafeAccessorType(TabPanelSettings)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "set_ToolBarImageSize")]
    internal static extern void ToolBarImageSize([UnsafeAccessorType(TabPanelSettings)] object? owner, int value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern void add_TabIconSizeChanged([UnsafeAccessorType(TabPanelSettings)] object? owner, EventHandler value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern void remove_TabIconSizeChanged([UnsafeAccessorType(TabPanelSettings)] object? owner, EventHandler value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern void add_ToolBarImageSizeChanged([UnsafeAccessorType(TabPanelSettings)] object? owner, EventHandler value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern void remove_ToolBarImageSizeChanged([UnsafeAccessorType(TabPanelSettings)] object? owner, EventHandler value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_ItemPadding")]
    internal static extern Eto.Drawing.Size ItemPadding([UnsafeAccessorType("Rhino.UI.Internal.TabPanels.Controls.BaseTabControlItem, Rhino.UI")] object? owner);

    // --- [STRIPS]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "CurrentIconSize")]
    internal static extern int OSnapIconSize([UnsafeAccessorType("Rhino.UI.DialogPanels.OSnapPanel, Rhino.UI")] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "CurrentIconSize")]
    internal static extern int SelectionFilterIconSize([UnsafeAccessorType("Rhino.UI.DialogPanels.SelectionFilterUi, Rhino.UI")] object? owner);

    // --- [TOOLBARS]
    internal static object ToolbarButtons => Buttons(Instance(owner: null));

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_Instance")]
    [return: UnsafeAccessorType(IToolbarSettings)]
    internal static extern object Instance([UnsafeAccessorType("Rhino.UI.Internal.TabPanels.ToolbarSettings, Rhino.UI")] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Buttons")]
    [return: UnsafeAccessorType(ToolbarButtonSettings)]
    internal static extern object Buttons([UnsafeAccessorType(IToolbarSettings)] object settings);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_ButtonSize")]
    internal static extern void ButtonSize([UnsafeAccessorType(ToolbarButtonSettings)] object buttons, int value);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    internal static extern int Get([UnsafeAccessorType(ToolbarIntSetting)] object setting);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    internal static extern void Set([UnsafeAccessorType(ToolbarIntSetting)] object setting, int value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PrivateButtonPadding")]
    [return: UnsafeAccessorType(ToolbarIntSetting)]
    internal static extern object PrivateButtonPadding([UnsafeAccessorType(ToolbarButtonSettings)] object buttons);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PrivatePanelButtonSize")]
    [return: UnsafeAccessorType(ToolbarIntSetting)]
    internal static extern object PrivatePanelButtonSize([UnsafeAccessorType(ToolbarButtonSettings)] object buttons);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    internal static extern void add_SettingChanged([UnsafeAccessorType(ToolbarButtonSettings)] object buttons, EventHandler<string> value);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    internal static extern void remove_SettingChanged([UnsafeAccessorType(ToolbarButtonSettings)] object buttons, EventHandler<string> value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern void RefreshIcons([UnsafeAccessorType("Rhino.UI.Internal.TabPanels.OldCodeExtensions, Rhino.UI")] object? owner);

    // --- [THEME]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern Color? GetPaintColorHook([UnsafeAccessorType(ThemeSettings)] object? owner, PaintColor paintColor);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_SettingId")]
    internal static extern string SettingId(ThemeBase zone);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern string Combine(ThemeBase? owner, string parentId, string childId);

    // --- [EDIT_DELAYS]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern double get_TextChangingEventsDelay([UnsafeAccessorType(RuntimeSettings)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern double get_StepEventsDelay([UnsafeAccessorType(RuntimeSettings)] object? owner);

    // --- [DOCK_SITES]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    [return: UnsafeAccessorType(TabPanelDockSites)]
    internal static extern object? FromDocument([UnsafeAccessorType(TabPanelDockSites)] object? owner, RhinoDoc doc);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_StatusBar")]
    [return: UnsafeAccessorType("Rhino.UI.Internal.TabPanels.Controls.StatusBar, Rhino.UI")]
    internal static extern object StatusBar([UnsafeAccessorType(TabPanelDockSites)] object sites);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_ResizerWidth")]
    internal static extern int ResizerWidth([UnsafeAccessorType("Rhino.UI.Internal.TabPanels.Controls.DockSiteResizer, Rhino.UI")] object? owner);

    // --- [SAVES]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern void add_SettingsSaved([UnsafeAccessorType("Rhino.RhinoApp, RhinoCommon")] object? owner, EventHandler<PersistentSettingsSavedEventArgs> value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern void remove_SettingsSaved([UnsafeAccessorType("Rhino.RhinoApp, RhinoCommon")] object? owner, EventHandler<PersistentSettingsSavedEventArgs> value);
}
