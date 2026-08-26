# Rasm.Bim LanguageExt / Thinktecture Audit

- `libs/dotnet/Rasm.Bim/.planning/Projection/egress.md:412`
  - from: `nests.Choose(...).OrderBy(...).Map(... )` inside an outer `toSeq`
  - to: `toSeq(nests.Choose(...).OrderBy(...)).Map(static row => row.Edge)`
  - `OrderBy` exits `Seq` as `IOrderedEnumerable`; only `Prelude.toSeq` restores the LanguageExt `Map` receiver, preserving the ordinal order.

- `libs/dotnet/Rasm.Bim/.planning/Semantics/classification.md:273`
  - from: `BsddWire.Rows(response.Hierarchy).OrderBy(...).Map(BsddWire.Ref).ToSeq()`
  - to: `toSeq(BsddWire.Rows(response.Hierarchy).OrderBy(...)).Map(BsddWire.Ref)`
  - LINQ `OrderBy` has neither LanguageExt `Map` nor `ToSeq`; `Prelude.toSeq` is the existing enumerable-admission owner.

- `libs/dotnet/Rasm.Bim/.planning/Exchange/tessellation.md:403`
  - from: `raw.Distinct(...).OrderBy(...).ToSeq()` and `ordered.ToSeq()` at line 407
  - to: `toSeq(raw.Distinct(...).OrderBy(...))` and `toSeq(ordered)`
  - Both receivers are `IEnumerable<string>`; LanguageExt exposes `Prelude.toSeq(IEnumerable<A>)`, not an enumerable `.ToSeq()` extension.

- `libs/dotnet/Rasm.Bim/.planning/Exchange/import.md:663`
  - from: `toSeq(group).OrderBy(static row => row.Ordinate).ToSeq()`
  - to: `toSeq(group.OrderBy(static row => row.Ordinate))`
  - Ordering leaves the carrier; direct `Prelude.toSeq` re-entry preserves the ordered rows with one fewer conversion.

- `libs/dotnet/Rasm.Bim/.planning/Model/structural.md:163`
  - from: `LoadFamily.Items.Map(static row => row.Key).ToSeq()`
  - to: `toSeq(LoadFamily.Items).Map(static row => row.Key)`
  - Thinktecture generates `Items` as `IReadOnlyList<LoadFamily>`; it has no LanguageExt `Map`, and `Seq.Map` already returns `Seq`.

- `libs/dotnet/Rasm.Bim/.planning/Exchange/wire.md:72`
  - from: `InterchangeFormat.Items.Find(...)`
  - to: `toSeq(InterchangeFormat.Items).Find(...)`
  - Thinktecture `Items` is an `IReadOnlyList`; LanguageExt `Find` binds only after the existing `Prelude.toSeq` admission.

- `libs/dotnet/Rasm.Bim/.planning/Model/eurocode.md:263`
  - from: `PsiRow.Items.ToSeq()` and `GammaRow.Items.ToSeq()` at line 272
  - to: `toSeq(PsiRow.Items)` and `toSeq(GammaRow.Items)`
  - Generated smart-enum rosters are `IReadOnlyList<T>`; the LanguageExt enumerable ingress is the static `toSeq`, not `.ToSeq()`.

- `libs/dotnet/Rasm.Bim/.planning/Model/zones.md:73`
  - from: `Items.Choose(static row => ...)`
  - to: `toSeq(Items).Choose(static row => ...)`
  - Thinktecture's generated roster has no `Choose`; admitting it once to `Seq` preserves the filter-map and the derived frozen index.

- `libs/dotnet/Rasm.Bim/.planning/Projection/wireform.md:80`
  - from: `IfcSerialization.Items.AsIterable().ToSeq()` and `IfcContainer.Items.AsIterable().ToSeq()` at line 208
  - to: `toSeq(IfcSerialization.Items)` and `toSeq(IfcContainer.Items)`
  - `Prelude.toSeq` directly admits the generated `IReadOnlyList`; the `Iterable` lift and immediate materialization add no behavior.

- `libs/dotnet/Rasm.Bim/.planning/Model/structural.md:268`
  - from: three `Accumulated(validation)` calls plus `Accumulated<A>(...) => accumulated.ToFin()` at line 345
  - to: call `.ToFin()` directly on each `Traverse(...).As()` result; delete `Accumulated`
  - The helper only renames LanguageExt `ValidationExtensions.ToFin`; direct composition preserves accumulation and removes one module symbol.

- `libs/dotnet/Rasm.Bim/.planning/Projection/semantic.md:629`
  - from: `AddedEdges.Map(Rule).Fold(Success(unit), (acc, rule) => (acc, rule).Apply(...).As())`
  - to: `AddedEdges.Traverse(edge => Rule(edge, endpoints)).As().Map(static _ => unit)`
  - LanguageExt `Traverse` already performs the applicative inversion and accumulates every `Error`; the manual `Apply` fold duplicates it.

- `libs/dotnet/Rasm.Bim/.planning/Energy/projector.md:210`
  - from: `rooms.Fold(Fin.Succ(scope), (acc, room) => acc.Bind(s => RaiseRoom(...)))`; same shell at lines 224 and 246
  - to: `rooms.FoldM(scope, (s, room) => RaiseRoom(s, library, room, ctx)).As()`; use the same `FoldM(seed, step).As()` form for faces/openings
  - LanguageExt `FoldM` is the existing dependent state fold; it preserves order and first-failure behavior while deleting the hand-carried `Fin` accumulator.

- `libs/dotnet/Rasm.Bim/.planning/Planning/schedule.md:692`
  - from: `Fold(Fin.Succ(seed), (carried, item) => carried.Bind(acc => step(acc, item)))` in `Forward`, `Backward`, and `Paths` at lines 692, 704, 717
  - to: `FoldM(seed, (acc, item) => step(acc, item)).As()` in all three folds
  - These are exactly LanguageExt monadic state folds; `FoldM` preserves topological order, map threading, and fail-fast scheduling faults.

- `libs/dotnet/Rasm.Bim/.planning/Semantics/properties.md:430`
  - from: `Distinct().Fold(Fin.Succ(occurrence), (result, member) => result.Bind(acc => ...))`; same shell at line 440
  - to: `Distinct().FoldM(occurrence, (acc, member) => ...).As()` and `toSeq(rows.Flatten()).FoldM(Map<...>(), (acc, row) => ...).As()`
  - `FoldM` owns dependent accumulation into the immutable maps and preserves each existing typed `Fin` refusal.

- `libs/dotnet/Rasm.Bim/.planning/Semantics/model.md:432`
  - from: `Features.Map(...).Fold(Fin.Succ(GeoImport.Empty), (held, row) => held.Bind(import => ...))`
  - to: `Features.Map(...).FoldM(GeoImport.Empty, (import, row) => ...).As()`
  - The cancellation/refusal decision remains the per-row `Fin`; LanguageExt `FoldM` removes only the manually nested carrier.

- `libs/dotnet/Rasm.Bim/.planning/Exchange/import.md:1399`
  - from: `products.Fold(Fin.Succ((Deferred: ..., Textures: ..., Decoded: 0)), (acc, product) => acc.Bind(split => ...))`
  - to: `products.FoldM((Deferred: ..., Textures: ..., Decoded: 0), (split, product) => ...).As()`
  - The fold is sequential state plus `Fin` already; `FoldM` preserves decode order and short-circuiting without the carrier shell.

- `libs/dotnet/Rasm.Bim/.planning/Energy/results.md:149`
  - from: `results.Fold(Fin.Succ(HashMap<...>()), (acc, result) => acc.Bind(grouped => ...))`
  - to: `results.FoldM(HashMap<...>(), (grouped, result) => ...).As()`
  - LanguageExt `FoldM` directly owns this dependent target-resolution/upsert fold and preserves the existing first unresolved-scope failure.

- `libs/dotnet/Rasm.Bim/.planning/Projection/semantic.md:454`
  - from: `toSeq(quantities).Fold(Fin.Succ(bag), (result, quantity) => result.Bind(acc => quantity switch { ... }))`
  - to: `toSeq(quantities).FoldM(bag, (acc, quantity) => quantity switch { ... }).As()`
  - The switch already returns `Fin` per quantity; `FoldM` preserves sequential bag construction and removes the duplicated bind accumulator.

- `libs/dotnet/Rasm.Bim/.planning/Planning/schedule.md:53`
  - from: one-argument `Option<T>` `TryGet` wrappers on `TaskStatus`, `WorkScheduleKind`, and `TaskKind`, consumed only by their `Of` methods
  - to: each `Of` calls generated `TryGet(key, out T? row) ? row : NotDefined`; delete the three wrappers
  - Thinktecture already generates the lookup and the configured comparer already ignores case; the wrappers add three public symbols and redundant uppercasing.

- `libs/dotnet/Rasm.Bim/.planning/Planning/cost.md:75`
  - from: one-argument `Option<T>` `TryGet` wrappers on `CostScheduleKind`, `ResourceKind`, `CostCategory`, and `ChangeOrderStatus` at lines 75, 96, 115, 454
  - to: each total `Of` invokes generated `TryGet(key, out T? row) ? row : fallback`; delete the four wrappers
  - Thinktecture owns lookup and case-insensitive comparison; no other consumer uses these wrapper symbols, so behavior stays total with less public surface.

- `libs/dotnet/Rasm.Bim/.planning/Projection/relations.md:196`
  - from: `groups.Flatten().ToSeq()` and `rows.Flatten().ToSeq()` at line 355
  - to: `toSeq(groups.Flatten())` and `toSeq(rows.Flatten())`
  - `Flatten` lands on LanguageExt's `IEnumerable` extension surface; only `Prelude.toSeq` re-enters the concrete `Seq` carrier.

- `libs/dotnet/Rasm.Bim/.planning/Semantics/classification.md:465`
  - from: `rows.Flatten().Filter(static row => !row.Candidates.IsEmpty).ToSeq()`
  - to: `toSeq(rows.Flatten()).Filter(static row => !row.Candidates.IsEmpty)`
  - `Flatten` returns an enumerable with no LanguageExt `Filter` or `.ToSeq`; one `toSeq` admission restores both semantics with less code.

- `libs/dotnet/Rasm.Bim/.planning/Semantics/properties.md:527`
  - from: `findings.Flatten().ToSeq()`
  - to: `toSeq(findings.Flatten())`
  - LanguageExt's enumerable `Flatten` has no instance `.ToSeq`; the static ingress preserves the same flattened order.

- `libs/dotnet/Rasm.Bim/.planning/Review/coordination.md:196`
  - from: `.Map(...).Filter(...).Distinct().ToSeq()` and the same trailing `.ToSeq()` at lines 214, 217, 224, and 302
  - to: remove each trailing `.ToSeq()`
  - These receivers are already `Seq`; LanguageExt `SeqExtensions.Distinct` returns `Seq`, so rematerializing through the foldable conversion is behavior-free noise.

- `libs/dotnet/Rasm.Bim/.planning/Model/zones.md:158`
  - from: `ElementQuery.Query(...).Ids.Filter(...).ToSeq()`
  - to: `ElementQuery.Query(...).Ids.Filter(...)`
  - `Ids` is already `Seq<NodeId>` and `Seq.Filter` returns `Seq<NodeId>`; the final carrier conversion is redundant.

- `libs/dotnet/Rasm.Bim/.planning/Projection/egress.md:187`
  - from: `Traverse(...).As().Match(Succ: rows => Fin.Succ(rows.Fold(...)), Fail: errors => Fin.Fail(errors))`
  - to: `Traverse(...).As().Map(rows => rows.Fold(...)).ToFin()`
  - `ValidationExtensions.ToFin` already preserves the accumulated `Error`; the hand `Match` only reconstructs the same `Fin` branches.

- `libs/dotnet/Rasm.Bim/.planning/Semantics/georeference.md:349,380`; `Semantics/vector.md:159,410,490,705`; `Semantics/raster.md:206`
  - from: `Fin<T>.Match(Succ: Some, Fail: _ => Option<T>.None)`
  - to: `Fin<T>.ToOption()`
  - LanguageExt owns this exact lossy projection; the repeated branch pairs add no classification or recovery behavior.

- `libs/dotnet/Rasm.Bim/.planning/Semantics/georeference.md:428`
  - from: `pairs.Fold(Fin.Succ((Memo: ..., Rows: ...)), (held, pair) => held.Bind(state => ...)).Map(...)`
  - to: `pairs.FoldM((Memo: ..., Rows: ...), (state, pair) => ...).As().Map(...)`
  - `FoldM` is the existing ordered, fail-fast state fold and preserves cancellation plus the memo threaded between pair probes.

- `libs/dotnet/Rasm.Bim/.planning/Semantics/raster.md:248`
  - from: `tile.Overviews.Fold(Fin.Succ((Grid: basis, Levels: ...)), (state, level) => state.Bind(carried => ...))`
  - to: `tile.Overviews.FoldM((Grid: basis, Levels: ...), (carried, level) => ...).As()`
  - Each level depends on the prior coarsened grid; LanguageExt `FoldM` preserves order and first-failure behavior without the carrier accumulator.

- `libs/dotnet/Rasm.Bim/.planning/Energy/projector.md:323,335,347`
  - from: the remaining Dragonfly `Fold(Fin.Succ(seed), (acc, row) => acc.Bind(...))` story/building/room folds
  - to: `FoldM(seed, (state, row) => ...).As()`, binding an existing `Fin` seed once before the fold where required
  - These are dependent state folds over `Fin`; `FoldM` removes the repeated manual shell while retaining source order and short-circuiting.

- `libs/dotnet/Rasm.Bim/.planning/Exchange/reconstruct.md:310`
  - from: `segments.Filter(...).Fold(Fin.Succ(GraphDelta.Empty.Reheader(...)), (acc, segment) => acc.Bind(delta => Author(...).Map(delta.Merge)))`
  - to: `segments.Filter(...).FoldM(GraphDelta.Empty.Reheader(...), (delta, segment) => Author(...).Map(delta.Merge)).As()`
  - `FoldM` directly owns this ordered `Fin`-state merge and preserves the first rejected segment plus every preceding delta.

- `libs/dotnet/Rasm.Bim/.planning/Model/emitter.md:293`
  - from: `Traverse(RowOf).As().Bind(rows => Audit(rows.ToSeq(), ...).ToFin())`
  - to: `Traverse(RowOf).As().Bind(rows => Audit(rows, ...).ToFin())`
  - `Seq.Traverse` already lands `Fin<Seq<VocabularyRow>>`; the immediate `ToSeq` rematerializes the same concrete carrier.

- `libs/dotnet/Rasm.Bim/.planning/Exchange/import.md:852`
  - from: `toSeq(...).Filter(...).Map(...).GroupBy(...).Choose(...).ToSeq()`
  - to: `toSeq(toSeq(...).Filter(...).Map(...).GroupBy(...)).Choose(...)`
  - LINQ `GroupBy` exits `Seq`; `Prelude.toSeq` must re-admit its groups before LanguageExt `Choose`, which already returns the final `Seq`.

- `libs/dotnet/Rasm.Bim/.planning/Exchange/tessellation.md:356`
  - from: `GeomSetting.Items.Where(...).Select(...).Distinct().OrderBy(...).ToSeq()`
  - to: `toSeq(GeomSetting.Items.Where(...).Select(...).Distinct().OrderBy(...))`
  - Thinktecture `Items` is `IReadOnlyList<GeomSetting>` and the whole chain is LINQ `IEnumerable`; only `Prelude.toSeq` is its LanguageExt ingress.

- `libs/dotnet/Rasm.Bim/.planning/Projection/foreign.md:89`
  - from: `toSeq(...).Choose(...).OrderBy(...).ToSeq().Traverse(...)`
  - to: `toSeq(toSeq(...).Choose(...).OrderBy(...)).Traverse(...)`
  - `OrderBy` changes the receiver to `IOrderedEnumerable`; static `toSeq` restores the ordered `Seq` consumed by the existing effect traversal.

- `libs/dotnet/Rasm.Bim/.planning/Exchange/export.md:372,385,997`
  - from: `Pool.Values.Head()`, `mesh.Blocks.Head()`, and `geometry.Blocks.Head()`
  - to: `Pool.Values.ToSeq()[0]`, `mesh.Blocks[0]`, and `geometry.Blocks[0]`
  - LanguageExt `Head` is an `Option<A>` property, not a method; indexing preserves these sites' existing required-leading-row assumption without adding a match shell.

- `libs/dotnet/Rasm.Bim/.planning/Exchange/saf.md:324`
  - from: `value.IfNoneUnsafe(() => null)`
  - to: `Op.ToHostSlot(value)`
  - `Rasm.Domain.Op.ToHostSlot` is the canonical `Option<T>` to nullable host-slot bridge; it replaces the removed v4 API without bypassing the domain boundary.

- `libs/dotnet/Rasm.Bim/.planning/Model/observability.md:98,115`
  - from: `GlobalIdSet.Of(ids) => Create(ids)` and `ContentKeySet.Of(keys) => Create(keys)`
  - to: delete both wrappers; replace the two `ContentKeySet.Of` calls with generated `ContentKeySet.Create` and delete the unused `GlobalIdSet.Of`
  - Thinktecture `Create` already runs each `ValidateFactoryArguments` normalization hook; the wrappers add two public symbols and no admission policy.

- `libs/dotnet/Rasm.Bim/.planning/Projection/value.md:281`
  - from: `Signature(value).Match(Some: row => MeasureOf(...).Map(Some), None: () => Fin.Succ(None))`
  - to: `Signature(value).TraverseM(row => MeasureOf(value!, row, scheme, declared, key)).As()`
  - LanguageExt `Option.TraverseM` owns this zero-or-one `Fin` inversion: `None` remains successful `None`, while `Some` runs the same measure admission.

- `libs/dotnet/Rasm.Bim/.planning/Planning/cost.md:390,533,536,640,643`
  - from: five `Option.Match(Some: value => effect(value).Map(Some), None: () => Fin.Succ(None))` shells
  - to: remove each success-side `.Map(Some)` and use `option.TraverseM(value => effect(value)).As()`, retaining the surrounding `Bind` only where the option itself is effectful
  - `Option.TraverseM` preserves absence, invokes each existing `Fin` effect only for `Some`, and removes the duplicated carrier branches.

- `libs/dotnet/Rasm.Bim/.planning/Semantics/appearance.md:474`
  - from: `IndexedColour.Of(...).Match(Some: colour => colour.Author(...).Map(Some), None: () => Fin.Succ(None))`
  - to: `IndexedColour.Of(...).TraverseM(colour => colour.Author(faceSet, key)).As()`
  - `Option.TraverseM` is the exact zero-or-one `Fin` inversion here: it retains `None` and the original authoring failure without rebuilding either branch.

- `libs/dotnet/Rasm.Bim/.planning/Semantics/connection.md:56`
  - from: `detail.Match(None: Fin.Succ(None), Some: bag => rooted.Find(...).ToFin(...).Map(node => Some((bag, edge))))`
  - to: `detail.TraverseM(bag => rooted.Find(...).ToFin(...).Map(node => (bag, edge))).As()`
  - Removing the inner `Some` lets `Option.TraverseM` supply the one optional layer, preserving absence and the same dangling-reference `Fin` fault.

- `libs/dotnet/Rasm.Bim/.planning/Semantics/connection.md:174,196`
  - from: `BagOf` manually matches the optional row while `Rows` returns `Fin<Option<PropertyBag>>` by wrapping every success in `Some`
  - to: make `Rows` return `Fin<PropertyBag>` without the `Some`; compose it as `Optional(...).Bind(...).TraverseM(hit => Rows(...)).As()`
  - `Rows` has no other consumer; LanguageExt supplies the sole optional layer, preserving absent rows and accumulated row failures with a smaller private signature.

- `libs/dotnet/Rasm.Bim/.planning/Projection/relations.md:259`
  - from: `Optional(rel.RealizingElement).Match(Some: element => Endpoint(...).Map(Some), None: () => Fin.Succ(None).ToValidation())`
  - to: `Optional(rel.RealizingElement).TraverseM(element => Endpoint(...)).As()`
  - `Option.TraverseM` inverts the optional realizing element into the existing `Validation` effect and preserves both successful absence and endpoint faults.

- `libs/dotnet/Rasm.Bim/.planning/Energy/exchange.md:109`; `Exchange/format.md:273`; `Review/validation.md:265`
  - from: `option.Match(Some: Fin.Succ, None: () => Fin.Fail(error))`
  - to: `option.ToFin(error)` at each admission site
  - LanguageExt `Option.ToFin` owns this exact required-value projection; each existing `BimFault` remains unchanged while the reconstructed branches disappear.

- `libs/dotnet/Rasm.Bim/.planning/Planning/cost.md:139`; `Projection/foreign.md:287`; `Review/issues.md:385`; `Semantics/vector.md:513`
  - from: `option.Match(Some: value => Fin.Succ(project(value)), None: () => Fin.Fail(error))`
  - to: `option.ToFin(error).Map(value => project(value))`
  - `Option.ToFin` preserves each current missing-value fault, and the existing success projection belongs in `Map` rather than a duplicated carrier match.

- `libs/dotnet/Rasm.Bim/.planning/Projection/relations.md:137,159`; `Projection/raise.md:174`; `Exchange/reconstruct.md:354`; `Semantics/composition.md:336`; `Semantics/raster.md:56`
  - from: generated `Items.AsIterable()` chains, including immediate `.ToSeq()` materializations
  - to: admit each roster once with `toSeq(Items)` (qualified at cross-type sites), then retain the existing `Map`, `Choose`, `Filter`, or `Traverse`
  - Thinktecture generates every `Items` receiver as `IReadOnlyList<T>`; `Prelude.toSeq` is the direct LanguageExt ingress and removes the intermediate `Iterable` shell.

- `libs/dotnet/Rasm.Bim/.planning/Model/systems.md:241`; `Semantics/model.md:385`
  - from: `ledger.Unbound.Distinct().ToSeq()` and `wgs.Bind(...).Distinct().ToSeq()`
  - to: remove the trailing `.ToSeq()` at both sites
  - Both receivers are already `Seq`; LanguageExt `Seq.Distinct` returns `Seq`, so the final foldable materialization has no behavior.

- `libs/dotnet/Rasm.Bim/.planning/Semantics/classification.md:96`; `Projection/egress.md:507`; `Exchange/saf.md:399`
  - from: `option.Match(Some: value => effect(value).Map(Some), None: () => Fin.Succ(None))`
  - to: remove the inner `Some` and use `option.TraverseM(value => effect(value)).As()`
  - Each effect returns the unwrapped admitted value; LanguageExt supplies the sole optional layer while preserving absence and the existing `BimFault` unchanged.

- `libs/dotnet/Rasm.Bim/.planning/Exchange/import.md:516,532`
  - from: unused `MeshoptMode.Route` and `MeshoptFilter.Route` wrappers over generated `TryGet(token, out row)`
  - to: delete both `Route` methods
  - Thinktecture already owns both lookups, and neither wrapper has a consumer anywhere in the Markdown corpus; deleting them removes two module symbols with no behavior change.

- `libs/dotnet/Rasm.Bim/.planning/Model/systems.md:572`
  - from: optional `(IfcClass, Body)` matched into `ClearanceOf(...).Map(...Some...)` or `Fin.Succ(None)`
  - to: `.TraverseM(row => ClearanceOf(...).Map(clearance => new ClashCandidate(...))).As()`
  - LanguageExt supplies the sole optional layer and keeps the existing ordered, fail-fast `Fin` traversal unchanged.

- `libs/dotnet/Rasm.Bim/.planning/Energy/projector.md:279,292`
  - from: `Materials.Find(...).Match(None: Success(None), Some: m => validation.Map(pair => Some(...)))` and the identical glazing shell
  - to: `Materials.Find(...).TraverseM(m => validation.Map(pair => (...))).As()` and the same `Glazings.Find(...).TraverseM(...)`
  - `Option.TraverseM` already inverts each optional lookup into `Validation`, preserving absence and every material-admission error without a nested option.

- `libs/dotnet/Rasm.Bim/.planning/Projection/relations.md:219`
  - from: `globalId.Match(None: Fin.Fail(absent), Some: id => rooted.Find(id).ToFin(missing)).ToValidation()`
  - to: `globalId.ToFin(absent).Bind(id => rooted.Find(id).ToFin(missing)).ToValidation()`
  - LanguageExt `Option.ToFin` owns required-value admission; the dependent lookup remains fail-fast before the one accumulation ingress.

- `libs/dotnet/Rasm.Bim/.planning/Exchange/export.md:598`
  - from: `row.Serialization.Match(None: () => Fin.Fail(missing), Some: form => projector.Emit(...).Map(...))`
  - to: `row.Serialization.ToFin(missing).Bind(form => projector.Emit(...).Map(...))`
  - `Option.ToFin` is the existing required-codec admission; dependent emit behavior and the current typed refusal remain unchanged.

- `libs/dotnet/Rasm.Bim/.planning/Model/structural.md:121`
  - from: private `ByEntity = Items.ToFrozenDictionary(row.Key, row)` plus `Token` lookup
  - to: delete `ByEntity`; implement `Token` from generated `TryGet(key, out LoadFamily? row)`
  - Thinktecture already caches the ordinal-keyed smart-enum lookup selected by the declared comparer, so the mirror dictionary duplicates generated ownership.

- `libs/dotnet/Rasm.Bim/.planning/Exchange/saf.md:119,154`; `libs/dotnet/Rasm.Bim/.planning/Exchange/format.md:270`
  - from: key-mirror dictionaries `ByAnalytical`, `ByVariety`, and `ByKey` built from each smart enum's `Items`
  - to: delete the three dictionaries; use each owner's generated `TryGet(key, out row)` at the existing lookup site
  - Thinktecture generated lookup uses each declared ordinal/ignore-case comparer; the non-key `ByPhysical`, `ByBehaviour`, media, and extension indexes remain intact.

- `libs/dotnet/Rasm.Bim/.planning/Exchange/events.md:158`; `libs/dotnet/Rasm.Bim/.planning/Semantics/vector.md:278,644`
  - from: hand `Option<T>.Match(..., null)` / `ValueUnsafe()` projections into nullable event, KML, and OGR slots
  - to: `Op.ToHostSlot(value)` or `Op.ToHostSlot(value.Map(existingProjection))`; delete vector's now-unused `LanguageExt.UnsafeValueAccess` import
  - `Rasm.Domain.Op.ToHostSlot` is the pre-existing sole reference-slot boundary; it preserves `Some` projection and `None -> null` while removing unsafe/manual unwraps.

- `libs/dotnet/Rasm.Bim/.planning/Review/validation.md:590,597,617,618,621,622`
  - from: `ValueUnsafe()` and `Match<int?>(Some: value, None: null)` writes into IDS nullable members
  - to: `Op.ToHostSlot(option.Map(existingProjection))` for reference slots and `Op.ToHostNullable(option)` for integer slots; delete the unsafe-access import
  - The Domain host bridges are the existing null-egress owner; all IDS values and absence semantics remain byte-for-byte equivalent.

- `libs/dotnet/Rasm.Bim/.planning/Review/issues.md:351,352,373,374,625,655,667,669,676-678,697,703,707,713,719`
  - from: Mapperly/BCF boundary helpers and body fields manually matching `Option` into nullable references or nullable structs
  - to: use `Op.ToHostSlot(option.Map(existingProjection))` and `Op.ToHostNullable(option)`; retain case filtering as `Option.Bind` before the bridge
  - `Op` already owns both host-null projections, reducing repeated branch pairs while preserving every BCF wire/body slot's absence.

- `libs/dotnet/Rasm.Bim/.planning/Exchange/saf.md:327,331,542,589,596,666`
  - from: repeated `Option.Match(Some: projected nullable enum/value, None: null)` at SAF mapper and workbook slots
  - to: `Op.ToHostNullable(option.Map(existingProjection))` for enum slots and `Op.ToHostSlot(option.Map(static value => (object)value))` for the object slot
  - The Domain bridges preserve the exact nullable SAF values and replace six local spellings of the same host-egress rule.

- `libs/dotnet/Rasm.Bim/.planning/Energy/derive.md:182,288,295`; `libs/dotnet/Rasm.Bim/.planning/Semantics/appearance.md:443`
  - from: optional construction/lighting values manually matched into projected host objects or `null`
  - to: `Op.ToHostSlot(option.Map(existingProjection))`
  - The pre-existing Domain host-slot bridge preserves object construction only for `Some` and writes the same `null` for `None` with fewer branches.

- `libs/dotnet/Rasm.Bim/.planning/Model/query.md:370`
  - from: `term switch` over `All`/`Any`/`Not`/`Leaf` with `_ => (None, Some(term))`
  - to: `term.Map(all: ..., any: ..., not: ..., leaf: ..., closure: _ => (None, Some(term)))` with the existing branch bodies
  - Thinktecture generated `Map` exhaustively owns all five `Predicate<TLeaf>` cases; naming `Closure` preserves the fallback while restoring compile-time pressure for a new case.

- `libs/dotnet/Rasm.Bim/.planning/Energy/derive.md:253,268,436,444,453`; `libs/dotnet/Rasm.Bim/.planning/Exchange/export.md:1360,1366,1406`
  - from: `graph.EdgesAt(...).Choose(...)`, with trailing `.ToSeq()` at lines 258, 272, 456, and 1409
  - to: `toSeq(graph.EdgesAt(...)).Choose(...)`; delete those trailing `.ToSeq()` calls
  - `ElementGraph.EdgesAt` returns `ImmutableArray<Relationship>`, which has no LanguageExt `Choose`; `Prelude.toSeq` admits it and `Seq.Choose` already returns `Seq`.

- `libs/dotnet/Rasm.Bim/.planning/Projection/egress.md:160,167,320,334,336`
  - from: each direct `graph.EdgesAt(...).Choose(...)` / `frame.Graph.EdgesAt(...).Choose(...)`, followed by `.ToSeq()` at lines 162, 170, 323, and 342
  - to: wrap every `EdgesAt(...)` receiver in `toSeq(...)` and delete the trailing `.ToSeq()` calls
  - `EdgesAt` is an `ImmutableArray`, while `Choose` binds only after `Prelude.toSeq`; its result and the intervening `Bind` remain `Seq`.

- `libs/dotnet/Rasm.Bim/.planning/Model/spatial.md:106`; `libs/dotnet/Rasm.Bim/.planning/Energy/derive.md:265,381,385`; `libs/dotnet/Rasm.Bim/.planning/Model/systems.md:571`; `libs/dotnet/Rasm.Bim/.planning/Exchange/export.md:1305`; `libs/dotnet/Rasm.Bim/.planning/Semantics/properties.md:513`; `libs/dotnet/Rasm.Bim/.planning/Review/diff.md:357`
  - from: `graph.ObjectNodes` / `Seq` `Filter`, `Map`, `Choose`, or `Take` pipelines ending in `.ToSeq()`
  - to: delete only the trailing `.ToSeq()` at each anchor
  - `ObjectNodes` is `Seq<Node.Object>` and every named LanguageExt operation here already returns `Seq`; the conversions add no behavior.

- `libs/dotnet/Rasm.Bim/.planning/Model/zones.md:177`; `libs/dotnet/Rasm.Bim/.planning/Projection/semantic.md:512`; `libs/dotnet/Rasm.Bim/.planning/Semantics/connection.md:65`; `libs/dotnet/Rasm.Bim/.planning/Projection/egress.md:384`
  - from: `Seq` pipelines ending in `.Distinct().ToSeq()`, `.Map(...).ToSeq()`, `.Choose(...).ToSeq()`, or `.Add(...).ToSeq()`
  - to: delete only the trailing `.ToSeq()` at each anchor
  - LanguageExt `Seq.Distinct`, `Map`, `Choose`, and `Add` preserve the concrete `Seq` carrier, so rematerialization is redundant.

- `libs/dotnet/Rasm.Bim/.planning/Projection/egress.md:547`
  - from: `toSeq(frame.Edges.Attachments.Values).Flatten().Choose(...).ToSeq()`
  - to: `toSeq(toSeq(frame.Edges.Attachments.Values).Flatten()).Choose(...)`
  - `Flatten` exits to `IEnumerable`, which has neither `Choose` nor foldable `.ToSeq()`; the existing `Prelude.toSeq` is the one required re-entry and `Choose` returns the final `Seq`.

- `libs/dotnet/Rasm.Bim/.planning/Exchange/events.md:168`
  - from: `Mint(...).Bind(held => held.Match(Some: port.Emit, None: () => Fin.Succ(unit)))`
  - to: `Mint(...).Bind(held => held.TraverseM(port.Emit).As().Map(static _ => unit))`
  - `Option.TraverseM` is the existing zero-or-one `Fin` effect: absence remains a successful no-op and `Some` emits with the same failure behavior.
