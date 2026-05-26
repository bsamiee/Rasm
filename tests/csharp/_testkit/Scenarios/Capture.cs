using Rasm.RhinoBridge.Protocol;
using Rhino;
using Rhino.Display;

namespace Rasm.TestKit.Scenarios;

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Capture {
    public static void Redraw(RhinoDoc doc) {
        ArgumentNullException.ThrowIfNull(argument: doc);
        doc.Views.Redraw();
    }
    public static void SetActiveView(RhinoDoc doc, RhinoView view) {
        ArgumentNullException.ThrowIfNull(argument: doc);
        ArgumentNullException.ThrowIfNull(argument: view);
        doc.Views.ActiveView = view;
    }
    public static Fin<Unit> Snapshot(string capturePath, int width = 1024, int height = 768) =>
        (RhinoDoc.ActiveDoc?.Views.ActiveView, capturePath) switch {
            (null, _) => Fin.Fail<Unit>(error: Error.New(message: "Capture.Snapshot: no active Rhino view available.")),
            (_, null or "") => Fin.Fail<Unit>(error: Error.New(message: "Capture.Snapshot: capturePath was empty.")),
            (RhinoView view, string path) => Write(view: view, path: path, width: width, height: height),
        };
    private static Fin<Unit> Write(RhinoView view, string path, int width, int height) {
        try {
            using ViewCaptureSettings settings = new(sourceView: view, mediaSize: new System.Drawing.Size(width: width, height: height), dpi: 96);
            using System.Drawing.Bitmap? bitmap = ViewCapture.CaptureToBitmap(settings: settings);
            if (bitmap is null) {
                return Fin.Fail<Unit>(error: Error.New(message: "Capture.Snapshot: ViewCapture.CaptureToBitmap returned null."));
            }
            _ = Directory.CreateDirectory(path: Path.GetDirectoryName(path: path) ?? Directory.GetCurrentDirectory());
            bitmap.Save(filename: path, format: System.Drawing.Imaging.ImageFormat.Png);
            BridgeMarker.EmitCapture(path: path, width: width, height: height);
            return Fin.Succ(value: unit);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException) {
            return Fin.Fail<Unit>(error: Error.New(message: $"Capture.Snapshot failed: {error.Message}"));
        }
    }
}
