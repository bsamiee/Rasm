using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.Persistence.Stores;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Runtime;
using Rhino.UI;
using Rhino.UI.Theme;

namespace Rasm.RhinoCommon.Persistence.Settings;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Accessors {
    // --- [OWNERS]
    private const string TabPanelSettings = "Rhino.UI.Internal.TabPanels.TabPanelSettings, Rhino.UI";
    private const string IToolbarSettings = "Rhino.UI.Internal.TabPanels.IToolbarSettings, Rhino.UI";
    private const string ToolbarButtonSettings = "Rhino.UI.Internal.TabPanels.ToolbarButtonSettings, Rhino.UI";
    private const string ToolbarIntSetting = "Rhino.UI.Internal.TabPanels.ToolbarIntSetting, Rhino.UI";
    private const string TabPanelDockSites = "Rhino.UI.Internal.TabPanels.TabPanelDockSites, Rhino.UI";
    private const string CommandPanels = "Rhino.UI.CommandPanels, RhinoCommon";
    private const string UnsafeNativeMethods = "UnsafeNativeMethods, RhinoCommon";
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

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_DefaultTabIconSize")]
    internal static extern int DefaultTabIconSize([UnsafeAccessorType(TabPanelSettings)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_ToolBarImageSize")]
    internal static extern int ToolBarImageSize([UnsafeAccessorType(TabPanelSettings)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "set_ToolBarImageSize")]
    internal static extern void ToolBarImageSize([UnsafeAccessorType(TabPanelSettings)] object? owner, int value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_DefaultToolBarImageSize")]
    internal static extern int DefaultToolBarImageSize([UnsafeAccessorType(TabPanelSettings)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_MinimumToolBarImageSize")]
    internal static extern int MinimumToolBarImageSize([UnsafeAccessorType(TabPanelSettings)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_MaximumToolBarImageSize")]
    internal static extern int MaximumToolBarImageSize([UnsafeAccessorType(TabPanelSettings)] object? owner);

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

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Min")]
    internal static extern int Min([UnsafeAccessorType(ToolbarIntSetting)] object setting);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Max")]
    internal static extern int Max([UnsafeAccessorType(ToolbarIntSetting)] object setting);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_DefaultValue")]
    internal static extern int DefaultValue([UnsafeAccessorType(ToolbarIntSetting)] object setting);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    internal static extern void add_SettingChanged([UnsafeAccessorType(ToolbarButtonSettings)] object buttons, EventHandler<string> value);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    internal static extern void remove_SettingChanged([UnsafeAccessorType(ToolbarButtonSettings)] object buttons, EventHandler<string> value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern void RefreshIcons([UnsafeAccessorType("Rhino.UI.Internal.TabPanels.OldCodeExtensions, Rhino.UI")] object? owner);

    // --- [THEME]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_Settings")]
    internal static extern PersistentSettings Settings([UnsafeAccessorType("Rhino.UI.ThemeSettings, Rhino.UI")] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern Color? GetPaintColorHook([UnsafeAccessorType("Rhino.UI.ThemeSettings, Rhino.UI")] object? owner, PaintColor paintColor);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_SettingId")]
    internal static extern string SettingId(ThemeBase zone);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern string Combine(ThemeBase? owner, string parentId, string childId);

    // --- [PROMPT]
    [UnsafeAccessor(UnsafeAccessorKind.StaticField)]
    internal static extern ref EventHandler? PromptLocationChanged([UnsafeAccessorType(CommandPanels)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticField)]
    internal static extern ref EventHandler? PromptStyleChanged([UnsafeAccessorType(CommandPanels)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern bool CRhinoAppSettings_GetAutocompleteCommands([UnsafeAccessorType(UnsafeNativeMethods)] object? owner, bool defaultValue);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern void CRhinoAppSettings_SetAutocompleteCommands([UnsafeAccessorType(UnsafeNativeMethods)] object? owner, bool enable);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern bool CRhinoAppSettings_GetFuzzyAutocomplete([UnsafeAccessorType(UnsafeNativeMethods)] object? owner, bool defaultValue);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern void CRhinoAppSettings_SetFuzzyAutocomplete([UnsafeAccessorType(UnsafeNativeMethods)] object? owner, bool enable);

    // --- [EDIT_DELAYS]
    internal static IO<double> TextChangingEventsDelay => Scoped(static () => get_TextChangingEventsDelay(owner: null));

    internal static IO<double> StepEventsDelay => Scoped(static () => get_StepEventsDelay(owner: null));

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
}

public static class AppearanceOptions {
    // --- [CHANGES]
    private static readonly HostEvent<EventArgs> PromptStyleChanged = Subscriptions.Host<EventHandler, EventArgs>(
        static handler => Accessors.PromptStyleChanged(owner: null) += handler,
        static handler => Accessors.PromptStyleChanged(owner: null) -= handler,
        static deliver => deliver.Invoke);

    private static readonly HostEvent<EventArgs> TabIconSizeChanged = Subscriptions.Host<EventHandler, EventArgs>(
        static handler => Accessors.add_TabIconSizeChanged(owner: null, handler),
        static handler => Accessors.remove_TabIconSizeChanged(owner: null, handler),
        static deliver => deliver.Invoke);

    private static readonly HostEvent<EventArgs> ToolBarImageSizeChanged = Subscriptions.Host<EventHandler, EventArgs>(
        static handler => Accessors.add_ToolBarImageSizeChanged(owner: null, handler),
        static handler => Accessors.remove_ToolBarImageSizeChanged(owner: null, handler),
        static deliver => deliver.Invoke);

    private static readonly HostEvent<string> SettingChanged = Subscriptions.Host<string>(
        static handler => Accessors.add_SettingChanged(Accessors.ToolbarButtons, handler),
        static handler => Accessors.remove_SettingChanged(Accessors.ToolbarButtons, handler));

    // --- [ROWS]
    public static ValueStore<bool> BlackWhiteSwitching { get; } =
        Live(
            IO.lift(static () => AppearanceSettings.BlackWhiteSwitching),
            IO.pure(value: false),
            static enabled => IO.lift(() => { AppearanceSettings.BlackWhiteSwitching = enabled; }));

    public static ValueStore<bool> AutocompleteCommands { get; } =
        Native(Accessors.CRhinoAppSettings_GetAutocompleteCommands, Accessors.CRhinoAppSettings_SetAutocompleteCommands);

    public static ValueStore<bool> FuzzyAutocomplete { get; } =
        Native(Accessors.CRhinoAppSettings_GetFuzzyAutocomplete, Accessors.CRhinoAppSettings_SetFuzzyAutocomplete);

    public static ValueStore<T> Member<T>(Func<AppearanceSettingsState, T> project, Action<T> set, Func<T, T, bool> same) where T : notnull =>
        Live(
            IO.lift(() => project(AppearanceSettings.GetCurrentState())),
            IO.lift(() => project(AppearanceSettings.GetDefaultState())),
            value => IO.lift(() => set(value)),
            Some(same));

    private static ValueStore<T> Live<T>(IO<T> read, IO<T> factory, Func<T, IO<Unit>> write, Option<Func<T, T, bool>> same = default) where T : notnull =>
        ValueStores.Defaulted(read, factory, write, same.IfNone(EqualityComparer<T>.Default.Equals), Applied.Live, AppSettings.Changed);

    private static ValueStore<bool> Native(Func<object?, bool, bool> get, Action<object?, bool> set,
        [CallerArgumentExpression(nameof(get))] string getter = "", [CallerArgumentExpression(nameof(set))] string setter = "") =>
        Live(Accessors.Scoped(() => get(null, false), getter), Accessors.Scoped(() => get(null, true), getter), enabled => Accessors.Scoped(() => set(null, enabled), setter));

    // --- [MODES]
    public static IO<Unit> SetMode(bool darkMode) =>
        IO.lift(() => darkMode ? RhinoFault.Unless(AppearanceSettings.SetToDarkMode()) : RhinoFault.Unless(AppearanceSettings.SetToLightMode()));

    // --- [COLORS]
    public static IO<Unit> SetPaint(PaintColor slot, Option<Color> color) =>
        from hook in Accessors.Scoped(() => Accessors.GetPaintColorHook(owner: null, slot))
        from writable in IO.lift(hook is null ? Fin.Succ(unit) : new RhinoFault(slot.ToString(), RhinoReason.Taken))
        from written in IO.lift(() => AppearanceSettings.SetPaintColor(slot, color.IfNone(() => AppearanceSettings.DefaultPaintColor(slot, HostUtils.RunningInDarkMode))))
        from saved in Applied.IdleSave.Drain.Traverse(static drain => drain).As()
        select unit;

    public static ValueStore<Color> Widget(WidgetColor slot) =>
        Live(
            IO.lift(() => AppearanceSettings.GetWidgetColor(slot)),
            IO.lift(() => AppearanceSettings.DefaultWidgetColor(slot)),
            color => IO.lift(() => AppearanceSettings.SetWidgetColor(slot, color, forceUiUpdate: true)),
            Some(Conversions.Same));

    // --- [THEME]
    public static IO<Seq<string>> ThemeKeys() =>
        Accessors.Scoped(static () => (
            from zone in Seq<ThemeZone>(ThemeSettings.Frame, ThemeSettings.Content)
            from entry in toSeq(zone.Enumerate())
            where entry.Value is Eto.Drawing.Color
            select Accessors.Combine(owner: null, Accessors.SettingId(zone), entry.Id)).Strict());

    public static IO<ValueStore<Color>> Theme(string key) =>
        from keys in ThemeKeys()
        from known in IO.lift(keys.Exists(entry => string.Equals(entry, key, StringComparison.Ordinal)) ? Fin.Succ(unit) : new RhinoFault(key, RhinoReason.Missing))
        from node in Accessors.Scoped(static () => Accessors.Settings(owner: null))
        select ValueStores.Of(
            IO.lift(() => Conversions.Found(node.TryGetColor(key, out Color held), held)),
            color =>
                from writable in IO.lift(() => PlugInSettings.Writable(node, key))
                from written in IO.lift(() => color.Match(Some: held => node.SetColor(key, held), None: () => node.DeleteItem(key)))
                select unit,
            Applied.IdleSave,
            Some(ValueStores.Signal(EventKind.ThemeChanged, static _ => true)), Some(Conversions.Same));

    // --- [PROMPT]
    private const string GetPresentationStyle = "Rhino.UI.Internal.DockBars.CommandLine.GetPresentationStyle";
    private const string SetPresentationStyle = "Rhino.UI.Internal.DockBars.CommandLine.SetPresentationStyle";
    private const string ModeEntry = "mode";
    private const string LocationKey = "CommandPromptLocation";
    private const string PresentationKey = "CommandOptionsPresentationStyle";
    private const string StyleName = "CommandPromptStyle";

    public static Type PromptLocationType { get; } = Type.GetType("Rhino.UI.CommandPromptLocation, RhinoCommon", throwOnError: true)!;

    public static Type PromptStyleType { get; } = Type.GetType("Rhino.UI.CommandPromptStyle, RhinoCommon", throwOnError: true)!;

    public static IO<Enum> PromptLocation { get; } =
        from location in NamedCallbacks.Execute(GetPresentationStyle, static args => Conversions.Found(args.TryGetInt(ModeEntry, out int mode), (Enum)Enum.ToObject(PromptLocationType, mode)))
        from valid in IO.lift(RhinoFault.Unless(Enum.IsDefined(PromptLocationType, location)).Map(_ => location))
        select valid;

    public static IO<Unit> SetPromptLocation(Option<Enum> location) =>
        from validated in IO.lift(() => location.Traverse(static K<Fin, Enum> (held) => Defined(PromptLocationType, held)).As())
        from options in SettingRoots.TryGetChild(SettingsNode.Options)
        let keys = Seq(LocationKey, PresentationKey)
        let existing = from node in options.ToSeq() from key in keys select (Node: node, Key: key)
        from writable in IO.lift(() => Callbacks.Each(existing, static (entry, _) => PlugInSettings.Writable(entry.Node, entry.Key)))
        let removals = validated.IsSome ? Seq(LocationKey) : keys
        let deleted = from node in options.ToSeq() from key in removals select (Node: node, Key: key)
        from cleared in IO.lift(() => deleted.Iter(static entry => entry.Node.DeleteItem(entry.Key)))
        from shown in validated.Match(
            Some: static held => NamedCallbacks.Execute(SetPresentationStyle, NamedCallbacks.Handled, args => args.Set(ModeEntry, Convert.ToInt32(held, CultureInfo.InvariantCulture))),
            None: static () => Accessors.Scoped(static () => Accessors.PromptLocationChanged(owner: null)?.Invoke(sender: null, EventArgs.Empty)))
        select shown;

    public static ValueStore<Enum> PromptStyle { get; } =
        ValueStores.Of(
            from found in SettingRoots.TryGetChild(SettingsNode.Options)
            from text in IO.lift(() => found.Bind(static node => node.TryGetString(StyleName, out string? held) ? Optional(held) : None))
            from style in IO.lift(() => text.Traverse(static K<Fin, Enum> (held) =>
                Enum.TryParse(PromptStyleType, held, ignoreCase: false, out object? parsed) ? Defined(PromptStyleType, (Enum)parsed) : new RhinoFault(held, RhinoReason.Unreadable)).As())
            select style,
            static style =>
                from validated in IO.lift(() => style.Traverse(static K<Fin, Enum> (held) => Defined(PromptStyleType, held)).As())
                from node in SettingRoots.AddChild(SettingsNode.Options)
                from writable in IO.lift(() => PlugInSettings.Writable(node, StyleName))
                from written in IO.lift(() => validated.Match(Some: held => node.SetString(StyleName, held.ToString()), None: () => node.DeleteItem(StyleName)))
                from raised in Accessors.Scoped(static () => Accessors.PromptStyleChanged(owner: null)?.Invoke(sender: null, EventArgs.Empty))
                select raised,
            Applied.Live,
            Some(ValueStores.Signal(PromptStyleChanged, static _ => true)));

    private static Fin<Enum> Defined(Type type, Enum held) =>
        held.GetType() != type ? new RhinoFault(held.ToString(), RhinoReason.Mismatched)
        : Enum.IsDefined(type, held) ? held
        : new RhinoFault(held.ToString(), RhinoReason.Outside);

    // --- [ICONS]
    private static readonly IO<(int Minimum, int Maximum)> ImageLimits =
        Accessors.Scoped(static () => (Accessors.MinimumToolBarImageSize(owner: null), Accessors.MaximumToolBarImageSize(owner: null)));

    public static ValueStore<int> TabIconSize { get; } =
        Sized(
            Accessors.Scoped(static () => Accessors.TabIconSize(owner: null)),
            Accessors.Scoped(static () => Accessors.DefaultTabIconSize(owner: null)),
            ImageLimits,
            static size => Accessors.Scoped(() => Accessors.TabIconSize(owner: null, size)),
            Applied.IdleSave,
            TabIconSizeChanged);

    public static ValueStore<int> ToolBarImageSize { get; } =
        Sized(
            Accessors.Scoped(static () => Accessors.ToolBarImageSize(owner: null)),
            Accessors.Scoped(static () => Accessors.DefaultToolBarImageSize(owner: null)),
            ImageLimits,
            static size => Accessors.Scoped(() => Accessors.ToolBarImageSize(owner: null, size))
                >> Accessors.Scoped(() => Accessors.ButtonSize(Accessors.ToolbarButtons, size))
                >> Accessors.Scoped(static () => Accessors.RefreshIcons(owner: null)),
            Applied.IdleSave,
            ToolBarImageSizeChanged);

    public static ValueStore<int> ButtonPadding { get; } =
        ToolbarSize(static () => Accessors.PrivateButtonPadding(Accessors.ToolbarButtons));

    public static ValueStore<int> PanelButtonSize { get; } =
        ToolbarSize(static () => Accessors.PrivatePanelButtonSize(Accessors.ToolbarButtons));

    public static ValueStore<int> OSnapIconSize { get; } =
        Strip("OSnapIconSize", Accessors.Scoped(static () => Accessors.OSnapIconSize(owner: null)));

    public static ValueStore<int> SelectionFilterIconSize { get; } =
        Strip("SelectionFilterIconSize", Accessors.Scoped(static () => Accessors.SelectionFilterIconSize(owner: null)));

    private static ValueStore<int> Sized<TArgs>(IO<int> current, IO<int> factory, IO<(int Minimum, int Maximum)> limits, Func<int, IO<Unit>> set, Applied applied, HostEvent<TArgs> changed) =>
        ValueStores.Defaulted(
            current,
            factory,
            size =>
                from range in limits
                from valid in IO.lift(size >= range.Minimum && size <= range.Maximum
                    ? Fin.Succ(unit)
                    : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"{size} in [{range.Minimum}, {range.Maximum}]"), RhinoReason.Outside))
                from written in set(size)
                select written,
            EqualityComparer<int>.Default.Equals,
            applied,
            Some(ValueStores.Signal(changed, static _ => true)));

    private static ValueStore<int> ToolbarSize(Func<object> setting) =>
        Sized(
            Accessors.Scoped(() => Accessors.Get(setting())),
            Accessors.Scoped(() => Accessors.DefaultValue(setting())),
            Accessors.Scoped(() => setting() switch { var held => (Accessors.Min(held), Accessors.Max(held)) }),
            size => Accessors.Scoped(() => Accessors.Set(setting(), size)),
            Applied.Live,
            SettingChanged);

    private static ValueStore<int> Strip(string name, IO<int> current) =>
        ValueStores.Defaulted(
            current,
            from registered in current
            from node in SettingRoots.AddChild(SettingsNode.Application)
            from size in IO.lift(() => RhinoFault.Unless(node.TryGetDefault(name, out int size)).Map(_ => size))
            select size,
            size =>
                from node in SettingRoots.AddChild(SettingsNode.Application)
                from writable in IO.lift(() => PlugInSettings.Writable(node, name))
                from written in IO.lift(() => node.SetInteger(name, size))
                from clamped in current
                select unit,
            EqualityComparer<int>.Default.Equals,
            Applied.IdleSave,
            Some(SettingRoots.Saved(SettingsNode.Application)));

    // --- [WINDOW]
    public static IO<Option<Rectangle>> InitialMainWindowPosition() =>
        IO.lift(static () => Conversions.Found(AppearanceSettings.InitialMainWindowPosition(out Rectangle bounds), bounds));

    // --- [POLICY]
    public static IO<int> HistoryBand(RhinoDoc doc, AppearanceSettingsState settings, Option<int> promptRow, int stack) =>
        from pitch in (from font in use(() => new Eto.Drawing.Font(settings.CommandPromptFontName, settings.CommandPromptFontSize / 10f)) select (int)font.LineHeight).Bracket()
        from found in Accessors.Scoped(() => Conversions.Found(Accessors.FromDocument(owner: null, doc)))
        from sites in IO.lift(found)
        from chrome in Accessors.Scoped(() => ((Eto.Forms.Control)Accessors.StatusBar(sites)).Height + Accessors.ResizerWidth(owner: null))
        let addend = promptRow.Match(Some: static row => row + 1, None: static () => 0)
        select ((stack - chrome - addend) / pitch * pitch) + addend;

    public static IO<Unit> IconCategory(int stripHeight, int glyph, int row) =>
        from even in IO.lift(int.IsEvenInteger(row - glyph) ? Fin.Succ(unit) : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"{row} - {glyph}"), RhinoReason.Outside))
        from inset in Accessors.Scoped(static () => Accessors.ItemPadding(owner: null).Height)
        let tab = stripHeight - (2 * inset)
        from committed in ValueStores.Commit(Seq(
            ButtonPadding.Edit(Some((row - glyph) / 2)),
            ToolBarImageSize.Edit(Some(glyph)),
            TabIconSize.Edit(Some(tab)),
            PanelButtonSize.Edit(Some(tab)),
            OSnapIconSize.Edit(Some(tab)),
            SelectionFilterIconSize.Edit(Some(tab))))
        select committed;
}
