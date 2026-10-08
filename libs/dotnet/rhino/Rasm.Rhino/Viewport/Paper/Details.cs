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
public sealed record DetailState(Option<string> Name = default, Option<DrawingScale> Scale = default, Option<bool> ProjectionLocked = default, Option<bool> Active = default) {
    public DetailState Except(DetailState held) =>
        new(Name.Filter(name => held.Name != name), Scale.Filter(scale => held.Scale != scale),
            ProjectionLocked.Filter(locked => held.ProjectionLocked != locked), Active.Filter(active => held.Active != active));
}

public sealed record DetailSnapshot(Guid Id, Option<string> Title, BoundingBox Frame, Option<(Transform WorldToPage, Transform PageToWorld)> Mapping, DetailState State);

[SmartEnum]
public sealed partial class FrameAlignment {
    public static readonly FrameAlignment Start = new(0d);
    public static readonly FrameAlignment Center = new(0.5d);
    public static readonly FrameAlignment End = new(1d);

    private readonly double _fraction;

    public Vector3d Shift(BoundingBox frame, BoundingBox target) => target.PointAt(_fraction, _fraction, _fraction) - frame.PointAt(_fraction, _fraction, _fraction);
}

[SmartEnum]
public sealed partial class PaperAxis {
    public static readonly PaperAxis Horizontal = new(Vector3d.XAxis);
    public static readonly PaperAxis Vertical = new(Vector3d.YAxis);

    private readonly Vector3d _direction;

    public Vector3d Along(Vector3d offset) => _direction * (offset * _direction);
}

[Union]
public abstract partial record Arrangement {
    public sealed record Grid(Seq<Seq<Guid>> Rows, FrameAlignment Across, FrameAlignment Down) : Arrangement;

    public sealed record Align(Seq<Guid> Ids, PaperAxis Axis, FrameAlignment Alignment) : Arrangement;

    public sealed record Distribute(Seq<Guid> Ids, PaperAxis Axis) : Arrangement;

    public Seq<Guid> Details => Switch(grid: static grid => grid.Rows.Flatten(), align: static align => align.Ids, distribute: static distribute => distribute.Ids);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Details {
    // --- [READS]
    public static IO<Seq<DetailSnapshot>> Read(RhinoDoc doc, RhinoPageView page) =>
        IO.lift(() => Callbacks.Each(toSeq(page.GetDetailViews()), (detail, _) => Geometry(detail).Bind(geometry => Snapshot(doc, detail, geometry))));

    private static Fin<DetailSnapshot> Snapshot(RhinoDoc doc, DetailViewObject detail, DetailView geometry) =>
        Callbacks.Found(detail.TryGetModelLength(1d, out double length), length).Filter(_ => geometry.IsParallelProjection)
            .Traverse(found => Conversions.Validated<DrawingScale, QuantityValue, InvalidDrafting>(Quantities.From(1d, doc.PageUnits) / Quantities.From(found, doc.ModelUnits))).As()
            .Map(scale => new DetailSnapshot(detail.Id, Conversions.Present(detail.DescriptiveTitle), geometry.GetBoundingBox(accurate: true),
                (Conversions.Present(detail.WorldToPageTransform), Conversions.Present(detail.PageToWorldTransform)).Apply(ValueTuple.Create).As(),
                new DetailState(Conversions.Present(detail.Attributes.Name), scale, geometry.IsProjectionLocked, detail.IsActive)));

    private static Fin<DetailView> Geometry(DetailViewObject detail) =>
        Missing.Unless(detail.DetailGeometry, nameof(DetailViewObject.DetailGeometry));

    // --- [WRITES]
    public static IO<Guid> Add(RhinoDoc doc, RhinoPageView page, string title, BoundingBox frame, DefinedViewportProjection projection) =>
        Viewports.WithActiveView(doc, page, IO.lift(() =>
            Missing.Unless(page.AddDetailView(title, new Point2d(frame.Min), new Point2d(frame.Max), projection), nameof(RhinoPageView.AddDetailView)).Map(static detail => detail.Id)));

    public static IO<Unit> Write(RhinoDoc doc, Guid detailId, DetailState wanted) =>
        from detail in Viewports.ResolveDetail(doc, detailId)
        from geometry in IO.lift(() => Geometry(detail))
        from held in IO.lift(() => Snapshot(doc, detail, geometry))
        let change = wanted.Except(held.State)
        from scaled in change.Scale.Traverse(scale => IO.lift(() => Refused.Unless(geometry.SetScale(
            Quantities.As(scale.ToModel(Quantities.From(1d, doc.PageUnits)), doc.ModelUnits), doc.ModelUnits, 1d, doc.PageUnits), nameof(DetailView.SetScale)))).As()
        from locked in IO.lift(() => change.ProjectionLocked.Iter(value => geometry.IsProjectionLocked = value))
        from committed in IO.lift(() => change.Scale.IsNone && change.ProjectionLocked.IsNone ? unit : Refused.Unless(detail.CommitChanges(), nameof(RhinoObject.CommitChanges)))
        from activated in IO.lift(() => change.Active.Iter(active => detail.IsActive = active))
        from named in change.Name.Traverse(name =>
            TableOps.Apply(doc, new TableOp.ModifyAttributes(new ObjectTarget.Ids(Seq(detailId)), attributes => IO.lift(() => { attributes.Name = name; }), Quiet: true))).As()
        select unit;

    // --- [ARRANGEMENT]
    public static BoundingBox Field(PaperRectangle field, LengthUnit page) =>
        new(Quantities.As(field.Left, page), Quantities.As(field.Bottom, page), 0d, Quantities.As(field.Right, page), Quantities.As(field.Top, page), 0d);

    public static BoundingBox Clear(BoundingBox field, BoundingBox keepOut) =>
        BoundingBox.Intersection(field, new BoundingBox(keepOut.Min.X, keepOut.Min.Y, field.Min.Z, keepOut.Max.X, keepOut.Max.Y, field.Max.Z)) is { IsValid: true } overlap
            ? toSeq(PaperAxis.Items).Bind(axis => Seq(
                    new BoundingBox(field.Min, field.Max - axis.Along(field.Max - overlap.Min)), new BoundingBox(field.Min + axis.Along(overlap.Max - field.Min), field.Max)))
                .MaxBy(static box => box.Diagonal.X * box.Diagonal.Y)
            : field;

    public static Seq<(TItem Item, BoundingBox Cell)> Cells<TItem>(BoundingBox region, Seq<Seq<TItem>> rows) =>
        rows.Map((row, r) => row.Map((item, c) => (Item: item, Cell: new BoundingBox(
            region.PointAt((double)c / row.Count, 1d - ((r + 1d) / rows.Count), 0d), region.PointAt((c + 1d) / row.Count, 1d - ((double)r / rows.Count), 1d))))).Flatten();

    public static Seq<(Guid Id, Vector3d Offset)> Offsets(BoundingBox region, Arrangement arrangement, HashMap<Guid, BoundingBox> frames) =>
        arrangement.Switch((Region: region, Frames: frames),
            grid: static (state, grid) => Cells(state.Region, grid.Rows).Map(cell => (Id: cell.Item, Offset:
                PaperAxis.Horizontal.Along(grid.Across.Shift(state.Frames[cell.Item], cell.Cell)) + PaperAxis.Vertical.Along(grid.Down.Shift(state.Frames[cell.Item], cell.Cell)))),
            align: static (state, align) => align.Ids.Map(id => (Id: id, Offset: align.Axis.Along(align.Alignment.Shift(state.Frames[id], state.Region)))),
            distribute: static (state, distribute) => Spaced(distribute.Axis, state.Region, distribute.Ids.Map(id => (Id: id, Frame: state.Frames[id]))));

    public static IO<Seq<Guid>> Arrange(RhinoDoc doc, BoundingBox region, Arrangement arrangement) =>
        from ids in IO.lift(Callbacks.Unique(arrangement.Details, identity, nameof(Arrange)).ToFin())
        from frames in ids.TraverseM(id => Viewports.ResolveDetail(doc, id).Bind(detail => IO.lift(() => Geometry(detail).Map(geometry => (Key: id, Value: geometry.GetBoundingBox(accurate: true)))))).As()
        from moved in Offsets(region, arrangement, toHashMap(frames)).Filter(static row => !row.Offset.IsZero).TraverseM(row =>
            TableOps.Apply(doc, new TableOp.Move(new ObjectTarget.Ids(Seq(row.Id)), Transform.Translation(row.Offset), new TransformMode.Move(DeleteOriginal: true)))).As()
        select moved.Flatten();

    private static Seq<(Guid Id, Vector3d Offset)> Spaced(PaperAxis axis, BoundingBox region, Seq<(Guid Id, BoundingBox Frame)> rows) =>
        fun((Vector3d gap) => rows.Zip(rows.Scan(gap, (start, row) => start + axis.Along(row.Frame.Diagonal) + gap), (row, start) => (row.Id, Offset: axis.Along(region.Min - row.Frame.Min) + start)))(
            rows.Fold(axis.Along(region.Diagonal), (free, row) => free - axis.Along(row.Frame.Diagonal)) / (rows.Count + 1));
}
