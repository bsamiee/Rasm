using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit.Scenarios;
using Rhino;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis.Scenarios;

// --- [OPERATIONS] ---------------------------------------------------------------------------

// Ownership: the Analysis theme — the native measure/selection rail through the public
// AnalysisQuery surface. Each projection runs one operation over one geometry and facts the
// scalar evidence before the Require gates decide the rail.
internal static class AnalysisScenarios {
    [RhinoScenario(theme: "analysis")]
    internal static Fin<Unit> NativeRail(ScenarioContext ctx) =>
        from op in Fin.Succ(value: Op.Of())
        let scope = Analyze.In(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Millimeters)
        let segment = new LineCurve(new Point3d(x: 0.0, y: 0.0, z: 0.0), new Point3d(x: 3.0, y: 4.0, z: 0.0))
        let polyline = new PolylineCurve(new Polyline([
            new Point3d(x: 0.0, y: 0.0, z: 0.0),
            new Point3d(x: 3.0, y: 0.0, z: 0.0),
            new Point3d(x: 3.0, y: 4.0, z: 0.0),
        ]))
        from segmentLength in RunOne<Curve, double>(ctx: ctx, scope: scope, query: AnalysisQuery.Measure(query: Measure.Length), geometry: segment, key: op, label: "segment length")
        from polylineLength in RunOne<Curve, double>(ctx: ctx, scope: scope, query: AnalysisQuery.Measure(query: Measure.Length), geometry: polyline, key: op, label: "polyline length")
        from segmentMidpoint in RunOne<Curve, Point3d>(ctx: ctx, scope: scope, query: AnalysisQuery.Measure(query: Measure.SpatialMidpoint), geometry: segment, key: op, label: "segment midpoint")
        from boundary in RunOne<Curve, Curve>(ctx: ctx, scope: scope, query: AnalysisQuery.Selection(query: Curves.Boundary), geometry: segment, key: op, label: "curve boundary")
        let lengthFact = Note(ctx: ctx, key: "segment.length", value: segmentLength.ToString(format: "F6", provider: System.Globalization.CultureInfo.InvariantCulture))
        let polylineFact = Note(ctx: ctx, key: "polyline.length", value: polylineLength.ToString(format: "F6", provider: System.Globalization.CultureInfo.InvariantCulture))
        let midpointFact = Note(ctx: ctx, key: "midpoint", value: Text(value: segmentMidpoint))
        let boundaryFact = Note(ctx: ctx, key: "boundary.valid", value: boundary.IsValid)
        from segmentLaw in ctx.Require(label: "segment length 5", observed: Math.Abs(value: segmentLength - 5.0) <= 1.0e-8)
        from polylineLaw in ctx.Require(label: "polyline length 7", observed: Math.Abs(value: polylineLength - 7.0) <= 1.0e-8)
        from midpointLaw in ctx.Require(label: "spatial midpoint", observed: segmentMidpoint.DistanceTo(other: new Point3d(x: 1.5, y: 2.0, z: 0.0)) <= 1.0e-8)
        from boundaryLaw in ctx.Require(label: "boundary length 5", observed: boundary.IsValid && Math.Abs(value: boundary.GetLength() - 5.0) <= 1.0e-8)
        select unit;

    private static T Note<T>(ScenarioContext ctx, string key, T value) {
        ctx.Fact(key: key, value: value);
        return value;
    }

    private static string Text(object? value) =>
        Convert.ToString(value: value, provider: System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

    private static Fin<TOut> RunOne<TGeometry, TOut>(ScenarioContext ctx, Analyze.Scope scope, AnalysisQuery query, TGeometry geometry, Op key, string label)
        where TGeometry : notnull where TOut : notnull =>
        ctx.Expect(label: label, projection: scope.Run(operation: Analyze.Query<TGeometry, TOut>(query: query, key: key), geometry).ToFin())
            .Bind(values => values.Head.ToFin(Fail: Error.New(message: $"{label}: empty result")));
}
