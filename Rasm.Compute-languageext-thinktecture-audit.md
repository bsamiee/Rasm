# Rasm.Compute LanguageExt / Thinktecture audit

- `libs/dotnet/Rasm.Compute/.planning/Stats/monitor.md:29`
  - from the five hand-written `From(raw)` members -> `Op.Of(name: nameof(OfEwma)).AcceptValidated<T>(raw)` in `OfEwma`, with `nameof(OfQuantile)` / `nameof(OfDetector)` at those factories; delete `MonitorKey.From`, `Smoothing.From`, `FalseAlarm.From`, `Warmup.From`, and `Threshold.From`.
  - `Rasm.Domain.OpExtensions.AcceptValidated` is the existing Thinktecture-to-`Fin` bridge; default `ValidationError` is not a LanguageExt `Error`, so the wrappers do not truthfully type-check.

- `libs/dotnet/Rasm.Compute/.planning/Stats/estimator.md:66`
  - from `WindowCapacity.From(raw)` plus its wrapper -> `Op.Of(name: nameof(OfDetector)).AcceptValidated<WindowCapacity>(capacity)` in `StreamMonitor.OfDetector` and `Op.Of(name: nameof(Of)).AcceptValidated<WindowCapacity>(policy.Scoring.WindowCapacity)` in `TwinLoop.Of`; delete `WindowCapacity.From`.
  - The Domain bridge already maps generated admission once to `KernelFault`; the wrapper duplicates it and attempts to put `ValidationError` directly in `Fin`.

- `libs/dotnet/Rasm.Compute/.planning/Stats/monitor.md:109`
  - from each tuple `.Apply(...).ToFin()` -> `.Apply(...).As().ToFin()` at all three `StreamMonitor` factories.
  - Tuple `Apply` returns `K<Validation<Error>,T>` at the pinned LanguageExt API; `As()` is the required existing re-anchor before concrete `ValidationExtensions.ToFin` can bind.

- `libs/dotnet/Rasm.Compute/.planning/Stats/estimator.md:184`
  - from every tuple `.Apply(...).ToFin()` in `FitBudget.Admit`, `EstimatorPolicy.Admit`, and `Design.Admit` -> `.Apply(...).As().ToFin()`.
  - These six calls currently leave a `K<Validation<Error>,T>`; LanguageExt requires `As()` and no local conversion or helper.

- `libs/dotnet/Rasm.Compute/.planning/Stats/families.md:272`
  - from each detector-policy tuple `.Apply(...).ToFin()` -> `.Apply(...).As().ToFin()`.
  - The existing `ValidationExtensions.As` is mandatory for the K-kinded tuple result; adding it preserves the accumulating policy and introduces no symbol.

- `libs/dotnet/Rasm.Compute/.planning/Symbolic/expression.md:99`
  - from `Validate(..., out SymbolicExpr admitted) is ComputeFault refusal ? Fin.Fail(refusal) : Fin.Succ(admitted)` -> `Op.Of(name: nameof(Admit)).AcceptValidated<SymbolicExpr>(Validate(..., out SymbolicExpr admitted), admitted)`.
  - `[ComplexValueObject]` returns `ValidationError?`, never `ComputeFault`; the existing multi-member Domain bridge performs the required one-time translation and removes the impossible type test.

- `libs/dotnet/Rasm.Compute/.planning/Symbolic/dimensional.md:136`
  - from the private `Admit` body testing `SymbolName.Validate(...) is ComputeFault` -> `Op.Of(name: nameof(Admit)).AcceptValidated<SymbolName>(name).ToValidation()`.
  - `SymbolName` uses the default Thinktecture error, so the current test can never match; Domain already owns the non-throwing keyed admission bridge.

- `libs/dotnet/Rasm.Compute/.planning/Analysis/lifecycle.md:97`
  - from `[ObjectFactory<string>]` plus obsolete `static Validation<string> ValidateFactoryArguments(ref string)` -> `[ValueObject<string>]` plus the pinned `static partial void ValidateFactoryArguments(ref ValidationError?, ref string)` hook; remove `[ObjectFactory<string>]`.
  - The pinned Thinktecture catalog exposes the ref-error partial hook; the current method is not generator-called validation and the extra factory attribute adds no domain behavior.

- `libs/dotnet/Rasm.Compute/.planning/Analysis/energy.md:30`
  - from `[ObjectFactory<string>]` plus obsolete `static Validation<string> ValidateFactoryArguments(ref string)` -> `[ValueObject<string>]` plus the pinned `static partial void ValidateFactoryArguments(ref ValidationError?, ref string)` hook; remove `[ObjectFactory<string>]`.
  - `SolverPin` currently declares a method the pinned generator does not call; the generated value-object hook preserves the same prefix rule with one owner.

- `libs/dotnet/Rasm.Compute/.planning/Stats/monitor.md:29`
  - from reads of `.Value` on bare `[ValueObject<T>]` owners (`MonitorKey`, `Smoothing`, `FalseAlarm`, `Warmup`, `Threshold`) -> their generated `.ToValue()` projection.
  - Bare Thinktecture value objects generate a private `_value`, not public `Value`; `ToValue()` is already generated and avoids widening every owner with a public key member.

- `libs/dotnet/Rasm.Compute/.planning/Stats/estimator.md:61`
  - from `Capacity.Value` on bare `[ValueObject<int>] WindowCapacity` -> `Capacity.ToValue()`.
  - The pinned generator already exposes `ToValue()`; using it fixes the nonexistent public member without adding a property or module symbol.

- `libs/dotnet/Rasm.Compute/.planning/Symbolic/expression.md:26`
  - from `.Value` reads on bare `SymbolName`, `Order`, `Degree`, and `Finite` value objects -> `.ToValue()`.
  - These declarations do not request a public key member; the generated projection is the truthful existing surface and preserves every scalar value.

- `libs/dotnet/Rasm.Compute/.planning/Symbolic/dimensional.md:44`
  - from `DimensionMonomial.Value` in the indexer/operators/projections -> `DimensionMonomial.ToValue()`.
  - `[ValueObject<Seq<ERational>>]` keeps `_value` private by default; the generated `ToValue()` preserves the exact sequence without adding a hand property.

- `libs/dotnet/Rasm.Compute/.planning/Symbolic/units.md:26`
  - from `UnitToken.Value` and `FormatSpec.Value` -> their generated `.ToValue()` projections.
  - Neither declaration requests public `Value`; this composes the pinned Thinktecture surface rather than inventing an absent member.

- `libs/dotnet/Rasm.Compute/.planning/Analysis/lifecycle.md:248`
  - from `Omf.Value` at URI/key writers -> `Omf.ToValue()`.
  - Bare `[ValueObject<string>]` exposes the generated key through `ToValue()`, not a public `Value` property.

- `libs/dotnet/Rasm.Compute/.planning/Analysis/energy.md:38`
  - from `SolverPin.Version => Value[...]` -> `SolverPin.Version => ToValue()[...]`.
  - This uses the generated key projection and removes reliance on a member the declaration does not emit.

- `libs/dotnet/Rasm.Compute/.planning/Runtime/scheduling.md:290`
  - from `.Value` on bare `Percent`, `SpillScale`, `JobId`, `GangKey`, `DeviceToken`, and `ByteBudget` owners -> `.ToValue()`.
  - The pinned generator keeps each key private; its existing projection preserves behavior without adding six public properties.

- `libs/dotnet/Rasm.Compute/.planning/Runtime/progress.md:93`
  - from `SegmentCount.Value` -> `SegmentCount.ToValue()`.
  - The bare value object has no generated public `Value`; `ToValue()` is the existing zero-symbol replacement.

- `libs/dotnet/Rasm.Compute/.planning/Tensor/vocabulary.md:86`
  - from `PositiveScale.Value` -> `PositiveScale.ToValue()`.
  - This uses the generated key projection instead of assuming a public member absent from the declaration.

- `libs/dotnet/Rasm.Compute/.planning/Model/generative.md:953`
  - from `RewindFloor.Value` at `RewindTo` -> `RewindFloor.ToValue()`.
  - The pinned value-object generator already publishes `ToValue()` while its default key field is private.

- `libs/dotnet/Rasm.Compute/.planning/Tensor/residency.md:127`
  - from `TensorBridge.NativeBytes(...).Match(Succ: Validation.Success, Fail: Validation.Fail)` -> `TensorBridge.NativeBytes(...).ToValidation()`.
  - LanguageExt owns this identity-preserving carrier conversion; the hand fold adds no policy or domain value.

- `libs/dotnet/Rasm.Compute/.planning/Symbolic/lowering.md:129`
  - from `source.Tree.Match(Succ: Success, Fail: Fail)` -> `source.Tree.ToValidation()`.
  - This is exactly LanguageExt's `Fin<Error,T> -> Validation<Error,T>` conversion and preserves both arms unchanged.

- `libs/dotnet/Rasm.Compute/.planning/Solver/discretization.md:131`
  - from `element.Quadrature.Match(Succ: Success(unit), Fail: Fail)` -> `element.Quadrature.Map(static _ => unit).ToValidation()`.
  - LanguageExt already preserves the failure while mapping the success payload; the two-arm fold is redundant.

- `libs/dotnet/Rasm.Compute/.planning/Symbolic/units.md:295`
  - from `UnitsEdge.Convert(...).Match(Some: Fin.Succ, None: () => Fin.Fail(error))` -> `UnitsEdge.Convert(...).ToFin(error)`.
  - `Option.ToFin` is the exact existing LanguageExt absence-to-error conversion.

- `libs/dotnet/Rasm.Compute/.planning/Symbolic/units.md:318`
  - from `ByType.Value.Find(type).Match(Some: Fin.Succ, None: () => Fin.Fail(error))` -> `ByType.Value.Find(type).ToFin(error)`.
  - LanguageExt owns the same projection with identical success and failure behavior.

- `libs/dotnet/Rasm.Compute/.planning/Tensor/vocabulary.md:226`
  - from `toSeq(...).OrderBy(...).Find(...).Match(...)` -> `toSeq(toSeq(...).OrderBy(...)).Find(...).ToFin(TensorReason.DtypeMismatch.Fault(...))`.
  - LINQ ordering exits `Seq`, so the outer `toSeq` restores `Find`; `Option.ToFin` then preserves the exact existing refusal without a hand fold.

- `libs/dotnet/Rasm.Compute/.planning/Tensor/vocabulary.md:238`
  - from `toSeq(...).OrderBy(...).Head.Match(Some: Fin.Succ, None: () => Fin.Succ(MixedSignFloor))` -> `Fin.Succ(toSeq(toSeq(...).OrderBy(...)).Head.IfNone(MixedSignFloor))`.
  - `OrderBy` returns `IOrderedEnumerable<T>`, which has neither LanguageExt `Head` nor `Match`; re-entry plus `Option.IfNone` is the existing total first-or-floor read.

- `libs/dotnet/Rasm.Compute/.planning/Tensor/vocabulary.md:258`
  - from `Find(admits).Match(Some: Fin.Succ, None: () => slug.Fail(...))` -> `Find(admits).ToFin(slug.Fault(...))`.
  - The row already exposes the exact `Error`; LanguageExt performs the carrier conversion directly.

- `libs/dotnet/Rasm.Compute/.planning/Tensor/residency.md:532`
  - from `axis.Derive(...).Match(Some: extent => Fin.Succ((axis, extent)), None: () => Fail(...))` -> `axis.Derive(...).Map(extent => (axis, extent)).ToFin(error)`.
  - LanguageExt `Map` plus `ToFin` is the existing option projection and preserves the same per-axis refusal before `TraverseM`.

- `libs/dotnet/Rasm.Compute/.planning/Model/sessions.md:96`
  - from private `Guarded(held, evidence)` -> `Refusal.Unless(held, ComputeArea.Model, evidence)`; delete `Guarded`.
  - `Runtime/admission` already owns this exact `Validation<Error,Unit>` constructor and is already composed by Model/identity.

- `libs/dotnet/Rasm.Compute/.planning/Model/sessions.md:85`
  - from `Seq(...).Traverse(static claim => claim).As().ToFin()` -> `AdmissionSlots.Accumulate(Seq(...)).ToFin()`.
  - Domain already owns this exact independent-validation accumulation; the replacement preserves all failures while removing the hand applicative fold.

- `libs/dotnet/Rasm.Compute/.planning/Model/sessions.md:234`
  - from both `residents.ToSeq()` pair walks -> `residents.AsIterable()` before the existing `Map`/`Filter`/`OrderBy` chains.
  - Pinned `AtomHashMap.ToSeq()` returns `Seq<Row>` values only; `AsIterable()` is its existing named `(Key, Value)` carrier, preserving eviction and seating while restoring the accessed fields.

- `libs/dotnet/Rasm.Compute/.planning/Model/stage.md:659`
  - from `Parity.ToSeq().Fold(... row.Key ... row.Value ...)` -> `Parity.AsIterable().Fold(... row.Key ... row.Value ...)`, with `coldest is { IsSome: true, Case: (string _, long read) }`.
  - `AtomHashMap.ToSeq()` drops keys; `AsIterable()` is the pinned pair walk, and the existing `Option` pattern must carry `IsSome` before reading `Case`.

- `libs/dotnet/Rasm.Compute/.planning/Model/generative.md:726`
  - from `Witnesses.ToSeq().Map(pair => pair.Key)` -> `Witnesses.AsIterable().Map(static pair => pair.Key)`.
  - The pinned atom-map `ToSeq()` yields values, not pairs; its existing `AsIterable()` snapshot preserves the key-removal policy without another projection owner.

- `libs/dotnet/Rasm.Compute/.planning/Model/stage.md:618`
  - from `provider.AutoSelect.HeadOrNone()` -> `provider.AutoSelect.Head`.
  - `HeadOrNone` is absent from pinned `Seq`; its `Head` property already returns the exact `Option<OrtEpDevice>` required by `ResultKey`.

- `libs/dotnet/Rasm.Compute/.planning/Model/generative.md:1044`
  - from `cursors.HeadOrNone().Case is SequenceCursor first` -> `cursors.Head is { IsSome: true, Case: SequenceCursor first }`.
  - Pinned `Seq` has no `HeadOrNone`; `Head` is the existing optional read and the proof-carrying pattern is the catalogued public `Option` projection.

- `libs/dotnet/Rasm.Compute/.planning/Solver/satisfy.md:51`
  - from `Items.Filter(...).OrderByDescending(...).HeadOrNone().IfNone(Rule)` -> `toSeq(toSeq(Items).Filter(...).OrderByDescending(...)).Head.IfNone(Rule)`.
  - Thinktecture `Items` is `IReadOnlyList<TrackClass>` and ordering then exits `Seq`; the two existing `toSeq` lifts plus `Head` preserve longest-prefix selection and the same floor.

- `libs/dotnet/Rasm.Compute/.planning/Runtime/tiles.md:84`
  - from `Items.Find(row => fraction <= row.UpperFraction).IfNone(Critical)` -> `toSeq(Items).Find(row => fraction <= row.UpperFraction).IfNone(Critical)`.
  - Thinktecture `FeatureBand.Items` is `IReadOnlyList<FeatureBand>`; Prelude `toSeq` reaches LanguageExt `Seq.Find` and preserves first declaration-order threshold plus the existing fallback.

- `libs/dotnet/Rasm.Compute/.planning/Runtime/field.md:208`
  - from both `FieldRank.Items.Find(...)` calls in `Ranked` -> `toSeq(FieldRank.Items).Find(...)` at lines 208 and 384.
  - Thinktecture exposes `Items` as `IReadOnlyList<FieldRank>`, not a LanguageExt carrier; `toSeq` makes the existing `Find(...).ToValidation(customError)` chains bind without changing either refusal.

- `libs/dotnet/Rasm.Compute/.planning/Analysis/capacity.md:458`
  - from `LimitState.Items.ToSeq().Filter(...).Map(...)` -> `toSeq(LimitState.Items).Filter(...).Map(...)`.
  - LanguageExt `.ToSeq()` has no `IEnumerable<T>` receiver, while Thinktecture `Items` is `IReadOnlyList<LimitState>`; Prelude `toSeq` preserves roster order and the complete capacity projection.

- `libs/dotnet/Rasm.Compute/.planning/Analysis/circulation.md:146`
  - from `view.Adjacency.Edges.ToSeq().Map(...)` -> `toSeq(view.Adjacency.Edges).Map(...)`.
  - QuikGraph `AdjacencyGraph.Edges` is `IEnumerable<EgressEdge>` and LanguageExt publishes no such `.ToSeq()` extension; Prelude `toSeq` preserves the exact arc enumeration before `Map`.

- `libs/dotnet/Rasm.Compute/.planning/Tensor/dispatch.md:1157`
  - from `Enumerable.Range(0, columns).Map(column => assigned.Find(column).IfNone(0))` -> `Enumerable.Range(0, columns).Select(column => assigned.Find(column).IfNone(0))`.
  - LanguageExt has no `Map` extension for `IEnumerable<int>`; BCL `Select` is the existing projection and preserves the collection-expression order and assigned-color fallback with no carrier shell.

- `libs/dotnet/Rasm.Compute/.planning/Solver/clash.md:577`
  - from `mismatched.Head.Computed` / `.Measured` -> `mismatched[0].Computed` / `.Measured` under the existing `!mismatched.IsEmpty` guard.
  - `Seq.Head` is `Option<T>` and cannot expose tuple fields; the pinned indexer preserves the already-proved nonempty first-row read without a helper.

- `libs/dotnet/Rasm.Compute/.planning/Solver/clash.md:716`
  - from `Segment.Add((scored.Window.Residuals.Last, scored.Verdict.Anomaly))` -> `Segment.Add((scored.Verdict.Residual, scored.Verdict.Anomaly))`.
  - `ResidualWindow` owns `Values`, not `Residuals`, and `Seq.Last` would be `Option<double>`; the already-returned `TwinVerdict.Residual` is the exact sample being committed, avoiding both the nonexistent member and a redundant carrier read.

- `libs/dotnet/Rasm.Compute/.planning/Runtime/channels.md:616`
  - from `admitted.Head.Artifact` and the three `ordered.Head.Artifact` reads -> `admitted[0].Artifact` and `ordered[0].Artifact`.
  - `Seq.Head` returns `Option<ArtifactFrame>`; the preceding shared `Admit` proves nonempty, so the existing indexer preserves the invariant and removes invalid direct dereferences.

- `libs/dotnet/Rasm.Compute/.planning/Runtime/scheduling.md:930`
  - from `component.Map(...).Order(...).Head()` -> `toSeq(component.Map(...).Order(...))[0]`.
  - Ordering exits `Seq` and pinned LanguageExt has no `IEnumerable.Head()`; the component was already filtered to `Count > 1`, so the existing indexer preserves the deterministic minimum member.

- `libs/dotnet/Rasm.Compute/.planning/Runtime/codecs.md:435`
  - from the hand `Identifier.Validate(... out admitted) ? Fail(error) : Success(admitted)` -> `Op.Of(name: nameof(Admitted)).AcceptValidated<Identifier>(raw.Replace('-', '_')).ToValidation()`.
  - `Identifier` returns Thinktecture `ValidationError`, not LanguageExt `Error`; the existing Domain bridge performs the typed translation while preserving the local normalization policy.

- `libs/dotnet/Rasm.Compute/.planning/Runtime/field.md:310`
  - from both `AdmittedField.Validate(... out admitted)` branches casting `ValidationError` to `ComputeFault` -> `Op.Of(name: nameof(FieldEncode)).AcceptValidated<AdmittedField>(AdmittedField.Validate(...), admitted).Bind(...)`, and the same with `nameof(Hdf5Encode)` at that entry.
  - The cast is invalid because the generated error is Thinktecture `ValidationError`; Domain's existing multi-member bridge preserves generated admission for both encode paths.

- `libs/dotnet/Rasm.Compute/.planning/Runtime/field.md:323`
  - from `FieldHeader.Validate(... out header)` plus `(ComputeFault)error` -> `Op.Of(name: nameof(Pack)).AcceptValidated<FieldHeader>(FieldHeader.Validate(...), header).Bind(header => ...)`.
  - `FieldHeader` uses the default generated error, so the cast cannot succeed; the existing multi-member Domain bridge keeps the exact header admission and compression continuation.

- `libs/dotnet/Rasm.Compute/.planning/Analysis/energy.md:892`
  - from `while (frontier.Head.Case is NodeId node)` -> `while (frontier.Head is { IsSome: true, Case: NodeId node })`.
  - `Head` returns `Option<NodeId>` and the pinned public payload read requires an `IsSome` proof; the loop still terminates exactly on an empty frontier.

- `libs/dotnet/Rasm.Compute/.planning/Solver/route.md:898`
  - from private `Claim(held, evidence)` -> `Refusal.Unless(held, ComputeArea.Solver, evidence)`; delete `Claim`.
  - The package-level owner has the identical success and `ComputeFault.Violation` failure branches.

- `libs/dotnet/Rasm.Compute/.planning/Solver/route.md:616`
  - from both `Seq(...).Traverse(static claim => claim).As().ToFin()` admission folds -> `AdmissionSlots.Accumulate(Seq(...)).ToFin()`.
  - The pre-existing Domain owner is definitionally the same accumulating traversal and preserves the following `Bind`/`Map` payload projections.

- `libs/dotnet/Rasm.Compute/.planning/Solver/discretization.md:166`
  - from both private `Claim` copies -> `Refusal.Unless(held, ComputeArea.Solver, evidence)`; delete both symbols.
  - These are byte-for-byte semantic duplicates of the existing package owner, including the fault area.

- `libs/dotnet/Rasm.Compute/.planning/Solver/discretization.md:133`
  - from both claim-sequence `.Traverse(identity).As().Map(...)` folds -> `AdmissionSlots.Accumulate(Seq(...)).Map(...)`, retaining the existing `.ToFin()`.
  - Domain's existing fold accumulates the same `Validation<Error,Unit>` slots; the candidate projections remain unchanged and the traversal spelling disappears.

- `libs/dotnet/Rasm.Compute/.planning/Solver/assembly.md:376`
  - from private `Claim` -> `Refusal.Unless(..., ComputeArea.Solver, ...)`, and from private applicative `Admit` -> `AdmissionSlots.Accumulate(toSeq(claims)).ToFin()`; delete both symbols.
  - Existing Compute and Domain owners preserve the same fault and independent-error accumulation while removing two module helpers.

- `libs/dotnet/Rasm.Compute/.planning/Solver/satisfy.md:289`
  - from private `Claim(held, evidence)` -> `Refusal.Unless(held, ComputeArea.Solver, evidence)`; delete `Claim`.
  - The shared owner returns the identical accumulating validation slot.

- `libs/dotnet/Rasm.Compute/.planning/Solver/satisfy.md:257`
  - from the outer and per-grounding `.Traverse(static claim => claim).As().Map(static _ => unit)` folds -> `AdmissionSlots.Accumulate(...)`.
  - These receivers are already `Seq<Validation<Error,Unit>>`; the Domain owner preserves nested independent-error accumulation without reimplementing it twice.

- `libs/dotnet/Rasm.Compute/.planning/Solver/contract.md:361`
  - from both private `Claim` copies -> `Refusal.Unless(held, ComputeArea.Solver, evidence)`; delete both symbols.
  - Runtime/admission already owns the exact `ComputeViolation -> Validation` lift used elsewhere by Solver.

- `libs/dotnet/Rasm.Compute/.planning/Solver/contract.md:342`
  - from both claim-roster `.Traverse(identity).As()` accumulation chains -> `AdmissionSlots.Accumulate(...)`, retaining their existing `.ToFin()` and payload `Map`.
  - The Domain function owns exactly this fold over concrete validation slots, preserving every accumulated error and result projection.

- `libs/dotnet/Rasm.Compute/.planning/Solver/constitutive.md:305`
  - from both private `Claim` copies and private `Require` -> `Refusal.Unless(held, ComputeArea.Solver, evidence)`, adding `.ToFin()` at monadic `Require` sites; delete all three symbols.
  - The shared owner preserves applicative accumulation or the same `Fin` short-circuit and identical typed violation while reducing duplicate module surface.

- `libs/dotnet/Rasm.Compute/.planning/Solver/constitutive.md:440`
  - from all three claim-sequence `.Traverse(identity).As().Map(unit)` folds -> `AdmissionSlots.Accumulate(...)`, retaining the enclosing payload `Map`/`.ToFin()`.
  - Domain's existing accumulator is the same applicative traversal, including the nested optional-weight slot, so no validation behavior moves.

- `libs/dotnet/Rasm.Compute/.planning/Solver/sweep.md:377`
  - from the claim roster's `.Traverse(static claim => claim).As().Map(static _ => unit).ToFin()` -> `AdmissionSlots.Accumulate(Seq(...)).ToFin()`.
  - The existing Domain fold accumulates precisely these independent validation slots and removes the repeated carrier plumbing.

- `libs/dotnet/Rasm.Compute/.planning/Solver/optimizer.md:459`
  - from both claim-roster `.Traverse(identity).As().Map(unit).ToFin()` chains -> `AdmissionSlots.Accumulate(Seq(...)).ToFin()`.
  - Domain already owns the identical accumulation semantics, preserving every typed Solver violation without local applicative repetition.

- `libs/dotnet/Rasm.Compute/.planning/Solver/uncertainty.md:306`
  - from the claim roster's `.Traverse(static claim => claim).As().Map(static _ => unit).ToFin()` -> `AdmissionSlots.Accumulate(Seq(...)).ToFin()`.
  - The existing Domain accumulator preserves all independent failures and removes the hand-written traversal shell.

- `libs/dotnet/Rasm.Compute/.planning/Model/generative.md:59`
  - from unused `ChatRole.FromWire` forwarding generated `TryGet` into `Option` -> delete `FromWire`.
  - Thinktecture already owns keyed smart-enum lookup, and no spec-sheet consumer reads this wrapper; deletion removes one module symbol with no behavior change.

- `libs/dotnet/Rasm.Compute/.planning/Model/stage.md:67`
  - from one-use `LicenseClass.FromWire(wire)` plus `SelectedLicense => LicenseClass.FromWire(License)` -> inline `LicenseClass.TryGet(License, out LicenseClass? row) ? Some(row!) : None` in `SelectedLicense`; delete `FromWire`.
  - Thinktecture already owns ordinal keyed lookup, and the only consumer needs that exact optional lift; inlining deletes a module symbol without changing comparison, absence, or dependency direction.

- `libs/dotnet/Rasm.Compute/.planning/Model/providers.md:164`
  - from both `row.WireKey.Case is string key && StringComparer.Ordinal.Equals(key, wire)` predicates -> `row.WireKey.Exists(key => StringComparer.Ordinal.Equals(key, wire))`.
  - LanguageExt `Option.Exists` is exactly false on absence and applies the same ordinal predicate on presence, removing manual payload probing in both `ModelPrecision.FromWire` and `ExecutionProvider.FromWire`.

- `libs/dotnet/Rasm.Compute/.planning/Model/embedding.md:75`
  - from the three `codebook.Match(...)` folds -> `codebook.Map(...).IfNone(0L)` for byte length and `codebook.Map(VectorOps.EncodeProduct/Reconstruct).ToFin(EmbedRefusal.CodebookMissing.Fault())` for encode/decode.
  - LanguageExt already owns the optional projection, default, and absence-to-error lift; the existing `EmbedRefusal` remains the sole failure owner.

- `libs/dotnet/Rasm.Compute/.planning/Analysis/lifecycle.md:485`
  - from `page.Match(Succ: answer => Freshest(...), Fail: _ => None)` -> `page.ToOption().Bind(answer => Freshest(answer.Rows, now).Bind(static row => row.Evidence.Reference))`.
  - `Fin.ToOption` discards exactly the failure the current fold discards, and the existing option chain preserves winner selection.

- `libs/dotnet/Rasm.Compute/.planning/Runtime/claims.md:117`
  - from `Extent` rebuilding `Validation` with `Match(Succ: Success(unit), Fail: Fail(Rejected(...)))` -> `.Map(static _ => unit).MapFail(static _ => Rejected("extent", "overflow")).ToValidation()`.
  - LanguageExt already owns success mapping, failure projection, and `Fin`-to-`Validation`; the deliberate overflow remap is preserved without branch reconstruction.

- `libs/dotnet/Rasm.Compute/.planning/Runtime/scheduling.md:98`
  - from `s.Rank.Match(Some: order => Fin.Succ(CreateUnboundedPrioritized(...)), None: () => Fin.Fail(error))` -> `s.Rank.ToFin(error).Map(order => CreateUnboundedPrioritized(...))`.
  - `Option.ToFin` and `Fin.Map` preserve the same missing-rank fault and channel construction without a manual carrier fold.

- `libs/dotnet/Rasm.Compute/.planning/Tensor/memory.md:86`
  - from `request.CopyReason.Match(Some: reason => !string.IsNullOrWhiteSpace(reason), None: () => false)` -> `request.CopyReason.Exists(static reason => !string.IsNullOrWhiteSpace(reason))`.
  - LanguageExt `Option.Exists` has the identical false-on-absence predicate semantics and removes both hand-written arms.

- `libs/dotnet/Rasm.Compute/.planning/Tensor/blas.md:334`
  - from `Optional(pivots).Match(Some: p => Fin.Succ(ldl_solve(...)), None: () => NativeRejected.Fail(...))` -> `Optional(pivots).Map(p => torch.linalg.ldl_solve(ld, p, b, hermitian: true)).ToFin(TensorReason.NativeRejected.Fault("aten-ldl-no-pivots"))`.
  - LanguageExt owns the option projection and lift, while the existing `TensorReason.Fault` preserves the exact failure payload.

- `libs/dotnet/Rasm.Compute/.planning/Tensor/residency.md:199`
  - from `OrtElementBytes.Match(None: ByteStrideAbsent.Fail, Some: stride => ...)` -> `OrtElementBytes.ToFin(TensorReason.ByteStrideAbsent.Fault("ingress-byte-stride", row.Key)).Bind(stride => ...)`.
  - The existing `Option.ToFin` performs the same missing-stride lift before the unchanged overflow and multiplication logic.

- `libs/dotnet/Rasm.Compute/.planning/Tensor/vocabulary.md:75`
  - from `OrtElementBytes.Match(...)` in `ElementCount` and `ZeroPointDomain.Match(...)` in `QuantizationPolicy.Admit` -> `.ToFin(TensorReason.ByteStrideAbsent.Fault("no-byte-stride", Key)).Bind(stride => ...)` and `.ToFin(TensorReason.QuantizationInvalid.Fault("quantization-on-unquantized-row", row.Key)).Bind(domain => ...)`.
  - Both folds only lift absence into an existing tensor fault; LanguageExt owns that lift and leaves all present-value validation unchanged.

- `libs/dotnet/Rasm.Compute/.planning/Tensor/factor.md:450`
  - from `Optional(DulmageMendelsohn.Generate(csc)).Match(None: StructuralRank.Fail, Some: dm => ...)` -> `.ToFin(TensorReason.StructuralRank.Fault("sparse-structural-matching", kind.Key, $"{csc.RowCount}x{csc.ColumnCount}")).Bind(dm => ...)`.
  - LanguageExt supplies the exact absence-to-error lift; the existing structural-rank branch and witness remain unchanged.

- `libs/dotnet/Rasm.Compute/.planning/Symbolic/units.md:137`
  - from the three parse/from option `Match(Some: typed => AdmitQuantity(...), None: () => Fin.Fail(ParseRejected))` folds -> `.ToFin(new ComputeFault.ParseRejected(current "unit-text" / "unit-declared" / "unit-abbreviation" payload)).Bind(typed => AdmitQuantity(...))`.
  - `Option.ToFin` preserves each distinct parsing refusal and `Bind` preserves the admitted-value continuation without three manual folds.

- `libs/dotnet/Rasm.Compute/.planning/Solver/exact.md:72`
  - from the outer `problem.Exact.Match(...)` in `SolveCpSat` / `SolveMilp` and `problem.Routing.Match(...)` in `RoutingSearch.Solve` -> `.ToFin(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Required(ComputeSubject.Resource))).Bind(model => ...)`.
  - LanguageExt owns the required-option lift; every solver body and its current failure behavior remain unchanged while three branch shells disappear.

- `libs/dotnet/Rasm.Compute/.planning/Solver/sweep.md:582`
  - from `settled.Value.Match(Some: field => Fin.Succ(field with {...}), None: () => Fin.Fail(required-value))` -> `settled.Value.Map(field => field with {...}).ToFin(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Required(ComputeSubject.Value)))`.
  - `Option.Map` plus `ToFin` preserves the exhausted verdict and identical absence fault without hand-constructing either carrier arm.

- `libs/dotnet/Rasm.Compute/.planning/Symbolic/expression.md:255`
  - from `c.Bindings.Fold(src.Entity, (acc, pair) => ... pair.Key ... pair.Value ...)` -> `c.Bindings.AsIterable().Fold(src.Entity, (acc, pair) => ...)`.
  - LanguageExt `Map.Fold` supplies values only; its existing `AsIterable()` pair carrier preserves every keyed substitution without a helper.

- `libs/dotnet/Rasm.Compute/.planning/Model/run.md:475`
  - from both `window.Iter((pending, row) => ...)` calls -> `window.Iter((row, pending) => ...)` at lines 475 and 488.
  - Pinned LanguageExt indexed `Iter` is index-first; swapping the existing parameters restores the intended row copy and reply indexing with no new surface.

- `libs/dotnet/Rasm.Compute/.planning/Solver/exact.md:291`
  - from `model.Dimensions.Iter((spec, index) => ...)` -> `model.Dimensions.Iter((index, spec) => ...)`.
  - LanguageExt indexed `Iter` passes the index first; the corrected binding preserves `spec.Slack` and the intended `callbacks[index + 1]` lookup.

- `libs/dotnet/Rasm.Compute/.planning/Symbolic/dimensional.md:131`
  - from `Bindings.Find(symbol).Match(Some: Success, None: () => Fail(error))` -> `Bindings.Find(symbol).ToValidation(error)`.
  - LanguageExt `Option.ToValidation(L)` owns the identical present/absent lift and preserves the existing `SymbolUndefined` refusal.

- `libs/dotnet/Rasm.Compute/.planning/Model/run.md:565`
  - from `envelope.Match(Some: held => held.Drift(...).Map(report => Some(report.Worst)), None: () => Fin.Succ(None))` -> `envelope.TraverseM(held => held.Drift(serving, policy).Map(static report => report.Worst)).As()`.
  - LanguageExt `Option.TraverseM` is total over absence and yields the same `Fin<Option<DriftVerdict>>` without rebuilding either carrier arm.

- `libs/dotnet/Rasm.Compute/.planning/Solver/contract.md:402`
  - from the four-case `this switch` plus `_ => false` -> generated `Switch(uniformElastic: ..., uniformScalar: ..., perCellElastic: ..., perCellScalar: ...)` returning `bool`.
  - Thinktecture already owns exhaustive union dispatch; this preserves all predicates while removing the bogus fallback that hides a newly generated case.

- `libs/dotnet/Rasm.Compute/.planning/Analysis/energy.md:650`
  - from `toSeq(cells).TraverseM(cell => Rows(... cell.Key, cell.Value))` -> `cells.AsIterable().TraverseM(cell => Rows(... cell.Key, cell.Value))`.
  - LanguageExt `HashMap` has two enumerable projections, so `toSeq(cells)` is ambiguous; `AsIterable()` is its existing named key/value walk.

- `libs/dotnet/Rasm.Compute/.planning/Symbolic/lowering.md:131`
  - from one-use `Proven(source, proof)` / `Distinct(symbolOrder)` validation helpers -> inline `AdmissionSlots.Gate` with their existing conditions and faults; delete `Proven` and `Distinct`.
  - Domain `AdmissionSlots.Gate` owns this exact package-fault lift; `Covering` retains its richer missing-symbol projection while two duplicate module symbols disappear.

- `libs/dotnet/Rasm.Compute/.planning/Analysis/daylight.md:124`
  - from one-use `Calendar(calendar)` -> inline `AdmissionSlots.Gate(Calendars.Contains(calendar), existing AnalysisFailed)` in the tuple; delete `Calendar`.
  - Domain already owns the same bool-plus-owned-error validation slot, preserving applicative accumulation and removing one module symbol.

- `libs/dotnet/Rasm.Compute/.planning/Analysis/daylight.md:239`
  - from one-use `DesignDays(request)` / `Requirement(request)` helpers -> inline two `AdmissionSlots.Gate` entries with their current predicates and faults; delete both helpers.
  - Domain's existing gate preserves the two independently accumulated daylight refusals while removing duplicate conditional validation constructors.

- `libs/dotnet/Rasm.Compute/.planning/Tensor/residency.md:115`
  - from one-use `Element(row)` / `Stride(row)` helpers -> inline `AdmissionSlots.Gate(row.Clr == typeof(T), existing DtypeMismatch)` and `Gate(row.OrtElementBytes.IsSome, existing ByteStrideAbsent)`; delete both helpers.
  - Domain owns the identical validation lift, so the existing tuple accumulation and `Volume` payload remain unchanged with two fewer module symbols.

- `libs/dotnet/Rasm.Compute/.planning/Solver/discretization.md:659`
  - from one-use `RefineTemplate.For(mesh.Element).Case is not RefineTemplate template` -> `!RefineTemplate.TryGet(mesh.Element.Key, out RefineTemplate? template)` at `Subdivide`; delete `For`.
  - Thinktecture already generates the exact keyed lookup consumed here; direct use preserves the carry-on-miss branch and removes the optional forwarding symbol.

- `libs/dotnet/Rasm.Compute/.planning/Stats/estimator.md:467`
  - from `IterativeResponse(context) => RegularizedResponse(context)` and callers naming `EstimatorFold.IterativeResponse` -> bind those callers directly to `EstimatorFold.RegularizedResponse`; delete `IterativeResponse`.
  - The existing fold is the complete implementation; the forwarding delegate adds no behavior and only expands module surface.

- `libs/dotnet/Rasm.Compute/.planning/Stats/estimator.md:486`
  - from `MixtureDesign(context) => GroupingDesign(context)` and the GMM row naming `EstimatorFold.MixtureDesign` -> bind that row directly to `EstimatorFold.GroupingDesign`; delete `MixtureDesign`.
  - The existing fold is behavior-identical; the forwarding symbol duplicates its owner without specialization.

- `libs/dotnet/Rasm.Compute/.planning/Solver/clash.md:702`
  - from `IO.lift(() => Attempt(signal, weight)).Bind(settled => settled.Match(Succ: IO.pure, Fail: IO.fail<TwinVerdict>)).RetryWhile(...)` -> `IO.lift(() => Attempt(signal, weight)).RetryWhile(...)`.
  - LanguageExt `IO.lift(Func<Fin<A>>)` already produces `IO<A>` and lifts `Fin.Fail` onto the IO error channel, so the nested `Fin.Match` is invalid and redundant.

- `libs/dotnet/Rasm.Compute/.planning/Model/sessions.md:479`
  - from `IO.lift(Drain).Bind(drained => drained.Match(Succ: _ => IO.pure(unit), Fail: IO.fail<Unit>))` -> `IO.lift(Drain).Map(_ => unit)`.
  - `Drain` returns `Fin<int>`; LanguageExt's result-typed lift already propagates its failure as IO failure, leaving only the successful value-to-`Unit` projection.

- `libs/dotnet/Rasm.Compute/.planning/Model/generative.md:744`
  - from `IO.lift(() => { ... return GenerativeChat.Sweep(...).Bind(...).Map(_ => unit); }).Bind(outcome => outcome.Match(...))` -> retain the `IO.lift` expression and delete its trailing `Bind`/`Fin.Match`.
  - LanguageExt `IO.lift(Func<Fin<Unit>>)` already returns `IO<Unit>` with the `Fin` failure on the IO error channel.

- `libs/dotnet/Rasm.Compute/.planning/Runtime/scheduling.md:180`
  - from `IO.lift(() => gate.Value.Switch(... /* Fin<WorkItem> */)).Bind(admitted => admitted.Match(Succ: IO.pure, Fail: IO.fail<WorkItem>))` -> `IO.lift(() => gate.Value.Switch(...))`.
  - LanguageExt's result-typed lift already converts the returned `Fin<WorkItem>` into `IO<WorkItem>` and preserves its failure.

- `libs/dotnet/Rasm.Compute/.planning/Runtime/archive.md:129`
  - from `IO.lift(() => Open(source, policy)).Bind(opened => opened.Match(...))` inside `IO<Fin<A>> Session(...)` -> `IO.lift<Fin<HdfHandle>>(() => Open(source, policy)).Bind(opened => opened.Match(...))`.
  - The explicit generic selects LanguageExt's ordinary lazy lift and retains the nested `Fin<HdfHandle>` consumed by `Match`; the result-typed overload instead flattens it to `IO<HdfHandle>`.

- `libs/dotnet/Rasm.Compute/.planning/Tensor/blas.md:676`
  - from bracket `IO.lift(() => pool.Get(...))` / `IO.lift(() => staged.Bind(...))` -> `IO.lift<Fin<RecyclableMemoryStream>>(() => pool.Get(...))` / `IO.lift<Fin<A>>(() => staged.Bind(...))`.
  - Explicit ordinary lifts preserve laziness and the nested `Fin` resource/result consumed by `release`, `use`, and declared `IO<Fin<A>>`; result-typed lifts flatten both layers.
