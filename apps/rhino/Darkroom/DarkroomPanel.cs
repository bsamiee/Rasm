using Rasm.Rhino;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;

using Rasm.Rhino.UI.Views;

using System.Runtime.InteropServices;
using Darkroom.Sections;
using Eto.Forms;
using Rhino;
using Rhino.UI;

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("4308ea2f-3fbd-48ea-bea8-6ca01d23f170")]
public sealed class DarkroomPanel(RhinoDoc? document) : DefinedPanel(document, Row) {
    public static readonly View.Panel Row = new() {
        Identity = typeof(DarkroomPanel),
        Caption = "Darkroom",
        Icon = Some<(IGlyph Glyph, int Master)>((DarkroomGlyph.Mark, DarkroomGlyph.MarkPoints)),
        InitialAfter = Some(PanelIds.Rendering),
        Children = [
            new ContainerRow.Split(
                First: IterableNE.create<Child>(new HolderRow.Filled(Above: [ScopesSection.Row], Fill: HistorySection.Row, Below: [])),
                Second: IterableNE.create<Child>(new HolderRow.Filled(Above: [], Fill: CompareSection.Row, Below: [RenderSetsSection.Row, OutputSection.Row, RenderQueueSection.Row])),
                Orientation: Orientation.Vertical,
                Anchor: SplitAnchor.Shared(ValueKey.Of<SplitShare, double, InvalidRhinoValue>(
                    "HistorySplit",
                    "Render history height",
                    "Share of the panel height the render history takes above compare",
                    SplitShare.Even))),
        ],
    };
}
