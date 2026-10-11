using Rasm.Rhino.Render.Queue;

using Rasm.Rhino.UI.Views;

using System.Runtime.InteropServices;

namespace Darkroom.Sections;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("5241D43D-4825-41EC-B908-468DFF1FF424")]
public sealed class RenderQueueSection() : DefinedSection(Row) {
    public static readonly View.Section Row = new() {
        Identity = typeof(RenderQueueSection),
        Caption = "Render Queue",
        Children = Queues.Rows(DarkroomSettings.Queue),
    };
}
