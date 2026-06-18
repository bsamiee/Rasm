using GhEditor = Grasshopper2.UI.Editor;

namespace Rasm.Bridge.Cargo;

// --- [MODELS] -------------------------------------------------------------------------------

// Ownership: scenarios and the runner exchange typed capture receipts, never raw GH2 graphs.
internal readonly record struct CaptureFile(string Path, int Width, int Height);

// --- [SERVICES] -----------------------------------------------------------------------------

// Ownership: the only Grasshopper2 type boundary. The render lane uses the editor constructor,
// never ShowEditor, so offscreen DrawToBitmap avoids NSView realization while keeping GH2 breakage
// local to this file.
internal sealed class Gh2Lane : IDisposable {
    private const int FallbackHeight = 720;
    private const int FallbackWidth = 1280;

    private GhEditor? editor;

    private Gh2Lane(GhEditor editor) => this.editor = editor;

    internal static Gh2Lane Acquire() => new(editor: GhEditor.Instance ?? new GhEditor());

    internal Fin<CaptureFile> DrawCanvas(string path) {
        // BOUNDARY ADAPTER: GH2 paint failures and null bitmaps stay on the typed rail.
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
            _ = Directory.CreateDirectory(path: Path.GetDirectoryName(path: path) ?? ".");
            string temp = path + ".tmp";
            File.WriteAllBytes(path: temp, bytes: bitmap.ToByteArray(ImageFormat.Png));
            File.Move(sourceFileName: temp, destFileName: path, overwrite: true);
            return Fin.Succ(value: new CaptureFile(Path: path, Width: width, Height: height));
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            return Fin.Fail<CaptureFile>(error: Error.New(message: $"canvas capture failed: {error.GetType().Name}: {error.Message}"));
        }
    }

    public void Dispose() => editor = null;
}
