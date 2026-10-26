using System.Runtime.InteropServices;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;
using Rhino;

namespace Darkroom;

// --- [MODELS] --------------------------------------------------------------------------
internal static class DarkroomGlyphs {
    public static readonly GlyphSource Mark = Icons.Embedded(typeof(DarkroomGlyphs).Assembly, "mark-16.svg");
    public static readonly GlyphSource VerticalWipe = Icons.Embedded(typeof(DarkroomGlyphs).Assembly, "vertical-wipe.svg");
    public static readonly GlyphSource HorizontalWipe = Icons.Embedded(typeof(DarkroomGlyphs).Assembly, "horizontal-wipe.svg");
    public static readonly GlyphSource SideBySide = Icons.Embedded(typeof(DarkroomGlyphs).Assembly, "side-by-side.svg");
    public static readonly GlyphSource Swap = Icons.Embedded(typeof(DarkroomGlyphs).Assembly, "swap.svg");
    public static readonly GlyphSource Flip = Icons.Embedded(typeof(DarkroomGlyphs).Assembly, "flip.svg");
    public static readonly GlyphSource Fit = Icons.Embedded(typeof(DarkroomGlyphs).Assembly, "fit.svg");
    public static readonly GlyphSource ActualSize = Icons.Embedded(typeof(DarkroomGlyphs).Assembly, "actual-size.svg");
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("4308ea2f-3fbd-48ea-bea8-6ca01d23f170")]
public sealed class DarkroomPanel(RhinoDoc? document) : DefinedPanel(document, Row) {
    public static readonly View.Panel Row = new() {
        Identity = typeof(DarkroomPanel),
        Caption = "Darkroom",
        Icon = Some(DarkroomGlyphs.Mark),
        Children = [new HolderRow.Stacked([HistorySection.Row, CompareSection.Row, RenderQueueSection.Row])],
    };
}
