using System.Globalization;
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
    // --- [SIGNALS]
    private static readonly HostEvent<Unit> TabIconSizeChanged =
        Subscriptions.On<EventHandler, EventArgs>(static h => Accessors.add_TabIconSizeChanged(owner: null, h), static h => Accessors.remove_TabIconSizeChanged(owner: null, h), static handler => handler.Invoke)
            .Choose(static _ => Some(unit));

    private static readonly HostEvent<Unit> ToolBarImageSizeChanged =
        Subscriptions.On<EventHandler, EventArgs>(static h => Accessors.add_ToolBarImageSizeChanged(owner: null, h), static h => Accessors.remove_ToolBarImageSizeChanged(owner: null, h), static handler => handler.Invoke)
            .Choose(static _ => Some(unit));

    private static readonly HostEvent<Unit> ButtonsChanged =
        Subscriptions.On<string>(static h => Accessors.add_SettingChanged(Accessors.ToolbarButtons, h), static h => Accessors.remove_SettingChanged(Accessors.ToolbarButtons, h))
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
        Delay(Accessors.Scoped(static () => Accessors.get_TextChangingEventsDelay(owner: null)), "TextChangingEventsDelay");

    public static Preference<Duration> StepDelay { get; } =
        Delay(Accessors.Scoped(static () => Accessors.get_StepEventsDelay(owner: null)), "StepEventsDelay");

    private static Preference<Duration> Delay(IO<double> seconds, string key) =>
        Preferences.Of(
            seconds.Map(Duration.FromSeconds),
            delay => Runtime.Created.Bind(node => IO.lift(() => node.SetDouble(key, delay.TotalSeconds))),
            Some(Runtime.Saved));

    // --- [ICONS]
    public static Preference<int> TabIconSize { get; } =
        Preferences.Of(
            Accessors.Scoped(static () => Accessors.TabIconSize(owner: null)),
            static size => Accessors.Scoped(() => Accessors.TabIconSize(owner: null, size)),
            Some(TabIconSizeChanged));

    public static Preference<int> ToolBarImageSize { get; } =
        Preferences.Of(
            Accessors.Scoped(static () => Accessors.ToolBarImageSize(owner: null)),
            static size => Accessors.Scoped(() => Accessors.ToolBarImageSize(owner: null, size))
                >> Accessors.Scoped(() => Accessors.ButtonSize(Accessors.ToolbarButtons, size))
                >> Accessors.Scoped(static () => Accessors.RefreshIcons(owner: null)),
            Some(ToolBarImageSizeChanged));

    public static Preference<int> ButtonPadding { get; } = Toolbar(static () => Accessors.PrivateButtonPadding(Accessors.ToolbarButtons));

    public static Preference<int> PanelButtonSize { get; } = Toolbar(static () => Accessors.PrivatePanelButtonSize(Accessors.ToolbarButtons));

    public static Preference<int> OSnapIconSize { get; } = Strip("OSnapIconSize", Accessors.Scoped(static () => Accessors.OSnapIconSize(owner: null)));

    public static Preference<int> SelectionFilterIconSize { get; } = Strip("SelectionFilterIconSize", Accessors.Scoped(static () => Accessors.SelectionFilterIconSize(owner: null)));

    private static Preference<int> Toolbar(Func<object> setting) =>
        Preferences.Of(
            Accessors.Scoped(() => Accessors.Get(setting())),
            size => Accessors.Scoped(() => Accessors.Set(setting(), size)),
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
            var even => (even >> Accessors.Scoped(static () => Accessors.ItemPadding(owner: null).Height)).Map(inset => stripHeight - (2 * inset)) switch {
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
        from sites in Accessors.Scoped(() => Conversions.Required(Accessors.FromDocument(owner: null, doc))).Bind(static found => IO.lift(found))
        from chrome in Accessors.Scoped(() => ((Control)Accessors.StatusBar(sites)).Height + Accessors.ResizerWidth(owner: null))
        let addend = promptRow.Match(Some: static at => at + 1, None: static () => 0)
        select ((stack - chrome - addend) / pitch * pitch) + addend;
}
