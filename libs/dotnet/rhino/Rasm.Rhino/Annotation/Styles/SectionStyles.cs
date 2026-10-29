using System.Drawing;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Annotation.Styles;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SectionFill(SectionBackgroundFillMode Mode, Option<Color> Color = default, Option<Color> PrintColor = default);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record BoundaryLinetype {
    public sealed record Table(LinetypeRef Reference) : BoundaryLinetype;
    public sealed record Embedded(LinetypeDefinition Definition) : BoundaryLinetype;
}

public sealed record SectionBoundary(double WidthScale, Option<PlotWeight> PrintWidth, Option<BoundaryLinetype> Linetype) {
    public Option<Color> Color { get; init; }
    public Option<Color> PrintColor { get; init; }
}

public sealed record SectionHatch(ComponentRef<HatchPattern> Pattern, double Scale, UnitsNet.Angle Rotation, Option<PlotWeight> PrintWidth) {
    public Option<Color> Color { get; init; }
    public Option<Color> PrintColor { get; init; }
}

public sealed record SectionStyleDefinition(ObjectSectionFillRule SectionFillRule, SectionFill Fill, Option<SectionBoundary> Boundary, Option<SectionHatch> Hatch);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SectionStyles {
    // --- [READS]
    public static IO<(SectionStyleDefinition Definition, bool BoundaryHasShapes)> Definition(SectionStyle style, Option<Seq<HatchPattern>> patterns = default) =>
        from visible in IO.lift(() => style.BoundaryVisible)
        from embedded in visible
            ? (from held in Products.Maybe(style.GetBoundaryLinetype)
               from definition in held.Traverse(Linetypes.Definition).As()
               select (Definition: definition, HasShapes: held.Exists(static linetype => linetype.HasShapes))).Bracket()
            : IO.pure((Definition: Option<LinetypeDefinition>.None, HasShapes: false))
        let requested = Conversions.Present(style.HatchIndex)
        from pattern in IO.lift(patterns.Traverse(rows => requested.Traverse(index => Conversions.Within(index, rows.Count).Map(_ => rows[index])).As()).As())
        let address = pattern.Match(
            Some: static held => held.Map<ComponentRef<HatchPattern>>(static row => row.Name),
            None: () => requested.Map<ComponentRef<HatchPattern>>(static index => index))
        let linetype = embedded.Definition.Map<BoundaryLinetype>(static definition => new BoundaryLinetype.Embedded(definition))
            || (visible && patterns.IsNone ? LinetypeRef.FromHost(style.BoundaryLinetypeIndex).Map<BoundaryLinetype>(static reference => new BoundaryLinetype.Table(reference)) : None)
        let boundary = Conversions.Present(visible, new SectionBoundary(style.BoundaryWidthScale, PlotWeight.FromInheritable(style.BoundaryPlotWeightMillimeters), linetype) {
            Color = Conversions.Present(style.BoundaryColor), PrintColor = Conversions.Present(style.BoundaryPrintColor),
        })
        let hatch = address.Map(reference => new SectionHatch(reference, style.HatchScale, UnitsNet.Angle.FromRadians(Quantities.Exact(style.HatchRotationRadians)), PlotWeight.FromInheritable(style.HatchPatternPlotWeightMillimeters)) {
            Color = Conversions.Present(style.HatchPatternColor), PrintColor = Conversions.Present(style.HatchPatternPrintColor),
        })
        let fill = new SectionFill(style.BackgroundFillMode, Conversions.Present(style.BackgroundFillColor), Conversions.Present(style.BackgroundFillPrintColor))
        select (Definition: new SectionStyleDefinition(style.SectionFillRule, fill, boundary, hatch), BoundaryHasShapes: embedded.HasShapes);

    public static IO<bool> Unchanged(SectionStyle live, SectionStyle staged) =>
        (Definition(live), Definition(staged)).Apply(static (held, wanted) => !held.BoundaryHasShapes && !wanted.BoundaryHasShapes && held.Definition == wanted.Definition).As();

    // --- [WRITES]
    public static TableKind<SectionStyle> References { get; } =
        TableKinds.SectionStyles with { Add = static (doc, row) => doc.SectionStyles.AddReferenceSectionStyle(row) };

    public static IO<Unit> Written(RhinoDoc doc, SectionStyle staged, SectionStyleDefinition definition) =>
        from pattern in Rows.Index(doc.HatchPatterns, definition.Hatch.Map(static hatch => hatch.Pattern))
        from cleared in IO.lift(staged.RemoveBoundaryLinetype)
        from linetype in definition.Boundary.Bind(static boundary => boundary.Linetype).Traverse(held => Bounded(doc, staged, held)).As()
        from settings in IO.lift(() => {
            (staged.SectionFillRule, staged.BoundaryVisible, staged.HatchIndex) = (definition.SectionFillRule, definition.Boundary.IsSome, pattern);
            (staged.BackgroundFillMode, staged.BackgroundFillColor, staged.BackgroundFillPrintColor) =
                (definition.Fill.Mode, Conversions.Unset(definition.Fill.Color), Conversions.Unset(definition.Fill.PrintColor));
            _ = definition.Boundary.Iter(boundary =>
                (staged.BoundaryColor, staged.BoundaryPrintColor, staged.BoundaryWidthScale, staged.BoundaryPlotWeightMillimeters, staged.BoundaryLinetypeIndex) = (
                    Conversions.Unset(boundary.Color), Conversions.Unset(boundary.PrintColor), boundary.WidthScale, PlotWeight.ToInheritable(boundary.PrintWidth),
                    linetype.Flatten().IfNone(RhinoMath.UnsetIntIndex)));
            _ = definition.Hatch.Iter(hatch =>
                (staged.HatchScale, staged.HatchRotationRadians, staged.HatchPatternColor, staged.HatchPatternPrintColor, staged.HatchPatternPlotWeightMillimeters) = (
                    hatch.Scale, hatch.Rotation.Radians.ToDouble(), Conversions.Unset(hatch.Color), Conversions.Unset(hatch.PrintColor), PlotWeight.ToInheritable(hatch.PrintWidth)));
        })
        select unit;

    private static IO<Option<int>> Bounded(RhinoDoc doc, SectionStyle staged, BoundaryLinetype linetype) =>
        linetype.Switch(
            (Doc: doc, Staged: staged),
            table: static (state, table) => table.Reference.Resolve(state.Doc).Map(Some),
            embedded: static (state, embedded) =>
                (from native in use(static () => new Linetype())
                 from written in Linetypes.Written(native, embedded.Definition)
                 from copied in IO.lift(() => state.Staged.SetBoundaryLinetype(native))
                 select Option<int>.None).Bracket());

    public static IO<int> Add(RhinoDoc doc, TableKind<SectionStyle> kind, string name, SectionStyleDefinition definition) =>
        Rows.Add(doc, kind, Some(name), [Rows.Write<SectionStyle>(staged => Written(doc, staged, definition))]).Map(static row => row.Index);

    // --- [FILES]
    public static IO<StyleSpec> ReadFile(InputPath path) =>
        from native in IO.lift(() => (Read: SectionStyle.ReadFromFile(path, out SectionStyle[] styles, out HatchPattern[] patterns), Styles: styles, Patterns: patterns))
        from read in IO.lift(() => RhinoFault.Unless(native.Read, RhinoReason.Unreadable, path))
        from styles in Products.Many(() => native.Styles, required: false)
        from patterns in Products.Many(() => native.Patterns, required: false)
        from sectioned in styles.TraverseM(style => Definition(style, Some(patterns)).Map(definition => (style.Name, definition.Definition))).As()
        select new StyleSpec { SectionStyles = sectioned, HatchPatterns = patterns.Map(static pattern => (pattern.Name, (Func<HatchPattern>)(() => new HatchPattern(pattern)))) };

    public static IO<Committed<ReconciledStyles>> Import(RhinoDoc doc, InputPath path, string name, RedrawPolicy redraw) =>
        ReadFile(path).Bind(spec => StyleTables.Reconcile(doc, spec, name, redraw)).Bracket();

    // --- [USAGE]
    public static IO<(bool InUse, int InstanceDefinitions, int Objects, int Layers)> Usage(RhinoDoc doc, ComponentRef<SectionStyle> address) =>
        Rows.Find(doc.SectionStyles, address)
            .Map(row => (InUse: doc.SectionStyles.InUse(row.Index, out int definitions, out int objects, out int layers), InstanceDefinitions: definitions, Objects: objects, Layers: layers));
}
