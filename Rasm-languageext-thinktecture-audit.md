# Rasm LanguageExt / Thinktecture Audit

## `libs/dotnet/Rasm/.planning/Domain/validation.md:93`
`checks.Fold((Value: Success, Context, Geometry, Cancel), ... tuple Apply ...).Value` -> `toSeq(checks).Traverse(check => check.Apply(context, geometry, cancel).ToValidation()).As().Map(_ => original)`.
`Seq.Traverse` already accumulates the independent checks applicatively; the tuple state, success seed, and repeated binary `Apply` duplicate that operator.

## `libs/dotnet/Rasm/.planning/Domain/validation.md:101`
`(Success(state), lease.ToValidation()).Apply(...).Bind(validation => validation).As()` -> `lease.ToValidation().Bind(native => native.Use(geometry => RunChecks(checks, context, geometry, original, cancel)))`.
The success slot cannot fail; `Validation.Bind` is the existing dependent composition and removes the artificial carrier and flattening layer.

## `libs/dotnet/Rasm/.planning/Domain/validation.md:263`
Unused `OpAcceptance` / `TRaw.TryCreateValidated<TVO>()` at lines 307-316 -> delete it and its index/owner/lowering claims at lines 11, 263, and 267; retain `OpExtensions.AcceptValidated<TRaw,TVO>` at lines 413-429.
No target consumer calls the extension, while `AdmissionProjection.Generated` already uses the keyed numeric bridge at line 329; deletion removes a second, keyless admission surface without a ripple.

## `libs/dotnet/Rasm/.planning/Domain/context.md:238`
`Of(units: UnitSystem.Millimeters).As().ThrowIfFail()` -> `Of(units: UnitSystem.Millimeters).ToFin().ThrowIfFail()`.
The pinned API exposes `ThrowIfFail` on `Fin`, not `Validation`; `ValidationExtensions.ToFin(Validation<Error,A>)` is the existing egress before this host-boundary unwrap.

## `libs/dotnet/Rasm/.planning/Domain/validation.md:504`
`value.Filter(invalid).Match(Some: Fail, None: Success(value))` -> `value.TraverseM(scalar => In(scalar, band, label, key)).As()`.
`Option.TraverseM` is total over `None`, preserves `Some`, and lifts the existing `In` validation without reimplementing either branch.

## `libs/dotnet/Rasm/.planning/Domain/validation.md:538`
`many.Errors.Bind(static member => Unpack(member)).ToSeq().Strict()` -> `many.Errors.Bind(static member => Unpack(member)).Strict()`.
`ManyErrors.Errors` is already `Seq<Error>` and `Seq.Bind` preserves that carrier; the `ToSeq` hop is an identity.

## `libs/dotnet/Rasm/.planning/Domain/validation.md:738`
`Admit.Claim` plus `toSeq(clauses).Traverse(clause => Claim(...)).As().Map(_ => unit).ToFin()` -> `AdmissionSlots.Accumulate(toSeq(clauses).Map(clause => AdmissionSlots.Gate(clause.Held, key.InvalidInput(axis: clause.Axis)))).ToFin()`.
`AdmissionSlots.Gate` and `Accumulate` already own the concrete validation lift and independent-error fan-in; delete the single-caller `Claim` module symbol.

## `libs/dotnet/Rasm/.planning/Analysis/relations.md:384`
`settled.Match(Succ: Fin.Succ, Fail: cause => cause is Unsupported ? Scan(...) : Fin.Fail(cause))` -> `settled.BindFail(cause => cause is Unsupported ? Scan(...) : Fin.Fail(cause))`.
`Fin.BindFail` preserves every success and sequences only the failure branch, exactly the retry-on-unsupported policy expressed by the hand fold.

## `libs/dotnet/Rasm/.planning/Analysis/measure.md:362`
`Filter(...).Fold(Fin.Succ(Seq()), (rows, kind) => rows.Bind(...Map(rows.Add)))` -> `Filter(...).TraverseM(kind => ...Map(magnitude => (kind, magnitude))).As().Bind(rows => Of(rows, op))`.
`Seq.TraverseM` preserves ordered fail-fast construction and removes the monadic seed and hand-built collection accumulator.

## `libs/dotnet/Rasm/.planning/Analysis/measure.md:247`
`Catch: error => IO.pure(Fin.Fail<IDisposable>(error))` -> `Catch: error => IO.fail<IDisposable>(error)`.
`IO.lift(Func<Fin<A>>)` already lowers the `Fin` onto the `IO<A>` error channel, so `IO.pure(Fin.Fail<A>)` wrongly nests `Fin` as the success value instead of returning the `IO<A>` the `Bracket` failure arm requires.

## `libs/dotnet/Rasm/.planning/Domain/objective.md:361`
`roster.Find(...).Filter(...).Match(Some: Validation.Success(projected), None: Validation.Fail(error))` -> `.ToValidation((Error)new KernelFault.InvalidValue(...)).Map(row => Widget.IfNone(PanelKind.For(row.Kind)))`.
`Option.ToValidation` owns the presence-to-refusal bridge and `Map` preserves the success projection.

## `libs/dotnet/Rasm/.planning/Numerics/integrate.md:495`
`Range(...).Fold(Fin.Succ(Seq.Empty), ...Map(columns.Add))` -> `Range(...).ToSeq().TraverseM(moment => BasisColumn(...)).As()`; `columns.Rev().HeadOrNone()` -> `columns.Last`.
`TraverseM` preserves ordered fail-fast construction, and the pinned API exposes `Seq.Last` but no `HeadOrNone` on `Seq`.

## `libs/dotnet/Rasm/.planning/Interaction/transfer.md:215`
`Slots.Choose(...).HeadOrNone()` -> `Slots.Choose(...).Head`.
`Seq.Head` already returns `Option<Error>`; `HeadOrNone` does not exist on the pinned `Seq` surface.

## `libs/dotnet/Rasm/.planning/Domain/hooks.md:411`
`toSeq(seats.Value.Values).OrderBy(row => row.Ordinal).Map(row => row.Binding).ToSeq().Strict()` -> `toSeq(seats.Value.Values.OrderBy(row => row.Ordinal).Select(row => row.Binding)).Strict()`.
`OrderBy` exits to `IOrderedEnumerable`, which has no LanguageExt `Map` or `ToSeq`; one `toSeq` re-entry after LINQ preserves order and type.

## `libs/dotnet/Rasm/.planning/Domain/hooks.md:153`
`Seq.Filter(...).ToSeq().Strict()` -> `Seq.Filter(...).Strict()` at lines 153, 347, 352, 355; likewise remove `.ToSeq()` after the inner `Census.Filter(...).Map(...)` at line 413.
`Seq.Filter` and `Seq.Map` already return `Seq`; every conversion is an identity hop.

## `libs/dotnet/Rasm/.planning/Drawing/sheet.md:1067`
`name.Fields.Map(static pair => pair.Value).ToSeq()` -> `name.Fields.Map(static pair => pair.Value)`.
`Fields` is already `Seq` and its `Map` preserves that carrier, so the final conversion is redundant.

## `libs/dotnet/Rasm/.planning/Drawing/sheet.md:145`
`Suffixes.Zip(Range(0, Suffixes.Count)).Find(...)` -> `Suffixes.Map(static (suffix, index) => (suffix, index)).Find(...)`; apply the same indexed `Map` replacement to `Sequence.Zip(Range(...)).Fold(...)` at line 857.
`Seq.Zip` requires a `Seq` peer while `Range` is a distinct foldable; `Seq.Map(value,index)` already owns both ordinal joins and stays on the carrier.

## `libs/dotnet/Rasm/.planning/Drawing/sheet.md:238`
`toSeq(Range(series.Bounds.Floor, series.Bounds.Ceiling - series.Bounds.Floor + 1)).Map(...)` -> `Range(series.Bounds.Floor, series.Bounds.Ceiling - series.Bounds.Floor + 1).ToSeq().Map(...)`.
`Prelude.Range` returns the LanguageExt `Range<int>` foldable, not `IEnumerable<int>` accepted by `Prelude.toSeq`; `FoldableExtensions.ToSeq` is its existing carrier conversion.

## `libs/dotnet/Rasm/.planning/Spatial/fields.md:348`
`leftFields.Concat(rightFields).ToSeq()` -> `leftFields.Concat(rightFields)`.
`Seq.Concat(Seq<T>)` already returns `Seq<T>`; the conversion is an identity hop.

## `libs/dotnet/Rasm/.planning/Numerics/calculus.md:663`
`Range(0, samples.Value).Map(...).ToSeq()` -> `toSeq(Enumerable.Range(0, samples.Value)).Map(...)`.
LanguageExt `Range` has no `Map`; the pinned catalog requires enumerable admission for an integer projection, after which `Seq.Map` returns the final carrier.

## `libs/dotnet/Rasm/.planning/Processing/sample.md:462`
`nearest.Min()` -> `nearest.Min(double.PositiveInfinity)` after the existing `IsEmpty` return.
Unseeded `Min()` is ambiguous between LanguageExt `Foldable` and LINQ on `Seq<double>`; the seeded carrier overload is total and preserves the nonempty minimum.

## `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:559`
`CurvatureRangeKind.Items.Find(predicate)` -> `toSeq(CurvatureRangeKind.Items).Find(predicate)`.
Generated `Items` is `IReadOnlyList<T>` and the pinned LanguageExt `IEnumerable` extensions do not include `Find`; explicit carrier admission makes the existing foldable search legal.

## `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:67`
`cap.Match(Some: c => AcceptValidated(c).Map(Some), None: Fin.Succ(None))` -> `cap.TraverseM(c => key.OrDefault().AcceptValidated<Dimension>(c)).As()`.
`Option.TraverseM` preserves `None`, validates `Some`, and removes the hand-written carrier reconstruction.

## `libs/dotnet/Rasm/.planning/Spatial/transport.md:85`
`massRelaxation.Match(Some: value => AcceptValidated(value).Map(Some), None: Fin.Succ(None))` -> `massRelaxation.TraverseM(value => op.AcceptValidated<PositiveMagnitude>(value)).As()`.
`Option.TraverseM` is the existing total optional-effect inversion and preserves both outcomes.

## `libs/dotnet/Rasm/.planning/Parametric/projections.md:405`
`count.Match(Some: value => guard(...).ToFin().Map(_ => Some(value)), None: Fin.Succ(None))` -> `count.TraverseM(value => guard(value >= 1, op.InvalidInput()).ToFin().Map(_ => value)).As()`.
`Option.TraverseM` supplies the absent success and preserves the admitted present value without a manual two-arm fold.

## `libs/dotnet/Rasm/.planning/Processing/intent.md:93`
`frame.Match(Some: plane => Admit.Plane(...).Map(Some), None: Fin.Succ(None))` -> `frame.TraverseM(plane => Admit.Plane(plane, op)).As()`.
The existing optional monadic traversal performs the identical conditional validation in one carrier expression.

## `libs/dotnet/Rasm/.planning/Solving/fit.md:744`
`probes.Traverse(identity).As().Map(project).ToFin()` -> `AdmissionSlots.Accumulate(probes).Map(project).ToFin()` here and at `Solving/solver.md:917`.
Domain already owns this exact `Seq<Validation<Error,Unit>>` accumulation; retain each existing result projection while deleting both re-spellings of its LanguageExt fold.

## `libs/dotnet/Rasm/.planning/Meshing/slice.md:355`
`toSeq(Enumerable.Range(...)).Fold(Fin.Succ(unit), (state, k) => state.Bind(_ => Layer(k)))` -> `.TraverseM(Layer).As().Map(_ => unit)`.
`Seq.TraverseM` preserves ordered fail-fast layer execution and removes the manual monadic seed.

## `libs/dotnet/Rasm/.planning/Parametric/panelize.md:167`
`.Map(row => Loop(...).Map(...)).TraverseM(static ring => ring)` -> `.TraverseM(row => Loop(...).Map(...)).As()`; apply the same fusion to `Map(FrameOf).TraverseM(identity)` at lines 336-339.
`TraverseM` already combines projection and fail-fast inversion, so the intermediate `Seq<Fin<T>>` and identity traversal are unnecessary.

## `libs/dotnet/Rasm/.planning/Meshing/arrangement.md:296`
`toSeq(buckets).Map(bucket => Shell(...)).TraverseM(identity).As()` -> `toSeq(buckets).TraverseM(bucket => Shell(...)).As()`; apply the same fusion to `Map(at => Lower(...)).TraverseM(identity)` at lines 628-629.
`Seq.TraverseM` already accepts the effectful projection and preserves the same ordered fail-fast result without an intermediate `Seq<Fin<T>>`.

## `libs/dotnet/Rasm/.planning/Meshing/mesh.md:134`
`toSeq(Enumerable.Range(...)).Map(block => draft.Place(...)).TraverseM(identity).As()` -> `toSeq(Enumerable.Range(...)).TraverseM(block => draft.Place(...)).As()`.
The direct effectful traversal preserves every placement and its failure order while deleting the identity inversion pass.

## `libs/dotnet/Rasm/.planning/Interaction/binding.md:155`
`Lower().Match(Some: parent => Fin.Succ(new FromContext(...)), None: () => Fin.Fail(fault))` -> `Lower().ToFin(fault).Map(parent => (BindSource<TNext>)new FromContext(...))`.
`Option.ToFin` owns the presence refusal and `Map` owns the successful projection; the manual eliminator duplicates both.

## `libs/dotnet/Rasm/.planning/Interaction/transfer.md:72`
`found.Match(Some: slot => Fin.Succ(Some(slot)), None: () => Fin.Fail<Option<PayloadSlot>>(fault))` -> `found.ToFin(fault).Map(Some)`.
`Option.ToFin` and `Fin.Map` preserve the required-presence fault and the successful optional payload without two hand-built carrier arms.

## `libs/dotnet/Rasm/.planning/Spatial/naming.md:218`
`prior.Entries.Find(name).Match(Some: prev => Fin.Succ(prev with {...}), None: () => Fin.Fail(fault))` -> `.ToFin(fault).Map(prev => prev with {...})`.
`HashMap.Find` already returns `Option`; its `ToFin` and `Map` path preserves the exact collision fault and projection.

## `libs/dotnet/Rasm/.planning/Meshing/mesh.md:399`
`self.Genus.Match(Some: genus => Fin.Succ((...)), None: () => Fin.Fail(fault))` -> `self.Genus.ToFin(fault).Map(genus => (...))`.
`Option.ToFin` owns the missing-genus refusal and `Map` owns the tuple projection, with identical output and fault.

## `libs/dotnet/Rasm/.planning/Drawing/sheet.md:1645`
`LineGroup.For(...).Match(Succ: Validation.Success, Fail: Validation.Fail)` and `PdfTrait.Law.Admit(...).Match(...)` -> `LineGroup.For(...).ToValidation()` and `PdfTrait.Law.Admit(...).ToValidation()`.
`Fin.ToValidation` preserves each success value and failure error directly, so both manual carrier reconstructions are redundant.

## `libs/dotnet/Rasm/.planning/Domain/instrument.md:490`
`MeasureForm.Items.AsIterable().Map(form => { form.Heard(...); return unit; }).Strict()` -> `MeasureForm.Items.AsIterable().Iter(form => form.Heard(...))`.
`Iterable.Iter` is the existing side-effect traversal; the current fake projection is not consumed, and `Strict` is a `Seq` member rather than an `Iterable` member.

## `libs/dotnet/Rasm/.planning/Domain/instrument.md:539`
`held.Cells.Fold(seed, static (rows, pair) => rows.AddOrUpdate(pair.Key.Row, cells => pair.Value.Cons(cells), () => [pair.Value]))` -> `held.Cells.AsIterable().Fold(seed, static (rows, pair) => rows.AddOrUpdate(pair.Key.Row, cells => pair.Value.Cons(cells), () => [pair.Value]))`.
`HashMap<K,V>.Fold` receives values alone; its existing `AsIterable()` projection is the pair carrier exposing the named `Key` and `Value` fields.

## `libs/dotnet/Rasm/.planning/Domain/event.md:480`
`EventType.Validate(..., out admittedType) is null ? Fin.Succ(admittedType) : Fin.Fail(...)` -> `key.AcceptValidated<EventType>(admitted.Type!).MapFail(_ => new KernelFault.InvalidValue(Label: nameof(EventType), Requirement: "the generated EventType admission", Key: Some(key)))`; apply the same replacement with the existing `EventSource` fault at lines 485-489.
Domain `Op.AcceptValidated<TValueObject>(string?)` owns generated Thinktecture admission; both validators ignore the provider, and mapping refusal to each existing inline fault preserves the current result and fault exactly.

## `libs/dotnet/Rasm/.planning/Spatial/cloud.md:435`
`CloudHullRejection.TryGet((int)outcome, out CloudHullRejection? row) ? Fin.Succ(Some(row!)) : Fin.Fail(key.InvalidResult())` -> `key.Row<int, CloudHullRejection>((int)outcome).Map(Some).MapFail(_ => key.InvalidResult())` for the non-success branch.
Domain `Op.Row<TKey,TRow>` already lifts generated smart-enum `TryGet` into `Fin`; the final `MapFail` retains the current fault payload while deleting the nullable lookup shell.

## `libs/dotnet/Rasm/.planning/Spatial/cloud.md:455`
`value.Match(Some: magnitude => key.AcceptValidated<PositiveMagnitude>(magnitude.Value).Map(Some), None: () => Fin.Succ(None))` -> `value.TraverseM(magnitude => key.AcceptValidated<PositiveMagnitude>(magnitude.Value)).As()` inside `AdmitMagnitude`.
`Option.TraverseM` preserves default-ghost revalidation for `Some` and total success for `None` without duplicating the shared alpha/lambda helper at its callers.

## `libs/dotnet/Rasm/.planning/Solving/solver.md:459`
`Rank(...).Match(Succ: rank => Fin.Fail<LmPass>(new GeometryFault.SingularSystem(rank, dof)), Fail: Fin.Fail<LmPass>)` -> `Rank(...).Bind(rank => Fin.Fail<LmPass>(new GeometryFault.SingularSystem(rank, dof)))`.
`Fin.Bind` preserves decomposition failure and sequences a successful rank into the same typed singular-system refusal.

## `libs/dotnet/Rasm/.planning/Processing/flow.md:490`
`Ordinals(partition).Fold(Fin.Succ(Seq<SEdge<int>>()), (acc, cell) => acc.Bind(rows => Visited(...).Map(visited => rows + Transitions(...))))` -> `Ordinals(partition).TraverseM(cell => Visited(...).Map(visited => Transitions(...))).As().Map(chunks => chunks.Bind(static chunk => chunk))`.
`Seq.TraverseM` owns ordered fail-fast inversion and `Seq.Bind` flattens the independent per-cell edge sequences, removing the artificial monadic accumulator.

## `libs/dotnet/Rasm/.planning/Processing/remesh.md:400`
`Range(0, arena.VertexCount).Fold(Fin.Succ(unit), (acc, v) => acc.Bind(_ => { ...existing vertex body... }))` -> `Range(0, arena.VertexCount).ToSeq().TraverseM(v => { ...existing vertex body... }).As().Map(_ => unit)`.
`Seq.TraverseM` preserves ordered fail-fast vertex mutation while deleting the artificial `Fin<Unit>` accumulator; no vertex body consumes a prior result.

## `libs/dotnet/Rasm/.planning/Analysis/select.md:430`
`toSeq(brep.Faces.Select(face => TopologyProjection.Of(face)).ToArray()).TraverseM(identity).As()` -> `toSeq(brep.Faces).TraverseM(face => TopologyProjection.Of(face)).As()` in both borrowed and owned branches at lines 430-431.
Direct `Seq.TraverseM` owns the effectful projection and preserves face order and failure, deleting LINQ projection, array materialization, and identity inversion.

## `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:903`
`facet.Boundary.AsIterable().Map(static (v, i) => i).ToSeq()` -> `Range(0, facet.Boundary.Count).ToSeq()`.
The existing LanguageExt range is the exact ordinal carrier; it removes the fake value projection and avoids relying on an indexed `Iterable.Map` surface the pinned catalog does not expose.

## `libs/dotnet/Rasm/.planning/Processing/sample.md:1272`
`Enumerable.Range(...).Where(...).Select(...).Fold(seed, step)` -> `toSeq(Enumerable.Range(...).Where(...).Select(...)).Fold(seed, step)`.
LINQ `Select` returns `IEnumerable`, whose pinned LanguageExt extension surface has no `Fold`; the one `toSeq` re-entry preserves the nearest-candidate fold unchanged.

## `libs/dotnet/Rasm/.planning/Drawing/pack.md:193`
`Witness.ChannelError.Find(d.Channel).Match(Some: error => error <= tolerance, None: false)` -> `Witness.ChannelError.Find(d.Channel).Exists(error => error <= tolerance)`.
`Option.Exists` applies the predicate to `Some` and is false for `None`, exactly preserving the lossless test without a hand-written fold.

## `libs/dotnet/Rasm/.planning/Processing/session.md:40`
`Genus.Match(Some: genus => closed-manifold predicate, None: false)` -> `Genus.Exists(genus => closed-manifold predicate)`.
`Option.Exists` owns the present-only predicate and preserves the absent false result.

## `libs/dotnet/Rasm/.planning/Processing/segment.md:306`
`FaceRegions.Match(Some: regions => differs, None: false)` and `edgeCurvature.Map(predicate).IfNone(false)` -> `FaceRegions.Exists(regions => differs)` and `edgeCurvature.Exists(predicate)`; likewise use `symmetryPlane.Exists(plane => !plane.IsValid)` at lines 1098 and 1110.
The pinned `Option.Exists` has the identical false-on-absence truth table and removes four manual presence shells.

## `libs/dotnet/Rasm/.planning/Domain/hooks.md:135`
`Modalities.Held.Find(...).Match(Some: row => ignore(buffer.Swap(...)), None: () => unit)` -> `Modalities.Held.Find(...).Iter(row => ignore(buffer.Swap(...)))`.
`Option.Iter` already returns `Unit` and performs the effect only for `Some`, so the absent arm is redundant.

## `libs/dotnet/Rasm/.planning/Analysis/query.md:503`
`runtime.Telemetry.Match(Some: sink => Facts(...).Choose(...).Iter(...), None: () => unit)` -> `runtime.Telemetry.Iter(sink => Facts(...).Choose(...).Iter(...))`.
`Option.Iter` preserves the entire present-sink effect and the absent no-op while removing the outer hand-written fold.

## `libs/dotnet/Rasm/.planning/Analysis/query.md:426`
`Cancellation.IsCancellationRequested switch { true => Fin.Fail<Unit>(Errors.Cancelled), false => Fin.Succ(unit) }` -> `guard(!runtime.Cancellation.IsCancellationRequested, Errors.Cancelled).ToFin()` before the existing `.ToEff()`.
LanguageExt `guard` plus the pinned `Guard<Error,Unit>.ToFin` is the existing Boolean refusal lift and preserves the same cancellation error.

## `libs/dotnet/Rasm/.planning/Analysis/relations.md:110`
`Optional(c.Curve).Map(curve => curve.IsValid).IfNone(false)` -> `Optional(c.Curve).Exists(curve => curve.IsValid)`.
`Option.Exists` preserves both the valid-present test and the invalid/null false result in one carrier operation.

## `libs/dotnet/Rasm/.planning/Meshing/dec.md:161`
`topology.Genus.Map(genus => harmonic predicate).IfNone(false)` -> `topology.Genus.Exists(genus => harmonic predicate)`.
`Option.Exists` is the exact present-only Boolean fold and preserves the decision to omit harmonic forms when genus is absent.

## `libs/dotnet/Rasm/.planning/Analysis/select.md:167`
`Kind.Of(type).Map(kind => CanProject(...)).IfNone(false)` -> `Kind.Of(type).Exists(kind => CanProject(...))`.
`Option.Exists` preserves the unknown-kind false branch and the existing capability predicate.

## `libs/dotnet/Rasm/.planning/Domain/evaluation.md:61`
`Distance.Map(predicate).IfNone(false)` -> `Distance.Exists(predicate)`; at lines 66 and 354, `OpAcceptance.ValidityOf(...).IfNone(false)` -> `.Exists(static valid => valid)`.
`Option.Exists` preserves every required-present validity test and the absent false result without the mapped Boolean/default shells.

## `libs/dotnet/Rasm/.planning/Meshing/mesh.md:427`
`CommonSubdivisionSegments.Map(segments => segments == SumNormalCoordinates).IfNone(false)` -> `CommonSubdivisionSegments.Exists(segments => segments == SumNormalCoordinates)`.
`Option.Exists` preserves the absent false result and the exact equality predicate.

## `libs/dotnet/Rasm/.planning/Processing/sample.md:338`
`option.Map(predicate).IfNone(false)` -> `option.Exists(predicate)` for `DualSolve.Bind(...Gauge)` here, `terminal.DualSolve` at line 837, `selection.Algorithm` at line 916, and `assignment` at line 1287.
Each is a present-only Boolean test; `Option.Exists` preserves all four absent false branches without carrier round-trips.

## `libs/dotnet/Rasm/.planning/Domain/normalization.md:189`
`Kind.Of(type).Map(predicate).IfNone(false)` -> `Kind.Of(type).Exists(predicate)` here and in `KindAdmits` at line 199; likewise `Optional(output).Map(present => present switch {...}).IfNone(false)` -> `.Exists(present => present switch {...})` at line 397.
At line 445, also replace `OpAcceptance.ValidityOf(source).IfNone(false)` with `.Exists(static valid => valid)`; all four preserve the present predicate and unknown/null false result.

## `libs/dotnet/Rasm/.planning/Numerics/integrate.md:343`
`Published.Map(held => tableau predicate).IfNone(false)` -> `Published.Exists(held => tableau predicate)`.
`Option.Exists` preserves the absent unpublished result and the complete published-tableau predicate.

## `libs/dotnet/Rasm/.planning/Interaction/control.md:696`
`Values.Find(tag).Map(value => guard.Admit(value).IsSucc).IfNone(false)` -> `Values.Find(tag).Exists(value => guard.Admit(value).IsSucc)`.
`Option.Exists` keeps missing guarded values invalid and evaluates the existing admission only when present.

## `libs/dotnet/Rasm/.planning/Processing/flatten.md:337`
`residual.Map(value => value <= tolerance).IfNone(false)` -> `residual.Exists(value => value <= tolerance)`.
`Option.Exists` preserves the solver's absent-residual unsettled result and its existing tolerance comparison.

## `libs/dotnet/Rasm/.planning/Processing/geodesics.md:447`
`policy.Barrier.Map(barrier => barrier.Contains(edgeIndex)).IfNone(false)` -> `policy.Barrier.Exists(barrier => barrier.Contains(edgeIndex))`.
`Option.Exists` preserves no-barrier traversal and the present barrier membership test.

## `libs/dotnet/Rasm/.planning/Drawing/hatch.md:187`
`Census.Culled.Map(culled => culled <= total).IfNone(false)` -> `Census.Culled.Exists(culled => culled <= total)` in the present-instances arm.
`Option.Exists` preserves the required-present culled count and its existing upper-bound predicate.

## `libs/dotnet/Rasm/.planning/Meshing/offset.md:362`
`lastTime.Map(prior => ev.Time == prior).IfNone(false)` -> `lastTime.Exists(prior => ev.Time == prior)`.
`Option.Exists` preserves the first-event false result and the same-time comparison for later events.

## `libs/dotnet/Rasm/.planning/Spatial/cloud.md:614`
`Volume.Map(positive).IfNone(false) && Centroid.Map(finite).IfNone(false) && Extent.Map(positive).IfNone(false)` -> `Volume.Exists(positive) && Centroid.Exists(finite) && Extent.Exists(positive)`.
`Option.Exists` preserves the bounded-cell requirement that all three facts be present and valid while removing three manual defaults.
