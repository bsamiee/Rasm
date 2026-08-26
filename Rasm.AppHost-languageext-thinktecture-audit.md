# Rasm.AppHost LanguageExt / Thinktecture audit

## `libs/dotnet/Rasm.AppHost/.planning/Sandbox/isolation.md:182-184`
`from`: `admitted.Match(Succ: IO.pure, Fail: faults => IO.fail<SupplyChainAdmission>(faults.Map(...).Reduce(...)))`
`to`: `IO.lift(admitted.ToFin())`
`why`: `Validation<Error, T>.Fail` already carries one possibly-`ManyErrors` `Error`; `ToFin` plus `IO.lift(Fin<T>)` preserves that error without fictitious sequence operations.

## `libs/dotnet/Rasm.AppHost/.planning/Agent/identity.md:390-394,407-411`
`from`: `Error.Many(errors)` in both `Validation<Error, T>.Match` failure arms
`to`: `errors`
`why`: each failure arm receives the existing `Error`, not `Seq<Error>`; retaining it preserves accumulated members and removes an invalid second aggregation.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/profiles.md:412-415`
`from`: `Admit(profile).Match(Succ: Fin.Succ, Fail: faults => Fin.Fail<ConsumptionProfile>(Error.Many(faults.Map(...).ToSeq())))`
`to`: `Admit(profile).ToFin()`
`why`: LanguageExt owns the exact `Validation<Error, A> -> Fin<A>` egress; the failure is already one accumulated `Error` and has no `Map` member.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/livewire.md:1255-1258`
`from`: `Admit(spec).Match(Succ: admitted => IO.pure(Seated(...)), Fail: errors => IO.fail<BindingHandle>(Error.Many(errors)))`
`to`: `IO.lift(Admit(spec).ToFin()).Map(admitted => Seated(runtime, admitted))`
`why`: `Validation.ToFin` and `IO.lift(Fin<A>)` are the exact existing ingress; the failure is already one accumulated `Error`, so the direct rail preserves it without invalid re-aggregation.

## `libs/dotnet/Rasm.AppHost/.planning/Observability/benchmarks.md:223-228`
`from`: `faults.Exists(static fault => fault is BenchmarkFault.HostMismatch)`
`to`: `faults.AsIterable().Exists(static fault => fault is BenchmarkFault.HostMismatch)`
`why`: `Error.AsIterable()` exposes the accumulated members as LanguageExt's foldable `Iterable<Error>`, whose existing `Exists` preserves the predicate without another carrier conversion.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/lifecycle.md:182,213,396,403,412,420-421,427`
`from`: every `host.Lifted(fin)` / `Lifted(fin)` call plus `internal IO<T> Lifted<T>(Fin<T> settled) => settled.Match(...)`
`to`: `IO.lift(fin)` at each call; delete `Lifted<T>`
`why`: LanguageExt already publishes `IO.lift(Fin<A>)` with identical success/error folding; the local wrapper is one redundant module symbol.

## `libs/dotnet/Rasm.AppHost/.planning/Observability/bundles.md:525-529,535-537,693-696`
`from`: `IO.lift(() => fin).Bind(static settled => settled.Match(Succ: IO.pure, Fail: IO.fail<T>))`
`to`: `IO.lift(() => fin)`
`why`: the exact `IO.lift(Func<Fin<A>>)` overload already lowers the `Fin` failure onto the IO channel; the second fold treats the successful `A` as another `Fin<A>`.

## `libs/dotnet/Rasm.AppHost/.planning/Observability/bundles.md:391-396`
`from`: `IO.lift(() => DumpTriage.Walk(...)).Bind(walked => walked.Match(Succ: IO.pure, Fail: IO.fail<DumpTriage>)).Map(...)`
`to`: `IO.lift(() => DumpTriage.Walk(...)).Map(...)`
`why`: `DumpTriage.Walk` already returns `Fin<DumpTriage>` and `IO.lift(Func<Fin<A>>)` performs the exact failure lowering before `Map`.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/outbound.md:1023-1024,1027-1031`; `libs/dotnet/Rasm.AppHost/.planning/Wire/topics.md:255-259`
`from`: `IO.lift(() => fin).Bind(static settled => settled.Match(Succ: IO.pure, Fail: IO.fail<T>))`
`to`: `IO.lift(() => fin)`
`why`: `IO.lift(Func<Fin<A>>)` performs this flattening itself; removing the duplicate bind preserves the same `Error` and shortens both paths.

## `libs/dotnet/Rasm.AppHost/.planning/Sandbox/isolation.md:214-237`; `libs/dotnet/Rasm.AppHost/.planning/Wire/livewire.md:389-395,1412-1413`
`from`: `IO.lift(() => fin).Bind(static settled => settled.Match(Succ: IO.pure, Fail: IO.fail<T>))`
`to`: `IO.lift(() => fin)`
`why`: the specialized LanguageExt lift already returns `IO<T>`, so the trailing bind is a second, invalid carrier fold.

## `libs/dotnet/Rasm.AppHost/.planning/Sandbox/isolation.md:188,193`; `libs/dotnet/Rasm.AppHost/.planning/Sandbox/solver.md:240`
`from`: `fin.Match(Succ: IO.pure, Fail: IO.fail<T>)`
`to`: `IO.lift(fin)`
`why`: LanguageExt owns the same total `Fin<T> -> IO<T>` fold directly, with no behavior or dependency change.

## `libs/dotnet/Rasm.AppHost/.planning/Agent/runtime.md:65,72,78-79`; `libs/dotnet/Rasm.AppHost/.planning/Agent/capability.md:436,513`; `libs/dotnet/Rasm.AppHost/.planning/Agent/federation.md:272,276-277`
`from`: `fin.Match(Succ: IO.pure, Fail: IO.fail<T>)`
`to`: `IO.lift(fin)`
`why`: the cataloged `IO.lift(Fin<A>)` is the identical settled-result ingress and removes the repeated manual fold.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/livewire.md:524,535,562,613,640-641,700,702,704,736,746,789,841,855,872,890-891,1087,1097,1124,1398,1446-1447,1497`
`from`: `fin.Match(Succ: IO.pure, Fail: IO.fail<T>)`
`to`: `IO.lift(fin)`
`why`: LanguageExt's settled-result lift preserves the exact success and `Error` arms while removing each hand-written fold.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/coordination.md:172,184`
`from`: `fin.Match(Succ: IO.pure, Fail: IO.fail<T>)`
`to`: `IO.lift(fin)`
`why`: `IO.lift(Fin<A>)` is the existing exact conversion, so the local two-arm restatement adds no behavior.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/companion.md:226-238,240-248,250-258`
`from`: `IO<Fin<T>> ... => IO.lift(() => fin)`
`to`: `IO<Fin<T>> ... => IO.lift<Fin<T>>(() => fin)`
`why`: the unqualified call selects `IO.lift(Func<Fin<A>>)` and flattens to `IO<T>`; the explicit existing generic overload preserves the specified delayed `IO<Fin<T>>` carrier.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/time.md:377-390`
`from`: `IO<Fin<FenceStep<TKey>>> Run(...) => IO.lift(() => fin)`
`to`: `IO<Fin<FenceStep<TKey>>> Run(...) => IO.lift<Fin<FenceStep<TKey>>>(() => fin)`
`why`: this port deliberately preserves the store verdict as a value; explicit `A = Fin<FenceStep<TKey>>` selects the ordinary thunk lift instead of LanguageExt's flattening `Func<Fin<A>>` overload.

## `libs/dotnet/Rasm.AppHost/.planning/Sandbox/isolation.md:208,410`; `libs/dotnet/Rasm.AppHost/.planning/Agent/mcp.md:212`; `libs/dotnet/Rasm.AppHost/.planning/Runtime/config.md:289,306`; `libs/dotnet/Rasm.AppHost/.planning/Runtime/modules.md:190`; `libs/dotnet/Rasm.AppHost/.planning/Runtime/profiles.md:925`
`from`: direct `option.Case is T value` / `{ State.Case: T value }` reads
`to`: `option is { IsSome: true, Case: T value }` / `{ State: { IsSome: true, Case: T value } }`
`why`: LanguageExt requires the `IsSome` proof for a `Case` read; the property patterns retain the same present-arm behavior without adding a helper.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/profiles.md:733-740`
`from`: `(reading.Thermal.Case, reading.Power, reading.BatteryFraction) switch { ... }`
`to`: `(reading.Thermal, reading.Power, reading.BatteryFraction) switch { ({ IsSome: true, Case: ThermalPressure heat }, _, _) ... }`
`why`: matching the existing `Option<ThermalPressure>` carries the required presence proof and keeps the same fallback behavior for `None`.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/features.md:102-153`; `libs/dotnet/Rasm.AppHost/.planning/Runtime/modules.md:836-845`
`from`: `IO<Fin<InMemoryProvider>>` / `IO<Fin<Unit>>` plus manual `Fin.Match` arms inside `Compile`, `Register`, and `Reload`
`to`: `IO<InMemoryProvider>` / `IO<Unit>` using `IO.lift(Fin<A>)`, then bind the provider directly and return bare values from the async bodies
`why`: `IO` already owns the `Error` channel and the module immediately runs it to `Fin`; flattening once removes the nested carrier, four branch arms, and makes the existing `.Run().Map(compiled => Some(compiled))` type correctly.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/modules.md:112-120`; `libs/dotnet/Rasm.AppHost/.planning/Runtime/config.md:118`
`from`: `toSeq(rows).OrderBy(...).ToSeq()`
`to`: `toSeq(rows.OrderBy(...))`
`why`: LINQ `OrderBy` exits the LanguageExt carrier and has no `.ToSeq()` extension; the catalogued `Prelude.toSeq(IEnumerable<A>)` is the one re-entry and removes the redundant first conversion.

## `libs/dotnet/Rasm.AppHost/.planning/Agent/capability.md:281-290`
`from`: `bySurface[q.Surface].ToSeq()`, `byId.Values.Where(...).ToSeq()`, and `byId.Values.ToSeq()`
`to`: `toSeq(bySurface[q.Surface])`, `toSeq(byId.Values.Where(...))`, and `toSeq(byId.Values)`
`why`: `ILookup` groups and `FrozenDictionary.Values` are BCL `IEnumerable` shapes; only `Prelude.toSeq` admits them to `Seq`, so the current instance-style calls bind no API.

## `libs/dotnet/Rasm.AppHost/.planning/Agent/capability.md:87-91`; `libs/dotnet/Rasm.AppHost/.planning/Runtime/features.md:234-238`
`from`: `HashMap<K,V>.Fold(..., (state, pair) => ... pair.Key ... pair.Value ...)`
`to`: `HashMap<K,V>.AsIterable().Fold(..., (state, pair) => ... pair.Key ... pair.Value ...)`
`why`: the two-parameter `HashMap` fold visits `V` only; LanguageExt's existing `AsIterable()` is the keyed `(K Key, V Value)` view required by `MeterVector` and `Features.Context`.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/companion.md:790-795`
`from`: `Range(start, count).Map(Cloexec).ToSeq().Strict()`
`to`: `toSeq(Enumerable.Range(start, count)).Map(Cloexec).Strict()`
`why`: LanguageExt `Range<A>` is foldable but publishes no `Map`; the catalog explicitly routes projected integer spans through `Enumerable.Range` and `Prelude.toSeq`.

## `libs/dotnet/Rasm.AppHost/.planning/Observability/egress.md:160-176`
`from`: `{ Case: DrainPass.Settled }`, `{ Case: PersistentBlob blob }`, and `{ Case: byte[] body }` over `Option<T>` values
`to`: `{ IsSome: true, Case: DrainPass.Settled }`, `{ IsSome: true, Case: PersistentBlob blob }`, and `{ IsSome: true, Case: byte[] body }`
`why`: LanguageExt requires the `IsSome` proof before reading `Option<T>.Case`; the strengthened property patterns preserve every existing `None` fallback with no helper.

## `libs/dotnet/Rasm.AppHost/.planning/Observability/benchmarks.md:209-220`
`from`: `(Fin<Unit>, Fin<Unit>).Apply(...).ToValidation()`
`to`: `(Fin<Unit>, Fin<Unit>).Apply(...).As().ToValidation()`
`why`: applicative `Apply` returns `K<Fin,Unit>`; LanguageExt's existing `.As()` must re-anchor it to concrete `Fin<Unit>` before the instance `ToValidation()` is available.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/features.md:177-190`
`from`: `TargetingRule` type-pattern `switch` with `_ => false`
`to`: `rule.Switch(all:, tenantIn:, attributeEquals:, segmentBand:)`
`why`: `TargetingRule` is an existing Thinktecture `[Union]`; generated exhaustive dispatch preserves all four results and makes a new rule case break compilation instead of silently becoming false.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/features.md:109-112`
`from`: `held.Resolve(...).ToValidation<Error, FlagDefinition>(new KernelFault.InvalidValue(...))`
`to`: `held.Resolve(...).ToValidation<Error>(new KernelFault.InvalidValue(...))`
`why`: the receiver already fixes `A = FlagDefinition`; LanguageExt `Option<A>.ToValidation<L>(L)` has only the failure-type generic, so the second explicit type argument is not an available API.

## `libs/dotnet/Rasm.AppHost/.planning/Agent/capability.md:505-509`
`from`: `CommandTxn` type-pattern `switch` with `_ => None`
`to`: `txn.Switch(committed:, rolledBack:, compensated:, refused:)`
`why`: `CommandTxn` is an existing Thinktecture `[Union]`; its generated fold expresses the same two present and two absent arms without a catch-all that silently swallows a future transaction case.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/outbox.md:303-322`
`from`: `settled.State switch { Deferred ..., DeadLettered ..., _ => RelayRejected }`
`to`: `settled.State.Switch(pending: _ => IO.fail<RelayResult>(...), deferred: ..., deadLettered: ...)`
`why`: `RelayState` is an existing Thinktecture `[Union]`; its generated exhaustive fold preserves the pending refusal and both active arms while preventing a new state from falling into a fabricated generic rejection.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/determinism.md:181-187,261,339,538,588`
`from`: `ChainHash.Of(UInt128) => ChainHash.Create(UInt128)` and every `ChainHash.Of(...)` call
`to`: delete `ChainHash.Of`; call the generated `ChainHash.Create(...)` directly
`why`: Thinktecture already generates the exact value-object factory; `Of` adds no admission or domain behavior, so deleting it removes one module-level symbol and one-hop rename wrapper.

## `libs/dotnet/Rasm.AppHost/.planning/Agent/runtime.md:25-31,148`; `libs/dotnet/Rasm.AppHost/.planning/Runtime/orchestration.md:305`
`from`: `CommandIntent.Of(...) => new CommandIntent(...)` and both `CommandIntent.Of(...)` calls
`to`: delete `CommandIntent.Of`; construct the existing positional record directly at the two call sites
`why`: `Of` is a pure constructor-forwarding module symbol with no admission or policy; the record constructor is the pre-existing owner and preserves the exact value.

## `libs/dotnet/Rasm.AppHost/.planning/Sandbox/isolation.md:280-290,428-431`
`from`: `plugin.Quota.Breach(...).Match(Succ: IO.pure, Fail: IO.fail<Option<Breach>>)`
`to`: `IO.lift(plugin.Quota.Breach(...))`
`why`: LanguageExt's direct `IO.lift(Fin<A>)` is the exact settled-result ingress, preserving the breach value and error while removing both repeated manual folds.

## `libs/dotnet/Rasm.AppHost/.planning/Sandbox/provisioning.md:311-313`; `libs/dotnet/Rasm.AppHost/.planning/Agent/reasoning.md:92-96`; `libs/dotnet/Rasm.AppHost/.planning/Observability/health.md:765-768`; `libs/dotnet/Rasm.AppHost/.planning/Runtime/orchestration.md:437,448`; `libs/dotnet/Rasm.AppHost/.planning/Agent/capability.md:288`
`from`: `.Take(...).ToSeq()`, `.Skip(...).ToSeq()`, and `Seq.Choose(...).ToSeq()` / `Seq<Option<T>>.Somes().ToSeq()`
`to`: remove the trailing `.ToSeq()` calls
`why`: LanguageExt `Seq.Take`, `Seq.Skip`, `SeqExtensions.Choose`, and `OptionExtensions.Somes(Seq<Option<A>>)` already return concrete `Seq`; each conversion is a redundant carrier restatement.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/outbound.md:257-261,776-781`; `libs/dotnet/Rasm.AppHost/.planning/Wire/topics.md:222-226`
`from`: `fin.Match(Succ: Validation<Error,T>.Success, Fail: Validation<Error,T>.Fail)` inside each `Traverse`
`to`: `fin.ToValidation()`; for `Seat`, use `Claim(...).ToValidation().Map(static _ => unit)`
`why`: LanguageExt's existing `Fin.ToValidation()` preserves the same success and `Error` arms directly, removing six manually reconstructed branches.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/determinism.md:348-351`
`from`: `ContentHash.Admit(...).Match(Succ: Validation<Error,UInt128>.Success, Fail: _ => new ReplayFault.ChainBroken(...))`
`to`: `ContentHash.Admit(...).MapFail(_ => new ReplayFault.ChainBroken(row.Sequence, "row-decode")).ToValidation()`
`why`: `Fin.MapFail` retains the intentional replay-fault projection and `Fin.ToValidation()` performs the existing carrier ingress without restating either arm.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/coordination.md:421-425`
`from`: `outcome.Match(Succ: Success<Error,A>, Fail: error => Fail<Error,A>(new CoordinationFault.FenceRejected(held.Key, error)))`
`to`: `outcome.MapFail(error => new CoordinationFault.FenceRejected(held.Key, error)).ToValidation()`
`why`: `Fin.MapFail` preserves the guarded-key fault projection and `Fin.ToValidation()` performs the exact existing accumulation ingress.

## `libs/dotnet/Rasm.AppHost/.planning/Sandbox/isolation.md:203-211`; `libs/dotnet/Rasm.AppHost/.planning/Sandbox/provisioning.md:163-170`
`from`: `IO.lift(() => Op.Catch(...))` followed by a `Fin.Match` that deliberately inspects the verdict
`to`: `IO.lift<Fin<T>>(() => Op.Catch(...))` (`T` is the catch's success type; `Unit` in `Rollover`)
`why`: ordinary generic lift must be selected explicitly here; the specialized `Func<Fin<A>>` overload flattens the verdict before the trap/reverted arms can inspect it.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/time.md:222-228`
`from`: `IO.lift(() => clocks.Gauged(... body: () => Op.Catch(() => Fin.Succ(Redrive.Run(...).Run())))).Bind(gauged => gauged.Match(...))`
`to`: `IO.lift(() => clocks.Gauged(... body: () => Op.Catch(() => Redrive.Run(...).Run())))`
`why`: `Op.Catch` already accepts the returned `Fin<Unit>`, and result-thunk lift already lowers `Gauged`'s outer `Fin`; both extra carrier shells are redundant while the measured inner verdict remains intact.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/companion.md:187-195`
`from`: `IO.lift(() => manifestOf(None)).Bind(read => read.Match(Succ: manifest => IO.pure(new CompanionPeer(...)), Fail: IO.fail<CompanionPeer>))`
`to`: `IO.lift(() => manifestOf(None)).Map(manifest => new CompanionPeer(row.Topology, None, Discovery.Connect(manifest, policy), manifest))`
`why`: `manifestOf` returns `Fin<DiscoveryManifest>` and `IO.lift(Func<Fin<A>>)` already lowers its failure; only the successful peer projection remains.

## `libs/dotnet/Rasm.AppHost/.planning/Observability/bundles.md:153-160`
`from`: `IO.lift(tally.Read).Bind(readings => readings.Match(Succ: rows => IO.pure(Rendered(rows,...)), Fail: fault => IO.fail(...)))`
`to`: `IO.lift(() => tally.Read(Op.Of()).MapFail(fault => new SupportFault.ContributorFaulted(...))).Map(rows => Rendered(rows,...))`
`why`: domain `InstrumentTally.Read` requires its existing `Op` argument and returns `Fin<Seq<InstrumentReading>>`; `MapFail` plus result-thunk lift preserves the named fault and removes the invalid second fold.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/orchestration.md:290-298`
`from`: flattened `Load`/`SignalPut` results followed by `Fin.Match` branches that remap both failures to `ResumeBroken`
`to`: `IO.lift(runtime.Store.Load(...).MapFail(_ => new ResumeBroken(...)))`, then the same for `SignalPut`, then `Drive(runtime, instance)`
`why`: `Fin.MapFail` preserves the two deliberate error projections before `IO.lift(Fin<A>)`; each lifted binder then receives the successful value, not another `Fin`.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/orchestration.md:441-450`
`from`: `IO.lift(() => runtime.Store.Expired(...)).Bind(expired => expired.Match(Succ: orphans => orphans.TraverseM(...), Fail: _ => IO.pure(Seq())))`
`to`: `IO.lift(() => runtime.Store.Expired(...).IfFail(static _ => Seq<WorkflowInstance>())).Bind(orphans => orphans.TraverseM(orphan => LeaseElection.Acquire(...).Match(...)).As().Map(static rows => rows.Somes()))`
`why`: `Fin.IfFail` is the existing failure-to-value escape and preserves the intentional empty fallback before ordinary IO lifting, without treating the flattened sequence as a `Fin`.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/outbox.md:286-292,303-320`
`from`: the three `IO.lift(() => fin).Bind(value => value.Match(Succ: ..., Fail: ...))` folds around `Fence/Advance`, `Parked`, and dead-letter `Fence/DeadLetter`
`to`: first path `.Map(cursor => new RelayResult(None, Some(cursor))).Catch(static _ => true, fault => Settle(runtime, tenant, row, fault))`; `Parked` and dead-letter paths use only `.Map(_/cursor => new RelayResult(...))`
`why`: result-thunk lift already lowers each `Fin` failure; catalogued `IO.Catch(predicate, handler)` retains Dialled's all-error recovery while the two direct-failure paths need only success projection.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/outbox.md:158-165`
`from`: `pending.State switch { Pending when ..., Deferred when ..., _ => Fin.Fail(...) }`
`to`: `pending.State.Switch(pending: s => s.Attempt == 0 ? Fin.Succ(s) : Fin.Fail(...), deferred: s => s.Attempt > 0 ? Fin.Succ(s) : Fin.Fail(...), deadLettered: _ => Fin.Fail(...))`
`why`: `RelayState` is an existing Thinktecture `[Union]`; generated exhaustive dispatch preserves both attempt guards and the dead-letter refusal without a catch-all swallowing a future case.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/livewire.md:1248-1253`
`from`: nested `value.Reading.Match(Some: reading => QuantityFamily.Admit(...).Match(Succ: evidence => Fin.Succ(new Coercion(...)), Fail: Fin.Fail), None: stale)`
`to`: `value.Reading.ToFin(new WireFault.StaleSource($"{value.Reason.Value}@{value.SourceAt}")).Bind(reading => spec.Family.Admit(...).Map(evidence => new Coercion(...)))`
`why`: `Option.ToFin`, `QuantityFamily.Admit`'s existing `Fin<MeasureEvidence>`, and `Fin.Map` preserve both current failures and the success projection without four rebuilt arms.

## `libs/dotnet/Rasm.AppHost/.planning/Observability/telemetry.md:546-555`
`from`: `InstrumentMount.Mount(...).Match(Succ: signals => Fin.Succ(new TelemetryComposition(...)), Fail: refused => (Dispose(), Fin.Fail<TelemetryComposition>(refused)).Item2)`
`to`: `InstrumentMount.Mount(...).Map(signals => new TelemetryComposition(...)).MapFail(refused => (fun(meters.Dispose)(), refused).Item2)`
`why`: `Fin.Map` and `MapFail` are the existing two projections; the failure map retains the mandatory provider disposal while removing the reconstructed `Fin` arms.

## `libs/dotnet/Rasm.AppHost/.planning/Observability/benchmarks.md:411-414`
`from`: `Op.Catch(() => Fin.Succ(call())).Match(Succ: held => held, Fail: _ => Option<T>.None)`
`to`: `Op.Catch(() => Fin.Succ(call())).IfFail(static _ => Option<T>.None)`
`why`: `Fin.IfFail` is the exact existing failure-to-value escape and preserves the successful `Option<T>` whole without a two-arm restatement.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/coordination.md:228-235`
`from`: `Validation<Error,Unit> outcome` followed by `outcome.Match(Succ: _ => IO.pure(unit), Fail: IO.fail<Unit>)`
`to`: `IO.lift(outcome.ToFin())`
`why`: LanguageExt already owns both carrier crossings; `Validation.ToFin` retains the accumulated `Error` and `IO.lift(Fin<Unit>)` lowers it directly.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/determinism.md:547-555`
`from`: `TraverseM(entry => entry.Body is LogBody.Command command ? Fin.Succ(...) : Fin.Fail(...)).As().Match(Succ: Batch, Fail: IO.fail)`
`to`: `IO.lift(macro.Commands.TraverseM(entry => entry.Body.Switch(command: command => Fin.Succ(...), chaos: _ => Fin.Fail(...))).As()).Bind(steps => CommandAlgebra.Batch(runtime.Command, steps))`
`why`: generated `LogBody.Switch` makes the two-case classification exhaustive, and `IO.lift(Fin<A>)` preserves the traversal failure without rebuilding its arms.

## `libs/dotnet/Rasm.AppHost/.planning/Observability/bundles.md:380-389`; `libs/dotnet/Rasm.AppHost/.planning/Wire/livewire.md:1017-1023`
`from`: `IO.liftAsync(...Fin<T>...).Bind(fin => fin.Match(Succ: IO.pure, Fail: IO.fail<T>))`
`to`: `IO.liftAsync(...Fin<T>...).Bind(static fin => IO.lift(fin))`
`why`: `liftAsync(Func<Task<Fin<T>>>)` preserves `Fin<T>` as its payload, after which the existing `IO.lift(Fin<T>)` performs the exact second crossing without restating both arms.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/outbox.md:278-283`
`from`: `IO.lift(() => (Error)new OutboxFault.ClassificationBarred(...)).Bind(cause => Settle(runtime, tenant, row, cause))`
`to`: `Settle(runtime, tenant, row, new OutboxFault.ClassificationBarred(...))`
`why`: the fault is an already-constructed immutable value and `Settle` already returns `IO<RelayResult>`; the preceding pure IO shell delays no boundary work and adds no behavior.

## `libs/dotnet/Rasm.AppHost/.planning/Agent/identity.md:197-209`
`from`: `Validate(...) => Admit(runtime, token)` plus the one-use private `Admit(...) => Parsed(...).Match(...)`
`to`: move the existing `Parsed(...).Match(...)` body onto `Validate`; delete `Admit`
`why`: the private method adds no admission, carrier, policy, or reuse; `Validate` is the existing public owner, so inlining removes one module-level symbol and one forwarding hop.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/ports.md:504-509`
`from`: `TenantId.TryOf(wire).Match(Some: tenant => Validation.Success(Some(tenant)), None: () => Violation(...))`
`to`: `TenantId.TryOf(wire).ToValidation<Error>(Violation(new WireViolation.Tenant(wire))).Map(static tenant => Some(tenant))`
`why`: Thinktecture's generated `TryOf` already supplies the `Option<TenantId>` and LanguageExt `Option.ToValidation` preserves the same missing-id violation without rebuilding both arms.

## `libs/dotnet/Rasm.AppHost/.planning/Agent/capability.md:668-672`; `libs/dotnet/Rasm.AppHost/.planning/Observability/telemetry.md:432-435`
`from`: `Resolve(...).Match(Some: descriptor => Admit(...), None: OutOfScope)` and `Items.Find(...).Match(Some: Fin.Succ, None: Unrostered)`
`to`: `registry.Resolve(step.Id).ToFin(new GrantFault.OutOfScope(step.Id)).Bind(descriptor => Admit(...))`; `Items.Find(...).ToFin(new TelemetryFault.Unrostered(name))`
`why`: LanguageExt `Option.ToFin` is the exact existing missing-value ingress and preserves both domain failures while removing four manually reconstructed arms.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/time.md:307-310`
`from`: `held.Match(Some: arm, None: () => Fin.Fail<Option<FencingToken>>(new KernelFault.InvalidValue(...)))`
`to`: `held.ToFin(new KernelFault.InvalidValue(...)).Bind(arm)`
`why`: LanguageExt `Option.ToFin` preserves the identical missing-generation fault and `Fin.Bind` retains the selected `FenceVerb` continuation without an explicit two-arm fold.

## `libs/dotnet/Rasm.AppHost/.planning/Observability/bundles.md:550-555`
`from`: `Op.Of().Catch(() => Fin.Succ(release())).Bind(static outcome => outcome).Match(...)`
`to`: `Op.Of().Catch(release).Match(...)`
`why`: `release` already has the exact `Func<Fin<Unit>>` shape domain `Op.Catch` accepts; passing it directly preserves thrown and returned failures while deleting the nested `Fin` and flattening bind.

## `libs/dotnet/Rasm.AppHost/.planning/Sandbox/admission.md:137-147`; `libs/dotnet/Rasm.AppHost/.planning/Runtime/modules.md:103-108`
`from`: `Op.Catch(() => anchor.Switch(...).Map(provider => new Runtime(...))).Bind(static admitted => admitted)` / `Op.Catch(() => Fin.Succ(Applied(...))).Bind(static admitted => admitted)`
`to`: `Op.Catch(() => anchor.Switch(...).Map(provider => new Runtime(...)))` / `Op.Catch(() => Applied(services, module))`
`why`: both existing bodies already return the exact `Fin<T>` accepted by domain `Op.Catch`; direct admission preserves thrown and returned failures and deletes each fabricated nested carrier.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/companion.md:735`; `libs/dotnet/Rasm.AppHost/.planning/Runtime/modules.md:883-887`
`from`: `IO<Fin<BoundEndpoint>> Acquire(...) => IO.pure(Bound(request))`, then `.Run().Bind(static bound => bound).Map(static _ => unit)`
`to`: `IO<BoundEndpoint> Acquire(...) => IO.lift(Bound(request))`, then `.Run().Map(static _ => unit)`
`why`: `Bound` already returns `Fin<BoundEndpoint>` and LanguageExt `IO.lift(Fin<A>)` owns its failure channel; the nested `Fin` and identity bind carry no distinct verdict.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/lifecycle.md:327-331`
`from`: `Op.Catch(...).Match(Succ: tuple, Fail: tuple).Apply(Some)`
`to`: `Some(Op.Catch(...).Match(Succ: tuple, Fail: tuple))`
`why`: LanguageExt `Apply` is K-kinded applicative fan-in, not a generic value pipe; direct `Some` wraps the identical settled tuple and removes the invalid call.

## `libs/dotnet/Rasm.AppHost/.planning/Runtime/laneguard.md:57-58`; `libs/dotnet/Rasm.AppHost/.planning/Wire/outbound.md:55-56`; `libs/dotnet/Rasm.AppHost/.planning/Wire/livewire.md:94-95`
`from`: handwritten `Validate(..., out owner)` ternaries producing `Option<PipelineKey>`, `Option<HopKey>`, or a fallback `Symbol`
`to`: `Op.Of().AcceptValidated<PipelineKey>(reported).ToOption()` / `Op.Of().AcceptValidated<HopKey>(reported).ToOption()` / `Op.Of().AcceptValidated<Symbol>(text).IfFail(_ => fallback)`
`why`: domain `Op.AcceptValidated` is the existing Thinktecture factory-evidence bridge; the current absence/fallback semantics remain while three out-parameter transcriptions disappear.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/livewire.md:566,645,750,877-879,1098,1409-1411`
`from`: `option.Match(Some: IO.pure, None: () => IO.fail<T>(fault))`
`to`: `IO.lift(option.ToFin(fault))`
`why`: LanguageExt `Option.ToFin` plus `IO.lift(Fin<A>)` is the exact existing absence-to-error rail, preserving each local fault without rebuilding either arm.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/livewire.md:1404-1408`
`from`: `prior.Reading.Match(Some: _ => IO.pure(prior), None: () => IO.fail<ExternalValue>(stale))`
`to`: `IO.lift(prior.Reading.ToFin(stale)).Map(_ => prior)`
`why`: LanguageExt `Option.ToFin` and `IO.lift(Fin<A>)` preserve the same stale failure; the final map retains the intentional projection from the present reading back to `prior`.

## `libs/dotnet/Rasm.AppHost/.planning/Wire/livewire.md:1138-1143`
`from`: `observations.Last.Match(Some: newest => IO.lift(() => { Advance(...); return newest.Value; }), None: () => IO.fail<ExternalValue>(stale))`
`to`: `IO.lift(observations.Last.ToFin(stale)).Bind(newest => IO.lift(() => { Advance(...); return newest.Value; }))`
`why`: `Option.ToFin` preserves the stale fault and sequences the cursor mutation only for `Some`, removing the manual presence fold without changing effects.

## `libs/dotnet/Rasm.AppHost/.planning/Observability/bundles.md:118`
`from`: `new(verdict is Masked.Redacted ? 1 : 0)`
`to`: `new(verdict.Switch(unchanged: static _ => 0, redacted: static _ => 1))`
`why`: domain `Masked` is an existing Thinktecture `[Union]`; generated total dispatch preserves both counts and makes a future case break instead of silently counting zero.

## `libs/dotnet/Rasm.AppHost/.planning/Agent/capability.md:445-460`
`from`: `Succ: body => Posture(descriptor) switch { var spec => from dispatched in Governed(runtime, spec, body, arguments) ... }`
`to`: `Succ: body => from dispatched in Governed(runtime, Posture(descriptor), body, arguments) ...`
`why`: `spec` is a one-use identity-pattern binding with no dispatch or policy; composing the existing `Posture` result directly deletes the meaningless switch without changing evaluation count.
