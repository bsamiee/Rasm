# Rasm.AppUi LanguageExt / Thinktecture Audit

## `libs/dotnet/Rasm.AppUi/.planning/Render/meshlets.md:40`
`from`: `Map(static (index, row) => (Row: row, Index: index))`
`to`: `Map(static (row, index) => (Row: row, Index: index))`
`Seq<A>.Map(Func<A,int,B>)` is value-first; the present lambda reverses the exact LanguageExt contract.

## `libs/dotnet/Rasm.AppUi/.planning/Render/meshlets.md:149`
`from`: `toSeq(CutPhase.Items).Choose(...).OrderBy(...).ToSeq().Map(...)`
`to`: `toSeq(toSeq(CutPhase.Items).Choose(...).OrderBy(...)).Map(...)`
`OrderBy` exits `Seq` to `IOrderedEnumerable`; only `Prelude.toSeq(IEnumerable<T>)` re-enters the carrier.

## `libs/dotnet/Rasm.AppUi/.planning/Shell/commands.md:105`
`from`: `kinds.Distinct().OrderBy(...).ToSeq()`
`to`: `toSeq(kinds.Distinct().OrderBy(...))`
`IOrderedEnumerable<T>` has no LanguageExt `ToSeq()` extension; the Prelude conversion preserves the same sorted distinct values.

## `libs/dotnet/Rasm.AppUi/.planning/Shell/accessibility.md:164`
`from`: `nodes.OrderBy(...).ThenBy(...).ToSeq()`
`to`: `toSeq(nodes.OrderBy(...).ThenBy(...))`
The LINQ ordering chain returns `IOrderedEnumerable<SceneAccessNode>`; direct `ToSeq()` cannot bind, while the existing Prelude lift preserves ordering.

## `libs/dotnet/Rasm.AppUi/.planning/Render/pipeline.md:101`
`from`: `toSeq(Items).Filter(...).OrderByDescending(...).Head`
`to`: `toSeq(toSeq(Items).Filter(...).OrderByDescending(...)).Head`
`OrderByDescending` leaves the carrier, so its result has no `Seq.Head` option property until `Prelude.toSeq` re-entry.

## `libs/dotnet/Rasm.AppUi/.planning/Render/viewpoint.md:197`
`from`: `magnitudes.Min()` / `magnitudes.Max()`
`to`: `magnitudes.Min(double.PositiveInfinity)` / `magnitudes.Max(double.NegativeInfinity)`
The unseeded LanguageExt and LINQ reductions are ambiguous; the guarded non-empty branch makes the seeded LanguageExt results identical.

## `libs/dotnet/Rasm.AppUi/.planning/Render/viewpoint.md:240`
`from`: `rows.Map(row => Fin<VisibilityOverride>).Traverse(identity).As().Map(rows => rows.ToSeq())`
`to`: `rows.Traverse(row => Fin<VisibilityOverride>).As()` in both `ColorBy` and `Participation` arms
LanguageExt `Traverse` fuses map-plus-inversion and already returns the concrete `Seq`; both extra layers are identity work.

## `libs/dotnet/Rasm.AppUi/.planning/Render/immersive.md:1266`
`from`: `stale.Traverse(...).As().Map(static rows => rows.ToSeq())`
`to`: `stale.Traverse(...).As()`
`Seq.Traverse` already lands `Fin<Seq<XrEvent>>`; the final `ToSeq` is an identity conversion.

## `libs/dotnet/Rasm.AppUi/.planning/Diagnostics/devloop.md:459`; `libs/dotnet/Rasm.AppUi/.planning/Diagnostics/devloop.md:539`
`from`: `block.Lines.Map((line, index) => Label(...)).ToSeq()`; `toSeq(HudFact.Items).Map(...).ToSeq().Strict()`
`to`: delete each terminal `ToSeq()` and retain the surrounding `Map`/`Strict`
Both receivers are already `Seq`; LanguageExt `Map` preserves that concrete carrier, so each conversion is identity work.

## `libs/dotnet/Rasm.AppUi/.planning/Document/notebook.md:410`
`from`: `rows.Map(static row => row.Cell).ToSeq()`
`to`: `rows.Map(static row => row.Cell)`
`rows` lands from `Seq.TraverseM`, so its LanguageExt `Map` already preserves the concrete `Seq` required by `Notebook.Of`.

## `libs/dotnet/Rasm.AppUi/.planning/Document/notebook.md:481`
`from`: `row.Inputs.TraverseM(...).As().Map(hashes => new RecomputeNode(..., hashes.ToSeq()))`
`to`: retain the `Map`, but pass `hashes` directly to `RecomputeNode`
`Seq.TraverseM` already lands the concrete `Seq<ChainHash>`; the nested conversion is identity work.

## `libs/dotnet/Rasm.AppUi/.planning/Render/reality.md:716`
`from`: `Epochs.Map(epoch => new Keyframe<int>(...)).ToSeq()`
`to`: `Epochs.Map(epoch => new Keyframe<int>(...))`
`Epochs` is `Seq<CaptureEpoch>`; deleting the identity conversion preserves keyframe order and shape.

## `libs/dotnet/Rasm.AppUi/.planning/Render/shading.md:565`
`from`: `context.Material.Bsdf.Lobes.Map(static lobe => lobe.Weight.Value).ToSeq()`
`to`: `context.Material.Bsdf.Lobes.Map(static lobe => lobe.Weight.Value)`
Domain's `LayeredBsdf.Lobes` is already `Seq<LobeWeight>`; LanguageExt `Map` returns the exact `Seq<double>` consumed by `ShadeSupply.Of`.

## `libs/dotnet/Rasm.AppUi/.planning/Theme/typography.md:1267`
`from`: `text.Runs.Bind(Marks).Filter(...).OrderByDescending(...).AsIterable().ToSeq().Head`
`to`: `toSeq(text.Runs.Bind(Marks).Filter(...).OrderByDescending(...)).Head`
`Prelude.toSeq` directly re-enters from the LINQ ordering result; the intermediate `Iterable` performs no transform.

## `libs/dotnet/Rasm.AppUi/.planning/Theme/typography.md:1275`
`from`: `covered.OrderBy(...).AsIterable().ToSeq().Head`
`to`: `toSeq(covered.OrderBy(...)).Head`
The direct LanguageExt carrier re-entry preserves the same ordered marks and option-shaped head with one fewer carrier conversion.

## `libs/dotnet/Rasm.AppUi/.planning/Editing/forms.md:965`
`from`: `toSeq(facets.Held).OrderBy(...).AsIterable().ToSeq().Traverse(...)`
`to`: `toSeq(toSeq(facets.Held).OrderBy(...)).Traverse(...)`
`Prelude.toSeq(IEnumerable<T>)` directly restores the ordered run to `Seq`; the `Iterable` detour owns no behavior.

## `libs/dotnet/Rasm.AppUi/.planning/Charts/telemetry.md:175`
`from`: `toSeq(StatBand.Items).OrderBy(...).AsIterable().ToSeq().Map(...)`
`to`: `toSeq(toSeq(StatBand.Items).OrderBy(...)).Map(...)`
The existing Prelude conversion directly restores the LINQ-ordered smart-enum rows before the same LanguageExt projection.

## `libs/dotnet/Rasm.AppUi/.planning/Vfx/compose.md:232`
`from`: hand-written `(Validate(...), admitted) switch` including the impossible `ComposeFault` validation-error arm
`to`: `Play.AcceptValidated<ComposeTrack>(Validate(slot, frames, out ComposeTrack? admitted), admitted)`
Thinktecture returns `ValidationError?`; the existing Domain bridge maps it to `Error` and removes the duplicated three-arm lift.

## `libs/dotnet/Rasm.AppUi/.planning/Document/board.md:108`
`from`: `Validate(...) is { } fault ? Fin.Fail<FrameCrop>(fault) : Fin.Succ(crop!)`
`to`: `Op.Of(name: "appui.board.crop").AcceptValidated<FrameCrop>(Validate(..., out FrameCrop? crop), crop)`
`ValidationError` is not a LanguageExt `Error`; the existing Domain complex-owner bridge performs the typed lift without a new symbol.

## `libs/dotnet/Rasm.AppUi/.planning/Document/board.md:158`
`from`: `MetricBinding.Validate(...) is { } fault ? Fin.Fail<BoardItem>(fault) : Fin.Succ<BoardItem>(card with { Binding = bound! })`
`to`: `Op.Of(name: "appui.board.reference").AcceptValidated<MetricBinding>(MetricBinding.Validate(..., out MetricBinding? bound), bound).Map<BoardItem>(binding => card with { Binding = binding })`
The existing Domain complex-owner bridge converts Thinktecture's `ValidationError?` and removes the unsafe nullable unwrap without new surface.

## `libs/dotnet/Rasm.AppUi/.planning/Document/board.md:468`; `libs/dotnet/Rasm.AppUi/.planning/Document/board.md:478`; `libs/dotnet/Rasm.AppUi/.planning/Document/board.md:487`; `libs/dotnet/Rasm.AppUi/.planning/Document/board.md:645`
`from`: `skeleton.ToSeq()` / `items.ToSeq()` / `toHashMap(bound.ToSeq())` / `blocks.ToSeq().Bind(...)` after `Seq.Traverse`
`to`: use `skeleton`, `items`, `toHashMap(bound)`, and `blocks.Bind(...)` directly
Every value already lands as concrete `Seq` from LanguageExt traversal; all four conversions are identity work and no consumer shape changes.

## `libs/dotnet/Rasm.AppUi/.planning/Document/media.md:127`
`from`: `Admitting.AcceptValidated<AssetKey, ValidationError>(source)`
`to`: `Admitting.AcceptValidated<AssetKey>(source)`
Domain's generic order is owner-then-raw; `AssetKey` is a string factory, so the existing string overload is the exact admission and removes the false raw error type.

## `libs/dotnet/Rasm.AppUi/.planning/Collab/compare.md:109`
`from`: `Branched(Document.Key)` plus the private `DocumentKey.Validate`/nullable-unwrapping helper
`to`: `Op.Of(name: "appui.compare.branch").AcceptValidated<DocumentKey>($"{Document.Key.Value}/fork/{Guid.CreateVersion7():N}")`
Domain already owns Thinktecture keyed-owner admission; inline it and delete `Branched` as a one-call forwarding symbol.

## `libs/dotnet/Rasm.AppUi/.planning/Editing/livedata.md:1363`
`from`: unused `OptionKey.Admit` forwarding as `AcceptValidated<string, OptionKey>(candidate)`
`to`: delete `OptionKey.Admit`; any future ingress calls `op.AcceptValidated<OptionKey>(candidate)` directly
The method has no consumer and reverses Domain's owner-then-raw generic order while only renaming its string admission bridge.

## `libs/dotnet/Rasm.AppUi/.planning/Editing/livedata.md:847`
`from`: `AcceptValidated<string, PropertyName>(property)`
`to`: `AcceptValidated<PropertyName>(property)`
Domain's existing string admission overload takes the Thinktecture owner alone; the current two generic arguments reverse owner and raw.

## `libs/dotnet/Rasm.AppUi/.planning/Theme/motion.md:95`; `libs/dotnet/Rasm.AppUi/.planning/Vfx/compose.md:290`
`from`: success arms cast null to `MotionFault?` / `ComposeFault?`
`to`: cast null to `ValidationError?`
Thinktecture `ValidateFactoryArguments` requires `ValidationError?`; the present branch types cannot join the failure arm truthfully.

## `libs/dotnet/Rasm.AppUi/.planning/Render/pathtrace.md:501`
`from`: private `Draw` using `UnitInterval.TryCreate(value, out unit) ? unit : default`
`to`: replace both `Draw(s.U0)` / `Draw(s.U1)` with `UnitInterval.Create(...)` and delete `Draw`
Both inputs come from `Deterministic.NextUnit`; Thinktecture construction consumes that proved unit domain without a default-valued fallback symbol.

## `libs/dotnet/Rasm.AppUi/.planning/Vfx/shader.md:444`
`from`: `row.Mark switch { TileMark.Cell cell => ..., var mark => ...((TileMark.Rule)mark)... }`
`to`: `row.Mark.Switch(cell: cell => ..., rule: rule => ...)`
Thinktecture already emits exhaustive dispatch; removing the catch-all and cast makes a future case break instead of miscasting.

## `libs/dotnet/Rasm.AppUi/.planning/Charts/boards.md:222`
`from`: `policy switch { SpanPolicy.Fixed fixedSpan => ..., _ => ... }`
`to`: `policy.Switch(@fixed: fixedSpan => ..., equal: _ => ...)`
Thinktecture's generated fold preserves both current results and removes false exhaustiveness over the closed owner.

## `libs/dotnet/Rasm.AppUi/.planning/Shell/dialogs.md:482`
`from`: `request.Friction switch` with `Inline`, `Typed`, and `_`
`to`: `request.Friction.Switch(inline: ..., typed: ..., acknowledge: ...)`
`ConfirmFriction` is a generated union; its existing exhaustive fold removes the catch-all without adding code or surface.

## `libs/dotnet/Rasm.AppUi/.planning/Shell/dialogs.md:465`
`from`: non-empty `stack` branches dereference `stack.Head.IsEnded`, pass `stack.Head` to `Blocks`, then call `stack.Head.Close`
`to`: fold `surface.Port.Sessions().Rev().Head.Match(Some: session => ..., None: missing)` once and use `session` throughout
LanguageExt `Seq.Head` is `Option<DialogSession>`, not a row; the existing option fold removes three invalid dereferences and preserves empty/ended/veto/close outcomes.

## `libs/dotnet/Rasm.AppUi/.planning/Render/pipeline.md:345`
`from`: `this switch { Ganesh ..., Wgpu ..., Browser ..., _ => Software }`
`to`: `Switch(ganesh: ..., raster: _ => Software, wgpu: _ => Wgpu, browser: _ => WebGpu)`
The generated `GpuBinding` fold preserves the mapping while replacing a catch-all that silently classifies every future case as software.

## `libs/dotnet/Rasm.AppUi/.planning/Render/pipeline.md:278`
`from`: the `GpuBackend.Target` delegate column plus `GaneshTarget`/`RasterTarget`/`WgpuTarget`/`BrowserTarget`, forwarded by `GpuBinding.Target => Backend.Target(this, request)`
`to`: remove the delegate constructor column and four helpers; implement `GpuBinding.Target` once with its generated `Switch(state: request, ganesh:, raster:, wgpu:, browser:)`
`GpuBinding` already owns the exhaustive substrate case and derives `Backend`; this preserves each acquisition arm while deleting five forwarding symbols and the duplicate discriminant.

## `libs/dotnet/Rasm.AppUi/.planning/Vfx/compose.md:381`
`from`: `span switch { ComposeSpan.Running run => new Advance(...), _ => new Halt() }`
`to`: `span.Switch(running: run => (VfxMessage)new Advance(...), collapsed: _ => new Halt())`
Thinktecture already owns the two-case dispatch; the generated fold removes the future-case catch-all at equal surface.

## `libs/dotnet/Rasm.AppUi/.planning/Vfx/compose.md:245`
`from`: `span switch { ComposeSpan.Running run => Run(mount, run), _ => Slot.Write(...) }`
`to`: `span.Switch(running: run => Run(mount, run), collapsed: _ => Slot.Write(...))`
The same generated two-case `ComposeSpan` fold is already available here; naming `Collapsed` preserves both outcomes and removes the catch-all.

## `libs/dotnet/Rasm.AppUi/.planning/Document/search.md:135`
`from`: `(Validation<Error, Unit>)(Error)new ValidationError(...)`
`to`: `Validation<Error, Unit>.Fail(new KernelFault.InvalidValue(nameof(SearchQuery), detail))`
Thinktecture `ValidationError` is unrelated to LanguageExt `Error`; the existing kernel fault is the native accumulating failure and preserves the same query-detail refusal.

## `libs/dotnet/Rasm.AppUi/.planning/Document/search.md:355`
`from`: `SearchSource.Items.Map(...).ToSeq()`
`to`: `toSeq(SearchSource.Items).Map(...)`
Thinktecture emits `Items` as `IReadOnlyList<SearchSource>`, not a LanguageExt carrier; the Prelude lift makes the existing projection bind and removes the false terminal conversion.

## `libs/dotnet/Rasm.AppUi/.planning/Vfx/shader.md:249`
`from`: `declared.Except(source).ToSeq() + source.Except(declared).ToSeq()`
`to`: `toSeq(declared.Except(source)) + toSeq(source.Except(declared))`
Both `Except` calls bind LINQ and return `IEnumerable<string>`; only `Prelude.toSeq` re-enters LanguageExt, while `FoldableExtensions.ToSeq` cannot bind either result.

## `libs/dotnet/Rasm.AppUi/.planning/Diagnostics/governor.md:378`; `libs/dotnet/Rasm.AppUi/.planning/Diagnostics/governor.md:391`
`from`: `planned.Map(...).ToSeq()`; `PassBoundaries.Map(...).Filter(...).Map(...).ToSeq().Strict()`
`to`: delete both terminal `ToSeq()` calls
Both roots are `Seq` and every named operator preserves `Seq`; the conversions are identity work and `Strict()` can remain where eager materialization is intended.

## `libs/dotnet/Rasm.AppUi/.planning/Diagnostics/evidence.md:651`; `libs/dotnet/Rasm.AppUi/.planning/Diagnostics/evidence.md:720`
`from`: `rows.GroupBy(...).AsIterable().Map(...).ToSeq()`; `rows.GroupBy(...).AsIterable().ToSeq().TraverseM(...)`
`to`: `toSeq(rows.GroupBy(...)).Map(...)`; `toSeq(rows.GroupBy(...)).TraverseM(...)`
LINQ `GroupBy` exits to `IEnumerable<IGrouping<...>>`; the existing Prelude lift restores `Seq` directly and deletes an inert intermediate carrier.

## `libs/dotnet/Rasm.AppUi/.planning/Editing/graph.md:322`; `libs/dotnet/Rasm.AppUi/.planning/Editing/graph.md:393`
`from`: `GroupBy(...).AsIterable().ToSeq().Fold(...)`; `GroupBy(...).AsIterable().Filter(...).ToSeq().Head`
`to`: `toSeq(GroupBy(...)).Fold(...)`; `toSeq(GroupBy(...)).Filter(...).Head`
`Prelude.toSeq` directly restores each LINQ grouping run; the `Iterable` detour performs no projection and owns no behavior.

## `libs/dotnet/Rasm.AppUi/.planning/Shell/screens.md:1034`; `libs/dotnet/Rasm.AppUi/.planning/Collab/compare.md:466`; `libs/dotnet/Rasm.AppUi/.planning/Charts/tiles.md:210`
`from`: each `GroupBy(...).AsIterable()...ToSeq()` re-entry chain
`to`: lift each grouping once with `toSeq(GroupBy(...))`, then retain its existing `Filter`/`Map`/`Traverse`
All three `GroupBy` calls return LINQ group enumerables; direct Prelude re-entry preserves grouping/order and removes one carrier conversion per site.

## `libs/dotnet/Rasm.AppUi/.planning/Theme/typography.md:525`; `libs/dotnet/Rasm.AppUi/.planning/Theme/typography.md:1252`; `libs/dotnet/Rasm.AppUi/.planning/Editing/graph.md:632`
`from`: `Enumerable.Range(...).AsIterable().ToSeq()` and each `Enum.GetValues<T>().AsIterable().ToSeq()`
`to`: `toSeq(Enumerable.Range(...))` and `toSeq(Enum.GetValues<T>())`
Each source is already `IEnumerable<T>`; `Prelude.toSeq` is the direct concrete-carrier lift and removes every allocation-free `Iterable` waypoint without changing order.

## `libs/dotnet/Rasm.AppUi/.planning/Charts/grammar.md:277`
`from`: `Layers.Head.Kind.Canvas`
`to`: `Layers[0].Kind.Canvas`
LanguageExt `Seq.Head` returns `Option<ChartLayer>`; `ChartSpec.Admit` already guards non-empty layers before reading `Canvas`, so the indexed read preserves the proved invariant without a fallback.

## `libs/dotnet/Rasm.AppUi/.planning/Charts/streams.md:231`; `libs/dotnet/Rasm.AppUi/.planning/Charts/streams.md:252`; `libs/dotnet/Rasm.AppUi/.planning/Charts/streams.md:261`
`from`: `cell.Rows.Head.X` / `.Stamp` / `.Civil` / `.Datum.Stamp`
`to`: read the same members from `cell.Rows[0]`
`Seq.Head` is an `Option`, while every `cell.Rows` comes from a LINQ `IGrouping`, which is non-empty by construction; indexed reads preserve each first-row value exactly.

## `libs/dotnet/Rasm.AppUi/.planning/Shell/navigation.md:396`
`from`: guarded `vertical.Head.Region.Rank`
`to`: `vertical[0].Region.Rank`
`Seq.Head` returns `Option`; the adjacent `column.IsEmpty` branch proves `vertical` non-empty because `column` is its one-to-one projection, so the index is total on this arm.

## `libs/dotnet/Rasm.AppUi/.planning/Collab/issues.md:263`; `libs/dotnet/Rasm.AppUi/.planning/Collab/issues.md:308`; `libs/dotnet/Rasm.AppUi/.planning/Collab/session.md:203`
`from`: `owner.Keys().AsIterable().Map/Choose(...).ToSeq()`
`to`: `toSeq(owner.Keys()).Map/Choose(...)`
The Loro key rosters are enumerable values; direct Prelude lift preserves their order and the existing `Option` filtering while removing a redundant carrier hop at all three reads.

## `libs/dotnet/Rasm.AppUi/.planning/Collab/issues.md:370`; `libs/dotnet/Rasm.AppUi/.planning/Collab/issues.md:392`
`from`: `topics/Issues.Traverse(...).As().Map(issues => ... issues.ToSeq())`
`to`: pass `issues` directly into `TriageBoard` / the `Issues` record column
Both traversals already return `Seq<Issue>`; the terminal conversions add no shape, order, or failure behavior.

## `libs/dotnet/Rasm.AppUi/.planning/Collab/sync.md:799`
`from`: `...OrderBy(...).AsIterable().Choose(...).ToSeq()`
`to`: `toSeq(...OrderBy(...)).Choose(...)`
The LINQ ordered run has no LanguageExt `Choose`; direct `Prelude.toSeq` restores it once and deletes the `Iterable` round-trip while retaining ordinal order.

## `libs/dotnet/Rasm.AppUi/.planning/Render/immersive.md:845`; `libs/dotnet/Rasm.AppUi/.planning/Render/immersive.md:1613`
`from`: `actions.Traverse(...).Map(bound => bound.ToSeq()).ToFin()`; `Traverse(...).As().Map(rows => rows.ToSeq().ToHashMap(...))`
`to`: `actions.Traverse(...).As().ToFin()`; in the second, call `rows.ToHashMap(...)` directly
LanguageExt `Seq.Traverse` already carries a concrete `Seq`; `As()` lands the first abstract validation carrier, and neither `ToSeq` contributes admission or ordering.

## `libs/dotnet/Rasm.AppUi/.planning/Theme/motion.md:71`; `Theme/assets.md:800`; `Vfx/compose.md:220`; `Collab/sync.md:157`; `Collab/tour.md:55,91,175`; `Document/board.md:82,116`; `Charts/basemap.md:133,239,647,774`; `Render/animation.md:312`; `Render/viewpoint.md:444`; `Render/immersive.md:39`; `Shell/hosts.md:641`; `Editing/history.md:64,73,82`; `Editing/livedata.md:423,1354`; `Editing/inspector.md:410`
`from`: every plain `[ValidationError]` attribute
`to`: delete all 23 attributes
Thinktecture's non-generic `ValidationErrorAttribute` is abstract; only `[ValidationError<T>]` selects a custom error, while these owners already use the generated default `ValidationError` contract.

## `libs/dotnet/Rasm.AppUi/.planning/Theme/tokens.md:915`
`from`: `UnitInterval.TryCreate(scaled - lo, out UnitInterval? amount) ? Mix(..., amount, ...) : Fin.Fail<Color>(...)`
`to`: `Mix(pair.Left, pair.Right, UnitInterval.Create(scaled - lo), Class.Path)`
`Sample` clamps `t`, `lo` is its segment floor, and every colormap has at least two stops, so `scaled - lo` is already proved in `[0,1]`; Thinktecture `Create` consumes that proof and deletes an unreachable failure arm.

## `libs/dotnet/Rasm.AppUi/.planning/Diagnostics/proof.md:292`
`from`: `new ProofFault.ReplayDiverged(indices.Head)`
`to`: `new ProofFault.ReplayDiverged(indices[0])`
LanguageExt `Seq.Head` returns `Option<int>` while the preceding empty arm proves this branch non-empty; the indexed read supplies the exact `int` the existing fault owns.

## `libs/dotnet/Rasm.AppUi/.planning/Diagnostics/governor.md:210`; `libs/dotnet/Rasm.AppUi/.planning/Render/viewpoint.md:541`; `libs/dotnet/Rasm.AppUi/.planning/Render/viewpoint.md:543`
`from`: `Cons(...).Take(...).ToSeq()`; `Keys.Take(...).ToSeq()`; `walked.Skip(...).ToSeq()`
`to`: delete each terminal `ToSeq()`
LanguageExt `Seq.Take` and `Seq.Skip` already return `Seq`; all three conversions are identity work and the existing `Strict`/`Add`/constructor consumers remain unchanged.

## `libs/dotnet/Rasm.AppUi/.planning/Collab/compare.md:283`
`from`: `Range(0, Layout.Panes).AsIterable().ToSeq().Map(...)`
`to`: `toSeq(Enumerable.Range(0, Layout.Panes)).Map(...)`
LanguageExt `Range<A>` is a foldable counter driver with no projection member; the catalog's existing projected-span rail is BCL `Enumerable.Range` admitted once through `Prelude.toSeq`.

## `libs/dotnet/Rasm.AppUi/.planning/Diagnostics/devloop.md:267`
`from`: `IO.lift(() => hooks.Fire(...)).Bind(static settled => IO.lift(settled)).Map(static _ => unit)`
`to`: `IO.lift(() => hooks.Fire(...)).Map(static _ => unit)`
LanguageExt's `IO.lift(Func<Fin<A>>)` already flattens the `Fin` onto the IO error channel; only the final projection from the fired fact to the installer's `Unit` result remains.

## `libs/dotnet/Rasm.AppUi/.planning/Shell/commands.md:710`
`from`: `IO.lift(() => deck.Composition.Hooks.Fire(...)).Bind(static settled => IO.lift(settled))`
`to`: `IO.lift(() => deck.Composition.Hooks.Fire(...))`
The exact LanguageExt overload already turns `Func<Fin<DeckOutcome>>` into `IO<DeckOutcome>`; the second lift is invalid carrier re-entry and contributes no behavior.

## `libs/dotnet/Rasm.AppUi/.planning/Render/pipeline.md:382`
`from`: `this switch { Composited c => ..., Swapchain swapchain => ..., Headless => ... }`
`to`: `Switch(composited: c => ..., swapchain: swapchain => ..., headless: static _ => IO.pure(unit))`
Thinktecture already generates exhaustive dispatch for all three `WgpuPresentation` cases; the existing arm bodies transfer unchanged and no catch-all or new symbol is needed.

## `libs/dotnet/Rasm.AppUi/.planning/Charts/grammar.md:395`; `libs/dotnet/Rasm.AppUi/.planning/Charts/grammar.md:401`; `libs/dotnet/Rasm.AppUi/.planning/Charts/grammar.md:406`
`from`: LiveCharts setters executed inside `Fin.Map`, eagerly inside `Fin.Succ`, and inside `Fin.Succ(unit).Map`
`to`: run each existing mutation block through `Op.Of(...).Catch(() => { ...; return Fin.Succ(result); })`, binding `Mount` after `groups.For(...)`
LanguageExt projection/construction does not admit provider exceptions; Domain `Op.Catch` is the existing exception-to-`Fin` boundary and preserves each success value without a helper or wrapper.

## `libs/dotnet/Rasm.AppUi/.planning/Diagnostics/proof.md:426`; `libs/dotnet/Rasm.AppUi/.planning/Theme/motion.md:240`; `libs/dotnet/Rasm.AppUi/.planning/Theme/emission.md:194,356,365`; `libs/dotnet/Rasm.AppUi/.planning/Editing/graph.md:583,928`; `libs/dotnet/Rasm.AppUi/.planning/Editing/history.md:249`; `libs/dotnet/Rasm.AppUi/.planning/Charts/basemap.md:415`; `libs/dotnet/Rasm.AppUi/.planning/Charts/boards.md:443`; `libs/dotnet/Rasm.AppUi/.planning/Shell/dialogs.md:457,859,862`; `libs/dotnet/Rasm.AppUi/.planning/Shell/screens.md:299`; `libs/dotnet/Rasm.AppUi/.planning/Document/media.md:815`; `libs/dotnet/Rasm.AppUi/.planning/Render/immersive.md:1687`
`from`: bare `IO.lift(() => <Fin<T>>)` at each site where the declared/consumed value remains `IO<Fin<T>>`
`to`: spell `IO.lift<Fin<T>>(() => <Fin<T>>)` with each site's existing concrete `T`
LanguageExt selects the more-specific flattening overload for a bare `Fin` thunk and returns `IO<T>`; its existing explicit type-argument form preserves the intended nested result and the same deferred execution.

## `libs/dotnet/Rasm.AppUi/.planning/Document/media.md:824`; `libs/dotnet/Rasm.AppUi/.planning/Render/capture.md:484`
`from`: `IO.lift(() => Facts.Fire(...)).Bind(static fired => IO.lift(fired)).Map(...)`
`to`: delete `.Bind(static fired => IO.lift(fired))` at both sites
`HookSet.Fire` returns `Fin<AppUiFact>` and LanguageExt's result-typed lift already produces `IO<AppUiFact>`; each remaining `Map` preserves hook failure and projects the original media/artifact value.

## `libs/dotnet/Rasm.AppUi/.planning/Shell/palette.md:297`
`from`: `new PaletteVerdict.Broken(broken.ToSeq())`
`to`: `new PaletteVerdict.Broken(broken)`
The matched `broken` value is already a concrete `Seq<PaletteKind>` from `Seq.Map`; the conversion is identity work.

## `libs/dotnet/Rasm.AppUi/.planning/Shell/navigation.md:935`
`from`: `row.Content switch { Entry => ..., Pane => ..., _ => null }`
`to`: `row.Content.Switch<(Option<BadgeMark>, OverflowMode)?>(entry: ..., pane: ..., chip: _ => null, items: _ => null)`
`ChromeContent` is a Thinktecture union; its generated exhaustive fold preserves both present results and both absent results without a future-case catch-all.

## `libs/dotnet/Rasm.AppUi/.planning/Collab/tour.md:291`
`from`: `seats.Filter(Presenting).AsIterable().Choose(Seated)`
`to`: `seats.Filter(Presenting).Choose(Seated)`
`Session.Seats()` returns `Seq<SessionSeat>` and `Seq.Filter` preserves it, so the existing LanguageExt `Choose` binds directly.
