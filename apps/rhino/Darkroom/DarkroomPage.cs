using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Queue;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class DarkroomPage() : DefinedOptionsPage(Row) {
    public static readonly View.OptionsPage Row = new() {
        Identity = typeof(DarkroomPage),
        Caption = "Darkroom",
        Commit = new CommitMode.Immediate(),
        Store = Some<ViewStore>(new ViewStore.ApplicationStores()),
        Children = [
            .. QueueSettingRows.Default.Children,
            .. HistorySettingRows.Default.Children,
            .. ComparisonSettingRows.Default.Children,
            .. TileLimitRows.Default.Children,
        ],
    };
}
