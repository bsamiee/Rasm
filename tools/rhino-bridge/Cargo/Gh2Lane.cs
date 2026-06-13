using System.Diagnostics.CodeAnalysis;
using GhEditor = Grasshopper2.UI.Editor;

namespace Rasm.Bridge.Cargo;

// --- [MODELS] -------------------------------------------------------------------------------

// Ownership: the lane's typed capture receipt — scenarios and the runner speak this vocabulary,
// never raw GH2 object graphs.
internal readonly record struct CaptureFile(string Path, int Width, int Height);

// --- [SERVICES] -----------------------------------------------------------------------------

// Ownership: the M2 choke point — the ONLY file in the rebuilt tool that names Grasshopper2.*
// types; a GH2 break has a one-file fix radius by construction. This build ships the render lane
// only: probe 0b pinned the dataflow lane Unsupported (Start(SolutionMode.Headless) blocked under
// the execute lane), so the solve and archive-diff vocabularies land here when their probes admit
// them. Acquire is ctor-never-Show, the StatusBar-SIGABRT root fix ported verbatim: ShowEditor
// always Form.Show()s (even createVisible:false) and schedules the fatal DrawRect against GH2's
// un-warmed icon subsystem; the bare ctor builds Canvas + Documents.Current with no NSView
// realization, and DrawToBitmap rasterizes offscreen independent of visibility.
internal sealed class Gh2Lane : IDisposable {
    private const int FallbackHeight = 720;
    private const int FallbackWidth = 1280;

    private GhEditor? editor;

    private Gh2Lane(GhEditor editor) => this.editor = editor;

    [SuppressMessage(category: "Reliability", checkId: "CA2000:Dispose objects before losing scope",
        Justification = "GH2's Editor is the host's process-static singleton (Editor.Instance); headless construction transfers ownership to the host for the process lifetime — disposing it would tear down the live canvas.")]
    internal static Gh2Lane Acquire() => new(editor: GhEditor.Instance ?? new GhEditor());

    internal Fin<CaptureFile> DrawCanvas(string path) {
        // BOUNDARY ADAPTER — GH2 paint can throw or swallow into a null bitmap; both project to
        // the typed rail, never a host crash.
        if (editor is not { Canvas: { } canvas }) {
            return Fin.Fail<CaptureFile>(error: Error.New(message: "Gh2Lane: editor canvas absent"));
        }
        try {
            int width = canvas.Width > 0 ? canvas.Width : FallbackWidth;
            int height = canvas.Height > 0 ? canvas.Height : FallbackHeight;
            using Bitmap? bitmap = canvas.DrawToBitmap(width: width, height: height, drawBackground: true, drawWires: true, drawMessages: true);
            if (bitmap is null) {
                return Fin.Fail<CaptureFile>(error: Error.New(message: "Canvas.DrawToBitmap returned null — GH2 swallowed a paint exception"));
            }
            File.WriteAllBytes(path: path, bytes: bitmap.ToByteArray(ImageFormat.Png));
            return Fin.Succ(value: new CaptureFile(Path: path, Width: width, Height: height));
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            return Fin.Fail<CaptureFile>(error: Error.New(message: $"canvas capture failed: {error.GetType().Name}: {error.Message}"));
        }
    }

    public void Dispose() => editor = null;  // the host owns the editor singleton; the lane drops its reference only
}
