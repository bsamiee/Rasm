using Rhino;

namespace Rasm.TestKit.Scenarios;

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class DocumentScope : IDisposable {
    private bool disposed;
    private DocumentScope(RhinoDoc? prior, bool ownedActive) {
        Prior = prior;
        OwnedActive = ownedActive;
    }
    public static DocumentScope Open() => new(prior: RhinoDoc.ActiveDoc, ownedActive: RhinoDoc.ActiveDoc is null);
    public RhinoDoc Active => RhinoDoc.ActiveDoc ?? Prior ?? throw new InvalidOperationException(message: "DocumentScope: no active Rhino document.");
    public bool OwnedActive { get; }
    public RhinoDoc? Prior { get; }
    public void Clear() {
        RhinoDoc active = Active;
        active.Objects.Clear();
        active.Views.Redraw();
    }
    public void Dispose() {
        if (disposed) {
            return;
        }
        disposed = true;
        if (OwnedActive && Prior is null && RhinoDoc.ActiveDoc is RhinoDoc lingering) {
            lingering.Objects.Clear();
        }
    }
}
