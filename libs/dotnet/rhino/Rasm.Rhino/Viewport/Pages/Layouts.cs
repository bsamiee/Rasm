using System.Collections.Generic;
using System.Globalization;
using System.IO.Hashing;
using LanguageExt.ClassInstances;
using Rasm.Drafting;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Runtime;

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
        from page in Find(doc, sheet.Name).Match(Some: IO.pure, None: () => Products.Of(() => doc.Views.AddPageView(sheet.Name, extent.Width, extent.Height, setActive: false)))
        from held in SheetOf(doc, page)
        from sized in unless(held == Some(sheet.Sheet), IO.lift(() => { (page.PageWidth, page.PageHeight) = extent; })).As()
        from described in IO.lift(() => sheet.Description.Iter(text => Update(page.Description, text, value => page.Description = value)))
        from printed in IO.lift(() => sheet.Printer.Iter(name => Update(page.PrinterName, name, value => page.PrinterName = value)))
        from framed in sheet.Frame.Traverse(style => Frame(doc, page, sheet.Sheet, style)).As()
        select page;

    public static IO<Option<Sheet>> SheetOf(RhinoDoc doc, RhinoPageView page) =>
        IO.lift(() => Sheet.Match(Quantities.From(page.PageWidth, doc.PageUnits), Quantities.From(page.PageHeight, doc.PageUnits), Quantities.From(doc.PageAbsoluteTolerance, doc.PageUnits)));

    public static IO<RhinoPageView> Duplicate(RhinoPageView page, LayoutName name, bool duplicatePageGeometry) =>
        from copy in Products.Of(() => page.Duplicate(duplicatePageGeometry))
        from named in IO.lift(() => copy.PageName = name)
        select copy;

    private static Option<RhinoPageView> Find(RhinoDoc doc, LayoutName name) =>
        Viewports.Pages(doc.Views.GetPageViews()).Find(page => name == page.PageName);

    private static Unit Update<T>(T held, T wanted, Action<T> write) =>
        EqualityComparer<T>.Default.Equals(held, wanted) ? unit : fun(write)(wanted);

    // --- [TITLES]
    public static IO<Unit> Title(RhinoDoc doc, RhinoPageView page, DraftingConvention convention, HashMap<TitleField, string> entries) =>
        from accepted in IO.lift(TitleField.Accept(convention, entries))
        from written in accepted.TraverseM(entry => entry.Field.Entry.Switch(
            (Doc: doc, Page: page, entry.Field.Key, entry.Text),
            projectEntry: static (state, _) => IO.lift(() => KeyName.Validated(state.Key))
                .Bind(name => UserStrings.Write(state.Doc, new DocumentKey(name), state.Text)).Map(static _ => unit),
            sheetEntry: static (state, _) => UserStrings.Write(state.Page.MainViewport, toHashMap<EqStringOrdinalIgnoreCase, string, Option<string>>(Seq((state.Key, state.Text)))),
            sheetName: static (_, _) => IO.pure(unit),
            sheetFact: static (_, _) => IO.pure(unit))).As()
        select unit;

    public static IO<Seq<(TextFields.InstanceAttributeField Attribute, Option<TitleField> Field)>> TitleFields(InstanceDefinition block) =>
        IO.lift(() => toSeq(TextFields.GetInstanceAttributeFields(block))
            .Map(static attribute => (Attribute: attribute, Field: TitleField.TryGet(attribute.Key, out TitleField? field) ? Some(field) : None))
            .Strict());

    // --- [ZONES]
    public static IO<Option<string>> Zone(RhinoDoc doc, RhinoObject placed) =>
        IO.lift(() => toSeq(doc.Views.GetPageViews()).Find(page => placed.Attributes.Space is ActiveSpace.PageSpace && page.MainViewport.Id == placed.Attributes.ViewportId))
            .Bind(found => found.Match(
                Some: page => SheetOf(doc, page).Map(sheet =>
                    from held in sheet
                    from center in Conversions.Present(placed.Geometry.GetBoundingBox(accurate: true)).Map(static box => box.Center)
                    from zone in held.Locate(Quantities.From(center.X, doc.PageUnits), Quantities.From(center.Y, doc.PageUnits))
                    select zone),
                None: static () => IO.pure(Option<string>.None)));

    // --- [GROUPS]
    public static IO<PageViewGroup> Ensure(RhinoDoc doc, LayoutGroup grouping) =>
        from members in IO.lift(() => (Conversions.Unique(grouping.Members, identity),
                grouping.Members.Traverse(name => Find(doc, name).ToFin(new RhinoFault(name, RhinoReason.Missing))).As())
            .Apply(static (_, pages) => pages).As())
        from row in Rows.Find(doc.PageViewGroups, new ComponentRef<PageViewGroup>.ByName(grouping.Name))
            .Catch(static error => error.Filter<RhinoFault>().Head is RhinoFault fault && fault.Reason == RhinoReason.Missing, _ => Add(doc, grouping.Name))
        from described in IO.lift(() => grouping.Description.Iter(text => Update(row.Description, text, value => row.Description = value)))
        from left in IO.lift(() => toSeq(row.GetMembers().Except(members)).Iter(member => member.RemoveFromPageViewGroup(row.Index)))
        from placed in IO.lift(() => members.Iter((position, page) => Update(page.PageViewGroupSortIndex(row.Index), position, index => page.SetPageViewGroupSortIndex(row.Index, index))))
        select row;

    private static IO<PageViewGroup> Add(RhinoDoc doc, LayoutName name) =>
        (from staged in use(() => new PageViewGroup { Name = name })
         from index in IO.lift(() => Conversions.Required(Conversions.Present(doc.PageViewGroups.Add(staged))))
         from row in Rows.Find(doc.PageViewGroups, new ComponentRef<PageViewGroup>.ByIndex(index))
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

    public static IO<Unit> NumberFollowsPages(NamingStandard grammar) =>
        from post in Mailboxes.Idle<uint, Unit>(serials =>
            Products.Each(toSeq(serials.Keys).Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial))).Map(doc => Number(doc, grammar))).Map(unit))
        from attached in Subscriptions.Merged(
                Subscriptions.Merged(
                        Subscriptions.On<ViewEventArgs>(static h => RhinoView.Create += h, static h => RhinoView.Create -= h),
                        Subscriptions.On<ViewEventArgs>(static h => RhinoView.Destroy += h, static h => RhinoView.Destroy -= h),
                        Subscriptions.On<ViewEventArgs>(static h => RhinoView.Rename += h, static h => RhinoView.Rename -= h))
                    .Choose(static args => Optional(args.View as RhinoPageView).Bind(static page => Conversions.Serial(page.Document))),
                Subscriptions.On<PageViewPropertiesChangeEventArgs>(static h => RhinoPageView.PageViewPropertiesChange += h, static h => RhinoPageView.PageViewPropertiesChange -= h)
                    .Choose(static args => Some(args.DocumentSerialNumber)))
            .Attach(serial => post(serial, unit))
        select unit;

    // --- [FRAMES]
    private const string FrameKey = "sheet-frame";

    private static IO<Unit> Frame(RhinoDoc doc, RhinoPageView page, Sheet sheet, FrameStyle style) =>
        from layer in Rows.Find(doc.Layers, style.Layer)
        from lettering in Rows.Find(doc.DimStyles, style.Lettering)
        let marks = Marks(doc, style, lettering, sheet)
        let name = $"{FrameKey} {ContentKey.Of<FrameStyle>(stream => marks.Fold(stream.Integer(layer.Index), static (held, mark) => mark.Fields(held))).Id:N}"
        from definition in Defined(doc, name, layer.Index, marks)
        from present in new ObjectTarget.Lookup(objects => objects.FindByUserString(FrameKey, "*", caseSensitive: true, searchGeometry: false, searchAttributes: true,
                new ObjectEnumeratorSettings { SpaceFilter = ActiveSpace.PageSpace, ViewportFilter = page.MainViewport, HiddenObjects = true, LockedObjects = true }))
            .Objects(doc).Map(static found => found.Map(static mark => (Key: mark.Attributes.GetUserString(FrameKey), mark.Id)).Strict())
        let stale = present.Filter(mark => !string.Equals(mark.Key, name, StringComparison.Ordinal))
        from cleared in when(!stale.IsEmpty, new ObjectTarget.Ids(stale.Map(static mark => mark.Id)).Each(doc, found => doc.Objects.Delete(found, quiet: true, ignoreModes: true)).Map(unit)).As()
        from placed in unless(present.Exists(mark => string.Equals(mark.Key, name, StringComparison.Ordinal)),
            GeometryPair.WithAttributes(doc.CreateDefaultAttributes, None, attributes => IO.lift(() => {
                (attributes.LayerIndex, attributes.Space, attributes.ViewportId) = (layer.Index, ActiveSpace.PageSpace, page.MainViewport.Id);
                return RhinoFault.Unless(attributes.SetUserString(FrameKey, name), RhinoReason.Declined, name)
                    .Bind(_ => Conversions.Required(Conversions.Present(doc.Objects.AddInstanceObject(definition, Transform.Identity, attributes))));
            })).Map(unit)).As()
        from purged in Products.Each(stale.Map(static mark => mark.Key).Distinct().Map(key => IO.lift(() => Optional(doc.InstanceDefinitions.Find(key))
            .Filter(static unused => unused.GetReferences(0).Length is 0)
            .Iter(unused => doc.InstanceDefinitions.Delete(unused.Index, deleteReferences: false, quiet: true)))))
        select unit;

    private static IO<int> Defined(RhinoDoc doc, string name, int layer, Seq<(Func<XxHash128, XxHash128> Fields, Option<double> PlotWeight, Func<Fin<GeometryBase>> Shape)> marks) =>
        Optional(doc.InstanceDefinitions.Find(name)).Match(
            Some: static held => IO.pure(held.Index),
            None: () => (from shapes in marks.TraverseM(static mark => IO.lift(mark.Shape).Bind(static made => use(() => made))).As()
                         from index in GeometryPair.WithAttributes(doc.CreateDefaultAttributes, shapes.Map(static shape => new GeometryPair(shape, None)), (geometry, attributes) => IO.lift(() => {
                             _ = attributes.Zip(marks).Iter(row => {
                                 row.First.LayerIndex = layer;
                                 _ = row.Second.PlotWeight.Iter(stroke => (row.First.PlotWeightSource, row.First.PlotWeight) = (ObjectPlotWeightSource.PlotWeightFromObject, stroke));
                             });
                             return Conversions.Required(Conversions.Present(doc.InstanceDefinitions.Add(name, FrameKey, Point3d.Origin, geometry, attributes)));
                         }))
                         select index).Bracket());

    private static Seq<(Func<XxHash128, XxHash128> Fields, Option<double> PlotWeight, Func<Fin<GeometryBase>> Shape)> Marks(RhinoDoc doc, FrameStyle style, DimensionStyle lettering, Sheet sheet) =>
        sheet.Frame.Map(mark => mark.Switch(
            (Doc: doc, Style: style, Lettering: lettering),
            stroke: static (state, stroke) => (PlotWeight.Of(state.Style.Lines.Width(stroke.Weight)).ToHost(), stroke.Points.Map(point => Placed(state.Doc, point)).Strict()) switch {
                var (plotted, points) => ((Func<XxHash128, XxHash128>)(stream => stream.Text(nameof(FrameMark.Stroke)).Number(plotted).Rows(points, static (held, point) => held.Number(point.X).Number(point.Y))),
                    Some(plotted), (Func<Fin<GeometryBase>>)(() => new PolylineCurve(points))),
            },
            label: static (state, label) => (Center: Placed(state.Doc, label.Center), Height: Quantities.As(label.Height, state.Doc.PageUnits)) switch {
                var placed => (stream => stream.Text(nameof(FrameMark.Label)).Id(state.Lettering.Id).Number(placed.Height).Text(label.Text).Number(placed.Center.X).Number(placed.Center.Y),
                    Option<double>.None,
                    () => Conversions.Required(TextEntity.Create(label.Text, new Plane(placed.Center, Vector3d.XAxis, Vector3d.YAxis), state.Lettering, wrapped: false, rectWidth: 0, rotationRadians: 0))
                        .Map(entity => {
                            (entity.Justification, entity.TextHeight) = (TextJustification.MiddleCenter, placed.Height);
                            return (GeometryBase)entity;
                        })),
            }));

    private static Point3d Placed(RhinoDoc doc, (Length X, Length Y) point) => new(Quantities.As(point.X, doc.PageUnits), Quantities.As(point.Y, doc.PageUnits), 0);
}
