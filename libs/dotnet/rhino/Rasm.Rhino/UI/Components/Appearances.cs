using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;

namespace Rasm.Rhino.UI.Components;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Appearances {
    // --- [ROWS]
    public static ControlRows Rows(Seq<RhinoTheme> themes, params Seq<Row> extra) =>
        ControlRows.Fixed(
            Seq(
                Row.Of("Theme", Themed(themes), themes.Map(static theme => new ChoiceItem<string>(theme.Name, theme.Name))),
                Row.Of("Dark mode", Appearance.DarkMode),
                Row.Of("Command prompt", Appearance.PromptLocation, toSeq(CommandPromptLocation.Items).Map(static location => new ChoiceItem<CommandPromptLocation>(location, location.Name))),
                Row.Of("Prompt style", Appearance.PromptStyle, toSeq(CommandPromptStyle.Items).Map(static style => new ChoiceItem<CommandPromptStyle>(style, style.Name))),
                Row.Of("Autocomplete commands", Appearance.AutocompleteCommands),
                Row.Of("Fuzzy autocomplete", Appearance.FuzzyAutocomplete),
                Row.Of("Toolbar icon size", Whole(Appearance.ToolBarImageSize), IconRange),
                Row.Of("Tab icon size", Whole(Appearance.TabIconSize), IconRange),
                Row.Of("Panel button size", Whole(Appearance.PanelButtonSize), IconRange),
                Row.Of("Sidebar", Toolbars.Sidebar))
            + extra);

    private static readonly NumberRange IconRange = new(16d, 64d) { Step = Some(1d), Decimals = Some(0) };

    private static Preference<string> Themed(Seq<RhinoTheme> themes) =>
        Preferences.Of(RhinoTheme.Current.Read, name => themes.Find(theme => string.Equals(theme.Name, name, StringComparison.Ordinal)).Traverse(static theme => theme.Apply).As().Map(unit), Some(PreferenceRows.Signal(RhinoTheme.Current)));

    private static Preference<double> Whole(Preference<int> size) =>
        Preferences.Of(size.Read.Map(static held => (double)held), value => size.Write((int)Math.Round(value, MidpointRounding.ToEven)), Some(PreferenceRows.Signal(size)));
}
