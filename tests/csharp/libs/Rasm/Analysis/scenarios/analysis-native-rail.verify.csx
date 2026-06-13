using System;
using Rasm.Analysis;
using Rasm.Domain;
using Rhino;
using Rhino.Geometry;
using TestProbe = Rasm.TestKit.Scenarios.Probe;

static T RunOne<TGeometry, T>(AnalysisQuery query, Analyze.Scope scope, TGeometry geometry, string label) where TGeometry : notnull where T : notnull =>
    scope.Run(operation: Analyze.Query<TGeometry, T>(query: query), input: new TGeometry[] { geometry }).Match(
        Succ: values => values[0],
        Fail: error => throw new InvalidOperationException(message: $"{label}: {error.Message}"));

Scenario.Run("analysis-native-rail", CAPTURE_PATH, (key, facts) => {
    Analyze.Scope scope = Analyze.In(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Millimeters);
    LineCurve segment = new(
        from: new Point3d(x: 0.0, y: 0.0, z: 0.0),
        to: new Point3d(x: 3.0, y: 4.0, z: 0.0));
    PolylineCurve polyline = new(new Polyline(new[] {
        new Point3d(x: 0.0, y: 0.0, z: 0.0),
        new Point3d(x: 3.0, y: 0.0, z: 0.0),
        new Point3d(x: 3.0, y: 4.0, z: 0.0),
    }));
    double segmentLength = RunOne<Curve, double>(query: AnalysisQuery.Measure(query: Measure.Length), scope: scope, geometry: segment, label: "segment length");
    double polylineLength = RunOne<Curve, double>(query: AnalysisQuery.Measure(query: Measure.Length), scope: scope, geometry: polyline, label: "polyline length");
    Point3d segmentMidpoint = RunOne<Curve, Point3d>(query: AnalysisQuery.Measure(query: Measure.SpatialMidpoint), scope: scope, geometry: segment, label: "segment midpoint");
    Curve boundary = RunOne<Curve, Curve>(query: AnalysisQuery.Selection(query: Curves.Boundary), scope: scope, geometry: segment, label: "curve boundary");

    TestProbe.Require(condition: Math.Abs(segmentLength - 5.0) <= 1.0e-8, message: $"segment.length={segmentLength:R}");
    TestProbe.Require(condition: Math.Abs(polylineLength - 7.0) <= 1.0e-8, message: $"polyline.length={polylineLength:R}");
    TestProbe.Require(condition: segmentMidpoint.DistanceTo(new Point3d(x: 1.5, y: 2.0, z: 0.0)) <= 1.0e-8, message: $"midpoint={segmentMidpoint}");
    TestProbe.Require(condition: boundary.IsValid && Math.Abs(boundary.GetLength() - 5.0) <= 1.0e-8, message: $"boundary.length={boundary.GetLength():R}");
    facts.Add("segment.length", segmentLength.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
    facts.Add("polyline.length", polylineLength.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
    facts.Add("midpoint", segmentMidpoint.ToString());
    facts.Add("boundary.valid", boundary.IsValid);
});
