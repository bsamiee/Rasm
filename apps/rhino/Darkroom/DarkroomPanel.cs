using System.Runtime.InteropServices;
using Eto.Forms;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.UI;

namespace Darkroom;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class DarkroomGlyph : IGlyph {
    public static readonly DarkroomGlyph Mark = new("mark");
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("4308ea2f-3fbd-48ea-bea8-6ca01d23f170")]
public sealed class DarkroomPanel(RhinoDoc? document) : DefinedPanel(document, Row) {
    public static readonly View.Panel Row = new() {
        Identity = typeof(DarkroomPanel),
        Caption = "Darkroom",
        Icon = Some<(IGlyph Glyph, int Master)>((DarkroomGlyph.Mark, IconSlot.PanelTab.Master)),
        InitialAfter = Some(PanelIds.Rendering),
        Children = [
            new ContainerRow.Split(
                First: IterableNE.create<Child>(new HolderRow.Filled(Above: [ScopesSection.Row], Fill: HistorySection.Row, Below: [])),
                Second: IterableNE.create<Child>(new HolderRow.Filled(Above: [], Fill: CompareSection.Row, Below: [RenderSetsSection.Row, OutputSection.Row, RenderQueueSection.Row])),
                Orientation: Orientation.Vertical,
                Anchor: SplitAnchor.Shared(ValueKey.Of<SplitShare, double>(
                    "HistorySplit",
                    "Render history height",
                    "Share of panel height above Compare",
                    SplitShare.Even))),
        ],
    };
}
