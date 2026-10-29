using Rasm.Rhino.Commands;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;

namespace Rasm.Rhino.UI;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelAction {
    public static readonly PanelAction Show = new("Show", static (panel, _) => HostPanels.Open(panel));
    public static readonly PanelAction Hide = new("Hide", static (panel, doc) => HostPanels.Close(panel, doc));
    public static readonly PanelAction Toggle = new("Toggle", static (panel, doc) => HostPanels.Toggle(panel, doc));

    [UseDelegateFromConstructor]
    public partial IO<Unit> Apply(View.Panel panel, RhinoDoc doc);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostPanels {
    // --- [PLACEMENT]
    public static IO<Unit> Open(View.Panel row) =>
        row.Placement.Switch(
            row.Identity,
            last: static (panel, _) => IO.lift(() => Panels.OpenPanel(panel, makeSelectedPanel: true)),
            into: static (panel, into) => Docked(into.DockBar, panel),
            beside: static (panel, beside) => IO.lift(() => Conversions.Present(Panels.PanelDockBar(beside.Panel)))
                .Bind(bar => bar.Match(Some: held => Docked(held, panel), None: () => IO.lift(() => Panels.OpenPanel(panel, makeSelectedPanel: true)))),
            floating: static (panel, _) => Products.Accepted(() => Panels.FloatPanel(panel, Panels.FloatPanelMode.Show)));

    public static IO<Unit> Close(View.Panel row, RhinoDoc doc) =>
        row.Placement is Placement.Floating
            ? Products.Accepted(() => Panels.FloatPanel(row.Identity, Panels.FloatPanelMode.Hide))
            : IO.lift(() => Panels.ClosePanel(row.Identity, doc));

    public static IO<Unit> Toggle(View.Panel row, RhinoDoc doc) =>
        IO.lift(() => Panels.IsPanelVisible(row.Identity, isSelectedTab: true)).Bind(shown => shown ? Close(row, doc) : Open(row));

    private static IO<Unit> Docked(Guid dockBar, Type panel) => IO.lift(() => Panels.OpenPanel(dockBar, panel, makeSelectedPanel: true)).Map(unit);

    // --- [ICONS]
    public static IO<Unit> Icon(View.Panel row, IconResource icon) =>
        IO.lift(static () => Panels.IconSizeInPixels).Bind(pixels => Icons.DrawingIcon(icon, pixels)).Bind(drawn => IO.lift(() => Panels.ChangePanelIcon(row.Identity, drawn)));

    // --- [EVENTS]
    public static HostEvent<(uint Document, bool Shown)> Shown(View.Panel row) =>
        Subscriptions.On<ShowPanelEventArgs>(static h => Panels.Show += h, static h => Panels.Show -= h)
            .Choose(args => Conversions.Present(args.PanelId == row.Identity.GUID, (Document: args.DocumentSerialNumber, Shown: args.Show)));

    public static HostEvent<uint> Closed(View.Panel row) =>
        Subscriptions.On<PanelEventArgs>(static h => Panels.Closed += h, static h => Panels.Closed -= h)
            .Choose(args => Conversions.Present(args.PanelId == row.Identity.GUID, args.DocumentSerialNumber));

    // --- [LAYOUTS]
    public static IO<Unit> RestoreLayout(RhinoDoc doc, string name) => Documents.RunScript(doc, $"""-_WindowLayout "{name}" _Enter""", echo: false, display: None);

    public static IO<Unit> SaveLayout(RhinoDoc doc, string name) => Documents.RunScript(doc, $"""-_SaveWindowLayout "{name}" _Enter""", echo: false, display: None);

    // --- [COMMANDS]
    public static CommandDefinition<CommandInput.Run> Command(View.Panel row) =>
        new(RhinoGet.StringToCommandOptionName(row.Caption), new CommandInput.Run((doc, mode, _) =>
            (mode is RunMode.Scripted
                ? Getters.Choice(doc, new GetterRequest<GetOption, PanelAction>(row.Caption) {
                    Options = toSeq(PanelAction.Items).Map(static action => OptionSpecs.Plain(action.Key, None, hidden: false, _ => IO.pure(action))),
                })
                : IO.pure(PanelAction.Toggle))
            .Bind(action => action.Apply(row, doc)))) { Help = row.Help };
}
