using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.UI;
using Wacton.Unicolour;

namespace Rasm.Rhino.Display;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate double VertexMeasure(RhinoObject obj, int meshIndex, Mesh mesh, int vertex);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FalseColorScale(Interval Range, Seq<ColorStop> Stops) {
    // --- [TABLE]
    public Option<Func<double, Color>> Tabulate() =>
        Stops.OrderBy(static stop => stop.Position).ToArray() switch {
            [] => None,
            var ordered => ordered.Select(static stop => stop.Position).ToArray() switch {
                var positions => Enumerable.Range(0, byte.MaxValue + 1).Select(texel => Sampled(ordered, positions, texel / (double)byte.MaxValue)).ToArray() switch {
                    var table => Some<Func<double, Color>>(value =>
                        table[(int)Math.Round(double.Clamp(Range.NormalizedParameterAt(value), 0d, 1d) * byte.MaxValue, MidpointRounding.ToEven)]),
                },
            },
        };

    // --- [LEGEND]
    public Option<UserInterfaceGrid> Legend(string heading, int rows, Func<double, string> label) =>
        Tabulate().Map(color => ViewportControls.Grid(Some(heading), toSeq(Enumerable.Range(0, int.Max(rows, 2)))
            .Map(row => Range.ParameterAt(1d - (row / (double)(int.Max(rows, 2) - 1))))
            .Map(value => (label(value), (UserInterfaceControl)Swatch(color(value))))));

    private static UserInterfaceIcon Swatch(Color color) {
        UserInterfaceIcon swatch = new();
        swatch.SetSvg($$"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1 1"><rect width="1" height="1" fill="#{{color.R:X2}}{{color.G:X2}}{{color.B:X2}}"/></svg>""");
        return swatch;
    }

    private static Color Sampled(ColorStop[] ordered, double[] positions, double at) =>
        System.Array.BinarySearch(positions, at) switch {
            var found => (found >= 0 ? found : ~found) switch {
                var high => (Low: ordered[int.Clamp(high - 1, 0, ordered.Length - 1)], High: ordered[int.Min(high, ordered.Length - 1)]) switch {
                    var (low, upper) => Painted(Colour(low.Color).Mix(Colour(upper.Color), ColourSpace.Oklab,
                        upper.Position > low.Position ? double.Clamp((at - low.Position) / (upper.Position - low.Position), 0d, 1d) : 0d)),
                },
            },
        };

    private static Unicolour Colour(Color color) => new(ColourSpace.Rgb255, color.R, color.G, color.B, color.A / (double)byte.MaxValue);

    private static Color Painted(Unicolour mixed) =>
        mixed.MapToRgbGamut() switch {
            var mapped => mapped.Rgb.Byte255 switch {
                var rgb => Color.FromArgb(mapped.Alpha.A255, rgb.R, rgb.G, rgb.B),
            },
        };
}

[Union]
public abstract partial record AnalysisDefinition {
    // --- [DEFINITION]
    private AnalysisDefinition(LocalizeStringPair name, VisualAnalysisMode.AnalysisStyle style) => (Name, Style) = (name, style);

    public LocalizeStringPair Name { get; }
    public VisualAnalysisMode.AnalysisStyle Style { get; }
    public Option<Func<RhinoObject, bool>> Supports { get; init; }
    public Option<bool> ShowIsoCurves { get; init; }
    public Option<Func<bool, IO<Unit>>> UserInterface { get; init; }

    // --- [STYLES]
    public sealed record FalseColor(LocalizeStringPair Name, VertexMeasure Measure, Func<FalseColorScale> Scale) : AnalysisDefinition(Name, VisualAnalysisMode.AnalysisStyle.FalseColor);

    public sealed record Texture(LocalizeStringPair Name, Func<RhinoObject, Action<DisplayPipelineAttributes>> SetUp) : AnalysisDefinition(Name, VisualAnalysisMode.AnalysisStyle.Texture);

    public sealed record Wireframe(LocalizeStringPair Name) : AnalysisDefinition(Name, VisualAnalysisMode.AnalysisStyle.Wireframe) {
        public Option<Action<RhinoObject, DisplayPipeline>> DrawObject { get; init; }
        public Option<Action<RhinoObject, GeometryBase, DisplayPipeline>> DrawGeometry { get; init; }
    }
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class CurvatureFit {
    // --- [FITS]
    public static readonly CurvatureFit Auto = new(
        IO.lift(static () => VisualAnalysisMode.CurvatureColorAutoRange()),
        static (meshes, state) => RhinoFault.Unless(CurvatureAnalysisSettings.CalculateCurvatureAutoRange(meshes, ref state), RhinoReason.Declined, meshes.Count).Map(_ => state));

    public static readonly CurvatureFit Max = new(
        IO.lift(static () => VisualAnalysisMode.CurvatureColorMaxRange()),
        static (meshes, state) => RhinoFault.Unless(CurvatureAnalysisSettings.CalculateCurvatureMaxRange(meshes, state), RhinoReason.Declined, meshes.Count).Map(_ => state));

    // --- [RANGES]
    public IO<Unit> Adjust { get; }

    public IO<CurvatureAnalysisSettingsState> Fit(Seq<Mesh> meshes, Option<CurvatureAnalysisSettings.CurvatureStyle> style = default) =>
        from state in IO.lift(CurvatureAnalysisSettings.GetCurrentState)
        from selected in IO.lift(() => style.Iter(value => state.Style = value))
        from fitted in IO.lift(() => Calculate(meshes, state))
        select fitted;

    [UseDelegateFromConstructor]
    private partial Fin<CurvatureAnalysisSettingsState> Calculate(Seq<Mesh> meshes, CurvatureAnalysisSettingsState state);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AnalysisModes {
    // --- [REGISTRATION]
    public static IO<Guid> Register<TMode>() where TMode : DefinedAnalysisMode<TMode>, IDefined<AnalysisDefinition>, new() =>
        Products.Of(static () => VisualAnalysisMode.Register(typeof(TMode))).Map(static mode => mode.Id);

    // --- [OBJECTS]
    public static IO<Unit> Enable(RhinoDoc doc, Seq<RhinoObject> objects, Guid mode, bool enabled, RedrawPolicy redraw) =>
        from found in Products.Of(() => VisualAnalysisMode.Find(mode))
        from shown in Commits.WithinRedraw(doc, redraw, IO.lift(() => Products.Each(objects, (obj, index) =>
            RhinoFault.Unless(obj.EnableVisualAnalysisMode(found, enabled), RhinoReason.Declined, obj.Id, index))))
        select unit;

    public static IO<Seq<Guid>> Refresh(RhinoDoc doc, Guid mode, RedrawPolicy redraw) =>
        from found in Products.Of(() => VisualAnalysisMode.Find(mode))
        from refreshed in Commits.WithinRedraw(doc, redraw, IO.lift(() =>
            Conversions.Rows(doc.Objects.GetObjectList(new ObjectEnumeratorSettings { HiddenObjects = true })).Filter(obj => obj.InVisualAnalysisMode(found)) switch {
                var shown => Products.Each(shown, (obj, index) => RhinoFault.Unless(obj.EnableVisualAnalysisMode(found, enable: false) && obj.EnableVisualAnalysisMode(found, enable: true),
                    RhinoReason.Declined, obj.Id, index)).Map(_ => shown.Map(static obj => obj.Id).Strict()),
            }))
        select refreshed;
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class DefinedAnalysisMode<TSelf> : VisualAnalysisMode where TSelf : DefinedAnalysisMode<TSelf>, IDefined<AnalysisDefinition> {
    // --- [STATE]
    private readonly AnalysisDefinition definition;
    private readonly Option<Action<RhinoObject, DisplayPipeline>> drawObject;
    private readonly Option<Action<RhinoObject, GeometryBase, DisplayPipeline>> drawGeometry;
    private readonly Atom<Option<(FalseColorScale Scale, Func<double, Color> Color)>> table = Atom(Option<(FalseColorScale, Func<double, Color>)>.None);

    protected DefinedAnalysisMode() {
        definition = TSelf.Definition;
        Option<AnalysisDefinition.Wireframe> wires = Optional(definition as AnalysisDefinition.Wireframe);
        (drawObject, drawGeometry) = (wires.Bind(static held => held.DrawObject), wires.Bind(static held => held.DrawGeometry));
    }

    // --- [DEFINITION]
    public sealed override string Name => definition.Name.Local;
    public sealed override AnalysisStyle Style => definition.Style;
    public sealed override bool ShowIsoCurves => definition.ShowIsoCurves.IfNone(base.ShowIsoCurves);

    public sealed override void EnableUserInterface(bool on) => _ = definition.UserInterface.Iter(show => Session.Run(show(on)));

    public sealed override bool ObjectSupportsAnalysisMode(RhinoObject obj) => definition.Supports.Match(Some: supports => supports(obj), None: () => base.ObjectSupportsAnalysisMode(obj));

    // --- [APPEARANCE]
    protected sealed override void SetUpDisplayAttributes(RhinoObject obj, DisplayPipelineAttributes attributes) =>
        definition.Switch(
            (Object: obj, Attributes: attributes),
            falseColor: static (input, _) => input.Attributes.ShadeVertexColors = true,
            texture: static (input, texture) => texture.SetUp(input.Object)(input.Attributes),
            wireframe: static (_, _) => { });

    protected sealed override void UpdateVertexColors(RhinoObject obj, Mesh[] meshes) =>
        _ = Optional(definition as AnalysisDefinition.FalseColor).Bind(Colored).Iter(painted => Painted(painted.Color, painted.Measure, obj, meshes));

    private Option<(VertexMeasure Measure, Func<double, Color> Color)> Colored(AnalysisDefinition.FalseColor falseColor) =>
        falseColor.Scale() switch {
            var scale => table.Value.Filter(held => held.Scale == scale).Match(
                    Some: static held => Some(held.Color),
                    None: () => scale.Tabulate().Do(color => table.Swap(_ => Some((scale, color)))))
                .Map(color => (falseColor.Measure, color)),
        };

    private static void Painted(Func<double, Color> color, VertexMeasure measure, RhinoObject obj, Mesh[] meshes) {
        for (int index = 0; index < meshes.Length; index++) {
            if (meshes[index] is not { } mesh)
                continue;
            Color[] colors = new Color[mesh.Vertices.Count];
            for (int vertex = 0; vertex < colors.Length; vertex++)
                colors[vertex] = color(measure(obj, index, mesh, vertex));
            _ = mesh.VertexColors.SetColors(colors);
        }
    }

    // --- [DRAWING]
    protected sealed override void DrawCurveObject(CurveObject curve, DisplayPipeline pipeline) {
        if (drawObject.Case is Action<RhinoObject, DisplayPipeline> draw)
            draw(curve, pipeline);
        else
            base.DrawCurveObject(curve, pipeline);
    }

    protected sealed override void DrawBrepObject(BrepObject brep, DisplayPipeline pipeline) => Drawn(brep, pipeline);

    protected sealed override void DrawExtrusionObject(ExtrusionObject extrusion, DisplayPipeline pipeline) => Drawn(extrusion, pipeline);

    protected sealed override void DrawMeshObject(MeshObject mesh, DisplayPipeline pipeline) => Drawn(mesh, pipeline);

    protected sealed override void DrawSubDObject(SubDObject subd, DisplayPipeline pipeline) => Drawn(subd, pipeline);

    protected sealed override void DrawPointObject(PointObject point, DisplayPipeline pipeline) => Drawn(point, pipeline);

    protected sealed override void DrawPointCloudObject(PointCloudObject pointCloud, DisplayPipeline pipeline) => Drawn(pointCloud, pipeline);

    protected sealed override void DrawMesh(RhinoObject obj, Mesh mesh, DisplayPipeline pipeline) => Drawn(obj, mesh, pipeline);

    protected sealed override void DrawNurbsCurve(RhinoObject obj, NurbsCurve curve, DisplayPipeline pipeline) => Drawn(obj, curve, pipeline);

    protected sealed override void DrawNurbsSurface(RhinoObject obj, NurbsSurface surface, DisplayPipeline pipeline) => Drawn(obj, surface, pipeline);

    private void Drawn(RhinoObject obj, DisplayPipeline pipeline) {
        if (drawObject.Case is Action<RhinoObject, DisplayPipeline> draw)
            draw(obj, pipeline);
    }

    private void Drawn(RhinoObject obj, GeometryBase geometry, DisplayPipeline pipeline) {
        if (drawGeometry.Case is Action<RhinoObject, GeometryBase, DisplayPipeline> draw)
            draw(obj, geometry, pipeline);
    }
}
