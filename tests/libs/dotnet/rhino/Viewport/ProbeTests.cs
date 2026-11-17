using System.Drawing;
using System.Globalization;
using Rasm.Testing;
using Rhino;
using Rhino.Display;
using Xunit;

namespace Rasm.Rhino.Tests.Viewport;

public sealed class ProbeTests(Fixtures fixtures) {
    [Fact]
    public async Task AnimatedRestoreAcrossProjections() {
        RhinoDoc doc = await fixtures.TakeAsync(RhinoHost.Document);
        string report = await RhinoHost.RunAsync(IO.lift(() => {
            RhinoView view = doc.Views.Add("Probe", DefinedViewportProjection.Perspective, new Rectangle(0, 0, 600, 400), floating: true);
            RhinoViewport vp = view.ActiveViewport;
            doc.Objects.AddSphere(new Sphere(Point3d.Origin, 5));
            System.Collections.Generic.List<string> lines = [];
            void Set(string kind, Point3d target, Point3d camera) {
                vp.SetCameraLocations(target, camera);
                _ = kind switch {
                    "parallel" => vp.ChangeToParallelProjection(symmetricFrustum: true),
                    "perspective" => vp.ChangeToPerspectiveProjection(symmetricFrustum: true, lensLength: 50),
                    _ => vp.ChangeToTwoPointPerspectiveProjection(lensLength: 50),
                };
                vp.SetCameraLocations(target, camera);
            }
            string State(RhinoViewport v) {
                v.GetFrustum(out double l, out double r, out double b, out double t, out double n, out double f);
                string kind = v.IsParallelProjection ? "parallel" : v.IsTwoPointPerspectiveProjection ? "twoPoint" : "perspective";
                double d = v.CameraLocation.DistanceTo(v.CameraTarget);
                double widthAtTarget = v.IsParallelProjection ? r - l : (r - l) * d / n;
                return string.Create(CultureInfo.InvariantCulture, $"{kind,-11} lens={v.Camera35mmLensLength:F2} cam=({v.CameraLocation.X:F2},{v.CameraLocation.Y:F2},{v.CameraLocation.Z:F2}) tgt=({v.CameraTarget.X:F2},{v.CameraTarget.Y:F2},{v.CameraTarget.Z:F2}) dist={d:F2} widthAtTarget={widthAtTarget:F2} up=({v.CameraUp.X:F2},{v.CameraUp.Y:F2},{v.CameraUp.Z:F2})");
            }
            void Draw(object? sender, DrawEventArgs e) {
                if (e.Viewport.Id == vp.Id)
                    lines.Add("  draw " + State(e.Viewport));
            }
            void Projected(object? sender, DrawEventArgs e) {
                if (e.Viewport.Id == vp.Id)
                    lines.Add("  projection " + State(e.Viewport));
            }
            void Box(object? sender, CalculateBoundingBoxEventArgs e) {
                if (e.Viewport.Id == vp.Id)
                    lines.Add("  box " + State(e.Viewport));
            }
            void Modified(object? sender, ViewEventArgs e) {
                if (e.View.ActiveViewportID == vp.Id)
                    lines.Add("  modified " + State(e.View.ActiveViewport));
            }
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection = true;
            foreach ((string from, string to) in new[] { ("perspective", "perspective"), ("parallel", "parallel"), ("parallel", "perspective"), ("perspective", "parallel"), ("perspective", "twoPoint"), ("twoPoint", "parallel") }) {
                Set(to, new Point3d(2, 1, 0), new Point3d(-20, -40, 10));
                int index = doc.NamedViews.Add(from + "-" + to, vp.Id);
                Set(from, Point3d.Origin, new Point3d(30, -30, 20));
                lines.Add($"{from} -> {to}: start {State(vp)}");
                DisplayPipeline.DrawForeground += Draw;
                DisplayPipeline.ViewportProjectionChanged += Projected;
                DisplayPipeline.CalculateBoundingBox += Box;
                RhinoView.Modified += Modified;
                bool ok = doc.NamedViews.RestoreAnimatedConstantTime(index, vp, 8, 0);
                DisplayPipeline.DrawForeground -= Draw;
                DisplayPipeline.ViewportProjectionChanged -= Projected;
                DisplayPipeline.CalculateBoundingBox -= Box;
                RhinoView.Modified -= Modified;
                lines.Add($"  restored={ok} end {State(vp)}");
            }
            return string.Join('\n', lines);
        }).OnUiThread());
        Assert.Fail(report);
    }
}
