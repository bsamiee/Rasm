# [APPHOST_COMMAND_DISPATCH]

`CommandIntent` is the suite's one command vocabulary and `Run(CommandIntent) -> CommandReceipt` its one execution door: the MCP tool call, the reasoning loop, the operator console, the sandboxed-plugin grant handle, the workflow step, the bus subscription, and every UI row a referencing package derives all enter here, pass the hook rail's `Command` veto, ride the one `Sandbox/isolation#GRANT_HANDLE` mediation that records the caller modality, dispatch through `Agent/capability#COMMAND_ALGEBRA` `CommandAlgebra.Run`, and chain their receipt into the one `Runtime/determinism#EVENT_LOG`.

This page declares the front door, its veto rail, and the tool-adoption seam the agent callers share. `CommandAlgebra` stays the one transaction owner, `EventLog` stays the one chained log on the durable `OpLog`, and no second dispatcher forks off here — it consumes `CommandAlgebra`/`CommandRuntime`/`CommandReceipt`/`CommandFault`/`CommandBody`, `McpRuntime`/`CommandAIFunction`/`ToolProjection`/`McpAdoption`/`McpDispatch`/`ToolResult`, `GrantHandleSurface`/`MediationRuntime`/`BrokeredCall`/`CallerModality`/`GrantScope`, `EventLog`/`ChangefeedPort`/`DeterminismContext`, `HookRail`, `CapabilityRegistry`, `TenantContext`, and `ClockPolicy` as settled vocabulary and mints no eighth port.

## [01]-[INDEX]

- [02]-[DISPATCH_FRONT_DOOR]: One veto-gated, mediation-recorded `Run` entry chaining every committed receipt into the one event log.
- [03]-[ADOPTION_SEAM]: One `ToolProjection.Adopt` product, three agent front doors reading disjoint halves of it.

## [02]-[DISPATCH_FRONT_DOOR]

- Owner: `CommandIntent` the front-door request carrier (descriptor id, arguments, caller modality); `DispatchRuntime` the dependency record threading the `CommandRuntime`, the mediation runtime, the per-caller scope resolver, the `EventLog` chain cell, the determinism context, the changefeed port, and the `Observability/hooks#HOOK_ROSTER` `HookRail<AppHostPoint, AppHostFact, TelemetrySource>` whose `AppHostPoint.Command` row this entry is the ONE fire site for, and the `Op` key every fire and every refusal threads; `CommandDispatch` the static front-door surface.
- Cases: six callers, one transaction — `CallerModality.Agent` for the MCP tool call and the in-process reasoning loop, `CallerModality.Operator` for the interactive host command, `CallerModality.Plugin` for the sandboxed-plugin route, with the durable workflow step and the event-bus subscription each entering under the modality that scheduled them.
- Entry: `Run(DispatchRuntime runtime, CommandIntent intent)` returns `IO<CommandReceipt>` — fires the rail's `Command` veto, folds its `Fin`, mediates the admitted intent, dispatches through `CommandAlgebra.Run`, chains the receipt, and returns it; `Project(CommandReceipt receipt, Option<string> tool = default)` returns `ToolResult` — THE structured projection every front door reads, delegating to the upstream `Agent/mcp#TOOL_DISPATCH` `McpDispatch.Project` fold with the descriptor as the tool key, so exactly one physical fold exists.
- Auto: the veto fires FIRST through the rail's GUARDED arity — `rail.Fire(at: AppHostPoint.Command, fact: new AppHostFact.Command(intent), key, body)` hands the body its admitted fact and answers `Fin<CommandIntent>`, so a transforming gate supplies the descriptor and arguments the transaction then uses, a subscriber returning a case the seam never fired refuses at the body's own case check, and a refusing gate lands `CommandFault.Vetoed` through the algebra's own refusal mint, meaning a refused command never opens a transaction, never brokers a grant, never meters a cost, and still leaves the same receipt on the same fan every dispatched command leaves; the admitted intent then crosses the ONE `GrantHandleSurface.Mediate` fold, which is what makes `CallerModality` a recorded fact rather than a column nothing reads — the mediation mints the `BrokeredCall` carrying the caller, the permitted flag, and the charged vector, with the transaction itself as the dispatch closure, so an operator call, an agent call, and a plugin call are one transaction discriminated by evidence rather than by three parallel dispatchers; every committed or compensated dispatch chains its receipt into the `EventLog` through the publish-free `EventLog.Project` inside a BOUNDED `Cell.Commit`, and the ONE `ChangefeedPort.Publish` runs after the head lands — so a contended head costs re-derivations and nothing durable, a chain that cannot advance inside its budget refuses as typed exhaustion instead of spinning, and no consumer of the feed observes an entry no head points at; re-minting per attempt is the chain's own requirement, because an entry's hash chains to its predecessor and a contended advance must re-derive against the head that won; the chain cell is an `Atom` so a dispatched command and a reasoning-transcript chain advance the same head under concurrent front doors; every stamp on this path reads `runtime.Command.Clocks`, the transaction's own handle, because two clock handles on one dispatch can disagree about when the command they stamp happened.
- Receipt: each dispatch mints one `CommandReceipt` — the command algebra's own, fanned through its `ReceiptSinkPort.Send` under `TelemetrySource.AppHost` and `ReceiptKind.Command` — plus one `BrokeredCall` at the mediation and one `EventLog.LogEntry` at the chain advance; no parallel dispatch receipt.
- Packages: Rasm (kernel `HookRail`, `Cell.Commit`/`Transition`, `ContentHash`), LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new front door is the SAME `Run` entry a new caller invokes carrying its `CallerModality`; a new orchestration consumer drives the same `Run`; zero new surface.
- Boundary: `CommandIntent` is the SUITE's command identity and no referencing package re-declares it — `Rasm.AppUi/Shell/commands#INTENT_TABLE` `CommandRow` is the UI DERIVATION over this vocabulary, holding the presentation columns (chord, palette text, mount predicate, argument schema) and minting one `CommandIntent` through `CommandRow.ToIntent` at each raise, so a UI verb reaches its work through this `Run` and never beside it; a second type named `CommandIntent` in a package that references this one is a strata twin, not a naming coincidence, because both spellings reach one compile leg and dispatch then resolves against whichever page a call site happened to cite; this front door is the one command-execution entry, and a caller reaching `CommandAlgebra.Run` directly is the deleted form — that bypass strands the veto rail with no firing site, leaves the caller modality unrecorded, and lands a dispatched command outside the hash chain, three failures one shared entry forecloses at once; `CommandAlgebra` stays the one commit-or-rollback transaction at `Agent/capability#COMMAND_ALGEBRA` and this surface is the gate over it, never a second transaction; `EventLog` stays the one hash-chained content-addressed command log on the durable `OpLog` changefeed (`Runtime/determinism#EVENT_LOG`) and both the mint and the publish are that owner's members, so this page composes `Project` and `Publish` in that order and derives no link of its own; the append takes a BODY — `LogBody.Command(descriptor, arguments.Digest)` — so the digest covers the canonical argument bytes and never the descriptor a second time; the veto rail is the app-composed admission policy's seat and this page names no point id and no modality of its own, reading only `Admitted`; a command whose caller resolves no `GrantScope` falls through to the algebra alone, where the broker's consent seat refuses it — one admission decision, never a second gate that could disagree with it.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record CommandIntent(
    string Descriptor,
    CommandArguments Arguments,
    CallerModality Caller) {
    public static CommandIntent Of(string descriptor, CommandArguments arguments, CallerModality caller) =>
        new(descriptor, arguments, caller);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record DispatchRuntime(
    CommandRuntime Command,
    MediationRuntime Mediation,
    Func<CommandIntent, Option<GrantScope>> ScopeOf,
    Atom<EventLog.Chain> Chain,
    DeterminismContext Context,
    ChangefeedPort Changefeed,
    HookRail<AppHostPoint, AppHostFact, TelemetrySource> Rail,
    Op Key);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CommandDispatch {
    public static IO<CommandReceipt> Run(DispatchRuntime runtime, CommandIntent intent) =>
        from admitted in IO.lift(() => runtime.Rail.Fire(
            at: AppHostPoint.Command,
            fact: new AppHostFact.Command(Intent: intent),
            key: runtime.Key,
            body: static fact => fact is AppHostFact.Command passed
                ? Fin.Succ(passed.Intent)
                : Fin.Fail<CommandIntent>(new KernelFault.InvalidResult(Op.Of(), Some(nameof(AppHostFact.Command))))))
        from receipt in admitted.Match(
            Succ: passed => Mediated(runtime, passed),
            Fail: refusal => CommandAlgebra.Refuse(runtime.Command, intent.Descriptor, new CommandFault.Vetoed(refusal.Message, refusal), intent.Arguments))
        from _chained in Chain(runtime, receipt, intent.Arguments)
        select receipt;

    static IO<CommandReceipt> Mediated(DispatchRuntime runtime, CommandIntent intent) =>
        runtime.ScopeOf(intent).Match(
            Some: scope => GrantHandleSurface.Mediate(runtime.Mediation, intent.Caller, scope, intent.Descriptor, intent.Arguments,
                    (_, _) => CommandAlgebra.Run(runtime.Command, intent.Descriptor, intent.Arguments))
                .Map(static mediated => mediated.Receipt),
            None: () => CommandAlgebra.Run(runtime.Command, intent.Descriptor, intent.Arguments));

    static IO<Unit> Chain(DispatchRuntime runtime, CommandReceipt receipt, CommandArguments arguments) =>
        receipt.Txn is CommandTxn.Committed or CommandTxn.Compensated
            ? from at in IO.lift(() => runtime.Command.Clocks.Now)
              from entry in Advanced(runtime, receipt, arguments, at).Match(Succ: IO.pure, Fail: IO.fail<LogEntry>)
              from published in runtime.Changefeed.Publish(entry).Match(Succ: IO.pure, Fail: IO.fail<Unit>)
              select published
            : IO.pure(unit);

    static Fin<LogEntry> Advanced(DispatchRuntime runtime, CommandReceipt receipt, CommandArguments arguments, Instant at) {
        LogEntry? minted = null;
        Transition<EventLog.Chain> landed = Cell.Commit(runtime.Chain, held => {
            (EventLog.Chain next, LogEntry entry) = EventLog.Project(
                held, new LogBody.Command(receipt.Descriptor, arguments.Digest), runtime.Context, at, (ulong)held.Sequence);
            minted = entry;
            return next;
        }, Cell.SwapBudget);
        return landed is Transition<EventLog.Chain>.Contended spent
            ? Fin.Fail<LogEntry>(new CommandFault.ExecutionFaulted($"chain-contended:{spent.Attempts.Value}"))
            : Fin.Succ(minted!);
    }

    public static ToolResult Project(CommandReceipt receipt, Option<string> tool = default) =>
        McpDispatch.Project(tool.IfNone(receipt.Descriptor), receipt);
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: One command-dispatch front door, one veto, one chain
    accDescr: Every caller invokes CommandDispatch.Run; the hook rail vetoes or transforms the intent, the mediation records the caller modality, the command algebra owns the transaction, and a committed receipt chains into the one event log while publishing through the changefeed port.
    Mcp["MCP tool call (agent)"] --> Run
    Loop["reasoning loop (agent)"] --> Run
    Plugin["GrantHandle (plugin)"] --> Run
    Operator["operator console"] --> Run
    Step["workflow step"] --> Run
    Sub["bus subscription"] --> Run
    Run["CommandDispatch.Run"] --> Veto["rail.Fire: AppHostPoint.Command"]
    Veto -->|refused| Refuse["CommandAlgebra.Refuse: Vetoed"]
    Veto -->|admitted| Mediate["GrantHandleSurface.Mediate: BrokeredCall"]
    Mediate --> Algebra["CommandAlgebra.Run (one transaction)"]
    Algebra --> Chain["EventLog.Append: chain advance + changefeed publish"]
```

## [03]-[ADOPTION_SEAM]

- Owner: the composition binding that turns ONE `Agent/mcp#METHOD_AXIS` `ToolProjection.Adopt` product into every agent front door's tool surface; `Agent/mcp.md` authors the `CommandAIFunction` subclass and the `McpServerTool.Create` adoption, and this page declares what each front door takes from that one product.
- Cases: three consumers, three disjoint halves of one `McpAdoption` — MCP registration takes `ServerTools` beside the prompt and resource primitives, the in-process reasoning loop takes each row's `Function` as its `AITool` set, and the plugin route takes neither, entering at `Run` with its own modality.
- Entry: the composition projects the catalog once at the live degradation level, adopts once, and hands each front door its half; every model draw is the endpoint's own through the one governed pipeline — the served revision's election deletes the client-sampling bridge whole, so no fourth carrier exists and a federated tool reaches no client model.
- Auto: adoption happens ONCE per composition and every front door reads that one product — a front door re-projecting the catalog for itself mints a second function identity under the same tool name, so a model tool call and an MCP tool call would resolve two brokered instances with two adoption-time captures; the brokered function's tenant and correlation resolve per invocation on the caller's own async flow, which is what makes one caller-neutral product safe to share across every agent; each row's `Command` half is the bare `CommandAIFunction` the receipt-asserting invoker reaches through the approval wrapper, so the irreversible class stays inside the assertion built for it rather than exempted by its own wrapper; the plugin route carries no tool catalog at all, because its authority is the mediated grant handle and a catalog handed to a sandbox is exactly the ambient authority the isolation law deletes.
- Packages: ModelContextProtocol, ModelContextProtocol.Core, Microsoft.Extensions.AI.Abstractions, LanguageExt.Core, BCL inbox
- Growth: a new tool front door is the SAME `McpAdoption` product read by a new consumer, never a new projection; zero new surface.
- Boundary: `ToolProjection.Adopt` is the one SDK-adoption site and it lives at `Agent/mcp.md` — this page binds its product and never re-authors the subclass or the `McpServerTool.Create` call, so the SDK adoption stays fenced at one site; a tool set divorced from that seam is the deleted form, so the MCP server, the reasoning loop, and the plugin route share one brokered catalog and one dispatch transaction, never three.

```csharp
// --- [COMPOSITION] ---------------------------------------------------------------------
McpAdoption adopted = ToolProjection.Adopt(
    mcpRuntime,
    ToolProjection.Project(mcpRuntime));

IMcpServerBuilder server = services.AddMcpServer()
    .WithTools(adopted.ServerTools)
    .WithPrompts(adopted.Prompts)
    .WithResources(adopted.Resources);

Seq<AITool> agentTools = adopted.Tools.Map(static row => (AITool)row.Function);

Func<string, CommandArguments, IO<CommandReceipt>> pluginDispatch =
    (descriptor, arguments) => CommandDispatch.Run(dispatchRuntime, CommandIntent.Of(descriptor, arguments, CallerModality.Plugin));
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
