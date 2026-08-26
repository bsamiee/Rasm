# Rasm.Fabrication LanguageExt / Thinktecture audit

### `libs/dotnet/Rasm.Fabrication/.planning/Kinematics/cell.md:608`
`from`: both tuple-validation branches call `.Apply(...).ToFin()`.
`to`: call `.Apply(...).As().ToFin()` at lines 608-609 and 614-615.
`why`: LanguageExt tuple `Apply` returns `K<Validation<Error>, A>`; `.As()` is required before concrete `ValidationExtensions.ToFin`.

### `libs/dotnet/Rasm.Fabrication/.planning/Tooling/wear.md:955`
`from`: `(rows.Head, rows.Last).Apply(...).ToFin(error)`.
`to`: `(rows.Head, rows.Last).Apply(...).As().ToFin(error)`.
`why`: tuple `Apply` returns `K<Option, A>`; `Option.ToFin(Error)` is available only after the documented `.As()` re-anchor.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/link.md:805`
`from`: repeated `Choose(attempt => attempt.Match(...))` scans split `Seq<Fin<BeamState>>` at lines 805-806 and 821-825.
`to`: deconstruct `var (fails, succs) = attempts.Partition();`, consume those sequences, and pass `fails` directly to `Error.Many`.
`why`: `FinExtensions.Partition` is the existing settled-roster split; it preserves every success and failure while deleting four hand-written folds and the redundant collection spread.

### `libs/dotnet/Rasm.Fabrication/.planning/Verify/removal.md:963`
`from`: `errors.Head.Map(first => errors.Tail.Fold(first, (combined, error) => combined + error))`.
`to`: `errors.Head.Map(_ => Error.Many(errors))`.
`why`: `Error.Many(Seq<Error>)` already owns whole-roster aggregation; the outer `Head.Map` preserves the empty sequence as `None`.

### `libs/dotnet/Rasm.Fabrication/.planning/Verify/probing.md:1000`
`from`: `errors.Head.Match(None: () => Fin.Succ(unit), Some: first => Fin.Fail(errors.Tail.Fold(first, ...)))`.
`to`: `errors.IsEmpty ? Fin.Succ(unit) : Fin.Fail<Unit>(Error.Many(errors))`.
`why`: LanguageExt `Error.Many` replaces the manual head/tail monoid fold while preserving the success arm for no refusals.

### `libs/dotnet/Rasm.Fabrication/.planning/Additive/production.md:1090`
`from`: `Fin.Fail<ThreeMfArtifact>(missing.Tail.Fold(missing.Head.Value!, (faults, fault) => faults + fault))`.
`to`: `Fin.Fail<ThreeMfArtifact>(Error.Many(missing))`.
`why`: the existing `missing.IsEmpty` branch proves non-emptiness; `Error.Many` removes the unsafe `Head.Value!` and manual aggregation.

### `libs/dotnet/Rasm.Fabrication/.planning/Joining/weld.md:1288`
`from`: `None: () => Fin.Succ(Chain(...)).Bind(identity)`.
`to`: `None: () => Chain(...)`.
`why`: `Chain` already returns `Fin<Seq<DepositSegment>>`; wrapping it in `Fin<Fin<_>>` only to flatten it is a behavior-free carrier shell.

### `libs/dotnet/Rasm.Fabrication/.planning/Tooling/cuttingdata.md:1084`
`from`: one `is Solved` conditional and one type switch with an unreachable `_` arm over `StabilityAttempt`.
`to`: project both `bands` and `gaps` with generated exhaustive `row.Map(solved: ..., rejected: ...)` calls.
`why`: the private Thinktecture `[Union]` has exactly `Solved` and `Rejected`; generated `Map` removes the bogus fallback and restores compile-time exhaustiveness.

### `libs/dotnet/Rasm.Fabrication/.planning/Spec/capability.md:1209`
`from`: `ControlConstant.Get(2).RangeMean`.
`to`: `ControlConstant.N2.RangeMean`.
`why`: the same `[SmartEnum<int>]` declares the exact `N2` row at line 238; a known literal should not pass through generated throwing lookup.

### `libs/dotnet/Rasm.Fabrication/.planning/Verify/simulate.md:1164`
`from`: `AdmissionSlots.Accumulate(slots).ToFin().Match(Succ: ..., Fail: ...)`.
`to`: `AdmissionSlots.Accumulate(slots).Match(Fail: _ => new ValidationError("simulate:admission"), Succ: _ => (ValidationError?)null)`.
`why`: `Accumulate` already returns concrete `Validation<Error, Unit>` and its total `Match` preserves both arms without the redundant Validation-to-Fin round trip.

### `libs/dotnet/Rasm.Fabrication/.planning/Fixturing/workholding.md:1130`
`from`: `rows.Choose(identity)` where `rows` is `Seq<Option<ExclusionZone>>`.
`to`: `rows.Somes()`.
`why`: `OptionExtensions.Somes(Seq<Option<A>>)` is the existing one-pass absent-member elimination.

### `libs/dotnet/Rasm.Fabrication/.planning/Forming/brake.md:426`
`from`: `Seq<Option<BrakeRejection>>(...).Choose(identity).ToSeq()`.
`to`: `Seq<Option<BrakeRejection>>(...).Somes()`.
`why`: LanguageExt `Somes` returns the present roster directly, deleting the generic choose and redundant `ToSeq`.

### `libs/dotnet/Rasm.Fabrication/.planning/Forming/brake.md:564`
`from`: `witnesses.Map(...MinimumClearanceMm).Choose(identity).ToSeq()`.
`to`: `witnesses.Map(...MinimumClearanceMm).Somes()`.
`why`: the receiver is `Seq<Option<double>>`; `OptionExtensions.Somes` is its exact flattening surface.

### `libs/dotnet/Rasm.Fabrication/.planning/Forming/brake.md:634`
`from`: `.Map(static rows => rows.Choose(identity).ToSeq())`.
`to`: `.Map(static rows => rows.Somes())`.
`why`: the traversed result is `Seq<Option<double>>`; `Somes` preserves order and present values with less code.

### `libs/dotnet/Rasm.Fabrication/.planning/Joining/sequence.md:1001`
`from`: the `Seq<Option<CandidateRejection>>` literal ends in `.Choose(identity)`.
`to`: end it in `.Somes()`.
`why`: `OptionExtensions.Somes` is the package-owned projection of present members from this exact receiver type.

### `libs/dotnet/Rasm.Fabrication/.planning/Nesting/nfp.md:950`
`from`: `Seq<Option<UnplacedReason>>(...).Choose(identity).Head`.
`to`: `Seq<Option<UnplacedReason>>(...).Somes().Head`.
`why`: LanguageExt `Somes` replaces the hand-selected Option flattening without changing first-present ordering.

### `libs/dotnet/Rasm.Fabrication/.planning/Nesting/nfp.md:1548`
`from`: `.Map(static rows => rows.Choose(identity).ToSeq())` over `Seq<Option<NoFitPolygon>>`.
`to`: `.Map(static rows => rows.Somes())`.
`why`: `OptionExtensions.Somes` already returns the present `Seq<NoFitPolygon>` in traversal order.

### `libs/dotnet/Rasm.Fabrication/.planning/Nesting/stock.md:624`
`from`: `attempts.Choose(identity).Filter(...)` where `attempts` is `Seq<Option<ProviderRun>>`.
`to`: `attempts.Somes().Filter(...)`.
`why`: LanguageExt `Somes` is the exact existing projection and leaves the subsequent placement filter unchanged.

### `libs/dotnet/Rasm.Fabrication/.planning/Nesting/linking.md:668`
`from`: `.Map(static rows => rows.Choose(identity))` over `Seq<Option<WasteRow>>`.
`to`: `.Map(static rows => rows.Somes())`.
`why`: `OptionExtensions.Somes` removes absent waste rows directly with identical order and cardinality.

### `libs/dotnet/Rasm.Fabrication/.planning/Verify/audit.md:1107`
`from`: `found.Choose(identity).Map(...)` where `found` is `Seq<Option<(Length Thickness, Point3d At)>>`.
`to`: `found.Somes().Map(...)`.
`why`: LanguageExt `Somes` already owns present-member extraction and preserves the downstream defect projection.

### `libs/dotnet/Rasm.Fabrication/.planning/Documentation/projection.md:371`
`from`: `policy.Hatching.Find(view.Key).Match(Some: plan => Hatching.Apply(...).Map(Optional), None: () => Fin.Succ(None))`.
`to`: `policy.Hatching.Find(view.Key).TraverseM(plan => Hatching.Apply(new HatchOp.Projection(projection, plan, HatchLane(policy)), HlrOp)).As()`.
`why`: `Option.TraverseM` is the existing absence-total effect inversion; `None` stays a pure `None` and `Hatching.Apply` still runs only for `Some`.

### `libs/dotnet/Rasm.Fabrication/.planning/Fixturing/setups.md:791`
`from`: `Evidence(...).Bind(evidence => evidence.Match(Some: accepted => Commit(...).Map(Some), None: () => Fin.Succ(None)))`.
`to`: `Evidence(...).Bind(evidence => evidence.TraverseM(accepted => Commit(space, state, operation, candidate, accepted)).As())`.
`why`: LanguageExt `Option.TraverseM` preserves the exact `Fin<Option<SetupDraft>>` result and deletes both hand-written Option arms.

### `libs/dotnet/Rasm.Fabrication/.planning/Process/derivation.md:695`
`from`: `topology.Ordered.Map(demand => from start in early.Find(...) from slack in tail.Find(...) select (...)).Traverse(identity)`.
`to`: `topology.Ordered.Traverse(demand => from start in early.Find(...) from slack in tail.Find(...) select (...))`.
`why`: `Seq.Traverse(Func<A,K<F,B>>)` already fuses this non-indexed projection and inversion; the resulting `K<Fin, Seq<_>>` is unchanged.

### `libs/dotnet/Rasm.Fabrication/.planning/Process/derivation.md:847`
`from`: `.Map(row => AdmissionSlots.Gate(...)).Traverse(identity)`.
`to`: `.Traverse(row => AdmissionSlots.Gate(...))`.
`why`: LanguageExt's function-taking `Seq.Traverse` is the same applicative inversion without the intermediate `Seq<Validation<Error,Unit>>` projection.

### `libs/dotnet/Rasm.Fabrication/.planning/Verify/simulate.md:724`
`from`: `Range(...).ToSeq().Map(index => Flatten(...)).TraverseM(identity).As()`.
`to`: `Range(...).ToSeq().TraverseM(index => Flatten(...)).As()`.
`why`: `Seq.TraverseM` directly owns non-indexed monadic projection and preserves the short-circuiting `Fin<Seq<_>>` result.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/skeleton.md:308`
`from`: `Range(...).ToSeq().Map(component => Component(...)).TraverseM(identity).As()`.
`to`: `Range(...).ToSeq().TraverseM(component => Component(...)).As()`.
`why`: LanguageExt's function-taking `TraverseM` removes the intermediate `Seq<Fin<CutPass>>` without changing order or failure semantics.

### `libs/dotnet/Rasm.Fabrication/.planning/Posting/conditioning.md:347`
`from`: `placement.Parts.Map(transform => profiles.Find(...).ToFin(...).Bind(transform.Apply)).TraverseM(identity).As()`.
`to`: `placement.Parts.TraverseM(transform => profiles.Find(...).ToFin(...).Bind(transform.Apply)).As()`.
`why`: the catalogued `Seq.TraverseM` overload fuses this non-indexed effect projection and returns the same `Fin<Seq<Loop>>`.

### `libs/dotnet/Rasm.Fabrication/.planning/Joining/procedure.md:821`
`from`: `Unreleased(releases).Map(hold => AdmissionSlots.Gate(...)).Traverse(identity)`.
`to`: `Unreleased(releases).Traverse(hold => AdmissionSlots.Gate(...))`.
`why`: direct `Seq.Traverse` retains Validation accumulation for every unreleased hold while deleting the intermediate effect roster.

### `libs/dotnet/Rasm.Fabrication/.planning/Joining/procedure.md:1041`
`from`: `request.Demands.Map(demand => AssessDemand(request, demand).ToValidation()).Traverse(identity)`.
`to`: `request.Demands.Traverse(demand => AssessDemand(request, demand).ToValidation())`.
`why`: LanguageExt `Seq.Traverse(f)` is the existing applicative projection-and-inversion surface and preserves accumulated `Validation<Error,_>` failures.

### `libs/dotnet/Rasm.Fabrication/.planning/Joining/procedure.md:1071`
`from`: `wps.Profile.Variables.Filter(...).Map(variable => Admit(...).ToValidation()).Traverse(identity)`.
`to`: `wps.Profile.Variables.Filter(...).Traverse(variable => Admit(...).ToValidation())`.
`why`: the receiver is `Seq<EssentialVariable>` and the lambda is non-indexed; direct `Traverse` preserves the same accumulated qualification failures.

### `libs/dotnet/Rasm.Fabrication/.planning/Joining/sequence.md:856`
`from`: `plan.Passes.Bind(...).Map(row => row.Segment.Window(...).Map(...)).Traverse(identity).As()`.
`to`: `plan.Passes.Bind(...).Traverse(row => row.Segment.Window(...).Map(...)).As()`.
`why`: direct LanguageExt `Traverse` preserves the `Fin<Seq<WeldSegment>>` result and removes the redundant mapped carrier.

### `libs/dotnet/Rasm.Fabrication/.planning/Joining/weld.md:1035`
`from`: `toSeq(request.Joints.OrderBy(...)).Map(joint => PlanJoint(...).ToValidation()).Traverse(identity)`.
`to`: `toSeq(request.Joints.OrderBy(...)).Traverse(joint => PlanJoint(...).ToValidation())`.
`why`: direct LanguageExt `Traverse` preserves ordered applicative accumulation and removes the intermediate effect-valued `Seq`.

### `libs/dotnet/Rasm.Fabrication/.planning/Joining/weld.md:1123`
`from`: `policy.Access.Map(constraint => constraint.Check(joint, passes)).Traverse(identity)`.
`to`: `policy.Access.Traverse(constraint => constraint.Check(joint, passes))`.
`why`: `policy.Access` is a `Seq` and `Seq.Traverse(f)` returns the same accumulated Validation roster without the effect-valued `Map`.

### `libs/dotnet/Rasm.Fabrication/.planning/Joining/weld.md:1137`
`from`: `passes.Map(pass => AdmissionSlots.Gate(...)).Traverse(identity)`.
`to`: `passes.Traverse(pass => AdmissionSlots.Gate(...))`.
`why`: direct LanguageExt `Traverse` retains all cooling-band refusals and deletes the intermediate `Seq<Validation<Error,Unit>>`.

### `libs/dotnet/Rasm.Fabrication/.planning/Joining/weld.md:1306`
`from`: `run.Zip(run.Tail).Map(pair => Move.Linear.Of(...).Map(cut => new DepositSegment(...))).Traverse(identity)`.
`to`: `run.Zip(run.Tail).Traverse(pair => Move.Linear.Of(...).Map(cut => new DepositSegment(...)))`.
`why`: the zipped receiver is a `Seq` and direct `Traverse` preserves station order and Fin refusal behavior with one less carrier projection.

### `libs/dotnet/Rasm.Fabrication/.planning/Joining/weld.md:1434`
`from`: `policy.DemandBindings.Map(binding => Op.Of(...).Catch(...).Map(...).ToValidation()).Traverse(identity)`.
`to`: `policy.DemandBindings.Traverse(binding => Op.Of(...).Catch(...).Map(...).ToValidation())`.
`why`: LanguageExt's function-taking traversal preserves Validation accumulation across all bindings and removes the intermediate effect roster.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/turning.md:1148`
`from`: `spans.Map(span => from intent in Intent(...) from load in demand.Cutting.Evaluate(intent) select load).TraverseM(identity).As()`.
`to`: `spans.TraverseM(span => from intent in Intent(...) from load in demand.Cutting.Evaluate(intent) select load).As()`.
`why`: LanguageExt `Seq.TraverseM(f)` directly owns the short-circuiting projection and preserves span order and the first refusal.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/motion.md:1113`
`from`: `stations.Tail.Map(station => Move.Circular.Of(...)).TraverseM(identity)`.
`to`: `stations.Tail.TraverseM(station => Move.Circular.Of(...)).As()`.
`why`: LanguageExt's function-taking `Seq.TraverseM` returns the same ordered Fin result without a redundant effect roster.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/motion.md:1405`
`from`: `Range(...).ToSeq().Map(index => LayerMove(...)).TraverseM(identity).As()`.
`to`: `Range(...).ToSeq().TraverseM(index => LayerMove(...)).As()`.
`why`: direct `TraverseM` preserves the indexed value passed by the captured range and the same first Fin refusal; no indexed traversal overload is required.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/motion.md:1512`
`from`: `Range(...).ToSeq().Map(index => Move.Linear.Of(...)).TraverseM(identity).As()`.
`to`: `Range(...).ToSeq().TraverseM(index => Move.Linear.Of(...)).As()`.
`why`: LanguageExt `Seq.TraverseM(f)` preserves perimeter order and Fin behavior while removing the intermediate effect-valued `Seq`.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/surface.md:705`
`from`: `path.ToSeq().Map(point => Move.Linear.Of(...)).TraverseM(identity).As()`.
`to`: `path.ToSeq().TraverseM(point => Move.Linear.Of(...)).As()`.
`why`: the existing `Seq.TraverseM` overload fuses this non-indexed projection with identical order and Fin failure behavior.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/wire.md:612`
`from`: `program.Blocks.Map(block => block.Action.Switch(...)).TraverseM(identity).As()`.
`to`: `program.Blocks.TraverseM(block => block.Action.Switch(...)).As()`.
`why`: each generated `Switch` arm already returns `Fin<Move>`; LanguageExt `TraverseM(f)` removes the redundant effect-valued `Map`.

### `libs/dotnet/Rasm.Fabrication/.planning/Posting/conditioning.md:479`
`from`: `Range(...).ToSeq().Map(index => from from in Sampled(...) from to in Sampled(...) select new PathSegment(...)).TraverseM(identity).As()`.
`to`: `Range(...).ToSeq().TraverseM(index => from from in Sampled(...) from to in Sampled(...) select new PathSegment(...)).As()`.
`why`: direct LanguageExt `TraverseM` preserves station order and the first sampling refusal while deleting the intermediate Fin roster.

### `libs/dotnet/Rasm.Fabrication/.planning/Documentation/report.md:1178`
`from`: `readings.Map(reading => from subject in ... from measurement in ... from row in ... select row).Traverse(identity).As()`.
`to`: `readings.Traverse(reading => from subject in ... from measurement in ... from row in ... select row).As()`.
`why`: LanguageExt `Seq.Traverse(f)` directly owns this non-indexed Fin projection and produces the same ordered admitted rows.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/motion.md:1518`
`from`: `moves.Map(move => move.Switch(...)).TraverseM(identity).As()`.
`to`: `moves.TraverseM(move => move.Switch(...)).As()`.
`why`: every generated `Move.Switch` arm returns `Fin<Move>`; direct traversal preserves exhaustive dispatch and Fin behavior without an effect-valued `Map`.

### `libs/dotnet/Rasm.Fabrication/.planning/Additive/production.md:1608`
`from`: `parts.Map((part, index) => part.Measured.Support.Map(support => Lattice(index, support)).Sequence()).Sequence().As()`.
`to`: `parts.Map((part, index) => part.Measured.Support.TraverseM(support => Lattice(index, support)).As()).Traverse(identity).As()`.
`why`: `Sequence` leaves both inner shapes abstract; `Option.TraverseM` plus `Seq.Traverse` lands `Fin<Seq<Option<ThreeMfResource.BeamLattice>>>` while retaining the indexed projection.

### `libs/dotnet/Rasm.Fabrication/.planning/Additive/support.md:874`
`from`: `nearest.Ordered.Head.Match(Some: slot => Fin.Succ((child.Id, Seq(lower[slot].Id))), None: () => Fin.Fail(...))`.
`to`: `nearest.Ordered.Head.ToFin(new KernelFault.InvalidValue("support", "support:parent-absent")).Map(slot => (child.Id, Seq(lower[slot].Id)))`.
`why`: LanguageExt `Option.ToFin(Error)` already owns this exact absence refusal; the following `Map` preserves the present projection and deletes both manual arms.

### `libs/dotnet/Rasm.Fabrication/.planning/Posting/optimization.md:331`
`from`: unused Mapperly methods `Smooth`, `Compact`, `Pattern`, `Feed`, and `Stability`, plus `Segments` calling throwing `PatternLength.Create` before lines 458-461 enter Validation.
`to`: delete the Mapperly import/package claim, all annotations, five generated methods, and `Segments`; retain `Mm`/`Rad`/`Fraction` plainly and query over two `Op.Of().AcceptValidated<PatternLength>(row.*Length)` calls before `PatternPolicy.Validate`.
`why`: none of the five mapper methods has a caller, while Thinktecture admission plus `Rasm.Domain.OpExtensions` keeps invalid lengths on Fin; six module methods and the dead generation plane disappear.

### `libs/dotnet/Rasm.Fabrication/.planning/Tooling/magazine.md:1143`
`from`: `Optional(ProcessRange.Create(minimum, maximum, nominal, current)).ToFin(Asset(...))`.
`to`: `ProcessRange.TryCreate(minimum, maximum, nominal, current, out ProcessRange range) ? Fin.Succ(range) : Fin.Fail<ProcessRange>(Asset(...))`.
`why`: generated `Create` can throw and a struct is never absent, so `Optional` cannot reach the refusal; Thinktecture's existing `TryCreate` preserves the intended asset fault without a wrapper.

### `libs/dotnet/Rasm.Fabrication/.planning/Documentation/report.md:1324`
`from`: module method `QualityEvidence.Gate(condition, reason) => AdmissionSlots.Gate(condition, Refused(reason))` used only at lines 1172-1174.
`to`: call `AdmissionSlots.Gate(condition, QualityEvidence.Refused(reason))` at those three slots and delete `QualityEvidence.Gate`.
`why`: `Rasm.Domain.AdmissionSlots` already owns the gate; inlining the retained local fault projection removes one module method and two fenced lines.

### `libs/dotnet/Rasm.Fabrication/.planning/Process/derivation.md:157`
`from`: three carrier runs chain `GroupBy(...).Map/Filter(...)` at lines 157-160, 390-394, and 574-577.
`to`: wrap each complete `...GroupBy(...)` with `toSeq(...)`; also use `toSeq(group).Fold(...)` at line 159 and `toSeq(group).Map(...)` at line 576.
`why`: `GroupBy` returns `IEnumerable<IGrouping<...>>`; `Prelude.toSeq` is the exact re-entry before carrier `Map`, `Filter`, `Bind`, `Fold`, or `Head`.

### `libs/dotnet/Rasm.Fabrication/.planning/Process/family.md:1150`
`from`: `family._graph.TopologicalSort().ToSeq()`.
`to`: `toSeq(family._graph.TopologicalSort())`.
`why`: `TopologicalSort()` returns `IEnumerable<T>` and LanguageExt publishes no `IEnumerable<T>.ToSeq()` extension; `Prelude.toSeq` is the existing boundary re-entry.

### `libs/dotnet/Rasm.Fabrication/.planning/Posting/program.md:903`
`from`: both regex projections use `Matches(...).Select(...).ToSeq()` at lines 903 and 912.
`to`: use `toSeq(Matches(...).Select(...))` for both projections.
`why`: LINQ `Select` returns `IEnumerable<string>` and `.ToSeq()` has no enumerable overload; `Prelude.toSeq` preserves the same order and materialization.

### `libs/dotnet/Rasm.Fabrication/.planning/Additive/production.md:573`
`from`: `model.Faces...GroupBy(...).Map(...).OrderByDescending(...).Take(cap).Map(...).ToSeq()`.
`to`: after `GroupBy`, use LINQ `Select` for both projections and wrap the complete ordered/taken run once with `toSeq(...)`.
`why`: `GroupBy` and `OrderByDescending` are enumerable exits; neither carrier `Map` nor `.ToSeq()` binds them, while one `Prelude.toSeq` preserves grouping, ranking, and cap.

### `libs/dotnet/Rasm.Fabrication/.planning/Fixturing/workholding.md:397`
`from`: `toSeq(Metrics).OrderBy(...).ToSeq()` and `toSeq(Active).OrderBy(...).ToSeq()` at lines 397 and 682.
`to`: use `toSeq(toSeq(Metrics).OrderBy(...))` and `toSeq(toSeq(Active).OrderBy(...))`.
`why`: `OrderBy` returns `IOrderedEnumerable<T>` and LanguageExt has no enumerable `.ToSeq()`; outer `Prelude.toSeq` is the documented re-entry.

### `libs/dotnet/Rasm.Fabrication/.planning/Process/atoms.md:1040`
`from`: `toSeq(Metrics).OrderBy(...).ToSeq()`.
`to`: `toSeq(toSeq(Metrics).OrderBy(...))`.
`why`: ordering exits `Seq` to `IOrderedEnumerable`; only `Prelude.toSeq` re-enters, so the current terminal `.ToSeq()` does not bind.

### `libs/dotnet/Rasm.Fabrication/.planning/Fixturing/assembly.md:381`
`from`: four ordered runs end in `.ToSeq()` at lines 381, 383, 414-415, and 1115.
`to`: wrap each complete existing `toSeq(...).OrderBy(...).ThenBy(...)` expression with an outer `toSeq(...)` and delete its terminal `.ToSeq()`.
`why`: LanguageExt's catalog names ordering as an enumerable exit and `Prelude.toSeq` as the sole re-entry; order and rows remain identical.

### `libs/dotnet/Rasm.Fabrication/.planning/Verify/estimation.md:848`
`from`: `toSeq(CommercialLoad.Items).OrderBy(...).ToSeq()`.
`to`: `toSeq(toSeq(CommercialLoad.Items).OrderBy(...))`.
`why`: the generated `Items` roster is enumerable and `OrderBy` stays enumerable; `Prelude.toSeq` replaces the nonexistent terminal extension without changing rank order.

### `libs/dotnet/Rasm.Fabrication/.planning/Verify/probing.md:1116`
`from`: `toSeq(Index(...)).OrderBy(...).ToSeq().TraverseM(...)`.
`to`: `toSeq(toSeq(Index(...)).OrderBy(...)).TraverseM(...)`.
`why`: `OrderBy` exits the carrier and `.ToSeq()` cannot re-enter an enumerable; `Prelude.toSeq` restores the ordered `Seq` before monadic traversal.

### `libs/dotnet/Rasm.Fabrication/.planning/Additive/implicit.md:1128`
`from`: `source.Match(None: () => Fin.Succ(None), Some: factory => factory().Map(Some))`.
`to`: `source.TraverseM(static factory => factory()).As()` inside the existing `Op.Catch`.
`why`: LanguageExt `Option.TraverseM` is the exact absence-total `Option<Fin<T>>` inversion and deletes both hand-written carrier arms.

### `libs/dotnet/Rasm.Fabrication/.planning/Additive/slicing.md:1190`
`from`: `policy.Match(None: () => Fin.Succ(None), Some: row => Support.Grow(stack, row).Map(Some))`.
`to`: `policy.TraverseM(row => Support.Grow(stack, row)).As()`.
`why`: `Option.TraverseM` preserves `None` without running `Grow` and yields the same `Fin<Option<SupportPlan>>` directly.

### `libs/dotnet/Rasm.Fabrication/.planning/Verify/simulate.md:941`
`from`: `raw.Match(None: () => Fin.Succ(None), Some: _ => Ordinal(raw, locus, admitted).Map(Some))`.
`to`: `raw.TraverseM(value => Ordinal(Some(value), locus, admitted)).As()`.
`why`: LanguageExt `Option.TraverseM` preserves absence and runs the existing admission only for a present scalar, deleting the manual result reconstruction.

### `libs/dotnet/Rasm.Fabrication/.planning/Tooling/wear.md:807`
`from`: `result.Diagnostics.Map(...).Max().Match(...)`.
`to`: branch on `result.Diagnostics.Head`, and in the present arm call the unambiguous selector overload `result.Diagnostics.Max(static row => row.RootMeanSquareResidual)`.
`why`: the LanguageExt catalog marks unseeded carrier `.Max()` ambiguous with LINQ; the selector overload preserves the empty no-write arm and the same maximum.

### `libs/dotnet/Rasm.Fabrication/.planning/Additive/production.md:1134`
`from`: `Placements(...).Choose(PlacementRefusal).Head.Case is string refusal`.
`to`: `Placements(...).Choose(PlacementRefusal).Head is { IsSome: true, Case: string refusal }`.
`why`: LanguageExt requires an `IsSome` proof with a `Case` read; the property pattern preserves the present-string branch without unsafe Option access.

### `libs/dotnet/Rasm.Fabrication/.planning/Process/atoms.md:419`
`from`: `Basis(forward).Match(Some: basis => Validate(...).Admitted(...), None: () => Fin.Fail(...))`.
`to`: `Basis(forward).ToFin(the existing DegenerateInput).Bind(basis => Validate(...).Admitted(...))`.
`why`: `Option.ToFin(Error)` already owns the exact absence refusal, and `Bind` preserves dependent generated admission with fewer arms.

### `libs/dotnet/Rasm.Fabrication/.planning/Geometry2D/arcs.md:559`
`from`: the accumulated nearest `Option` is manually matched to `Fin.Succ<ArcTrace>(...)` or `Fin.Fail<ArcTrace>(...)`.
`to`: call `.ToFin(the existing DegenerateInput).Map(row => new ArcTrace.Inspection(...))`.
`why`: `Option.ToFin` plus `Map` preserves the same empty refusal and present projection while deleting both manual result constructors.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/link.md:829`
`from`: `state.Current.Match(Some: current => Transition(...), None: () => Fin.Fail(Blocked(...)))`.
`to`: `state.Current.ToFin(Blocked(job, Seq(state))).Bind(current => Transition(...))` with the existing success projection retained.
`why`: LanguageExt `Option.ToFin` is the existing absence-to-error bridge and `Bind` preserves the dependent transition.

### `libs/dotnet/Rasm.Fabrication/.planning/Additive/slicing.md:1195`
`from`: `input.Model.Match(None: () => Fin.Fail(...), Some: model => Slicing.Apply(...))`.
`to`: `input.Model.ToFin(the existing model-missing fault).Bind(model => Slicing.Apply(...))`.
`why`: `Option.ToFin` and `Bind` encode the same required-model dependency without a hand-written carrier fold.

### `libs/dotnet/Rasm.Fabrication/.planning/Nesting/nfp.md:1239`
`from`: under `collision.Count == 1`, `collision.Head.Match(Some: envelope => Fin.Succ(new Variant(...)), None: () => Fin.Fail(fault))`.
`to`: use `collision.Head.ToFin(fault).Map(envelope => new Variant(...))` in that branch.
`why`: `Option.ToFin` preserves the topology refusal and `Map` preserves construction, deleting the redundant success/failure shell.

### `libs/dotnet/Rasm.Fabrication/.planning/Posting/dialect.md:711`
`from`: the admitted WCS value is matched to either `Base`/`Extended` or `Fin.Fail<GWord>(DialectUnsupported)`.
`to`: call `.ToFin(the existing DialectUnsupported).Bind(value => command == ... ? Extended(...) : Base(...))`.
`why`: `Option.ToFin` already owns required-presence admission and `Bind` keeps the command-dependent effect unchanged.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/surface.md:332`
`from`: `policy.Layout.Match(Some: layout => Op.Catch(...), None: () => Fin.Fail(...))`.
`to`: `policy.Layout.ToFin(the existing unbound-layout fault).Bind(layout => Op.Of(...).Catch(...))`.
`why`: LanguageExt `Option.ToFin` preserves the required delegate refusal and removes the hand-written result fold without adding an owner.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/surface.md:909`
`from`: both `Paths` and `Fibers` match `run.Drives` to a computation or the same required-drives failure at lines 909-916 and 965-980.
`to`: start each body with `run.Drives.ToFin(the existing path/fiber fault).Bind(set => ...)` and retain its present computation.
`why`: `Option.ToFin` is the exact existing required-presence bridge; both rewrites delete duplicate `Match` scaffolding and preserve each distinct fault.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/motion.md:1111`
`from`: `stations.Head.Match(Some: first => Move.Rapid.Of(...).Bind(...), None: () => Fin.Fail(...))`.
`to`: `stations.Head.ToFin(the existing helix-stations fault).Bind(first => Move.Rapid.Of(...).Bind(...))`.
`why`: LanguageExt `Option.ToFin` preserves the empty-station refusal and dependent entry/arc construction with no manual carrier arms.

### `libs/dotnet/Rasm.Fabrication/.planning/Ingress/element.md:648`
`from`: `baked.Representations.At(part.Slot).Map(...).ToFin(fault).ToValidation()`.
`to`: `baked.Representations.At(part.Slot).Map(...).ToValidation(fault)`.
`why`: LanguageExt `Option.ToValidation(L)` is the direct accumulating ingress and preserves the same absent fault without the intermediate `Fin`.

### `libs/dotnet/Rasm.Fabrication/.planning/Joining/sequence.md:835`
`from`: `toSeq(components).GroupBy(...).Filter(...).Bind(...).ToArr()`.
`to`: `toSeq(toSeq(components).GroupBy(...)).Filter(...).Bind(...).ToArr()`.
`why`: `GroupBy` exits to `IEnumerable<IGrouping<...>>`; `Prelude.toSeq` is the existing re-entry before carrier `Filter` and `Bind`, with grouping unchanged.

### `libs/dotnet/Rasm.Fabrication/.planning/Posting/optimization.md:842`
`from`: `state.Rows.Last.Case is not GNode.Word previous`.
`to`: `state.Rows.Last is not { IsSome: true, Case: GNode.Word previous }`.
`why`: LanguageExt requires an `IsSome` proof for `Option.Case`; the property pattern preserves the empty-or-non-word fallback.

### `libs/dotnet/Rasm.Fabrication/.planning/Nesting/remnant.md:805`
`from`: the traversal arms construct `Fin.Succ(unit).ToValidation()` and `Fin.Fail<Unit>(error).ToValidation()`.
`to`: construct `Validation<Error, Unit>.Success(unit)` and `.Fail(error)` directly.
`why`: LanguageExt already exposes the exact target carrier factories; both arms retain Validation accumulation while deleting four carrier conversions.

### `libs/dotnet/Rasm.Fabrication/.planning/Additive/production.md:1218`
`from`: `meshes.Map(static (mesh, index) => ...)` where `meshes` is `Arr<CMeshObject>`.
`to`: `toSeq(meshes).Map(static (mesh, index) => ...)`.
`why`: LanguageExt `Arr<A>.Map` has no indexed overload; `Prelude.toSeq` supplies the indexed `Seq.Map` without changing order or build-item identity.

### `libs/dotnet/Rasm.Fabrication/.planning/Spec/tolerance.md:504`
`from`: indexed `Modifiers.Map(...)` and `Datums.Map(...)` where both properties return `Arr<A>`.
`to`: use `toSeq(Modifiers).Map(...)` and `toSeq(Datums).Map(...)`, deleting both terminal `.ToSeq()` calls.
`why`: `Arr<A>` publishes only unary `Map`; `Prelude.toSeq` restores the ordinal projection and already returns the concatenated carrier.

### `libs/dotnet/Rasm.Fabrication/.planning/Process/derivation.md:109`
`from`: `predecessors.Distinct().Count` and `assigned.Distinct().Count` at lines 109 and 766, both on `Arr<A>`.
`to`: use `toSeq(predecessors).Distinct().Count` and `toSeq(assigned).Distinct().Count`.
`why`: `Arr<A>` has no `Distinct`; `Prelude.toSeq` binds the carrier operation and preserves both uniqueness tests.

### `libs/dotnet/Rasm.Fabrication/.planning/Process/derivation.md:749`
`from`: `policy.Match(Some: plan => ... Fin<Option<SetupSchedule>>, None: () => Fin.Succ(None))`.
`to`: `policy.TraverseM(plan => ... returning Fin<SetupSchedule>).As()` inside the existing `topology.IsEmpty` branch.
`why`: LanguageExt `Option.TraverseM` preserves absent policy and runs `SetupSchedule.Apply` only for `Some`, deleting manual `Option` reconstruction.

### `libs/dotnet/Rasm.Fabrication/.planning/Process/atoms.md:1185`
`from`: `operations.Distinct().Count` where `operations` is `Arr<int>`.
`to`: `toSeq(operations).Distinct().Count`.
`why`: LanguageExt `Arr<A>` has no `Distinct`; the existing `Prelude.toSeq` re-entry preserves the generated owner's duplicate-operation gate.

### `libs/dotnet/Rasm.Fabrication/.planning/Process/faults.md:109`
`from`: `row.Predecessors.Distinct().Count` where `DeriveWitness.LotInadmissible.Predecessors` is `Arr<UInt128>`.
`to`: `toSeq(row.Predecessors).Distinct().Count`.
`why`: `Arr<A>` cannot bind `Distinct`; re-entering through `Prelude.toSeq` keeps the witness predicate aligned with `LotPolicy` admission.

### `libs/dotnet/Rasm.Fabrication/.planning/Tooling/magazine.md:454`
`from`: `edges.Map(static edge => edge.Key).Distinct().Count` where `edges` is `Arr<ToolEdge>`.
`to`: `toSeq(edges).Map(static edge => edge.Key).Distinct().Count`.
`why`: unary `Arr.Map` returns another `Arr`, which has no `Distinct`; `Prelude.toSeq` preserves edge order and duplicate-key semantics.

### `libs/dotnet/Rasm.Fabrication/.planning/Spec/tolerance.md:334`
`from`: `references`, `basics`, and `targets` use `Arr.Map(...).Distinct().Count` at lines 337-338 and 375-376.
`to`: begin each projection with `toSeq(references)`, `toSeq(basics)`, or `toSeq(targets)` before `Map(...).Distinct().Count`.
`why`: every receiver is `Arr<A>`, whose unary `Map` still returns an `Arr` with no `Distinct`; `Prelude.toSeq` preserves all four uniqueness gates.

### `libs/dotnet/Rasm.Fabrication/.planning/Fixturing/setups.md:847`
`from`: `Holding`, `Clearance`, and `Machined` construct known `WorkholdingOp` cases, then downcast `WorkholdingResult` and throw for every other arm.
`to`: call `Fixtures.Restrain(..., SafetyFactor.As(RatioUnit.DecimalFraction))`; map `Fixtures.Clear(...)` to `new WorkholdingResult.Clearance(blocked)`; call `Fixtures.Machined(...)` in the existing stock `Match`.
`why`: `Workholding.Apply` dispatches those exact cases to these existing typed methods; bypassing its construct/project round trips preserves each `Fin` result and deletes three impossible-result switches.

### `libs/dotnet/Rasm.Fabrication/.planning/Fixturing/assembly.md:815`
`from`: each known `WorkholdingOp.Clear` is sent through `Workholding.Apply` and an otherwise-throwing `WorkholdingResult` switch.
`to`: call `Fixtures.Clear(fixture, FixtureState.Clamp, Corridor(...)).Map(static blocked => new WorkholdingResult.Clearance(blocked)).ToValidation()`.
`why`: the `clear` arm of `Workholding.Apply` is exactly that typed call and projection; direct composition preserves accumulated clearance failures and removes the impossible fallback.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/motion.md:970`
`from`: `Workholding.Apply(new WorkholdingOp.Condition(...))` followed by a `Conditioned` type test and synthetic `cam:workholding` mismatch fault.
`to`: `Fixtures.Condition(run.Mounts.Fixture, run.Mounts.State, moves)`.
`why`: `Workholding.Apply` delegates its `condition` arm to this existing typed owner; the direct result is the same `Fin<Seq<Move>>` without a union object or unreachable mismatch branch.

### `libs/dotnet/Rasm.Fabrication/.planning/Posting/conditioning.md:319`
`from`: `Workholding.Apply(new WorkholdingOp.Condition(...))` followed by a `Conditioned` type test and synthetic `post:workholding-result` fault.
`to`: bind `moves` directly from `Fixtures.Condition(policy.Setup.Workholding.Fixture, policy.Setup.Workholding.State, motion.Moves)`.
`why`: the public dispatcher already routes this known case to `Fixtures.Condition`; direct binding preserves its `Fin` and deletes the construct/project shell and impossible fault.

### `libs/dotnet/Rasm.Fabrication/.planning/Joining/sequence.md:875`
`from`: `segments.Bind(...).Map(row => row.Segment.Source.Window(...).Map(...)).Traverse(identity).As()`.
`to`: `segments.Bind(...).Traverse(row => row.Segment.Source.Window(...).Map(...)).As()`.
`why`: LanguageExt `Seq.Traverse(f)` fuses the non-indexed Fin projection and inversion, preserving subdivision order and refusal behavior without the intermediate effect roster.

### `libs/dotnet/Rasm.Fabrication/.planning/Ingress/solid.md:333`
`from`: `Optional(face.GetMesh(...)).ToFin(error).Map(SolidImport.FromThreeDm).ToValidation()`.
`to`: `Optional(face.GetMesh(...)).Map(SolidImport.FromThreeDm).ToValidation(error)`.
`why`: LanguageExt `Option.ToValidation(Error)` owns the same absent-face refusal directly, preserving per-face accumulation while removing the Option-to-Fin-to-Validation round trip.

### `libs/dotnet/Rasm.Fabrication/.planning/Nesting/remnant.md:518`
`from`: the parent `Option` chain ends in `.ToFin(orphanError).Map(owner => (...)).ToValidation()`.
`to`: end it in `.Map(owner => (...)).ToValidation(orphanError)`.
`why`: LanguageExt `Option.ToValidation(Error)` preserves the same missing/filtered-parent failure and present tuple while deleting the intermediate `Fin` carrier.

### `libs/dotnet/Rasm.Fabrication/.planning/Fixturing/assembly.md:942`
`from`: both independent `Map.Find` results call `.ToFin(Absent(...)).ToValidation()` before tuple `Apply`.
`to`: call `.ToValidation(Absent(...))` on each `Option` directly.
`why`: LanguageExt `Option.ToValidation(Error)` supplies the same two independently accumulated absence faults without either redundant `Fin` conversion.

### `libs/dotnet/Rasm.Fabrication/.planning/Fixturing/setups.md:985`
`from`: each `Evidence(...)` result binds its `Option`, calls `.ToFin(SetupInfeasible(...))`, then `.ToValidation()`.
`to`: `Evidence(...).Bind(evidence => evidence.ToValidation(new FabricationFault.SetupInfeasible(Some(row.Key), schedule.Setups.Count)))`.
`why`: LanguageExt `Option.ToValidation(Error)` preserves the same missing-evidence fault and outer traversal accumulation without the intermediate `Fin` carrier.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/motion.md:955`
`from`: the no-robot arm manually matches `run.Mounts.Kinematics` to `MachineTool.Solve(...)` or the existing motion-evidence fault.
`to`: `run.Mounts.Kinematics.ToFin(the existing motion-evidence fault).Bind(kinematics => MachineTool.Solve(kinematics, guarded).Map(static solution => solution.Motion))`.
`why`: LanguageExt `Option.ToFin(Error)` preserves the required-kinematics refusal and dependent solve while deleting the inner hand-written carrier fold.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/motion.md:1232`
`from`: `run.Engagement.Bevel.Match(Some: policy => the existing Fin query, None: () => Fin.Fail(the existing unpolicied fault))`.
`to`: `run.Engagement.Bevel.ToFin(the existing unpolicied fault).Bind(policy => the existing Fin query)`.
`why`: LanguageExt `Option.ToFin(Error)` preserves required-policy admission and the dependent budget/profile work without the manual Option result fold.

### `libs/dotnet/Rasm.Fabrication/.planning/Posting/program.md:956`
`from`: nonempty tokens still manually match `segments.Head` to the existing `FoldM` or `ProgramParse` failure.
`to`: `segments.Head.ToFin(new FabricationFault.ProgramParse(line, ModalGroup.NonModal)).Bind(head => the existing FoldM expression)`.
`why`: LanguageExt `Option.ToFin(Error)` preserves the empty-segment refusal and dependent parse fold while removing both hand-written carrier arms.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/motion.md:942`
`from`: both `Error.Many([.. open])` and `Error.Many([.. walked.Faults])` spread existing `Seq<Error>` values at lines 942 and 980.
`to`: use `Error.Many(open)` and `Error.Many(walked.Faults)`.
`why`: LanguageExt `Error.Many(Seq<Error>)` accepts both carriers directly; removing the collection copies preserves fault order and membership.

### `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/turning.md:759`
`from`: `Error.Many([.. gouges])` spreads an existing `Seq<Error>`.
`to`: `Error.Many(gouges)`.
`why`: LanguageExt's exact `Error.Many(Seq<Error>)` overload preserves the gouge roster while deleting the redundant collection copy.
