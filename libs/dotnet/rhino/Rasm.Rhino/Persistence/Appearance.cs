using System.Globalization;
using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.PlugIns;
using Rhino.Runtime;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum<int>]
public sealed partial class CommandPromptStyle {
    public static readonly CommandPromptStyle Links = new(0, "Links");
    public static readonly CommandPromptStyle Graphical = new(1, "Graphical");
    public static readonly CommandPromptStyle Buttons = new(2, "Buttons");

    public string Name { get; }
}

[SmartEnum<int>]
public sealed partial class CommandPromptLocation {
    public static readonly CommandPromptLocation SeparatePanel = new(0, "SeparatePanel", CommandPromptStyle.Graphical);
    public static readonly CommandPromptLocation SideBar = new(1, "SideBar", CommandPromptStyle.Graphical);
    public static readonly CommandPromptLocation CommandHistory = new(2, "CommandHistory", CommandPromptStyle.Links);

    public string Name { get; }

    public CommandPromptStyle Style { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Appearance {

    // --- [HOST_SETTINGS]

    private const string TabPanelSettings = "Rhino.UI.Internal.TabPanels.TabPanelSettings, Rhino.UI";
    private const string IToolbarSettings = "Rhino.UI.Internal.TabPanels.IToolbarSettings, Rhino.UI";
    private const string ToolbarButtonSettings = "Rhino.UI.Internal.TabPanels.ToolbarButtonSettings, Rhino.UI";
    private const string ToolbarIntSetting = "Rhino.UI.Internal.TabPanels.ToolbarIntSetting, Rhino.UI";
    private const string TabPanelDockSites = "Rhino.UI.Internal.TabPanels.TabPanelDockSites, Rhino.UI";
    private const string RuntimeSettings = "Rhino.UI.Runtime.Settings, Rhino.UI";

    private static object ToolbarButtons => Buttons(Instance(owner: null));

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_TabIconSize")]
    private static extern int TabPanelIconSize([UnsafeAccessorType(TabPanelSettings)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "set_TabIconSize")]
    private static extern void TabPanelIconSize([UnsafeAccessorType(TabPanelSettings)] object? owner, int value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_ToolBarImageSize")]
    private static extern int TabPanelToolBarImageSize([UnsafeAccessorType(TabPanelSettings)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "set_ToolBarImageSize")]
    private static extern void TabPanelToolBarImageSize([UnsafeAccessorType(TabPanelSettings)] object? owner, int value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern void add_TabIconSizeChanged([UnsafeAccessorType(TabPanelSettings)] object? owner, EventHandler value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern void remove_TabIconSizeChanged([UnsafeAccessorType(TabPanelSettings)] object? owner, EventHandler value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern void add_ToolBarImageSizeChanged([UnsafeAccessorType(TabPanelSettings)] object? owner, EventHandler value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern void remove_ToolBarImageSizeChanged([UnsafeAccessorType(TabPanelSettings)] object? owner, EventHandler value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_ItemPadding")]
    private static extern Size ItemPadding([UnsafeAccessorType("Rhino.UI.Internal.TabPanels.Controls.BaseTabControlItem, Rhino.UI")] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "CurrentIconSize")]
    private static extern int OSnapPanelIconSize([UnsafeAccessorType("Rhino.UI.DialogPanels.OSnapPanel, Rhino.UI")] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "CurrentIconSize")]
    private static extern int SelectionFilterUiIconSize([UnsafeAccessorType("Rhino.UI.DialogPanels.SelectionFilterUi, Rhino.UI")] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_Instance")]
    [return: UnsafeAccessorType(IToolbarSettings)]
    private static extern object Instance([UnsafeAccessorType("Rhino.UI.Internal.TabPanels.ToolbarSettings, Rhino.UI")] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Buttons")]
    [return: UnsafeAccessorType(ToolbarButtonSettings)]
    private static extern object Buttons([UnsafeAccessorType(IToolbarSettings)] object settings);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_ButtonSize")]
    private static extern void ButtonSize([UnsafeAccessorType(ToolbarButtonSettings)] object buttons, int value);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern int Get([UnsafeAccessorType(ToolbarIntSetting)] object setting);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern void Set([UnsafeAccessorType(ToolbarIntSetting)] object setting, int value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PrivateButtonPadding")]
    [return: UnsafeAccessorType(ToolbarIntSetting)]
    private static extern object PrivateButtonPadding([UnsafeAccessorType(ToolbarButtonSettings)] object buttons);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PrivatePanelButtonSize")]
    [return: UnsafeAccessorType(ToolbarIntSetting)]
    private static extern object PrivatePanelButtonSize([UnsafeAccessorType(ToolbarButtonSettings)] object buttons);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern void add_SettingChanged([UnsafeAccessorType(ToolbarButtonSettings)] object buttons, EventHandler<string> value);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern void remove_SettingChanged([UnsafeAccessorType(ToolbarButtonSettings)] object buttons, EventHandler<string> value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern void RefreshIcons([UnsafeAccessorType("Rhino.UI.Internal.TabPanels.OldCodeExtensions, Rhino.UI")] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern double get_TextChangingEventsDelay([UnsafeAccessorType(RuntimeSettings)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern double get_StepEventsDelay([UnsafeAccessorType(RuntimeSettings)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    [return: UnsafeAccessorType(TabPanelDockSites)]
    private static extern object? FromDocument([UnsafeAccessorType(TabPanelDockSites)] object? owner, RhinoDoc doc);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_StatusBar")]
    [return: UnsafeAccessorType("Rhino.UI.Internal.TabPanels.Controls.StatusBar, Rhino.UI")]
    private static extern object StatusBar([UnsafeAccessorType(TabPanelDockSites)] object sites);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_ResizerWidth")]
    private static extern int ResizerWidth([UnsafeAccessorType("Rhino.UI.Internal.TabPanels.Controls.DockSiteResizer, Rhino.UI")] object? owner);

    // --- [SIGNALS]

    private static readonly HostEvent<Unit> TabIconSizeChanged =
        Subscriptions.On<EventHandler, EventArgs>(static h => add_TabIconSizeChanged(owner: null, h), static h => remove_TabIconSizeChanged(owner: null, h), static handler => handler.Invoke)
            .Choose(static _ => Some(unit));

    private static readonly HostEvent<Unit> ToolBarImageSizeChanged =
        Subscriptions.On<EventHandler, EventArgs>(static h => add_ToolBarImageSizeChanged(owner: null, h), static h => remove_ToolBarImageSizeChanged(owner: null, h), static handler => handler.Invoke)
            .Choose(static _ => Some(unit));

    private static readonly HostEvent<Unit> ButtonsChanged =
        Subscriptions.On<string>(static h => add_SettingChanged(ToolbarButtons, h), static h => remove_SettingChanged(ToolbarButtons, h))
            .Choose(static _ => Some(unit));

    // --- [MODES]

    public static Preference<bool> DarkMode { get; } =
        Preferences.Of(
            IO.lift(static () => HostUtils.RunningInDarkMode),
            static dark => Products.Accepted(() => dark ? AppearanceSettings.SetToDarkMode() : AppearanceSettings.SetToLightMode()),
            Some(Preferences.AppSettingsChanged));

    // --- [PROMPT]

    private const string GetPresentationStyle = "Rhino.UI.Internal.DockBars.CommandLine.GetPresentationStyle";
    private const string ModeEntry = "mode";
    private const string StyleKey = "CommandPromptStyle";

    public static Preference<CommandPromptLocation> PromptLocation { get; } =
        Preferences.Of(
            NamedCallbacks.Execute(GetPresentationStyle, static args => Conversions.Present(args.TryGetInt(ModeEntry, out int mode), mode).Bind(static mode => Optional(CommandPromptLocation.TryGet(mode, out CommandPromptLocation? held) ? held : null))),
            static location => CommandMode($"_Location=_{location.Name}"),
            Some(SettingsNode.Application.Saved));

    public static Preference<CommandPromptStyle> PromptStyle { get; } =
        Preferences.Of(
            from location in PromptLocation.Read
            from found in SettingsNode.Options.Existing
            let text = found.Bind(static node => Conversions.Present(node.TryGetString(StyleKey, out string held), held)).Bind(Conversions.Present)
            select text.Bind(static held => int.TryParse(held, NumberStyles.Integer, CultureInfo.InvariantCulture, out int key)
                    ? Optional(CommandPromptStyle.TryGet(key, out CommandPromptStyle? keyed) ? keyed : null)
                    : toSeq(CommandPromptStyle.Items).Find(item => string.Equals(item.Name, held, StringComparison.OrdinalIgnoreCase)))
                .IfNone(location.Style),
            static style => CommandMode($"_Style=_{style.Name}"),
            Some(SettingsNode.Application.Saved));

    public static Preference<bool> AutocompleteCommands { get; } = Preferences.Setting(SettingsNode.Options.Child("Appearance"), "AutocompleteCommands", fallback: true);

    public static Preference<bool> FuzzyAutocomplete { get; } = Preferences.Setting(SettingsNode.Options.Child("Appearance"), "FuzzyAutocomplete", fallback: true);

    private static IO<Unit> CommandMode(string option) =>
        Products.Accepted(() => RhinoApp.RunScript($"-_CommandMode {option} _Enter", echo: false));

    // --- [EDIT_DELAYS]

    private static readonly SettingsNode Runtime = new SettingsNode(new SettingsRoot.PlugInNode(RhinoApp.CurrentRhinoId)).Child("UI").Child("Settings");

    public static Preference<Duration> TextChangingDelay { get; } =
        Delay(Products.Scoped(static () => get_TextChangingEventsDelay(owner: null)), "TextChangingEventsDelay");

    public static Preference<Duration> StepDelay { get; } =
        Delay(Products.Scoped(static () => get_StepEventsDelay(owner: null)), "StepEventsDelay");

    private static Preference<Duration> Delay(IO<double> seconds, string key) =>
        Preferences.Of(
            seconds.Map(Duration.FromSeconds),
            delay => Runtime.Created.Bind(node => IO.lift(() => node.SetDouble(key, delay.TotalSeconds))),
            Some(Runtime.Saved));

    // --- [ICONS]

    public static Preference<int> TabIconSize { get; } =
        Preferences.Of(
            Products.Scoped(static () => TabPanelIconSize(owner: null)),
            static size => Products.Scoped(() => TabPanelIconSize(owner: null, size)),
            Some(TabIconSizeChanged));

    public static Preference<int> ToolBarImageSize { get; } =
        Preferences.Of(
            Products.Scoped(static () => TabPanelToolBarImageSize(owner: null)),
            static size => Products.Scoped(() => TabPanelToolBarImageSize(owner: null, size))
                >> Products.Scoped(() => ButtonSize(ToolbarButtons, size))
                >> Products.Scoped(static () => RefreshIcons(owner: null)),
            Some(ToolBarImageSizeChanged));

    public static Preference<int> ButtonPadding { get; } = Toolbar(static () => PrivateButtonPadding(ToolbarButtons));

    public static Preference<int> PanelButtonSize { get; } = Toolbar(static () => PrivatePanelButtonSize(ToolbarButtons));

    public static Preference<int> OSnapIconSize { get; } = Strip("OSnapIconSize", Products.Scoped(static () => OSnapPanelIconSize(owner: null)));

    public static Preference<int> SelectionFilterIconSize { get; } = Strip("SelectionFilterIconSize", Products.Scoped(static () => SelectionFilterUiIconSize(owner: null)));

    private static Preference<int> Toolbar(Func<object> setting) =>
        Preferences.Of(
            Products.Scoped(() => Get(setting())),
            size => Products.Scoped(() => Set(setting(), size)),
            Some(ButtonsChanged));

    private static Preference<int> Strip(string key, IO<int> current) =>
        Preferences.Of(
            current,
            size =>
                from node in SettingsNode.Application.Created
                from written in IO.lift(() => node.SetInteger(key, size))
                from flushed in IO.lift(static () => PlugIn.FlushSettingsSavedQueue())
                select unit,
            Some(SettingsNode.Application.Saved));

    // --- [DENSITY]

    public static Seq<IO<IO<Unit>>> IconSizes(int stripHeight, int glyph, int row) =>
        IO.lift(() => RhinoFault.Unless(int.IsEvenInteger(row - glyph), RhinoReason.Outside, (row, glyph))) switch {
            var even => (even >> Products.Scoped(static () => ItemPadding(owner: null).Height)).Map(inset => stripHeight - (2 * inset)) switch {
                var tab => Seq(
                    even >> ButtonPadding.Swap((row - glyph) / 2),
                    even >> ToolBarImageSize.Swap(glyph),
                    tab.Bind(TabIconSize.Swap),
                    tab.Bind(PanelButtonSize.Swap),
                    tab.Bind(OSnapIconSize.Swap),
                    tab.Bind(SelectionFilterIconSize.Swap)),
            },
        };

    public static IO<int> HistoryBand(RhinoDoc doc, Option<int> promptRow, int stack) =>
        from settings in IO.lift(AppearanceSettings.GetCurrentState)
        from pitch in (from font in use(() => new Font(settings.CommandPromptFontName, settings.CommandPromptFontSize / 10f)) select (int)font.LineHeight).Bracket()
        from sites in Products.Scoped(() => Conversions.Required(FromDocument(owner: null, doc))).Bind(static found => IO.lift(found))
        from chrome in Products.Scoped(() => ((Control)StatusBar(sites)).Height + ResizerWidth(owner: null))
        let addend = promptRow.Match(Some: static at => at + 1, None: static () => 0)
        select ((stack - chrome - addend) / pitch * pitch) + addend;
}
