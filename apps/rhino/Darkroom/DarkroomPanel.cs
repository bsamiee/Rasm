using System.Runtime.InteropServices;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Styling;
using Rhino;

namespace Darkroom;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class DarkroomGlyph : IGlyph {
    public static readonly DarkroomGlyph Mark = new("mark-16.svg");
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("4308ea2f-3fbd-48ea-bea8-6ca01d23f170")]
public sealed class DarkroomPanel(RhinoDoc? document) : DefinedPanel(document, Row) {
    public static readonly View.Panel Row = new() {
        Identity = typeof(DarkroomPanel),
        Caption = "Darkroom",
        Icon = Some<IGlyph>(DarkroomGlyph.Mark),
        Children = [new HolderRow.Stacked([HistorySection.Row, CompareSection.Row, RenderQueueSection.Row])],
    };
}
