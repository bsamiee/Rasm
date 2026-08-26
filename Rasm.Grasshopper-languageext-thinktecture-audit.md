# Rasm.Grasshopper LanguageExt and Thinktecture audit

## `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/wires.md:188`
`from` `wires.Map(row => Fin<GhMark>).Sequence().As()` at lines 188-193.
`to` `wires.Traverse(row => Fin<GhMark>).As()` with the existing projection body unchanged.
`why` LanguageExt's concrete landing is fused `Traverse`; `Map(...).Sequence()` leaves an abstract inner carrier and does not produce `Fin<Seq<GhMark>>`.

## `libs/dotnet/Rasm.Grasshopper/.planning/Components/data.md:71`
`from` the four `HostCall.Run` forwarding overloads and all `HostCall.Run` call sites in `Components/*.md`.
`to` `op.Catch(() => Fin.Succ(call()), token)` for bare values, direct `op.Catch(finBody, token)` for `Fin<T>` bodies (deleting `Bind(identity)` at `objects.md:742`), `op.Catch(action)` for untokened actions, and the existing tokened `Op.Side` form for tokened actions; delete `HostCall`.
`why` `Rasm.Domain.Op.Catch` already owns the same exception and cancellation classification; the wrapper contributes no Grasshopper policy and costs one module type plus four members.

## `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/wires.md:75`
`from` each untokened `op.Catch(() => Fin.Succ(Op.Side(action)))`: `Canvas/{wires:75,motion:270,canvas:425-511}`, `Document/history.md:74`, `Platform/{capture:333-539,native:236-377}`, and `Shell/{chrome:281-411,journal:111,icons:185-222,session:134-182}`.
`to` `op.Catch(action)` with the existing action body unchanged.
`why` `Rasm.Domain.Op.Catch(Action)` is the exact existing exception funnel returning `Fin<Unit>`; the nested `Fin.Succ` and `Op.Side` duplicate its implementation at every site.

## `libs/dotnet/Rasm.Grasshopper/.planning/Components/data.md:369`
`from` `toSeq(rows.Value.Values.Filter(...).OrderBy(...).Map(entry => entry.Row))`.
`to` `toSeq(rows.ToSeq().Filter(...).OrderBy(...)).Map(static entry => entry.Row)`.
`why` LanguageExt `AtomHashMap` exposes values through `ToSeq`, not `.Value.Values`, and LINQ `OrderBy` exits the carrier before LanguageExt `Map`; the replacement uses both exact existing surfaces.

## `libs/dotnet/Rasm.Grasshopper/.planning/Components/data.md:318`
`from` `Option.Map(predicate).IfNone(false)` for the document/plugin scope tests at lines 318-319 and the trim-axis test in `Components/ports.md:591-593`.
`to` `option.Exists(predicate)` with each existing predicate unchanged.
`why` LanguageExt `Option.Exists` is the exact present-value predicate with false on absence, so the intermediate `Option<bool>` and fallback are redundant.

## `libs/dotnet/Rasm.Grasshopper/.planning/Shell/events.md:122`
`from` local `Wired<THost,TArgs>` and its uses at lines 129-220.
`to` existing `Rasm.Interaction.EventTable<THost,TArgs>`; delete `Wired`.
`why` `EventTable` is the identical public `Add`/`Drop` pair with the same `EventArgs` constraint, so attachment and detachment behavior are unchanged.

## `libs/dotnet/Rasm.Grasshopper/.planning/Components/attributes.md:26`
`from` local `PointerKind` and `KeyPhase`, plus their uses at lines 72-73 and 176-220.
`to` `Rasm.Interaction.PointerPhase` and `Rasm.Interaction.KeyPhase` (`KeyDown`/`KeyUp`); delete both local generated owners.
`why` the kernel already owns the exact eight pointer phases and two key phases used for these GH2 callbacks.

## `libs/dotnet/Rasm.Grasshopper/.planning/Components/attributes.md:194`
`from` identity-success `Fin.Match` folds at lines 194-198; `Components/component.md:256-258,283-288,327-329,375-378,387-410`; `Canvas/canvas.md:466-471`; `Eto/runtime.md:45-47`; and `Platform/native.md:262-264`.
`to` `.IfFail(existingFailureFallback)` on the same `Fin`, deleting every `Succ: identity`, `Succ: result => result`, or `Succ: _ => unit` arm.
`why` LanguageExt `Fin.IfFail(Func<Error,A>)` returns every success unchanged and substitutes only a failed value, exactly the current folds' behavior.

## `libs/dotnet/Rasm.Grasshopper/.planning/Document/graph.md:66`
`from` `PinSide.Inlet`/`PinSide.Outlet` at lines 66, 69 and 77-78.
`to` the existing `Components/ports.md` rows `PinSide.Input`/`PinSide.Output`.
`why` `PinSide` declares only `Input` and `Output`; the four current references name nonexistent generated members, while the replacement is the folder's canonical side vocabulary.

## `libs/dotnet/Rasm.Grasshopper/.planning/Shell/chrome.md:378`
`from` `Flat`/`Tinted` at lines 414-418 and `Shell/icons.md`'s `Named` at lines 185, 188 and 252-253.
`to` direct `Op.ToHostSlot` for reference options and `Op.ToHostNullable` for `Option<Color>`; delete all three helpers.
`why` `Rasm.Domain.Op` already owns both Option-to-host nullable projections, including the class/struct split these helpers duplicate.

## `libs/dotnet/Rasm.Grasshopper/.planning/Components/component.md:151`
`from` manual reference-option null projections at lines 151, 156; `Components/objects.md:760`; `Shell/chrome.md:304`; `Shell/editor.md:174`; `Shell/session.md:136`; `Shell/telemetry.md:255,263-264,296`; `Platform/native.md:370-390`.
`to` `Op.ToHostSlot(option)`, retaining `!` only where the host annotation requires it and mapping inside the `Option` first where a value needs formatting or array conversion.
`why` every site is the exact nullable host-slot boundary centralized by `Rasm.Domain.Op.ToHostSlot`; no fallback or failure semantics change.

## `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/interaction.md:158`
`from` `hooks.Match(Some: live => live.Fire(...).Match(Succ: _ => Fin.Succ(verdict), Fail: _ => Fin.Succ(Ignored)), None: () => Fin.Succ(verdict))`.
`to` `hooks.TraverseM(live => live.Fire(...)).As().Map(_ => verdict).BindFail(_ => Fin.Succ(InputVerdict.Ignored))`.
`why` LanguageExt's option traversal owns absence-as-success and `BindFail` owns the deliberate hook-failure recovery; both original result policies remain unchanged.

## `libs/dotnet/Rasm.Grasshopper/.planning/Components/objects.md:523`
`from` recursive `Ancestry(Type?)` at lines 523-528.
`to` inline `toSeq(LanguageExt.List.unfold(host, static current => Optional(current).Map(type => (type, type.BaseType))))`; delete `Ancestry`.
`why` LanguageExt's state-seeded unfold yields the same type-to-base-type order and removes one recursive module member.

## `libs/dotnet/Rasm.Grasshopper/.planning/Components/objects.md:43`
`from` unused generated owners `AccumulationMode` at lines 43-51 and `BoundaryRole` at lines 53-61.
`to` delete both declarations and their owner prose; no replacement call site is needed.
`why` neither type has a fenced consumer anywhere in the folder, so the two host-enum mirrors and their generated surfaces implement no behavior.

## `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/layout.md:203`
`from` `rows.Zip(toSeq(Range(0, rows.Count))).Traverse(pair => pair.Item1.IsValid ? Validation.Success(...) : Validation.Fail(...))`.
`to` indexed `rows.Map((row, index) => (Row: row, Index: index)).Traverse(pair => key.AcceptValue(pair.Row).MapFail(_ => new KernelFault.InvalidValue(... pair.Index ...)).ToValidation())`.
`why` LanguageExt indexed `Seq.Map` removes the synthetic range, and `Rasm.Domain.Op.AcceptValue` is the sole `IValidityEvidence` oracle; `MapFail` preserves the indexed fault and applicative accumulation.

## `libs/dotnet/Rasm.Grasshopper/.planning/Platform/native.md:320`
`from` three nullable locals lifted solely into `Optional(...).Match(Some: cleanup, None: Fin.Succ(unit))` at lines 320-324, 347-351 and 374-381.
`to` direct nullable patterns, `local is { } active ? cleanup(active) : Fin.Succ(unit)`, with each existing `Custody.Release` body unchanged.
`why` these are frame-local nullable unwind sentinels, not domain presence; plain C# preserves cleanup behavior with no transient `Option` shell.

## `libs/dotnet/Rasm.Grasshopper/.planning/Platform/capture.md:414`
`from` four nonexistent `Guard.ToValidation()` calls at lines 414-418, with line 414 also re-spelling `CapturePace`'s finite-positive invariant.
`to` call `.ToFin().ToValidation()` on each guard, and use `CapturePace.TryCreate((double)plan.Pace, out _)` as line 414's predicate.
`why` LanguageExt exposes `Guard.ToFin` and `Fin.ToValidation`, not `Guard.ToValidation`; Thinktecture's generated `TryCreate` is the existing pace admission owner.

## `libs/dotnet/Rasm.Grasshopper/.planning/Platform/capture.md:424`
`from` `Optional(refusal).Match(Some: fault => Fin.Fail(NativeFailure(fault)), None: () => Fin.Succ(unit))`.
`to` `refusal is { } fault ? Fin.Fail<Unit>(NativeFailure(fault)) : Fin.Succ(unit)`.
`why` the nullable callback result is consumed immediately and never enters a carrier pipeline; the direct pattern is behavior-identical and smaller.

## `libs/dotnet/Rasm.Grasshopper/.planning/Shell/editor.md:192`
`from` `Optional(shell.Documents.Current).IsSome`.
`to` `shell.Documents.Current is not null`.
`why` this is a one-bit host-null probe, so constructing a LanguageExt carrier adds no semantics and plain C# is the smaller exact expression.

## `libs/dotnet/Rasm.Grasshopper/.planning/Shell/icons.md:189`
`from` manual nullable-struct matches for `Option<Duration>` and `Option<Motion>` at lines 189-190.
`to` `Op.ToHostNullable(c.Span)` and `Op.ToHostNullable(c.Curve)`.
`why` `Rasm.Domain.Op.ToHostNullable<T>` already owns the struct-option projection and returns the same `T?` host arguments.

## `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/canvas.md:507`
`from` `delay.Match(Some: span => guard(...).ToFin().Map(_ => Some(span)), None: () => Fin.Succ(None))`.
`to` `delay.TraverseM(span => guard(span >= TimeSpan.Zero, op.InvalidInput()).ToFin().Map(_ => span)).As()`.
`why` LanguageExt `Option.TraverseM` is total over absence and preserves the same `Fin<Option<TimeSpan>>` without rebuilding either carrier arm.

## `libs/dotnet/Rasm.Grasshopper/.planning/Components/component.md:351`
`from` `Option.Match` folds with a `Fin.Succ(unit)` `None` arm in `AppendToInputPanel` at lines 317-319, both `Stage` overloads and `Maintained` at lines 351-368.
`to` `option.TraverseM(action => key.Catch(...)).As().Map(static _ => unit)`, retaining the surrounding `Bind` operations.
`why` LanguageExt's total option traversal already sequences each optional `Fin` effect; it preserves no-op absence and the existing fault paths with fewer branch arms.

## `libs/dotnet/Rasm.Grasshopper/.planning/Components/component.md:398`
`from` both `access.Map(effectReturningUnit).IfNone(unit)` pipelines at lines 398-404 and 415-416.
`to` `access.Iter(effect)` with the existing notice-reporting bodies unchanged.
`why` LanguageExt `Option.Iter(Action<A>)` is the existing present-arm effect fold and returns `Unit`; the mapped `Option<Unit>` is never consumed.

## `libs/dotnet/Rasm.Grasshopper/.planning/Components/ports.md:344`
`from` `PinTrim` omits `IValidityEvidence`, then `Realize` manually reads `trim.IsValid` inside nested `Trim.Match(...).Bind(_ => Persistent.Match(...))` at lines 188-224 and 344-354.
`to` add `: IValidityEvidence`; sequence `Trim.TraverseM(trim => op.AcceptValue(trim).MapFail(_ => existing ContractRefused).Bind(valid => valid.Apply(parameter, op))).As()` then the existing persistent write through `Persistent.TraverseM(...).As()`, ending in `Map(_ => unit)`.
`why` `Rasm.Domain.Op.AcceptValue` is the registered validity oracle and `Option.TraverseM` owns both absent no-ops; remapping refusal to the existing `GhFault` preserves the public failure contract.

## `libs/dotnet/Rasm.Grasshopper/.planning/Shell/icons.md:255`
`from` `state.Match(Some: name => key.Catch(...), None: () => Fin.Succ(unit))`.
`to` `state.TraverseM(name => key.Catch(...)).As().Map(static _ => unit)`.
`why` LanguageExt `Option.TraverseM` preserves the optional lookup effect and makes absent state the same successful no-op without a manual fold.

## `libs/dotnet/Rasm.Grasshopper/.planning/Shell/telemetry.md:258`
`from` `GhTelemetry.Seat.Value.Match(Some: held => write(held.Instruments), None: () => Fin.Succ(unit))`.
`to` `GhTelemetry.Seat.Value.TraverseM(held => write(held.Instruments)).As().Map(static _ => unit)`.
`why` LanguageExt's total option traversal preserves the same optional telemetry effect and successful unseated no-op without a manual result fold.

## `libs/dotnet/Rasm.Grasshopper/.planning/Document/history.md:111`
`from` duplicate optional-hook `Match(Some: live => live.Fire(...).Map(_ => unit), None: () => Fin.Succ(unit))` in `history.md:111-119`, `solution.md:113-121`, and `graph.md:333-341`.
`to` `hooks.TraverseM(live => live.Fire(...)).As().Map(static _ => unit)` at all three sites.
`why` LanguageExt `Option.TraverseM` owns the same absent-hook no-op and fail-fast hook effect, deleting six duplicated carrier arms without moving hook policy.

## `libs/dotnet/Rasm.Grasshopper/.planning/Components/objects.md:710`
`from` three `Seq.Map(id => Fin<Unit>).TraverseM(identity).As()` pipelines at lines 710-717 and 778-784.
`to` `Seq.TraverseM(id => Fin<Unit>).As()` with each existing lambda body unchanged.
`why` LanguageExt monadic traversal fuses the projection and sequencing directly; the intermediate `Seq<Fin<Unit>>` has no consumer or policy.

## `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/motion.md:239`
`from` `drives.Map(row => MotionDrive.Admit(...).Map(_ => row)).TraverseM(identity).As()`.
`to` `drives.TraverseM(row => MotionDrive.Admit(...).Map(_ => row)).As()`.
`why` LanguageExt `TraverseM` already owns the effectful projection and sequencing, preserving order and short-circuiting while deleting the intermediate carrier.

## `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/layout.md:300`
`from` the three `Map(row => Fin<...>).TraverseM(identity).As()` pipelines at lines 300-303, 322-324 and 338-340.
`to` direct `TraverseM(row => Fin<...>).As()` pipelines, retaining the existing final `Strict` projections.
`why` LanguageExt's fused monadic traversal has the same ordered fail-fast result and removes three unused intermediate `Seq<Fin<...>>` values.
