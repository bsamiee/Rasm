using System.Drawing;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Annotation.Styles;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SectionFill {
    public sealed record NoFill() : SectionFill;
    public sealed record ViewportFill() : SectionFill;
    public sealed record SolidFill(Option<Color> Color, Option<Color> PrintColor) : SectionFill;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record BoundaryLinetype {
    // --- [CASES]
    public sealed record Table(LinetypeRef Reference) : BoundaryLinetype;
    public sealed record Embedded(LinetypeDefinition Definition) : BoundaryLinetype;

    // --- [WRITES]
    internal IO<int> Written(RhinoDoc doc, SectionStyle staged) =>
        Switch(
            table: table => table.Reference.Resolve(doc),
            embedded: embedded => (from native in use(static () => new Linetype())
                                   from written in Linetypes.Written(native, embedded.Definition)
                                   from copied in IO.lift(() => staged.SetBoundaryLinetype(native))
                                   select RhinoMath.UnsetIntIndex).Bracket());
}

public sealed record SectionBoundary(Option<Color> Color, Option<Color> PrintColor, double WidthScale, Option<PlotWeight> PrintWidth, Option<BoundaryLinetype> Linetype) {
    internal IO<Unit> Written(RhinoDoc doc, SectionStyle staged) =>
        from cleared in IO.lift(staged.RemoveBoundaryLinetype)
        from linetype in Linetype.Traverse(held => held.Written(doc, staged)).As()
        from settings in IO.lift(() => {
            (staged.BoundaryColor, staged.BoundaryPrintColor, staged.BoundaryWidthScale, staged.BoundaryPlotWeightMillimeters, staged.BoundaryLinetypeIndex) =
                (Conversions.Unset(Color), Conversions.Unset(PrintColor), WidthScale, PlotWeight.ToInheritable(PrintWidth), linetype.IfNone(RhinoMath.UnsetIntIndex));
        })
        select settings;
}

public sealed record SectionHatch(ComponentRef<HatchPattern> Pattern, double Scale, double Rotation, Option<Color> Color, Option<Color> PrintColor, Option<PlotWeight> PrintWidth) {
    internal Unit Written(SectionStyle staged) {
        (staged.HatchScale, staged.HatchRotationRadians, staged.HatchPatternColor, staged.HatchPatternPrintColor, staged.HatchPatternPlotWeightMillimeters) =
            (Scale, Rotation, Conversions.Unset(Color), Conversions.Unset(PrintColor), PlotWeight.ToInheritable(PrintWidth));
        return unit;
    }
}

public sealed record SectionStyleDefinition(ObjectSectionFillRule SectionFillRule, SectionFill Fill, Option<SectionBoundary> Boundary, Option<SectionHatch> Hatch);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SectionStyles {
    // --- [READS]
    public static IO<(SectionStyleDefinition Definition, bool BoundaryHasShapes)> Definition(SectionStyle style, Option<Seq<HatchPattern>> patterns = default) =>
        from visible in IO.lift(() => style.BoundaryVisible)
        from embedded in visible
            ? (from held in Products.Held(style.GetBoundaryLinetype())
               from definition in held.Traverse(Linetypes.Definition).As()
               select (Definition: definition, HasShapes: held.Exists(static linetype => linetype.HasShapes))).Bracket()
            : IO.pure((Definition: Option<LinetypeDefinition>.None, HasShapes: false))
        let requested = Conversions.Present(style.HatchIndex)
        from pattern in IO.lift(patterns.Traverse(rows => requested.Traverse(index =>
            Conversions.Within(index, rows.Count).Map(_ => rows[index])).As()).As())
        let address = pattern.Match(
            Some: static held => held.Map<ComponentRef<HatchPattern>>(static row => row.Name),
            None: () => requested.Map<ComponentRef<HatchPattern>>(static index => index))
        let linetype = embedded.Definition.Map<BoundaryLinetype>(static definition => new BoundaryLinetype.Embedded(definition))
            || (visible && patterns.IsNone ? LinetypeRef.FromHost(style.BoundaryLinetypeIndex).Map<BoundaryLinetype>(static reference => new BoundaryLinetype.Table(reference)) : None)
        let boundary = Conversions.Found(visible, new SectionBoundary(
            Conversions.Present(style.BoundaryColor), Conversions.Present(style.BoundaryPrintColor), style.BoundaryWidthScale, PlotWeight.FromInheritable(style.BoundaryPlotWeightMillimeters), linetype))
        let hatch = address.Map(reference => new SectionHatch(
            reference, style.HatchScale, style.HatchRotationRadians, Conversions.Present(style.HatchPatternColor), Conversions.Present(style.HatchPatternPrintColor), PlotWeight.FromInheritable(style.HatchPatternPlotWeightMillimeters)))
        select (Definition: new SectionStyleDefinition(
            style.SectionFillRule,
            style.BackgroundFillMode switch {
                SectionBackgroundFillMode.None => new SectionFill.NoFill(),
                SectionBackgroundFillMode.Viewport => new SectionFill.ViewportFill(),
                SectionBackgroundFillMode.SolidColor => new SectionFill.SolidFill(Conversions.Present(style.BackgroundFillColor), Conversions.Present(style.BackgroundFillPrintColor)),
            }, boundary, hatch), BoundaryHasShapes: embedded.HasShapes);

    public static IO<bool> Unchanged(SectionStyle live, SectionStyle staged) =>
        (Definition(live), Definition(staged)).Apply(static (held, wanted) => !held.BoundaryHasShapes && !wanted.BoundaryHasShapes && held.Definition == wanted.Definition).As();

    // --- [WRITES]
    public static TableKind<SectionStyle> References { get; } =
        TableKinds.SectionStyles with { Add = static (doc, row) => doc.SectionStyles.AddReferenceSectionStyle(row) };

    public static IO<Unit> Written(RhinoDoc doc, SectionStyle staged, SectionStyleDefinition definition) =>
        from pattern in TableOps.Index(doc.HatchPatterns, definition.Hatch.Map(static hatch => hatch.Pattern))
        from settings in IO.lift(() => {
            (staged.SectionFillRule, staged.BoundaryVisible, staged.HatchIndex, staged.BackgroundFillMode) = (definition.SectionFillRule, definition.Boundary.IsSome, pattern, definition.Fill.Switch(
                noFill: static _ => SectionBackgroundFillMode.None,
                viewportFill: static _ => SectionBackgroundFillMode.Viewport,
                solidFill: static _ => SectionBackgroundFillMode.SolidColor));
        })
        from filled in IO.lift(() => definition.Fill.Switch(
            staged,
            noFill: static (_, _) => { },
            viewportFill: static (_, _) => { },
            solidFill: static (target, solid) => (target.BackgroundFillColor, target.BackgroundFillPrintColor) = (Conversions.Unset(solid.Color), Conversions.Unset(solid.PrintColor))))
        from bordered in definition.Boundary.Traverse(boundary => boundary.Written(doc, staged)).As()
        from hatched in IO.lift(() => definition.Hatch.Iter(hatch => hatch.Written(staged)))
        select unit;

    public static IO<int> Add(RhinoDoc doc, TableKind<SectionStyle> kind, string name, SectionStyleDefinition definition) =>
        TableOps.AddRow(doc, kind, staged => from named in TableOps.Named(staged, Some(name))
                                             from written in Written(doc, staged, definition)
                                             select unit);

    // --- [FILES]
    public static IO<(Seq<(string Name, SectionStyleDefinition Definition)> Styles, Seq<(string Name, HatchPatternDefinition Definition)> Patterns)> ReadFile(InputPath path) =>
        (from native in IO.lift(() => {
            bool read = SectionStyle.ReadFromFile(path, out SectionStyle[] styles, out HatchPattern[] patterns);
            return (Read: read, Styles: styles, Patterns: patterns);
        })
         from read in IO.lift(() => RhinoFault.Unless(native.Read))
         from styles in Products.Made(native.Styles)
         from patterns in Products.Made(native.Patterns)
         from file in (
                 styles.TraverseM(style => from definition in Definition(style, Some(patterns))
                                           select (style.Name, definition.Definition)).As(),
                 patterns.TraverseM(static pattern => from definition in HatchPatterns.Definition(pattern)
                                                      select (pattern.Name, Definition: definition)).As())
             .Apply(static (styles, patterns) => (Styles: styles, Patterns: patterns)).As()
         select file).Bracket();

    public static IO<Committed<Seq<(string Name, int Index)>>> Import(RhinoDoc doc, InputPath path, string name, RedrawPolicy redraw) =>
        from file in ReadFile(path)
        from landed in Commits.Commit(doc, name, redraw,
            from patternRows in TableOps.Rows(doc.HatchPatterns)
            from styleRows in TableOps.Rows(doc.SectionStyles)
            from plans in IO.lift((TableOps.Plan(patternRows, new TableSpec<HatchPatternDefinition>(file.Patterns, Exclusive: false)), TableOps.Plan(styleRows, new TableSpec<SectionStyleDefinition>(file.Styles, Exclusive: false)))
                .Apply(static (patterns, styles) => (Patterns: patterns, Styles: styles)).As())
            from patterns in TableOps.Upsert(doc, TableKinds.HatchPatterns, plans.Patterns.Upserts.Filter(static upsert => upsert.Existing.IsNone), HatchPatterns.Written, HatchPatterns.Unchanged)
            from styles in TableOps.Upsert(doc, TableKinds.SectionStyles, plans.Styles.Upserts, (staged, definition) => Written(doc, staged, definition), Unchanged)
            select styles)
        select landed;

    // --- [USAGE]
    public static IO<(bool InUse, int InstanceDefinitions, int Objects, int Layers)> Usage(RhinoDoc doc, ComponentRef<SectionStyle> address) =>
        TableOps.Find(doc.SectionStyles, address, includeDeleted: false)
            .Map(row => (InUse: doc.SectionStyles.InUse(row.Index, out int definitions, out int objects, out int layers), InstanceDefinitions: definitions, Objects: objects, Layers: layers));
}
