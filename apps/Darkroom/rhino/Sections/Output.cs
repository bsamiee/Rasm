using Rasm.Rhino.Document;
using Rasm.Rhino.Render.Scenes;
using Rasm.Rhino.Render.Sessions;

using Rasm.Rhino.UI.Views;

using System.Runtime.InteropServices;

namespace Darkroom.Sections;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("b72c0d83-70be-4b3a-b087-7f2fccad64ec")]
public sealed class OutputSection() : DefinedSection(Row) {
    public static readonly View.Section Row = new() {
        Identity = typeof(OutputSection),
        Caption = "Output",
        Store = Some<ViewStore>(new ViewStore.DocumentStores(new RedrawPolicy.Silent())),
        Children = (SceneSources.Fields<ImageOutputState, ImageOutputParameter>(Seq(ImageOutputParameter.ImageDpi)) + TargetRows.Page).Map(static row => (Child)row),
    };
}
