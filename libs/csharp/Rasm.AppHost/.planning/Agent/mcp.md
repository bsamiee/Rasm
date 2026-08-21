# [APPHOST_MCP_PROJECTION]

Model Context Protocol serving for the runtime spine rides the official `ModelContextProtocol` SDK, which owns JSON-RPC framing, transport, initialization, and error mapping, serving the stateless revision the folder's revision-election ruling settles. This page projects the capability registry onto its tool/resource/prompt surface. Each `CapabilityDescriptor` projects once to a brokered `Microsoft.Extensions.AI` `AIFunction` and its adopted `McpServerTool`; `McpAdoptedTool` exposes that exact pair to MCP registration and in-process reasoning.

Brokered dry-runs price tool calls before invocation, dispatch routes through the command algebra and yields the `CommandReceipt` the transport edge projects, a mid-call ask suspends onto the SDK's input-required rail and the client's retry answers it under one echoed round identity, and the protocol's own poll leg carries long-running calls over the cancel spine. This page owns the method axis, descriptor-to-`AIFunction` projection, the round-idempotent brokered dispatch, the input-required overage ask, and the agent-session roster. It consumes `CapabilityRegistry`/`DiscoveryQuery`, `CommandAlgebra`/`GrantBroker`, `ControlInbound.DispatchTool`, `CancelScope`, `TenantContext`, and `ReceiptSinkPort` as settled vocabulary and mints no eighth port.

## [01]-[INDEX]

- [02]-[METHOD_AXIS]: MCP method vocabulary with tool, resource, and prompt projection from the registry.
- [03]-[TOOL_DISPATCH]: Dry-run cost preview, brokered dispatch idempotent across MRTR rounds, the input-required overage ask, and the transport-edge result projection.
- [04]-[STREAM_PROGRESS]: Server-stream progress fan with cancellation, backpressure, and the request-polling leg a disconnected agent re-reads.
- [05]-[PROTOCOL_FACES]: Three shapes the SDK emits on the wire — tool catalog, progress notification, structured result.
- [06]-[APP_ROOT]: Service-host-root builder fold mounting the adopted primitives and the one ingress filter.

## [02]-[METHOD_AXIS]

- Owner: `McpMethod` `[SmartEnum<string>]` the MCP method vocabulary under the `ComparerAccessors.StringOrdinal` accessor; `ToolProjection` the descriptor-to-tool fold; `McpTool` the projected tool descriptor; `McpAnnotations` the one boundary projection of the SDK's tool hints off the effect class and the key regime; `McpAdoptedTool` the brokered function/server pair; `McpAdoption` the whole registration product carrying the tool, prompt, and resource primitives the server mounts; `McpResource` the projected resource handle; `McpPrompt` the projected prompt template.
- Cases: 8 method rows — initialize, tools-list, tools-call, resources-list, resources-read, prompts-list, prompts-get, ping — the closed MCP request surface; tool/resource/prompt projections fold the registry's `DiscoveryResult` rows.
- Entry: `Project(CapabilityRegistry registry, DegradationLevel level, Func<DiscoveryResult, JsonNode> schemaOf, JsonNode receiptSchema)` returns `McpCatalog` — one fold projects the level-gated discovery result into the MCP tool catalog (each tool carrying its descriptor input schema and the uniform `CommandReceipt` output schema), so an agent sees exactly the tools the host can serve at its current degradation; `Tool(DiscoveryResult descriptor, JsonNode inputSchema, JsonNode outputSchema)` is the single descriptor-to-tool projection; `Adopt(McpRuntime runtime, McpCatalog catalog)` returns `McpAdoption` — one fold constructs each caller-neutral brokered function beside its SDK serving type AND mints the prompt and resource primitives off the same effect-filtered rows, safe to reuse across every agent because no caller identity is baked at adoption.
- Auto: each `DiscoveryResult` projects to one `Microsoft.Extensions.AI.AIFunction` (the `AIFunction : AIFunctionDeclaration : AITool` chain, where `JsonSchema` is a `JsonElement` on `AIFunctionDeclaration` and `Name`/`Description` are virtuals on `AITool`) whose overridden `JsonSchema` is the `JsonSchemaExporter` schema the descriptor's `CommandArguments` resolves through `SuiteContracts.Schema`, so the SDK's `inputSchema` derives from the same schema the codegen and command binder read, never a hand-authored JSON Schema and never the SDK's reflected delegate-parameter schema; the projection adopts a `CommandAIFunction : AIFunction` subclass whose `InvokeCoreAsync` resolves the ambient `TenantContext.Current` and mints a fresh `CorrelationId` per invocation on the caller's async flow — tenant identity and correlation are per-call facts, never adoption-time captures a boot-adopted tool would replay for every later caller — keeping `payload` the sole agent-facing input, and overrides `JsonSchema` to the descriptor schema, and `McpServerTool.Create(AIFunction, McpServerToolCreateOptions)` adopts it, with the projection setting the `McpServerToolCreateOptions` annotations from the descriptor's `EffectClass` (`pure`/`read` set `ReadOnly`, `write`/`external`/`irreversible` set `Destructive`) and `Idempotency` so an agent reads the side-effect class from the SDK's tool metadata; an `irreversible`-effect descriptor wraps its `CommandAIFunction` in the catalogued `ApprovalRequiredAIFunction` before `McpServerTool.Create` adopts it, so the destructive-side-effect class is a real human-in-the-loop approval gate the SDK enforces before invoke rather than only the advisory `Destructive` bool hint — the descriptor effect class drives both the metadata annotation and the enforcing wrapper from one source, never a parallel approval flag; the `Destructive` knob is `bool?` and the SDK treats unset/`true` as destructive, meaningful only when `ReadOnly=false`, so the projection always sets both explicitly with `ReadOnly` forcing `Destructive=false`, never inheriting the destructive default on an unset path; `Permitting` gating means a degraded host registers only the still-servable tools with zero parallel catalog.
- Auto: a call needing more than one round trip rides the SDK's own multi-round-trip input rail, never a host-held task cell — a tool body throws `InputRequiredException` carrying its `InputRequest` asks and an opaque `RequestState`, the client resolves each locally and RETRIES the same `tools/call` with the answers plus the echoed state, and `McpServer.IsMrtrSupported` is the guard a body checks before choosing that route. The retry is what makes the rail durable without host state: a suspended call holds nothing on this side of the wire, so a host restart between rounds costs the client one re-send rather than a lost in-flight task. The consequence the brokered dispatch owns is IDEMPOTENCE — a body reached again for the same `RequestState` must not re-charge its grant or re-mint its receipt, so the pre-flight ask keys off that state and `CommandAlgebra.Run` stays the single commit the last round reaches.
- Receipt: the projection is a pure fold producing the brokered `AIFunction`/registered `McpServerTool` pairs; every dispatched call's `CommandReceipt` crosses `AppHostPoint.Receipt` through the `AppHostHooks.Tap` sink decoration and its intent crosses `AppHostPoint.Command` at the `Agent/runtime#DISPATCH_FRONT_DOOR` veto seat, so this page spells no fire of its own; the served-method transition logs through one `SpineLog` event in the 1000-1099 EVENT stride (`FaultBand.SpineEvents`) — no parallel projection receipt.
- Packages: ModelContextProtocol, ModelContextProtocol.Core, Microsoft.Extensions.AI.Abstractions, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new method row tracks a new MCP request kind the SDK already serves; a new projection target is one fold arm on `Adopt` plus its `With*` registration leg; a new ingress concern is one `AddIncomingFilter` row; zero new surface — the agent transport is the registry projected onto the SDK, never a parallel command catalog.
- Boundary: the MCP projection is a read-only view of the capability registry — an MCP-specific tool definition divorced from a `CapabilityDescriptor` is the deleted form, so every advertised tool is a real registry descriptor adopted as an `AIFunction` and every tool call routes through the command algebra; the projection mints exactly one fault union, `McpFault` in the 4640 band at TOOL_DISPATCH, and consumes neither namespace-fenced `WireFault` — the `Rasm.Compute.Remote` `WireFault` (the Compute Remote gRPC `StatusCode` rail, mirror-pinned in the `Runtime/lifecycle#FAULT_TABLES` registry) and the `Rasm.AppHost.LiveWire` `WireFault` (the external-binding rail, `FaultBand.LiveWire`) are distinct types in distinct namespaces, and a single blanket `using` pulling both `Rasm.Compute` and `Rasm.AppHost` collides on the bare `WireFault` symbol, so any consumer touching both references each through its namespace-qualified path or a `using`-alias, never a bare import, per `docs/stacks/csharp/language#FORM_CHOOSER`; the JSON-RPC framing, the initialize handshake, and the method dispatch belong to the SDK — a hand-rolled JSON-RPC dispatcher is the deleted form, so `McpMethod` is the closed vocabulary the projection reads to gate per-method behavior, never a transport re-implementation; a host-specific verb rides the `ControlService` `DispatchTool` route instead, never a tenth MCP method; resource and prompt projections read the same descriptor rows filtered by effect class — a `read` descriptor projects as both a tool and a resource, a `pure` template-shaped descriptor projects as a prompt — so one descriptor source serves all three MCP surfaces, and all three REGISTER: a projection that computes resource and prompt rows and then chains `WithTools` alone is the deleted form, since it answers `resources/list` and `prompts/list` empty while the catalog advertises four of its eight declared methods; tool names are the descriptor ids verbatim so the SDK's `tools/call` resolves through `CapabilityRegistry.Resolve` with no name translation; the page-local `McpTool`/`McpResource`/`McpPrompt` records are the projected descriptors and `ToolProjection.Adopt` is the only SDK-adoption seam — its returned `McpAdoptedTool` rows carry the exact brokered `AIFunction` consumed by reasoning and the matching `McpServerTool` consumed by registration, so neither consumer reconstructs the function surface.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class McpMethod {
    public static readonly McpMethod Initialize = new("initialize");
    public static readonly McpMethod ToolsList = new("tools/list");
    public static readonly McpMethod ToolsCall = new("tools/call");
    public static readonly McpMethod ResourcesList = new("resources/list");
    public static readonly McpMethod ResourcesRead = new("resources/read");
    public static readonly McpMethod PromptsList = new("prompts/list");
    public static readonly McpMethod PromptsGet = new("prompts/get");
    public static readonly McpMethod Ping = new("ping");
}

// Effect and idempotency ride the row as COLUMNS and every SDK annotation derives from them: the invoker's
// route-ceiling gate needs the class itself, which no boolean carries.
// --- [MODELS] ----------------------------------------------------------------------------
public sealed record McpTool(
    string Name,
    string Title,
    JsonNode InputSchema,
    JsonNode OutputSchema,
    EffectClass Effect,
    // Named `Repeat` rather than `Idempotency`: a column whose name equals its own type name resolves to the
    // column inside the record, making every static row on the type unreachable from its own members.
    Idempotency Repeat,
    MeterVector EstimatedCost) {
    public McpAnnotations Hints => McpAnnotations.Of(Effect, Repeat);
}

// SDK tool hints project ONCE as a boundary value off exactly two facts, rather than
// four `bool` members on the domain record each reader could re-derive differently. `Destructive` is the
// complement of `ReadOnly` and never a fourth answer; `Approval` also decides the serving wrapper.
public readonly record struct McpAnnotations(bool ReadOnly, bool Idempotent, bool Approval) {
    public bool Destructive => !ReadOnly;

    public static McpAnnotations Of(EffectClass effect, Idempotency repeat) =>
        new(ReadOnly: effect.Rank <= EffectClass.Read.Rank,
            Idempotent: Repeatable(repeat.Regime),
            Approval: effect == EffectClass.Irreversible);

    // `idempotentHint` asks whether repeating a call with the same arguments adds no further effect, which the
    // KEY REGIME answers directly: an intrinsically repeat-safe op and a caller-keyed one do, a host-minted
    // once key and an absent key do not. A fifth regime breaks this arm rather than falling through it.
    static bool Repeatable(KeyRegime regime) => regime.Switch(
        intrinsic: static () => true,
        supplied: static () => true,
        minted: static () => false,
        absent: static () => false);
}

public sealed record McpResource(string Uri, string Name, string Surface);

public sealed record McpPrompt(string Name, JsonNode ArgumentsSchema);

public sealed record McpCatalog(
    Seq<McpTool> Tools,
    Seq<McpResource> Resources,
    Seq<McpPrompt> Prompts) {
    public static readonly McpCatalog Empty = new([], [], []);
}

// Two columns because two consumers need different halves: the model and the SDK take `Function`, while the
// receipt-asserting invoker and the prompt and resource mints take `Command`, so a wrapper cannot hide the
// brokered identity from the assertion that exists to check it.
public sealed record McpAdoptedTool(
    McpTool Descriptor,
    CommandAIFunction Command,
    AIFunction Function,
    McpServerTool ServerTool);

public sealed record McpAdoption(
    Seq<McpAdoptedTool> Tools,
    Seq<McpServerPrompt> Prompts,
    Seq<McpServerResource> Resources) {
    public IEnumerable<McpServerTool> ServerTools => Tools.Map(static adopted => adopted.ServerTool);
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class ToolProjection {
    public static McpTool Tool(DiscoveryResult descriptor, JsonNode inputSchema, JsonNode outputSchema) =>
        new(
            Name: descriptor.Descriptor,
            Title: descriptor.Surface,
            InputSchema: inputSchema,
            OutputSchema: outputSchema,
            Effect: EffectClass.Get(descriptor.Effect),
            Repeat: Idempotency.Get(descriptor.Idempotency),
            EstimatedCost: descriptor.Estimated);

    public static McpCatalog Project(CapabilityRegistry registry, DegradationLevel level, Func<DiscoveryResult, JsonNode> schemaOf, JsonNode receiptSchema) =>
        registry.Discover(new DiscoveryQuery.Permitting(level)) is var rows
            ? new McpCatalog(
                Tools: rows.Map(row => Tool(row, schemaOf(row), receiptSchema)),
                Resources: rows.Filter(static row => row.Effect is "pure" or "read").Map(static row => new McpResource($"rasm://{row.Surface}/{row.Descriptor}", row.Descriptor, row.Surface)),
                Prompts: rows.Filter(static row => row.Effect is "pure").Map(row => new McpPrompt(row.Descriptor, schemaOf(row))))
            : McpCatalog.Empty;

    // All three primitives share ONE mint shape — `McpServerPrompt.Create` and `McpServerResource.Create` are
    // exact `AIFunction` peers of `McpServerTool.Create` — so the tool leg folds FIRST into a descriptor-keyed
    // index of brokered functions and the prompt and resource legs each look their own row up in it. Handing
    // those two mints one delegate where the delegate takes two routes them around the broker entirely.
    public static McpAdoption Adopt(McpRuntime runtime, McpCatalog catalog) {
        var tools = catalog.Tools.Map(tool => Adopted(runtime, tool));
        var brokered = tools.ToFrozenDictionary(static row => row.Descriptor.Name, static row => row.Command, StringComparer.Ordinal);
        return new(
            Tools: tools,
            Prompts: catalog.Prompts.Choose(prompt => brokered.TryGetValue(prompt.Name, out var command)
                ? Some(runtime.MintPrompt(prompt, command))
                : Option<McpServerPrompt>.None),
            Resources: catalog.Resources.Choose(resource => brokered.TryGetValue(resource.Name, out var command)
                ? Some(runtime.MintResource(resource, command))
                : Option<McpServerResource>.None));
    }

    // Both the brokered function and its serving wrapper ride the adopted row, so an approval wrapper never
    // hides the brokered identity from the invoker that must assert on it.
    static McpAdoptedTool Adopted(McpRuntime runtime, McpTool tool) {
        var command = new CommandAIFunction(runtime, tool);
        McpAnnotations hints = tool.Hints;
        AIFunction served = hints.Approval ? new ApprovalRequiredAIFunction(command) : command;
        return new McpAdoptedTool(
                tool,
                command,
                served,
                McpServerTool.Create(
                    served,
                    new McpServerToolCreateOptions {
                        Name = tool.Name,
                        Title = tool.Title,
                        ReadOnly = hints.ReadOnly,
                        Destructive = hints.Destructive,
                        Idempotent = hints.Idempotent,
                        // `UseStructuredContent` is what carries the receipt across the protocol boundary;
                        // unset, the receipt degrades to a text block under a schema nothing fills.
                        UseStructuredContent = true,
                        // Left unset the SDK defaults `OutputSchema` to the return type's shape, which is
                        // the invoker's and never the receipt's.
                        OutputSchema = JsonSerializer.SerializeToElement(tool.OutputSchema, runtime.Wire),
                        SerializerOptions = runtime.Wire,
                    }));
    }
}

// --- [COMPOSITION] -----------------------------------------------------------------------
public sealed class CommandAIFunction(McpRuntime runtime, McpTool tool) : AIFunction {
    public override string Name => tool.Name;
    public override string Description => tool.Title;

    public EffectClass Effect => tool.Effect;
    public override JsonElement JsonSchema { get; } = JsonSerializer.SerializeToElement(tool.InputSchema, runtime.Wire);

    // Tenant identity is a per-invocation fact resolved on the caller's async flow, never an adoption-time
    // capture; `Correlation` ADOPTS the ambient MRTR round, so every round of one logical call keys ONE
    // identity and the ask, the transient consent, the charge, and the receipt each land once.
    //
    // The return is the `CommandReceipt` itself — the in-process invoker carries it verbatim onto the function
    // result and the serving leg serializes it into `CallToolResult.StructuredContent` under the declared
    // schema. The MRTR suspension crosses the IO rail as its own exception identity, so THIS edge unwraps and
    // rethrows it raw: the SDK's input_required framing keys on the type, and a wrapped one serializes as a
    // tool fault instead of a suspension.
    protected override async ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken) {
        try {
            return await McpDispatch.Call(runtime, tool.Name, ServerInitiated.Round.Match(
                    Some: round => new CommandArguments((JsonElement)arguments["payload"]!, TenantContext.Current, round.Correlation, Some(round.State)),
                    None: () => new CommandArguments((JsonElement)arguments["payload"]!, TenantContext.Current, Correlation.Mint())))
                .RunAsync(EnvIO.New(token: cancellationToken));
        }
        catch (ErrorException raised) when (raised.ToError().Exception.Case is InputRequiredException suspension) {
            throw suspension;
        }
    }
}
```

## [03]-[TOOL_DISPATCH]

- Owner: `McpFault` `[Union]` fault family riding the kernel `[FaultCase]`/`Fault` floor — `[FaultCase]` realizes the registry over `FaultBand.Mcp`, the band the MCP transport maps onto its JSON-RPC error frame (the JSON-RPC intent keeps 4640, the registry enforcing disjointness); `CostPreview` the dry-run pricing record; `CostApproval` the elicited overage answer; `McpRound` the one-logical-call identity a retry echoes across MRTR rounds; `ToolResult` the transport-edge structured result; `ServerInitiated` the static round plane owning the ambient session handle, the ambient retry round, and the input-required overage ask; `McpDispatch` the static brokered-dispatch surface.
- Cases: `McpFault` = UnknownTool | InvalidArguments | CostRejected | Cancelled | ExecutionFailed | Uncompensated | Incomplete | Vetoed | Shed — each mapping to a JSON-RPC error code at the transport edge, so a client can distinguish a retriable execution fault from an unwind that left state uncertain.
- Entry: `Preview(McpRuntime runtime, string tool, CommandArguments arguments)` returns `IO<CostPreview>` — the dry-run cost preview prices the tool call through `GrantBroker.Admit(…, DrawMode.Priced)` and returns the estimated cost and whether the standing grant covers it, before any execution; `Call(McpRuntime runtime, string tool, CommandArguments arguments)` returns `IO<CommandReceipt>` — the brokered dispatch prices the call, surfaces an uncovered price through elicitation, and routes through `CommandAlgebra.Run`, yielding the receipt itself; `Project(string tool, CommandReceipt receipt)` returns `ToolResult` — THE one transport-edge projection of a receipt onto the structured result and its fault mapping; `ServerInitiated.Live` and `ServerInitiated.Retry` are the two one-level `AmbientSlot` values whose `Enter` the ingress filter and the call-tool request filter open; `ServerInitiated.Confirm(McpRuntime runtime, CostPreview preview, CommandArguments arguments)` returns `IO<Fin<CostApproval>>` — the input-required overage ask carrying the exact shortfall unit and price, suspending a first round on the MRTR rail and refusing on the typed rail where the client lacks it.
- Auto: the preview reuses the broker's admission fold so the previewed price is the exact price the live call charges, never an estimate that drifts from the charge, and an uncovered FIRST round surfaces its price through the input-required ask BEFORE any byte moves — the ask ASKS and the broker DECIDES, so an approved retry lands the transient `Consent.Elevated` the composition's consent seat reads for that round key, and a refusal, a declined answer, or a client without the MRTR rail all fall through to the broker's own ceiling refusal, leaving exactly one refusal owner and one receipt mint; a retry round never re-asks — `Call` reads `CommandArguments.Round` and routes straight to the algebra, so the pre-flight keys off the echoed `RequestState`, `GrantBroker.Admit(…, DrawMode.Priced)` runs once per logical call rather than once per round, and `CommandAlgebra.Run` stays the single commit the last round reaches; the ingress filter seats the session handle beside the tenant scope on every transport, because `McpServer.IsMrtrSupported` reads off that handle; the dispatch routes through `ControlInbound.DispatchTool` so an agent call on a companion lands through the same audit-and-redaction seam an operator tool call lands through; `Call` yields the `CommandReceipt` and `Project` is where it becomes a `CallToolResult` body, so the one value both the in-process invoker and the serving leg need crosses each seam once; a `CommandTxn.Refused` projects to the matching `McpFault` whose generated code the SDK maps onto its JSON-RPC `-32xxx` error frame so a denied tool call returns a protocol error, never a thrown exception — the mapping direction is one-way, the host emitting the 4640-band application code and the SDK framing it from a thrown `McpException` or a returned `JsonRpcError` at the transport edge, so the interior never emits a reserved-range code directly and never re-numbers a fault into it; `McpFault`, `CommandFault`, and `GrantFault` all declare `ConversionFromValue = ConversionOperatorsGeneration.None`, so union conversion cannot open a payload ingress beside their coded constructors and the `Fault` base.
- Receipt: `ToolResult` carries the structured content blocks and the `isError` flag the SDK emits as `CallToolResult`, plus the `CommandReceipt` correlation id so the agent result correlates with the host evidence stream.
- Packages: ModelContextProtocol.Core, Microsoft.Extensions.AI.Abstractions, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Law: every dispatch over an OWNED union runs the generated total `Switch` — `CommandTxn` at `Project` and `ProgressFrame` at `Stream` — so a new case breaks each arm at compile time. The retired catch-alls each answered a fabricated empty value for a case the fold could not produce.
- Growth: one fault case is one `McpFault` row the SDK maps to a JSON-RPC code; a new content-block kind is one column on `ToolResult`; a new mid-call ask is one `InputRequest` key beside `CostAsk` with its decode row on `McpRound.Of`; zero new surface.
- Boundary: the tool dispatch is the only MCP execution owner — it never executes an op itself, it routes through the command algebra, so the transaction, grant, and cost semantics are the command algebra's and the MCP layer is the protocol projection over the SDK; a `CommandReceipt` crosses the MCP SERVER boundary only as JSON on `CallToolResult.StructuredContent` under the tool's declared `OutputSchema`, reconstructed by the remote caller — the SDK's tool adapter converts any non-`AIContent` return by serializing it, so the CLR instance is gone at the protocol edge by construction and a receipt is a live value only on the in-process side of the seam, which is exactly why `Call` yields it and `Project` alone shapes what crosses; the dry-run preview is backed by the broker's simulate fold and projected through the input-required ask, so the preview and the charge share one pricing source and the ask never becomes a second admission decision; `McpServer.IsMrtrSupported` is the ONE availability read and it gates the suspension alone — a supported client suspends, an unsupported one falls to the broker refusal, and no arm blocks on a round trip the stateless transport cannot open; the suspension crosses the IO rail as the SDK's own `InputRequiredException` identity, unwrapped and rethrown raw at the one transport edge, because the SDK's input_required framing keys on the exception type and a wrapped error would serialize as a tool fault; the retry decodes off the SDK params exactly once, at the call-tool request filter — an interior member never touches an SDK params shape, and an unparseable `RequestState` runs as a first round rather than faulting the retry; cancellation maps the SDK's `notifications/cancelled` onto the `CancelScope` the call derived, so an agent cancel propagates through the same cancel spine a drain or deadline propagates through, never a parallel cancellation flag; the `isError` result and the JSON-RPC error are distinct — a tool that runs and reports a domain failure returns `isError: true` content while a tool that cannot run returns a JSON-RPC `McpFault`, so the agent distinguishes a failed execution from a refused dispatch; the `McpRuntime.Wire` `JsonSerializerOptions` is the single converter-owner handle threaded from the composition edge into the runtime record — the `PROTOCOL_EDGE`/`CONVERTER_OWNER` law admits it only as that one handle the dispatch reads when it projects a `CommandReceipt` onto a structured result, never a codec surface the interior transforms re-derive or a second serializer beside the generated Thinktecture and NodaTime converters.

```csharp signature
// --- [ERRORS] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record McpFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Mcp;
    private McpFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record UnknownTool : McpFault { public UnknownTool(string detail) : base(detail) { } }
    [FaultCase(1)]
    public sealed partial record InvalidArguments : McpFault { public InvalidArguments(string detail) : base(detail) { } }
    [FaultCase(2)]
    public sealed partial record CostRejected : McpFault { public CostRejected(string detail) : base(detail) { } }
    [FaultCase(3)]
    public sealed partial record Cancelled : McpFault { public Cancelled(string detail) : base(detail) { } }
    // Five rows the catch-all erased, each a different remote answer: an execution fault is retriable, an
    // uncompensated one leaves state UNCERTAIN and must never be retried blind, an incomplete macro is a
    // client-side gap, a veto is a policy refusal, and a shed call asks to be asked again later.
    [FaultCase(4)]
    public sealed partial record ExecutionFailed : McpFault, ICausedFault {
        public ExecutionFailed(string detail, Error cause) : base(detail) => Cause = cause;
        public Error Cause { get; }
    }
    [FaultCase(5)]
    public sealed partial record Uncompensated : McpFault { public Uncompensated(string detail) : base(detail) { } }
    [FaultCase(6)]
    public sealed partial record Incomplete : McpFault { public Incomplete(string detail) : base(detail) { } }
    [FaultCase(7)]
    public sealed partial record Vetoed : McpFault { public Vetoed(string detail) : base(detail) { } }
    [FaultCase(8)]
    public sealed partial record Shed : McpFault { public Shed(string detail) : base(detail) { } }
}

// --- [MODELS] ----------------------------------------------------------------------------
public sealed record CostPreview(
    string Tool,
    MeterVector Estimated,
    bool Covered,
    Option<string> ShortfallUnit);

// This ask crosses as a form-mode `InputRequest` whose `RequestedSchema` spells exactly these two fields and
// the retry's `InputResponse` decodes back through the wire options' `JsonTypeInfo` — one type, both ways.
public sealed record CostApproval(bool Approved, string Approver);

public sealed record ToolResult(
    string Tool,
    Seq<JsonNode> Content,
    bool IsError,
    CorrelationId Correlation);

// --- [SERVICES] --------------------------------------------------------------------------
public sealed record McpRuntime(
    CapabilityRegistry Registry,
    CommandRuntime Command,
    GrantBroker Broker,
    Func<DegradationLevel> Level,
    // `SchemaOf` DERIVES the parameter schema from the descriptor's own `JsonTypeInfo` through
    // `AIJsonUtilities.CreateFunctionJsonSchema`, the generated key projection landing through a
    // `TransformSchemaNode` callback — never a hand-authored roster the declared contract can outgrow.
    Func<DiscoveryResult, JsonNode> SchemaOf,
    // The primitive mints for the non-tool halves of the declared method axis, each the AIFunction-shaped
    // Create peer of McpServerTool.Create — so all three primitives adopt the ONE brokered CommandAIFunction
    // and a prompt or resource can never route around the broker the tool leg goes through.
    Func<McpPrompt, CommandAIFunction, McpServerPrompt> MintPrompt,
    Func<McpResource, CommandAIFunction, McpServerResource> MintResource,
    // The ingress-tenancy adoption the incoming message filter runs before any tool executes: it reads the
    // carrier's own claims principal off MessageContext.User and resolves it under the MCP carrier's
    // TenantAdoption trust row, so an adopting leg yields the wire tenant and a refusing leg the root row.
    Func<ClaimsPrincipal?, TenantContext> Adopt,
    // The consent-elevation seat: the call-tool filter hands an approved retry answer here and the
    // composition lands the transient Consent.Elevated its own ConsentOf resolver reads for the round's
    // correlation — scoped, so the elevation dies with the round and never becomes a standing grant.
    Func<McpRound, CostApproval, IDisposable> Elevate,
    ClockPolicy Clocks,
    ReceiptSinkPort Sink,
    JsonSerializerOptions Wire);

// `State` is the host-minted opaque `RequestState` a retry echoes and it PACKS the first round's correlation,
// so the ask, the transient consent, the charge, and the receipt each key ONE identity across every round.
public sealed record McpRound(string State, CorrelationId Correlation, Option<CostApproval> Approval) {
    public static string Mint(CorrelationId correlation, JsonSerializerOptions wire) =>
        Base64Url.EncodeToString(JsonSerializer.SerializeToUtf8Bytes(correlation, wire));

    // A retry round decodes off the SDK params exactly once, at the call-tool request filter: an unparseable
    // state is a foreign or replayed token, so the call runs as a FIRST round rather than faulting the retry,
    // and the CostAsk answer decodes here so no interior member touches an SDK params shape.
    public static Option<McpRound> Of(string? state, IDictionary<string, InputResponse>? answers, JsonSerializerOptions wire) =>
        Optional(state)
            .Bind(token => Base64Url.IsValid(token)
                ? Op.Of().Catch(() => Fin.Succ(JsonSerializer.Deserialize<CorrelationId>(Base64Url.DecodeFromChars(token), wire))).ToOption()
                : None)
            .Map(correlation => new McpRound(
                state!,
                correlation,
                Optional(answers).Bind(set => set.TryGetValue(ServerInitiated.CostAsk, out InputResponse? answer)
                    ? Op.Of().Catch(() => Fin.Succ(answer.Deserialize(
                            (JsonTypeInfo<CostApproval>)wire.GetTypeInfo(typeof(CostApproval)))))
                        .ToOption().Bind(Optional)
                    : None)));
}

// Under the elected stateless revision the server opens NO round trip of its own — a mid-call ask suspends the
// tool call as an input_required result and the CLIENT retries the same `tools/call` with the answers plus the
// echoed `RequestState`.
// --- [BOUNDARIES] ------------------------------------------------------------------------
public static class ServerInitiated {
    public const string CostAsk = "cost-approval";

    // Both ambients are kernel `AmbientSlot` values reached from deep inside a tool body that threads no
    // server handle, and each exists only inside the filter scope that opened it: the message filter for the
    // live server, the call-tool filter for the retry round. Both are declared ONE-LEVEL because each filter
    // wraps the whole downstream flow and nothing nests inside it, so a second `Enter` is a defect at the seam
    // that nested rather than a value a reader must disambiguate. Unbound is `None` — a first round carries no
    // round scope, and neither absence faults. NAMED LOSS: the retired hand scope's `Interlocked` double-
    // dispose guard, unreachable under the slot's `using`-only contract.
    public static readonly AmbientSlot<McpServer> Live = AmbientSlot<McpServer>.One("mcp-session");
    public static readonly AmbientSlot<McpRound> Retry = AmbientSlot<McpRound>.One("mcp-round");

    public static Option<McpServer> Current => Live.Current;
    public static Option<McpRound> Round => Retry.Current;

    // A FIRST round whose client speaks MRTR SUSPENDS — the tool fails with the SDK's own
    // `InputRequiredException` carrying the form-mode ask and the minted `RequestState`, and the transport edge
    // rethrows that identity raw. A client without the rail refuses on the typed rail instead of blocking.
    public static IO<Fin<CostApproval>> Confirm(McpRuntime runtime, CostPreview preview, CommandArguments arguments) =>
        Current.Filter(static server => server.IsMrtrSupported).Match(
            Some: _ => IO.fail<Fin<CostApproval>>(Capture(new InputRequiredException(
                new Dictionary<string, InputRequest> {
                    [CostAsk] = InputRequest.ForElicitation(new ElicitRequestParams {
                        Message = $"{preview.Tool} exceeds the standing grant on {preview.ShortfallUnit.IfNone("cost")}",
                        RequestedSchema = new ElicitRequestParams.RequestSchema {
                            Properties = new Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition> {
                                [nameof(CostApproval.Approved)] = new ElicitRequestParams.BooleanSchema(),
                                [nameof(CostApproval.Approver)] = new ElicitRequestParams.StringSchema(),
                            },
                            Required = [nameof(CostApproval.Approved), nameof(CostApproval.Approver)],
                        },
                    }),
                },
                McpRound.Mint(arguments.Correlation, runtime.Wire)))),
            None: () => IO.pure(Fin.Fail<CostApproval>(new McpFault.CostRejected(preview.Tool))));

    static Error Capture(Exception raised) => Error.New(raised.Message, raised);
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class McpDispatch {
    public static IO<CostPreview> Preview(McpRuntime runtime, string tool, CommandArguments arguments) =>
        runtime.Registry.Resolve(tool).Match(
            Some: descriptor => IO.pure(runtime.Broker.Admit(descriptor, arguments, DrawMode.Priced).Match(
                Succ: cost => new CostPreview(tool, cost, Covered: true, None),
                Fail: fault => new CostPreview(tool, descriptor.Cost.Estimate(arguments), Covered: false, Shortfall(fault)))),
            None: () => IO.pure(new CostPreview(tool, MeterVector.Zero, Covered: false, Some(nameof(McpFault.UnknownTool)))));

    // The unknown-tool arm is the ALGEBRA's, so a second resolve-and-refuse here would fork the not-found path
    // and mint a result no evidence stream sees. The pre-flight ask runs once per LOGICAL call: a retry round
    // skips it whole, because the answer rode in with the round and the request filter already seated consent.
    public static IO<CommandReceipt> Call(McpRuntime runtime, string tool, CommandArguments arguments) =>
        arguments.Round.IsSome
            ? CommandAlgebra.Run(runtime.Command, tool, arguments)
            : from preview in Preview(runtime, tool, arguments)
              from _asked in preview.Covered ? IO.pure(unit) : ServerInitiated.Confirm(runtime, preview, arguments).Map(static _ => unit)
              from receipt in CommandAlgebra.Run(runtime.Command, tool, arguments)
              select receipt;

    // THE one `CommandReceipt`-to-`ToolResult` fold every front door shares, and the transport EDGE where a
    // receipt stops being a CLR value: `Agent/runtime` `CommandDispatch.Project` and the federated projection
    // both delegate HERE, never a switch copy.
    public static ToolResult Project(string tool, CommandReceipt receipt) =>
        receipt.Txn.Switch(
            committed: c => new ToolResult(tool,
                [JsonSerializer.SerializeToNode(c.Dispatch, SuiteContracts.Host)!], IsError: false, receipt.Correlation),
            rolledBack: r => Failed(tool, r.Reason, receipt.Correlation),
            compensated: c => new ToolResult(tool,
                [JsonSerializer.SerializeToNode(c.Compensation, SuiteContracts.Host)!, Fault(c.Reason)],
                IsError: true, receipt.Correlation),
            refused: f => Failed(tool, f.Fault, receipt.Correlation));

    public static ToolResult Failed(string tool, Error error, CorrelationId correlation) =>
        Failed(tool, AppHostFaultMap.Wire(error), correlation);

    public static ToolResult Failed(string tool, FaultObservationWire fault, CorrelationId correlation) =>
        new(tool, [Fault(fault)], IsError: true, correlation);

    static JsonNode Fault(Error error) => Fault(AppHostFaultMap.Wire(error));
    static JsonNode Fault(FaultObservationWire fault) =>
        JsonSerializer.SerializeToNode(fault, SuiteContracts.Host)!;

    static Option<string> Shortfall(GrantFault fault) => fault.Switch(
        outOfScope: static _ => Option<string>.None,
        ceilingExceeded: static f => Some(f.Unit),
        windowClosed: static _ => Option<string>.None,
        consentRequired: static _ => Option<string>.None,
        fenced: static _ => Option<string>.None,
        contended: static _ => Option<string>.None);
}
```

## [04]-[STREAM_PROGRESS]

- Owner: `ProgressFrame` `[Union]` the progress-notification vocabulary; `AgentSession` the per-agent credential-and-cancel cell; `StreamProgress` the static progress-fan surface over the SDK's progress-notification transport and its request-polling leg.
- Cases: `ProgressFrame` = Started | Progress | Partial | Completed | Failed | Cancelled — the frame sequence a long tool call emits as SDK progress notifications.
- Entry: `Stream(McpRuntime runtime, AgentSession session, string tool, CommandArguments arguments, IProgress<ProgressNotificationValue> reporter)` returns `IO<ToolResult>` — the call runs under a scope derived from the session spine while each frame projects to a `ProgressNotificationValue` the SDK fans over the SSE transport, terminating with a completed, failed, or cancelled frame and yielding the projected result; `Report(IProgress<ProgressNotificationValue> reporter, ProgressFrame frame)` returns `IO<Unit>` — THE producer seat every frame crosses, the terminal ones this fold mints and the intermediate ones an executing fold publishes.
- Auto: progress fan rides the SDK's `IProgress<ProgressNotificationValue>` reporter over the request's own response stream, never a new transport; the SDK auto-binds the reporter from the request's `_meta.progressToken` (the host never news up the internal `TokenProgress` implementation), so a tool method declaring an `IProgress<ProgressNotificationValue>` parameter receives the live reporter; each interior `ProgressFrame` projects to one `ProgressNotificationValue` (`Progress` fraction, optional `Total`, optional `Message`) at the single `ToNotification` seam, so the frame union is the host vocabulary and the notification value is the wire shape; a dropped connection recovers through the protocol's own two legs — a long call opts its request into polling through `RequestContext<T>.EnablePollingAsync(interval, ct)` so a disconnected agent re-reads progress off the poll leg with no host-held stream, and the terminal answer recovers by the client RETRYING the same call under the echoed round, which `[03]`'s idempotence law makes a read of the one commit rather than a second execution; backpressure rides the keyed token-bucket admission so a slow agent consumer applies pressure to the producer through the existing rate-limiter; cancellation runs the dispatch under a `CancelScope` derived from the session spine, so the SDK's `notifications/cancelled` cancels that call alone and the fold emits a `Cancelled` frame.
- Receipt: each completed stream mints one `CommandReceipt` through the command algebra; the per-frame fan is the progress notification itself, not a separate receipt; the session roster transition logs through one `SpineLog` event.
- Packages: ModelContextProtocol.Core, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: one frame case is one `ProgressFrame` row breaking every consumer arm, reported through the SAME `Report` seat that takes the frame whole; a new session-policy column is one field on `AgentSession`; zero new surface.
- Boundary: a progress frame is EPHEMERAL narration and the receipt is the durable truth — the terminal `CommandReceipt` commits in the `Wire/outbox#OUTBOX_FABRIC` transactional store as every receipt does, while no frame store, replay cursor, or resume token exists on either side of the wire, because the SDK's whole SSE event-store family is the obsolete back-compat rail the served-revision election deleted and a reconnecting agent recovers through the poll leg and the idempotent retry instead; `ProgressFrame` therefore carries no sequence column of its own, and the host conflates nothing with the per-request `progressToken` (typed `object`, string-or-long) the SDK correlates live notifications by; intermediate `Progress` and `Partial` frames belong to the EXECUTING producer — the host owns the frame vocabulary and the one fan, while the fraction and the chunk come from the fold that measured them, so a host-side fraction interpolated between the terminal frames is the fabricated measurement this split deletes and a fold reporting its own stage fractions crosses as raw `double` onto `ProgressFrame.Progress` at the `Report` seat; whether a producer may report at all is its own contract's admission column, read where the plugin emits and never re-derived at this dispatch; the streaming substrate is the SDK's progress-notification transport and its request-polling leg — a bespoke WebSocket, a gRPC server-stream, and a host-held frame buffer are the deleted forms; the session roster keys by the agent's `PeerCredential` from the accept seam, mirroring the `PeerRoster` lease-epoch law, so a vanished agent's session sweeps on the same crash-staleness window; cancellation and deadline never race silently — a deadline-expired stream emits a `Failed` frame carrying the `DeadlineReceipt` while a cancelled stream emits `Cancelled`, so the agent distinguishes timeout from cancel.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProgressFrame {
    private ProgressFrame() { }
    public sealed record Started(string Tool) : ProgressFrame;
    public sealed record Progress(double Fraction, string Stage) : ProgressFrame;
    public sealed record Partial(JsonNode Chunk) : ProgressFrame;
    public sealed record Completed(ToolResult Result) : ProgressFrame;
    public sealed record Failed(McpFault Fault) : ProgressFrame;
    public sealed record Cancelled(string Reason) : ProgressFrame;
}

// A credential, a cancel spine, and a lease — the whole cell. A frame buffer or resume cursor beside it would
// answer a reattach the elected revision routes to the poll leg and the idempotency key instead.
// --- [SERVICES] --------------------------------------------------------------------------
public sealed record AgentSession(PeerCredential Agent, CancelScope Spine, Instant LeaseUntil) {
    public static AgentSession Open(PeerCredential agent, CancelScope parent, ClockPolicy clocks) =>
        new(agent, parent.Derive($"agent-{agent.Pid}", clocks.Time), clocks.Now + LeasePolicy.Maintenance.CrashStaleness);

    public string Key => Agent.Pid.ToString(CultureInfo.InvariantCulture);
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class StreamProgress {
    public static IO<ToolResult> Stream(McpRuntime runtime, AgentSession session, string tool, CommandArguments arguments, IProgress<ProgressNotificationValue> reporter) =>
        from _start in Report(reporter, new ProgressFrame.Started(tool))
        from frame in Dispatched(runtime, session, tool, arguments)
        from _end in Report(reporter, frame)
        from result in frame.Switch(
            started: Nonterminal<ToolResult>,
            progress: Nonterminal<ToolResult>,
            partial: Nonterminal<ToolResult>,
            completed: static c => IO.pure(c.Result),
            failed: f => IO.pure(McpDispatch.Failed(tool, f.Fault, arguments.Correlation)),
            cancelled: x => IO.pure(new ToolResult(tool, [JsonValue.Create(x.Reason)!], IsError: true, arguments.Correlation)))
        select result;

    // `Dispatched` answers a TERMINAL frame by construction, so an intermediate one arriving here is a
    // producer publishing through the wrong seat. It refuses rather than framing an empty success the client
    // would read as a completed call — which is exactly what the retired catch-all did for all three.
    static IO<T> Nonterminal<T>(ProgressFrame frame) =>
        IO.fail<T>(new KernelFault.InvalidResult(Op.Of(), Some($"<non-terminal-frame:{frame.GetType().Name}>")));

    public static IO<Unit> Report(IProgress<ProgressNotificationValue> reporter, ProgressFrame frame) =>
        IO.lift(() => { reporter.Report(ToNotification(frame)); return unit; });

    // The call runs under a scope DERIVED from the session spine, so `notifications/cancelled` cancels this
    // call alone — a bare ambient token cancels the whole agent.
    static IO<ProgressFrame> Dispatched(McpRuntime runtime, AgentSession session, string tool, CommandArguments arguments) =>
        IO.liftAsync(async () => {
            using var call = session.Spine.Derive(tool, runtime.Clocks.Time);
            return await McpDispatch.Call(runtime, tool, arguments).RunAsync(EnvIO.New(token: call.Token));
        })
        .Map(receipt => new ProgressFrame.Completed(McpDispatch.Project(tool, receipt)) as ProgressFrame)
        | @catch<IO, ProgressFrame>(static error => error.Is(Errors.Cancelled), static _ => IO.pure(new ProgressFrame.Cancelled("agent-cancel") as ProgressFrame))
        | @catch<IO, ProgressFrame>(static _ => true, static error => IO.pure(
            new ProgressFrame.Failed(new McpFault.ExecutionFailed(error.Message, error)) as ProgressFrame));

    static ProgressNotificationValue ToNotification(ProgressFrame frame) => frame.Switch(
        started: static f => new ProgressNotificationValue { Progress = 0f, Message = $"started:{f.Tool}" },
        progress: static f => new ProgressNotificationValue { Progress = (float)f.Fraction, Total = 1f, Message = f.Stage },
        partial: static f => new ProgressNotificationValue { Progress = 0f, Message = "partial" },
        completed: static f => new ProgressNotificationValue { Progress = 1f, Total = 1f, Message = "completed" },
        failed: static f => new ProgressNotificationValue { Progress = 1f, Total = 1f, Message = $"failed:{f.Fault.Message}" },
        cancelled: static f => new ProgressNotificationValue { Progress = 1f, Total = 1f, Message = $"cancelled:{f.Reason}" });
}
```

```mermaid
stateDiagram-v2
    accTitle: MCP tool-call progress lifecycle
    accDescr: A started tool call advancing through repeated intermediate progress and chunked partials onto exactly one terminal outcome — completed, faulted, or cancelled through the protocol cancellation notification.
    [*] --> Started
    Started --> Progress : intermediate
    Progress --> Progress : intermediate
    Progress --> Partial : chunk
    Partial --> Progress
    Progress --> Completed : done
    Progress --> Failed : fault
    Progress --> Cancelled : notifications/cancelled
    Completed --> [*]
    Failed --> [*]
    Cancelled --> [*]
```

## [05]-[PROTOCOL_FACES]

- Owner: `McpToolWire`, `ProgressNotificationWire`, and `ToolResultWire` — the three shapes the SDK actually puts on the wire, whose decoder is any conforming MCP client rather than a Rasm branch. `ToolResultWire` is the `ToolResult` record (`Tool`/`Content`/`IsError`/`Correlation`) projected as the `TPayload` of the existing `ReceiptEnvelopeWire`, single-minted here so the agent transport decodes it rather than re-authoring the payload shape.
- Entry: the tool catalog crosses as the standard MCP `tools/list` JSON, the progress values as `notifications/progress`, and the structured result as `CallToolResult.StructuredContent` under the declared output schema.
- Packages: BCL inbox
- Growth: one wire-member row per new tool annotation or notification field; zero new surface.
- Boundary: the tool input schema crosses as the standard JSON Schema the descriptor resolves, so an MCP client's schema validation reads the same schema the host binder reads; effect annotations cross as the MCP `readOnlyHint`/`destructiveHint` booleans the projection sets from `EffectClass`; WITHDRAWN this pass: `ProgressFrameWire` (the interior `ProgressFrame` union never crosses — `ToNotification` projects it INTO `notifications/progress`, and this page's own boundary already stated that no peer reconstructs it) and `CostPreviewWire` (no MCP method carries a preview and no branch decodes one — the preview feeds `Confirm`'s elicitation schema in-process and stops there). Neither had a producer on the wire or a decoder anywhere; no resume cursor crosses at all, because a disconnected agent re-reads progress off the request-polling leg and recovers the terminal result by retrying the same call under the echoed `requestState`, so a host token on the wire would be a cursor no reader consults; the structured tool result is `ToolResultWire` (the `ToolResult` `Tool`/`Content`/`IsError`/`Correlation` projection) ridden as the `TPayload` of the `Runtime/ports#TS_PROJECTION` `ReceiptEnvelopeWire`, single-minted here so the agent transport decodes the payload shape rather than re-authoring it — a branch-side `ToolResultWire` mint is the named drift defect this projection deletes.

```ts signature
interface McpToolWire {
  readonly name: string;
  readonly title: string;
  readonly inputSchema: unknown;
  readonly annotations: {
    readonly readOnlyHint: boolean;
    readonly destructiveHint: boolean;
    readonly idempotentHint: boolean;
    readonly approvalRequired: boolean;
  };
  readonly estimatedCost: Readonly<Record<string, number>>;
}

interface ProgressNotificationWire {
  readonly progress: number;
  readonly total?: number;
  readonly message?: string;
}

interface ToolResultWire {
  readonly tool: string;
  readonly content: ReadonlyArray<unknown>;
  readonly isError: boolean;
  readonly correlation: string;
}
```

## [06]-[APP_ROOT]

- Owner: the service-host root composes the SDK server; this page's interior carries no `AddMcpServer` or transport call, so the MCP HTTP transport is the app-root pin `ARCHITECTURE.md` names and never an interior dependency of this package.
- Entry: `ToolProjection.Project` gates the catalog to the live degradation level, `ToolProjection.Adopt` mints the brokered function/server pairs and the prompt and resource primitives once, then MCP registration maps `ServerTools`/`Prompts`/`Resources` while reasoning maps `Function`.
- Auto: the builder fold is `services.AddMcpServer()` for the `IMcpServerBuilder`, one transport extension, and the enumerable `WithTools`/`WithPrompts`/`WithResources` registrations over the adopted primitives — the enumerable overloads, never the generic `WithTools<T>()` or `WithToolsFromAssembly()` and never the reflection `Create(MethodInfo, …)`/`Create(Delegate, …)` mints, because `Adopt` constructs its primitives programmatically and leaves no `[McpServerToolType]`/`[McpServerTool]` attribute surface for a discovery scan to find; the stdio builder extension `WithStdioServerTransport()` lives in the host `ModelContextProtocol` package while the `StdioServerTransport` type it mounts is `.Core`'s, and stdio hosting routes logging to stderr because stdout is the JSON-RPC channel; HTTP hosting serves the transport's own stateless default — the folder's revision-election ruling — so no `Stateless` pin and no `EventStreamStore` assignment exist to drift: a `2026-07-28` agent rides per-request metadata, a down-level client still lands the initialize handshake the SDK answers at the last session revision, and every mid-call ask on either rides the MRTR guard, whose unsupported arm is the broker refusal rather than a blocked round trip; the call-tool request filter is the round seat — it decodes `RequestState` and the `CostAsk` answer once at the edge through `McpRound.Of`, stamps the round scope, and hands an approved answer to `McpRuntime.Elevate` for exactly the round's lifetime.
- Boundary: `WithMessageFilters(Action<IMcpMessageFilterBuilder>)` opens the `AddIncomingFilter`/`AddOutgoingFilter` extend and `WithRequestFilters` its per-request `IMcpRequestFilterBuilder` sibling; `McpMessageFilter` is handler-wrapping rather than call-shaped — `delegate McpMessageHandler McpMessageFilter(McpMessageHandler next)` over `delegate Task McpMessageHandler(MessageContext context, CancellationToken cancellationToken)` — so a filter returns a handler closing over `next` and registration order is outermost-first; a `(context, next, ct)` call-shaped lambda is the rejected form the builder cannot bind. Ingress tenancy reads `MessageContext.User`, resolves it through `McpRuntime.Adopt`, and scopes the resolved `TenantContext` across every ambient store for the duration of the awaited `next`.

```csharp signature
McpCatalog catalog = ToolProjection.Project(mcpRuntime.Registry, mcpRuntime.Level(), mcpRuntime.SchemaOf, receiptSchema);
McpAdoption adopted = ToolProjection.Adopt(mcpRuntime, catalog);

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Services.AddSingleton(mcpRuntime);
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools(adopted.ServerTools)
    .WithPrompts(adopted.Prompts)
    .WithResources(adopted.Resources);
await builder.Build().RunAsync();

var web = WebApplication.CreateBuilder(args);
web.Services.AddSingleton(mcpRuntime);
web.Services.AddMcpServer()
    // The transport serves its stateless default per the folder's revision-election ruling: no `Stateless` pin,
    // no `EventStreamStore`, no session state — the client's retry with the echoed `RequestState` is the whole
    // resume story.
    .WithHttpTransport()
    .WithTools(adopted.ServerTools)
    .WithPrompts(adopted.Prompts)
    .WithResources(adopted.Resources)
    // The ONE ingress seam an MCP message crosses before its tool runs, where the MCP carrier ADMITS tenancy
    // per the folder's ingress ruling. Registration wraps the HANDLER, never the call, so the scope encloses
    // the whole downstream flow — and the same filter seats the live server, because `MessageContext` carries
    // both the principal and the server and the MRTR availability guard reads off that handle.
    .WithMessageFilters(filters => filters.AddIncomingFilter(next => async (context, ct) => {
        using var tenancy = Correlation.Stamp(mcpRuntime.Adopt(context.User));
        using IDisposable session = ServerInitiated.Live.Enter(context.Server).ThrowIfFail();
        await next(context, ct).ConfigureAwait(false);
    }))
    // A retry's `RequestState` and `CostAsk` answer decode ONCE at this edge, the round scope stamps for the
    // downstream flow, and an approved answer seats the transient consent for exactly this round's lifetime.
    .WithRequestFilters(filters => filters.AddCallToolFilter(next => async (request, ct) => {
        Option<McpRound> round = McpRound.Of(request.Params?.RequestState, request.Params?.InputResponses, mcpRuntime.Wire);
        using IDisposable? scope = round.Map(row => ServerInitiated.Retry.Enter(row).ThrowIfFail()).ValueUnsafe();
        using IDisposable? consent = round
            .Bind(row => row.Approval.Filter(static answer => answer.Approved).Map(answer => mcpRuntime.Elevate(row, answer)))
            .ValueUnsafe();
        return await next(request, ct).ConfigureAwait(false);
    }));
var app = web.Build();
app.MapMcp();
await app.RunAsync();
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
