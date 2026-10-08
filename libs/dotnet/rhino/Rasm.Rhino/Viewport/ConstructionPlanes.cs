using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Viewport;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ConstructionPlanes {
    // --- [VIEWS]
    public static IO<ConstructionPlane> Read(ViewInfo view) =>
        IO.lift(() => {
            ConstructionPlane plane = view.GetConstructionPlane();
            plane.ShowZAxis = view.ShowConstructionZAxis;
            return plane;
        });

    public static IO<Unit> Write(ViewInfo view, ConstructionPlane plane) =>
        IO.lift(() => Written(view, plane));

    private static Unit Written(ViewInfo view, ConstructionPlane plane) {
        view.SetConstructionPlane(plane);
        view.ShowConstructionZAxis = plane.ShowZAxis;
        return unit;
    }

    // --- [VIEWPORTS]
    public static IO<ConstructionPlane> Read(RhinoViewport viewport) =>
        use(() => new ViewInfo(viewport)).Bind(Read).Bracket();

    public static Fin<Unit> Write(RhinoViewport viewport, ConstructionPlane plane) {
        viewport.SetConstructionPlane(plane);
        using ViewInfo view = new(viewport);
        return Written(view, plane);
    }
}
