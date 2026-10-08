using System.Buffers;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Runtime;
using Rhino.UI;
using Rhino.UI.DialogPanels;
using Rhino.UI.Theme;
using Riok.Mapperly.Abstractions;


namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
public enum CommandPromptLocation { SeparatePanel = 0, SideBar = 1, CommandHistory = 2 }

public enum CommandPromptStyle { Links = 0, Graphical = 1, Buttons = 2 }

public enum Applied { Live = 0, IdleSave = 1, Relaunch = 2 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ShortcutRow(KeyboardKey Key, ModifierKey Modifier, Option<string> Macro);

public sealed record AppearanceEdit(Applied Applied, IO<Unit> Write);

public sealed class AppearanceSetting<T> {
    internal AppearanceSetting(Applied applied, IO<T> read, Func<T, IO<Unit>> write, Func<T, T, bool> same, Func<T, Fin<Unit>> valid) =>
        (Applied, Read, Write, Same, Valid) = (applied, read, write, same, valid);

    public IO<T> Read { get; }

    private Applied Applied { get; }

    private Func<T, IO<Unit>> Write { get; }

    private Func<T, T, bool> Same { get; }

    private Func<T, Fin<Unit>> Valid { get; }

    public Fin<AppearanceEdit> Set(T value) =>
        Valid(value).Map(_ => new AppearanceEdit(Applied, Read.Bind(held => when(!Same(held, value), Write(value)).As())));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Accessors {
    // --- [OWNERS]
    private const string TabPanelSettings = "Rhino.UI.Internal.TabPanels.TabPanelSettings, Rhino.UI";

    private const string IToolbarSettings = "Rhino.UI.Internal.TabPanels.IToolbarSettings, Rhino.UI";

    private const string ToolbarButtonSettings = "Rhino.UI.Internal.TabPanels.ToolbarButtonSettings, Rhino.UI";

    private const string ToolbarIntSetting = "Rhino.UI.Internal.TabPanels.ToolbarIntSetting, Rhino.UI";

    private const string TabPanelDockSites = "Rhino.UI.Internal.TabPanels.TabPanelDockSites, Rhino.UI";

    private const string UnsafeNativeMethods = "UnsafeNativeMethods, RhinoCommon";

    // --- [TAB_PANELS]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_TabIconSize")]
    internal static extern int TabIconSize([UnsafeAccessorType(TabPanelSettings)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "set_TabIconSize")]
    internal static extern void TabIconSize([UnsafeAccessorType(TabPanelSettings)] object? owner, int value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_ToolBarImageSize")]
    internal static extern int ToolBarImageSize([UnsafeAccessorType(TabPanelSettings)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "set_ToolBarImageSize")]
    internal static extern void ToolBarImageSize([UnsafeAccessorType(TabPanelSettings)] object? owner, int value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_MinimumToolBarImageSize")]
    internal static extern int MinimumToolBarImageSize([UnsafeAccessorType(TabPanelSettings)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_MaximumToolBarImageSize")]
    internal static extern int MaximumToolBarImageSize([UnsafeAccessorType(TabPanelSettings)] object? owner);

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

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern void RefreshIcons([UnsafeAccessorType("Rhino.UI.Internal.TabPanels.OldCodeExtensions, Rhino.UI")] object? owner);

    // --- [STRIPS]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern int CurrentIconSize(OSnapPanel? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern int CurrentIconSize(SelectionFilterUi? owner);

    // --- [THEME]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_Settings")]
    internal static extern PersistentSettings Settings([UnsafeAccessorType("Rhino.UI.ThemeSettings, Rhino.UI")] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_SettingId")]
    internal static extern string SettingId(ThemeBase zone);

    // --- [PROMPT]
    [UnsafeAccessor(UnsafeAccessorKind.StaticField)]
    internal static extern ref EventHandler? PromptStyleChanged([UnsafeAccessorType("Rhino.UI.CommandPanels, RhinoCommon")] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern bool CRhinoAppSettings_GetAutocompleteCommands([UnsafeAccessorType(UnsafeNativeMethods)] object? owner, bool defaultValue);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern void CRhinoAppSettings_SetAutocompleteCommands([UnsafeAccessorType(UnsafeNativeMethods)] object? owner, bool enable);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern bool CRhinoAppSettings_GetFuzzyAutocomplete([UnsafeAccessorType(UnsafeNativeMethods)] object? owner, bool defaultValue);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    internal static extern void CRhinoAppSettings_SetFuzzyAutocomplete([UnsafeAccessorType(UnsafeNativeMethods)] object? owner, bool enable);

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

[Mapper]
internal static partial class SettingsMapper {
    internal static partial ShortcutRow ToRow(KeyboardShortcut shortcut);
}

public static class AppSettings {
    // --- [SEPARATORS]
    private const char PackageSourceSeparator = ';';

    private static Fin<Unit> Tokens(Seq<string> names, SearchValues<char> separators, string member) =>
        Invalid.Unless(names.ForAll(name => !string.IsNullOrWhiteSpace(name) && !name.AsSpan().ContainsAny(separators)), member);

    // --- [WINDOW]
    public static IO<Option<Rectangle>> InitialMainWindowPosition() =>
        IO.lift(static () => Answers.Found(AppearanceSettings.InitialMainWindowPosition(out Rectangle bounds), bounds));

    // --- [ANALYSIS]
    public static IO<CurvatureAnalysisSettingsState> CurvatureAutoRange(CurvatureAnalysisSettingsState initial, Seq<Mesh> meshes) =>
        IO.lift(() => {
            CurvatureAnalysisSettingsState state = initial;
            return Refused.Unless(CurvatureAnalysisSettings.CalculateCurvatureAutoRange(meshes, ref state), state, nameof(CurvatureAnalysisSettings.CalculateCurvatureAutoRange));
        });

    // --- [ALIASES]
    public static IO<Seq<CommandAlias>> Aliases() =>
        IO.lift(static () => toSeq(Range(0, CommandAliasList.Count)).Map(CommandAliasList.GetAlias).Strict());

    public static IO<Option<string>> AliasMacro(string alias) =>
        IO.lift(() => Optional(CommandAliasList.GetMacro(alias)));

    public static IO<Unit> DeleteAlias(string alias) =>
        IO.lift(() => Refused.Unless(CommandAliasList.Delete(alias), nameof(CommandAliasList.Delete)));

    // --- [SHORTCUTS]
    public static IO<Seq<ShortcutRow>> Shortcuts() =>
        IO.lift(static () => Rows(ShortcutKeySettings.GetShortcuts()));

    public static IO<Seq<ShortcutRow>> DefaultShortcuts() =>
        IO.lift(static () => Rows(ShortcutKeySettings.GetDefaults()));

    public static IO<Unit> SetShortcut(KeyboardKey key, ModifierKey modifier, string macro) =>
        from acceptable in IO.lift(() => Refused.Unless(ShortcutKeySettings.IsAcceptableKeyCombo(key, modifier), nameof(ShortcutKeySettings.IsAcceptableKeyCombo)))
        from written in IO.lift(() => ShortcutKeySettings.SetMacro(key, modifier, macro))
        select written;

    private static Seq<ShortcutRow> Rows(KeyboardShortcut[] shortcuts) =>
        toSeq(shortcuts).Map(SettingsMapper.ToRow).Strict();

    // --- [NEVER_REPEAT]
    public static IO<(bool Enabled, Seq<string> Names)> NeverRepeat() =>
        IO.lift(static () => (Enabled: NeverRepeatList.UseNeverRepeatList, Names: toSeq(NeverRepeatList.CommandNames()).Choose(Answers.Present).Strict()));

    public static IO<int> SetNeverRepeat(Seq<string> names) =>
        from tokens in IO.lift(() => Tokens(names, SearchValues.Create(" ,;\b\v\r\n\t"), nameof(NeverRepeatList.SetList)))
        from count in IO.lift(() => NeverRepeatList.SetList([.. names]))
        select count;

    // --- [FILES]
    public static IO<int> AddSearchPath(string folder, Option<int> index) =>
        from qualified in Answers.QualifiedPath(folder)
        from inserted in IO.lift(() => Answers.Required(FileSettings.AddSearchPath(qualified, Answers.Unset(index)), nameof(FileSettings.AddSearchPath)))
        select inserted;

    public static IO<Unit> DeleteSearchPath(string folder) =>
        IO.lift(() => Refused.Unless(FileSettings.DeleteSearchPath(folder), nameof(FileSettings.DeleteSearchPath)));

    public static IO<Option<string>> FindFile(string fileName) =>
        IO.lift(() => Answers.Present(FileSettings.FindFile(fileName)));

    public static IO<Seq<string>> AutoSaveBeforeCommands() =>
        IO.lift(static () => Answers.Present(FileSettings.AutoSaveBeforeCommands()));

    public static IO<Unit> SetAutoSaveBeforeCommands(Seq<string> commands) =>
        from tokens in IO.lift(() => Tokens(commands, SearchValues.Create(" "), nameof(FileSettings.SetAutoSaveBeforeCommands)))
        from written in IO.lift(() => FileSettings.SetAutoSaveBeforeCommands([.. commands]))
        select written;

    // --- [PACKAGES]
    public static IO<Seq<string>> PackageSources() =>
        IO.lift(static () => toSeq(PackageManagerSettings.Sources.Split(PackageSourceSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)));

    public static IO<Unit> SetPackageSources(Seq<string> sources) =>
        from tokens in IO.lift(() => Tokens(sources, SearchValues.Create([PackageSourceSeparator]), nameof(PackageManagerSettings.Sources)))
        from written in IO.lift(() => { PackageManagerSettings.Sources = string.Join(PackageSourceSeparator, sources); })
        select written;

    // --- [MODEL_AIDS]
    public static IO<IDisposable> NudgeFollowsActiveDocument(Func<double, ConstructionPlaneGridDefaults, (double Nudge, double Ctrl, double Shift)> steps, Action<Error> reject) =>
        from nudged in NudgeActiveDocument(steps)
        from attached in Events.AttachAll(
            Seq(
                Events.Attach(static h => RhinoDoc.NewDocument += h, static h => RhinoDoc.NewDocument -= h, Answers.Handler<DocumentEventArgs>(_ => NudgeActiveDocument(steps), reject)),
                Events.Attach(static h => RhinoDoc.EndOpenDocument += h, static h => RhinoDoc.EndOpenDocument -= h, Answers.Handler<DocumentOpenEventArgs>(_ => NudgeActiveDocument(steps), reject)),
                EventKind.ActiveDocumentChanged.Attach(_ => NudgeActiveDocument(steps), reject),
                EventKind.DocumentPropertiesChanged.Attach(_ => NudgeActiveDocument(steps), reject)),
            reject)
        select attached;

    private static IO<Unit> NudgeActiveDocument(Func<double, ConstructionPlaneGridDefaults, (double Nudge, double Ctrl, double Shift)> steps) =>
        IO.lift(static () => Optional(RhinoDoc.ActiveDoc)).Bind(doc => doc.Traverse(active => Nudge(active, steps)).As()).Map(static _ => unit);

    private static IO<Unit> Nudge(RhinoDoc doc, Func<double, ConstructionPlaneGridDefaults, (double Nudge, double Ctrl, double Shift)> steps) =>
        from resolution in IO.lift(() => DisplayResolution(doc))
        from grid in IO.lift(doc.GetGridDefaults)
        from written in IO.lift(() => (ModelAidSettings.NudgeKeyStep, ModelAidSettings.CtrlNudgeKeyStep, ModelAidSettings.ShiftNudgeKeyStep) = steps(resolution, grid))
        select unit;

    private static double DisplayResolution(RhinoDoc doc) =>
        doc.ModelDistanceDisplayMode switch {
            global::Rhino.UI.DistanceDisplayMode.Decimal => Math.Pow(10, -doc.ModelDistanceDisplayPrecision),
            global::Rhino.UI.DistanceDisplayMode.Fractional => Math.ScaleB(1, -doc.ModelDistanceDisplayPrecision),
            global::Rhino.UI.DistanceDisplayMode.FeetInches => Math.ScaleB(LengthUnit.Scale(LengthUnit.Inches, doc.ModelUnits), -doc.ModelDistanceDisplayPrecision),
        };
}

public static class Appearance {
    // --- [KEYS]
    private const string GetPresentationStyle = "Rhino.UI.Internal.DockBars.CommandLine.GetPresentationStyle";

    private const string SetPresentationStyle = "Rhino.UI.Internal.DockBars.CommandLine.SetPresentationStyle";

    private const string ModeParameter = "mode";

    private const string CommandPromptLocationKey = "CommandPromptLocation";

    private const string CommandPromptStyleKey = "CommandPromptStyle";

    private const string OSnapIconSizeKey = "OSnapIconSize";

    private const string SelectionFilterIconSizeKey = "SelectionFilterIconSize";

    // --- [LIMITS]
    private const int PromptFontFloor = 60;

    private const int MinIconSize = 16;

    private const int MaxIconSize = 32;

    private const int ThemeColorHeight = 18;

    // --- [READS]
    public static IO<PersistentSettings> Options() =>
        IO.lift(static () => PersistentSettings.RhinoAppSettings.AddChild("Options"));

    public static IO<Seq<string>> ThemeKeys() =>
        IO.lift(static () => (
            from zone in Seq<ThemeBase>(ThemeSettings.Frame, ThemeSettings.Content)
            from entry in toSeq(zone.Enumerate())
            where entry.Value is Eto.Drawing.Color
            select $"{Accessors.SettingId(zone)}.{entry.Id}").Distinct());

    private static Fin<int> LinePitch(AppearanceSettingsState settings) =>
        from tenths in Limits.AtLeast(PromptFontFloor).Check(settings.CommandPromptFontSize, nameof(AppearanceSettingsState.CommandPromptFontSize))
        from pitch in Try.lift(() => Pitch(settings.CommandPromptFontName, tenths / 10f)).Run().MapFail(error => new FontUnavailable(settings.CommandPromptFontName, error))
        select pitch;

    private static int Pitch(string family, float points) {
        using Eto.Drawing.Font font = new(family, points);
        return (int)font.LineHeight;
    }

    // --- [ROWS]
    private static readonly Seq<PropertyInfo> Members =
        toSeq(typeof(AppearanceSettingsState).GetProperties(BindingFlags.Public | BindingFlags.Instance)).Filter(static property => property.CanWrite).Strict();

    public static readonly AppearanceSetting<AppearanceSettingsState> Settings = new(
        Applied.Live,
        IO.lift(AppearanceSettings.GetCurrentState),
        static state => IO.lift(() => AppearanceSettings.UpdateFromState(state)),
        static (held, desired) => Members.ForAll(property =>
            property.GetValue(held) is Color color ? Answers.Same(color, (Color)property.GetValue(desired)!) : Equals(property.GetValue(held), property.GetValue(desired))),
        static state => (Seq(LinePitch(state).Map(static _ => unit))
                + Members
                    .Filter(static property => property.Name is not (nameof(AppearanceSettingsState.SelectionWindowFillColor) or nameof(AppearanceSettingsState.SelectionWindowCrossingFillColor)))
                    .Map(property => property.GetValue(state) is Color color ? TranslucentColor.Unless(color.A == byte.MaxValue, property.Name, color.A) : unit))
            .Traverse(static check => check)
            .As()
            .Map(static _ => unit));

    public static readonly AppearanceSetting<CommandPromptLocation> Location = Row(
        Applied.Live,
        DisposalOps.Using(static () => new NamedParametersEventArgs(), static args => IO.lift(() =>
            from handled in Refused.Unless(HostUtils.ExecuteNamedCallback(GetPresentationStyle, args), nameof(HostUtils.ExecuteNamedCallback))
            from mode in InvalidAnswer.Unless(args.TryGetInt(ModeParameter, out int value) && Enum.IsDefined((CommandPromptLocation)value), (CommandPromptLocation)value, GetPresentationStyle)
            select mode)),
        static location =>
            from options in Options()
            from cleared in IO.lift(() => options.DeleteItem(CommandPromptLocationKey))
            from shown in DisposalOps.Using(static () => new NamedParametersEventArgs(), args => IO.lift(() => {
                args.Set(ModeParameter, (int)location);
                return Refused.Unless(HostUtils.ExecuteNamedCallback(SetPresentationStyle, args), nameof(HostUtils.ExecuteNamedCallback));
            }))
            select shown);

    public static readonly AppearanceSetting<CommandPromptStyle> Style = Row(
        Applied.Live,
        from options in Options()
        from location in Location.Read
        from stored in IO.lift(SettingType.Enumeration<CommandPromptStyle>().Read(options, CommandPromptStyleKey))
        select stored.IfNone(location switch {
            CommandPromptLocation.SeparatePanel or CommandPromptLocation.SideBar => CommandPromptStyle.Graphical,
            CommandPromptLocation.CommandHistory => CommandPromptStyle.Links,
        }),
        static style =>
            from options in Options()
            from written in IO.lift(() => {
                options.SetEnumValue(CommandPromptStyleKey, style);
                Accessors.PromptStyleChanged(owner: null)?.Invoke(sender: null, EventArgs.Empty);
            })
            select written);

    public static readonly AppearanceSetting<bool> AutocompleteCommands = Row(
        Applied.Live,
        IO.lift(static () => Accessors.CRhinoAppSettings_GetAutocompleteCommands(owner: null, defaultValue: false)),
        static on => IO.lift(() => Accessors.CRhinoAppSettings_SetAutocompleteCommands(owner: null, on)));

    public static readonly AppearanceSetting<bool> FuzzyAutocomplete = Row(
        Applied.Live,
        IO.lift(static () => Accessors.CRhinoAppSettings_GetFuzzyAutocomplete(owner: null, defaultValue: false)),
        static on => IO.lift(() => Accessors.CRhinoAppSettings_SetFuzzyAutocomplete(owner: null, on)));

    public static readonly AppearanceSetting<bool> BlackWhiteSwitching = Row(
        Applied.Live,
        IO.lift(static () => AppearanceSettings.BlackWhiteSwitching),
        static on => IO.lift(() => { AppearanceSettings.BlackWhiteSwitching = on; }));

    public static readonly AppearanceSetting<int> TabIconSize = Row(
        Applied.Relaunch,
        IO.lift(static () => Accessors.TabIconSize(owner: null)),
        static value => IO.lift(() => Accessors.TabIconSize(owner: null, value)));

    public static readonly AppearanceSetting<int> ToolBarImageSize = Row(
        Applied.Relaunch,
        IO.lift(static () => Accessors.ToolBarImageSize(owner: null)),
        static value => IO.lift(() => {
            Accessors.ToolBarImageSize(owner: null, value);
            Accessors.ButtonSize(Accessors.ToolbarButtons, value);
            Accessors.RefreshIcons(owner: null);
        }));

    public static readonly AppearanceSetting<int> ButtonPadding = Row(
        Applied.Live,
        IO.lift(static () => Accessors.ButtonPadding(Accessors.ToolbarButtons)),
        static value => IO.lift(() => Accessors.ButtonPadding(Accessors.ToolbarButtons, value)));

    public static readonly AppearanceSetting<int> PanelButtonSize = Row(
        Applied.Live,
        IO.lift(static () => Accessors.PanelButtonSize(Accessors.ToolbarButtons)),
        static value => IO.lift(() => Accessors.PanelButtonSize(Accessors.ToolbarButtons, value)));

    public static readonly AppearanceSetting<int> OSnapIconSize = Row(
        Applied.IdleSave,
        IO.lift(static () => Accessors.CurrentIconSize((OSnapPanel?)null)),
        static value => IO.lift(() => PersistentSettings.RhinoAppSettings.SetInteger(OSnapIconSizeKey, value)));

    public static readonly AppearanceSetting<int> SelectionFilterIconSize = Row(
        Applied.IdleSave,
        IO.lift(static () => Accessors.CurrentIconSize((SelectionFilterUi?)null)),
        static value => IO.lift(() => PersistentSettings.RhinoAppSettings.SetInteger(SelectionFilterIconSizeKey, value)));

    public static AppearanceSetting<Color> Widget(WidgetColor axis) =>
        new(Applied.Live, IO.lift(() => AppearanceSettings.GetWidgetColor(axis)), color => IO.lift(() => AppearanceSettings.SetWidgetColor(axis, color)), Answers.Same, static _ => unit);

    public static IO<AppearanceSetting<Option<Color>>> Theme(string key) =>
        from keys in ThemeKeys()
        from known in IO.lift(() => UnknownThemeKey.Unless(keys.Exists(entry => string.Equals(entry, key, StringComparison.Ordinal)), key))
        select new AppearanceSetting<Option<Color>>(
            Applied.IdleSave,
            IO.lift(() => SettingType.Color.Read(Accessors.Settings(owner: null), key)),
            color => IO.lift(() => Accessors.Settings(owner: null).SetColor(key, color.ToNullable())),
            static (held, desired) => held.Map(static color => color.ToArgb()) == desired.Map(static color => color.ToArgb()),
            static _ => unit);

    private static AppearanceSetting<T> Row<T>(Applied applied, IO<T> read, Func<T, IO<Unit>> write) =>
        new(applied, read, write, EqualityComparer<T>.Default.Equals, static _ => unit);

    // --- [EDITS]
    public static IO<Unit> Edit(Seq<Fin<AppearanceEdit>> edits) =>
        from valid in IO.lift(edits.Traverse(static edit => edit).As())
        from written in valid.TraverseM(static edit => edit.Write).As()
        from flushed in when(valid.Exists(static edit => edit.Applied != Applied.Live), IO.lift(PlugIn.FlushSettingsSavedQueue)).As()
        select unit;

    // --- [POLICY]
    public static IO<int> HistoryBand(RhinoDoc doc, AppearanceSettingsState settings, Option<int> promptRow, double stack) =>
        from pitch in IO.lift(() => LinePitch(settings))
        from sites in IO.lift(() => Document.Missing.Unless(Accessors.FromDocument(owner: null, doc), nameof(Accessors.FromDocument)))
        let addend = promptRow.Map(static row => 1 + row).IfNone(0)
        let strip = (int)Math.Floor((stack - ((Eto.Forms.Control)Accessors.StatusBar(sites)).Height - Accessors.ResizerWidth(owner: null) - addend) / pitch) * pitch
        from band in IO.lift(() => Limits.Above(ThemeColorHeight).Check(strip, nameof(strip)).Map(height => height + addend))
        select band;

    public static IO<Seq<Fin<AppearanceEdit>>> IconCategory(int stripHeight, int glyph, int row) =>
        from buttons in IO.lift(static () => Accessors.ToolbarButtons)
        let padding = (row - glyph) / 2
        let paddingSetting = Accessors.PrivateButtonPadding(buttons)
        from valid in IO.lift(() => Seq(
                OddPadding.Unless((row - glyph) % 2 == 0, row, glyph),
                Limits.AtLeast(Accessors.Min(paddingSetting)).AtMost(Accessors.Max(paddingSetting)).Check(padding, nameof(padding)).Map(static _ => unit))
            .Traverse(static check => check)
            .As())
        let panelSetting = Accessors.PrivatePanelButtonSize(buttons)
        let image = (Minimum: Accessors.MinimumToolBarImageSize(owner: null), Maximum: Accessors.MaximumToolBarImageSize(owner: null))
        let tab = int.Clamp(stripHeight - (2 * Accessors.ItemPadding(owner: null).Height), image.Minimum, image.Maximum)
        let icon = int.Clamp(tab, MinIconSize, MaxIconSize)
        select Seq(
            TabIconSize.Set(tab),
            ToolBarImageSize.Set(int.Clamp(glyph, image.Minimum, image.Maximum)),
            ButtonPadding.Set(padding),
            PanelButtonSize.Set(int.Clamp(tab, Accessors.Min(panelSetting), Accessors.Max(panelSetting))),
            OSnapIconSize.Set(icon),
            SelectionFilterIconSize.Set(icon));
}
