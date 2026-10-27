using System.Collections.Generic;
using System.Globalization;
using System.IO.Hashing;
using LanguageExt.ClassInstances;
using Rasm.Drafting;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Viewport.Pages;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads, ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[ValidationError<RhinoFault>]
public sealed partial class LayoutName {
    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value) ? new RhinoFault($"value = '{value}'", RhinoReason.Invalid) : null;
}

public sealed record FrameStyle(LineGroup Lines, ComponentRef<Layer> Layer, ComponentRef<DimensionStyle> Lettering);

public sealed record LayoutSheet(LayoutName Name, Sheet Sheet, Option<string> Description, Option<FrameStyle> Frame, Option<string> Printer);

public sealed record LayoutGroup(LayoutName Name, Option<string> Description, Seq<LayoutName> Members);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Layouts {
    // --- [SHEETS]
    public static IO<RhinoPageView> Ensure(RhinoDoc doc, LayoutSheet sheet) =>
        from extent in IO.lift(() => (Width: Quantities.As(sheet.Sheet.Extent.Width, doc.PageUnits), Height: Quantities.As(sheet.Sheet.Extent.Height, doc.PageUnits)))
        from page in IO.lift(() => Find(doc, sheet.Name).Match(Some: Fin.Succ, None: () => Conversions.Found(doc.Views.AddPageView(sheet.Name, extent.Width, extent.Height, setActive: false))))
        from held in SheetOf(doc, page)
        from sized in unless(held == Some(sheet.Sheet), IO.lift(() => { (page.PageWidth, page.PageHeight) = extent; })).As()
        from described in IO.lift(() => sheet.Description.Iter(text => Update(page.Description, text, value => page.Description = value)))
        from printed in IO.lift(() => sheet.Printer.Iter(name => Update(page.PrinterName, name, value => page.PrinterName = value)))
        from framed in sheet.Frame.Traverse(style => Frame(doc, page, sheet.Sheet, style)).As()
        select page;

    public static IO<Option<Sheet>> SheetOf(RhinoDoc doc, RhinoPageView page) =>
        IO.lift(() => Sheet.Match(Quantities.From(page.PageWidth, doc.PageUnits), Quantities.From(page.PageHeight, doc.PageUnits), Quantities.From(doc.PageAbsoluteTolerance, doc.PageUnits)));

    public static IO<RhinoPageView> Duplicate(RhinoPageView page, LayoutName name, bool duplicatePageGeometry) =>
        from copy in IO.lift(() => Conversions.Found(page.Duplicate(duplicatePageGeometry)))
        from named in IO.lift(() => copy.PageName = name)
        select copy;

    private static Option<RhinoPageView> Find(RhinoDoc doc, LayoutName name) =>
        Viewports.Pages(doc.Views.GetPageViews()).Find(page => name == page.PageName);

    private static Unit Update<T>(T held, T wanted, Action<T> write) =>
        EqualityComparer<T>.Default.Equals(held, wanted) ? unit : fun(write)(wanted);

    // --- [TITLES]
    public static IO<Unit> Title(RhinoDoc doc, RhinoPageView page, DraftingConvention convention, HashMap<TitleField, string> entries) =>
        from accepted in IO.lift(TitleField.Accept(convention, entries))
        from written in accepted.TraverseM(entry => entry.Field.Switch(
            (Doc: doc, Page: page, entry.Text),
            projectEntry: static (state, field) => IO.lift(() => KeyName.TryCreate(field.Key, out KeyName? name, out RhinoFault? fault) ? Fin.Succ(name) : fault)
                .Bind(name => UserStrings.Write(state.Doc, new DocumentKey(name), state.Text)).Map(static _ => unit),
            sheetEntry: static (state, field) => UserStrings.Write(state.Page.MainViewport, toHashMap<EqStringOrdinalIgnoreCase, string, Option<string>>(Seq((field.Key, state.Text)))),
            sheetName: static (_, _) => IO.pure(unit),
            sheetFact: static (_, _) => IO.pure(unit))).As()
        select unit;

    // --- [GROUPS]
    public static IO<PageViewGroup> Ensure(RhinoDoc doc, LayoutGroup grouping) =>
        from members in IO.lift(() => (Conversions.Unique(grouping.Members, identity),
                grouping.Members.Traverse(name => Find(doc, name).ToFin(new RhinoFault(name, RhinoReason.Missing))).As())
            .Apply(static (_, pages) => pages).As())
        from row in TableOps.Find(doc.PageViewGroups, new ComponentRef<PageViewGroup>.ByName(grouping.Name), includeDeleted: false)
            .Catch(static error => error.Filter<RhinoFault>().Head is RhinoFault fault && fault.Reason == RhinoReason.Missing, _ => Add(doc, grouping.Name))
        from described in IO.lift(() => grouping.Description.Iter(text => Update(row.Description, text, value => row.Description = value)))
        from left in IO.lift(() => toSeq(row.GetMembers().Except(members)).Iter(member => member.RemoveFromPageViewGroup(row.Index)))
        from placed in IO.lift(() => members.Iter((position, page) => Update(page.PageViewGroupSortIndex(row.Index), position, index => page.SetPageViewGroupSortIndex(row.Index, index))))
        select row;

    private static IO<PageViewGroup> Add(RhinoDoc doc, LayoutName name) =>
        (from staged in use(static () => new PageViewGroup())
         from named in TableOps.Named(staged, Some<string>(name))
         from index in IO.lift(() => Conversions.Found(Conversions.Present(doc.PageViewGroups.Add(staged))))
         from row in TableOps.Find(doc.PageViewGroups, index, includeDeleted: false)
         select row).Bracket();

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

    public static Func<IPlugInViews, IO<Unit>> NumberFollowsPages(NamingStandard grammar) =>
        views =>
            from post in Mailboxes.Idle<uint, Unit>(serials =>
                Callbacks.Each(toSeq(serials.Keys).Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial))).Map(doc => Number(doc, grammar))).Map(unit))(views)
            from attached in Subscriptions.Merged(
                    Seq(EventKind.RhinoViewCreate, EventKind.RhinoViewDestroy, EventKind.RhinoViewRename)
                        .Map(static row => row.Choose(args => row.Serial(args).Filter(_ => args.View is RhinoPageView)))
                        .Add(EventKind.PageViewPropertiesChange.Choose(static args => EventKind.PageViewPropertiesChange.Serial(args))))
                .Inline(serial => post(serial, unit), views)
            select unit;

    // --- [FRAMES]
    private const string FrameKey = "sheet-frame";

    private static IO<Unit> Frame(RhinoDoc doc, RhinoPageView page, Sheet sheet, FrameStyle style) =>
        from layer in TableOps.Find(doc.Layers, style.Layer, includeDeleted: false)
        from lettering in TableOps.Find(doc.DimStyles, style.Lettering, includeDeleted: false)
        let wanted = Marks(doc, page, style, layer.Index, lettering, sheet).Fold(HashMap<string, Func<IO<Guid>>>(), static (held, mark) => held.AddOrUpdate(mark.Key, mark.Add))
        from present in new ObjectTarget.Lookup(objects => objects.FindByUserString(FrameKey, "*", caseSensitive: true, searchGeometry: false, searchAttributes: true,
                new ObjectEnumeratorSettings { SpaceFilter = ActiveSpace.PageSpace, ViewportFilter = page.MainViewport, HiddenObjects = true, LockedObjects = true }))
            .Objects(doc).Map(static found => found.Map(static mark => (Key: mark.Attributes.GetUserString(FrameKey), mark.Id)).Strict())
        let stale = present.Filter(mark => !wanted.ContainsKey(mark.Key)).Map(static mark => mark.Id)
        let kept = toHashSet(present.Map(static mark => mark.Key))
        from cleared in when(!stale.IsEmpty, TableOps.Apply(doc, new TableOp.Delete(new ObjectTarget.Ids(stale), Quiet: true, IgnoreModes: true)).Map(unit)).As()
        from added in Callbacks.Each(wanted.Filter((key, _) => !kept.Contains(key)).Values.ToSeq().Map(static add => add()))
        select unit;

    private static Seq<(string Key, Func<IO<Guid>> Add)> Marks(RhinoDoc doc, RhinoPageView page, FrameStyle style, int layer, DimensionStyle lettering, Sheet sheet) {
        (PaperRectangle field, ZoneGrid zones, (Length width, Length height), double letterHeight) =
            (sheet.Field, sheet.Zones, sheet.Extent, Quantities.As(sheet.Size.Convention.Lettering.ZoneHeight, doc.PageUnits));
        Option<Length> reach = sheet.Size.Convention.Centering.Map(static centering => centering.Reach);
        Option<Length> blocked = sheet.Size.Convention.Centering.Map(centering => width / 2 < field.Right - centering.TitleBlock ? centering.Reach : Length.Zero);
        return Stroke(LineWeight.ExtraWide, toSeq(new Rectangle3d(Plane.WorldXY, At(field.Left, field.Bottom), At(field.Right, field.Top)).ToPolyline()))
            + Axis(zones.Columns, field.Left, field.Right, width, zones.Column, At, [(field.Top, height, true, reach), (field.Bottom, Length.Zero, zones.AllEdges, blocked)])
            + Axis(zones.Rows, field.Bottom, field.Top, height, zones.Row, (along, across) => At(across, along), [(field.Right, width, true, reach), (field.Left, Length.Zero, zones.AllEdges, reach)]);

        Seq<(string Key, Func<IO<Guid>> Add)> Axis(Seq<Length> divisions, Length near, Length far, Length edge, Func<int, string> label, Func<Length, Length, Point3d> at, Seq<(Length Inner, Length Outer, bool Zoned, Option<Length> Reach)> bands) =>
            (from band in bands
             where band.Zoned
             from mark in divisions.Map(division => Stroke(LineWeight.Wide, [at(division, band.Inner), at(division, band.Outer)]))
                 + near.Cons(divisions).Zip(divisions.Add(far)).Map((span, index) => Label(label(index), at((span.First + span.Second) / 2, (band.Inner + band.Outer) / 2)))
             select mark)
            + (from band in bands
               from inward in band.Reach.ToSeq()
               select Stroke(LineWeight.ExtraWide, [at(edge / 2, band.Outer), at(edge / 2, band.Inner < band.Outer ? band.Inner - inward : band.Inner + inward)]));

        Point3d At(Length x, Length y) => new(Quantities.As(x, doc.PageUnits), Quantities.As(y, doc.PageUnits), 0);

        (string Key, Func<IO<Guid>> Add) Stroke(LineWeight weight, Seq<Point3d> points) =>
            PlotWeight.Of(style.Lines.Width(weight)).ToHost() switch {
                var plotted => Row(
                    Keyed(stream => stream.Text(nameof(Stroke)).Number(plotted).Rows(points, static (held, point) => held.Number(point.X).Number(point.Y))),
                    Some(plotted), () => new PolylineCurve(points)),
            };

        (string Key, Func<IO<Guid>> Add) Label(string text, Point3d center) =>
            Row(Keyed(stream => stream.Text(nameof(Label)).Id(lettering.Id).Number(letterHeight).Text(text).Number(center.X).Number(center.Y)), None,
                () => Conversions.Found(TextEntity.Create(text, new Plane(center, Vector3d.XAxis, Vector3d.YAxis), lettering, wrapped: false, rectWidth: 0, rotationRadians: 0))
                    .Map(entity => { (entity.Justification, entity.TextHeight) = (TextJustification.MiddleCenter, letterHeight); return (GeometryBase)entity; }));

        string Keyed(Func<XxHash128, XxHash128> fields) =>
            string.Create(CultureInfo.InvariantCulture, $"{ContentKey.Of<FrameStyle>(stream => fields(stream.Integer(layer)))}");

        (string Key, Func<IO<Guid>> Add) Row(string key, Option<double> plotWeight, Func<Fin<GeometryBase>> shape) =>
            (key, () => TableOps.WithAttributes(doc.CreateDefaultAttributes, None, attributes =>
                from placed in IO.lift(() => { (attributes.LayerIndex, attributes.Space, attributes.ViewportId) = (layer, ActiveSpace.PageSpace, page.MainViewport.Id); })
                from weighted in IO.lift(() => plotWeight.Iter(stroke => (attributes.PlotWeightSource, attributes.PlotWeight) = (ObjectPlotWeightSource.PlotWeightFromObject, stroke)))
                from tagged in IO.lift(() => RhinoFault.Unless(attributes.SetUserString(FrameKey, key)))
                from id in (from geometry in IO.lift(shape).Bind(static made => use(() => made))
                            from added in TableOps.Add(doc, new GeometryPair(geometry, Some(attributes)), None, reference: false)
                            select added).Bracket()
                select id));
    }
}
