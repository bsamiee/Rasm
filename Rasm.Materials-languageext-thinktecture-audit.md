# Rasm.Materials LanguageExt / Thinktecture audit

## `libs/dotnet/Rasm.Materials/.planning/Appearance/graph.md:45`
`from`: `PortId.Of(int) => Create(value)` plus `PortId.Of` at lines 348/393 and `Raster/set.md:85`.
`to`: delete `PortId.Of`; call generated `PortId.Create` at all three consumers.
`why`: Thinktecture `[ValueObject<int>]` already generates `Create`; the wrapper changes no admission behavior and only adds a module symbol.

## `libs/dotnet/Rasm.Materials/.planning/Raster/set.md:293`
`from`: `UdimTile.Of(int)` and `UdimTile.Admit(int)`; token parsing calls `Admit` at line 701.
`to`: delete both; parse with `int.TryParse(...) && UdimTile.TryCreate(value, out UdimTile tile) ? Some(tile) : None`.
`why`: Thinktecture generates both `Create` and `TryCreate`; `Of` is unused and `Admit` merely discards the same generated refusal into `Option`.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/texture.md:356`
`from`: `NoisePeriod.Of(int) => Create(value)` and its sole call at line 845.
`to`: delete `NoisePeriod.Of`; call generated `NoisePeriod.Create(period)`.
`why`: Thinktecture owns identical factory validation for this `[ValueObject<int>]`; the forwarding symbol adds no domain meaning.

## `libs/dotnet/Rasm.Materials/.planning/Component/connector.md:227`
`from`: `from row in Fin.Succ(this) from allowable in row.Allowable(key) select new ConnectorCapacity(...)`.
`to`: `Allowable(key).Map(allowable => new ConnectorCapacity(Type, LoadDirection.Download.Published(allowable).Map(cell => cell.DesignKn(duration)), LoadDirection.Uplift.Published(allowable).Map(cell => cell.DesignKn(duration)), LoadDirection.LateralF1.Published(allowable).Map(cell => cell.DesignKn(duration)), LoadDirection.LateralF2.Published(allowable).Map(cell => cell.DesignKn(duration)), Report.CombinesDirections))`.
`why`: `this` is infallible; LanguageExt `Map` preserves the sole `Allowable` failure and deletes the meaningless success shell.

## `libs/dotnet/Rasm.Materials/.planning/Component/pipework.md:244`
`from`: two calls to local `Optional(...)`, whose lines 256-257 hand-write `Option`/`Fin` inversion with `Match`.
`to`: call `r.Size.RatedPsi.TraverseM(static psi => ComponentDetail.Measured(SegmentRows.WorkingPressure, Dimension.PressureDim, psi * PsiPa)).As()` and the analogous `HubMm` expression; delete `Optional`.
`why`: LanguageExt `Option.TraverseM` is the exact absence-total effect inversion; it preserves `None` and the mint failure while deleting a module symbol.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/neural.md:650`
`from`: `replay.Bind(port => port(request)).Match(Some: held => StageResult.Admit(...).Map(Some), None: () => Fin.Succ(None))`.
`to`: `replay.Bind(port => port(request)).TraverseM(held => StageResult.Admit(held, card, request, key)).As()`.
`why`: LanguageExt `Option.TraverseM` produces the same `Fin<Option<StageResult>>` and preserves absence and first failure without hand-written carrier branches.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/finish.md:229`
`from`: `stack.Find(...).Match(Some: bad => Fin.Fail<Seq<FinishLayer>>(...), None: () => Fin.Succ(stack))`.
`to`: `stack.Find(...).TraverseM(bad => Fin.Fail<Unit>(new MaterialFault.Parameter(...))).As().Map(_ => stack)`.
`why`: LanguageExt `Option.TraverseM` preserves first inadmissible-layer failure and empty success without the manual carrier split.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/neural.md:329`
`from`: `Census`, `Stage`, and `Card` chain binary `.Apply`; `Breach` hand-writes `Validation.Success/Fail` at lines 356-359.
`to`: use `AdmissionSlots.Accumulate(Seq(...))` for all three censuses; make `Breach` return `AdmissionSlots.Gate(held, new MaterialFault.Parameter(...))`.
`why`: Domain's concrete/K overloads own these typed applicative folds; all registry faults still accumulate while repeated projectors disappear.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/interchange.md:912`
`from`: `key.AcceptValidated<ModelCardId>(ModelCardId.Validate(message.ModelCardId, null, out ModelCardId id), id)`.
`to`: `key.AcceptValidated<ModelCardId>(message.ModelCardId)`.
`why`: Rasm Domain `OpAcceptance.AcceptValidated<TVO>(string?)` already invokes the generated Thinktecture factory and preserves the same key-shaped refusal.

## `libs/dotnet/Rasm.Materials/.planning/Component/capacity.md:519`
`from`: map refusals to `Error`, then `Match(Empty: Success(Create(...)), More: Fail(faults.Reduce(Error.+)))`.
`to`: `Refusals(...).Traverse(token => Validation<Error, Unit>.Fail(new KernelFault.InvalidValue(...))).Map(_ => Create(...)).As()`.
`why`: `Seq.Traverse` over `Validation<Error,_>` uses the existing `Error` monoid, preserving all independent faults and their order without a manual empty/reduce split.

## `libs/dotnet/Rasm.Materials/.planning/Component/component.md:1785`
`from`: `roster.Map(row => RowOf(context, row, law).ToValidation()).Sequence().As().ToFin()`.
`to`: `roster.Traverse(row => RowOf(context, row, law).ToValidation()).As().ToFin()`.
`why`: LanguageExt `Map(...).Sequence()` leaves an abstract inner carrier; direct `Traverse` lands the concrete ordered `Seq<ComponentRow>` while retaining `Validation` accumulation.

## `libs/dotnet/Rasm.Materials/.planning/Component/component.md:1790`
`from`: `law.Detail.Match(Some: fold => fold(row, profile, context.Key).Map(Some), None: static () => Fin.Succ(None))`.
`to`: `law.Detail.TraverseM(fold => fold(row, profile, context.Key)).As()`.
`why`: LanguageExt `Option.TraverseM` is the exact absence-total inversion to `Fin<Option<PropertyBag>>`; the delegate failure and absence arm are unchanged.

## `libs/dotnet/Rasm.Materials/.planning/Component/component.md:749`
`from`: the sole `Gate(condition, key, source.GetType())` call plus its local success/failure helper at lines 766-767.
`to`: `AdmissionSlots.Gate(condition, new ComponentFault.SectionIncoherent(key, source.GetType()))`; delete local `Gate`.
`why`: Domain owns the identical typed validation gate; section behavior is unchanged and one module symbol disappears.

## `libs/dotnet/Rasm.Materials/.planning/Component/electrical.md:61`
`from`: `toSeq(Items).Map(alloy => guard(...).ToValidation()).Sequence().As()`.
`to`: `toSeq(Items).Traverse(alloy => AdmissionSlots.Gate(alloy.AppearanceMetal.IsSome, new KernelFault.InvalidValue(...))).As()`.
`why`: direct LanguageExt `Traverse` fixes the abstract-inner landing; Domain `AdmissionSlots.Gate` preserves and accumulates every existing typed alloy fault.

## `libs/dotnet/Rasm.Materials/.planning/Component/electrical.md:376`
`from`: `toSeq(WireSystem.Items).Map(system => guard(...).ToValidation()).Sequence().As()`.
`to`: `toSeq(WireSystem.Items).Traverse(system => AdmissionSlots.Gate(Roster.Exists(row => row.System == system), new KernelFault.InvalidValue(...))).As()`.
`why`: LanguageExt `Traverse` preserves the full census; Domain `AdmissionSlots.Gate` replaces both the invalid `Guard.ToValidation` call and duplicated gate lifting.

## `libs/dotnet/Rasm.Materials/.planning/Component/fastener.md:283`
`from`: `Positive(...) => guard(...).ToValidation()` plus both one-line `Prove` helpers at lines 382/589 and their tuple/`Apply` callers.
`to`: make `Positive` return `AdmissionSlots.Gate(...)`; replace each `Prove` tuple with `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), ...))`, then delete both `Prove` symbols.
`why`: Domain already owns typed gates and their applicative accumulation; all faults remain independent while two module symbols and invalid `Guard.ToValidation` calls disappear.

## `libs/dotnet/Rasm.Materials/.planning/Component/reinforcement.md:389`
`from`: both `Prove(...) => guard(...).ToValidation()` helpers and their coherence/shape callers through line 647.
`to`: use `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), ...))` for `Coherence`, use `AdmissionSlots.Gate` in `Shape`, and delete both `Prove` symbols.
`why`: Domain owns the same typed gate and accumulating fold; the generated-union dispatch and exact faults remain while two package helpers disappear.

## `libs/dotnet/Rasm.Materials/.planning/Component/finishes.md:215`
`from`: the two `Certified` union arms call one-line `Prove(...) => guard(...).ToValidation()`.
`to`: call `AdmissionSlots.Gate(condition, fault)` in those arms and delete `Prove`.
`why`: Domain owns the identical typed admission gate; Thinktecture's generated exhaustive `Switch` is retained while the invalid call and module symbol disappear.

## `libs/dotnet/Rasm.Materials/.planning/Properties/sustainability.md:165`
`from`: both coherence gates call `guard(...).ToValidation()` at lines 165-168.
`to`: call `AdmissionSlots.Gate(condition, new ElementFault.ValueRejected(...))` for both existing tuple slots.
`why`: Domain's concrete gate preserves both `ElementFault`s and the value-producing tuple slots; `Guard.ToValidation` is not a LanguageExt member.

## `libs/dotnet/Rasm.Materials/.planning/Component/concrete.md:124`
`from`: all three coherence tuple slots call `guard(...).ToValidation()` at lines 124-129.
`to`: `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), AdmissionSlots.Gate(...), AdmissionSlots.Gate(...)))`.
`why`: Domain already owns typed gate accumulation; all three faults remain applicative while the tuple projector and nonexistent member calls disappear.

## `libs/dotnet/Rasm.Materials/.planning/Component/insulation.md:109`
`from`: both coherence tuple slots call `guard(...).ToValidation()` at lines 109-113.
`to`: `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), AdmissionSlots.Gate(...)))`.
`why`: Domain's existing admission fold preserves both independent refusals and removes the tuple `Apply` plus invalid LanguageExt calls.

## `libs/dotnet/Rasm.Materials/.planning/Component/pipework.md:233`
`from`: both coherence tuple slots call `guard(...).ToValidation()` at lines 233-237.
`to`: `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), AdmissionSlots.Gate(...)))`.
`why`: Domain owns this exact applicative gate census; both pipe faults remain and the tuple projector plus invalid member calls are deleted.

## `libs/dotnet/Rasm.Materials/.planning/Component/ductwork.md:192`
`from`: both coherence tuple slots call `guard(...).ToValidation()` at lines 192-197.
`to`: `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), AdmissionSlots.Gate(...)))`.
`why`: Domain's admission fold preserves both gauge/shape failures while removing duplicated tuple accumulation and absent `Guard.ToValidation` calls.

## `libs/dotnet/Rasm.Materials/.planning/Component/timber.md:159`
`from`: four coherence tuple slots call `guard(...).ToValidation()` at lines 159-168.
`to`: `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), ...))` over the same four predicates/faults.
`why`: Domain already owns applicative admission accumulation; all timber faults remain independent and the tuple projector plus invalid calls disappear.

## `libs/dotnet/Rasm.Materials/.planning/Component/finishes.md:337`
`from`: both covering coherence gates call `guard(...).ToValidation()` at lines 337-340.
`to`: `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), AdmissionSlots.Gate(...), row.Specification.Certified(key)))`.
`why`: Domain's fold preserves both local faults plus the existing certification validation while removing the tuple `Apply` and invalid member calls.

## `libs/dotnet/Rasm.Materials/.planning/Component/electrical.md:395`
`from`: both row coherence slots call `guard(...).ToValidation()` at lines 395-398.
`to`: `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), AdmissionSlots.Gate(...)))`.
`why`: Domain already owns the accumulating gate rail; both range faults remain and the tuple `Apply` plus nonexistent calls disappear.

## `libs/dotnet/Rasm.Materials/.planning/Component/steel.md:718`
`from`: both steel coherence slots call `guard(...).ToValidation()` at lines 718-721.
`to`: `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), AdmissionSlots.Gate(...)))`.
`why`: Domain's shared fold preserves the family/body fault census and removes the duplicated tuple projector and invalid LanguageExt calls.

## `libs/dotnet/Rasm.Materials/.planning/Component/precast.md:133`
`from`: three precast coherence slots call `guard(...).ToValidation()` at lines 133-138.
`to`: `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), AdmissionSlots.Gate(...), AdmissionSlots.Gate(...)))`.
`why`: Domain's existing admission fold preserves all three independent faults while deleting the tuple projector and nonexistent member calls.

## `libs/dotnet/Rasm.Materials/.planning/Component/cmu.md:334`
`from`: all seven CMU coherence gates call `guard(...).ToValidation()` through line 355.
`to`: `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), ...))` over the same seven predicates/faults.
`why`: Domain already owns the applicative admission census; every CMU fault remains and the seven-slot tuple projector plus invalid calls disappear.

## `libs/dotnet/Rasm.Materials/.planning/Component/glazing.md:477`
`from`: five glazing-detail gates at lines 477-486 and the muntin gate at 686 call `guard(...).ToValidation()`.
`to`: use `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), ...))` for the five-gate detail census; use `AdmissionSlots.Gate(...)` beside `GlazingDetail.Stack` at line 686.
`why`: Domain owns concrete gates and accumulation; every typed glazing fault and the stack validation remain while invalid LanguageExt calls disappear.

## `libs/dotnet/Rasm.Materials/.planning/Component/glazing.md:528`
`from`: the odd cavity branch calls one-use `CavityPly`; lines 541-542 only wrap one pure `Seq<Ply>` in `Fin.Succ`.
`to`: inline `Fin.Succ(Seq(new Ply(MaterialId.Of("gas.cavity"), cavities[slot / 2].WidthMm, PlyRole.Cavity)))`; delete `CavityPly`.
`why`: LanguageExt's success carrier remains solely for the conditional branch shape; the forwarding method adds no failure or domain meaning.

## `libs/dotnet/Rasm.Materials/.planning/Component/aluminum.md:168`
`from`: two `guard(...).ToValidation()` slots plus `AluminumArm.Map(arm => arm.Strengths(...).ToValidation()).IfNone(Success(unit))` in a tuple/`Apply`.
`to`: `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), AdmissionSlots.Gate(...), AluminumArm.Traverse(arm => arm.Strengths(...).ToValidation().Map(static _ => unit)).As().Map(static _ => unit)))`.
`why`: Domain owns accumulation and LanguageExt `Option.Traverse` owns absent-as-success inversion; both grade faults and the optional strength fault remain.

## `libs/dotnet/Rasm.Materials/.planning/Component/connector.md:356`
`from`: the envelope and fastener gates call `guard(...).ToValidation()` at lines 356-360.
`to`: `AdmissionSlots.Accumulate(Seq(row.Allowable(key).ToValidation().Map(static _ => unit), AdmissionSlots.Gate(...), AdmissionSlots.Gate(...)))`.
`why`: Domain owns the accumulating gate fold and LanguageExt owns the valid `Fin` conversion; the same three faults accumulate without a tuple projector.

## `libs/dotnet/Rasm.Materials/.planning/Component/panel.md:645`
`from`: the three panel gates at lines 645-651 and deck gate at 658-661 call `guard(...).ToValidation()`.
`to`: use `AdmissionSlots.Gate` for `DeckDrift`; make `Coherence` an `AdmissionSlots.Accumulate(Seq(...))` of its three gates, `FastenPattern.Of(...).ToValidation().Map(static _ => unit)`, and `DeckDrift`.
`why`: Domain owns the gate census and LanguageExt retains the valid `Fin` conversion/bind; every panel refusal remains without nested tuple projectors.

## `libs/dotnet/Rasm.Materials/.planning/Component/fastener.md:262`
`from`: the finite-angle tuple slot calls `guard(...).ToValidation()`.
`to`: include `AdmissionSlots.Gate(double.IsFinite(loadToGrainDeg), fault)` with the four `Positive(...)` slots in `AdmissionSlots.Accumulate(Seq(...))`.
`why`: Domain owns the five-way applicative admission fold; the same faults accumulate and the tuple projector plus invalid `Guard` member disappear.

## `libs/dotnet/Rasm.Materials/.planning/Component/masonry.md:883`
`from`: the course and frog gates call `guard(...).ToValidation()` at lines 883-886.
`to`: flatten the nested tuple into `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), AdmissionSlots.Gate(...), ...valid Fin.ToValidation().Map(static _ => unit) slots...))`.
`why`: Domain owns the flat applicative census; the same course, frog, unit, void, mean-deviation, and permitted-range faults accumulate without nested tuple projectors.

## `libs/dotnet/Rasm.Materials/.planning/Component/reinforcement.md:633`
`from`: the bar-admission tuple slot calls `guard(...).ToValidation()`.
`to`: `AdmissionSlots.Accumulate(Seq(AdmissionSlots.Gate(...), Shape(layout, key))).ToFin().Map(_ => layout)`.
`why`: Domain owns this two-slot admission fold; the bar and generated-union shape faults still accumulate while the tuple projector and invalid call disappear.

## `libs/dotnet/Rasm.Materials/.planning/Component/reinforcement.md:403`
`from`: the optional bend, bend row, and duct measurement each use `Match(Some: effect.Map(Some), None: Fin.Succ(None))` at lines 403-415.
`to`: use `item.Bend.TraverseM(policy => RebarSchedule.StandardHook(...)).As()`, `bend.TraverseM(BendRow).As()`, and `post.Duct.InnerDiameterMm.TraverseM(mm => ComponentDetail.Measured(...)).As()`.
`why`: LanguageExt `Option.TraverseM` preserves each absence and its existing first `Fin` failure while deleting all six hand-written carrier arms.

## `libs/dotnet/Rasm.Materials/.planning/Component/cmu.md:394`
`from`: CMU fire properties and `Component/masonry.md:844` duplicate `rating.Map(period => FireResistance.I(...).Map(Seq)).IfNone(Fin.Succ(Seq.Empty))`.
`to`: use `physics.FireRating.TraverseM(period => FireResistance.I(period.Key, key).Map(r => Seq(MaterialPropertySet.OfFire(...)))).As().Map(rows => rows.IfNone(Seq<MaterialPropertySet>()))` at both sites.
`why`: LanguageExt `Option.TraverseM` preserves absent-as-empty and the same fire lookup fault while removing the duplicated carrier inversion.

## `libs/dotnet/Rasm.Materials/.planning/Component/finishes.md:246`
`from`: `row.ModuleMm.Match(Some: module => ComponentDetail.Measured(...).Map(Some), None: static () => Fin.Succ(None))`.
`to`: `row.ModuleMm.TraverseM(module => ComponentDetail.Measured(DetailSchema.BoardLength, Dimension.LengthDim, module.LengthMm * 1e-3)).As()`.
`why`: LanguageExt `Option.TraverseM` yields the same `Fin<Option<(PropertyName, PropertyValue)>>`, including absence and mint failure.

## `libs/dotnet/Rasm.Materials/.planning/Raster/set.md:951`
`from`: `Channels.Find(...).Map(pyramid => pyramid.AsImage(key).Map(image => Some<GraphEdit>(...))).IfNone(Fin.Succ(None))`.
`to`: `Channels.Find(...).TraverseM(pyramid => pyramid.AsImage(key).Map(image => (GraphEdit)new GraphEdit.Seat(...))).As()`.
`why`: LanguageExt `Option.TraverseM` preserves absent channels and image failures while removing the nested `Option<Fin<_>>` hand inversion.

## `libs/dotnet/Rasm.Materials/.planning/Raster/set.md:942`
`from`: `SinkSlot.Items.Fold(Fin.Succ(Seq<GraphEdit>()), ... SlotEdit(...).Map(edit => edit.Map(edits.Add).IfNone(edits)))`.
`to`: `toSeq(SinkSlot.Items).TraverseM(slot => SlotEdit(set, slot, sampler, key)).As().Map(static edits => edits.Somes())`.
`why`: LanguageExt `TraverseM` preserves slot order and first failure; `Somes` is the existing one-pass absence drop, replacing the hand accumulator.

## `libs/dotnet/Rasm.Materials/.planning/Raster/press.md:227`
`from`: `compiled.Order.Fold(Fin.Succ(Seq<ChainNode>()), (acc, node) => acc.Bind(built => ChainKernel(...).Map(built.Add)))`.
`to`: `compiled.Order.TraverseM(node => ChainKernel(node, key).Map(kernel => new ChainNode(kernel, compiled.Operands(node), ChainWords(node, kernel)))).As()`.
`why`: each output depends only on its input node; LanguageExt `TraverseM` preserves order and first failure without the manual `Seq` builder.

## `libs/dotnet/Rasm.Materials/.planning/Raster/press.md:326`
`from`: range `Fold(Fin.Succ(Seq<OpenPbrSurface>()), ... Weathering.Apply(...).Map(built.Add))`.
`to`: range `TraverseM(cell => Weathering.Apply(...).Map(aged => OpenPbrSurface.Of(aged, conductor))).As()`.
`why`: every ladder cell is a one-to-one fallible projection; LanguageExt `TraverseM` preserves ordering and short-circuiting with no accumulator.

## `libs/dotnet/Rasm.Materials/.planning/Raster/tile.md:114`
`from`: `set.Packs.Fold(Fin.Succ(Seq<ChannelPackPlane>()), ... Apply(...).Map(rows.Add))`.
`to`: `set.Packs.TraverseM(pack => Apply(plan, pack.Plane, key).Map(tiled => pack with { Plane = tiled })).As()`.
`why`: each pack is independent and yields exactly one pack; LanguageExt `TraverseM` preserves order and first failure while removing the hand builder.

## `libs/dotnet/Rasm.Materials/.planning/Raster/codec.md:528`
`from`: `toSeq(part.Levels).Fold(Fin.Succ(Seq<TexturePlane>()), ... Level(...).Map(levels.Add))`.
`to`: `toSeq(part.Levels).TraverseM(level => Level(level, part.Header, key)).As()` before the existing pyramid `Map`.
`why`: LanguageExt `TraverseM` is the exact ordered, fail-fast inversion for the one decoded plane per level.

## `libs/dotnet/Rasm.Materials/.planning/Raster/codec.md:804`
`from`: mip range `Fold(Fin.Succ(Seq<TexturePlane>()), ... Level(...).Map(levels.Add))`.
`to`: mip range `TraverseM(level => Level(container, coder, level, storage, transfer, key)).As()` before the existing pyramid `Map`.
`why`: LanguageExt `TraverseM` preserves level order and first decode failure without duplicating its sequence accumulator.

## `libs/dotnet/Rasm.Materials/.planning/Raster/codec.md:962`
`from`: level `Fold(Fin.Succ(Seq<string>()), ... Encode(...).Map(bytes => { write; return leaves.Add(leaf); }))`.
`to`: level `TraverseM(slot => Encode(...).Map(bytes => { write; return leaf; })).As()` before `Run(...)`.
`why`: LanguageExt `TraverseM` remains sequential and fail-fast, produces the same ordered leaf paths, and removes the hand builder.

## `libs/dotnet/Rasm.Materials/.planning/Raster/set.md:731`
`from`: grouped maps `Fold(Fin.Succ(Seq<(Option<UdimTile>, TextureSetDraft)>()), ... Tile(...).Map(drafts.Add))`.
`to`: grouped maps `TraverseM(group => Tile(toSeq(group), manifest.Convention, planes, intent, key)).As()`.
`why`: each group yields one draft independently; LanguageExt `TraverseM` preserves source order and first failure with no accumulator.

## `libs/dotnet/Rasm.Materials/.planning/Raster/gpu.md:1538`
`from`: `plan.Steps.Fold(Fin.Succ(Seq<nint>()), ... Pipeline(...).Map(built.Add))`.
`to`: `plan.Steps.TraverseM(step => Pipeline(step.Kernel, key)).As()`.
`why`: each step yields one cached pipeline independently; LanguageExt `TraverseM` preserves order and the first pipeline fault.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/graph.md:351`
`from`: `edits.Fold(Fin.Succ(this), (graph, edit) => graph.Bind(g => g.Apply(edit, key)))`.
`to`: `edits.FoldM(this, (graph, edit) => graph.Apply(edit, key)).As()`.
`why`: LanguageExt `FoldM` is the existing state-dependent fail-fast fold; edit order and the first `Apply` failure are unchanged.

## `libs/dotnet/Rasm.Materials/.planning/Raster/tile.md:110`
`from`: `PairingOrder(...).Fold(Fin.Succ(HashMap.Empty), (acc, row) => acc.Bind(map => ... map.Add(...)))`.
`to`: `PairingOrder(...).FoldM(HashMap.Empty, (map, row) => ...Map(tiled => map.Add(row, tiled))).As()`.
`why`: the companion lookup genuinely depends on prior state, which is precisely LanguageExt `FoldM`; the manual carrier-threading layer is redundant.

## `libs/dotnet/Rasm.Materials/.planning/Raster/plane.md:643`
`from`: `grids.Fold(Fin.Succ(Seq(basePlane)), (state, grid) => state.Bind(levels => Fold(levels[levels.Count - 1], ...).Map(levels.Add)))`.
`to`: `grids.FoldM(Seq(basePlane), (levels, grid) => Fold(levels[levels.Count - 1], ...).Map(levels.Add)).As()`.
`why`: mip descent depends on the prior level; LanguageExt `FoldM` preserves that dependency, order, and first failure without hand-threading `Fin`.

## `libs/dotnet/Rasm.Materials/.planning/Properties/assessment.md:336`
`from`: `live.Fold(Fin.Succ((Sets: published, Claimed: Set<string>.Empty)), (state, record) => state.Bind(carried => ...))`.
`to`: `live.FoldM((Sets: published, Claimed: Set<string>.Empty), (carried, record) => ...).As()`.
`why`: overlay deduplication is a state-dependent fail-fast fold; LanguageExt `FoldM` preserves claim precedence and `Apply` failures with less carrier plumbing.

## `libs/dotnet/Rasm.Materials/.planning/Raster/filter.md:444`
`from`: `ops.Fold(Fin.Succ((Shape: input, Stages: Seq<PlaneStage>.Empty)), (state, op) => state.Bind(carry => op.Project(...)))`.
`to`: `ops.FoldM((Shape: input, Stages: Seq<PlaneStage>.Empty), (carry, op) => op.Project(...)).As()`.
`why`: stage fusion depends on the prior shape/stage state; LanguageExt `FoldM` preserves the exact state transition and first projection failure.

## `libs/dotnet/Rasm.Materials/.planning/Component/connector.md:309`
`from`: `row.Plate.Match(Some: p => PlateRow(p).Map(Some), None: () => Fin.Succ(None))`.
`to`: `row.Plate.TraverseM(PlateRow).As()`.
`why`: LanguageExt `Option.TraverseM` returns the same `Fin<Option<(PropertyName, PropertyValue)>>`, preserving absence and `PlateRow` failure.

## `libs/dotnet/Rasm.Materials/.planning/Component/fastener.md:292`
`from`: `thread.Match(Some: t => FormRow(kind, t, facts.LengthMm).Map(Some), None: () => Fin.Succ(None))`.
`to`: `thread.TraverseM(t => FormRow(kind, t, facts.LengthMm)).As()`.
`why`: LanguageExt `Option.TraverseM` owns this exact optional effect inversion; no branch or failure behavior changes.

## `libs/dotnet/Rasm.Materials/.planning/Component/insulation.md:123`
`from`: both `ExtentMm` and `RValueSi(...)` use `Match(Some: value => Measured(...).Map(Some), None: () => Fin.Succ(None))`.
`to`: use `option.TraverseM(value => ComponentDetail.Measured(...)).As()` at lines 123-128.
`why`: LanguageExt `Option.TraverseM` preserves each optional row and its independent mint failure while deleting four carrier branches.

## `libs/dotnet/Rasm.Materials/.planning/Component/panel.md:495`
`from`: `FloorSpanMm` and `ShankDiameterMm` each `Match` an optional `Measured(...).Map(Some)` against `Fin.Succ(None)`.
`to`: use `FloorSpanMm.TraverseM(span => Measured(...)).As()` and `ShankDiameterMm.TraverseM(mm => Measured(...)).As()`.
`why`: LanguageExt `Option.TraverseM` gives the identical optional rows and preserves their mint failures without branch pairs.

## `libs/dotnet/Rasm.Materials/.planning/Component/aluminum.md:110`
`from`: `GlazingPocketMm.Match(Some: mm => Measured(...).Map(Some), None: () => Fin.Succ(None))`.
`to`: `GlazingPocketMm.TraverseM(mm => ComponentDetail.Measured(...)).As()`.
`why`: LanguageExt `Option.TraverseM` preserves absence and the measurement failure with less carrier code.

## `libs/dotnet/Rasm.Materials/.planning/Component/joint.md:337`
`from`: `EffectiveThroatMm.Match(Some: value => Measured(...).Map(Some), None: () => Fin.Succ(None))`.
`to`: `EffectiveThroatMm.TraverseM(value => Measured(...)).As()`.
`why`: LanguageExt `Option.TraverseM` is behavior-identical for the optional fallible property mint.

## `libs/dotnet/Rasm.Materials/.planning/Component/precast.md:67`
`from`: `BearingLengthMm.Match(Some: mm => Measured(...).Map(Some), None: () => Fin.Succ(None))`.
`to`: `BearingLengthMm.TraverseM(mm => ComponentDetail.Measured(...)).As()`.
`why`: LanguageExt `Option.TraverseM` retains the `Fin<Option<_>>` shape, absence, and measurement fault directly.

## `libs/dotnet/Rasm.Materials/.planning/Component/finishes.md:294`
`from`: `Bounded` uses `value.Match(Some: bound => Measured(...).Map(Seq), None: () => Fin.Succ(Seq.Empty))`.
`to`: `value.TraverseM(bound => ComponentDetail.Measured(name, dim, bound * toSi)).As().Map(static row => row.ToSeq())`.
`why`: LanguageExt performs the optional effect inversion and `Option.ToSeq` performs the existing absence-to-empty egress; behavior and helper surface stay unchanged.

## `libs/dotnet/Rasm.Materials/.planning/Projection/component.md:165`
`from`: `set.Match(Some: a => PropertyValue.Of(...).Map(Optional), None: () => Fin.Succ(Option<PropertyValue>.None))`.
`to`: `set.TraverseM(a => PropertyValue.Of(new PropertyValue.Text(a.ToValue()), key)).As()`.
`why`: LanguageExt `Option.TraverseM` preserves the absent texture address and the exact `PropertyValue.Of` fault without hand-inverting `Option<Fin<_>>`.

## `libs/dotnet/Rasm.Materials/.planning/Projection/component.md:198`
`from`: `section.Match(None: Fin.Succ(None), Some: c => ...)` encloses a second density `Match(Some: effect.Map(Some), None: Fin.Succ(None))` at line 205.
`to`: `section.TraverseM(c => ... properties.Density.TraverseM(density => area.Multiply(density).Bind(...)).As() ... select Mint(...)).As()`.
`why`: nested LanguageExt `Option.TraverseM` preserves absent section/density and both quantity failures while deleting four carrier arms.

## `libs/dotnet/Rasm.Materials/.planning/Projection/component.md:381`
`from`: `sets.Density.Match(Some: density => Fin.Succ(layer.ThicknessMm * 1e-3 * density.Si), None: () => new ElementFault.ValueRejected(...))`.
`to`: `sets.Density.ToFin(new ElementFault.ValueRejected(...)).Map(density => layer.ThicknessMm * 1e-3 * density.Si)`.
`why`: LanguageExt `Option.ToFin` is the existing required-value boundary; it preserves the exact missing-density fault without manual branches.

## `libs/dotnet/Rasm.Materials/.planning/Projection/component.md:416`
`from`: `recipeOf(id).Match(Some: recipe => Constituents.Of(...).Bind(...), None: () => Fin.Succ(CompositionAuthor.Single(id)))`.
`to`: `recipeOf(id).TraverseM(recipe => Constituents.Of(recipe, key).Bind(rows => CompositionAuthor.ConstituentSet(rows, key))).As().Map(row => row.IfNone(CompositionAuthor.Single(id)))`.
`why`: LanguageExt `Option.TraverseM` preserves undeclared homogeneous material and constituent failures without the manual effect split.

## `libs/dotnet/Rasm.Materials/.planning/Projection/component.md:448`
`from`: `row.Grade.TimberArm.Match(Some: arm => arm.ToProperties(key), None: () => new ProjectionFault.Unresolved(...))`.
`to`: `row.Grade.TimberArm.ToFin(new ProjectionFault.Unresolved(...)).Bind(arm => arm.ToProperties(key))`.
`why`: LanguageExt `Option.ToFin` directly owns required-arm admission and preserves the exact unresolved fault plus lowering failure.

## `libs/dotnet/Rasm.Materials/.planning/Projection/component.md:434`
`from`: `FamilyProperties(...)` only forwards to `Lowerings.Value[item.Family](item, key)`; default row `Barren` only returns `Fin.Succ(Seq.Empty)`.
`to`: call `Lowerings.Value[entry.Row.Item.Family](entry.Row.Item, key)` at line 434 and inline `(Func<Component, Op, Fin<Seq<MaterialPropertySet>>>)(static (_, _) => Fin.Succ(Seq<MaterialPropertySet>()))` at line 458; delete both methods.
`why`: the existing lowering table and LanguageExt success carrier preserve behavior while two one-hop module symbols disappear.

## `libs/dotnet/Rasm.Materials/.planning/Projection/observability.md:518`
`from`: the optional effects at lines 518, 561, 564, 582, and 595 each `Match(Some: write, None: Fin.Succ(unit))`.
`to`: use `option.TraverseM(value => write(value)).As().Map(static _ => unit)` at each site.
`why`: LanguageExt `Option.TraverseM` preserves absence, write order, and the first instrumentation fault; the final `Map` deliberately erases only the traversed `Option<Unit>`.

## `libs/dotnet/Rasm.Materials/.planning/Properties/sustainability.md:379`
`from`: `row!.Classification.Match(Some: c => Classification.Of(...).Map(Some), None: () => Fin.Succ(None))`.
`to`: `row!.Classification.TraverseM(c => global::Rasm.Element.Classification.Classification.Of(c.System, c.Code, key)).As()`.
`why`: LanguageExt `Option.TraverseM` preserves both the absent classification and the existing domain mint failure with no manual carrier arms.

## `libs/dotnet/Rasm.Materials/.planning/Properties/assessment.md:415`
`from`: the first selected property set uses `Match(Some: held => rebuild(...), None: () => Fin.Succ(sets))`.
`to`: `sets.Choose(...).Head.TraverseM(held => rebuild(held).Map(replaced => sets.Filter(set => !ReferenceEquals(set, held)).Add(replaced))).As().Map(result => result.IfNone(sets))`.
`why`: LanguageExt `Option.TraverseM` preserves absence, reference-identity replacement, and the rebuild failure without hand-written carrier arms.

## `libs/dotnet/Rasm.Materials/.planning/Properties/assessment.md:195`
`from`: four calls through line 225 use nonexistent `AdmissionSlots.Gate(condition, key, token)`.
`to`: `AdmissionSlots.Gate(condition, new ElementFault.ValueRejected(key, token))` at each site.
`why`: Domain publishes only `Gate(bool, Error)` or the four-argument typed minter; the existing assessment tokens are package-semantic and retain their `ElementFault` owner.

## `libs/dotnet/Rasm.Materials/.planning/Raster/codec.md:433`
`from`: `chain.Levels.Fold(Fin.Succ(Seq<TexturePlane>()), (state, level) => state.Bind(converted => level.ToAlpha(...).Map(converted.Add).Rollback(...)))`.
`to`: `chain.Levels.FoldM(Seq<TexturePlane>(), (converted, level) => level.ToAlpha(to, key).Map(converted.Add).Rollback([.. converted])).As()`.
`why`: LanguageExt `FoldM` is the same ordered state-dependent conversion; first failure and rollback of already converted planes remain unchanged.

## `libs/dotnet/Rasm.Materials/.planning/Raster/codec.md:628`
`from`: `chain.Levels.Select(...).Fold(Fin.Succ(unit), (state, slot) => state.Bind(_ => WriteLevelTiles(...)))`.
`to`: `toSeq(chain.Levels.Select(...)).FoldM(unit, (_, slot) => WriteLevelTiles(writer, part, slot.Index, slot.Level, format, names, key)).As()`.
`why`: LanguageExt `FoldM` owns the sequential fail-fast unit fold; level order and the first writer fault are unchanged.

## `libs/dotnet/Rasm.Materials/.planning/Raster/codec.md:644`
`from`: tile slots `Fold(Fin.Succ(unit), (state, slot) => state.Bind(_ => { ... return Sealed(...); }))`.
`to`: tile slots `FoldM(unit, (_, slot) => { ... return Sealed(...); }).As()`.
`why`: LanguageExt `FoldM` preserves the imperative per-tile body, sequential writes, and first seal failure while deleting only manual `Fin` threading.

## `libs/dotnet/Rasm.Materials/.planning/Raster/set.md:997`
`from`: two pure projections use `Fold(Fin.Succ(seed), (acc, row) => acc.Map(carried => Apply/Unpack(...)))` before `MaterialParameters.Of`.
`to`: use ordinary `Fold(seed, (carried, row) => Apply/Unpack(...))` for both, then pass the result to `MaterialParameters.Of`.
`why`: `Apply` and `Unpack` are pure and infallible; LanguageExt `Fin` shells add no effect, while the sole real `MaterialParameters.Of` failure remains intact.

## `libs/dotnet/Rasm.Materials/.planning/Raster/gpu.md:1411`
`from`: `kernel.Oracle.Fold(Fin.Succ(unit), (acc, fixture) => acc.Bind(_ => Dispatch(...).Bind(readback => Compare(...))))`.
`to`: `kernel.Oracle.TraverseM(fixture => device.Dispatch(...).Bind(readback => Compare(kernel, fixture, readback, key))).As().Map(static _ => unit)`.
`why`: every fixture produces only `Unit`; LanguageExt `TraverseM` preserves fixture order and first dispatch/compare failure without a fake state accumulator.

## `libs/dotnet/Rasm.Materials/.planning/Raster/gpu.md:1545`
`from`: `plan.Steps.Fold(Fin.Succ(0), (acc, step) => acc.Bind(index => Record(...).Map(_ => index + 1)))`.
`to`: `plan.Steps.FoldM(0, (index, step) => Record(pool, encoder, pipelines[index], step, width, height, layers, key).Map(_ => index + 1)).As()`.
`why`: LanguageExt `FoldM` directly owns the stateful pipeline index; step order and first recording failure are identical.

## `libs/dotnet/Rasm.Materials/.planning/Raster/press.md:491`
`from`: both direct and derived binding passes use `Fold(Fin.Succ(seed), (acc, binding) => acc.Bind(carried => Staged(...)))` at lines 491/497.
`to`: use `.FoldM(seed, (carried, binding) => Staged(...)).As()` for each pass.
`why`: each press binding depends on prior landed/evidence/downgrade/fault state; LanguageExt `FoldM` preserves staging, rollback, order, and first failure without manual carrier threading.

## `libs/dotnet/Rasm.Materials/.planning/Raster/press.md:546`
`from`: pack rows `Fold(Fin.Succ((Channels: landed, Packs: Seq.Empty)), (acc, pack) => acc.Bind(carried => ...))`.
`to`: pack rows `FoldM((Channels: landed, Packs: Seq<ChannelPackPlane>()), (carried, pack) => ...).As()`.
`why`: pack composition consumes the prior channel/packs state; LanguageExt `FoldM` preserves that dependency and every existing refusal.

## `libs/dotnet/Rasm.Materials/.planning/Raster/press.md:566`
`from`: level range `Fold(Fin.Succ(Seq<PackLevelJob>()), (acc, levelIndex) => acc.Bind(jobs => ...Rollback(...)))`.
`to`: level range `FoldM(Seq<PackLevelJob>(), (jobs, levelIndex) => ...Rollback([.. jobs.Map(static job => job.Target)])).As()`.
`why`: LanguageExt `FoldM` preserves ordered job construction, the prior-job rollback set, and first plane fault while removing the redundant outer `Fin` accumulator.

## `libs/dotnet/Rasm.Materials/.planning/Raster/filter.md:454`
`from`: `stages.Fold(Fin.Succ((Plane: source, Evidence: None, Done: 0)), (state, stage) => state.Bind(carry => ...))`.
`to`: `stages.FoldM((Plane: source, Evidence: Option<HeightEvidence>.None, Done: 0), (carry, stage) => ...).As()`.
`why`: LanguageExt `FoldM` preserves the prior-plane custody, evidence, progress, cancellation, and first kernel failure while deleting hand-threaded carrier state.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/environment.md:872`
`from`: mip range `Fold(Fin.Succ(Seq<TexturePlane>()), (acc, mip) => acc.Bind(levels => Dispatch(...).Bind(Decode).Map(levels.Add).Rollback(...)))`.
`to`: mip range `FoldM(Seq<TexturePlane>(), (levels, mip) => Dispatch(...).Bind(Decode).Map(levels.Add).Rollback([.. levels])).As()`.
`why`: LanguageExt `FoldM` preserves mip order, first GPU/decode failure, and rollback of already decoded levels.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/environment.md:947`
`from`: `Specular` uses a mip `Fold(Fin.Succ(Seq<TexturePlane>()), (acc, mip) => acc.Bind(levels => budget.Opened(...).Match(...)))`.
`to`: use `FoldM(Seq<TexturePlane>(), (levels, mip) => budget.Opened(...).Match(...)).As()` before the existing `Strict` map.
`why`: LanguageExt `FoldM` retains governance cancellation, rollback of landed levels, mip order, and first level failure without the manual carrier layer.

## `libs/dotnet/Rasm.Materials/.planning/Raster/codec.md:865`
`from`: nested mip/layer folds each use `Fold(Fin.Succ(seed), (state, item) => state.Bind(rows => ...))`.
`to`: use outer `FoldM(Seq<TextureSubresource>(), (built, slot) => inner).As()` and inner `FoldM(built, (rows, layer) => slot.Level.Layer(...).Map(rows.Add)).As()`.
`why`: LanguageExt `FoldM` preserves mip-major/layer-minor ordering, prior rows, and the first `Layer` failure without either hand-threaded `Fin` accumulator.

## `libs/dotnet/Rasm.Materials/.planning/Raster/set.md:351`
`from`: `Gates(...).Fold(Fin.Succ(unit), (admitted, gate) => admitted.Bind(_ => gate()))`.
`to`: `Gates(...).TraverseM(static gate => gate()).As().Map(_ => new TextureSet(...))`.
`why`: every gate produces only `Unit`; LanguageExt `TraverseM` preserves declaration order and the first refusal without a fake state value.

## `libs/dotnet/Rasm.Materials/.planning/Raster/set.md:372`
`from`: channel and pack gate delegates each use a `Fold(Fin.Succ(unit), ...Bind(_ => Admit...))`.
`to`: use `toSeq(draft.Channels.AsIterable()).TraverseM(pair => AdmitChannel(...)).As().Map(_ => unit)` and `draft.Packs.TraverseM(pack => AdmitPack(...)).As().Map(_ => unit)`.
`why`: LanguageExt `TraverseM` is the same ordered fail-fast gate walk; no accumulator state is read or produced.

## `libs/dotnet/Rasm.Materials/.planning/Raster/set.md:739`
`from`: `maps.Fold(Fin.Succ((Channels: Empty, Packs: Seq.Empty)), (acc, map) => acc.Bind(carried => ...))`.
`to`: `maps.FoldM((Channels: HashMap<TextureChannel, TexturePyramid>.Empty, Packs: Seq<ChannelPackPlane>()), (carried, map) => ...).As()`.
`why`: each classified map updates prior channel/pack state; LanguageExt `FoldM` preserves that dependency and first lookup/conversion failure.

## `libs/dotnet/Rasm.Materials/.planning/Raster/set.md:758`
`from`: pyramid levels `Fold(Fin.Succ(Seq<TexturePlane>()), (acc, level) => acc.Bind(built => PlaneOp.Apply(...).Map(built.Add).Rollback(...)))`.
`to`: levels `FoldM(Seq<TexturePlane>(), (built, level) => PlaneOp.Apply(...).Map(result => built.Add(result.Plane)).Rollback([.. built])).As()`.
`why`: LanguageExt `FoldM` preserves level order, first operation failure, and rollback of already converted planes.

## `libs/dotnet/Rasm.Materials/.planning/Raster/set.md:962`
`from`: channel then pack sampling uses two `Fold(Fin.Succ(state), (acc, item) => acc.Bind(carried => Read(...).Map(...)))` passes.
`to`: use `FoldM(fallback, (row, pair) => Read(...).Map(texel => Apply(...))).As()` then `FoldM(row, (carried, pack) => Read(...).Map(texel => Unpack(...))).As()`.
`why`: LanguageExt `FoldM` preserves the channel-before-pack state dependency, sample order, and first read failure without nested carrier accumulators.

## `libs/dotnet/Rasm.Materials/.planning/Raster/press.md:143`
`from`: `Gates(...).Fold(Fin.Succ(unit), (admitted, gate) => admitted.Bind(_ => gate()))`.
`to`: `Gates(...).TraverseM(static gate => gate()).As()` before the existing plan maps.
`why`: every gate returns only `Unit`; LanguageExt `TraverseM` preserves gate order and first refusal with no synthetic state.

## `libs/dotnet/Rasm.Materials/.planning/Raster/press.md:154`
`from`: `Degenerate.Map(bad => Fin.Fail<Unit>(...)).IfNone(Fin.Succ(unit))` and `Tile.Map(policy => guard(...).ToFin()).IfNone(Fin.Succ(unit))`.
`to`: use `Degenerate.TraverseM(bad => Fin.Fail<Unit>(...)).As().Map(_ => unit)` and `Tile.TraverseM(policy => guard(...).ToFin()).As().Map(_ => unit)`.
`why`: LanguageExt `Option.TraverseM` preserves absent policies, the exact failures, and fail-fast gate order without hand inversion.

## `libs/dotnet/Rasm.Materials/.planning/Raster/press.md:163`
`from`: `draft.Bindings.Fold(Fin.Succ(unit), (acc, binding) => acc.Bind(_ => AdmitBinding(...)))`.
`to`: `draft.Bindings.TraverseM(binding => AdmitBinding(draft, binding, key)).As().Map(static _ => unit)`.
`why`: each binding is only a gate; LanguageExt `TraverseM` preserves binding order and first refusal without an unused accumulator.

## `libs/dotnet/Rasm.Materials/.planning/Raster/press.md:508`
`from`: `plan.Tile.Map(policy => TileSynth.Tileify(...).Map(pair => pair.Set)).IfNone(Fin.Succ(set))`.
`to`: `plan.Tile.TraverseM(policy => TileSynth.Tileify(set, policy, key, ticks).Map(static pair => pair.Set)).As().Map(tiled => tiled.IfNone(set))`.
`why`: LanguageExt `Option.TraverseM` preserves absent tiling as the original set and propagates the same synthesis failure without nested carriers.

## `libs/dotnet/Rasm.Materials/.planning/Raster/press.md:617`
`from`: accelerated bindings `Fold(Fin.Succ((Rows: Empty, Done: 0)), (acc, binding) => acc.Bind(carried => Staged(...)))`.
`to`: bindings `FoldM((Rows: HashMap<TextureChannel, TexturePyramid>.Empty, Done: 0), (carried, binding) => Staged(...)).As()`.
`why`: LanguageExt `FoldM` preserves prior landed planes, progress, rollback, binding order, and first lowering/device failure.

## `libs/dotnet/Rasm.Materials/.planning/Raster/press.md:1058`
`from`: parity rows `Fold(Fin.Succ(None), (worst, entry) => worst.Bind(carried => Channels.Find(...).Map(Fin).IfNone(Fin.Succ(carried))))`.
`to`: use `FoldM(Option<double>.None, (carried, entry) => Channels.Find(entry.Key).Map(cpu => Divergence(...).Map(delta => Some(carried.Map(seen => Math.Max(seen, delta)).IfNone(delta)))).IfNone(Fin.Succ(carried))).As()`.
`why`: LanguageExt `FoldM` removes only the redundant outer `Fin` threading; missing channels, running maximum, row order, and first divergence fault remain unchanged.

## `libs/dotnet/Rasm.Materials/.planning/Projection/observability.md:527`
`from`: `Assessment.Map(assessment => assessment.Switch(...)).IfNone(Fin.Succ(unit))`.
`to`: `Assessment.TraverseM(assessment => assessment.Switch(...)).As().Map(static _ => unit)`.
`why`: LanguageExt `Option.TraverseM` preserves absent assessment, ordered fit writes, and the first instrumentation fault without nested `Option<Fin<Unit>>`.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/acquisition.md:349`
`from`: `Chart.Map(chart => Solve(chart, key).Map(fit => acquired with { ... })).IfNone(Fin.Succ(acquired))`.
`to`: `Chart.TraverseM(chart => Solve(chart, key).Map(fit => acquired with { ... })).As().Map(result => result.IfNone(acquired))`.
`why`: LanguageExt `Option.TraverseM` preserves the unchanged acquisition when no chart exists and propagates the identical solve failure.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/graph.md:331`
`from`: the first bad node is mapped to `Fin.Fail<Unit>` and then `IfNone(Fin.Succ(unit))`.
`to`: `Nodes.Choose(n => Admit(n, byId)).Head.TraverseM(reason => Fin.Fail<Unit>(new MaterialFault.Graph(key, reason))).As()` in the existing ignored `from` clause.
`why`: LanguageExt `Option.TraverseM` preserves no-bad-node success and the first declared graph refusal without manually inverting `Option<Fin<Unit>>`.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/texture.md:880`
`from`: the first seam probe is mapped to `Fin.Fail<Unit>` and then `IfNone(Fin.Succ(unit))`.
`to`: append `.TraverseM(probe => Fin.Fail<Unit>(new MaterialFault.Parameter(key, ...))).As()` directly to the filtered `Head` in the existing ignored `from` clause.
`why`: LanguageExt `Option.TraverseM` preserves empty-probe success and the first seam refusal without a manual absent-success arm.

## `libs/dotnet/Rasm.Materials/.planning/Component/ductwork.md:103`
`from`: both filtered `Seq` reads call `.HeadOrNone()`.
`to`: replace each `.HeadOrNone()` with the `.Head` property.
`why`: LanguageExt v5 removed `HeadOrNone` from `Seq`; `Seq.Head` is the exact `Option<A>` first read and preserves the same empty result.

## `libs/dotnet/Rasm.Materials/.planning/Component/concrete.md:171`
`from`: `toSeq(Items).OrderBy(static row => row.Ordinal).ToSeq()`.
`to`: `toSeq(Items.OrderBy(static row => row.Ordinal))`.
`why`: LINQ `OrderBy` returns `IOrderedEnumerable<StructuralClass>`, which LanguageExt `.ToSeq()` cannot bind; `Prelude.toSeq` is the catalogued re-entry and preserves ordinal order.

## `libs/dotnet/Rasm.Materials/.planning/Component/masonry.md:801`
`from`: `toSeq(Items).OrderBy(static row => row.Key).ToSeq()`.
`to`: `toSeq(Items.OrderBy(static row => row.Key))`.
`why`: LINQ `OrderBy` returns `IOrderedEnumerable<RatingPeriod>`, which LanguageExt `.ToSeq()` cannot bind; `Prelude.toSeq` is the catalogued re-entry and preserves key order.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/acquisition.md:402`
`from`: `toSeq(level.Channels).Filter(...).Choose(...).OrderBy(...).Map(...)`.
`to`: `toSeq(toSeq(level.Channels).Filter(...).Choose(...).OrderBy(...)).Map(...)`.
`why`: LINQ `OrderBy` exits `Seq` as `IOrderedEnumerable`; re-entry through existing `Prelude.toSeq` restores LanguageExt `Map` while preserving wavelength order.

## `libs/dotnet/Rasm.Materials/.planning/Component/capacity.md:1275`
`from`: `toSeq(ranked.OrderBy(...)).AsIterable().Map(...).Choose(...).Head()`.
`to`: `toSeq(ranked.OrderBy(...)).Map(...).Choose(...).Head`.
`why`: the ordered run already re-enters as `Seq`; deleting the needless `AsIterable` keeps the same lazy first-result search and uses LanguageExt's property-shaped `Seq.Head`.

## `libs/dotnet/Rasm.Materials/.planning/Raster/gpu.md:215`
`from`: `Ceilings(...).Find(...).Map(ceiling => (Error)new RasterFault.Device(...)).Match(Some: Fin.Fail<Unit>, None: static () => Fin.Succ(unit))`.
`to`: `Ceilings(...).Find(...).TraverseM(ceiling => Fin.Fail<Unit>(new RasterFault.Device(...))).As().Map(static _ => unit)`.
`why`: LanguageExt `Option.TraverseM` preserves absence as success and the first exceeded ceiling as the identical failure without the manual `Option<Fin<Unit>>` inversion.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/neural.md:638`
`from`: `ordered.Fold(Fin.Succ(state), (state, stage) => state.Bind(carried => ...))` in `Thread`.
`to`: `ordered.FoldM((Steps: Seq<StageStep>(), Prefix: Seq<PbrStage>(), Width: intent.Width, Height: intent.Height), (carried, stage) => ...).As().Map(static carried => carried.Steps.Strict())`.
`why`: LanguageExt `FoldM` owns this state-dependent fail-fast fold; each request still sees the prior prefix/dimensions and the first stage fault is unchanged.

## `libs/dotnet/Rasm.Materials/.planning/Raster/plane.md:429`
`from`: `pitchMm.Match(Some: pitch => Placement.Build(...), None: () => Fin.Succ(Transform.Identity))`.
`to`: `pitchMm.TraverseM(pitch => Placement.Build(new TransformSpec.UniformScale(Point3d.Origin, pitch.Value), key: key)).As().Map(static map => map.IfNone(Transform.Identity))`.
`why`: LanguageExt `Option.TraverseM` preserves absent pitch as identity and the exact placement failure without hand-inverting `Option<Fin<Transform>>`.

## `libs/dotnet/Rasm.Materials/.planning/Component/panel.md:392`
`from`: `found.Match(Some: row => cell(row).ToFin(fault), None: () => Fin.Fail<double>(fault))`.
`to`: `found.Bind(cell).ToFin(new ComponentFault.LateralCellMissing(x.Key, x.Grade, x.Nail, x.ThicknessIn))`.
`why`: LanguageExt `Option.Bind` merges the row and cell absence paths before the existing `ToFin`; both still produce the identical fault.

## `libs/dotnet/Rasm.Materials/.planning/Component/component.md:1313`
`from`: the elected `Option` uses `Match(Some: election => Component.Of(...).Map(item => Some(new ComponentRow(...))), None: () => Fin.Succ(None))`.
`to`: append `.TraverseM(election => Component.Of(...).Map(item => new ComponentRow(item, EvidenceGrade.Import))).As()` to the election query.
`why`: LanguageExt `Option.TraverseM` preserves unelected candidates and the same component admission failure while deleting both manual carrier arms.

## `libs/dotnet/Rasm.Materials/.planning/Component/glazing.md:547`
`from`: `muntin.Match(Some: MuntinRows, None: () => Fin.Succ(Seq<(PropertyName, PropertyValue)>()))`.
`to`: `muntin.TraverseM(MuntinRows).As().Map(static rows => rows.IfNone(Seq<(PropertyName, PropertyValue)>()))`.
`why`: LanguageExt `Option.TraverseM` preserves absent muntins as the empty row set and propagates the same `MuntinRows` failure without a carrier split.

## `libs/dotnet/Rasm.Materials/.planning/Properties/properties.md:214`
`from`: module-level `NoGroup` plus six `Option.Match(None: NoGroup, Some: value => ...ToValidation())` branches at lines 220-243.
`to`: use `option.Traverse(value => ...ToValidation()).As().Map(groups => groups.IfNone(Seq<MaterialPropertySet>()))` for each; delete `NoGroup`.
`why`: LanguageExt `Option.Traverse` preserves absent groups as validation success and every present-group fault inside the existing accumulating tuple while removing one symbol.

## `libs/dotnet/Rasm.Materials/.planning/Properties/properties.md:651`
`from`: `At(...).Match(Some: mix => MaterialPropertySet.OfDurability(...), None: () => new ElementFault.ValueRejected(...))`.
`to`: `At(...).ToFin(new ElementFault.ValueRejected(...)).Bind(mix => MaterialPropertySet.OfDurability(...))`.
`why`: LanguageExt `Option.ToFin` is the existing required-row boundary; it preserves both reason tokens and the durability mint failure without manual carrier branches.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/weathering.md:344`
`from`: `target.Match(Some: tint => Mapped(current.Mix(...)), None: () => current)`.
`to`: `target.Map(tint => Mapped(current.Mix(new Unicolour(...), ColourSpace.RgbLinear, f, premultiplyAlpha: false))).IfNone(current)`.
`why`: LanguageExt `Option.Map`/`IfNone` expresses the same pure optional tint projection in one expression; absence and gamut mapping are unchanged.

## `libs/dotnet/Rasm.Materials/.planning/Projection/analytics.md:363`
`from`: the sole call `Scored(entry.Tile)` plus `Scored(Option<TileRun>) => tile.Bind(run => run.Score.Value())` at line 380.
`to`: inline `entry.Tile.Bind(static run => run.Score.Value())`; delete `Scored`.
`why`: LanguageExt `Option.Bind` already owns the exact flattening; the one-hop wrapper adds no analytics policy and only enlarges module surface.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/graph.md:592`
`from`: `Scatter(r, g, b) => SubsurfaceRadius.Create(r, g, b)` plus its nine roster calls.
`to`: call generated `SubsurfaceRadius.Create` at the roster rows; delete `Scatter`.
`why`: Thinktecture's complex-value-object factory is already the admission owner; the forwarding method changes no validation and only adds a module symbol.

## `libs/dotnet/Rasm.Materials/.planning/Appearance/interchange.md:693`
`from`: `Mix.Map(row => Fin.Succ(MixNodes(...))).IfNone(() => Fin.Fail<Seq<MtlxNode>>(fault))`.
`to`: `Mix.ToFin(new MaterialFault.Graph(s.Key, $"<mtlx-mix-unresolved:{x.Id.Value}>")).Map(row => MixNodes(x, row, s.Ports))`.
`why`: LanguageExt `Option.ToFin` is the existing required-mix boundary; it preserves the exact missing-mix fault and deletes the redundant nested success carrier.
