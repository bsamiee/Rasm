using Rasm.Rhino.Render.Slots;

using Rasm.Rhino.UI.Views;

using System.Runtime.InteropServices;

namespace Darkroom.Sections;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("F9092DA1-C8A1-4935-A3D8-0E800F9EDC45")]
public sealed class HistorySection() : DefinedSection(Row) {
    public static readonly View.Section Row = new() {
        Identity = typeof(HistorySection),
        Caption = "Render History",
        Children = [RenderHistory.Browser],
    };
}
