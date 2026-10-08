using System.Drawing;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation.Styles;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct BoundaryWidthScale : System.Numerics.IMinMaxValue<BoundaryWidthScale> {
    public static BoundaryWidthScale MinValue { get; } = new(double.BitIncrement(0.0));
    public static BoundaryWidthScale MaxValue { get; } = new(double.MaxValue);
    public static BoundaryWidthScale Unit { get; } = new(1.0);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

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

public sealed record SectionBoundary(Option<Color> Color, Option<Color> PrintColor, BoundaryWidthScale WidthScale, Option<PlotWeight> PrintWidth, Option<BoundaryLinetype> Linetype) {
    internal IO<Unit> Written(RhinoDoc doc, SectionStyle staged) =>
        from cleared in IO.lift(staged.RemoveBoundaryLinetype)
        from linetype in Linetype.Traverse(held => held.Written(doc, staged)).As()
        from settings in IO.lift(() => SectionStyleMapper.Update((Color, PrintColor, WidthScale, PrintWidth, linetype.IfNone(RhinoMath.UnsetIntIndex)), staged))
        select settings;
}

public sealed record SectionHatch(ComponentRef<HatchPattern> Pattern, HatchScale Scale, double Rotation, Option<Color> Color, Option<Color> PrintColor, Option<PlotWeight> PrintWidth);

public sealed record SectionStyleDefinition(ObjectSectionFillRule SectionFillRule, SectionFill Fill, Option<SectionBoundary> Boundary, Option<SectionHatch> Hatch);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class SectionStyleMapper {
    // --- [UPDATES]
    internal static partial void Update((ObjectSectionFillRule SectionFillRule, bool BoundaryVisible, int HatchIndex, SectionBackgroundFillMode BackgroundFillMode) settings, SectionStyle staged);

    internal static partial void Update((Option<Color> BackgroundFillColor, Option<Color> BackgroundFillPrintColor) fill, SectionStyle staged);

    [MapProperty(nameof(SectionStyle.BoundaryPlotWeightMillimeters), nameof(SectionStyle.BoundaryPlotWeightMillimeters), Use = nameof(@PlotWeight.ToInheritable))]
    internal static partial void Update((Option<Color> BoundaryColor, Option<Color> BoundaryPrintColor, BoundaryWidthScale BoundaryWidthScale, Option<PlotWeight> BoundaryPlotWeightMillimeters, int BoundaryLinetypeIndex) boundary, SectionStyle staged);

    [MapProperty(nameof(SectionStyle.HatchPatternPlotWeightMillimeters), nameof(SectionStyle.HatchPatternPlotWeightMillimeters), Use = nameof(@PlotWeight.ToInheritable))]
    internal static partial void Update((HatchScale HatchScale, double HatchRotationRadians, Option<Color> HatchPatternColor, Option<Color> HatchPatternPrintColor, Option<PlotWeight> HatchPatternPlotWeightMillimeters) hatch, SectionStyle staged);

    // --- [VALUES]
    [UserMapping]
    private static double Width(BoundaryWidthScale width) => width.ToValue();

    [UserMapping]
    private static double Scale(HatchScale scale) => scale.ToValue();
}

public static class SectionStyles {
    // --- [READS]
    public static IO<(SectionStyleDefinition Definition, bool BoundaryHasShapes)> Definition(SectionStyle style, Option<Seq<HatchPattern>> patterns = default) =>
        from visible in IO.lift(() => style.BoundaryVisible)
        from embedded in visible
            ? IO.lift(() => Optional(style.GetBoundaryLinetype())).Bracket(
                Use: static held => from definition in held.Traverse(Linetypes.Definition).As()
                                    select (Definition: definition, HasShapes: held.Exists(static linetype => linetype.HasShapes)), Fin: static held => DisposalOps.Release(held.ToSeq()))
            : IO.pure((Definition: Option<LinetypeDefinition>.None, HasShapes: false))
        let requested = Conversions.Present(style.HatchIndex)
        from pattern in IO.lift(patterns.Traverse(rows => requested.Traverse(index =>
            rows.At(index).ToFin(new InvalidAnswer(nameof(SectionStyle.ReadFromFile)))).As()).As())
        let address = pattern.Match(
            Some: static held => held.Map<ComponentRef<HatchPattern>>(static row => row.Name),
            None: () => requested.Map<ComponentRef<HatchPattern>>(static index => index))
        let linetype = embedded.Definition.Map<BoundaryLinetype>(static definition => new BoundaryLinetype.Embedded(definition))
            || (visible && patterns.IsNone ? LinetypeRef.FromHost(style.BoundaryLinetypeIndex).Map<BoundaryLinetype>(static reference => new BoundaryLinetype.Table(reference)) : None)
        let boundary = visible
            ? Conversions.Validated<BoundaryWidthScale, double, InvalidRhinoValue>(style.BoundaryWidthScale).Map(width => Some(new SectionBoundary(
                Conversions.Present(style.BoundaryColor), Conversions.Present(style.BoundaryPrintColor), width, PlotWeight.FromInheritable(style.BoundaryPlotWeightMillimeters), linetype)))
            : Fin.Succ(Option<SectionBoundary>.None)
        let hatch = address.Traverse(reference => Conversions.Validated<HatchScale, double, InvalidRhinoValue>(style.HatchScale).Map(scale => new SectionHatch(
            reference, scale, style.HatchRotationRadians, Conversions.Present(style.HatchPatternColor), Conversions.Present(style.HatchPatternPrintColor), PlotWeight.FromInheritable(style.HatchPatternPlotWeightMillimeters)))).As()
        from definition in IO.lift((boundary.ToValidation(), hatch.ToValidation()).Apply((edge, hatched) => new SectionStyleDefinition(
            style.SectionFillRule,
            style.BackgroundFillMode switch {
                SectionBackgroundFillMode.None => new SectionFill.NoFill(),
                SectionBackgroundFillMode.Viewport => new SectionFill.ViewportFill(),
                SectionBackgroundFillMode.SolidColor => new SectionFill.SolidFill(Conversions.Present(style.BackgroundFillColor), Conversions.Present(style.BackgroundFillPrintColor)),
            }, edge, hatched)).As().ToFin())
        select (Definition: definition, BoundaryHasShapes: embedded.HasShapes);

    public static IO<bool> Unchanged(SectionStyle live, SectionStyle staged) =>
        (Definition(live), Definition(staged)).Apply(static (held, wanted) => !held.BoundaryHasShapes && !wanted.BoundaryHasShapes && held.Definition == wanted.Definition).As();

    // --- [WRITES]
    public static TableKind<SectionStyle> References { get; } =
        TableKinds.SectionStyles with { Add = static (doc, row) => doc.SectionStyles.AddReferenceSectionStyle(row) };

    public static IO<Unit> Written(RhinoDoc doc, SectionStyle staged, SectionStyleDefinition definition) =>
        from pattern in TableOps.Index(doc.HatchPatterns, definition.Hatch.Map(static hatch => hatch.Pattern))
        from settings in IO.lift(() => SectionStyleMapper.Update((definition.SectionFillRule, definition.Boundary.IsSome, pattern, definition.Fill.Switch(
            noFill: static _ => SectionBackgroundFillMode.None,
            viewportFill: static _ => SectionBackgroundFillMode.Viewport,
            solidFill: static _ => SectionBackgroundFillMode.SolidColor)), staged))
        from filled in definition.Fill is SectionFill.SolidFill solid
            ? IO.lift(() => SectionStyleMapper.Update((solid.Color, solid.PrintColor), staged))
            : IO.pure(unit)
        from bordered in definition.Boundary.Traverse(boundary => boundary.Written(doc, staged)).As()
        from hatched in IO.lift(() => definition.Hatch.Iter(hatch => SectionStyleMapper.Update((hatch.Scale, hatch.Rotation, hatch.Color, hatch.PrintColor, hatch.PrintWidth), staged)))
        select unit;

    public static IO<int> Add(RhinoDoc doc, TableKind<SectionStyle> kind, string name, SectionStyleDefinition definition) =>
        TableOps.AddRow(doc, kind, staged => from named in TableOps.Named(staged, Some(name))
                                             from written in Written(doc, staged, definition)
                                             select unit);

    // --- [FILES]
    public static IO<(Seq<(string Name, SectionStyleDefinition Definition)> Styles, Seq<(string Name, HatchPatternDefinition Definition)> Patterns)> ReadFile(string path) =>
        from existing in IO.lift(() => Exchange.ExistingPath(path))
        from file in IO.lift(() => Refused.Unless(
                SectionStyle.ReadFromFile(existing, out SectionStyle[] styles, out HatchPattern[] patterns),
                (Styles: Conversions.Rows(styles), Patterns: Conversions.Rows(patterns)), nameof(SectionStyle.ReadFromFile)))
            .Bracket(
                Use: static read => (
                    read.Styles.TraverseM(style => from definition in Definition(style, Some(read.Patterns))
                                                   select (style.Name, definition.Definition)).As(),
                    read.Patterns.TraverseM(static pattern => from definition in HatchPatterns.Definition(pattern)
                                                              select (pattern.Name, Definition: definition)).As())
                    .Apply(static (styles, patterns) => (Styles: styles, Patterns: patterns)).As(),
                Fin: static read => DisposalOps.Release<IDisposable>([.. read.Styles, .. read.Patterns]))
        select file;

    public static IO<Committed<Seq<(string Name, int Index)>>> Import(RhinoDoc doc, string path, string name, RedrawPolicy redraw) =>
        from file in ReadFile(path)
        from landed in Commits.Commit(doc, name, redraw,
            from patternRows in TableOps.Rows(doc.HatchPatterns)
            from styleRows in TableOps.Rows(doc.SectionStyles)
            from plans in IO.lift((TableOps.Plan(patternRows, new TableSpec<HatchPatternDefinition>(file.Patterns, Exclusive: false)), TableOps.Plan(styleRows, new TableSpec<SectionStyleDefinition>(file.Styles, Exclusive: false)))
                .Apply(static (patterns, styles) => (Patterns: patterns, Styles: styles)).As().ToFin())
            from patterns in TableOps.Upsert(doc, TableKinds.HatchPatterns, plans.Patterns.Upserts.Filter(static upsert => upsert.Existing.IsNone), HatchPatterns.Written, HatchPatterns.Unchanged)
            from styles in TableOps.Upsert(doc, TableKinds.SectionStyles, plans.Styles.Upserts, (staged, definition) => Written(doc, staged, definition), Unchanged)
            select styles)
        select landed;

    // --- [USAGE]
    public static IO<(bool InUse, int InstanceDefinitions, int Objects, int Layers)> Usage(RhinoDoc doc, ComponentRef<SectionStyle> address) =>
        TableOps.Find(doc.SectionStyles, address, includeDeleted: false)
            .Map(row => (InUse: doc.SectionStyles.InUse(row.Index, out int definitions, out int objects, out int layers), InstanceDefinitions: definitions, Objects: objects, Layers: layers));
}
