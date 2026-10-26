using Rasm.Rhino.Document;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class OutputPage(IPlugInViews owner) : DefinedOptionsPage(Row, owner) {
    public static readonly View.DocumentPage Row = new() {
        Identity = typeof(OutputPage),
        Caption = "Render Output",
        Commit = new CommitMode.Immediate(),
        Store = Some<ViewStore>(new ViewStore.DocumentStores(new RedrawPolicy.Silent())),
        Children = [.. TargetRows.Page],
    };
}

public sealed class DarkroomPage(IPlugInViews owner) : DefinedOptionsPage(Row, owner) {
    public static readonly View.OptionsPage Row = new() {
        Identity = typeof(DarkroomPage),
        Caption = "Darkroom",
        Commit = new CommitMode.Immediate(),
        Store = Some<ViewStore>(new ViewStore.ApplicationStores()),
        Children = [
            .. RenderQueueSection.Page,
            .. HistorySection.Page,
            .. CompareSection.Page,
            .. LooksSection.Page,
        ],
    };
}
