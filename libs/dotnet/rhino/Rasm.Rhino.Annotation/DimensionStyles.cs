using Rasm.Rhino.Document;
using Rasm.Rhino.Objects;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;


namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimensionStyleSource {
    public sealed record BuiltIn(string Name) : DimensionStyleSource {
        public bool Names(DimensionStyle style) => TableOps.Names<DimensionStyle>().Equals(style.Name, Name);
    }

    public sealed record Row(ComponentRef Address) : DimensionStyleSource;
}

public sealed record DimensionStyleSpec(string Name, Option<DimensionStyleSource> Source, Option<FontQuery> Font, Func<DimensionStyle, IO<Unit>> Edit);

public sealed record DimensionStyleRow(
    Guid Id,
    int Index,
    Option<string> Name,
    Option<Guid> ParentId,
    bool IsChild,
    bool HasFieldOverrides,
    Seq<DimensionStyle.Field> Overridden,
    FontState Font,
    UnitSystem DimensionLengthDisplayUnit,
    UnitSystem AlternateDimensionLengthDisplayUnit,
    bool IsReference,
    bool IsDeleted,
    HashMap<string, string> UserStrings);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class DimensionStyles {
    // --- [READS]
    public static IO<DimensionStyleRow> Row(RhinoDoc doc, DimensionStyle style) =>
        from userStrings in GeometryOps.ReadUserStrings(GeometryOps.UserStrings(style))
        select Project(style, Fonts.State(style.Font), style.DimensionLengthDisplayUnit(doc.RuntimeSerialNumber), style.AlternateDimensionLengthDisplayUnit(doc.RuntimeSerialNumber), userStrings);

    [MapPropertyFromSource(nameof(DimensionStyleRow.Overridden), Use = nameof(Overridden))]
    private static partial DimensionStyleRow Project(DimensionStyle style, FontState font, UnitSystem dimensionLengthDisplayUnit, UnitSystem alternateDimensionLengthDisplayUnit, HashMap<string, string> userStrings);

    private static Seq<DimensionStyle.Field> Overridden(DimensionStyle style) =>
        Overrides(style.IsFieldOverriden);

    internal static Seq<DimensionStyle.Field> Overrides(Func<DimensionStyle.Field, bool> marked) =>
        toSeq(Enum.GetValues<DimensionStyle.Field>()).Filter(field => field is not (DimensionStyle.Field.Unset or DimensionStyle.Field.Count) && marked(field)).Strict();

    public static IO<Seq<DimensionStyleRow>> BuiltIns(RhinoDoc doc) =>
        TableOps.ReadRows(() => doc.DimStyles.BuiltInStyles, style => Row(doc, style));

    public static IO<TValue> WithPreview<TValue>(RhinoDoc doc, int index, int width, int height, bool transparent, Func<System.Drawing.Bitmap, IO<TValue>> body) =>
        from style in TableOps.Row(doc.DimStyles, index)
        from value in DisposalOps.Using(IO.lift(() => Missing.Unless(style.CreatePreviewBitmap(width, height, transparent), nameof(DimensionStyle.CreatePreviewBitmap))), body)
        select value;

    // --- [WRITES]
    public static IO<Unit> Written(RhinoDoc doc, DimensionStyle staged, DimensionStyleSpec spec) =>
        from copied in spec.Source.Traverse(source => Copied(doc, staged, source)).As()
        from named in TableOps.Named(staged, Some(spec.Name))
        from font in spec.Font.Traverse(query => Fonts.Resolve(query).Bind(resolved => IO.lift(() => { staged.Font = resolved; }))).As()
        from edited in spec.Edit(staged)
        select unit;

    private static IO<Unit> Copied(RhinoDoc doc, DimensionStyle staged, DimensionStyleSource source) =>
        source.Switch(
            (Doc: doc, Staged: staged),
            builtIn: static (state, builtIn) => DisposalOps.Using(
                IO.lift(() => toSeq(state.Doc.DimStyles.BuiltInStyles)),
                styles => IO.lift(() => styles.Find(builtIn.Names).ToFin(new Missing(nameof(DimStyleTable.BuiltInStyles))).Map(fun<DimensionStyle>(state.Staged.CopyFrom)))),
            row: static (state, row) => TableOps.Find(state.Doc.DimStyles, row.Address, includeDeleted: false).Map(fun<DimensionStyle>(state.Staged.CopyFrom)));

    public static IO<Unit> ModifyFromAnnotation(RhinoDoc doc, Guid annotationId, bool quiet) =>
        from resolved in Queries.Resolve<RhinoObject, AnnotationBase>(doc, annotationId)
        from address in IO.lift(() => ComponentRef.ById.From(resolved.Geometry.DimensionStyleId))
        from style in TableOps.Find(doc.DimStyles, address, includeDeleted: false)
        from modified in TableOps.ModifyRow(TableAccessors.DimStyles(doc), style.Index, staged => DisposalOps.Using(() => resolved.Geometry.DimensionStyle, effective => IO.lift(() => {
            staged.CopyFrom(effective);
            staged.ClearAllFieldOverrides();
            staged.ParentId = Guid.Empty;
        })), quiet)
        from cleared in RhinoObjects.ReplaceGeometry<AnnotationBase>(doc, annotationId, ClearOverrides)
        select cleared;

    public static IO<Unit> SetCurrent(RhinoDoc doc, int index, bool quiet) =>
        IO.lift(() => Refused.Unless(doc.DimStyles.SetCurrent(index, quiet), nameof(DimStyleTable.SetCurrent)));

    // --- [OVERRIDES]
    public static IO<Unit> SetOverride(AnnotationBase annotation, Func<DimensionStyle, IO<Unit>> edit) =>
        from parent in IO.lift(() => Missing.Unless(annotation.ParentDimensionStyle, nameof(AnnotationBase.ParentDimensionStyle)))
        from applied in DisposalOps.Using(
            TableOps.Locked(IO.lift(() => parent.Duplicate("", Guid.Empty, annotation.DimensionStyleId)), nameof(DimensionStyle.Duplicate)),
            child =>
                from edited in edit(child)
                from applied in IO.lift(() => Refused.Unless(annotation.SetOverrideDimStyle(child), nameof(AnnotationBase.SetOverrideDimStyle)))
                select applied)
        select applied;

    public static IO<Unit> ClearOverrides(AnnotationBase annotation) =>
        IO.lift(() => Refused.Unless(annotation.ClearPropertyOverrides(), nameof(AnnotationBase.ClearPropertyOverrides)));
}
