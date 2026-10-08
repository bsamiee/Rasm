using Rasm.Drafting;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using UnitsNet;

namespace Rasm.Rhino.Viewport.Paper;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DetailState(
    Option<string> Name = default, Option<DrawingScale> Scale = default, Option<bool> ProjectionLocked = default, Option<bool> Active = default) {
    public DetailState Except(DetailState held) =>
        new(Name.Filter(name => held.Name != name), Scale.Filter(scale => held.Scale != scale),
            ProjectionLocked.Filter(locked => held.ProjectionLocked != locked), Active.Filter(active => held.Active != active));
}

public sealed record DetailSnapshot(
    Guid Id, Option<string> Title, BoundingBox Frame, Option<(Transform WorldToPage, Transform PageToWorld)> Mapping, DetailState State);

[SmartEnum]
public sealed partial class FrameAlignment {
    public static readonly FrameAlignment Start = new(0d);
    public static readonly FrameAlignment Center = new(0.5d);
    public static readonly FrameAlignment End = new(1d);

    public double Fraction { get; }

    public double Place(global::Rhino.Geometry.Interval span, double size) => span.T0 + (Fraction * (span.Length - size));
}

[SmartEnum]
public sealed partial class PaperAxis {
    public static readonly PaperAxis Horizontal = new(Vector3d.XAxis, static point => point.X);
    public static readonly PaperAxis Vertical = new(Vector3d.YAxis, static point => point.Y);

    public Vector3d Direction { get; }

    [UseDelegateFromConstructor]
    public partial double Coordinate(Point3d point);

    public global::Rhino.Geometry.Interval Span(BoundingBox box) => new(Coordinate(box.Min), Coordinate(box.Max));

    public Vector3d Offset(BoundingBox frame, double start) => Direction * (start - Coordinate(frame.Min));

    public Vector3d Align(BoundingBox frame, global::Rhino.Geometry.Interval span, FrameAlignment alignment) => Offset(frame, alignment.Place(span, Span(frame).Length));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Arrangement {
    public sealed record Grid(Seq<Seq<Guid>> Rows, FrameAlignment Across, FrameAlignment Down) : Arrangement;

    public sealed record Align(Seq<Guid> Ids, PaperAxis Axis, FrameAlignment Alignment) : Arrangement;

    public sealed record Distribute(Seq<Guid> Ids, PaperAxis Axis) : Arrangement;

    public Seq<Guid> Details =>
        Switch(grid: static grid => grid.Rows.Flatten(), align: static align => align.Ids, distribute: static distribute => distribute.Ids);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Details {
    // --- [READS]
    public static IO<Seq<DetailSnapshot>> Read(RhinoDoc doc, RhinoPageView page) =>
        IO.lift(() => toSeq(page.GetDetailViews()).Traverse(detail => Snapshot(detail, doc.PageUnits, doc.ModelUnits).ToValidation()).As().ToFin());

    private static Fin<DetailSnapshot> Snapshot(DetailViewObject detail, LengthUnit page, LengthUnit model) =>
        from geometry in Geometry(detail)
        from scale in Scale(detail, geometry, page, model)
        select new DetailSnapshot(
            detail.Id,
            Conversions.Present(detail.DescriptiveTitle),
            geometry.GetBoundingBox(accurate: true),
            (Conversions.Present(detail.WorldToPageTransform), Conversions.Present(detail.PageToWorldTransform))
                .Apply(static (toPage, toWorld) => (WorldToPage: toPage, PageToWorld: toWorld)).As(),
            new DetailState(Conversions.Present(detail.Attributes.Name), scale, geometry.IsProjectionLocked, detail.IsActive));

    private static Fin<Option<DrawingScale>> Scale(DetailViewObject detail, DetailView geometry, LengthUnit page, LengthUnit model) =>
        Callbacks.Found(detail.TryGetModelLength(1d, out double length), length)
            .Filter(_ => geometry.IsParallelProjection)
            .Traverse(found => Conversions.Validated<DrawingScale, QuantityValue, InvalidDrafting>(Quantities.From(1d, page) / Quantities.From(found, model)))
            .As();

    private static Fin<DetailView> Geometry(DetailViewObject detail) =>
        Missing.Unless(detail.DetailGeometry, nameof(DetailViewObject.DetailGeometry));

    // --- [WRITES]
    public static IO<Guid> Add(RhinoDoc doc, RhinoPageView page, string title, BoundingBox frame, DefinedViewportProjection projection) =>
        Viewports.WithActiveView(doc, page, IO.lift(() =>
            Missing.Unless(page.AddDetailView(title, new Point2d(frame.Min.X, frame.Min.Y), new Point2d(frame.Max.X, frame.Max.Y), projection), nameof(RhinoPageView.AddDetailView))
                .Map(static detail => detail.Id)));

    public static IO<Unit> Write(RhinoDoc doc, Guid detailId, DetailState wanted) =>
        from detail in Viewports.ResolveDetail(doc, detailId)
        from units in IO.lift(() => (Page: doc.PageUnits, Model: doc.ModelUnits))
        from change in IO.lift(() => Snapshot(detail, units.Page, units.Model).Map(held => wanted.Except(held.State)))
        from edited in IO.lift(() => Edit(detail, change, units.Page, units.Model))
        from activated in IO.lift(() => change.Active.Iter(active => detail.IsActive = active))
        from named in change.Name.Match(
            Some: name => TableOps.Apply(doc, new TableOp.ModifyAttributes(new ObjectTarget.Ids(Seq(detailId)), attributes => IO.lift(() => { attributes.Name = name; }), Quiet: true))
                .Map(static _ => unit),
            None: static () => IO.pure(unit))
        select named;

    private static Fin<Unit> Edit(DetailViewObject detail, DetailState change, LengthUnit page, LengthUnit model) =>
        change.Scale.IsNone && change.ProjectionLocked.IsNone
            ? unit
            : from geometry in Geometry(detail)
              from scaled in change.Scale.Traverse(scale => Refused.Unless(
                  geometry.SetScale(Quantities.As(scale.ToModel(Quantities.From(1d, page)), model), model, 1d, page), nameof(DetailView.SetScale))).As()
              from locked in Fin.Succ(change.ProjectionLocked.Iter(value => geometry.IsProjectionLocked = value))
              from committed in Refused.Unless(detail.CommitChanges(), nameof(RhinoObject.CommitChanges))
              select committed;

    // --- [ARRANGEMENT]
    public static BoundingBox Field(PaperRectangle field, LengthUnit page) =>
        new(Quantities.As(field.Left, page), Quantities.As(field.Bottom, page), 0d, Quantities.As(field.Right, page), Quantities.As(field.Top, page), 0d);

    public static BoundingBox Clear(BoundingBox field, BoundingBox keepOut) =>
        BoundingBox.Intersection(field, new BoundingBox(keepOut.Min.X, keepOut.Min.Y, field.Min.Z, keepOut.Max.X, keepOut.Max.Y, field.Max.Z)) is { IsValid: true } overlap
            ? Seq(
                    new BoundingBox(field.Min, new Point3d(overlap.Min.X, field.Max.Y, field.Max.Z)),
                    new BoundingBox(new Point3d(overlap.Max.X, field.Min.Y, field.Min.Z), field.Max),
                    new BoundingBox(field.Min, new Point3d(field.Max.X, overlap.Min.Y, field.Max.Z)),
                    new BoundingBox(new Point3d(field.Min.X, overlap.Max.Y, field.Min.Z), field.Max))
                .MaxBy(static box => box.Diagonal.X * box.Diagonal.Y)
            : field;

    public static Seq<(TItem Item, BoundingBox Cell)> Cells<TItem>(BoundingBox region, Seq<Seq<TItem>> rows) =>
        rows.Map((row, r) => row.Map((item, c) => (Item: item, Cell: new BoundingBox(
            region.Min.X + (region.Diagonal.X * c / row.Count), region.Max.Y - (region.Diagonal.Y * (r + 1) / rows.Count), region.Min.Z,
            region.Min.X + (region.Diagonal.X * (c + 1) / row.Count), region.Max.Y - (region.Diagonal.Y * r / rows.Count), region.Max.Z)))).Flatten();

    public static Seq<(Guid Id, Vector3d Offset)> Offsets(BoundingBox region, Arrangement arrangement, HashMap<Guid, BoundingBox> frames) =>
        arrangement.Switch(
            (Region: region, Frames: frames),
            grid: static (state, grid) => Cells(state.Region, grid.Rows).Map(cell => (Id: cell.Item, Offset:
                PaperAxis.Horizontal.Align(state.Frames[cell.Item], PaperAxis.Horizontal.Span(cell.Cell), grid.Across)
                + PaperAxis.Vertical.Align(state.Frames[cell.Item], PaperAxis.Vertical.Span(cell.Cell), grid.Down))),
            align: static (state, align) => align.Ids.Map(id => (Id: id, Offset: align.Axis.Align(state.Frames[id], align.Axis.Span(state.Region), align.Alignment))),
            distribute: static (state, distribute) =>
                Spaced(distribute.Axis, distribute.Axis.Span(state.Region), distribute.Ids.Map(id => (Id: id, Frame: state.Frames[id]))));

    public static IO<Seq<Guid>> Arrange(RhinoDoc doc, BoundingBox region, Arrangement arrangement) =>
        from ids in IO.lift(Callbacks.Unique(arrangement.Details, identity, nameof(Arrange)).ToFin())
        from frames in ids.TraverseM(id => Viewports.ResolveDetail(doc, id)
            .Bind(detail => IO.lift(() => Geometry(detail).Map(geometry => (Key: id, Value: geometry.GetBoundingBox(accurate: true)))))).As()
        from moved in Offsets(region, arrangement, toHashMap(frames)).Filter(static row => !row.Offset.IsZero).TraverseM(row =>
            TableOps.Apply(doc, new TableOp.Move(new ObjectTarget.Ids(Seq(row.Id)), Transform.Translation(row.Offset), new TransformMode.Move(DeleteOriginal: true)))).As()
        select moved.Flatten();

    private static Seq<(Guid Id, Vector3d Offset)> Spaced(PaperAxis axis, global::Rhino.Geometry.Interval field, Seq<(Guid Id, BoundingBox Frame)> rows) =>
        fun((double gap) => rows.Zip(rows.Scan(field.T0 + gap, (start, row) => start + axis.Span(row.Frame).Length + gap))
            .Map(pair => (pair.First.Id, Offset: axis.Offset(pair.First.Frame, pair.Second))))(
            (field.Length - rows.Fold(0d, (sum, row) => sum + axis.Span(row.Frame).Length)) / (rows.Count + 1));
}
