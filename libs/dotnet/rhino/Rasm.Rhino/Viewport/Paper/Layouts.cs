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
[ValueObject<string>(SkipIParsable = true, EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads, ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
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
        from page in IO.lift(() => Find(doc, sheet.Name).Match(
            Some: static existing => Fin.Succ(existing),
            None: () => Missing.Unless(doc.Views.AddPageView(sheet.Name, setActive: false), nameof(ViewTable.AddPageView))))
        from held in SheetOf(doc, page)
        from sized in unless(held == Some(sheet.Sheet), IO.lift(() => {
            (page.PageWidth, page.PageHeight) = sheet.Sheet.Extent switch {
                var (width, height) => (Quantities.As(width, doc.PageUnits), Quantities.As(height, doc.PageUnits)),
            };
        })).As()
        from described in IO.lift(() => sheet.Description.Filter(text => !string.Equals(text, page.Description, StringComparison.Ordinal)).Iter(text => page.Description = text))
        from framed in sheet.Frame.Match(Some: style => Frame(doc, page, sheet.Sheet, style), None: static () => IO.pure(unit))
        select page;

    public static IO<Option<Sheet>> SheetOf(RhinoDoc doc, RhinoPageView page) =>
        IO.lift(() => Sheet.Match(Quantities.From(page.PageWidth, doc.PageUnits), Quantities.From(page.PageHeight, doc.PageUnits)));

    public static IO<RhinoPageView> Duplicate(RhinoPageView page, LayoutName name, bool duplicatePageGeometry) =>
        from copy in IO.lift(() => Missing.Unless(page.Duplicate(duplicatePageGeometry), nameof(RhinoPageView.Duplicate)))
        from named in IO.lift(() => { copy.PageName = name; })
        select copy;

    private static Seq<RhinoPageView> Pages(RhinoDoc doc) =>
        toSeq(doc.Views.GetPageViews().OrderBy(static page => page.PageNumber)).Strict();

    private static Option<RhinoPageView> Find(RhinoDoc doc, LayoutName name) =>
        Pages(doc).Find(page => name == page.PageName);

    // --- [TITLES]
    public static IO<Unit> Title(RhinoDoc doc, RhinoPageView page, RevisionAlphabet revisions, HashMap<TitleField, string> entries) =>
        from accepted in IO.lift(TitleField.Accept(revisions, entries).ToFin())
        from held in IO.lift(() => UserStrings.Held(page.MainViewport.GetUserStrings()))
        from written in toSeq(TitleField.Items).TraverseM(field => field.Source.Switch(
            (Doc: doc, Page: page, Held: held, field.Key, Text: accepted.Find(field)),
            projectText: static (state, _) => Documented(state.Doc, state.Key, state.Text),
            sheetText: static (state, _) => UserStrings.Write(state.Held, state.Page.MainViewport.SetUserString, toHashMap<EqStringOrdinalIgnoreCase, string, Option<string>>(Seq((state.Key, state.Text)))),
            identifier: static (_, _) => IO.pure(unit),
            ordinal: static (_, _) => IO.pure(unit),
            count: static (_, _) => IO.pure(unit),
            designation: static (_, _) => IO.pure(unit))).As()
        select unit;

    private static IO<Unit> Documented(RhinoDoc doc, string key, Option<string> text) =>
        from flat in IO.lift(Conversions.Validated<KeyName, string, InvalidRhinoValue>(key).Map(static name => new DocumentKey.Flat(name)))
        from prior in UserStrings.Value(doc, flat)
        from written in prior == text ? IO.pure(prior) : UserStrings.Write(doc, flat, text)
        select unit;

    // --- [GROUPS]
    public static IO<PageViewGroup> Ensure(RhinoDoc doc, LayoutGroup grouping) =>
        from members in IO.lift(() => (
                Callbacks.Unique(grouping.Members, identity, nameof(LayoutGroup.Members)),
                grouping.Members.Traverse(name => Find(doc, name).ToValidation<Error>(new MissingLayout(name))).As())
            .Apply(static (_, pages) => pages).As().ToFin())
        from row in TableOps.Find(doc.PageViewGroups, new ComponentRef<PageViewGroup>.ByName(grouping.Name), includeDeleted: false)
            .Catch(static error => error.IsType<MissingComponent<PageViewGroup>>(), _ => Add(doc, grouping.Name))
        from described in IO.lift(() => grouping.Description.Filter(text => !string.Equals(text, row.Description, StringComparison.Ordinal)).Iter(text => row.Description = text))
        from left in IO.lift(() => toSeq(row.GetMembers())
            .Filter(member => !members.Exists(page => page.RuntimeSerialNumber == member.RuntimeSerialNumber))
            .Iter(member => member.RemoveFromPageViewGroup(row.Index)))
        from placed in members.Map(static (page, position) => (Page: page, Position: position)).TraverseM(placement => IO.lift(() => {
            if (placement.Page.PageViewGroupSortIndex(row.Index) == placement.Position)
                return;
            placement.Page.SetPageViewGroupSortIndex(row.Index, placement.Position);
        })).As()
        select row;

    private static IO<PageViewGroup> Add(RhinoDoc doc, LayoutName name) =>
        (from staged in use(static () => new PageViewGroup())
         from named in TableOps.Named(staged, Some<string>(name))
         from index in IO.lift(() => Conversions.Required(doc.PageViewGroups.Add(staged), nameof(PageViewGroupTable.Add)))
         from row in TableOps.Find(doc.PageViewGroups, index, includeDeleted: false)
         select row).Bracket();

    // --- [ORDER]
    public static IO<Seq<(RhinoPageView Page, StandardName Number)>> Sheets(RhinoDoc doc, NamingStandard grammar) =>
        IO.lift(() => Pages(doc).Choose(page => StandardName.From(grammar, page.PageName).ToOption().Map(number => (Page: page, Number: number))).Strict());

    public static IO<Unit> Arrange(Seq<RhinoPageView> pages) =>
        from start in IO.lift(() => pages.Fold(int.MaxValue, static (least, page) => int.Min(least, page.PageNumber)))
        from placed in pages.Map((page, offset) => (Page: page, Position: start + offset)).TraverseM(static placement => IO.lift(() => {
            if (placement.Page.PageNumber == placement.Position)
                return;
            placement.Page.PageNumber = placement.Position;
        })).As()
        select unit;

    public static IO<Unit> Order(RhinoDoc doc, NamingStandard grammar) =>
        Sheets(doc, grammar).Bind(static sheets => Arrange(toSeq(sheets.OrderBy(static sheet => sheet.Number)).Map(static sheet => sheet.Page)));

    public static IO<Unit> Renumber(RhinoDoc doc, NamingStandard grammar, Seq<RhinoPageView> ordered) =>
        from numbered in IO.lift(() => ordered.Traverse(page => StandardName.From(grammar, page.PageName).ToValidation().Map(number => (Sheet: page, Number: number))).As().ToFin())
        from others in Sheets(doc, grammar).Map(sheets => sheets
            .Filter(sheet => !ordered.Exists(page => page.RuntimeSerialNumber == sheet.Page.RuntimeSerialNumber))
            .Map(static sheet => sheet.Number))
        from renumbered in IO.lift(StandardName.Renumber(numbered, others).ToFin())
        from named in renumbered.TraverseM(static sheet => IO.lift(() => {
            if (string.Equals(sheet.Sheet.PageName, sheet.Number.Text, StringComparison.Ordinal))
                return;
            sheet.Sheet.PageName = sheet.Number.Text;
        })).As()
        from arranged in Arrange(renumbered.Map(static sheet => sheet.Sheet))
        select unit;

    // --- [FRAMES]
    private const string FrameKey = "sheet-frame";

    private static IO<Unit> Frame(RhinoDoc doc, RhinoPageView page, Sheet sheet, FrameStyle style) =>
        from layer in TableOps.Find(doc.Layers, style.Layer, includeDeleted: false)
        from cleared in TableOps.Apply(doc, new TableOp.Delete(
            new ObjectTarget.UserString(FrameKey, "*", CaseSensitive: true, SearchGeometry: false, SearchAttributes: true,
                new ObjectEnumeratorSettings { SpaceFilter = ActiveSpace.PageSpace, ViewportFilter = page.MainViewport, HiddenObjects = true, LockedObjects = true }),
            Quiet: true, IgnoreModes: true))
        from added in Callbacks.Each(Rows(doc, page, sheet, style, layer.Index))
        select unit;

    private static Seq<(Seq<(Length X, Length Y)> Points, LineWeight Weight)> Strokes(Sheet sheet) =>
        (sheet.Field, sheet.Size.Series.Convention.Centering.ToSeq(), Axes(sheet)) switch {
            var (field, centering, axes) =>
                Seq((Points: Seq<(Length X, Length Y)>((field.Left, field.Bottom), (field.Right, field.Bottom), (field.Right, field.Top), (field.Left, field.Top), (field.Left, field.Bottom)),
                    Weight: LineWeight.ExtraWide))
                + (from axis in axes
                   from band in axis.Bands
                   where band.Zoned
                   from division in axis.Divisions
                   select (Points: Seq(axis.Point(division, band.Inner), axis.Point(division, band.Outer)), Weight: LineWeight.Wide))
                + (from reach in centering
                   from axis in axes
                   from band in axis.Bands
                   select (Points: Seq(axis.Point(axis.Middle, band.Outer), axis.Point(axis.Middle, band.Inner < band.Outer ? band.Inner - reach : band.Inner + reach)),
                       Weight: LineWeight.ExtraWide)),
        };

    private static Seq<(string Text, (Length X, Length Y) Center)> Labels(Sheet sheet) =>
        from axis in Axes(sheet)
        from band in axis.Bands
        where band.Zoned
        from label in axis.Near.Cons(axis.Divisions).Zip(axis.Divisions.Add(axis.Far))
            .Map((span, index) => (axis.Label(index), axis.Point((span.First + span.Second) / 2, (band.Inner + band.Outer) / 2)))
        select label;

    private static Seq<(Seq<Length> Divisions, Length Near, Length Far, Length Middle, Func<int, string> Label, Func<Length, Length, (Length X, Length Y)> Point,
        Seq<(Length Inner, Length Outer, bool Zoned)> Bands)> Axes(Sheet sheet) =>
        (sheet.Extent, sheet.Field, sheet.Zones) switch {
            var ((width, height), field, zones) => [
                (zones.Columns, field.Left, field.Right, width / 2, zones.Column, static (along, across) => (along, across),
                    [(field.Top, height, true), (field.Bottom, Length.Zero, zones.AllEdges)]),
                (zones.Rows, field.Bottom, field.Top, height / 2, zones.Row, static (along, across) => (across, along),
                    [(field.Right, width, true), (field.Left, Length.Zero, zones.AllEdges)]),
            ],
        };

    private static Seq<IO<Guid>> Rows(RhinoDoc doc, RhinoPageView page, Sheet sheet, FrameStyle style, int layer) {
        Length lettering = sheet.Size.Series.Convention.Lettering.Height(sheet.Size);
        return Strokes(sheet).Map(stroke => Row(Some(style.Lines.Width(stroke.Weight)), () => new PolylineCurve(stroke.Points.Map(At))))
            + Labels(sheet).Map(label => Row(None, () => Missing.Unless(
                    TextEntity.Create(label.Text, new Plane(At(label.Center), Vector3d.XAxis, Vector3d.YAxis), doc.DimStyles.Current, wrapped: false, rectWidth: 0, rotationRadians: 0),
                    nameof(TextEntity.Create))
                .Map(text => {
                    (text.Justification, text.TextHeight) = (TextJustification.MiddleCenter, Quantities.As(lettering, doc.PageUnits));
                    return (GeometryBase)text;
                })));

        Point3d At((Length X, Length Y) point) => new(Quantities.As(point.X, doc.PageUnits), Quantities.As(point.Y, doc.PageUnits), 0);

        IO<Guid> Row(Option<LineWidth> width, Func<Fin<GeometryBase>> shape) =>
            TableOps.WithAttributes(doc.CreateDefaultAttributes, None, attributes =>
                from placed in IO.lift(() => { (attributes.LayerIndex, attributes.Space, attributes.ViewportId) = (layer, ActiveSpace.PageSpace, page.MainViewport.Id); })
                from weighted in IO.lift(() => width.Iter(stroke =>
                    (attributes.PlotWeightSource, attributes.PlotWeight) = (ObjectPlotWeightSource.PlotWeightFromObject, PlotWeight.Of(stroke).ToHost())))
                from tagged in IO.lift(() => Refused.Unless(attributes.SetUserString(FrameKey, sheet.Size.Designation), nameof(ObjectAttributes.SetUserString)))
                from geometry in use(IO.lift(shape))
                from id in TableOps.Add(doc, new GeometryPair(geometry, Some(attributes)), None, reference: false)
                select id);
    }
}
