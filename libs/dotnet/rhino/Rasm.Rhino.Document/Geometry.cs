using System.Collections.Specialized;
using Rhino.DocObjects;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class DuplicateMode {
    public static readonly DuplicateMode InPlace = new(nameof(InPlace), owned: false, static source => NotOwned.Unless(!source.IsDocumentControlled).Map(_ => source));

    public static readonly DuplicateMode Duplicate = new(nameof(Duplicate), owned: true, static source => Missing.Unless(source.Duplicate(), nameof(GeometryBase.Duplicate)));

    public static readonly DuplicateMode CopyOnWrite = new(
        nameof(CopyOnWrite),
        owned: true,
        static source => source.IsDocumentControlled
            ? Missing.Unless(source.Duplicate(), nameof(GeometryBase.Duplicate))
            : Missing.Unless(source.DuplicateShallow(), nameof(GeometryBase.DuplicateShallow)));

    public bool Owned { get; }

    private readonly Func<GeometryBase, Fin<GeometryBase>> copy;

    public IO<GeometryHandle<T>> Acquire<T>(T source) where T : GeometryBase =>
        IO.lift(() => copy(source).Map(value => new GeometryHandle<T>((T)value, this)));
}

public sealed record GeometryPair(GeometryBase Geometry, Option<ObjectAttributes> Attributes);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ClippingPlaneSource {
    public sealed record Frame(Plane Plane) : ClippingPlaneSource;

    public sealed record FromSurface(PlaneSurface Surface) : ClippingPlaneSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeometryMotion {
    public sealed record ByTransform(Transform Value) : GeometryMotion;

    public sealed record Translation(Vector3d Vector) : GeometryMotion;

    public sealed record UniformScale(double Factor) : GeometryMotion;

    public sealed record Rotation(double AngleRadians, Vector3d Axis, Point3d Center) : GeometryMotion;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ClipScope {
    public sealed record Everything() : ClipScope;

    public sealed record Only(Seq<Guid> Objects, Seq<int> Layers) : ClipScope;

    public sealed record Except(Seq<Guid> Objects, Seq<int> Layers) : ClipScope;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ClipOp {
    public sealed record Scope(ClipScope Participation) : ClipOp;

    public sealed record Depth(Option<double> Value) : ClipOp;

    public sealed record AddViewports(Seq<Guid> ViewportIds) : ClipOp;

    public sealed record RemoveViewports(Seq<Guid> ViewportIds) : ClipOp;

    public sealed record ReplaceViewports(Seq<Guid> ViewportIds) : ClipOp;

    public sealed record Style(Option<Guid> DimensionStyleId) : ClipOp;
}

public sealed record ClipState(ClipScope Participation, Option<double> Depth, Seq<Guid> ViewportIds, Option<Guid> DimensionStyleId);

public sealed record GeometryState(ObjectType Type, bool DocumentControlled, bool Shallow, Option<string> Invalidity, uint Crc);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UserStringEdit {
    public sealed record Set(string Key, string Value) : UserStringEdit;

    public sealed record Delete(string Key) : UserStringEdit;

    public sealed record DeleteAll() : UserStringEdit;
}

public sealed record UserStringAccessors(
    Func<string, string, bool> Set,
    Func<string, string?> Get,
    Func<string, bool> Delete,
    Action DeleteAll,
    Func<int> ItemCount,
    Func<NameValueCollection> GetAll);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BoundsFrame {
    public sealed record AxisAligned(bool Accurate) : BoundsFrame;

    public sealed record Transformed(Transform Xform) : BoundsFrame;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class GeometryHandle<T>(T Value, DuplicateMode Mode) : IDisposable where T : GeometryBase {
    private readonly Disposal disposal = new(Mode.Owned ? Value.Dispose : Thinktecture.Empty.Action);

    public T Value { get; } = Value;

    public DuplicateMode Mode { get; } = Mode;

    public void Dispose() => disposal.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GeometryOps {
    // --- [HANDLES]
    public static IO<GeometryHandle<ClippingPlaneSurface>> CreateClippingPlane(ClippingPlaneSource source) =>
        IO.lift(() => source.Switch(
            frame: static frame => Invalid.Unless(frame.Plane.IsValid, nameof(Plane.IsValid)).Map(_ => Created(new ClippingPlaneSurface(frame.Plane))),
            fromSurface: static fromSurface => Fin.Succ(Created(new ClippingPlaneSurface(fromSurface.Surface)))));

    private static GeometryHandle<ClippingPlaneSurface> Created(ClippingPlaneSurface surface) =>
        new(surface, DuplicateMode.Duplicate);

    // --- [SCOPES]
    public static IO<TValue> WithGeometry<T, TValue>(T source, DuplicateMode mode, Func<T, IO<TValue>> body) where T : GeometryBase =>
        Disposal.Using(mode.Acquire(source), handle => body(handle.Value));

    public static IO<(T Copy, TResult Result)> EditCopy<T, TResult>(T source, Func<T, Fin<TResult>> edit) where T : GeometryBase =>
        from handle in DuplicateMode.Duplicate.Acquire(source)
        from result in OnFailure(IO.lift(() => edit(handle.Value)), IO.lift(handle.Dispose))
        select (handle.Value, result);

    public static IO<T> OnFailure<T>(IO<T> effect, IO<Unit> release) =>
        effect.IfFail(error => release.MapFail(fault => error + fault).Bind(_ => IO.fail<T>(error)));

    // --- [MOTION]
    public static IO<Option<GeometryMotion>> Move<T>(GeometryHandle<T> handle, GeometryMotion motion) where T : GeometryBase =>
        from owned in IO.lift(() => NotOwned.Unless(handle.Mode != DuplicateMode.InPlace))
        from reverse in motion.Switch(
            handle.Value,
            byTransform: static (value, byTransform) =>
                from valid in IO.lift(() => Invalid.Unless(byTransform.Value.IsValid, nameof(Transform.IsValid)))
                from moved in IO.lift(() => Refused.Unless(value.Transform(byTransform.Value), nameof(GeometryBase.Transform)))
                select byTransform.Value.TryGetInverse(out Transform inverse) ? Some<GeometryMotion>(new GeometryMotion.ByTransform(inverse)) : Option<GeometryMotion>.None,
            translation: static (value, translation) =>
                IO.lift(() => Refused.Unless(value.Translate(translation.Vector), nameof(GeometryBase.Translate)))
                    .Map(_ => Some<GeometryMotion>(new GeometryMotion.Translation(-translation.Vector))),
            uniformScale: static (value, scale) =>
                from valid in IO.lift(() => Invalid.Unless(double.IsFinite(scale.Factor) && (scale.Factor != 0.0), nameof(GeometryMotion.UniformScale.Factor)))
                from scaled in IO.lift(() => Refused.Unless(value.Scale(scale.Factor), nameof(GeometryBase.Scale)))
                select Some<GeometryMotion>(new GeometryMotion.UniformScale(1.0 / scale.Factor)),
            rotation: static (value, rotation) =>
                IO.lift(() => Refused.Unless(value.Rotate(rotation.AngleRadians, rotation.Axis, rotation.Center), nameof(GeometryBase.Rotate)))
                    .Map(_ => Some<GeometryMotion>(new GeometryMotion.Rotation(-rotation.AngleRadians, rotation.Axis, rotation.Center))))
        select reverse;

    // --- [CLIPPING]
    public static IO<ClipState> ReadClip(ClippingPlaneSurface surface) =>
        IO.lift(() => new ClipState(
            Participation(surface),
            surface.PlaneDepthEnabled ? Some(surface.PlaneDepth) : Option<double>.None,
            toSeq(surface.ViewportIds()),
            Answers.Present(surface.DimensionStyleId)));

    public static IO<ClipState> Clip(GeometryHandle<ClippingPlaneSurface> handle, ClipOp op) =>
        from owned in IO.lift(() => NotOwned.Unless(handle.Mode != DuplicateMode.InPlace))
        let surface = handle.Value
        from written in op.Switch(
            surface,
            scope: static (plane, scope) => IO.lift(() => scope.Participation.Switch(
                plane,
                everything: static (target, _) => {
                    target.ClearClipParticipationLists();
                    target.ParticipationListsEnabled = false;
                },
                only: static (target, only) => Listed(target, only.Objects, only.Layers, exclusion: false),
                except: static (target, except) => Listed(target, except.Objects, except.Layers, exclusion: true))),
            depth: static (plane, depth) => IO.lift(() => {
                _ = depth.Value.Iter(value => plane.PlaneDepth = value);
                plane.PlaneDepthEnabled = depth.Value.IsSome;
            }),
            addViewports: static (plane, add) => Added(plane, add.ViewportIds),
            removeViewports: static (plane, remove) => Removed(plane, remove.ViewportIds),
            replaceViewports: static (plane, replace) =>
                from present in IO.lift(() => toSeq(plane.ViewportIds()))
                from removed in Removed(plane, Without(present, replace.ViewportIds))
                from added in Added(plane, replace.ViewportIds)
                select added,
            style: static (plane, style) => IO.lift(() => plane.DimensionStyleId = style.DimensionStyleId.IfNone(Guid.Empty)).Map(static _ => unit))
        from state in ReadClip(surface)
        from verified in IO.lift(() => op.Switch(
            state,
            scope: static (read, scope) =>
                Mismatch.Unless(SameScope(read.Participation, scope.Participation), nameof(ClippingPlaneSurface.GetClipParticipation)).Map(_ => read),
            depth: static (read, depth) =>
                Mismatch.Unless(read.Depth == depth.Value, nameof(ClippingPlaneSurface.PlaneDepth)).Map(_ => read),
            addViewports: static (read, add) =>
                Mismatch.Unless(Without(add.ViewportIds, read.ViewportIds).IsEmpty, nameof(ClippingPlaneSurface.ViewportIds)).Map(_ => read),
            removeViewports: static (read, remove) =>
                Mismatch.Unless(Within(remove.ViewportIds, read.ViewportIds).IsEmpty, nameof(ClippingPlaneSurface.ViewportIds)).Map(_ => read),
            replaceViewports: static (read, replace) =>
                Mismatch.Unless(SameSet(read.ViewportIds, replace.ViewportIds), nameof(ClippingPlaneSurface.ViewportIds)).Map(_ => read),
            style: static (read, style) =>
                Mismatch.Unless(read.DimensionStyleId == style.DimensionStyleId, nameof(ClippingPlaneSurface.DimensionStyleId)).Map(_ => read)))
        select verified;

    private static ClipScope Participation(ClippingPlaneSurface surface) {
        if (!surface.ParticipationListsEnabled)
            return new ClipScope.Everything();
        surface.GetClipParticipation(out IEnumerable<Guid> objects, out IEnumerable<int> layers, out bool exclusion);
        return exclusion ? new ClipScope.Except(toSeq(objects), toSeq(layers)) : new ClipScope.Only(toSeq(objects), toSeq(layers));
    }

    private static void Listed(ClippingPlaneSurface surface, Seq<Guid> objects, Seq<int> layers, bool exclusion) {
        surface.SetClipParticipation(objects, layers, exclusion);
        surface.ParticipationListsEnabled = true;
    }

    private static IO<Unit> Added(ClippingPlaneSurface surface, Seq<Guid> ids) =>
        from present in IO.lift(() => toSeq(surface.ViewportIds()))
        from added in Without(ids, present)
            .TraverseM(id => IO.lift(() => Refused.Unless(surface.AddClipViewportId(id), nameof(ClippingPlaneSurface.AddClipViewportId))))
            .As()
        select unit;

    private static IO<Unit> Removed(ClippingPlaneSurface surface, Seq<Guid> ids) =>
        from present in IO.lift(() => toSeq(surface.ViewportIds()))
        from removed in Within(ids, present)
            .TraverseM(id => IO.lift(() => Refused.Unless(surface.RemoveClipViewportId(id), nameof(ClippingPlaneSurface.RemoveClipViewportId))))
            .As()
        select unit;

    private static Seq<Guid> Without(Seq<Guid> ids, Seq<Guid> excluded) =>
        ids.Filter(id => !excluded.Exists(other => other == id));

    private static Seq<Guid> Within(Seq<Guid> ids, Seq<Guid> included) =>
        ids.Filter(id => included.Exists(other => other == id));

    private static bool SameScope(ClipScope written, ClipScope requested) =>
        requested.Switch(
            written,
            everything: static (read, _) => read is ClipScope.Everything,
            only: static (read, only) => read is ClipScope.Only listed && SameSet(listed.Objects, only.Objects) && SameSet(listed.Layers, only.Layers),
            except: static (read, except) => read is ClipScope.Except listed && SameSet(listed.Objects, except.Objects) && SameSet(listed.Layers, except.Layers));

    private static bool SameSet<A>(Seq<A> left, Seq<A> right) =>
        toHashSet(left).Equals(toHashSet(right));

    // --- [FACTS]
    public static IO<GeometryState> Inspect(GeometryBase geometry) =>
        IO.lift(() => new GeometryState(
            geometry.ObjectType,
            geometry.IsDocumentControlled,
            geometry.IsShallowDuplicate,
            Answers.Found(!geometry.IsValidWithLog(out string log), log),
            geometry.DataCRC(0u)));

    // --- [USER_STRINGS]
    public static IO<HashMap<string, string>> ReadUserStrings(UserStringAccessors accessors) =>
        from strings in IO.lift(accessors.GetAll)
        select toHashMap(toSeq(strings.AllKeys).Choose(key =>
            from present in Optional(key)
            from value in Optional(strings[present])
            select (present, value)));

    public static IO<Unit> WriteUserStrings(UserStringAccessors accessors, HashMap<string, string> strings) =>
        toSeq<(string Key, string Value)>(strings)
            .TraverseM(pair => EditUserStrings(accessors, new UserStringEdit.Set(pair.Key, pair.Value)))
            .As()
            .Map(static _ => unit);

    public static IO<Unit> EditUserStrings(UserStringAccessors accessors, UserStringEdit edit) =>
        edit.Switch(
            accessors,
            set: static (target, set) =>
                from keyed in IO.lift(() => Invalid.Unless(set.Key.Length > 0, nameof(UserStringEdit.Set.Key)))
                from written in IO.lift(() => Refused.Unless(target.Set(set.Key, set.Value), nameof(GeometryBase.SetUserString)))
                from same in IO.lift(() => Mismatch.Unless(string.Equals(target.Get(set.Key), set.Value, StringComparison.Ordinal), nameof(GeometryBase.GetUserString)))
                select same,
            delete: static (target, delete) =>
                from present in IO.lift(() => Optional(target.Get(delete.Key)))
                from removed in present.Match(
                    Some: _ =>
                        from deleted in IO.lift(() => Refused.Unless(target.Delete(delete.Key), nameof(GeometryBase.DeleteUserString)))
                        from absent in IO.lift(() => Mismatch.Unless(target.Get(delete.Key) is null, nameof(GeometryBase.GetUserString)))
                        select absent,
                    None: static () => IO.pure(unit))
                select removed,
            deleteAll: static (target, _) =>
                from cleared in IO.lift(target.DeleteAll)
                from empty in IO.lift(() => Mismatch.Unless(target.ItemCount() == 0, nameof(GeometryBase.UserStringCount)))
                select empty);

    public static UserStringAccessors UserStrings(GeometryBase geometry) =>
        new(geometry.SetUserString, geometry.GetUserString, geometry.DeleteUserString, geometry.DeleteAllUserStrings, () => geometry.UserStringCount, geometry.GetUserStrings);

    public static UserStringAccessors UserStrings(ObjectAttributes attributes) =>
        new(attributes.SetUserString, attributes.GetUserString, attributes.DeleteUserString, attributes.DeleteAllUserStrings, () => attributes.UserStringCount, attributes.GetUserStrings);

    public static UserStringAccessors UserStrings(DimensionStyle style) =>
        new(style.SetUserString, style.GetUserString, style.DeleteUserString, style.DeleteAllUserStrings, () => style.UserStringCount, style.GetUserStrings);

    public static UserStringAccessors UserStrings(HatchPattern pattern) =>
        new(pattern.SetUserString, pattern.GetUserString, pattern.DeleteUserString, pattern.DeleteAllUserStrings, () => pattern.UserStringCount, pattern.GetUserStrings);

    public static UserStringAccessors UserStrings(Linetype linetype) =>
        new(linetype.SetUserString, linetype.GetUserString, linetype.DeleteUserString, linetype.DeleteAllUserStrings, () => linetype.UserStringCount, linetype.GetUserStrings);

    // --- [BOUNDS]
    public static IO<BoundingBox> Bounds(GeometryBase geometry, BoundsFrame frame) =>
        IO.lift(() => Valid(frame.Switch(
            geometry,
            axisAligned: static (target, axis) => target.GetBoundingBox(axis.Accurate),
            transformed: static (target, transformed) => target.GetBoundingBox(transformed.Xform))));

    public static IO<(BoundingBox Local, Box World)> OrientedBounds(GeometryBase geometry, Plane plane) =>
        from framed in IO.lift(() => Invalid.Unless(plane.IsValid, nameof(Plane.IsValid)))
        from bounds in IO.lift(() => Valid(geometry.GetBoundingBox(plane, out Box world)).Map(local => (Local: local, World: world)))
        select bounds;

    private static Fin<BoundingBox> Valid(BoundingBox box) =>
        Invalid.Unless(box.IsValid, box, nameof(BoundingBox.IsValid));
}
