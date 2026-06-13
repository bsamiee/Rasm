using System.Collections.Immutable;

namespace Rasm.Csp.Kernel;

// --- [CONSTANTS] -------------------------------------------------------------------------

// Rule vocabulary as code: DocumentationCommentId rows resolved per compilation by CompilationFacts
// (unresolvable rows are inert). Section -> rule: mutable-collections=CSP0011, time=CSP0911,
// ambient-state=CSP0723, admission-gate + admissible-types=CSP0742; prefixes=CSP0708.
internal static class Vocabulary {
    public static readonly ImmutableArray<string> Prefixes = ["Get", "TryGet", "GetOr"];

    public static readonly ImmutableDictionary<string, ImmutableArray<string>> BannedSections =
        ImmutableDictionary.CreateRange(StringComparer.OrdinalIgnoreCase, [
            new KeyValuePair<string, ImmutableArray<string>>("mutable-collections", [
                "T:System.Collections.Generic.List`1",
                "T:System.Collections.Generic.Dictionary`2",
                "T:System.Collections.Generic.HashSet`1",
                "T:System.Collections.Generic.Queue`1",
                "T:System.Collections.Generic.Stack`1",
                "T:System.Collections.Generic.SortedDictionary`2",
                "T:System.Collections.Generic.SortedSet`1",
                "T:System.Collections.Concurrent.ConcurrentDictionary`2",
                "T:System.Collections.Concurrent.ConcurrentQueue`1",
                "T:System.Collections.Concurrent.ConcurrentStack`1",
                "T:System.Collections.Concurrent.ConcurrentBag`1",
                "T:System.Collections.Concurrent.BlockingCollection`1",
            ]),
            new KeyValuePair<string, ImmutableArray<string>>("time", [
                "P:System.DateTime.Now",
                "P:System.DateTime.UtcNow",
                "P:System.DateTime.Today",
                "P:System.DateTimeOffset.Now",
                "P:System.DateTimeOffset.UtcNow",
                "M:System.Diagnostics.Stopwatch.StartNew",
                "T:System.Timers.Timer",
                "T:System.Threading.Timer",
            ]),
            new KeyValuePair<string, ImmutableArray<string>>("ambient-state", [
                "P:Rhino.RhinoDoc.ActiveDoc",
                "T:Rhino.RhinoApp",
            ]),
            new KeyValuePair<string, ImmutableArray<string>>("admission-gate", [
                "T:Rasm.Domain.OpAcceptance",
            ]),
            new KeyValuePair<string, ImmutableArray<string>>("admissible-types", [
                "T:Rhino.Geometry.Arc",
                "T:Rhino.Geometry.BoundingBox",
                "T:Rhino.Geometry.Box",
                "T:Rhino.Geometry.Circle",
                "T:Rhino.Geometry.Cone",
                "T:Rhino.Geometry.Cylinder",
                "T:Rhino.Geometry.Ellipse",
                "T:Rhino.Geometry.Interval",
                "T:Rhino.Geometry.Line",
                "T:Rhino.Geometry.Plane",
                "T:Rhino.Geometry.Point2d",
                "T:Rhino.Geometry.Point3d",
                "T:Rhino.Geometry.Polyline",
                "T:Rhino.Geometry.Rectangle3d",
                "T:Rhino.Geometry.Sphere",
                "T:Rhino.Geometry.Torus",
                "T:Rhino.Geometry.Transform",
                "T:Rhino.Geometry.Vector3d",
                "T:Rasm.Domain.ClosestHit",
                "T:Rasm.Analysis.IntersectionHit",
                "T:Rasm.Analysis.RayQuery",
            ]),
        ]);
}
