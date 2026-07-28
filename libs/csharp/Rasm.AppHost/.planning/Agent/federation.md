# [APPHOST_TOOL_FEDERATION]

Rasm.AppHost inverts `Agent/mcp#METHOD_AXIS` into an MCP-CLIENT federation surface: the official `ModelContextProtocol` SDK owns the peer session — JSON-RPC framing, the `IClientTransport` connection, the initialize handshake, and `McpClientTool : AIFunction` adoption — and this surface folds each attached external server's tools, resources, and prompts inward as brokered `CapabilityDescriptor` rows under a `federated.{server}.{tool}` key, wrapping the same `CommandAIFunction` the server projection mints so one tool-adoption seam serves both directions.

Every federated call therefore routes through `Agent/capability#COMMAND_ALGEBRA` `CommandAlgebra.Run`, the `Agent/capability#GRANT_BROKER` admission, the dry-run cost preview, and the `Runtime/determinism#EVENT_LOG` chain exactly as a native op; `Wire/outbound#HOP_AXIS` `OutboundHop` carries the peer bytes under the existing retry, breaker, and deadline; and the peer-tool JSON Schema becomes the descriptor's content-keyed argument schema for the `Agent/capability#SDK_CODEGEN` identity gate.

`CapabilityDescriptor`/`DescriptorSurface.Describe`, `CapabilityRegistry`, `CommandAlgebra`/`CommandRuntime`/`GrantBroker`, `ComputeIntent`/`AdmittedIntent.Admit`, `ToolResult`, `CommandAIFunction`, `SubscriptionLane`/`ExternalValue`, `OutboundHop`/`OutboundSurface`, `TenantContext`, `ReceiptSinkPort`, and `CancelScope` arrive settled, and no eighth port is minted.

## [01]-[INDEX]

- [02]-[FEDERATION_AXIS]: Transport-kind taxonomy with external-server admission rows, trust scope, and fault bands.
- [03]-[FEDERATION_PROJECTION]: Peer tool-to-descriptor inversion fold; the reused `CommandAIFunction` wrap.
- [04]-[FEDERATED_DISPATCH]: Brokered dispatch over `McpClient.CallToolAsync` riding the command algebra.
- [05]-[RESOURCE_PROMPT_FOLD]: Peer resource, prompt, and template projection with resource-update subscription drain.
- [06]-[TS_PROJECTION]: Federated-descriptor wire shapes additive to the one capability catalog.
- [07]-[APP_ROOT_COMPOSITION]: Catalog admission ahead of the registry freeze, refusal logging, and the census mount.

## [02]-[FEDERATION_AXIS]

- Owner: `TransportKind` `[SmartEnum<string>]` the closed three-row transport taxonomy under the `ComparerAccessors.StringOrdinal` accessor, each row carrying its `IClientTransport` factory delegate; `TrustScope` the per-server permission envelope the federated descriptors inherit; `FederationFault` `[Union]` fault family in the fresh 4800 band; `FederatedServer` `[ValueObject]` the admitted external-server row carrying its transport kind, its constructed `IClientTransport`, and its trust scope; `FederationCatalog` the frozen admitted-server set.
- Cases: 3 transport rows — stdio, http, streamable — the closed `IClientTransport` selection the SDK serves; `FederationFault` = Text | TransportRejected | HandshakeFailed | PeerUnavailable | ToolCallFaulted | UntrustedScope; server identity is open at composition so `FederatedServer` is a `[ValueObject]` admitted dynamically, never a `[SmartEnum]` row.
- Entry: `TransportKind.Transport(string endpoint, StdioClientTransportOptions? stdio)` returns `IClientTransport` — the row's factory delegate constructs the SDK transport from the endpoint and the per-kind options; `FederatedServer.Admit(string server, TransportKind kind, string endpoint, TrustScope trust, StdioClientTransportOptions? stdio)` returns `Validation<FederationFault, FederatedServer>` — the admission rail validates the server id, constructs the transport through the kind's factory, and admits the row, mirroring the `CapabilityDescriptor` admission through `DescriptorSurface.Describe`.
- Auto: the `TransportKind` row owns the `IClientTransport` construction so `Stdio` news a `StdioClientTransport(StdioClientTransportOptions)` over the spawned peer process command, `Http` news a `HttpClientTransport(HttpClientTransportOptions)` at `HttpTransportMode.AutoDetect` (streamable-first, SSE-fallback) over the peer endpoint uri, and `Streamable` news the SAME `HttpClientTransport` pinned to `HttpTransportMode.StreamableHttp` over the resumable HTTP session — the streamable session transport is the SDK's internal `TransportBase` the `HttpClientTransport` selects by mode at connect, never a directly-constructed type, and the three public `IClientTransport` implementors are exactly `StdioClientTransport`/`HttpClientTransport`/`StreamClientTransport`; the kind is the closed vocabulary the admission reads to gate the transport, never a per-server transport reimplementation; the `TrustScope` carries the `PermissionShape` floor every federated descriptor from that server inherits so an untrusted server's tools admit only as `read`-effect descriptors and a trusted server's tools admit at their declared effect class, the trust decision made once at admission and never re-evaluated per call; the admission folds through `Validation<FederationFault, T>` so a malformed server id, an unreachable endpoint, or a scope violation accumulates rather than aborting on the first, and the frozen `FederationCatalog` is the composition-time admitted set the projection reads.
- Receipt: `FederatedServer` is its own value-object evidence carrying the server key, the transport kind key, the endpoint, and the trust-scope hash; the admission transition logs through one `SpineLog` event in the 1000-1099 EVENT stride (`FaultBand.SpineEvents`) — no parallel admission receipt.
- Packages: ModelContextProtocol.Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new transport kind is one `TransportKind` row carrying its `IClientTransport` factory the SDK already serves; a new server is one `FederatedServer.Admit` call, never a parallel client; a new fault is one `FederationFault` case; zero new surface.
- Boundary: the federation axis is the only external-MCP-server admission owner — a per-server client, a server-specific connection manager, and a second tool catalog are the deleted forms, so every external server rides one `FederatedServer` row admitted through one rail; the three `IClientTransport` cases are the SDK's transport selection — a hand-rolled JSON-RPC client transport beside the official SDK is the named drift defect at `ARCHITECTURE.md#[05]-[BOUNDARIES]`, so `TransportKind` reads the closed SDK transport vocabulary and never a bespoke socket; `FederatedServer` is a `[ValueObject]` not a `[SmartEnum]` because server identity is composition-open — the admitted set is dynamic config/discovery data, distinct from the closed `TransportKind` taxonomy that IS a smart enum; `FederationFault` derives its codes through `FaultBand.Federation` — band disjointness is the `Runtime/lifecycle#FAULT_TABLES` registry's type-enforced fact (a duplicate integer fails at type initialization), so NO prose census exists here or anywhere; a consumer touching two fault families references each through its namespace-qualified path per `docs/stacks/csharp/language#FORM_CHOOSER`; the `TrustScope` is the federated descriptor's permission floor so a federated tool can never declare a wider effect class than its server's trust admits, the broker reading the inherited `PermissionShape` exactly as it reads a native descriptor's.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TransportKind {
    public static readonly TransportKind Stdio = new("stdio", Spawn);
    public static readonly TransportKind Http = new("http", Connect);
    public static readonly TransportKind Streamable = new("streamable", Stream);

    [UseDelegateFromConstructor]
    public partial IClientTransport Transport(string endpoint, StdioClientTransportOptions? stdio);

    static IClientTransport Spawn(string endpoint, StdioClientTransportOptions? stdio) =>
        new StdioClientTransport(stdio ?? new StdioClientTransportOptions { Command = endpoint });

    static IClientTransport Connect(string endpoint, StdioClientTransportOptions? stdio) =>
        new HttpClientTransport(new HttpClientTransportOptions { Endpoint = new Uri(endpoint), TransportMode = HttpTransportMode.AutoDetect });

    static IClientTransport Stream(string endpoint, StdioClientTransportOptions? stdio) =>
        new HttpClientTransport(new HttpClientTransportOptions { Endpoint = new Uri(endpoint), TransportMode = HttpTransportMode.StreamableHttp });
}

// --- [MODELS] ---------------------------------------------------------------------------
public sealed record TrustScope(
    EffectClass Ceiling,
    DataClassification Classification,
    FrozenSet<string> ObjectSet) {
    public static readonly TrustScope ReadOnly = new(EffectClass.Read, DataClassification.Operational, FrozenSet<string>.Empty);

    public PermissionShape Floor(EffectClass declared) =>
        new(ObjectSet, declared.Rank <= Ceiling.Rank ? declared : Ceiling, Classification);

    public string Hash => $"{Ceiling.Key}:{Classification.Key}:{string.Join(',', ObjectSet.Order(StringComparer.Ordinal))}";
}

[ValueObject<string>]
[ValidationError<FederationFault>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FederatedServer {
    // The generator's error type is this page's OWN fault family, so a malformed id lands as the fault naming it
    // rather than a trust violation the scope never refused — the admission below then carries the error out with
    // no re-wrap, and one fault vocabulary answers both the id gate and every later peer failure.
    static partial void ValidateFactoryArguments(ref FederationFault? validationError, ref string value) {
        value = value.Trim().ToLowerInvariant();
        validationError = string.IsNullOrWhiteSpace(value) || value.Contains('.', StringComparison.Ordinal)
            ? new FederationFault.Text($"federated server id must be non-empty and dot-free: {value}")
            : null;
    }

    public TransportKind Kind { get; private init; } = TransportKind.Stdio;
    public IClientTransport Transport { get; private init; } = default!;
    public TrustScope Trust { get; private init; } = TrustScope.ReadOnly;
    public string Endpoint { get; private init; } = string.Empty;

    // `Validate` is the error-RETURNING factory the generator publishes — `TryCreate` answers a bool and hands
    // back no error — so one member decides the arm and the typed refusal crosses whole.
    public static Validation<FederationFault, FederatedServer> Admit(string server, TransportKind kind, string endpoint, TrustScope trust, StdioClientTransportOptions? stdio = null) =>
        Validate(server, provider: null, out FederatedServer? admitted) is { } rejected
            ? Prelude.Fail<FederationFault, FederatedServer>(rejected)
            : Prelude.Success<FederationFault, FederatedServer>(admitted! with {
                Kind = kind,
                Transport = kind.Transport(endpoint, stdio),
                Trust = trust,
                Endpoint = endpoint,
            });
}

public sealed record FederationCatalog(FrozenDictionary<string, FederatedServer> Servers) {
    public static readonly FederationCatalog Empty = new(FrozenDictionary<string, FederatedServer>.Empty);

    public static Validation<FederationFault, FederationCatalog> Admit(Seq<(string Server, TransportKind Kind, string Endpoint, TrustScope Trust, StdioClientTransportOptions? Stdio)> rows) =>
        rows.Traverse(row => FederatedServer.Admit(row.Server, row.Kind, row.Endpoint, row.Trust, row.Stdio))
            .Map(servers => new FederationCatalog(servers.ToFrozenDictionary(static s => s.Value, StringComparer.Ordinal)));

    public Option<FederatedServer> Resolve(string server) =>
        Servers.TryGetValue(server, out var row) ? Optional(row) : None;
}

// --- [ERRORS] ---------------------------------------------------------------------------
[Union]
public abstract partial record FederationFault : Expected, IValidationError<FederationFault> {
    private FederationFault(string detail, int code) : base(detail, code, None) { }
    public static FederationFault Create(string message) => new Text(message);
    public sealed record Text : FederationFault { public Text(string detail) : base(detail, FaultBand.Federation.Code(0)) { } }
    public sealed record TransportRejected : FederationFault { public TransportRejected(string detail) : base(detail, FaultBand.Federation.Code(1)) { } }
    public sealed record HandshakeFailed : FederationFault { public HandshakeFailed(string detail) : base(detail, FaultBand.Federation.Code(2)) { } }
    public sealed record PeerUnavailable : FederationFault { public PeerUnavailable(string detail) : base(detail, FaultBand.Federation.Code(3)) { } }
    public sealed record ToolCallFaulted : FederationFault { public ToolCallFaulted(string detail) : base(detail, FaultBand.Federation.Code(4)) { } }
    public sealed record UntrustedScope : FederationFault { public UntrustedScope(string detail) : base(detail, FaultBand.Federation.Code(5)) { } }
}
```

## [03]-[FEDERATION_PROJECTION]

- Owner: `FederationProjection` the static peer-to-descriptor inversion fold; `PeerSession` the held `McpClient` session owner per admitted server; `FederationRuntime` the held composition state the fold reads — the `FederationCatalog`, the session accessor, the live `PeerSchemas` id→schema map, and the `McpRuntime` the reused `CommandAIFunction` closes over.
- Cases: the projection folds each `McpClientTool` the peer's `McpClient.ListToolsAsync` enumerates into one `CapabilityDescriptor` under `federated.{server}.{tool}`, reusing the one `CommandAIFunction : AIFunction` subclass the server projection mints — never a second `AIFunction` subclass and never an `AIFunctionFactory` delegate whose reflected-parameter schema is blind to the peer-tool contract.
- Entry: `Project(FederationRuntime runtime, FederatedServer server)` returns `IO<Seq<CapabilityDescriptor>>` — opens the peer session through `McpClient.CreateAsync(server.Transport, ...)`, lists the peer's tools, and folds each `McpClientTool` into one brokered descriptor whose `Compile` projects to the federated-call `ComputeIntent`, returning the descriptor set the composition admits through `DescriptorSurface.Describe`; `Federate(FederationRuntime runtime, IServiceCollection services)` returns `IO<IServiceCollection>` — folds the tool projection beside the `#RESOURCE_PROMPT_FOLD` resource, prompt, and template projection over every admitted server and lands each peer's whole federated census as one `DescriptorSurface.Describe` snapshot across the four surface keys `Surfaces` derives.
- Auto: the peer session is constructed once per server through `McpClient.CreateAsync(IClientTransport, McpClientOptions?, ILoggerFactory?, CancellationToken)` so the SDK owns the initialize handshake and the session lifecycle, the federation holding one `McpClient` per server in the `PeerSession` cell and never re-initializing per call; each `McpClientTool : AIFunction` the peer exposes carries its `JsonSchema` (the `JsonElement` parameter schema on `AIFunctionDeclaration`) which the projection folds into the live `PeerSchemas` id→schema map under the descriptor id — the composition's `McpRuntime.SchemaOf` resolver consults that map so a federated row's argument schema is the peer's published contract verbatim while a native row falls to the `SuiteContracts.Schema<CommandArguments>` shape — content-keyed for the `Agent/capability#SDK_CODEGEN` identity gate and never a re-derived schema; the descriptor's `EffectClass` derives from the peer-tool `ToolAnnotations` alone — a `DestructiveHint` tool is `External` (the saga/compensation path), a `ReadOnlyHint` tool is `Read`, and an unannotated tool defaults to `External` because the host cannot prove a remote tool side-effect-free — and the SAME derived value feeds both the declared effect and the `TrustScope.Floor` lowering (never a `ReturnJsonSchema` gate under-declaring a schema-less destructive tool as `Read`), so a peer tool's declared effect class never exceeds the server's trust ceiling; the `Idempotency` reads the `IdempotentHint` (`Idempotent` when the peer asserts it, else `NonIdempotent` for a tool the host cannot prove repeat-safe), and the `CostModel` carries a fixed `CostUnit.Calls` beside a `CostUnit.BytesEgress` variable the broker meters; the descriptor's `Compile` projects the `CommandArguments` payload into the federated-call `ComputeIntent` the command algebra dispatches at `FEDERATED_DISPATCH`, so the federated tool enters the registry as a real descriptor whose body is the brokered peer call, never an unbrokered side channel; the `federated.{server}.{tool}` surface key namespaces every federated descriptor under its server so two peers exposing a same-named tool never collide and the catalog stays one flat registry.
- Receipt: each projected descriptor is one `CapabilityDescriptor` the registry folds through `DescriptorSurface.Describe`; the federation transition mints one `DescriptorReceipt` per admitted row exactly as a native descriptor's, never a parallel federation receipt.
- Packages: ModelContextProtocol.Core, Microsoft.Extensions.AI.Abstractions, LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox
- Growth: a new federated server is one `FederationProjection.Project` fold over its session; a new peer tool is one descriptor row the fold absorbs at composition with no per-tool edit; the projection is the one inversion seam, so a second descriptor-from-peer fold is the deleted form; zero new surface.
- Boundary: the projection is the only peer-tool-to-descriptor owner — a per-server descriptor table, a hand-mirrored federated tool list, and a second tool catalog are the deleted forms, so every federated tool is a real registry descriptor adopted as the one `CommandAIFunction`; the inversion is the exact mirror of `Agent/mcp#METHOD_AXIS` — that page folds `DiscoveryResult` outward to an `McpTool` adopted as `AIFunction`, this page folds `McpClientTool` inward to a `CapabilityDescriptor`, so one tool-adoption seam serves both front doors and a federated call and a native call share one `CommandAIFunction` invoker; the peer-tool `JsonSchema` crosses verbatim through the `PeerSchemas` map into the one `McpRuntime.SchemaOf` resolver so the content-addressed codegen identity gate at `Agent/capability#SDK_CODEGEN` sees the peer's published schema, never a host-fabricated one; the `McpClient` session is the SDK's — the federation never re-implements the JSON-RPC client, the initialize handshake, or the tool enumeration, it composes `ListToolsAsync` and holds the session; the `TrustScope` ceiling is the trust boundary so a federated descriptor's effect class is the lesser of the peer's declared class and the server's trust, the broker reading the inherited `PermissionShape` with no federation-specific permission path.

```csharp signature
// --- [SERVICES] -------------------------------------------------------------------------
public sealed record PeerSession(
    FederatedServer Server,
    Atom<Option<McpClient>> Client,
    CancelScope Spine);

public sealed record FederationRuntime(
    FederationCatalog Catalog,
    Func<FederatedServer, IO<McpClient>> SessionOf,
    Atom<HashMap<string, JsonNode>> PeerSchemas,
    McpRuntime Mcp,
    ClockPolicy Clocks,
    ReceiptSinkPort Sink,
    JsonSerializerOptions Wire,
    CancelScope Spine);

// --- [OPERATIONS] -----------------------------------------------------------------------
public static partial class FederationProjection {
    public static IO<Seq<CapabilityDescriptor>> Project(FederationRuntime runtime, FederatedServer server) =>
        from client in runtime.SessionOf(server)
        from tools in IO.liftAsync(() => client.ListToolsAsync(null, runtime.Spine.Token).AsTask())
        select toSeq(tools).Map(tool => Descriptor(runtime, server, tool));

    // One snapshot per peer covers every surface key it spans: tool rows and resource/prompt/template rows
    // land in ONE Describe, so a re-projected peer replaces its whole federated census atomically and a peer
    // dropping an entire class retires that surface instead of stranding stale rows beside live siblings.
    // Two separate folds sweep the tool surface twice and leave the other three unswept.
    public static IO<IServiceCollection> Federate(FederationRuntime runtime, IServiceCollection services) =>
        runtime.Catalog.Servers.Values.AsIterable().ToSeq()
            .FoldM(services, (current, server) =>
                from tools in Project(runtime, server)
                from bound in ProjectResources(runtime, server)
                select DescriptorSurface.Describe(current, Surfaces(server), [.. tools + bound]))
            .As();

    // Federated surface grammar spells once: keys the four projection arms mint and keys a re-projection
    // sweeps derive together, so a fifth arm cannot leave a surface unswept.
    static Seq<string> Surfaces(FederatedServer server) =>
        Seq("", ".resource", ".prompt", ".template").Map(suffix => $"federated.{server.Value}{suffix}");

    static CapabilityDescriptor Descriptor(FederationRuntime runtime, FederatedServer server, McpClientTool tool) {
        var declared = EffectOf(tool.ProtocolTool.Annotations);
        // Each peer publishes its own JsonSchema into the live id->schema map the composition's
        // McpRuntime.SchemaOf resolver consults by descriptor id, so the SDK_CODEGEN identity gate
        // reads that peer contract verbatim while a native row falls to the SuiteContracts.Schema shape.
        ignore(runtime.PeerSchemas.Swap(current =>
            current.AddOrUpdate($"federated.{server.Value}.{tool.Name}", JsonNode.Parse(tool.JsonSchema.GetRawText())!)));
        return CapabilityDescriptor.Of(
            surface: $"federated.{server.Value}",
            op: tool.Name,
            effect: declared,
            idempotency: tool.ProtocolTool.Annotations?.IdempotentHint == true ? Idempotency.Idempotent : Idempotency.NonIdempotent,
            cost: new CostModel(
                Fixed: new CostVector(HashMap((CostUnit.Calls, 1L))),
                Variable: static args => new CostVector(HashMap((CostUnit.BytesEgress, args.Payload.GetRawText().Length)))),
            permission: server.Trust.Floor(declared),
            compile: _ => FederatedDispatch.Compile(server, tool.Name));
    }

    static EffectClass EffectOf(ToolAnnotations? annotations) =>
        annotations?.DestructiveHint == true ? EffectClass.External
        : annotations?.ReadOnlyHint == true ? EffectClass.Read
        : EffectClass.External;

    public static IO<McpClient> Open(FederationRuntime runtime, FederatedServer server) =>
        IO.liftAsync(() => McpClient.CreateAsync(
            server.Transport,
            new McpClientOptions { ClientInfo = new Implementation { Name = "rasm-apphost", Version = "1.0.0" } },
            loggerFactory: null,
            cancellationToken: runtime.Spine.Token).AsTask());
}
```

## [04]-[FEDERATED_DISPATCH]

- Owner: `FederatedDispatch` the static brokered-call surface routing a federated descriptor's invocation through the command algebra and onto the peer `McpClient.CallToolAsync`; `FederatedCall` the call-intent record the descriptor's `Compile` mints; `FederatedReceipt` the per-call evidence record projecting the peer `CallToolResult` onto the reused `ToolResult`.
- Cases: a federated descriptor's `Compile` projects to one `FederatedCall` carrying the server, the tool name, and the payload; the dispatch resolves the peer session, sends `CallToolAsync` over the server's `OutboundHop`, and projects the peer result onto the reused `Agent/mcp#TOOL_DISPATCH` `ToolResult` — never a branch-side `ToolResultWire` mint.
- Entry: `Dispatch(MediationRuntime mediation, GrantScope scope, FederationRuntime runtime, string descriptorId, FederatedCall call, CommandArguments arguments)` returns `IO<(BrokeredCall, ToolResult)>` — the brokered federated front door riding the ONE `Sandbox/isolation#GRANT_HANDLE` `GrantHandleSurface.Mediate` fold (policy gate, scope cover, broker charge) with the peer call as the dispatch closure; `Compile(FederatedServer server, string tool)` returns `Fin<ComputeIntent>` — the typed never-compiles sentinel, because a federated call is not a Compute intent (the landed `ComputeIntent` union carries no federated case, and the strata law forbids AppHost adding one); `Call(FederationRuntime runtime, FederatedCall call)` returns `IO<ToolResult>` — resolves the peer session, invokes `McpClient.CallToolAsync(name, args, progress: null, options, ct)` over the `OutboundHop`, and projects the peer `CallToolResult` content blocks onto the reused `ToolResult`.
- Auto: the federated descriptor routes through the one `Mediate` fold so the grant brokerage at `Agent/capability#GRANT_BROKER` runs before the peer call — a denied federated call never reaches the peer and never charges the broker's `CostUnit.Calls`/`CostUnit.BytesEgress` ceiling, the dry-run cost preview the `Agent/mcp#TOOL_DISPATCH` `McpDispatch.Preview` exposes pricing the federated call against the same standing grant a native call prices against; the dispatch closure sends `McpClient.CallToolAsync(name, args, progress: null, options, ct)` over the server's `OutboundHop` so the peer call inherits the hop's retry, breaker, and deadline — a flapping peer breaks on the same circuit breaker an HTTP API breaks on, never a per-server retry loop; the peer's `CallToolResult` content blocks and `IsError` flag project onto the reused `ToolResult` (`Tool`/`Content`/`IsError`/`Correlation`) so the federated result rides the existing structured-result wire the agent transport already decodes; a peer-call fault projects to `FederationFault.ToolCallFaulted` (registry-banded) the mediation evidence carries, so a faulted federated call returns a typed transaction disposition, never a thrown exception; the mediation's `BrokeredCall` evidence and the projected `ToolResult` ride the same `ReceiptSinkPort.Send` fan a native command's evidence rides, so a federated tool call is content-addressed and replayable exactly as a native op.
- Receipt: the federated call mints one `BrokeredCall` through the mediation carrying the caller modality, permitted flag, and charged cost vector; the peer result is the reused `ToolResult`, never a parallel federation receipt; the determinism chain seat is the mediation's `ReceiptSinkPort` fan, so federation owns no direct `EventLog.Append`.
- Packages: ModelContextProtocol.Core, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a federated call is one descriptor `Compile` projection over one `CallToolAsync` on the hop; a new peer-result content kind is one column the reused `ToolResult` already carries; zero new surface.
- Boundary: the federated dispatch is the only federated-call owner — a direct `McpClient.CallToolAsync` outside the mediation, a per-server call helper, and an unbrokered peer call are the deleted forms, so every federated call routes through the ONE `GrantHandleSurface.Mediate` fold exactly as `ARCHITECTURE.md#[05]-[BOUNDARIES]` mandates (the same fold the operator, agent, and plugin callers share — the federation adds a dispatch closure, never a second admission path); the dispatch never invokes Compute — a federated call is externally executed, so the Compute rail is untouched and the never-compiles sentinel makes the impossibility a compile-time fact; the peer call rides the server's `OutboundHop` so the external server's bytes inherit the existing resilience and the federation owns no transport retry; the `ToolResult` is the reused `Agent/mcp#TOOL_DISPATCH` record ridden as the `ReceiptEnvelopeWire` `TPayload` at `TS_PROJECTION` — a branch-side `ToolResultWire` mint is the named drift defect both this page and the server projection delete; the federated `EffectClass.External` forces the command algebra onto the saga path because no rollback restores a peer's side effect, so a federated write declares a compensation descriptor on the runtime or admits as a single-shot non-compensatable op, never a phantom undo of a remote side effect.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------
public sealed record FederatedCall(
    string Server,
    string Tool,
    JsonElement Payload,
    CorrelationId Correlation);

// --- [OPERATIONS] -----------------------------------------------------------------------
public static class FederatedDispatch {
    // Compile is the typed never-compiles sentinel (the model-descriptor precedent): a federated call is
    // NOT a Compute intent — the landed ComputeIntent union carries no Federated case and AppHost cannot add
    // one — so this delegate rejects by construction and brokered execution routes through the one
    // GrantHandleSurface.Mediate fold below, with zero reversed Compute coupling.
    public static Fin<ComputeIntent> Compile(FederatedServer server, string tool) =>
        Fin.Fail<ComputeIntent>(new CommandFault.CompileRejected($"federated.{server.Value}.{tool}:not-a-compute-intent"));

    // Dispatch is the brokered federated front door: policy gate, scope cover, and broker charge ride the
    // ONE Sandbox/isolation#GRANT_HANDLE Mediate fold with the peer call as the dispatch closure, so grant,
    // cost, and evidence stay identical to a native op.
    public static IO<(BrokeredCall Call, ToolResult Result)> Dispatch(MediationRuntime mediation, GrantScope scope, FederationRuntime runtime, string descriptorId, FederatedCall call, CommandArguments arguments) =>
        GrantHandleSurface.Mediate(mediation, CallerModality.Agent, scope, descriptorId, arguments,
            (_, _) => Call(runtime, call));

    public static IO<ToolResult> Call(FederationRuntime runtime, FederatedCall call) =>
        runtime.Catalog.Resolve(call.Server).Match(
            Some: server =>
                from client in runtime.SessionOf(server)
                from result in OutboundSurface.Run(runtime.Mcp.Outbound, HopOf(server), async ct =>
                    await client.CallToolAsync(call.Tool, Arguments(call.Payload), progress: null, options: new RequestOptions { Timeout = runtime.Mcp.CallTimeout }, cancellationToken: ct)
                        is { } peer
                        ? new HopOutcome.Delivered<CallToolResult>(peer)
                        : new HopOutcome.Faulted(new FederationFault.PeerUnavailable(call.Server)))
                select Project(call, result),
            None: () => IO.pure(new ToolResult(call.Tool, [JsonValue.Create(new FederationFault.PeerUnavailable(call.Server).Message)!], IsError: true, call.Correlation)));

    static IReadOnlyDictionary<string, object?> Arguments(JsonElement payload) =>
        payload.ValueKind is JsonValueKind.Object
            ? payload.EnumerateObject().ToDictionary(static p => p.Name, static p => (object?)p.Value)
            : new Dictionary<string, object?>();

    static OutboundHop HopOf(FederatedServer server) => server.Kind.Switch(
        stdio: static () => new OutboundHop.CompanionSpawn(new ProcessStartInfo(server.Endpoint)),
        http: static () => new OutboundHop.HttpApi(new Uri(server.Endpoint)),
        streamable: static () => new OutboundHop.ServerStream(new Uri(server.Endpoint)));

    static ToolResult Project(FederatedCall call, CallToolResult result) =>
        new(call.Tool,
            toSeq(result.Content).Map(static block => JsonSerializer.SerializeToNode(block)!),
            IsError: result.IsError ?? false,
            call.Correlation);
}
```

## [05]-[RESOURCE_PROMPT_FOLD]

- Owner: the `FederationProjection` fold EXTENSION — `Resources`/`Prompts`/`Templates` projection arms added to the tool fold; `FederationSubscription` the `McpClient.SubscribeToResourceAsync` per-uri handler seam draining a peer resource-update into the one bounded `Wire/livewire#TRANSPORT_BINDING` `SubscriptionLane` as one `ExternalValue` (the reused at-edge carrier, never a federation-local value type).
- Cases: a peer resource projects to a `read`-effect descriptor under `federated.{server}.resource.{uri}`; a peer prompt projects to a `pure`-effect descriptor under `federated.{server}.prompt.{name}`; a peer resource template projects to a `read`-effect descriptor under `federated.{server}.template.{uri}`, mirroring the server projection's effect-class filter where a `read` descriptor projects as both a tool and a resource and a `pure` template-shaped descriptor projects as a prompt; a peer resource-update notification drains into the same bounded lane the OPC-UA and MQTT subscriptions drain into.
- Entry: `ProjectResources(FederationRuntime runtime, FederatedServer server)` returns `IO<Seq<CapabilityDescriptor>>` — lists the peer's resources, prompts, and templates and folds each into a brokered descriptor, the same fold the tool projection runs extended with three more list-and-wrap arms; `Subscribe(FederationRuntime runtime, FederatedServer server, string uri, ChannelWriter<ExternalValue> sink)` returns `IO<IAsyncDisposable>` — binds `McpClient.SubscribeToResourceAsync(uri, handler, options, ct)` registering the per-uri update handler at subscribe and returning the SDK unsubscribe handle, so a peer resource-change drains into the bounded lane as one `ExternalValue`.
- Auto: the resource fold lists the peer through `McpClient.ListResourcesAsync(RequestOptions?, CancellationToken)` and the prompt fold through `McpClient.ListPromptsAsync(RequestOptions?, CancellationToken)`, each `McpClientResource`/`McpClientPrompt` wrapping into one descriptor whose `Compile` projects the read or prompt-get call exactly as the tool fold projects a `CallToolAsync`; the resource-template fold lists through `McpClient.ListResourceTemplatesAsync(RequestOptions?, CT)` (catalogued at `.api/api-mcp.md` row [9]) so a parameterized peer resource projects as a `read`-effect descriptor carrying the `McpClientResourceTemplate.UriTemplate` RFC-6570 template the SDK evaluates; the subscription binds the `McpClient.SubscribeToResourceAsync(string uri, Func<ResourceUpdatedNotificationParams, CancellationToken, ValueTask> handler, RequestOptions?, CT)` per-uri overload — the SDK registers the handler at subscribe and returns one `IAsyncDisposable` unsubscribe handle — and the handler, on a peer resource-update, `TryWrite`s one `ExternalValue` (the `federated.{server}.resource.{uri}` descriptor key the `ResourceUpdatedNotificationParams.Uri` composes as the unit, the good flag, the `ClockPolicy` instant) into the same bounded `Channel<ExternalValue>` under `BoundedChannelFullMode.DropOldest` the live-wire subscriptions drain into — the foreign notification thread never runs the interior, the bounded lane's drop policy is the producer back-pressure, and the reactive consumer resolves that key and drains the changed resource as one brokered inbound command, so a peer resource-change re-projects through the federated descriptor exactly as a native binding's inbound value re-projects; the subscription is a read-shape variant on the federation fold, never a parallel notification handler — `SubscribeToResourceAsync` with its per-uri handler is the one subscription seam, never a mutated `McpClientHandlers` bag (the registry exposes no `ResourceUpdatedHandler` slot).
- Receipt: each projected resource/prompt/template descriptor is one `CapabilityDescriptor` the `#FEDERATION_PROJECTION` `Federate` fold lands in the same per-peer snapshot the tool rows ride; each drained resource-update is one `ExternalValue` carrying its federated resource-descriptor key as the unit, which the `Wire/livewire#TRANSPORT_BINDING` lane consumer resolves, brokers, and mints one `CommandReceipt` from — this page writes the keyed value and mints no receipt of its own; no parallel federation-resource receipt.
- Packages: ModelContextProtocol.Core, LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox
- Growth: a peer resource is one fold arm; a peer prompt is one fold arm; a peer template is one fold arm; the subscription drain reuses the one bounded lane the live-wire subscriptions own; zero new surface.
- Boundary: the resource/prompt fold is the same `FederationProjection` extended with three list-and-wrap arms, never a second projection — a peer resource/prompt enters only as a brokered descriptor through the one registry, a second resource catalog being the named drift defect; the effect-class filter mirrors the server projection so a federated resource is `read`, a federated prompt is `pure`, and the trust-scope ceiling lowers each exactly as the tool fold lowers a tool's effect class; the subscription drains into the ONE bounded `SubscriptionLane` the OPC-UA, MQTT, and OPC-UA-PubSub subscriptions drain into — a parallel notification handler, a federation-specific subscription buffer, and a second bounded channel are the deleted forms, so a peer resource-update and an industrial sensor value ride one inbound contract; the `McpClient.SubscribeToResourceAsync` per-uri handler overload is the SDK's resource-update seam so the federation never re-implements the JSON-RPC notification dispatch, it passes the handler at subscribe and holds the returned `IAsyncDisposable`; the resource-update re-projects through the federated descriptor so the changed resource lands as a brokered, metered, audited inbound command, the same brokerage a live-wire inbound value rides, never a privileged write path.

```csharp signature
// --- [OPERATIONS] -----------------------------------------------------------------------
public static partial class FederationProjection {
    public static IO<Seq<CapabilityDescriptor>> ProjectResources(FederationRuntime runtime, FederatedServer server) =>
        from client in runtime.SessionOf(server)
        from resources in IO.liftAsync(() => client.ListResourcesAsync(null, runtime.Spine.Token).AsTask())
        from prompts in IO.liftAsync(() => client.ListPromptsAsync(null, runtime.Spine.Token).AsTask())
        from templates in IO.liftAsync(() => client.ListResourceTemplatesAsync(null, runtime.Spine.Token).AsTask())
        select toSeq(resources).Map(resource => ResourceDescriptor(server, resource.Uri))
            + toSeq(prompts).Map(prompt => PromptDescriptor(server, prompt.Name))
            + toSeq(templates).Map(template => TemplateDescriptor(server, template.UriTemplate));

    static CapabilityDescriptor ResourceDescriptor(FederatedServer server, string uri) =>
        CapabilityDescriptor.Of(
            surface: $"federated.{server.Value}.resource",
            op: uri,
            effect: EffectClass.Read,
            idempotency: Idempotency.Idempotent,
            cost: new CostModel(new CostVector(HashMap((CostUnit.Calls, 1L))), static _ => CostVector.Zero),
            permission: server.Trust.Floor(EffectClass.Read),
            compile: _ => FederatedDispatch.Compile(server, uri));

    static CapabilityDescriptor PromptDescriptor(FederatedServer server, string name) =>
        CapabilityDescriptor.Of(
            surface: $"federated.{server.Value}.prompt",
            op: name,
            effect: EffectClass.Pure,
            idempotency: Idempotency.Idempotent,
            cost: CostModel.Free,
            permission: server.Trust.Floor(EffectClass.Pure),
            compile: _ => FederatedDispatch.Compile(server, name));

    static CapabilityDescriptor TemplateDescriptor(FederatedServer server, string template) =>
        CapabilityDescriptor.Of(
            surface: $"federated.{server.Value}.template",
            op: template,
            effect: EffectClass.Read,
            idempotency: Idempotency.Idempotent,
            cost: new CostModel(new CostVector(HashMap((CostUnit.Calls, 1L))), static _ => CostVector.Zero),
            permission: server.Trust.Floor(EffectClass.Read),
            compile: _ => FederatedDispatch.Compile(server, template));
}

public static class FederationSubscription {
    // The lane carries VALUES and the drain that re-projects them is the live-wire consumer's, so identity has to
    // ride the value: `Unit` spells the resource descriptor's own registry key rather than the bare peer uri, which
    // is what lets that consumer resolve the brokered descriptor, meter the call, and mint the receipt. An
    // unkeyed uri strands every update as an anonymous reading no consumer can route back to its peer.
    public static IO<IAsyncDisposable> Subscribe(FederationRuntime runtime, FederatedServer server, string uri, ChannelWriter<ExternalValue> sink) =>
        from client in runtime.SessionOf(server)
        from handle in IO.liftAsync(() => client.SubscribeToResourceAsync(
            uri,
            (notification, ct) => {
                ignore(sink.TryWrite(new ExternalValue(
                    Raw: 0d,
                    Unit: $"federated.{server.Value}.resource.{notification.Uri}",
                    Good: true,
                    SourceAt: runtime.Clocks.Now)));
                return ValueTask.CompletedTask;
            },
            options: null,
            cancellationToken: runtime.Spine.Token))
        select handle;
}
```

## [06]-[TS_PROJECTION]

- Owner: `FederatedServerWire`, `FederatedDescriptorWire` — the admitted-server and federated-descriptor wire shapes the dashboard federation panel decodes additive to the one `Agent/capability#TS_PROJECTION` `DiscoveryResultWire` catalog; the federated tool result rides the reused `Agent/mcp#TS_PROJECTION` `ToolResultWire`, never a branch-side mint.
- Entry: the admitted-server roster crosses as the `FederatedServerWire[]` the dashboard federation panel ingests, the federated descriptors cross as additional `DiscoveryResultWire` rows under the `federated.{server}.*` surface keys the one catalog already carries, and a federated tool call's structured result reconstructs through the existing `ReceiptEnvelopeWire<ToolResultWire>`.
- Packages: Thinktecture.Runtime.Extensions.Json, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, BCL `System.Text.Json`
- Growth: one wire-member row per new server or federated-descriptor field; a new transport kind crosses as its `TransportKind` smart-enum token; zero new surface.
- Boundary: the `TransportKind` `[SmartEnum<string>]` serializes by its string `Key` through the `ThinktectureJsonConverterFactory` so the dashboard switches on the transport token, never the ordinal; the federated descriptors cross as `DiscoveryResultWire` rows in the ONE capability catalog the dashboard command palette already reads — a second federated-descriptor catalog beside the one `Agent/capability#TS_PROJECTION` catalog is the deleted form, the `federated.{server}.{tool}` surface key being the only marker distinguishing a federated row from a native row; the federated tool result is the reused `ToolResultWire` ridden as the `ReceiptEnvelopeWire` `TPayload` so the federation transport decodes the same payload shape the server projection emits, never a re-authored federated-result shape; the trust-scope hash crosses as the deterministic permission-scope string so the dashboard groups federated tools by their server's trust without re-deriving the scope.

```ts signature
type TransportKindKey = "stdio" | "http" | "streamable";

interface FederatedServerWire {
  readonly server: string;
  readonly kind: TransportKindKey;
  readonly endpoint: string;
  readonly trustHash: string;
}

// Federated descriptors cross as DiscoveryResultWire rows under federated.{server}.* surface keys in the
// one capability catalog (Agent/capability#TS_PROJECTION), and a federated tool result rides the existing
// ReceiptEnvelopeWire<ToolResultWire> from Agent/mcp#TS_PROJECTION.
interface FederatedDescriptorWire {
  readonly descriptor: string;
  readonly server: string;
  readonly surface: string;
  readonly effect: "read" | "external" | "pure";
  readonly trustHash: string;
}
```

## [07]-[APP_ROOT_COMPOSITION]

App-root composition admits the federation catalog before the registry freeze: `FederationCatalog.Admit(rows)` validates the configured servers, `FederationProjection.Federate(runtime, services)` folds each server's tools, resources, prompts, and templates into the descriptor fan-in, the registry freezes the combined native-and-federated set into one `FrozenDictionary`, and `CapabilityRegistry.Mount` projects that frozen set's surface index onto the keyed roster gauge.

Admitting at composition time puts the additive `federated.{server}.*` rows through the `ContractGuard.AdditiveOnly` gate the native descriptors ride, and the cross-language SDK codegen reads the combined catalog so a federated tool emits a typed command method in all three languages off the one descriptor source.

```csharp signature
Validation<FederationFault, FederationCatalog> catalog =
    FederationCatalog.Admit(Seq(
        ("filesystem", TransportKind.Stdio, "rasm-mcp-fs", TrustScope.ReadOnly, default(StdioClientTransportOptions)),
        ("database", TransportKind.Http, "https://peer.local/mcp", new TrustScope(EffectClass.External, DataClassification.Operational, FrozenSet<string>.Empty), null)));

// Refused peers boot the host federation-free and never vanish silently: accumulated admission faults ride
// one SpineLog event on the stride the [02] card names, so a mistyped endpoint reads as a logged absence
// rather than an unexplained gap in the command palette.
IServiceCollection federated = await catalog.Match(
    Succ: admitted => FederationProjection.Federate(federationRuntime with { Catalog = admitted }, services).RunAsync(EnvIO.New()),
    Fail: faults => (SpineLog.PeersRefused(logger, faults.Count, faults.Head.Message), services).Item2);

federated.AddSingleton(sp => new CapabilityRegistry(sp.GetServices<CapabilityDescriptor>()));

// Census mount reads the frozen combined catalog against the same instruments every contributor's board
// pack already proved inside `InstrumentFan.Mount`, so this leg carries the one descriptor-side claim the
// mount fold cannot make — a registry fan-in the fan never sees — and refuses while the composition is
// still editable rather than reading empty at first collection. The provider builds from the collection
// this page just folded, and the fan resolves as the composition's ALREADY-mounted one: re-mounting here
// mints a second meter per contributor and proves nothing about the streams the first mount bound.
ServiceProvider provider = federated.BuildServiceProvider();
ReceiptFan fan = provider.GetRequiredService<ReceiptFan>();
Fin<Unit> roster = provider.GetRequiredService<CapabilityRegistry>().Mount(fan.Set);
```


## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
