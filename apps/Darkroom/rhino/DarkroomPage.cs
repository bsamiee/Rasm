using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI.Rows;

using Rasm.Rhino.UI.Views;

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class DarkroomPage() : DefinedOptionsPage(Row) {
    public static readonly View.OptionsPage Row = new() {
        Identity = typeof(DarkroomPage),
        Caption = "Darkroom",
        Commit = new CommitMode.Immediate(),
        Store = Some<ViewStore>(new ViewStore.ApplicationStores()),
        Children = [
            .. DarkroomSettings.Queue.Children,
            new ControlRow.Group("Section history", RowRules.Always),
            .. SectionRows.Setting(DarkroomSettings.History.Capacity, RowRules.Always).Map(static row => (Child)row),
            .. SectionRows.Setting(DarkroomSettings.History.Window, RowRules.Always).Map(static row => (Child)row),
            new ControlRow.Group("Render comparison", RowRules.Always),
            .. DarkroomSettings.Comparison.Children,
            new ControlRow.Group("Preview tiles", RowRules.Always),
            .. DarkroomSettings.Tiles.Children,
        ],
    };
}
