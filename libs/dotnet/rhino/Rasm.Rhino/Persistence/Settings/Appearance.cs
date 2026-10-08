using System.Drawing;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Runtime;
using Rhino.UI;
using Rhino.UI.Theme;

namespace Rasm.Rhino.Persistence.Settings;

// --- [TYPES] ---------------------------------------------------------------------------
public enum CommandPromptLocation { SeparatePanel = 0, SideBar = 1, CommandHistory = 2 }

public enum CommandPromptStyle { Links = 0, Graphical = 1, Buttons = 2 }

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct StripIconSize : System.Numerics.IMinMaxValue<StripIconSize> {
    public static StripIconSize MinValue { get; } = new(16);
    public static StripIconSize MaxValue { get; } = new(32);
    public static StripIconSize Default { get; } = new(18);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value == int.Clamp(value, MinValue._value, MaxValue._value) ? null : new InvalidRhinoValue();
}

[ValueObject<int>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct PromptFontHeight : System.Numerics.IMinMaxValue<PromptFontHeight> {
    public static PromptFontHeight MinValue { get; } = new(60);
    public static PromptFontHeight MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value == int.Clamp(value, MinValue._value, MaxValue._value) ? null : new InvalidRhinoValue();
}

[ValueObject<int>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct HistoryStrip : System.Numerics.IMinMaxValue<HistoryStrip> {
    public static HistoryStrip MinValue { get; } = new(19);
    public static HistoryStrip MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value == int.Clamp(value, MinValue._value, MaxValue._value) ? null : new InvalidRhinoValue();
}

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
    internal static IO<T> Scoped<T>(string member, IO<T> call, [CallerFilePath] string file = "", [CallerMemberName] string caller = "", [CallerLineNumber] int line = 0) =>
        use(() => new RiskyAction(member, file, caller, line)).Bind(_ => call).Bracket();

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

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_ButtonPadding")]
    internal static extern int ButtonPadding([UnsafeAccessorType(ToolbarButtonSettings)] object buttons);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_ButtonPadding")]
    internal static extern void ButtonPadding([UnsafeAccessorType(ToolbarButtonSettings)] object buttons, int value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PanelButtonSize")]
    internal static extern int PanelButtonSize([UnsafeAccessorType(ToolbarButtonSettings)] object buttons);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_PanelButtonSize")]
    internal static extern void PanelButtonSize([UnsafeAccessorType(ToolbarButtonSettings)] object buttons, int value);

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

    internal static (int Minimum, int Maximum) Limits(object setting) => (Min(setting), Max(setting));

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

    // --- [SETTINGS]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern void add_SettingsSaved([UnsafeAccessorType("Rhino.RhinoApp, RhinoCommon")] object? owner, EventHandler<PersistentSettingsSavedEventArgs> value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern void remove_SettingsSaved([UnsafeAccessorType("Rhino.RhinoApp, RhinoCommon")] object? owner, EventHandler<PersistentSettingsSavedEventArgs> value);

    // --- [EDIT_DELAYS]
    internal static IO<double> TextChangingEventsDelay =>
        Scoped(nameof(get_TextChangingEventsDelay), IO.lift(static () => get_TextChangingEventsDelay(owner: null)));

    internal static IO<double> StepEventsDelay =>
        Scoped(nameof(get_StepEventsDelay), IO.lift(static () => get_StepEventsDelay(owner: null)));

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

public static class Appearance {
    // --- [CHANGES]
    private static readonly HostEvent<EventArgs> PromptLocationChanged = Subscriptions.Host<EventHandler, EventArgs>(
        typeof(Accessors),
        static handler => {
            ref EventHandler? field = ref Accessors.PromptLocationChanged(owner: null);
            field += handler;
        },
        static handler => {
            ref EventHandler? field = ref Accessors.PromptLocationChanged(owner: null);
            field -= handler;
        },
        static deliver => deliver.Invoke);

    private static readonly HostEvent<EventArgs> PromptStyleChanged = Subscriptions.Host<EventHandler, EventArgs>(
        typeof(Accessors),
        static handler => {
            ref EventHandler? field = ref Accessors.PromptStyleChanged(owner: null);
            field += handler;
        },
        static handler => {
            ref EventHandler? field = ref Accessors.PromptStyleChanged(owner: null);
            field -= handler;
        },
        static deliver => deliver.Invoke);

    private static readonly HostEvent<EventArgs> TabIconSizeChanged = Subscriptions.Host<EventHandler, EventArgs>(
        typeof(Accessors),
        static handler => Accessors.add_TabIconSizeChanged(owner: null, handler),
        static handler => Accessors.remove_TabIconSizeChanged(owner: null, handler),
        static deliver => deliver.Invoke);

    private static readonly HostEvent<EventArgs> ToolBarImageSizeChanged = Subscriptions.Host<EventHandler, EventArgs>(
        typeof(Accessors),
        static handler => Accessors.add_ToolBarImageSizeChanged(owner: null, handler),
        static handler => Accessors.remove_ToolBarImageSizeChanged(owner: null, handler),
        static deliver => deliver.Invoke);

    private static readonly HostEvent<string> SettingChanged = Subscriptions.Host<string>(
        typeof(Accessors),
        static handler => Accessors.add_SettingChanged(Accessors.ToolbarButtons, handler),
        static handler => Accessors.remove_SettingChanged(Accessors.ToolbarButtons, handler));

    private static readonly HostEvent<PersistentSettingsSavedEventArgs> SettingsSaved = Subscriptions.Host<PersistentSettingsSavedEventArgs>(
        typeof(RhinoApp),
        static handler => Accessors.add_SettingsSaved(owner: null, handler),
        static handler => Accessors.remove_SettingsSaved(owner: null, handler));

    // --- [ROWS]
    public static ValueStore<bool> BlackWhiteSwitching { get; } =
        Row(
            IO.lift(static () => AppearanceSettings.BlackWhiteSwitching),
            IO.pure(value: false),
            static on => IO.lift(() => { AppearanceSettings.BlackWhiteSwitching = on; }),
            EqualityComparer<bool>.Default.Equals,
            Applied.Live,
            EventKind.AppSettingsChanged);

    public static ValueStore<bool> AutocompleteCommands { get; } =
        WithoutFactory(
            Accessors.Scoped(nameof(Accessors.CRhinoAppSettings_GetAutocompleteCommands), IO.lift(static () => Accessors.CRhinoAppSettings_GetAutocompleteCommands(owner: null, defaultValue: false))),
            static on => Accessors.Scoped(nameof(Accessors.CRhinoAppSettings_SetAutocompleteCommands), IO.lift(() => Accessors.CRhinoAppSettings_SetAutocompleteCommands(owner: null, on))),
            nameof(Accessors.CRhinoAppSettings_SetAutocompleteCommands));

    public static ValueStore<bool> FuzzyAutocomplete { get; } =
        WithoutFactory(
            Accessors.Scoped(nameof(Accessors.CRhinoAppSettings_GetFuzzyAutocomplete), IO.lift(static () => Accessors.CRhinoAppSettings_GetFuzzyAutocomplete(owner: null, defaultValue: false))),
            static on => Accessors.Scoped(nameof(Accessors.CRhinoAppSettings_SetFuzzyAutocomplete), IO.lift(() => Accessors.CRhinoAppSettings_SetFuzzyAutocomplete(owner: null, on))),
            nameof(Accessors.CRhinoAppSettings_SetFuzzyAutocomplete));

    public static ValueStore<Color> Member(Func<AppearanceSettingsState, Color> project, Action<Color> set) =>
        Member(project, set, Conversions.Same);

    public static ValueStore<T> Member<T>(Func<AppearanceSettingsState, T> project, Action<T> set) where T : notnull =>
        Member(project, set, EqualityComparer<T>.Default.Equals);

    private static ValueStore<T> Member<T>(Func<AppearanceSettingsState, T> project, Action<T> set, Func<T, T, bool> same) where T : notnull =>
        Row(
            IO.lift(() => project(AppearanceSettings.GetCurrentState())),
            IO.lift(() => project(AppearanceSettings.GetDefaultState(HostUtils.RunningInDarkMode))),
            value => IO.lift(() => set(value)),
            same,
            Applied.Live,
            EventKind.AppSettingsChanged);

    private static ValueStore<T> Row<T, TArgs>(IO<T> current, IO<T> factory, Func<T, IO<Unit>> set, Func<T, T, bool> same, Applied applied, HostEvent<TArgs> changed)
        where T : notnull =>
        new(
            from held in current
            from initial in factory
            select Some(held).Filter(value => !same(value, initial)),
            value => value.Match(Some: set, None: () => factory.Bind(set)),
            same,
            applied,
            Some(ValueStore.Signal(changed, static _ => true)));

    private static ValueStore<T> WithoutFactory<T>(IO<T> current, Func<T, IO<Unit>> set, string setter) where T : notnull =>
        ValueStore.Of(
            current.Map(static held => Some(held)),
            value => value.Match(Some: set, None: () => IO.fail<Unit>(new Missing(setter))),
            Applied.Live,
            Some(ValueStore.Signal(EventKind.AppSettingsChanged, static _ => true)));

    // --- [MODES]
    public static IO<Unit> SetMode(bool darkMode) =>
        IO.lift(() => darkMode
            ? Refused.Unless(AppearanceSettings.SetToDarkMode(), nameof(AppearanceSettings.SetToDarkMode))
            : Refused.Unless(AppearanceSettings.SetToLightMode(), nameof(AppearanceSettings.SetToLightMode)));

    public static IO<bool> UsingModeDefaults(bool darkMode) =>
        IO.lift(() => darkMode ? AppearanceSettings.UsingDefaultDarkModeColors() : AppearanceSettings.UsingDefaultLightModeColors());

    // --- [COLORS]
    public static ValueStore<Color> Paint(PaintColor slot) =>
        Row(
            IO.lift(() => AppearanceSettings.GetPaintColor(slot, compute: false)),
            IO.lift(() => AppearanceSettings.DefaultPaintColor(slot, HostUtils.RunningInDarkMode)),
            color =>
                Accessors.Scoped(nameof(Accessors.GetPaintColorHook), IO.lift(() => ThemedPaintColor.Unless(Accessors.GetPaintColorHook(owner: null, slot) is null, slot)))
                >> IO.lift(() => AppearanceSettings.SetPaintColor(slot, color, forceUiUpdate: false)),
            Conversions.Same,
            Applied.IdleSave,
            EventKind.ThemeChanged);

    public static ValueStore<Color> Widget(WidgetColor slot) =>
        Row(
            IO.lift(() => AppearanceSettings.GetWidgetColor(slot)),
            IO.lift(() => AppearanceSettings.DefaultWidgetColor(slot)),
            color => IO.lift(() => AppearanceSettings.SetWidgetColor(slot, color, forceUiUpdate: true)),
            Conversions.Same,
            Applied.Live,
            EventKind.AppSettingsChanged);

    // --- [THEME]
    private static readonly IO<PersistentSettings> ThemeNode =
        Accessors.Scoped(nameof(Accessors.Settings), IO.lift(static () => Accessors.Settings(owner: null)));

    public static IO<Seq<string>> ThemeKeys() =>
        Accessors.Scoped(nameof(Accessors.SettingId), IO.lift(static () => (
            from zone in Seq<ThemeZone>(ThemeSettings.Frame, ThemeSettings.Content)
            from entry in toSeq(zone.Enumerate())
            where entry.Value is Eto.Drawing.Color
            select Accessors.Combine(owner: null, Accessors.SettingId(zone), entry.Id)).Strict()));

    public static IO<ValueStore<Color>> Theme(string key) =>
        from keys in ThemeKeys()
        from known in IO.lift(UnknownThemeKey.Unless(keys.Exists(entry => string.Equals(entry, key, StringComparison.Ordinal)), key))
        select new ValueStore<Color>(
            ThemeNode.Bind(node => IO.lift(() => SettingType.Color.TryGet(node, key))),
            color => ThemeNode.Bind(node => IO.lift(() => color.Match(Some: held => SettingType.Color.Set(node, key, held), None: () => node.DeleteItem(key)))),
            Conversions.Same,
            Applied.IdleSave,
            Some(ValueStore.Signal(EventKind.ThemeChanged, static _ => true)));

    // --- [PROMPT]
    private const string GetPresentationStyle = "Rhino.UI.Internal.DockBars.CommandLine.GetPresentationStyle";
    private const string SetPresentationStyle = "Rhino.UI.Internal.DockBars.CommandLine.SetPresentationStyle";
    private const string ModeEntry = "mode";
    private const string LocationKey = "CommandPromptLocation";
    private const string PresentationKey = "CommandOptionsPresentationStyle";
    private const string StyleName = "CommandPromptStyle";

    private static readonly ValueStore<CommandPromptStyle> StyleSetting = PlugInSettings.Store(
        SettingsNode.Options,
        new PlugInSetting<CommandPromptStyle, CommandPromptStyle, InvalidRhinoValue>(
            ValueKey.Enum(
                StyleName,
                "Command prompt style",
                "Links, graphical, or buttons command options",
                HostUtils.RunningOnOSX ? CommandPromptStyle.Graphical : CommandPromptStyle.Links),
            SettingType.Enumeration<CommandPromptStyle>(),
            Applied.Live,
            Seq<string>(),
            Hidden: false));

    private static readonly IO<CommandPromptLocation> Location =
        NamedCallbacks.Execute(GetPresentationStyle, static args => Callbacks.Found(args.TryGetInt(ModeEntry, out int mode), (CommandPromptLocation)mode))
            .Bind(static location => IO.lift(InvalidAnswer.Unless(Enum.IsDefined(location), location, GetPresentationStyle)));

    public static ValueStore<CommandPromptLocation> PromptLocation { get; } =
        ValueStore.Of(
            Location.Map(static location => Some(location)),
            static location =>
                Clear(location.Match(Some: static _ => Seq(LocationKey), None: static () => Seq(LocationKey, PresentationKey)))
                >> location.Match(Some: static held => IO.pure(held), None: static () => Location)
                    .Bind(static held => NamedCallbacks.Execute(SetPresentationStyle, NamedCallbacks.Handled, args => args.Set(ModeEntry, (int)held))),
            Applied.Live,
            Some(ValueStore.Signal(PromptLocationChanged, static _ => true)));

    public static ValueStore<CommandPromptStyle> PromptStyle { get; } =
        StyleSetting with {
            Put = static style =>
                style.Match(Some: _ => StyleSetting.Put(style), None: static () => Clear(Seq(StyleName)))
                >> Accessors.Scoped(nameof(Accessors.PromptStyleChanged), IO.lift(static () => Accessors.PromptStyleChanged(owner: null)?.Invoke(sender: null, EventArgs.Empty))),
            Changed = Some(ValueStore.Signal(PromptStyleChanged, static _ => true)),
        };

    private static IO<Unit> Clear(Seq<string> keys) =>
        SettingRoots.TryGetChild(SettingsNode.Options).Bind(options => IO.lift(() => options.Iter(node => keys.Iter(node.DeleteItem))));

    // --- [ICONS]
    private static readonly IO<(int Minimum, int Maximum)> ImageLimits =
        Accessors.Scoped(nameof(Accessors.MinimumToolBarImageSize), IO.lift(static () => (Accessors.MinimumToolBarImageSize(owner: null), Accessors.MaximumToolBarImageSize(owner: null))));

    public static ValueStore<int> TabIconSize { get; } =
        Sized(
            nameof(Accessors.TabIconSize),
            Accessors.Scoped(nameof(Accessors.TabIconSize), IO.lift(static () => Accessors.TabIconSize(owner: null))),
            Accessors.Scoped(nameof(Accessors.DefaultTabIconSize), IO.lift(static () => Accessors.DefaultTabIconSize(owner: null))),
            ImageLimits,
            static size => Accessors.Scoped(nameof(Accessors.TabIconSize), IO.lift(() => Accessors.TabIconSize(owner: null, size))),
            Applied.Relaunch,
            TabIconSizeChanged);

    public static ValueStore<int> ToolBarImageSize { get; } =
        Sized(
            nameof(Accessors.ToolBarImageSize),
            Accessors.Scoped(nameof(Accessors.ToolBarImageSize), IO.lift(static () => Accessors.ToolBarImageSize(owner: null))),
            Accessors.Scoped(nameof(Accessors.DefaultToolBarImageSize), IO.lift(static () => Accessors.DefaultToolBarImageSize(owner: null))),
            ImageLimits,
            static size => Accessors.Scoped(
                nameof(Accessors.ToolBarImageSize),
                IO.lift(() => Accessors.ToolBarImageSize(owner: null, size))
                >> IO.lift(() => Accessors.ButtonSize(Accessors.ToolbarButtons, size))
                >> IO.lift(static () => Accessors.RefreshIcons(owner: null))),
            Applied.Relaunch,
            ToolBarImageSizeChanged);

    public static ValueStore<int> ButtonPadding { get; } =
        ToolbarSize(
            nameof(Accessors.ButtonPadding),
            static buttons => Accessors.PrivateButtonPadding(buttons),
            static buttons => Accessors.ButtonPadding(buttons),
            static (buttons, size) => Accessors.ButtonPadding(buttons, size));

    public static ValueStore<int> PanelButtonSize { get; } =
        ToolbarSize(
            nameof(Accessors.PanelButtonSize),
            static buttons => Accessors.PrivatePanelButtonSize(buttons),
            static buttons => Accessors.PanelButtonSize(buttons),
            static (buttons, size) => Accessors.PanelButtonSize(buttons, size));

    public static ValueStore<StripIconSize> OSnapIconSize { get; } =
        Strip("OSnapIconSize", "Osnap icon size", "Point size of the Osnap panel icons");

    public static ValueStore<StripIconSize> SelectionFilterIconSize { get; } =
        Strip("SelectionFilterIconSize", "Selection filter icon size", "Point size of the selection filter icons");

    private static ValueStore<int> Sized<TArgs>(string member, IO<int> current, IO<int> factory, IO<(int Minimum, int Maximum)> limits, Func<int, IO<Unit>> set, Applied applied, HostEvent<TArgs> changed) =>
        Row(
            current,
            factory,
            size => limits.Bind(range => IO.lift(SizeOutOfRange.Unless(size, range.Minimum, range.Maximum, member))) >> set(size),
            EqualityComparer<int>.Default.Equals,
            applied,
            changed);

    private static ValueStore<int> ToolbarSize(string member, Func<object, object> setting, Func<object, int> current, Action<object, int> set) =>
        Sized(
            member,
            Accessors.Scoped(member, IO.lift(() => current(Accessors.ToolbarButtons))),
            Accessors.Scoped(nameof(Accessors.DefaultValue), IO.lift(() => Accessors.DefaultValue(setting(Accessors.ToolbarButtons)))),
            ToolbarLimits(setting),
            size => Accessors.Scoped(member, IO.lift(() => set(Accessors.ToolbarButtons, size))),
            Applied.Live,
            SettingChanged);

    private static IO<(int Minimum, int Maximum)> ToolbarLimits(Func<object, object> setting) =>
        Accessors.Scoped(nameof(Accessors.Min), IO.lift(() => Accessors.Limits(setting(Accessors.ToolbarButtons))));

    private static ValueStore<StripIconSize> Strip(string name, string caption, string help) =>
        PlugInSettings.Store(
            SettingsNode.Application,
            new PlugInSetting<StripIconSize, int, InvalidRhinoValue>(
                ValueKey.Of<StripIconSize, int, InvalidRhinoValue>(name, caption, help, StripIconSize.Default),
                SettingType.Integer,
                Applied.IdleSave,
                Seq<string>(),
                Hidden: false)) with {
            Changed = Some(ValueStore.Signal(SettingsSaved, static _ => true)),
        };

    // --- [WINDOW]
    public static IO<Option<Rectangle>> InitialMainWindowPosition() =>
        IO.lift(static () => Callbacks.Found(AppearanceSettings.InitialMainWindowPosition(out Rectangle bounds), bounds));

    // --- [POLICY]
    public static IO<int> HistoryBand(RhinoDoc doc, AppearanceSettingsState settings, Option<int> promptRow, int stack) =>
        from height in IO.lift(Conversions.Validated<PromptFontHeight, int, InvalidRhinoValue>(settings.CommandPromptFontSize))
        from pitch in use(() => new Eto.Drawing.Font(settings.CommandPromptFontName, height / 10f)).Map(static font => (int)font.LineHeight).Bracket()
        from sites in Accessors.Scoped(nameof(Accessors.FromDocument), IO.lift(() => Missing.Unless(Accessors.FromDocument(owner: null, doc), nameof(Accessors.FromDocument))))
        from chrome in Accessors.Scoped(nameof(Accessors.StatusBar), IO.lift(() => ((Eto.Forms.Control)Accessors.StatusBar(sites)).Height + Accessors.ResizerWidth(owner: null)))
        let addend = promptRow.Match(Some: static row => row + 1, None: static () => 0)
        from strip in IO.lift(Conversions.Validated<HistoryStrip, int, InvalidRhinoValue>((stack - chrome - addend) / pitch * pitch))
        select strip + addend;

    public static IO<Unit> IconCategory(int stripHeight, int glyph, int row) =>
        from even in IO.lift(OddPadding.Unless((row - glyph) % 2 == 0, row, glyph))
        from image in ImageLimits
        from panel in ToolbarLimits(static buttons => Accessors.PrivatePanelButtonSize(buttons))
        from inset in Accessors.Scoped(nameof(Accessors.ItemPadding), IO.lift(static () => Accessors.ItemPadding(owner: null).Height))
        let tab = int.Clamp(stripHeight - (2 * inset), image.Minimum, image.Maximum)
        let strip = Conversions.Validated<StripIconSize, int, InvalidRhinoValue>(tab)
        from committed in ValueStore.Commit(Seq(
            ButtonPadding.Edit(Some((row - glyph) / 2)),
            ToolBarImageSize.Edit(Some(int.Clamp(glyph, image.Minimum, image.Maximum))),
            TabIconSize.Edit(Some(tab)),
            PanelButtonSize.Edit(Some(int.Clamp(tab, panel.Minimum, panel.Maximum))),
            IO.lift(strip).Bind(static size => OSnapIconSize.Edit(Some(size))),
            IO.lift(strip).Bind(static size => SelectionFilterIconSize.Edit(Some(size)))))
        select committed;
}
