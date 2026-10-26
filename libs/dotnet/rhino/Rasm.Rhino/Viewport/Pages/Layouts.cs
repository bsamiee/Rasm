using System.Globalization;
using LanguageExt.ClassInstances;
using Rasm.Drafting;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Viewport.Pages;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads, ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class LayoutName {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value) ? new ValidationError("layout name is blank") : null;
}

public sealed record FrameStyle(LineGroup Lines, ComponentRef<Layer> Layer);

public sealed record LayoutSheet(LayoutName Name, Sheet Sheet, Option<string> Description, Option<FrameStyle> Frame);

public sealed record LayoutGroup(LayoutName Name, Option<string> Description, Seq<LayoutName> Members);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Layouts {
    // --- [SHEETS]
    public static IO<RhinoPageView> Ensure(RhinoDoc doc, LayoutSheet sheet) =>
        from extent in IO.lift(() => (Width: Quantities.As(sheet.Sheet.Extent.Width, doc.PageUnits), Height: Quantities.As(sheet.Sheet.Extent.Height, doc.PageUnits)))
        from page in IO.lift(() => Find(doc, sheet.Name).Match(Some: Fin.Succ, None: () => Refused.UnlessFound(doc.Views.AddPageView(sheet.Name, extent.Width, extent.Height, setActive: false), nameof(ViewTable.AddPageView))))
        from held in SheetOf(doc, page)
        from sized in unless(held == Some(sheet.Sheet), IO.lift(() => { (page.PageWidth, page.PageHeight) = extent; })).As()
        from described in IO.lift(() => sheet.Description.Iter(text => Update(page.Description, text, value => page.Description = value)))
        from framed in sheet.Frame.Traverse(style => Frame(doc, page, sheet.Sheet, style)).As()
        select page;

    public static IO<Option<Sheet>> SheetOf(RhinoDoc doc, RhinoPageView page) =>
        IO.lift(() => Sheet.Match(Quantities.From(page.PageWidth, doc.PageUnits), Quantities.From(page.PageHeight, doc.PageUnits), Quantities.From(doc.PageAbsoluteTolerance, doc.PageUnits)));

    public static IO<RhinoPageView> Duplicate(RhinoPageView page, LayoutName name, bool duplicatePageGeometry) =>
        from copy in IO.lift(() => Refused.UnlessFound(page.Duplicate(duplicatePageGeometry), nameof(RhinoPageView.Duplicate)))
        from named in IO.lift(() => copy.PageName = name)
        select copy;

    private static Option<RhinoPageView> Find(RhinoDoc doc, LayoutName name) =>
        Viewports.Pages(doc.Views.GetPageViews()).Find(page => name == page.PageName);

    private static Unit Update<T>(T held, T wanted, Action<T> write) =>
        EqualityComparer<T>.Default.Equals(held, wanted) ? unit : fun(write)(wanted);

    // --- [TITLES]
    public static IO<Unit> Title(RhinoDoc doc, RhinoPageView page, DraftingConvention convention, HashMap<TitleField, string> entries) =>
        from accepted in IO.lift(TitleField.Accept(convention, entries))
        from written in accepted.TraverseM(entry => entry.Field.Source.Switch(
            (Doc: doc, Page: page, entry.Field.Key, entry.Text),
            projectText: static state => UserStrings.Write(state.Doc, new DocumentKey(KeyName.Create(state.Key)), state.Text).Map(static _ => unit),
            sheetText: static state => UserStrings.Write(state.Page.MainViewport, toHashMap<EqStringOrdinalIgnoreCase, string, Option<string>>(Seq((state.Key, state.Text)))),
            identifier: static _ => IO.pure(unit),
            designation: static _ => IO.pure(unit))).As()
        select unit;

    // --- [GROUPS]
    public static IO<PageViewGroup> Ensure(RhinoDoc doc, LayoutGroup grouping) =>
        from members in IO.lift(() => (Callbacks.Unique(grouping.Members, identity, nameof(LayoutGroup.Members)),
                grouping.Members.Traverse(name => Find(doc, name).ToFin(new NotFound("layout", $"{name}"))).As())
            .Apply(static (_, pages) => pages).As())
        from row in TableOps.Find(doc.PageViewGroups, new ComponentRef<PageViewGroup>.ByName(grouping.Name), includeDeleted: false)
            .Catch(static error => error.IsType<NotFound>(), _ => Add(doc, grouping.Name))
        from described in IO.lift(() => grouping.Description.Iter(text => Update(row.Description, text, value => row.Description = value)))
        from left in IO.lift(() => toSeq(row.GetMembers().Except(members)).Iter(member => member.RemoveFromPageViewGroup(row.Index)))
        from placed in IO.lift(() => members.Iter((position, page) => Update(page.PageViewGroupSortIndex(row.Index), position, index => page.SetPageViewGroupSortIndex(row.Index, index))))
        select row;

    private static IO<PageViewGroup> Add(RhinoDoc doc, LayoutName name) =>
        DisposalOps.Within(static () => new PageViewGroup(), staged =>
            from named in TableOps.Named(staged, Some<string>(name))
            from index in IO.lift(() => Conversions.Required(doc.PageViewGroups.Add(staged), nameof(PageViewGroupTable.Add)))
            from row in TableOps.Find(doc.PageViewGroups, index, includeDeleted: false)
            select row);

    // --- [ORDER]
    public static IO<Seq<(RhinoPageView Page, StandardName Number)>> Sheets(RhinoDoc doc, NamingStandard grammar) =>
        IO.lift(() => toSeq(Viewports.Pages(doc.Views.GetPageViews()).Choose(page => StandardName.From(grammar, page.PageName).ToOption().Map(number => (Page: page, Number: number))).OrderBy(static sheet => sheet.Number)).Strict());

    public static IO<Unit> Arrange(Seq<RhinoPageView> pages) =>
        from start in IO.lift(() => pages.Fold(int.MaxValue, static (least, page) => int.Min(least, page.PageNumber)))
        from placed in IO.lift(() => pages.Iter((offset, page) => Update(page.PageNumber, start + offset, position => page.PageNumber = position)))
        select placed;

    public static IO<Unit> Renumber(RhinoDoc doc, NamingStandard grammar, Seq<RhinoPageView> ordered) =>
        from numbered in IO.lift(() => ordered.Traverse(page => StandardName.From(grammar, page.PageName).Map(number => (Sheet: page, Number: number))).As())
        from others in Sheets(doc, grammar).Map(sheets => sheets.Filter(sheet => !ordered.Exists(sheet.Page.Equals)).Map(static sheet => sheet.Number))
        from renumbered in IO.lift(StandardName.Renumber(numbered, others))
        from named in IO.lift(() => renumbered.Iter(static sheet => Update(sheet.Sheet.PageName, sheet.Number.Text, name => sheet.Sheet.PageName = name)))
        from arranged in Arrange(renumbered.Map(static sheet => sheet.Sheet))
        from counted in Number(doc, grammar)
        select counted;

    public static IO<Unit> Number(RhinoDoc doc, NamingStandard grammar) =>
        from sheets in Sheets(doc, grammar)
        let positions = toHashMap(StandardName.Positions(sheets).Map(static sheet => (sheet.Sheet.RuntimeSerialNumber, (sheet.Ordinal, sheet.Count))))
        from written in Viewports.Pages(doc.Views.GetPageViews()).TraverseM(page =>
            from position in IO.pure(positions.Find(page.RuntimeSerialNumber))
            from stored in UserStrings.Write(page.MainViewport, toHashMap<EqStringOrdinalIgnoreCase, string, Option<string>>(Seq(
                (TitleField.SegmentNumber.Key, position.Map(static at => at.Ordinal.ToString(CultureInfo.InvariantCulture))),
                (TitleField.SegmentCount.Key, position.Map(static at => at.Count.ToString(CultureInfo.InvariantCulture))))))
            select stored).As()
        select unit;

    public static Func<IPlugInSink, Action<IDisposable>, IO<Unit>> NumberFollowsPages(NamingStandard grammar) =>
        (sink, retain) => Subscriptions.Host(typeof(Layouts), static () => Seq(EventKind.RhinoViewCreate, EventKind.RhinoViewDestroy, EventKind.RhinoViewRename)
                .Map(static row => row.Choose(args => row.DocumentSerial(args).Filter(_ => args.View is RhinoPageView).Map(static serial => (serial, unit)))))
            .Through(Subscriptions.Idle<uint, Unit>(serials =>
                Callbacks.Each(toSeq(serials.Keys).Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial))).Map(doc => Number(doc, grammar))).Map(unit)), sink, retain);

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
            (sheet.Field, sheet.Zones, sheet.Extent, Quantities.As(sheet.Size.Convention.Lettering.ZoneHeight, doc.PageUnits));
        Option<Length> reach = sheet.Size.Convention.Centering.Map(static centering => centering.Reach);
        Option<Length> blocked = sheet.Size.Convention.Centering.Map(centering => width / 2 < field.Right - centering.TitleBlock ? centering.Reach : Length.Zero);
        return Stroke(LineWeight.ExtraWide, new Rectangle3d(Plane.WorldXY, At(field.Left, field.Bottom), At(field.Right, field.Top)).ToPolyline())
            + Axis(zones.Columns, field.Left, field.Right, width, zones.Column, At, [(field.Top, height, true, reach), (field.Bottom, Length.Zero, zones.AllEdges, blocked)])
            + Axis(zones.Rows, field.Bottom, field.Top, height, zones.Row, (along, across) => At(across, along), [(field.Right, width, true, reach), (field.Left, Length.Zero, zones.AllEdges, reach)]);

        Seq<IO<Guid>> Axis(Seq<Length> divisions, Length near, Length far, Length edge, Func<int, string> label, Func<Length, Length, Point3d> at, Seq<(Length Inner, Length Outer, bool Zoned, Option<Length> Reach)> bands) =>
            (from band in bands
             where band.Zoned
             from mark in divisions.Map(division => Stroke(LineWeight.Wide, [at(division, band.Inner), at(division, band.Outer)]))
                 + near.Cons(divisions).Zip(divisions.Add(far)).Map((span, index) => Label(label(index), at((span.First + span.Second) / 2, (band.Inner + band.Outer) / 2)))
             select mark)
            + (from band in bands
               from inward in band.Reach.ToSeq()
               select Stroke(LineWeight.ExtraWide, [at(edge / 2, band.Outer), at(edge / 2, band.Inner < band.Outer ? band.Inner - inward : band.Inner + inward)]));

        Point3d At(Length x, Length y) => new(Quantities.As(x, doc.PageUnits), Quantities.As(y, doc.PageUnits), 0);

        IO<Guid> Stroke(LineWeight weight, IEnumerable<Point3d> points) => Row(Some(style.Lines.Width(weight)), () => new PolylineCurve(points));

        IO<Guid> Label(string text, Point3d center) =>
            Row(None, () => Refused.UnlessFound(TextEntity.Create(text, new Plane(center, Vector3d.XAxis, Vector3d.YAxis), doc.DimStyles.Current, wrapped: false, rectWidth: 0, rotationRadians: 0), nameof(TextEntity.Create))
                .Map(entity => { (entity.Justification, entity.TextHeight) = (TextJustification.MiddleCenter, lettering); return (GeometryBase)entity; }));

        IO<Guid> Row(Option<LineWidth> width, Func<Fin<GeometryBase>> shape) =>
            TableOps.WithAttributes(doc.CreateDefaultAttributes, None, attributes =>
                from placed in IO.lift(() => { (attributes.LayerIndex, attributes.Space, attributes.ViewportId) = (layer, ActiveSpace.PageSpace, page.MainViewport.Id); })
                from weighted in IO.lift(() => width.Iter(stroke => (attributes.PlotWeightSource, attributes.PlotWeight) = (ObjectPlotWeightSource.PlotWeightFromObject, PlotWeight.Of(stroke).ToHost())))
                from tagged in IO.lift(() => Refused.Unless(attributes.SetUserString(FrameKey, sheet.Size.Name), nameof(ObjectAttributes.SetUserString)))
                from id in DisposalOps.Within(shape,
                    made => IO.lift(made).Bind(geometry => TableOps.Add(doc, new GeometryPair(geometry, Some(attributes)), None, reference: false)),
                    static made => DisposalOps.Release(made.ToOption().ToSeq()))
                select id);
    }
}
