using LanguageExt.ClassInstances;
using Rasm.Drafting;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Viewport.Paper;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads, ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class LayoutName {
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value) ? new InvalidRhinoValue() : null;
}

public sealed record FrameStyle(LineGroup Lines, ComponentRef<Layer> Layer);

public sealed record LayoutSheet(LayoutName Name, Sheet Sheet, Option<string> Description, Option<FrameStyle> Frame);

public sealed record LayoutGroup(LayoutName Name, Option<string> Description, Seq<LayoutName> Members);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Layouts {
    // --- [SHEETS]
    public static IO<RhinoPageView> Ensure(RhinoDoc doc, LayoutSheet sheet) =>
        from extent in IO.lift(() => (Width: Quantities.As(sheet.Sheet.Extent.Width, doc.PageUnits), Height: Quantities.As(sheet.Sheet.Extent.Height, doc.PageUnits)))
        from page in IO.lift(() => Find(doc, sheet.Name).Match(Some: Fin.Succ, None: () => Missing.Unless(doc.Views.AddPageView(sheet.Name, extent.Width, extent.Height, setActive: false), nameof(ViewTable.AddPageView))))
        from held in SheetOf(doc, page)
        from sized in unless(held == Some(sheet.Sheet), IO.lift(() => { (page.PageWidth, page.PageHeight) = extent; })).As()
        from described in IO.lift(() => sheet.Description.Iter(text => Update(page.Description, text, value => page.Description = value)))
        from framed in sheet.Frame.Traverse(style => Frame(doc, page, sheet.Sheet, style)).As()
        select page;

    public static IO<Option<Sheet>> SheetOf(RhinoDoc doc, RhinoPageView page) =>
        IO.lift(() => Sheet.Match(Quantities.From(page.PageWidth, doc.PageUnits), Quantities.From(page.PageHeight, doc.PageUnits)));

    public static IO<RhinoPageView> Duplicate(RhinoPageView page, LayoutName name, bool duplicatePageGeometry) =>
        from copy in IO.lift(() => Missing.Unless(page.Duplicate(duplicatePageGeometry), nameof(RhinoPageView.Duplicate)))
        from named in IO.lift(() => copy.PageName = name)
        select copy;

    private static Seq<RhinoPageView> Pages(RhinoDoc doc) =>
        toSeq(doc.Views.GetPageViews().OrderBy(static page => page.PageNumber)).Strict();

    private static Option<RhinoPageView> Find(RhinoDoc doc, LayoutName name) =>
        Pages(doc).Find(page => name == page.PageName);

    private static Unit Update<T>(T held, T wanted, Action<T> write) =>
        EqualityComparer<T>.Default.Equals(held, wanted) ? unit : fun(write)(wanted);

    // --- [TITLES]
    public static IO<Unit> Title(RhinoDoc doc, RhinoPageView page, RevisionAlphabet revisions, HashMap<TitleField, string> entries) =>
        from accepted in IO.lift(TitleField.Accept(revisions, entries).ToFin())
        from held in IO.lift(() => UserStrings.Held(page.MainViewport.GetUserStrings()))
        from written in toSeq(TitleField.Items).TraverseM(field => field.Source.Switch(
            (Doc: doc, Page: page, Held: held, field.Key, Text: accepted.Find(field)),
            projectText: static (state, _) =>
                from key in IO.lift(Conversions.Validated<KeyName, string, InvalidRhinoValue>(state.Key).Map(static name => new DocumentKey(name)))
                from stored in UserStrings.Write(state.Doc, key, state.Text)
                select unit,
            sheetText: static (state, _) => UserStrings.Write(state.Held, state.Page.MainViewport.SetUserString, toHashMap<EqStringOrdinalIgnoreCase, string, Option<string>>(Seq((state.Key, state.Text)))),
            identifier: static (_, _) => IO.pure(unit),
            ordinal: static (_, _) => IO.pure(unit),
            count: static (_, _) => IO.pure(unit),
            designation: static (_, _) => IO.pure(unit))).As()
        select unit;

    // --- [GROUPS]
    public static IO<PageViewGroup> Ensure(RhinoDoc doc, LayoutGroup grouping) =>
        from members in IO.lift(() => (Callbacks.Unique(grouping.Members, identity, nameof(LayoutGroup.Members)), grouping.Members.Traverse(name => Find(doc, name).ToValidation<Error>(new MissingLayout(name))).As())
            .Apply(static (_, pages) => pages).As().ToFin())
        from row in TableOps.Find(doc.PageViewGroups, new ComponentRef<PageViewGroup>.ByName(grouping.Name), includeDeleted: false)
            .Catch(static error => error.IsType<MissingComponent<PageViewGroup>>(), _ => Add(doc, grouping.Name))
        from described in IO.lift(() => grouping.Description.Iter(text => Update(row.Description, text, value => row.Description = value)))
        from left in IO.lift(() => toSeq(row.GetMembers().Except(members)).Iter(member => member.RemoveFromPageViewGroup(row.Index)))
        from placed in IO.lift(() => members.Iter((position, page) => Update(page.PageViewGroupSortIndex(row.Index), position, index => page.SetPageViewGroupSortIndex(row.Index, index))))
        select row;

    private static IO<PageViewGroup> Add(RhinoDoc doc, LayoutName name) =>
        (from staged in use(static () => new PageViewGroup())
         from named in TableOps.Named(staged, Some<string>(name))
         from index in IO.lift(() => Conversions.Required(doc.PageViewGroups.Add(staged), nameof(PageViewGroupTable.Add)))
         from row in TableOps.Find(doc.PageViewGroups, index, includeDeleted: false)
         select row).Bracket();

    // --- [ORDER]
    public static IO<Seq<(RhinoPageView Page, StandardName Number)>> Sheets(RhinoDoc doc, NamingStandard grammar) =>
        IO.lift(() => toSeq(Pages(doc).Choose(page => StandardName.From(grammar, page.PageName).ToOption().Map(number => (Page: page, Number: number))).OrderBy(static sheet => sheet.Number)).Strict());

    public static IO<Unit> Arrange(Seq<RhinoPageView> pages) =>
        from start in IO.lift(() => pages.Fold(int.MaxValue, static (least, page) => int.Min(least, page.PageNumber)))
        from placed in IO.lift(() => pages.Iter((offset, page) => Update(page.PageNumber, start + offset, position => page.PageNumber = position)))
        select placed;

    public static IO<Unit> Renumber(RhinoDoc doc, NamingStandard grammar, Seq<RhinoPageView> ordered) =>
        from numbered in IO.lift(() => ordered.Traverse(page => StandardName.From(grammar, page.PageName).ToValidation().Map(number => (Sheet: page, Number: number))).As().ToFin())
        from others in Sheets(doc, grammar).Map(sheets => sheets.Filter(sheet => !ordered.Exists(sheet.Page.Equals)).Map(static sheet => sheet.Number))
        from renumbered in IO.lift(StandardName.Renumber(numbered, others).ToFin())
        from named in IO.lift(() => renumbered.Iter(static sheet => Update(sheet.Sheet.PageName, sheet.Number.Text, name => sheet.Sheet.PageName = name)))
        from arranged in Arrange(renumbered.Map(static sheet => sheet.Sheet))
        select arranged;

    // --- [FRAMES]
    private const string FrameKey = "sheet-frame";

    private static IO<Unit> Frame(RhinoDoc doc, RhinoPageView page, Sheet sheet, FrameStyle style) =>
        from layer in TableOps.Find(doc.Layers, style.Layer, includeDeleted: false)
        from cleared in TableOps.Apply(doc, new TableOp.Delete(
            new ObjectTarget.Lookup(objects => objects.FindByUserString(FrameKey, "*", caseSensitive: true, searchGeometry: false, searchAttributes: true,
                new ObjectEnumeratorSettings { SpaceFilter = ActiveSpace.PageSpace, ViewportFilter = page.MainViewport, HiddenObjects = true, LockedObjects = true })),
            Quiet: true, IgnoreModes: true))
        from added in Callbacks.Each(Marks(doc, page, sheet, style, layer.Index))
        select unit;

    private static Seq<IO<Guid>> Marks(RhinoDoc doc, RhinoPageView page, Sheet sheet, FrameStyle style, int layer) {
        (PaperRectangle field, ZoneGrid zones, (Length width, Length height), double lettering) =
            (sheet.Field, sheet.Zones, sheet.Extent, Quantities.As(sheet.Size.Series.Convention.Lettering.Height(sheet.Size), doc.PageUnits));
        return Stroke(LineWeight.ExtraWide, new Rectangle3d(Plane.WorldXY, At(field.Left, field.Bottom), At(field.Right, field.Top)).ToPolyline())
            + Axis(zones.Columns, field.Left, field.Right, width, zones.Column, At, [(field.Top, height, true), (field.Bottom, Length.Zero, zones.AllEdges)])
            + Axis(zones.Rows, field.Bottom, field.Top, height, zones.Row, (along, across) => At(across, along), [(field.Right, width, true), (field.Left, Length.Zero, zones.AllEdges)]);

        Seq<IO<Guid>> Axis(Seq<Length> divisions, Length near, Length far, Length edge, Func<int, string> label, Func<Length, Length, Point3d> at, Seq<(Length Inner, Length Outer, bool Zoned)> bands) =>
            (from band in bands
             where band.Zoned
             from mark in divisions.Map(division => Stroke(LineWeight.Wide, [at(division, band.Inner), at(division, band.Outer)]))
                 + near.Cons(divisions).Zip(divisions.Add(far)).Map((span, index) => Label(label(index), at((span.First + span.Second) / 2, (band.Inner + band.Outer) / 2)))
             select mark)
            + (from reach in sheet.Size.Series.Convention.Centering.ToSeq()
               from band in bands
               select Stroke(LineWeight.ExtraWide, [at(edge / 2, band.Outer), at(edge / 2, band.Inner < band.Outer ? band.Inner - reach : band.Inner + reach)]));

        Point3d At(Length x, Length y) => new(Quantities.As(x, doc.PageUnits), Quantities.As(y, doc.PageUnits), 0);

        IO<Guid> Stroke(LineWeight weight, IEnumerable<Point3d> points) => Row(Some(style.Lines.Width(weight)), () => new PolylineCurve(points));

        IO<Guid> Label(string text, Point3d center) =>
            Row(None, () => Missing.Unless(TextEntity.Create(text, new Plane(center, Vector3d.XAxis, Vector3d.YAxis), doc.DimStyles.Current, wrapped: false, rectWidth: 0, rotationRadians: 0), nameof(TextEntity.Create))
                .Map(entity => { (entity.Justification, entity.TextHeight) = (TextJustification.MiddleCenter, lettering); return (GeometryBase)entity; }));

        IO<Guid> Row(Option<LineWidth> width, Func<Fin<GeometryBase>> shape) =>
            TableOps.WithAttributes(doc.CreateDefaultAttributes, None, attributes =>
                from placed in IO.lift(() => { (attributes.LayerIndex, attributes.Space, attributes.ViewportId) = (layer, ActiveSpace.PageSpace, page.MainViewport.Id); })
                from weighted in IO.lift(() => width.Iter(stroke => (attributes.PlotWeightSource, attributes.PlotWeight) = (ObjectPlotWeightSource.PlotWeightFromObject, PlotWeight.Of(stroke).ToHost())))
                from tagged in IO.lift(() => Refused.Unless(attributes.SetUserString(FrameKey, sheet.Size.Designation), nameof(ObjectAttributes.SetUserString)))
                from geometry in use(IO.lift(shape))
                from id in TableOps.Add(doc, new GeometryPair(geometry, Some(attributes)), None, reference: false)
                select id);
    }
}
