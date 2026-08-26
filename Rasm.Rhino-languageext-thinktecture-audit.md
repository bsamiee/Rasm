# Rasm.Rhino LanguageExt / Thinktecture Audit

## `libs/dotnet/Rasm.Rhino/.planning/Exchange/publish.md:335`
From: `Option.Map(f).Sequence().As()` at lines 335-340 and 367.
To: `Option.Traverse(f).As()` at each site.
Why: LanguageExt documents `Map(...).Sequence()` as leaving an abstract inner; fused `Traverse` preserves the same optional effect and lands `Fin<Option<T>>` directly.

## `libs/dotnet/Rasm.Rhino/.planning/HostUi/panels.md:1502`
From: `row.Colour.Map(colour => colour.ToEto()).Sequence().Bind(...)`.
To: `row.Colour.Traverse(colour => colour.ToEto()).As().Bind(...)`.
Why: `Option.Traverse` is the existing conditional-effect inversion; it removes the invalid abstract `Sequence` result without changing the `None` behavior.

## `libs/dotnet/Rasm.Rhino/.planning/Objects/authoring.md:192`
From: `EgressFaults.Swap(static _ => Seq<Error>())` as the value returned by `Drain`.
To: `Cell.Take(EgressFaults).Current`.
Why: LanguageExt `Atom.Swap` returns the new empty sequence; kernel `Cell.Take` already returns the drained prior value and deletes the broken hand-drain.

## `libs/dotnet/Rasm.Rhino/.planning/Objects/authoring.md:218`
From: `toSeq(Sinks.Value).Map(static row => row.Value)`.
To: `Sinks.Value.Values.ToSeq()`.
Why: `HashMap<K,V>` has two enumerable projections, so `toSeq(map)` is ambiguous; LanguageExt's `Values` is the existing value-only carrier.

## `libs/dotnet/Rasm.Rhino/.planning/HostUi/shell.md:1994`
From: `toSeq(names.Value).Map(static row => (Name: row.Key, Plugin: row.Value.Plugin))`.
To: `names.Value.AsIterable().ToSeq().Map(static row => (Name: row.Key, Plugin: row.Value.Plugin))`.
Why: LanguageExt requires `HashMap.AsIterable()` for keyed iteration; direct `toSeq(HashMap)` cannot infer which `IEnumerable` projection is intended.

## `libs/dotnet/Rasm.Rhino/.planning/Document/events.md:1530`
From: `toSeq(Seats.Value).Choose(...)`.
To: `Seats.Value.AsIterable().ToSeq().Choose(...)`.
Why: `AsIterable` is LanguageExt's named `(Key, Value)` projection; it preserves the census logic and removes ambiguous `HashMap` enumeration.

## `libs/dotnet/Rasm.Rhino/.planning/Display/interaction.md:829`
From: `Atom<HashMap<WidgetId, WidgetMount>> mounted` plus whole-map `Value`/`Swap` calls at lines 889, 978, 981, and 999.
To: `AtomHashMap(HashMap<WidgetId, WidgetMount>())` plus `Add`, `Find`, `Remove`, and `AsIterable().ToSeq()`.
Why: LanguageExt `AtomHashMap` already owns per-key CAS mutation and keyed snapshot traversal; no caller needs a whole-map transition verdict.

## `libs/dotnet/Rasm.Rhino/.planning/Persistence/settings.md:234`
From: `map.Map(static row => KeyValuePair.Create(row.Key, row.Value))` on `HashMap<string,string>`.
To: `map.AsIterable().ToSeq().Map(static row => KeyValuePair.Create(row.Key, row.Value))`.
Why: `HashMap.Map` maps values only; `AsIterable` is the package-owned keyed-pair surface and makes the existing sort projection truthful.

## `libs/dotnet/Rasm.Rhino/.planning/Commands/options.md:546`
From: the success/failure `Match` at lines 564-568 that manually calls `lease.Release` and combines errors.
To: append `.Map(_ => lease).Rollback(release: () => lease.Release(op), key: op)` to the existing bind pipeline.
Why: kernel `Fin.Rollback` already runs failure-only cleanup and combines the primary and cleanup errors; the hand fold duplicates Domain custody.

## `libs/dotnet/Rasm.Rhino/.planning/Objects/history.md:544`
From: `mutation.BindFail(primary => restore.Match(... primary + cleanup ...))`.
To: `mutation.Rollback(release: () => { data.UpdateResultArray(prior); return Fin.Succ(unit); }, key: op)`.
Why: `Rollback` is the existing failure-only compensation owner and preserves both the original failure and any restore failure with less branching.

## `libs/dotnet/Rasm.Rhino/.planning/Objects/state.md:345`
From: `primary.BindFail(failure => Restore(...).Match(...))`.
To: `primary.Rollback(release: () => Restore(states: states, key: key), key: key)`.
Why: kernel custody already implements this exact failure-only restore posture and error aggregation.

## `libs/dotnet/Rasm.Rhino/.planning/Document/session.md:1119`
From: `admitted.BindFail(error => op.Catch(dispose).BiBind(...))`.
To: `admitted.Rollback(release: () => { acquired.Dispose(); return Fin.Succ(unit); }, key: op)`.
Why: `Rollback` preserves admission failure, appends disposal failure, and deletes the manually duplicated two-branch carrier fold.

## `libs/dotnet/Rasm.Rhino/.planning/Display/modes.md:1047`
From: copied-mode `Commit(...).BindFail(failure => DeleteCase.Apply(...).Match(...))`.
To: `Commit(...).Rollback(release: () => new ModeOp.DeleteCase(...).Apply(held.Op).Map(static _ => unit), key: held.Op)`.
Why: the copied mode is acquired state; Domain `Rollback` owns failure-only compensation and identical failure combination.

## `libs/dotnet/Rasm.Rhino/.planning/Display/modes.md:1089`
From: the configured-mode body followed by `BindFail(failure => Restore(...).Match(...))`.
To: apply `.Rollback(release: () => Restore(mode, prior, band, key), key: key)` to that body.
Why: the existing Domain custody operator exactly preserves the primary/restore semantics and removes the repeated recovery fold.

## `libs/dotnet/Rasm.Rhino/.planning/Display/interaction.md:912`
From: `SetPosture(...).Map(...).BindFail(primary => SetPosture(prior).Match(...))`.
To: `SetPosture(...).Map(...).Rollback(release: () => SetPosture(value, prior.Posture, op), key: op)`.
Why: this is failure-only compensation already owned by `Fin.Rollback`; no widget-specific recovery algebra remains.

## `libs/dotnet/Rasm.Rhino/.planning/Display/conduit.md:807`
From: retained-mark mutation followed by `BindFail(failure => Restore(prior, op).Match(...))`.
To: apply `.Rollback(release: () => ctx.Self.Restore(prior, ctx.Op), key: ctx.Op)` to the mutation.
Why: `Rollback` preserves the same restore boundary and aggregates cleanup errors without the duplicated `Match` shell.

## `libs/dotnet/Rasm.Rhino/.planning/Viewport/camera.md:457`
From: `Apply(...).BindFail(primary => Restore(...).Match(...))`.
To: `Apply(...).Rollback(release: () => Restore(target, prior, op), key: op)`.
Why: Domain custody already owns failure-only restoration; the camera-local fold adds no behavior.

## `libs/dotnet/Rasm.Rhino/.planning/Render/settings.md:1268`
From: `apply(...).BindFail(fault => record.Apply(...).Match(...))`.
To: `apply(...).Rollback(release: () => record.Apply(owners: owners, key: op), key: op)`.
Why: the prior render state is rollback custody, and kernel `Rollback` preserves both failures without a local carrier reconstruction.

## `libs/dotnet/Rasm.Rhino/.planning/Viewport/motion.md:403`
From: `Append(Append(primary, pause), release)` plus the local `Append` function at lines 421-425.
To: `primary.Settled(release: () => Custody.Release(Seq<Func<Fin<Unit>>>(() => pause, () => release), key), key)`; delete `Append`.
Why: the direct delegate-roster `Custody.Release` overload walks in input order, while `Settled` preserves primary-then-cleanup error order.

## `libs/dotnet/Rasm.Rhino/.planning/Commands/command.md:280`
From: private `AdmitKey`, which calls generated `StageKey.Validate`, reconstructs `Fin`, and is called at lines 212, 289, and 291.
To: call `op.AcceptValidated<StageKey>(candidate: entry.ToValue())`, the same over `row.Key`, and the same inside the successors traversal; delete `AdmitKey`.
Why: Domain's Thinktecture bridge already maps generated `ValidationError` onto the canonical `Error`; the helper is a forwarding shell.

## `libs/dotnet/Rasm.Rhino/.planning/Objects/history.md:530`
From: `Enumerable.Range(...).AsIterable().ToSeq()` and the private `Range(int)` wrapper at lines 552-553.
To: `toSeq(Enumerable.Range(...))` at all three call sites; delete `Range(int)`.
Why: `Prelude.toSeq(IEnumerable<T>)` is the direct LanguageExt admission, so both the carrier round-trip and module-level wrapper are unnecessary.

## `libs/dotnet/Rasm.Rhino/.planning/Commands/acquisition.md:1041`
From: optional `promptDefault` and `@default` effects expressed as paired `Match(Some: ..., None: Fin.Succ(unit))` blocks.
To: use `.TraverseM(effect).As().Map(static _ => unit)` for each optional effect.
Why: LanguageExt `Option.TraverseM` is the existing absence-total conditional-effect fold and removes both hand-written `None` arms.

## `libs/dotnet/Rasm.Rhino/.planning/Document/layers.md:67`
From: `.Head.IfNoneUnsafe(default(ValidationError))`.
To: `.Head.IfNone(default(ValidationError))`.
Why: LanguageExt v5 removed `IfNoneUnsafe`; `Option.IfNone(A)` is the exact value-fallback member and preserves the default-null validation result.

## `libs/dotnet/Rasm.Rhino/.planning/Persistence/settings.md:224`
From: `SettingKind.Items.Find(...)` at lines 224 and 229.
To: `toSeq(SettingKind.Items).Find(...)` at both sites.
Why: Thinktecture generates `Items` as `IReadOnlyList<SettingKind>`, which has no `Find`; LanguageExt `Seq.Find` preserves the existing `Option`-to-`Fin` path.

## `libs/dotnet/Rasm.Rhino/.planning/Commands/acquisition.md:901`
From: generated `Switch(...) && this switch { Transform ... => true, _ => true }`, including the tautological transform predicate at lines 905-906.
To: the generated `Switch` alone, with `transform: static (_, _) => true`.
Why: every arm of the raw switch and both sides of the transform disjunction are true; Thinktecture's generated exhaustive fold already returns the identical verdict.

## `libs/dotnet/Rasm.Rhino/.planning/Commands/acquisition.md:1309`
From: `toSeq(Enumerable.Range(...)).foldUntil(seed, folder, statePredicate)`.
To: `Prelude.Range(...).FoldUntil(seed, folder, pair => pair.State.Match(...))`.
Why: LanguageExt `Range` is already `Foldable`, and the catalog exposes only `FoldUntil` with a `(State, Value)` predicate; the lowercase member does not exist.

## `libs/dotnet/Rasm.Rhino/.planning/Objects/attributes.md:671`
From: the `AttributeChange.Validate` null tests and manual `Succ`/`Fail` reconstruction.
To: `Op.Of(nameof(AttributeProgram)).AcceptValidated<AttributeChange>(AttributeChange.Validate(Apply, out AttributeChange? admitted), admitted)`.
Why: Domain's Thinktecture outcome-lifter overload already preserves generated validation detail instead of replacing it with a generic invalid-input fault.

## `libs/dotnet/Rasm.Rhino/.planning/Objects/history.md:601`
From: the `ReplayHook.Validate` null tests and manual `Succ`/`Fail` reconstruction.
To: `Op.Of(nameof(ReplayProgram)).AcceptValidated<ReplayHook>(ReplayHook.Validate(Delegate, out ReplayHook? admitted), admitted)`.
Why: Domain's existing Thinktecture outcome lifter handles multi-member generated validation and retains its actual refusal.

## `libs/dotnet/Rasm.Rhino/.planning/Document/commit.md:65`
From: `Append(primary: outcome, side: restored)` and private `Append<T>` at lines 71-75.
To: `outcome.Settled(release: () => restored, key: key)`; delete `Append<T>`.
Why: Domain `Fin.Settled` is the existing both-arm cleanup combiner and preserves the already-captured redraw restoration result and error order.

## `libs/dotnet/Rasm.Rhino/.planning/Persistence/snapshots.md:168`
From: local `Settled(body, finalizers)` and its manual fold at lines 184-189.
To: `body.Settled(release: () => Custody.Release(finalizers, op), key: op)`; delete local `Settled<T>`.
Why: the direct delegate-roster `Custody.Release` attempts finalizers in input order, and Domain `Settled` preserves identical both-arm aggregation.

## `libs/dotnet/Rasm.Rhino/.planning/Display/interaction.md:445`
From: both nested `BiBind`/`Match` cleanup ladders at lines 445-463.
To: apply `.Rollback(release: () => { ball.Dispose(); return Fin.Succ(unit); }, key: op)` to conduit acquisition, and `.Rollback(release: () => rig.Release(op), key: op)` to the mount pipeline.
Why: Domain `Rollback` owns both failure-only custody edges; placing the first before the downstream bind preserves the current no-double-dispose ownership transfer.

## `libs/dotnet/Rasm.Rhino/.planning/Display/conduit.md:553`
From: the conduit mount pipeline followed by `BiBind` and manual `adapter.Release` error combination at lines 557-561.
To: append `.Rollback(release: adapter.Release, key: op)` to that pipeline.
Why: Domain `Rollback` performs the same failure-only release and primary-plus-cleanup aggregation without the carrier reconstruction.

## `libs/dotnet/Rasm.Rhino/.planning/Objects/history.md:784`
From: precomputed `primary`/`cleanup` followed by the nested `Match` restoration fold.
To: `op.Catch(body).Settled(release: () => Write(value: prior, op: op), key: op)`.
Why: the prior signal must restore on both arms; Domain `Settled` owns that exact cleanup posture and aggregation.

## `libs/dotnet/Rasm.Rhino/.planning/Objects/authoring.md:730`
From: the grip-add pipeline followed by `.MapFail(primary => dispose.Match(...))` at lines 735-740.
To: append `.Rollback(release: () => { Dispose(); return Fin.Succ(unit); }, key: op)` to the pipeline.
Why: Domain `Rollback` preserves failure-only disposal and combined cleanup failure without manually rewriting `Error`.

## `libs/dotnet/Rasm.Rhino/.planning/Display/conduit.md:817`
From: `tally.Refused.Fold(Errors.None, static (folded, cause) => folded + cause)`.
To: `Error.Many(tally.Refused)`.
Why: LanguageExt's existing accumulated-error constructor consumes the same refusal roster; this branch is reached only when the tally is invalid.

## `libs/dotnet/Rasm.Rhino/.planning/Exchange/formats.md:573`
From: `Atom<HashMap<Guid, Error>> Refusals` plus `Value.Find` and whole-map `Swap` calls at lines 575, 600, and 619.
To: `AtomHashMap(HashMap<Guid, Error>())` plus `Find`, `Remove`, and `AddOrUpdate`.
Why: LanguageExt `AtomHashMap` is the existing key-grain concurrent store, and none of these callers needs a whole-map transition result.

## `libs/dotnet/Rasm.Rhino/.planning/Exchange/formats.md:545`
From: `tune.Dial.Match(Some: dial => dial.Admit(...), None: () => Fin.Succ(unit))`.
To: `tune.Dial.TraverseM(dial => dial.Admit(...)).As().Map(static _ => unit)`.
Why: LanguageExt `Option.TraverseM` is the package-owned absence-total conditional effect and preserves `None` as successful no-op.

## `libs/dotnet/Rasm.Rhino/.planning/HostUi/shell.md:867`
From: `Styler.Match(Some: dress => dress.Dress(...), None: () => Fin.Succ(unit))`.
To: `Styler.TraverseM(dress => dress.Dress(window, key)).As().Map(static _ => unit)`.
Why: LanguageExt `Option.TraverseM` removes the duplicated `None` success arm without changing conditional styling behavior.

## `libs/dotnet/Rasm.Rhino/.planning/Blocks/lifecycle.md:369`
From: `discharged.Match(Some: held => Closed(...), None: () => Fin.Succ(unit))`.
To: `discharged.TraverseM(held => Closed(watch: held.Observation, op: op)).As().Map(static _ => unit)`.
Why: LanguageExt already owns optional effect traversal; absence remains a successful no-op and the close failure still short-circuits.

## `libs/dotnet/Rasm.Rhino/.planning/Plugin/licensing.md:374`
From: `answer.Match(Some: raster => Badge(raster, op).Map(Some), None: () => Fin.Succ(Option<GdiIcon>.None))`.
To: `answer.TraverseM(raster => Badge(raster, op)).As()`.
Why: LanguageExt `Option.TraverseM` directly inverts `Option<AssetRaster>` through `Fin` to the same `Fin<Option<GdiIcon>>`.

## `libs/dotnet/Rasm.Rhino/.planning/Commands/command.md:254`
From: `Range(...).AsIterable().ToSeq().foldUntil(seed, folder, statePredicate)`.
To: `Range(...).FoldUntil(seed, folder, pair => pair.State.Match(...))`.
Why: `Prelude.Range` is already `Foldable`, and LanguageExt exposes only the capitalized pure fold with a `(State, Value)` predicate.

## `libs/dotnet/Rasm.Rhino/.planning/Objects/state.md:737`
From: private `Part`, which switches over generated `PartIndex.Validate` and even matches its default `ValidationError` as `DraftFault`.
To: `key.AcceptValidated<PartIndex, ComponentIndex>(candidate: component)`; delete `Part`.
Why: Domain's Thinktecture bridge already invokes generated admission and maps its actual `ValidationError`; the current failure arm cannot match that type.

## `libs/dotnet/Rasm.Rhino/.planning/Document/lifetime.md:140`
From: `trouble.Fold(Errors.None, static (folded, failure) => folded + failure)` after the non-empty guard.
To: `Error.Many(trouble)`.
Why: LanguageExt owns accumulated-error construction, and this branch has already proved the sequence non-empty.

## `libs/dotnet/Rasm.Rhino/.planning/Exchange/publish.md:1095`
From: `Restored(body, restore)` and the private two-arm combiner at lines 1108-1113.
To: `body().Settled(release: restore, key: op)`; delete `Restored<T>`.
Why: Domain `Fin.Settled` already runs cleanup on both arms and appends restoration failure after the primary in the same order.

## `libs/dotnet/Rasm.Rhino/.planning/Display/render.md:1415`
From: `Atom<HashMap<Guid, LightAuthorityHost>> Hosts` with whole-map `Swap`/`Value.Find` at lines 1421, 1434, and 1441.
To: `AtomHashMap(HashMap<Guid, LightAuthorityHost>())` with `AddOrUpdate`, `Remove`, and `Find`.
Why: LanguageExt's keyed cell owns exactly these independent key mutations; no caller consumes a whole-map transition verdict.

## `libs/dotnet/Rasm.Rhino/.planning/Display/render.md:339`
From: `FramebufferRow.ViewportIntent` and `CaptureIntent`, hand-built as `HostRow<bool>` rows at lines 346-347.
To: delete both rows.
Why: the same page's generated `RenderIntent.Viewport`/`Capture` is the existing bool-keyed owner already used by the render pipeline; these duplicates have no consumer.

## `libs/dotnet/Rasm.Rhino/.planning/HostUi/panels.md:1214`
From: `.MapFail(fault => HostThread.Release(...).Match(Succ: _ => fault, Fail: cleanup => fault + cleanup))`.
To: `.Rollback(release: () => HostThread.Release(...), key: op)`.
Why: Domain `Rollback` is the existing failure-only release and error-aggregation owner; the local `MapFail` recreates it exactly.

## `libs/dotnet/Rasm.Rhino/.planning/Modeling/solids.md:219`
From: `BindFail` plus `Optional(detached).Match` to conditionally dispose the copied geometry.
To: `.Rollback(release: () => Custody.Dispose(held: Optional(detached).ToSeq(), key: key), key: key)`.
Why: `Option.ToSeq` already turns absence into the empty release roster, and Domain rollback preserves the same failure-only custody without two arms.

## `libs/dotnet/Rasm.Rhino/.planning/Annotation/typeface.md:697`
From: `.BindFail(primary => Fin.Fail<Unit>(primary).Rollback(release, key))`.
To: apply `.Rollback(release, key)` directly to `def.Apply(...)`.
Why: `Rollback` already preserves a success and acts only on failure, so rebuilding the identical failed `Fin` inside `BindFail` is redundant.

## `libs/dotnet/Rasm.Rhino/.planning/Annotation/linetype.md:369`
From: `.BindFail(primary => Fin.Fail<Unit>(primary).Rollback(release, key))`.
To: apply `.Rollback(release, key)` directly to `def.Apply(...)`.
Why: Domain rollback receives the same primary error and performs the same failure-only disposal without the carrier reconstruction.

## `libs/dotnet/Rasm.Rhino/.planning/Annotation/text.md:694`
From: `.BindFail(primary => Fin.Fail<OutlineProduct>(primary).Rollback(release, key))`.
To: apply `.Rollback(release, key)` directly to the outline pipeline.
Why: `Rollback` is already failure-selective and preserves `OutlineProduct` on success; the outer `BindFail` adds no behavior.

## `libs/dotnet/Rasm.Rhino/.planning/Annotation/hatch.md:114`
From: `BindFail(primary => Fin.Fail<T>(primary).Rollback(...))` at lines 114, 399, 415, 494, 497, 593, and 632.
To: apply each existing `.Rollback(...)` directly to its preceding `Fin<T>`.
Why: Domain rollback already leaves success untouched and combines cleanup on failure; every outer bind only reconstructs the same failed carrier.

## `libs/dotnet/Rasm.Rhino/.planning/Annotation/style.md:169`
From: `BindFail(primary => Fin.Fail<T>(primary).Rollback(...))` at lines 169, 172, 239, 246, 695, 698, and 799.
To: apply each existing `.Rollback(...)` directly to its preceding `Fin<T>`.
Why: Domain rollback owns the exact failure-only compensation semantics, so the repeated `BindFail`/`Fin.Fail` shells are behaviorless.

## `libs/dotnet/Rasm.Rhino/.planning/Annotation/typeface.md:862`
From: private `Rollback`/`Reverted` folds joined by private `Merge` at lines 862-874.
To: inline `Custody.Release(landed, row => row.Undo(op), op)` and `Fin.Fail<T>(primary).Rollback(runs, run => Custody.Release(run, row => row.Undo(op), op), op)`; delete all three helpers.
Why: Domain custody already releases in reverse, attempts every entry, and preserves all cleanup errors; the local result algebra has no additional policy.

## `libs/dotnet/Rasm.Rhino/.planning/Display/modes.md:355`
From: `toSeq(DisplayAxis.Items).GroupBy(...).Fold(...)`.
To: `toSeq(DisplayAxis.Items.GroupBy(...)).Fold(...)`.
Why: LINQ `GroupBy` exits `Seq`; LanguageExt exposes no `Fold` on that `IEnumerable`, so `Prelude.toSeq` is the required carrier re-entry.

## `libs/dotnet/Rasm.Rhino/.planning/Display/conduit.md:376`
From: both `held.Steps.Choose(...).GroupBy(...).Fold(...)` pipelines at lines 376-383.
To: wrap each complete `...GroupBy(...)` result in `toSeq(...)` before `Fold`.
Why: `GroupBy` returns `IEnumerable<IGrouping<...>>`; LanguageExt `Fold` binds only after the documented `Prelude.toSeq` re-entry.

## `libs/dotnet/Rasm.Rhino/.planning/Blocks/graph.md:474`
From: the ranked `Seq` pipeline ending `.OrderBy(...).ToSeq().Strict()` at lines 474-478.
To: wrap the complete `.OrderBy(...)` result in `toSeq(...)`, then call `.Strict()`.
Why: LINQ `OrderBy` returns `IOrderedEnumerable<T>` and LanguageExt `ToSeq()` has no `IEnumerable` receiver overload; `Prelude.toSeq` is the exact landing.

## `libs/dotnet/Rasm.Rhino/.planning/HostUi/dialogs.md:429`
From: `seeds.HeadOrNone().IfNone(-1)`.
To: `seeds.Head.IfNone(-1)`.
Why: LanguageExt v5 removed `HeadOrNone` from `Seq`; `Head` is the existing `Option<T>` property and preserves the fallback.

## `libs/dotnet/Rasm.Rhino/.planning/Persistence/snapshots.md:651`
From: `toSeq(Range(0, archiveArray.Count)).TraverseM(...)`.
To: `toSeq(Enumerable.Range(0, archiveArray.Count)).TraverseM(...)`.
Why: unqualified `Range` resolves to LanguageExt's `Foldable`, which `Prelude.toSeq` does not accept; LINQ's existing range preserves the same indices.

## `libs/dotnet/Rasm.Rhino/.planning/Persistence/usertext.md:589`
From: `toSeq(Range(0, document.Strings.Count)).Traverse(...)`.
To: `toSeq(Enumerable.Range(0, document.Strings.Count)).Traverse(...)`.
Why: `Prelude.toSeq` accepts `IEnumerable<T>`, not LanguageExt `Range<T>`; `Enumerable.Range` yields the identical zero-based roster.

## `libs/dotnet/Rasm.Rhino/.planning/Annotation/linetype.md:243`
From: `toSeq(Range(from: 0, count: linetype.SegmentCount))` here and at line 533.
To: `toSeq(Enumerable.Range(start: 0, count: linetype.SegmentCount))` at both sites.
Why: LanguageExt's `Range` is already a `Foldable` and cannot enter `toSeq`; LINQ's range is the existing `IEnumerable<int>` source these traversals require.

## `libs/dotnet/Rasm.Rhino/.planning/Exchange/publish.md:637`
From: `toSeq(Range(1, source.Count.Value)).Map(...)`.
To: `toSeq(Enumerable.Range(1, source.Count.Value)).Map(...)`.
Why: the LanguageExt range receiver does not satisfy `Prelude.toSeq(IEnumerable<T>)`; LINQ preserves the same one-based ordinal projection.

## `libs/dotnet/Rasm.Rhino/.planning/Exchange/operations.md:297`
From: `toSeq(Range(1, bound.Value))` here and `toSeq(Range(0, requested))` at line 423.
To: `toSeq(Enumerable.Range(1, bound.Value))` and `toSeq(Enumerable.Range(0, requested))`.
Why: `Prelude.Range` is a `Foldable`, not an `IEnumerable`; `Enumerable.Range` is the package-compatible source with identical start/count semantics.

## `libs/dotnet/Rasm.Rhino/.planning/Display/render.md:477`
From: `BindFail(failure => (reset, opened.Close(), Fin.Fail<Unit>(failure)).Item3)`.
To: `MapFail(failure => (reset, opened.Close(), failure).Item3)`.
Why: the side effects return no fallible cleanup result; LanguageExt `MapFail` preserves the same failure without reconstructing an identical failed carrier.

## `libs/dotnet/Rasm.Rhino/.planning/HostUi/shell.md:2311`
From: `BindFail(failure => (faults.Park(failure), Fin.Fail<Unit>(failure)).Item2)` here and the equivalent teardown fold at line 2492.
To: `MapFail(failure => (faults.Park(failure), failure).Item2)` at both sites.
Why: both arms only tap the existing error; LanguageExt `MapFail` keeps the failure and deletes the behaviorless `Fin.Fail` round trip.

## `libs/dotnet/Rasm.Rhino/.planning/Objects/materials.md:467`
From: `Optional(policy).Match(Some: value => ...Capture(...).Map(Some), None: () => Fin.Succ(None))`.
To: `Optional(policy).TraverseM(value => new Lease<MeshingParameters>.Owned(value).Use(held => RenderMeshPolicy.Capture(held, key), key)).As()`.
Why: `Option.TraverseM` already preserves absence and returns `Fin<Option<RenderMeshPolicy>>`; the explicit carrier reconstruction is duplicate logic.

## `libs/dotnet/Rasm.Rhino/.planning/Plugin/licensing.md:471`
From: optional status `Match`, manually wrapping `Some(state)` or returning `Fin.Succ(Row(None))`.
To: `Optional(...).TraverseM(status => State(status, held)).As().Map<LicenseVerdict>(state => new LicenseVerdict.Row(state))`.
Why: LanguageExt's optional monadic traversal produces the same `Fin<Option<LicenseState>>` before the one shared row construction.

## `libs/dotnet/Rasm.Rhino/.planning/Render/settings.md:1382`
From: `Optional(args.Document).Match(Some: document => DocKey.Of(...).Map(...), None: () => Fin.Succ(contextual))`.
To: `Optional(args.Document).TraverseM(document => DocKey.Of(document: document, key: op)).As().Map(key => contextual with { Key = key })`.
Why: `Option.TraverseM` carries `None` as a successful empty key and removes the duplicated `AmbientFact` construction.

## `libs/dotnet/Rasm.Rhino/.planning/Persistence/snapshots.md:636`
From: `writer.Match(Some: archive => spec.Codec.Write(...), None: () => Fin.Succ(unit))`.
To: `writer.TraverseM(archive => spec.Codec.Write(archive, state.Payload, key)).As().Map(static _ => unit)`.
Why: LanguageExt owns absence-total conditional effects through `Option.TraverseM`; the native write failure and no-writer success remain unchanged.

## `libs/dotnet/Rasm.Rhino/.planning/Commands/acquisition.md:1268`
From: `request.Default.Match(Some: value => value.Apply(...), None: () => Fin.Succ(unit))`.
To: `request.Default.TraverseM(value => value.Apply(getter, op)).As().Map(static _ => unit)`.
Why: `Option.TraverseM` is the existing conditional-effect fold and deletes the explicit no-op arm without changing sequencing.

## `libs/dotnet/Rasm.Rhino/.planning/Commands/acquisition.md:1332`
From: `dragging.Match(Some: buffer => buffer.Census(op).Map(Some), None: () => Fin.Succ(None))`.
To: `dragging.TraverseM(buffer => buffer.Census(op)).As()`.
Why: LanguageExt directly inverts `Option<DragBuffer>` through `Fin` to the same `Fin<Option<DragCensus>>`.

## `libs/dotnet/Rasm.Rhino/.planning/Exchange/archive.md:936`
From: `Optional(held).Match(Some: live => EarthAnchor.Located(...), None: () => Fin.Succ(None))`.
To: `Optional(held).TraverseM(live => EarthAnchor.Located(anchor: live, op: op)).As()`.
Why: LanguageExt `Option.TraverseM` preserves the absent-anchor success and the located-anchor failure while removing both manual carrier arms.

## `libs/dotnet/Rasm.Rhino/.planning/Objects/state.md:902`
From: `path.Match(Some: value => OpenArchive(value, op), None: () => Fin.Succ(None))`.
To: `path.TraverseM(value => OpenArchive(value, op)).As()`.
Why: `Option.TraverseM` yields the existing `Fin<Option<ArchiveExtent>>` exactly; the explicit `Match` duplicates that inversion.

## `libs/dotnet/Rasm.Rhino/.planning/Viewport/operations.md:702`
From: `row.Detail.Match(Some: detail => Details.Commits ? CommitViewportChanges() : Fin.Succ(unit), None: () => Fin.Succ(unit))`.
To: `row.Detail.TraverseM(detail => Details.Commits ? key.Catch(() => key.Confirm(detail.CommitViewportChanges())) : Fin.Succ(unit)).As().Map(static _ => unit)`.
Why: LanguageExt's optional monadic traversal preserves the conditional commit failure and both successful no-op paths without duplicate arms.

## `libs/dotnet/Rasm.Rhino/.planning/Viewport/operations.md:771`
From: `PinnedActive.Match(Some: id => guard(...).ToFin(), None: () => Fin.Succ(unit))`.
To: `PinnedActive.TraverseM(id => guard(Optional(document.Views.ActiveView).Map(view => view.ActiveViewport.Id == id).IfNone(false), key.InvalidContext()).ToFin()).As().Map(static _ => unit)`.
Why: `Option.TraverseM` is the existing absence-total guard; it retains the pinned-ID refusal while deleting the manual absent success.

## `libs/dotnet/Rasm.Rhino/.planning/Persistence/settings.md:684`
From: `changed.CompareWith.Match(Some: path => Admit(path, op).Map(Some), None: () => Fin.Succ(None))`.
To: `changed.CompareWith.TraverseM(path => Admit(path, op)).As()` before the existing `Bind(compare => At(...))`.
Why: LanguageExt directly inverts the optional admission to the same `Fin<Option<SettingPath>>`; the explicit carrier construction is redundant.

## `libs/dotnet/Rasm.Rhino/.planning/Persistence/settings.md:882`
From: `existing.Match(Some: found => guard(kind.Accepts(found, value), op.InvalidInput()).ToFin(), None: () => Fin.Succ(unit))`.
To: `existing.TraverseM(found => guard(kind.Accepts(found, value), op.InvalidInput()).ToFin()).As().Map(static _ => unit)`.
Why: LanguageExt's optional effect traversal keeps absence compatible and preserves the present-type guard without a hand-written no-op arm.

## `libs/dotnet/Rasm.Rhino/.planning/Modeling/solids.md:255`
From: `success.Match(Some: verdict => op.Confirm(verdict), None: () => Fin.Succ(unit))` before the existing rollback.
To: `success.TraverseM(verdict => op.Confirm(success: verdict)).As().Map(static _ => unit)` before that rollback.
Why: `Option.TraverseM` preserves the optional confirmation failure and absent success while leaving the custody boundary unchanged.

## `libs/dotnet/Rasm.Rhino/.planning/Display/render.md:2939`
From: `policy.Residency.Match(Some: pack => Encode.Apply(...).Map(Some), None: () => Fin.Succ(None))`.
To: `policy.Residency.TraverseM(pack => Encode.Apply(new PackOp.MeshPatch(Source: space, Policy: pack), key)).As()`.
Why: LanguageExt already inverts the optional encoding effect to the same `Fin<Option<EncodedGeometry>>`; both manual wrapping arms disappear.

## `libs/dotnet/Rasm.Rhino/.planning/Display/render.md:506`
From: `program.Stopped.Match(Some: hook => key.Catch(hook), None: () => Fin.Succ(unit))` in the release roster.
To: `program.Stopped.TraverseM(hook => key.Catch(hook)).As().Map(static _ => unit)`.
Why: LanguageExt's optional traversal is the same absent-success/present-hook release step and preserves its position in `Custody.Release`.

## `libs/dotnet/Rasm.Rhino/.planning/Display/render.md:693`
From: `decide.Match(Some: predicate => WithWindow(...), None: () => Fin.Succ(unit))`.
To: `decide.TraverseM(predicate => WithWindow(...)).As().Map(static _ => unit)`.
Why: `Option.TraverseM` retains the armed-window failure and the no-predicate success while deleting the duplicate effect arms.

## `libs/dotnet/Rasm.Rhino/.planning/Display/render.md:1301`
From: `bound.Bind(...Viewed).Match(Some: seat => key.Catch(() => seat(frame)), None: () => Fin.Succ(unit))`.
To: `bound.Bind(...Viewed).TraverseM(seat => key.Catch(() => seat(frame))).As().Map(static _ => unit)`.
Why: LanguageExt already owns the optional callback effect; the frame swap and callback failure behavior remain unchanged.

## `libs/dotnet/Rasm.Rhino/.planning/Display/render.md:1391`
From: `prior.Held.Match(Some: held => Custody.Release(...), None: () => Fin.Succ(unit))`.
To: `prior.Held.TraverseM(held => Custody.Release(...)).As().Map(static _ => unit)`.
Why: `Option.TraverseM` preserves the existing two-step release when held and the successful empty release when absent.

## `libs/dotnet/Rasm.Rhino/.planning/Objects/state.md:619`
From: `Attributes.Match(Some: value => key.Catch(() => Fin.Succ(Op.Side(value.Dispose))), None: () => Fin.Succ(unit))`.
To: `Attributes.TraverseM(value => key.Catch(() => Fin.Succ(Op.Side(value.Dispose)))).As().Map(static _ => unit)`.
Why: LanguageExt's optional effect fold leaves the surrounding reverse-order `Custody.Release` roster and disposal failure semantics intact.

## `libs/dotnet/Rasm.Rhino/.planning/Objects/authoring.md:838`
From: `factory(candidate).Match(Some: grips => Confirm(...).Rollback(grips), None: () => Fin.Succ(unit))`.
To: `factory(candidate).TraverseM(grips => key.Confirm(...).Rollback(grips)).As().Map(static _ => unit)`.
Why: LanguageExt preserves absence as a successful registration no-op and the present grip's existing rollback boundary.

## `libs/dotnet/Rasm.Rhino/.planning/Render/registry.md:1674`
From: `.Match(Some: deliver, None: () => Fin.Succ(unit))` on gated `Option<ContentFact>` here and at lines 1688, 1700, 1718, and 1738.
To: `.TraverseM(deliver).As().Map(static _ => unit)` at all five event adapters.
Why: `Option.TraverseM` is the existing conditional delivery fold; filtered events remain successful no-ops and delivery failures still surface.

## `libs/dotnet/Rasm.Rhino/.planning/Document/events.md:318`
From: `projected.Match(Some: fact => key.Catch(() => deliver(...)), None: () => Fin.Succ(unit))` inside the successful projection arm.
To: `projected.TraverseM(fact => key.Catch(() => deliver(fact.Key, fact.Payload))).As().Map(static _ => unit)`.
Why: LanguageExt removes the hand-written optional delivery split while the outer failure arm still records and returns projection errors.

## `libs/dotnet/Rasm.Rhino/.planning/Document/events.md:1375`
From: separate `Optional(timer).Match(...)` and `Optional(watcher).Match(...)` disposal delegates.
To: use `Optional(value).TraverseM(live => key.Catch(() => { live.Dispose(); return Fin.Succ(unit); })).As().Map(static _ => unit)` for each.
Why: LanguageExt owns the absent resource no-op; both delegates stay in the same `Custody.Release` roster and preserve disposal faults.

## `libs/dotnet/Rasm.Rhino/.planning/Document/session.md:1382`
From: `scaling.Match(Some: policy => AdjustLengthUnits(...), None: () => Fin.Succ(unit)).ToValidation()`.
To: `scaling.TraverseM(policy => AdjustLengthUnits(...)).As().Map(static _ => unit).ToValidation()`.
Why: `Option.TraverseM` preserves the optional restoration refusal before the existing applicative accumulation with tolerances and precision.

## `libs/dotnet/Rasm.Rhino/.planning/HostUi/pages.md:483`
From: `release.Match(Some: children => ReleaseTree(...), None: () => Fin.Succ(unit))` here and at line 519.
To: `release.TraverseM(children => ReleaseTree(children, key)).As().Map(static _ => unit)` at both custody exits.
Why: LanguageExt's optional traversal is the same empty-or-release fold and leaves each surrounding `Settled`/ownership boundary intact.

## `libs/dotnet/Rasm.Rhino/.planning/HostUi/shell.md:739`
From: reads of nonexistent `Cell.Take(...).State` here and at line 912, including the manual optional disposal fold.
To: use `.Current.TraverseM(mount => op.Catch(() => Fin.Succ(mount.Dispose()))).As().Map(static _ => unit)` here and `.Current.Iter(static row => row.Dispose())` at line 912.
Why: Domain `Transition<T>` exposes the post-state only as `Current`; `Option.TraverseM` preserves the drained mount's absent-success disposal behavior.

## `libs/dotnet/Rasm.Rhino/.planning/Document/layers.md:1220`
From: `currentKey.Match(Some: _ => projected.Choose(...).Head.ToFin(...).Map(Some), None: () => Fin.Succ(None))`.
To: `currentKey.TraverseM(_ => projected.Choose(static row => row.Current).Head.ToFin(op.InvalidResult())).As()`.
Why: `Option.TraverseM` produces the same `Fin<Option<EntityPath>>`; the projected-current refusal still occurs only when an input current key exists.

## `libs/dotnet/Rasm.Rhino/.planning/HostUi/shell.md:2359`
From: `outcome.Match(Some: settled => ...select Some(answered), None: () => Fin.Succ(None))`.
To: `outcome.TraverseM(settled => from spec in ... from answered in ... select answered).As()`.
Why: LanguageExt directly inverts the optional notice run to `Fin<Option<T>>`; the same presentation/body failures remain on the present path.

## `libs/dotnet/Rasm.Rhino/.planning/Persistence/settings.md:320`
From: `probe(...).Match(Some: value => ArchiveValue.Of(value, op).Map(Some), None: () => Fin.Succ(None))` here and for `probePreset` at line 328.
To: `probe(...).TraverseM(value => ArchiveValue.Of(value, op)).As()` at both reads.
Why: LanguageExt already produces the required `Fin<Option<ArchiveValue>>`; the explicit optional carrier arms duplicate that inversion.

## `libs/dotnet/Rasm.Rhino/.planning/Annotation/typeface.md:723`
From: optional existing hatch `Match`, returning `Some(ResourceIndex)` after comparison or successful `None`.
To: `Optional(...).TraverseM(held => from current in ... from _ in guard(...) select ResourceIndex.Create(held.Index)).As()`.
Why: `Option.TraverseM` preserves absence and the definition mismatch refusal while deleting both explicit `Option<ResourceIndex>` constructions.

## `libs/dotnet/Rasm.Rhino/.planning/Annotation/typeface.md:797`
From: optional existing section-style `Match`, returning `Some(ImportSeat)` after capture or successful `None`.
To: `Optional(...).TraverseM(held => from index in ... from original in ... select new ImportSeat(index, original)).As()`.
Why: LanguageExt lands the same `Fin<Option<ImportSeat>>`; the native lookup/copy failures and absent style success are unchanged.

## `libs/dotnet/Rasm.Rhino/.planning/Display/interaction.md:887`
From: the mount insertion followed by `BiBind` and a manual `retire()` error-combination fold at lines 887-895.
To: append `.Rollback(release: retire, key: op)` to the insertion pipeline.
Why: Domain `Rollback` already releases only after failure and appends retirement failure after the primary without reconstructing either carrier arm.

## `libs/dotnet/Rasm.Rhino/.planning/Persistence/snapshots.md:427`
From: registration followed by `BindFail` and a hand-switched `Cell.Step` release at lines 427-438.
To: append `.Rollback(release: () => Cell.Step(...).Switch(committed: _ => Fin.Succ(unit), ceded: _ => Fin.Fail<Unit>(op.InvalidResult()), refused: row => Fin.Fail<Unit>(row.Cause), contended: _ => Fin.Fail<Unit>(op.InvalidResult())), key: op)`.
Why: `Rollback` owns failure-only compensation and preserves the same transition-specific cleanup fault before combining it with registration failure.

## `libs/dotnet/Rasm.Rhino/.planning/Persistence/settings.md:975`
From: registration `.BindFail(error => Released(...))` plus private `Released` at lines 993-1005.
To: append `.Rollback(release: () => Cell.Step(...).Switch(committed: _ => Fin.Succ(unit), ceded: _ => Fin.Fail<Unit>(op.InvalidResult()), refused: row => Fin.Fail<Unit>(row.Cause), contended: _ => Fin.Fail<Unit>(op.InvalidResult())), key: op)`; delete `Released`.
Why: Domain `Rollback` combines the identical release verdict with the primary and removes the package-local failure algebra and module symbol.

## `libs/dotnet/Rasm.Rhino/.planning/Persistence/snapshots.md:656`
From: `Reported<T>` uses `BindFail`, calls `spec.Report`, then rebuilds `Fin.Fail<T>(error)`.
To: `outcome.MapFail(error => { spec.Report(error); return error; })`.
Why: LanguageExt `MapFail` is the existing failure tap when the error value is unchanged; the reconstructed failed carrier adds no behavior.

## `libs/dotnet/Rasm.Rhino/.planning/Document/layers.md:1014`
From: `BiBind` plus private `Release`, manually disposing every staged attribute and folding cleanup errors at lines 1024-1031 and 1072-1075.
To: append `.Settled(release: () => Custody.Dispose(moves.Map(static move => move.Original), op), key: op)` to `DocumentCommit.Compensated`; delete `Release`.
Why: Domain custody owns unconditional LIFO disposal, all-attempt execution, and primary-plus-cleanup aggregation with fewer branches and one fewer symbol.

## `libs/dotnet/Rasm.Rhino/.planning/Document/lifetime.md:54`
From: `TryClaim() ? Settle(Marked(body, key)) : ...` plus private two-arm `Settle<T>` at lines 98-100.
To: `TryClaim() ? Marked(body, key).Settled(release: () => Fin.Succ(Release()), key: key) : key.Catch(refused)`; delete `Settle<T>`.
Why: Domain `Settled` already runs release on both outcomes and preserves the primary; the local `BiBind` duplicates that posture.
