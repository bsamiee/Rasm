# [RASM_GRASSHOPPER_DATA]

`GardenData` is the GH2 data boundary: one transfer policy carries item, pear, twig, and tree topology across `IDataAccess`, the `Garden` algebra builds and folds trees, and one conversion fold resolves any host object onto a typed carrier through scoped broker rows, the merit-scored `ConversionServer`, and the geometry cast-or-convert families. The page mints the closed `GhFault` family every host crossing in the sub-domain reports through, the `Hosted` exception funnel every host call enters, and the diagnostic vocabulary that lands faults back on the document. Broker participation is document- and plugin-scoped rows with deterministic precedence and revocation — a process-global broker is unregistrable by construction.

## [01]-[INDEX]

- [02]-[FAULT_AND_NOTICE]: the `GhFault` family, the `Hosted` funnel, and the `Severity`/`Notice` diagnostic vocabulary
- [03]-[TRANSFER]: the `Transfer<T>` topology union, `Retention`, and the typed read/write/promotion folds over `IDataAccess` and `Garden`
- [04]-[CONVERSION]: scoped broker rows, the `Coerce` fold, the merit receipt, and the geometry shape families
- [05]-[HOST_CONTEXT]: tolerance, unit-system, and unit-scaling capture
- [06]-[RESEARCH]

## [02]-[FAULT_AND_NOTICE]

- Owner: `GhFault` is the one closed host-boundary fault family for the Components sub-domain — a `[Union]` on the kernel `Expected` bridge in the 4600-4699 code band, every case carrying the raising `Op` key beside its failure detail with `Message`, `Category`, and `Code` rendered once on the base; `Semigroup` `Combine` folds any two faults into the `Aggregate` case for GhFault-typed accumulation; `Hosted` is the one inbound exception funnel, so no host `try`/`catch` exists outside it; `Severity` and `Notice` land any fault or message on the document through the verified emission members.
- Cases: `Text`, `Absent`, `Refused`, `Conversion`, `Host`, `Registration`, and the `Aggregate` fold case.
- Entry: `Hosted.Bound` absorbs value-returning and void host calls by argument shape; every public entry in the sub-domain accepts `Op? key = null` and resolves through the kernel `OrDefault()`, threading the resolved key into every fault it mints; `Notice.Of(Error)` projects any rail failure into a document-visible error notice.
- Packages: `Rasm.Domain` carries `Expected`, `Op`, `OrDefault()`, and `Fault.Cancelled`; `LanguageExt.Core` carries `Fin`, `Validation`, and `Try`; `Thinktecture.Runtime.Extensions` generates the union and the severity rows.
- Growth: a new crossing cause is one fault case; a new document message channel is one `Severity` row.
- Boundary: cancellation never enters this family — a captured `OperationCanceledException` surfaces as the kernel `Fault.Cancelled`, keeping its category across the host crossing; the `Try` capture inside `Hosted` is the named platform-forced seam.

```csharp signature
namespace Rasm.Grasshopper.Components;

// --- [ERRORS] ----------------------------------------------------------------------------

[Union]
public abstract partial record GhFault : Expected, Semigroup<GhFault> {
    private GhFault(Op key, string detail, int code) => (Key, Detail, Code) = (key, detail, code);

    public Op Key { get; }

    public string Detail { get; }

    public sealed override int Code { get; }

    public sealed override string Category => "Grasshopper";

    public override string Message => $"Grasshopper operation '{Key}': {Detail}";

    public sealed record Text : GhFault { public Text(Op key, string detail) : base(key, detail, 4600) { } }
    public sealed record Absent : GhFault { public Absent(Op key, string detail) : base(key, detail, 4601) { } }
    public sealed record Refused : GhFault { public Refused(Op key, string detail) : base(key, detail, 4602) { } }
    public sealed record Conversion : GhFault {
        public Conversion(Op key, string source, string target, string detail) : base(key, $"{source}->{target}: {detail}", 4603) =>
            (Source, Target) = (source, target);
        public string Source { get; }
        public string Target { get; }
    }
    public sealed record Host : GhFault { public Host(Op key, string detail) : base(key, detail, 4604) { } }
    public sealed record Registration : GhFault { public Registration(Op key, string detail) : base(key, detail, 4605) { } }
    public sealed record Aggregate : GhFault {
        public Aggregate(Op key, Seq<GhFault> faults) : base(key, $"{faults.Count} faults", 4699) => Faults = faults;
        public Seq<GhFault> Faults { get; }
    }

    public GhFault Combine(GhFault rhs) => (this, rhs) switch {
        (Aggregate left, Aggregate right) => new Aggregate(left.Key, left.Faults + right.Faults),
        (Aggregate left, _) => new Aggregate(left.Key, left.Faults.Add(rhs)),
        (_, Aggregate right) => new Aggregate(Key, this.Cons(right.Faults)),
        _ => new Aggregate(Key, Seq(this, rhs)),
    };
}

// --- [SERVICES] --------------------------------------------------------------------------

public static class Hosted {
    public static Fin<T> Bound<T>(Func<T> call, Op? key = null) =>
        Try.lift(() => Fin.Succ(call()))
            .Run()
            .MapFail(error => error.HasException<OperationCanceledException>()
                ? (Error)new Fault.Cancelled()
                : new GhFault.Host(key.OrDefault(), error.Message))
            .Bind(static result => result);

    public static Fin<Unit> Bound(Action call, Op? key = null) =>
        Bound(() => { call(); return unit; }, key);
}

[SmartEnum]
public sealed partial class Severity {
    public static readonly Severity Remark = new(static (a, t, d, x) => a.AddRemark(t, d, x));
    public static readonly Severity Warning = new(static (a, t, d, x) => a.AddWarning(t, d, x));
    public static readonly Severity Error = new(static (a, t, d, x) => a.AddError(t, d, x));

    [UseDelegateFromConstructor]
    public partial void Emit(IDataAccess access, string title, string detail, Grasshopper2.Doc.MessageAction[] actions);
}

public sealed record Notice(Severity Severity, string Title, string Detail, Seq<Grasshopper2.Doc.MessageAction> Actions) {
    public static Notice Of(Error fault) => new(Severity.Error, fault.GetType().Name, fault.Message, []);

    public Unit Report(IDataAccess access) => fun(() => Severity.Emit(access, Title, Detail, [.. Actions]))();
}
```

## [03]-[TRANSFER]

- Owner: `Transfer<T>` is the one topology union a pin payload rides — item, pear, twig, and tree are cases of one carrier, so read, write, promotion, and retention discriminate on the payload's own shape; `Retention` closes the metadata disposition at the write seam.
- Cases: `Item` carries a bare value with its `MetaData`; `OfPear`, `OfTwig`, and `OfTree` carry the host carriers verbatim.
- Entry: `GardenData.Read<T>` discriminates depth by the `PinAccess` row; `GardenData.Write<T>` discriminates by the payload case; `AsTree` promotes any case onto the tree topology through the `Garden` builders; `Zip`, `Amend`, and `ConvertTwig` capture the `Garden` and `Twig` fold algebra onto the rail.
- Receipt: every fold lands on `Fin` with a `GhFault` cause — an absent pin is `Absent`, a host raise is `Host` through the funnel.
- Packages: `Grasshopper2` `Garden`, `Tree<T>`/`Twig<T>`/`Pear<T>`, and `MetaData` are the composed algebra; no local tree walker exists beside them.
- Growth: a new topology the host admits is one `Transfer<T>` case plus one arm per fold.
- Boundary: presence law rides the pin's declared `Requirement` at the host — a failed typed read is `GhFault.Absent`, and a component mixing presence semantics reads through its own access fold, never a widened carrier; `Retag` applies only at the `Item` write seam, so a carrier-case retag refuses loudly until the `Pear` mint verifies and unlocks meta reprojection.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Transfer<T> {
    private Transfer() { }

    public sealed record Item(T Value, MetaData Meta) : Transfer<T>;
    public sealed record OfPear(Pear<T> Pear) : Transfer<T>;
    public sealed record OfTwig(Twig<T> Twig) : Transfer<T>;
    public sealed record OfTree(Tree<T> Tree) : Transfer<T>;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Retention {
    private Retention() { }

    public sealed record Preserve : Retention;
    public sealed record Retag(Func<MetaData, MetaData> Project) : Retention;

    public MetaData Applied(MetaData incoming) => Switch(
        state: incoming,
        preserve: static (meta, _) => meta,
        retag: static (meta, row) => row.Project(meta));
}

// --- [OPERATIONS] ------------------------------------------------------------------------

public static class GardenData {
    public static Fin<Transfer<T>> Read<T>(IDataAccess access, int pin, PinAccess depth, Op? key = null) =>
        depth.Switch(
            state: (Access: access, Pin: pin, Key: key.OrDefault()),
            item: static held => held.Access.GetPear<T>(held.Pin, out Pear<T> pear)
                ? Fin.Succ<Transfer<T>>(new Transfer<T>.OfPear(pear))
                : Missing<Transfer<T>>(held.Pin, held.Key),
            twig: static held => held.Access.GetTwig<T>(held.Pin, out Twig<T> twig)
                ? Fin.Succ<Transfer<T>>(new Transfer<T>.OfTwig(twig))
                : Missing<Transfer<T>>(held.Pin, held.Key),
            tree: static held => held.Access.GetTree<T>(held.Pin, out Tree<T> tree)
                ? Fin.Succ<Transfer<T>>(new Transfer<T>.OfTree(tree))
                : Missing<Transfer<T>>(held.Pin, held.Key));

    public static Fin<Unit> Write<T>(IDataAccess access, int pin, Transfer<T> payload, Retention retention, Op? key = null) =>
        payload.Switch(
            state: (Access: access, Pin: pin, Retention: retention, Key: key.OrDefault()),
            item: static (held, item) => Hosted.Bound(() => held.Access.SetItem(held.Pin, item.Value!, held.Retention.Applied(item.Meta)), held.Key),
            ofPear: static (held, row) => Carried(held, () => held.Access.SetPear(held.Pin, row.Pear)),
            ofTwig: static (held, row) => Carried(held, () => held.Access.SetTwig(held.Pin, row.Twig)),
            ofTree: static (held, row) => Carried(held, () => held.Access.SetTree(held.Pin, row.Tree)));

    public static Fin<Tree<T>> AsTree<T>(Transfer<T> payload, Op? key = null) =>
        payload.Switch(
            state: key.OrDefault(),
            item: static (op, row) => Hosted.Bound(() => Garden.TreeFromPears([Pear<T>.Create(row.Value, row.Meta)]), op),
            ofPear: static (op, row) => Hosted.Bound(() => Garden.TreeFromPears([row.Pear]), op),
            ofTwig: static (op, row) => Hosted.Bound(() => Garden.TreeFromTwigs([row.Twig]), op),
            ofTree: static (_, row) => Fin.Succ(row.Tree));

    public static Fin<Tree<TOut>> Zip<TLeft, TRight, TOut>(
        Tree<TLeft> left, Tree<TRight> right, Func<TLeft, TRight, TOut> merge, CancellationToken cancel, Op? key = null) =>
        Hosted.Bound(() => Garden.PairWiseOp(left, right, merge, cancel), key.OrDefault());

    public static Fin<Tree<T>> Amend<T>(Tree<T> tree, Func<Pear<T>, Pear<T>> project, CancellationToken cancel, Op? key = null) =>
        Hosted.Bound(() => Garden.PearWiseOp(tree, project, cancel), key.OrDefault());

    public static Fin<Twig<TOut>> ConvertTwig<TIn, TOut>(
        Twig<TIn> twig, Grasshopper2.Types.Conversion.ConversionDelegate<TIn, TOut> convert,
        CancellationToken cancel, Grasshopper2.Data.ConversionRecord record, Op? key = null) =>
        Hosted.Bound(() => twig.Convert(convert, cancel, record), key.OrDefault());

    private static Fin<Unit> Carried((IDataAccess Access, int Pin, Retention Retention, Op Key) held, Action write) =>
        held.Retention is Retention.Retag
            ? Fin.Fail<Unit>(new GhFault.Refused(held.Key, $"{nameof(Retention.Retag)}:pin:{held.Pin}"))
            : Hosted.Bound(write, held.Key);

    private static Fin<T> Missing<T>(int pin, Op key) => Fin.Fail<T>(new GhFault.Absent(key, $"pin:{pin}"));
}
```

## [04]-[CONVERSION]

- Owner: `Coerce` is the one conversion fold — a direct cast admits first, scoped broker rows fold in deterministic precedence, and the merit-scored `ConversionServer` is the terminal host route; kernel geometry coercion stays `Rasm.Domain` `Normalization.CoerceTo<TTarget>`-owned, composed by callers holding a kernel `Context`, never re-derived here; `BrokerLedger` owns broker participation as revocable document- and plugin-scoped rows; `CurveShape` and `SurfaceShape` type the cast-or-convert families the host resolves.
- Cases: `BrokerScope` closes at `Document` and `Plugin`; precedence orders document before plugin, then row rank, then enrolment ordinal.
- Entry: `Coerce.To<TOut>` is the one typed conversion entry over any raw host object with a null admitted as `GhFault.Absent` before any probe runs; `CurveOf` and `SurfaceOf` are the geometry-family probes under the same null gate.
- Receipt: every success carries a `ConversionReceipt` naming source, target, route, and the host `Merit` where the server resolved it.
- Auto: enrolment returns an `Enrolment` revocation value — the row dies with its owner's scope, so a leaked broker is a live `IDisposable`, never an orphan delegate.
- Growth: a new conversion route is one broker row; a new geometry family member is one union case plus one probe arm.
- Boundary: the `BrokerLedger` swap bodies and the cast-or-convert out-probes are the named boundary-kernel statement seam; interior code receives typed carriers and receipts only.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BrokerScope {
    private BrokerScope() { }

    public sealed record Document(Guid DocumentId) : BrokerScope;
    public sealed record Plugin(Guid PluginId) : BrokerScope;

    public int Precedence => Switch(document: static _ => 0, plugin: static _ => 1);
}

public sealed record BrokerRow(BrokerScope Scope, int Rank, Type Source, Type Target, Func<object, Fin<object>> Convert);

public sealed record ConversionScope(Option<Guid> Document, Option<Guid> Plugin) {
    public static readonly ConversionScope Unscoped = new(None, None);

    public bool Admits(BrokerScope scope) => scope.Switch(
        state: this,
        document: static (held, row) => held.Document.Map(id => id == row.DocumentId).IfNone(false),
        plugin: static (held, row) => held.Plugin.Map(id => id == row.PluginId).IfNone(false));
}

public sealed record ConversionReceipt(string Source, string Target, string Route, Option<Grasshopper2.Types.Conversion.Merit> Merit);

public readonly record struct Enrolment(Guid Row, Action Revoke) : IDisposable {
    public void Dispose() => Revoke();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveShape {
    private CurveShape() { }

    public sealed record OfLine(Rhino.Geometry.Line Value) : CurveShape;
    public sealed record OfTriangle(Grasshopper2.Types.Shapes.Triangle Value) : CurveShape;
    public sealed record OfRectangle(Rhino.Geometry.Rectangle3d Value) : CurveShape;
    public sealed record OfPolyline(Rhino.Geometry.Polyline Value) : CurveShape;
    public sealed record OfCircle(Rhino.Geometry.Circle Value) : CurveShape;
    public sealed record OfArc(Rhino.Geometry.Arc Value) : CurveShape;
    public sealed record OfCurve(Rhino.Geometry.Curve Value) : CurveShape;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceShape {
    private SurfaceShape() { }

    public sealed record OfSurface(Rhino.Geometry.Surface Value) : SurfaceShape;
    public sealed record OfBrep(Rhino.Geometry.Brep Value) : SurfaceShape;
    public sealed record OfSubD(Rhino.Geometry.SubD Value) : SurfaceShape;
}

// --- [COMPOSITION] -----------------------------------------------------------------------

public static class BrokerLedger {
    private sealed record Ledger(HashMap<Guid, (BrokerRow Row, long Ordinal)> Rows, long Next);

    private static readonly Atom<Ledger> Cell = Atom(new Ledger(HashMap<Guid, (BrokerRow, long)>(), 0));

    public static Enrolment Enroll(BrokerRow row) {
        Guid id = Guid.NewGuid();
        Cell.Swap(ledger => new Ledger(ledger.Rows.Add(id, (row, ledger.Next)), ledger.Next + 1));
        return new Enrolment(id, () => ignore(Cell.Swap(ledger => ledger with { Rows = ledger.Rows.Remove(id) })));
    }

    public static Seq<BrokerRow> Resolved(Type source, Type target, ConversionScope scope) =>
        toSeq(Cell.Value.Rows.Values
            .Filter(entry => entry.Row.Source.IsAssignableFrom(source) && target.IsAssignableFrom(entry.Row.Target) && scope.Admits(entry.Row.Scope))
            .OrderBy(static entry => (entry.Row.Scope.Precedence, entry.Row.Rank, entry.Ordinal)))
            .Map(static entry => entry.Row);
}

public static class Coerce {
    public static Fin<(TOut Value, ConversionReceipt Receipt)> To<TOut>(object? raw, ConversionScope scope, Op? key = null) => raw switch {
        null => Fin.Fail<(TOut, ConversionReceipt)>(new GhFault.Absent(key.OrDefault(), typeof(TOut).Name)),
        TOut direct => Fin.Succ((direct, Receipt<TOut>(raw, nameof(Type), None))),
        _ => BrokerLedger.Resolved(raw.GetType(), typeof(TOut), scope)
            .Fold(Fin.Fail<(TOut, ConversionReceipt)>(new GhFault.Conversion(key.OrDefault(), raw.GetType().Name, typeof(TOut).Name, nameof(BrokerLedger))),
                (state, row) => state.IsSucc ? state : row.Convert(raw).Map(value => ((TOut)value, Receipt<TOut>(raw, nameof(BrokerRow), None))))
            .BindFail(_ => Served<TOut>(raw, key.OrDefault())),
    };

    public static Fin<CurveShape> CurveOf(object? raw, Op? key = null) =>
        Optional(raw).ToFin(new GhFault.Absent(key.OrDefault(), nameof(CurveShape)))
            .Bind(held => Hosted.Bound(() => CurveProbe(held, key.OrDefault()), key.OrDefault()).Bind(identity));

    public static Fin<SurfaceShape> SurfaceOf(object? raw, Op? key = null) =>
        Optional(raw).ToFin(new GhFault.Absent(key.OrDefault(), nameof(SurfaceShape)))
            .Bind(held => Hosted.Bound(() => SurfaceProbe(held, key.OrDefault()), key.OrDefault()).Bind(identity));

    private static Fin<(TOut Value, ConversionReceipt Receipt)> Served<TOut>(object raw, Op key) =>
        Hosted.Bound(() =>
            Grasshopper2.Types.Conversion.ConversionServer.Convert(raw, typeof(TOut), out object converted, out Grasshopper2.Types.Conversion.Merit merit, out string detail)
                ? Fin.Succ(((TOut)converted, Receipt<TOut>(raw, nameof(Grasshopper2.Types.Conversion.ConversionServer), Optional(merit))))
                : Fin.Fail<(TOut, ConversionReceipt)>(new GhFault.Conversion(key, raw.GetType().Name, typeof(TOut).Name, detail)), key)
            .Bind(identity);

    private static Fin<CurveShape> CurveProbe(object raw, Op key) =>
        CurveBroker.CastOrConvert(
            raw, out Rhino.Geometry.Line line, out Grasshopper2.Types.Shapes.Triangle triangle, out Rhino.Geometry.Rectangle3d rectangle,
            out Rhino.Geometry.Polyline polyline, out Rhino.Geometry.Circle circle, out Rhino.Geometry.Arc arc, out Rhino.Geometry.Curve curve)
            ? (line, rectangle, circle, arc, polyline, curve) switch {
                ({ IsValid: true }, _, _, _, _, _) => new CurveShape.OfLine(line),
                (_, { IsValid: true }, _, _, _, _) => new CurveShape.OfRectangle(rectangle),
                (_, _, { IsValid: true }, _, _, _) => new CurveShape.OfCircle(circle),
                (_, _, _, { IsValid: true }, _, _) => new CurveShape.OfArc(arc),
                (_, _, _, _, { } value, _) => new CurveShape.OfPolyline(value),
                (_, _, _, _, _, { } value) => new CurveShape.OfCurve(value),
                _ => new CurveShape.OfTriangle(triangle),
            }
            : Fin.Fail<CurveShape>(new GhFault.Conversion(key, raw.GetType().Name, nameof(CurveShape), nameof(CurveBroker)));

    private static Fin<SurfaceShape> SurfaceProbe(object raw, Op key) =>
        SurfaceBroker.CastOrConvert(raw, out Rhino.Geometry.Surface surface, out Rhino.Geometry.Brep brep, out Rhino.Geometry.SubD subd)
            ? (surface, brep, subd) switch {
                ({ } value, _, _) => new SurfaceShape.OfSurface(value),
                (_, { } value, _) => new SurfaceShape.OfBrep(value),
                (_, _, { } value) => new SurfaceShape.OfSubD(value),
                _ => Fin.Fail<SurfaceShape>(new GhFault.Conversion(key, raw.GetType().Name, nameof(SurfaceShape), nameof(SurfaceBroker))),
            }
            : Fin.Fail<SurfaceShape>(new GhFault.Conversion(key, raw.GetType().Name, nameof(SurfaceShape), nameof(SurfaceBroker)));

    private static ConversionReceipt Receipt<TOut>(object raw, string route, Option<Grasshopper2.Types.Conversion.Merit> merit) =>
        new(raw.GetType().Name, typeof(TOut).Name, route, merit);
}
```

## [05]-[HOST_CONTEXT]

- Owner: `HostUnits` captures the solution's tolerance and unit context once per `Process` pass — absolute and relative tolerance, angle tolerance, the host unit system, and on-demand scaling factors ride one record every execution scope holds.
- Entry: `HostUnits.Of(IDataAccess)` is the one capture; `ScalingTo` projects the factor onto any `Rhino.UnitSystem`.
- Boundary: kernel tolerance policy stays `Rasm.Domain` `Context`-owned — this record is the host-side capture a caller feeds into kernel admission, never a second tolerance policy.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

public sealed record HostUnits(double Absolute, double Relative, Grasshopper2.Types.Numeric.Angle Angle, UnitSystem Units) {
    public static Fin<HostUnits> Of(IDataAccess access, Op? key = null) =>
        access.GetTolerance(out double absolute, out double relative)
        && access.GetTolerance(out Grasshopper2.Types.Numeric.Angle angle)
        && access.GetUnitSystem(out UnitSystem units)
            ? Fin.Succ(new HostUnits(absolute, relative, angle, units))
            : Fin.Fail<HostUnits>(new GhFault.Absent(key.OrDefault(), nameof(HostUnits)));

    public Fin<double> ScalingTo(IDataAccess access, Rhino.UnitSystem target, Op? key = null) =>
        access.GetUnitScaling(target, out double factor)
            ? Fin.Succ(factor)
            : Fin.Fail<double>(new GhFault.Absent(key.OrDefault(), nameof(ScalingTo)));
}
```

## [06]-[RESEARCH]

- [PEAR_CREATE]-[OPEN]: the exact `Pear<T>.Create` parameter list backing `AsTree`'s item promotion; verify arity and `MetaData` position through the decompile rail — the same mint unlocks carrier-case `Retag` reprojection in `Write`.
- [BROKER_RETURN]-[OPEN]: the return contracts of `ConversionServer.Convert` and the `CurveBroker`/`SurfaceBroker` `CastOrConvert` members, and the fill-discrimination protocol for the struct out-parameters — the probes assume a `bool` success flag, `IsValid` on the filled struct forms, and null on unfilled reference forms, with the `Triangle` arm as the residual; verify through the decompile rail and re-shape the probes to the host's own discriminant.
- [GARDEN_RETURNS]-[OPEN]: the return types of `Garden.PairWiseOp`/`PearWiseOp` and the tree builders — the folds assume the constructed `Tree<T>`; verify through the decompile rail.
- [BRANCH_EVALUATE]-[OPEN]: the verified `Twig<T>.Apply(Expression, Resolver, out IExpressionReport)` branch-expression seam and the `GetItemArray`/`GetIPears`/`GetNullArray`/`GetMetaArray` array reads — rule a `GardenData` evaluation fold and an array-read arm once the `Expression`/`Resolver`/`IExpressionReport` namespaces verify.
