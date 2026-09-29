using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using LanguageExt.UnsafeValueAccess;
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

[assembly: UseStaticMapper(typeof(Answers))]

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate (double Nudge, double Ctrl, double Shift) NudgeSteps(double resolution, ConstructionPlaneGridDefaults grid);

public enum CommandPromptLocation { SeparatePanel = 0, SideBar = 1, CommandHistory = 2 }

public enum CommandPromptStyle { Links = 0, Graphical = 1, Buttons = 2 }

public enum IconSize { TabIcon = 0, ToolBarImage = 1, ButtonPadding = 2, PanelButton = 3, OSnapIcon = 4, SelectionFilterIcon = 5 }

public enum Applied { Live = 0, IdleSave = 1, Relaunch = 2 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record AliasRow(Option<string> Alias, Option<string> Macro, bool Instant);

public sealed record ShortcutRow(KeyboardKey Key, ModifierKey Modifier, Option<string> Macro);

[ValueObject<int>]
[ValidationError<ValidationFailure>]
public readonly partial struct CommandPromptFontHeight {
    private const int LoadFloor = 60;

    public float Points => _value / 10f;

    public static Fin<CommandPromptFontHeight> From(int tenths) =>
        Validate(tenths, provider: null, out CommandPromptFontHeight item) is { } error ? error : item;

    public static Fin<CommandPromptFontHeight> FromPoints(int points) =>
        From(points * 10);

    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref int value) {
        if (value < LoadFloor)
            validationError = new BelowLowerLimit(nameof(CommandPromptFontHeight), LoadFloor);
    }
}

public sealed record CommandPrompt(CommandPromptLocation Location, CommandPromptStyle Style, bool AutocompleteCommands, bool FuzzyAutocomplete);

public readonly record struct ThemeKey(string Zone, string Entry) {
    public string Text => $"{Zone}.{Entry}";
}

public sealed record AppearanceState(
    AppearanceSettingsState Settings,
    CommandPrompt Prompt,
    HashMap<ThemeKey, Color> Theme,
    HashMap<WidgetColor, Color> Widgets,
    bool BlackWhiteSwitching,
    HashMap<IconSize, int> Icons);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Setting {
    public Applied Applied =>
        Switch(
            member: static _ => Applied.Live,
            location: static _ => Applied.Live,
            style: static _ => Applied.Live,
            autocompleteCommands: static _ => Applied.Live,
            fuzzyAutocomplete: static _ => Applied.Live,
            themeEntry: static _ => Applied.IdleSave,
            widget: static _ => Applied.Live,
            blackWhiteSwitching: static _ => Applied.Live,
            icon: static icon => icon.Size switch {
                IconSize.TabIcon or IconSize.ToolBarImage => Applied.Relaunch,
                IconSize.ButtonPadding or IconSize.PanelButton => Applied.Live,
                IconSize.OSnapIcon or IconSize.SelectionFilterIcon => Applied.IdleSave,
            });

    public string Label =>
        Switch(
            member: static member => $"{nameof(AppearanceSettings)}.{member.Property.Name}",
            location: static _ => $"{nameof(CommandPrompt)}.{nameof(CommandPrompt.Location)}",
            style: static _ => $"{nameof(CommandPrompt)}.{nameof(CommandPrompt.Style)}",
            autocompleteCommands: static _ => $"{nameof(CommandPrompt)}.{nameof(CommandPrompt.AutocompleteCommands)}",
            fuzzyAutocomplete: static _ => $"{nameof(CommandPrompt)}.{nameof(CommandPrompt.FuzzyAutocomplete)}",
            themeEntry: static entry => $"{nameof(ThemeSettings)}.{entry.Key.Text}",
            widget: static widget => $"{nameof(WidgetColor)}.{widget.Axis}",
            blackWhiteSwitching: static _ => $"{nameof(AppearanceSettings)}.{nameof(AppearanceSettings.BlackWhiteSwitching)}",
            icon: static icon => $"{nameof(IconSize)}.{icon.Size}");

    public Option<string> Text =>
        Switch(
            member: static member => Formatted(member.Value),
            location: static location => Formatted(location.Value),
            style: static style => Formatted(style.Value),
            autocompleteCommands: static flag => Formatted(flag.On),
            fuzzyAutocomplete: static flag => Formatted(flag.On),
            themeEntry: static entry => entry.Color.Map(static color => Formatted(color)),
            widget: static widget => Formatted(widget.Color),
            blackWhiteSwitching: static switching => Formatted(switching.On),
            icon: static icon => Formatted(icon.Value));

    private static string Formatted(object value) =>
        value is Color color
            ? $"{color.A},{color.R},{color.G},{color.B}"
            : string.Format(CultureInfo.InvariantCulture, "{0}", value);

    public sealed record Member(System.Reflection.PropertyInfo Property, object Value) : Setting;

    public sealed record Location(CommandPromptLocation Value) : Setting;

    public sealed record Style(CommandPromptStyle Value) : Setting;

    public sealed record AutocompleteCommands(bool On) : Setting;

    public sealed record FuzzyAutocomplete(bool On) : Setting;

    public sealed record ThemeEntry(ThemeKey Key, Option<Color> Color) : Setting;

    public sealed record Widget(WidgetColor Axis, Color Color) : Setting;

    public sealed record BlackWhiteSwitching(bool On) : Setting;

    public sealed record Icon(IconSize Size, int Value) : Setting;
}

public sealed record SettingChange(Setting Held, Setting Desired);

// --- [SERVICES] ------------------------------------------------------------------------
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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AppSettings {
    // --- [SEPARATORS]
    private const char CommandNameSeparator = ' ';

    private const char PackageSourceSeparator = ';';

    // --- [WINDOW]
    public static IO<Option<Rectangle>> InitialMainWindowPosition() =>
        IO.lift(static () => Answers.Found(AppearanceSettings.InitialMainWindowPosition(out Rectangle bounds), bounds));

    // --- [ANALYSIS]
    public static IO<CurvatureAnalysisSettingsState> CurvatureAutoRange(CurvatureAnalysisSettingsState initial, Seq<Mesh> meshes) =>
        from populated in IO.lift(() => Invalid.Unless(!meshes.IsEmpty, nameof(CurvatureAnalysisSettings.CalculateCurvatureAutoRange)))
        from computed in IO.lift(() => {
            CurvatureAnalysisSettingsState state = initial;
            return Refused.Unless(CurvatureAnalysisSettings.CalculateCurvatureAutoRange(meshes, ref state), state, nameof(CurvatureAnalysisSettings.CalculateCurvatureAutoRange));
        })
        select computed;

    // --- [ALIASES]
    public static IO<Seq<AliasRow>> Aliases() =>
        IO.lift(static () => toSeq(Range(0, CommandAliasList.Count))
            .TraverseM(static index => Missing.Unless(CommandAliasList.GetAlias(index), nameof(CommandAliasList.GetAlias)))
            .As()
            .Map(static rows => rows.Map(static row => SettingsMapper.ToRow(row)).Strict()));

    public static IO<Option<string>> AliasMacro(string alias) =>
        from named in IO.lift(() => Invalid.Unless(alias.Length > 0, nameof(CommandAliasList.IsAlias)))
        from macro in IO.lift(() => CommandAliasList.IsAlias(alias) ? Some(CommandAliasList.GetMacro(alias)) : Option<string>.None)
        select macro;

    public static IO<Unit> DeleteAlias(string alias) =>
        from named in IO.lift(() => Invalid.Unless(alias.Length > 0, nameof(CommandAliasList.Delete)))
        from deleted in IO.lift(() => Refused.Unless(CommandAliasList.Delete(alias), nameof(CommandAliasList.Delete)))
        select deleted;

    public static IO<Unit> UpdateAliases(Seq<AliasRow> rows, bool replaceAll) =>
        from named in IO.lift(() => Invalid.Unless(rows.ForAll(static row => row.Alias.IsSome), nameof(CommandAliasList.Update)))
        from updated in IO.lift(() => CommandAliasList.Update(rows.Map(static row => new CommandAlias(row.Alias.ValueUnsafe(), row.Macro.ValueUnsafe(), row.Instant)), replaceAll))
        select updated;

    // --- [SHORTCUTS]
    public static IO<Seq<ShortcutRow>> Shortcuts() =>
        IO.lift(static () => Rows(ShortcutKeySettings.GetShortcuts()));

    public static IO<Seq<ShortcutRow>> DefaultShortcuts() =>
        IO.lift(static () => Rows(ShortcutKeySettings.GetDefaults()));

    public static IO<Unit> SetShortcut(KeyboardKey key, ModifierKey modifier, string macro) =>
        from acceptable in IO.lift(() => Refused.Unless(ShortcutKeySettings.IsAcceptableKeyCombo(key, modifier), nameof(ShortcutKeySettings.IsAcceptableKeyCombo)))
        from written in IO.lift(() => ShortcutKeySettings.SetMacro(key, modifier, macro))
        select written;

    public static IO<Unit> UpdateShortcuts(Seq<ShortcutRow> rows, bool replaceAll) =>
        from populated in IO.lift(() => Invalid.Unless(!rows.IsEmpty, nameof(ShortcutKeySettings.Update)))
        from updated in IO.lift(() => ShortcutKeySettings.Update(rows.Map(static row => new KeyboardShortcut { Key = row.Key, Modifier = row.Modifier, Macro = row.Macro.ValueUnsafe() }), replaceAll))
        select updated;

    private static Seq<ShortcutRow> Rows(KeyboardShortcut[] shortcuts) =>
        toSeq(shortcuts).Map(static row => SettingsMapper.ToRow(row)).Strict();

    // --- [NEVER_REPEAT]
    public static IO<(bool Enabled, Seq<string> Names)> NeverRepeat() =>
        IO.lift(static () => (Enabled: NeverRepeatList.UseNeverRepeatList, Names: toSeq(NeverRepeatList.CommandNames()).Filter(static name => name.Length > 0).Strict()));

    public static IO<int> SetNeverRepeat(Seq<string> names) =>
        from tokens in IO.lift(() => Tokens(names, CommandNameSeparator, nameof(NeverRepeatList.SetList)))
        from count in IO.lift(() => NeverRepeatList.SetList([.. names]))
        select count;

    // --- [FILES]
    public static IO<int> AddSearchPath(string folder, int index) =>
        from placed in IO.lift(() => unless(index == -1, IndexOutOfRange.Unless(index, FileSettings.SearchPathCount + 1, nameof(FileSettings.AddSearchPath))).As())
        from qualified in Answers.QualifiedPath(folder)
        from inserted in IO.lift(() => Answers.NonNegative(FileSettings.AddSearchPath(qualified, index), nameof(FileSettings.AddSearchPath)))
        select inserted;

    public static IO<Unit> DeleteSearchPath(string folder) =>
        IO.lift(() => Refused.Unless(FileSettings.DeleteSearchPath(folder), nameof(FileSettings.DeleteSearchPath)));

    public static IO<Option<string>> FindFile(string fileName) =>
        from named in IO.lift(() => Invalid.Unless(fileName.Length > 0, nameof(FileSettings.FindFile)))
        from found in IO.lift(() => Optional(FileSettings.FindFile(fileName)).Filter(static path => !string.IsNullOrWhiteSpace(path)))
        select found;

    public static IO<Seq<string>> AutoSaveBeforeCommands() =>
        IO.lift(static () => toSeq(FileSettings.AutoSaveBeforeCommands()));

    public static IO<Unit> SetAutoSaveBeforeCommands(Seq<string> commands) =>
        from tokens in IO.lift(() => Tokens(commands, CommandNameSeparator, nameof(FileSettings.SetAutoSaveBeforeCommands)))
        from written in IO.lift(() => FileSettings.SetAutoSaveBeforeCommands([.. commands]))
        select written;

    // --- [PACKAGES]
    public static IO<Seq<string>> PackageSources() =>
        IO.lift(static () => toSeq(PackageManagerSettings.Sources.Split(PackageSourceSeparator, StringSplitOptions.RemoveEmptyEntries)));

    public static IO<Unit> SetPackageSources(Seq<string> sources) =>
        from tokens in IO.lift(() => Tokens(sources, PackageSourceSeparator, nameof(PackageManagerSettings.Sources)))
        from written in IO.lift(() => { PackageManagerSettings.Sources = string.Join(PackageSourceSeparator, sources); })
        select written;

    private static Fin<Unit> Tokens(Seq<string> names, char separator, string member) =>
        Invalid.Unless(names.ForAll(name => (name.Length > 0) && !name.Contains(separator, StringComparison.Ordinal)), member);

    // --- [MODEL_AIDS]
    public static IO<IDisposable> NudgeFollowsActiveDocument(NudgeSteps steps, Action<Error> reject) =>
        from nudged in NudgeActiveDocument(steps)
        from attached in Events.AttachAll(
            Seq(EventKind.NewDocument, EventKind.EndOpenDocument, EventKind.ActiveDocumentChanged, EventKind.DocumentPropertiesChanged)
                .Map(kind => Events.Attach(kind, new EventScope.Any(), _ => NudgeActiveDocument(steps), reject)))
        select attached;

    private static IO<Unit> NudgeActiveDocument(NudgeSteps steps) =>
        IO.lift(static () => Optional(RhinoDoc.ActiveDoc)).Bind(doc => doc.Map(active => Nudge(active, steps)).IfNone(IO.pure(unit)));

    private static IO<Unit> Nudge(RhinoDoc doc, NudgeSteps steps) =>
        from resolution in DocumentUnits.DisplayResolution(doc, DocumentSpace.Model)
        from grid in IO.lift(doc.GetGridDefaults)
        from written in IO.lift(() => (ModelAidSettings.NudgeKeyStep, ModelAidSettings.CtrlNudgeKeyStep, ModelAidSettings.ShiftNudgeKeyStep) = steps(resolution, grid))
        select unit;
}

public static class Appearance {
    // --- [KEYS]
    private const string CommandPromptLocationKey = "CommandPromptLocation";

    private const string CommandOptionsPresentationStyleKey = "CommandOptionsPresentationStyle";

    private const string CommandPromptStyleKey = "CommandPromptStyle";

    private const string OSnapIconSizeKey = "OSnapIconSize";

    private const string SelectionFilterIconSizeKey = "SelectionFilterIconSize";

    private const string SetPresentationStyle = "Rhino.UI.Internal.DockBars.CommandLine.SetPresentationStyle";

    private const string ModeParameter = "mode";

    // --- [LIMITS]
    private const int MinIconSize = 16;

    private const int MaxIconSize = 32;

    private const int ThemeColorHeight = 18;

    // --- [STATE]
    private static readonly Seq<System.Reflection.PropertyInfo> Members =
        toSeq(typeof(AppearanceSettingsState).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)).Filter(static property => property.CanWrite).Strict();

    // --- [READS]
    public static IO<AppearanceState> Read() =>
        from options in Options()
        from buttons in Buttons()
        from keys in ThemeKeys()
        from state in IO.lift(() => new AppearanceState(
            AppearanceSettings.GetCurrentState(),
            Prompt(options, Location(options)),
            toHashMap(keys.Choose(static key => ThemeColor(key).Map(color => (key, color)))),
            toHashMap(toSeq(Enum.GetValues<WidgetColor>()).Map(static axis => (axis, AppearanceSettings.GetWidgetColor(axis)))),
            AppearanceSettings.BlackWhiteSwitching,
            toHashMap(Seq(
                (IconSize.TabIcon, Accessors.TabIconSize(owner: null)),
                (IconSize.ToolBarImage, Accessors.ToolBarImageSize(owner: null)),
                (IconSize.ButtonPadding, Accessors.ButtonPadding(buttons)),
                (IconSize.PanelButton, Accessors.PanelButtonSize(buttons)),
                (IconSize.OSnapIcon, Accessors.CurrentIconSize((OSnapPanel?)null)),
                (IconSize.SelectionFilterIcon, Accessors.CurrentIconSize((SelectionFilterUi?)null))))))
        select state;

    public static IO<Seq<ThemeKey>> ThemeKeys() =>
        IO.lift(static () => (
            from zone in Seq<ThemeBase>(ThemeSettings.Frame, ThemeSettings.Content)
            from entry in toSeq(zone.Enumerate())
            where entry.Value is Eto.Drawing.Color
            select new ThemeKey(Accessors.SettingId(zone), entry.Id)).Distinct());

    private static CommandPrompt Prompt(PersistentSettings options, CommandPromptLocation location) =>
        new(
            location,
            Stored<CommandPromptStyle>(options, CommandPromptStyleKey).IfNone(location is CommandPromptLocation.CommandHistory ? CommandPromptStyle.Links : CommandPromptStyle.Graphical),
            Accessors.CRhinoAppSettings_GetAutocompleteCommands(owner: null, defaultValue: false),
            Accessors.CRhinoAppSettings_GetFuzzyAutocomplete(owner: null, defaultValue: false));

    private static CommandPromptLocation Location(PersistentSettings options) =>
        (Stored<CommandPromptLocation>(options, CommandPromptLocationKey) | Stored<CommandPromptLocation>(options, CommandOptionsPresentationStyleKey))
            .IfNone(HostUtils.RunningOnOSX ? CommandPromptLocation.SideBar : CommandPromptLocation.CommandHistory);

    private static Option<T> Stored<T>(PersistentSettings options, string key) where T : struct, Enum =>
        Answers.Found(options.TryGetEnumValue(key, out T value), value).Filter(static value => Enum.IsDefined(value));

    private static Option<Color> ThemeColor(ThemeKey key) =>
        Answers.Found(Accessors.Settings(owner: null).TryGetColor(key.Text, out Color? color), color).Bind(static stored => Optional(stored));

    private static IO<PersistentSettings> Options() =>
        IO.lift(static () => PersistentSettings.RhinoAppSettings.AddChild("Options"));

    private static IO<object> Buttons() =>
        IO.lift(static () => Accessors.Buttons(Accessors.Instance(owner: null)));

    // --- [PLAN]
    public static IO<Seq<SettingChange>> Plan(AppearanceState held, AppearanceState desired) =>
        from keys in ThemeKeys()
        from valid in IO.lift(() => (
                LinePitch(desired.Settings).ToValidation(),
                Members.Filter(static property => property.PropertyType == typeof(Color)
                        && property.Name is not (nameof(AppearanceSettingsState.SelectionWindowFillColor) or nameof(AppearanceSettingsState.SelectionWindowCrossingFillColor)))
                    .Map(property => (property.Name, Color: (Color)property.GetValue(desired.Settings)!))
                    .Traverse(static member => TranslucentColor.Unless(member.Color.A == byte.MaxValue, member.Name, member.Color.A).ToValidation())
                    .As(),
                toSeq(desired.Theme.Keys).Traverse(key => UnknownThemeKey.Unless(keys.Exists(known => known == key), key.Text).ToValidation()).As())
            .Apply(static (_, _, _) => unit)
            .As()
            .ToFin())
        select Changes(keys, held, desired).Filter(static change => change.Held.Text != change.Desired.Text);

    private static Seq<SettingChange> Changes(Seq<ThemeKey> keys, AppearanceState held, AppearanceState desired) =>
        Members.Map(property => Change(value => new Setting.Member(property, value), property.GetValue(held.Settings)!, property.GetValue(desired.Settings)!))
        + Seq(
            Change(static value => new Setting.Location(value), held.Prompt.Location, desired.Prompt.Location),
            Change(static value => new Setting.Style(value), held.Prompt.Style, desired.Prompt.Style),
            Change(static value => new Setting.AutocompleteCommands(value), held.Prompt.AutocompleteCommands, desired.Prompt.AutocompleteCommands),
            Change(static value => new Setting.FuzzyAutocomplete(value), held.Prompt.FuzzyAutocomplete, desired.Prompt.FuzzyAutocomplete),
            Change(static value => new Setting.BlackWhiteSwitching(value), held.BlackWhiteSwitching, desired.BlackWhiteSwitching))
        + keys.Map(key => Change(value => new Setting.ThemeEntry(key, value), held.Theme.Find(key), desired.Theme.Find(key)))
        + toSeq(Enum.GetValues<WidgetColor>()).Choose(axis =>
            from before in held.Widgets.Find(axis)
            from after in desired.Widgets.Find(axis)
            select Change(value => new Setting.Widget(axis, value), before, after))
        + toSeq(Enum.GetValues<IconSize>()).Choose(size =>
            from before in held.Icons.Find(size)
            from after in desired.Icons.Find(size)
            select Change(value => new Setting.Icon(size, value), before, after));

    private static SettingChange Change<T>(Func<T, Setting> setting, T held, T desired) =>
        new(setting(held), setting(desired));

    private static Fin<int> LinePitch(AppearanceSettingsState settings) =>
        from height in CommandPromptFontHeight.From(settings.CommandPromptFontSize)
        from pitch in Try.lift(() => Pitch(settings.CommandPromptFontName, height.Points)).Run().MapFail(_ => new FontUnavailable(settings.CommandPromptFontName))
        select pitch;

    private static int Pitch(string family, float points) {
        using Eto.Drawing.Font font = new(family, points);
        return (int)font.LineHeight;
    }

    // --- [APPLY]
    public static IO<Unit> Apply(Seq<SettingChange> plan) =>
        from options in Options()
        from buttons in Buttons()
        from current in IO.lift(AppearanceSettings.GetCurrentState)
        from written in plan.TraverseM(change => change.Desired.Switch(
            (Options: options, Buttons: buttons, Current: current),
            member: static (scope, member) => IO.lift(() => member.Property.SetValue(scope.Current, member.Value)),
            location: static (scope, location) => SetLocation(scope.Options, location.Value),
            style: static (scope, style) => IO.lift(() => {
                scope.Options.SetEnumValue(CommandPromptStyleKey, style.Value);
                Accessors.PromptStyleChanged(owner: null)?.Invoke(sender: null, EventArgs.Empty);
            }),
            autocompleteCommands: static (_, flag) => IO.lift(() => Accessors.CRhinoAppSettings_SetAutocompleteCommands(owner: null, flag.On)),
            fuzzyAutocomplete: static (_, flag) => IO.lift(() => Accessors.CRhinoAppSettings_SetFuzzyAutocomplete(owner: null, flag.On)),
            themeEntry: static (_, entry) => IO.lift(() => Accessors.Settings(owner: null).SetColor(entry.Key.Text, entry.Color.ToNullable())),
            widget: static (_, widget) => IO.lift(() => AppearanceSettings.SetWidgetColor(widget.Axis, widget.Color)),
            blackWhiteSwitching: static (_, switching) => IO.lift(() => { AppearanceSettings.BlackWhiteSwitching = switching.On; }),
            icon: static (scope, icon) => SetIconSize(scope.Buttons, icon))).As()
        from committed in when(plan.Exists(static change => change.Desired is Setting.Member), IO.lift(() => AppearanceSettings.UpdateFromState(current))).As()
        from flushed in when(plan.Exists(static change => change.Desired.Applied != Applied.Live), IO.lift(PlugIn.FlushSettingsSavedQueue)).As()
        select unit;

    private static IO<Unit> SetLocation(PersistentSettings options, CommandPromptLocation location) =>
        from cleared in IO.lift(() => options.DeleteItem(CommandPromptLocationKey))
        from shown in Disposal.Using(static () => new NamedParametersEventArgs(), args => IO.lift(() => {
            args.Set(ModeParameter, (int)location);
            return Refused.Unless(HostUtils.ExecuteNamedCallback(SetPresentationStyle, args), nameof(HostUtils.ExecuteNamedCallback));
        }))
        select shown;

    private static IO<Unit> SetIconSize(object buttons, Setting.Icon icon) =>
        icon.Size switch {
            IconSize.TabIcon => IO.lift(() => Accessors.TabIconSize(owner: null, icon.Value)),
            IconSize.ToolBarImage => IO.lift(() => {
                Accessors.ToolBarImageSize(owner: null, icon.Value);
                Accessors.ButtonSize(buttons, icon.Value);
                Accessors.RefreshIcons(owner: null);
            }),
            IconSize.ButtonPadding => IO.lift(() => Accessors.ButtonPadding(buttons, icon.Value)),
            IconSize.PanelButton => IO.lift(() => Accessors.PanelButtonSize(buttons, icon.Value)),
            IconSize.OSnapIcon => IO.lift(() => PersistentSettings.RhinoAppSettings.SetInteger(OSnapIconSizeKey, icon.Value)),
            IconSize.SelectionFilterIcon => IO.lift(() => PersistentSettings.RhinoAppSettings.SetInteger(SelectionFilterIconSizeKey, icon.Value)),
        };

    // --- [POLICY]
    public static IO<int> HistoryBand(RhinoDoc doc, AppearanceSettingsState settings, Option<int> promptRow, double stack) =>
        from pitch in IO.lift(() => LinePitch(settings))
        from sites in IO.lift(() => Missing.Unless(Accessors.FromDocument(owner: null, doc), nameof(Accessors.FromDocument)))
        let addend = promptRow.Map(static row => 1 + row).IfNone(0)
        let strip = (int)Math.Floor((stack - ((Eto.Forms.Control)Accessors.StatusBar(sites)).Height - Accessors.ResizerWidth(owner: null) - addend) / pitch) * pitch
        from band in IO.lift(() => Limits.Above(ThemeColorHeight).Check(strip, nameof(strip)).Map(height => height + addend))
        select band;

    public static IO<HashMap<IconSize, int>> IconCategory(int stripHeight, int glyph, int row) =>
        from buttons in Buttons()
        let panelSetting = Accessors.PrivatePanelButtonSize(buttons)
        let paddingSetting = Accessors.PrivateButtonPadding(buttons)
        let padding = (row - glyph) / 2
        from valid in IO.lift(() => (
                OddPadding.Unless((row - glyph) % 2 == 0, row, glyph).ToValidation(),
                Limits.AtLeast(Accessors.Min(paddingSetting)).AtMost(Accessors.Max(paddingSetting), nameof(IconSize.ButtonPadding))
                    .Bind(limits => limits.Check(padding, nameof(padding)))
                    .ToValidation())
            .Apply(static (_, _) => unit)
            .As()
            .ToFin())
        let image = (Minimum: Accessors.MinimumToolBarImageSize(owner: null), Maximum: Accessors.MaximumToolBarImageSize(owner: null))
        let tab = int.Clamp(stripHeight - (2 * Accessors.ItemPadding(owner: null).Height), image.Minimum, image.Maximum)
        select toHashMap(Seq(
            (IconSize.TabIcon, tab),
            (IconSize.ToolBarImage, int.Clamp(glyph, image.Minimum, image.Maximum)),
            (IconSize.ButtonPadding, padding),
            (IconSize.PanelButton, int.Clamp(tab, Accessors.Min(panelSetting), Accessors.Max(panelSetting))),
            (IconSize.OSnapIcon, int.Clamp(tab, MinIconSize, MaxIconSize)),
            (IconSize.SelectionFilterIcon, int.Clamp(tab, MinIconSize, MaxIconSize))));
}

[Mapper]
internal static partial class SettingsMapper {
    internal static partial AliasRow ToRow(CommandAlias alias);

    internal static partial ShortcutRow ToRow(KeyboardShortcut shortcut);

    internal static partial SettingsState ToState(PersistentSettings node);
}
