using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Answers))]

namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DimensionStyleRow(
    Guid Id,
    int Index,
    Option<string> Name,
    Option<Guid> ParentId,
    bool IsChild,
    bool HasFieldOverrides,
    Seq<DimensionStyle.Field> Overridden,
    string FontFaceName,
    UnitSystem DimensionLengthDisplayUnit,
    UnitSystem AlternateDimensionLengthDisplayUnit,
    bool IsReference,
    bool IsDeleted,
    HashMap<string, string> UserStrings);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class DimensionStyles {
    // --- [TABLE]
    internal static TableAccessors<DimensionStyle> Accessors(RhinoDoc doc, bool reference) =>
        new(doc.DimStyles, style => doc.DimStyles.Add(style, reference), doc.DimStyles.Modify, doc.DimStyles.FindName, doc.DimStyles.FindIndex);

    public static IO<DimensionStyleRow> Row(RhinoDoc doc, DimensionStyle style) =>
        from userStrings in GeometryOps.ReadUserStrings(GeometryOps.UserStrings(style))
        from row in IO.lift(() => Project(style, style.DimensionLengthDisplayUnit(doc.RuntimeSerialNumber), style.AlternateDimensionLengthDisplayUnit(doc.RuntimeSerialNumber), userStrings))
        select row;

    internal static Seq<DimensionStyle.Field> Fields(Func<DimensionStyle.Field, bool> marked) =>
        toSeq(Enum.GetValues<DimensionStyle.Field>()).Filter(field => field is not (DimensionStyle.Field.Unset or DimensionStyle.Field.Count) && marked(field)).Strict();

    [MapPropertyFromSource(nameof(DimensionStyleRow.Overridden), Use = nameof(Overridden))]
    [MapProperty(nameof(@DimensionStyle.Font.FaceName), nameof(DimensionStyleRow.FontFaceName), SuppressNullMismatchDiagnostic = true)]
    private static partial DimensionStyleRow Project(DimensionStyle style, UnitSystem dimensionLengthDisplayUnit, UnitSystem alternateDimensionLengthDisplayUnit, HashMap<string, string> userStrings);

    private static Seq<DimensionStyle.Field> Overridden(DimensionStyle style) =>
        Fields(style.IsFieldOverriden);

    // --- [ROWS]
    public static IO<int> Add(RhinoDoc doc, string name, Option<int> template, Option<Guid> parent, Func<DimensionStyle, IO<Unit>> edit, bool reference) =>
        TableOps.AddRow(
            Accessors(doc, reference),
            template.Match(
                Some: index =>
                    from source in IO.lift(() => Missing.Unless(doc.DimStyles.FindIndex(index), nameof(DimStyleTable.FindIndex)))
                    from duplicate in TableOps.Locked(IO.lift(() => source.Duplicate(name, Guid.NewGuid(), parent.IfNone(Guid.Empty))), nameof(DimensionStyle.Duplicate))
                    select duplicate,
                None: () => IO.lift(() => new DimensionStyle { Name = name, ParentId = parent.IfNone(Guid.Empty) })),
            edit);

    public static IO<Unit> Modify(RhinoDoc doc, int index, Func<DimensionStyle, IO<Unit>> edit, bool quiet) =>
        TableOps.ModifyRow(Accessors(doc, reference: false), index, static live => live.Duplicate(), edit, quiet);

    public static IO<ModifyType> ModifyFromAnnotation(RhinoDoc doc, Guid annotationId) =>
        from resolved in Queries.Resolve<RhinoObject, AnnotationBase>(doc, annotationId)
        from modifyType in Disposal.Using(() => resolved.Geometry.DimensionStyle, style =>
            from modifyType in IO.lift(() => doc.DimStyles.Modify(style, resolved.Geometry))
            from saved in IO.lift(() => Refused.Unless(modifyType != ModifyType.NotSaved, nameof(DimStyleTable.Modify)))
            select modifyType)
        select modifyType;

    public static IO<Unit> SetCurrent(RhinoDoc doc, int index, bool quiet) =>
        IO.lift(() => Refused.Unless(doc.DimStyles.SetCurrent(index, quiet), nameof(DimStyleTable.SetCurrent)));

    public static IO<Unit> Delete(RhinoDoc doc, Seq<int> indices, bool quiet) =>
        indices.TraverseM(index => IO.lift(() => Refused.Unless(doc.DimStyles.Delete(index, quiet), nameof(DimStyleTable.Delete)))).As().Map(static _ => unit);

    public static IO<Option<DimensionStyleRow>> Find(RhinoDoc doc, ComponentRef address) =>
        from found in TableOps.Find(Accessors(doc, reference: false), address)
        from row in found.Traverse(style => Row(doc, style)).As()
        select row;

    public static IO<Seq<DimensionStyleRow>> BuiltIns(RhinoDoc doc) =>
        TableOps.ReadRows(() => doc.DimStyles.BuiltInStyles, style => Row(doc, style));

    public static IO<TValue> WithPreview<TValue>(RhinoDoc doc, int index, int width, int height, bool transparent, Func<System.Drawing.Bitmap, IO<TValue>> body) =>
        from style in IO.lift(() => Missing.Unless(doc.DimStyles.FindIndex(index), nameof(DimStyleTable.FindIndex)))
        from value in Disposal.Using(IO.lift(() => Missing.Unless(style.CreatePreviewBitmap(width, height, transparent), nameof(DimensionStyle.CreatePreviewBitmap))), body)
        select value;

    // --- [OVERRIDES]
    public static IO<Unit> SetOverride(AnnotationBase annotation, Func<DimensionStyle, IO<Unit>> edit) =>
        from parent in IO.lift(() => Missing.Unless(annotation.ParentDimensionStyle, nameof(AnnotationBase.ParentDimensionStyle)))
        from applied in Disposal.Using(
            TableOps.Locked(IO.lift(() => parent.Duplicate("", Guid.Empty, annotation.DimensionStyleId)), nameof(DimensionStyle.Duplicate)),
            child =>
                from edited in edit(child)
                from applied in IO.lift(() => Refused.Unless(annotation.SetOverrideDimStyle(child), nameof(AnnotationBase.SetOverrideDimStyle)))
                select applied)
        select applied;

    public static IO<Unit> ClearOverrides(AnnotationBase annotation) =>
        IO.lift(() => Refused.Unless(annotation.ClearPropertyOverrides(), nameof(AnnotationBase.ClearPropertyOverrides)));
}
