# [APPHOST_TOOL_FEDERATION]

Rasm.AppHost inverts `Agent/mcp#METHOD_AXIS` into an MCP-CLIENT federation surface: the official `ModelContextProtocol` SDK owns the peer session — JSON-RPC framing, the `IClientTransport` connection, the initialize handshake, and `McpClientTool : AIFunction` adoption — and this surface folds each attached external server's tools, resources, and prompts inward as brokered `CapabilityDescriptor` rows under a `federated.{server}.{tool}` key, so one registry, one broker, and one codegen source serve both directions.

A federated call therefore compiles to the same `CommandBody` a native op compiles to and rides `Agent/capability#COMMAND_ALGEBRA` `CommandAlgebra.Run` through the composition-bound dispatch seam, whose federated arm is the peer call below — so the `Agent/capability#GRANT_BROKER` admission, the dry-run cost preview, the `Agent/runtime#DISPATCH_FRONT_DOOR` veto and mediation, and the `Runtime/determinism#EVENT_LOG` chain all hold exactly as they hold for a native op; `Wire/outbound#HOP_AXIS` `OutboundHop` carries the peer bytes under the existing retry, breaker, and deadline; and the peer-tool JSON Schema becomes the descriptor's content-keyed argument schema for the `Agent/capability#SDK_CODEGEN` pin.

`CapabilityDescriptor`/`DescriptorSurface.Describe`, `CapabilityRegistry`, `CommandAlgebra`/`CommandRuntime`/`CommandBody`/`DispatchReceipt`/`GrantBroker`, `McpDispatch.Project`/`ToolResult`, `SubscriptionLane`/`ExternalValue`, `OutboundHop`/`OutboundSurface`, `TenantContext`, `ReceiptSinkPort`, and `CancelScope` arrive settled, and no eighth port is minted.

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

    // The trust hash IS the ceiling shape's own scope key, so the dashboard groups a server's federated
    // tools by exactly the value it groups every other permission shape by. A second hand-rolled projection
    // over the same three fields is one more unbounded string that drifts from the first the day either
    // spelling changes — and one of the two would be the wrong one with nothing to say which.
    public string Hash => Floor(Ceiling).ScopeHash;
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
            .Map(servers => new FederationCatalog(servers.ToFrozenDictionary(static s => s.Value, StringComparer.Ordinal)))
            .As();

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
- Cases: the projection folds each `McpClientTool` the peer's `McpClient.ListToolsAsync` enumerates into one `CapabilityDescriptor` under `federated.{server}.{tool}`, and the row's model-facing adoption is the one `CommandAIFunction : AIFunction` subclass the server projection mints — never a second `AIFunction` subclass, and never the peer's own `McpClientTool` handed to a model directly, which routes the call around the broker AND carries no `CommandReceipt` at all.
- Entry: `Project(FederationRuntime runtime, FederatedServer server)` returns `IO<Seq<CapabilityDescriptor>>` — opens the peer session through `McpClient.CreateAsync(server.Transport, ...)`, lists the peer's tools, and folds each `McpClientTool` into one brokered descriptor whose `Compile` projects to the federated-call `CommandBody`, returning the descriptor set the composition admits through `DescriptorSurface.Describe`; `Federate(FederationRuntime runtime, IServiceCollection services)` returns `IO<IServiceCollection>` — folds the tool projection beside the `#RESOURCE_PROMPT_FOLD` resource, prompt, and template projection over every admitted server and lands each peer's whole federated census as one `DescriptorSurface.Describe` snapshot across the four surface keys `Surfaces` derives.
- Auto: the peer session is constructed once per server through `McpClient.CreateAsync(IClientTransport, McpClientOptions?, ILoggerFactory?, CancellationToken)` so the SDK owns the initialize handshake and the session lifecycle, the federation holding one `McpClient` per server in the `PeerSession` cell and never re-initializing per call; each `McpClientTool : AIFunction` the peer exposes carries its `JsonSchema` (the `JsonElement` parameter schema on `AIFunctionDeclaration`) which the projection folds into the live `PeerSchemas` id→schema map under the descriptor id — the composition's `McpRuntime.SchemaOf` resolver consults that map so a federated row's argument schema is the peer's published contract verbatim while a native row falls to the `SuiteContracts.Schema<CommandArguments>` shape — content-keyed for the `Agent/capability#SDK_CODEGEN` identity gate and never a re-derived schema; the descriptor's `EffectClass` derives from the peer-tool `ToolAnnotations` alone — a `DestructiveHint` tool is `External` (the saga/compensation path), a `ReadOnlyHint` tool is `Read`, and an unannotated tool defaults to `External` because the host cannot prove a remote tool side-effect-free — and the SAME derived value feeds both the declared effect and the `TrustScope.Floor` lowering (never a `ReturnJsonSchema` gate under-declaring a schema-less destructive tool as `Read`), so a peer tool's declared effect class never exceeds the server's trust ceiling; the `Idempotency` reads the `IdempotentHint` (`Idempotent` when the peer asserts it, else `NonIdempotent` for a tool the host cannot prove repeat-safe), and the `CostModel` carries a fixed `CostUnit.Calls` beside a `CostUnit.BytesEgress` variable the broker meters; the descriptor's `Compile` projects the `CommandArguments` payload into the same `CommandBody` a native op compiles to — surface, op, payload and nothing else — so the federated row enters the registry as a real descriptor the command algebra dispatches, and the composition's dispatch seam routes it to the peer call by that body's surface key, never through an unbrokered side channel and never through a sentinel that only ever refuses; the `federated.{server}.{tool}` surface key namespaces every federated descriptor under its server so two peers exposing a same-named tool never collide and the catalog stays one flat registry.
- Receipt: each projected descriptor is one `CapabilityDescriptor` the registry folds through `DescriptorSurface.Describe`; the federation transition mints one `DescriptorReceipt` per admitted row exactly as a native descriptor's, never a parallel federation receipt.
- Packages: ModelContextProtocol.Core, Microsoft.Extensions.AI.Abstractions, LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox
- Growth: a new federated server is one `FederationProjection.Project` fold over its session; a new peer tool is one descriptor row the fold absorbs at composition with no per-tool edit; the projection is the one inversion seam, so a second descriptor-from-peer fold is the deleted form; zero new surface.
- Boundary: the projection is the only peer-tool-to-descriptor owner — a per-server descriptor table, a hand-mirrored federated tool list, and a second tool catalog are the deleted forms, so every federated tool is a real registry descriptor adopted as the one `CommandAIFunction`; a peer tool the SDK adopted reaches a model as `McpClientTool` ONLY when nothing brokered it, and such a call carries no `CommandReceipt` BY STRUCTURE — that function's own invoke answers `AIContent`, an `AIContent` array, or the serialized `CallToolResult`, never a CLR domain value, so the brokered invoker's foreign arm passes it through and the turn's receipt is `None`, which is the honest read of an unbrokered peer call rather than a gap to close; the inversion is the exact mirror of `Agent/mcp#METHOD_AXIS` — that page folds `DiscoveryResult` outward to an `McpTool` adopted as `AIFunction`, this page folds `McpClientTool` inward to a `CapabilityDescriptor`, so one tool-adoption seam serves both front doors and a federated call and a native call share one `CommandAIFunction` invoker; the peer-tool `JsonSchema` crosses verbatim through the `PeerSchemas` map into the one `McpRuntime.SchemaOf` resolver so the content-addressed codegen identity gate at `Agent/capability#SDK_CODEGEN` sees the peer's published schema, never a host-fabricated one; the `McpClient` session is the SDK's — the federation never re-implements the JSON-RPC client, the initialize handshake, or the tool enumeration, it composes `ListToolsAsync` and holds the session; the `TrustScope` ceiling is the trust boundary so a federated descriptor's effect class is the lesser of the peer's declared class and the server's trust, the broker reading the inherited `PermissionShape` with no federation-specific permission path.

```csharp signature
// --- [SERVICES] -------------------------------------------------------------------------
public sealed record PeerSession(
    FederatedServer Server,
    Atom<Option<McpClient>> Client,
    CancelScope Spine);

// Outbound and CallTimeout live HERE, not on McpRuntime: the server projection has no outbound concern at
// all, and reading a hop surface off it names two members that record declares nowhere. Sessions memoize on
// this record too, so the held client the Auto line promises is a cell this runtime owns rather than a
// re-initialization on every list, call, and subscribe.
public sealed record FederationRuntime(
    FederationCatalog Catalog,
    Atom<HashMap<string, PeerSession>> Sessions,
    Atom<HashMap<string, JsonNode>> PeerSchemas,
    McpRuntime Mcp,
    // The hop CAPABILITY record, never the static surface that consumes it: `OutboundSurface` declares the
    // entries and holds no state, so a column typed on it names a type no instance exists of.
    OutboundRuntime Outbound,
    // The peer-facing client identity, composed rather than literal: a hardcoded name and version report the
    // same build forever to every peer that logs, gates, or negotiates on it.
    Implementation Identity,
    TimeSpan CallTimeout,
    ClockPolicy Clocks,
    // The composition root's one latency-context factory (the modules ledger's latency-context seat), so a
    // federated hop records its phase boundaries instead of compiling silent on the trailing default.
    Func<ILatencyContext> Latency,
    ReceiptSinkPort Sink,
    JsonSerializerOptions Wire,
    CancelScope Spine) {
    // SessionOf is the memoizing read the whole page dispatches through: the first caller opens the peer and
    // seats it, every later caller reads the held client, and the SDK's initialize handshake runs once per
    // server for the process rather than once per request.
    public IO<McpClient> SessionOf(FederatedServer server) =>
        Sessions.Value.Find(server.Value).Bind(static held => held.Client.Value).Match(
            Some: IO.pure,
            None: () => FederationProjection.Open(this, server).Map(opened => Seat(server, opened)));

    McpClient Seat(FederatedServer server, McpClient opened) =>
        (ignore(Sessions.Swap(map => map.AddOrUpdate(
            server.Value,
            existing => (ignore(existing.Client.Swap(_ => Some(opened))), existing).Item2,
            new PeerSession(server, Atom(Some(opened)), Spine)))), opened).Item2;
}

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

    // The schema publish is the fold's ONE declared effect and it is named in the signature rather than
    // hidden inside a projection helper: a static Descriptor(...) that mutates a shared cell reads pure at
    // every call site and re-publishes on every re-projection with nothing saying so.
    static CapabilityDescriptor Descriptor(FederationRuntime runtime, FederatedServer server, McpClientTool tool) {
        var declared = EffectOf(tool.ProtocolTool.Annotations);
        // Each peer publishes its own JsonSchema into the live id->schema map the composition's
        // McpRuntime.SchemaOf resolver consults by descriptor id, so the SDK_CODEGEN pin
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
            // A peer tool reports on its OWN MCP session — the progress notifications the peer emits ride the
            // client the federation holds, never the Compute cell this host's lane would mint, so the federated
            // rows carry no progress admission and the peer's stream stays the peer's.
            progress: None,
            compile: args => FederatedDispatch.Compile(server, tool.Name, args));
    }

    static EffectClass EffectOf(ToolAnnotations? annotations) =>
        annotations?.DestructiveHint == true ? EffectClass.External
        : annotations?.ReadOnlyHint == true ? EffectClass.Read
        : EffectClass.External;

    // Client identity is a PARAMETER of the composed runtime, never a literal: a hardcoded name and version
    // report the same build forever to every peer that logs or gates on it.
    public static IO<McpClient> Open(FederationRuntime runtime, FederatedServer server) =>
        IO.liftAsync(() => McpClient.CreateAsync(
            server.Transport,
            new McpClientOptions { ClientInfo = runtime.Identity },
            loggerFactory: null,
            cancellationToken: runtime.Spine.Token).AsTask());
}
```

## [04]-[FEDERATED_DISPATCH]

- Owner: `FederatedDispatch` the static peer-call surface the composition binds as the dispatch seam's federated arm; `FederatedCall` the call-intent record decoded from the compiled `CommandBody`.
- Cases: a federated descriptor's `Compile` projects to a `CommandBody` under the `federated.{server}` surface key carrying the tool name and the payload; the seam's federated arm decodes it, resolves the peer session, sends `CallToolAsync` over the server's `OutboundHop`, and answers the `DispatchReceipt` the command algebra commits — the agent-facing structured result is the reused `Agent/mcp#TOOL_DISPATCH` `ToolResult` off `McpDispatch.Project`, never a branch-side mint.
- Entry: `Compile(FederatedServer server, string tool, CommandArguments arguments)` returns `Fin<CommandBody>` — the same body shape a native op compiles to, so one algebra dispatches both; `Call(FederationRuntime runtime, FederatedCall call)` returns `IO<Fin<DispatchReceipt>>` — resolves the peer session, invokes `McpClient.CallToolAsync(name, args, progress: null, options, ct)` through `OutboundSurface.Carry<CallToolResult>` on the server's `OutboundHop` under the composition root's `ILatencyContext`, and answers the decoded execution evidence; `Decode(CommandBody body, CorrelationId correlation)` returns `FederatedCall` — the seam's one body-to-peer-call read.
- Auto: the federated row rides the ONE command algebra, so the grant brokerage at `Agent/capability#GRANT_BROKER` runs before the peer call exactly as it runs before a compute dispatch — a denied federated call never reaches the peer and never charges the broker's `CostUnit.Calls`/`CostUnit.BytesEgress` ceiling, the dry-run cost preview the `Agent/mcp#TOOL_DISPATCH` `McpDispatch.Preview` exposes pricing the federated call against the same standing grant a native call prices against, and the `Agent/runtime#DISPATCH_FRONT_DOOR` veto and mediation cover it without a second admission fold; the dispatch closure sends `McpClient.CallToolAsync(name, args, progress: null, options, ct)` over the server's `OutboundHop` so the peer call inherits the hop's retry, breaker, and deadline — a flapping peer breaks on the same circuit breaker an HTTP API breaks on, never a per-server retry loop — and it crosses through the value-producing `Carry<T>` run rather than the outcome-only `Run`, because the peer's result IS the hop's product and a second raw call to fetch it rides no pipeline while the receipt times the first; the hop body states its own `HopOutcome`, so the peer's `IsError` flag lands as `Refused` on the hop's own accounting rather than as a delivery the breaker credits; the peer's `CallToolResult` content blocks and `IsError` flag project onto the reused `ToolResult` (`Tool`/`Content`/`IsError`/`Correlation`) so the federated result rides the existing structured-result wire the agent transport already decodes; a peer-call fault projects to `FederationFault.ToolCallFaulted` (registry-banded) the mediation evidence carries, so a faulted federated call returns a typed transaction disposition, never a thrown exception; the mediation's `BrokeredCall` evidence and the projected `ToolResult` ride the same `ReceiptSinkPort.Send` fan a native command's evidence rides, so a federated tool call is content-addressed and replayable exactly as a native op.
- Receipt: the federated call mints the command algebra's own `CommandReceipt` carrying the decoded `DispatchReceipt`, with the front door's `BrokeredCall` recording the caller modality — never a parallel federation receipt; the chain seat is the front door's `EventLog.Append`, so federation owns no direct chain advance.
- Packages: ModelContextProtocol.Core, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a federated call is one descriptor `Compile` projection over one `CallToolAsync` on the hop; a new peer-result content kind is one column the reused `ToolResult` already carries; zero new surface.
- Boundary: the federated dispatch is the only federated-call owner — a direct `McpClient.CallToolAsync` outside the algebra, a per-server call helper, and an unbrokered peer call are the deleted forms, so every federated call routes through the ONE front door and the ONE transaction `ARCHITECTURE.md#[05]-[BOUNDARIES]` mandates, the federation binding a dispatch ARM and never a second admission path; the dispatch never invokes the compute rail — a federated call is externally executed, so the seam's federated arm is where the body lands and no executing-stratum type appears anywhere on this page; the peer call rides the server's `OutboundHop` so the external server's bytes inherit the existing resilience and the federation owns no transport retry; the `ToolResult` is the reused `Agent/mcp#TOOL_DISPATCH` record ridden as the `ReceiptEnvelopeWire` `TPayload` at `TS_PROJECTION` — a branch-side `ToolResultWire` mint is the named drift defect both this page and the server projection delete; the federated `EffectClass.External` forces the command algebra onto the saga path because no rollback restores a peer's side effect, so a federated write declares a compensation descriptor on the runtime or admits as a single-shot non-compensatable op, never a phantom undo of a remote side effect.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------
public sealed record FederatedCall(
    string Server,
    string Tool,
    JsonElement Payload,
    CorrelationId Correlation);

// --- [OPERATIONS] -----------------------------------------------------------------------
public static class FederatedDispatch {
    // Compile answers the SAME CommandBody a native op answers: the surface key is what the composition's
    // dispatch seam routes on, so one algebra brokers, meters, chains, and compensates every command in the
    // suite. A sentinel that could only ever refuse buys the strata law nothing the body already buys, and it
    // costs the federated rows every guarantee the algebra carries.
    public static Fin<CommandBody> Compile(FederatedServer server, string tool, CommandArguments arguments) =>
        Fin.Succ(new CommandBody($"federated.{server.Value}", tool, arguments.Payload));

    public static FederatedCall Decode(CommandBody body, CorrelationId correlation) =>
        new(body.Surface["federated.".Length..], body.Op, body.Payload, correlation);

    // The seam's federated arm: the peer call rides the server's own hop, so its retry, breaker, and deadline
    // are the transport owner's and this surface holds none. A resolved peer answering nothing and an
    // unresolvable server both land on the typed rail the algebra folds into a rolled-back transaction, so
    // the ONE catch here is what turns the hop's railed error into the Fin the seam contract declares.
    public static IO<Fin<DispatchReceipt>> Call(FederationRuntime runtime, FederatedCall call) =>
        runtime.Catalog.Resolve(call.Server).Match(
            Some: server => Hopped(runtime, server, call)
                | @catch<IO, Fin<DispatchReceipt>>(static _ => true, error => IO.pure(Fin.Fail<DispatchReceipt>(error))),
            None: () => IO.pure(Fin.Fail<DispatchReceipt>(new FederationFault.PeerUnavailable(call.Server))));

    // Carry, never Run: the peer's CallToolResult is a VALUE this hop produces, and Run answers the receipt
    // alone — so a Run here buys the receipt and nothing else, and the second raw call it then forces
    // rides no pipeline, no retry, and no breaker while the receipt attributes its timing to the first.
    static IO<Fin<DispatchReceipt>> Hopped(FederationRuntime runtime, FederatedServer server, FederatedCall call) =>
        from mark in IO.lift(runtime.Clocks.Mark)
        from client in runtime.SessionOf(server)
        from latency in IO.lift(runtime.Latency)
        from _peer in OutboundSurface.Carry(runtime.Outbound, HopOf(server), ct => Peer(runtime, client, call, ct), Some(latency))
        from elapsed in IO.lift(() => runtime.Clocks.Elapsed(mark))
        select Fin.Succ(new DispatchReceipt($"federated.{call.Server}", call.Tool, elapsed));

    // The body states its OWN HopOutcome, which is what puts the peer's error flag on the hop's accounting:
    // an isError result reported as Delivered records a failed peer call as a completed command on every
    // evidence surface downstream AND credits the breaker a success the peer never gave it.
    static async Task<(HopOutcome Outcome, CallToolResult Value)> Peer(FederationRuntime runtime, McpClient client, FederatedCall call, CancellationToken ct) {
        CallToolResult peer = await client.CallToolAsync(
            call.Tool,
            Arguments(call.Payload),
            progress: null,
            options: new RequestOptions { Timeout = runtime.CallTimeout },
            cancellationToken: ct).ConfigureAwait(false);
        return (peer.IsError is true
            ? new HopOutcome.Refused(new FederationFault.ToolCallFaulted($"{call.Server}.{call.Tool}"))
            : new HopOutcome.Delivered(), peer);
    }

    static IReadOnlyDictionary<string, object?> Arguments(JsonElement payload) =>
        payload.ValueKind is JsonValueKind.Object
            ? payload.EnumerateObject().ToDictionary(static p => p.Name, static p => (object?)p.Value)
            : new Dictionary<string, object?>();

    // The arms READ the server, so none of them is static: a static lambda cannot close over the parameter,
    // and the generated dispatch would compile against a capture that is not there.
    static OutboundHop HopOf(FederatedServer server) => server.Kind.Switch(
        stdio: () => new OutboundHop.CompanionSpawn(new ProcessStartInfo(server.Endpoint)),
        http: () => new OutboundHop.HttpApi(new Uri(server.Endpoint)),
        streamable: () => new OutboundHop.ServerStream(new Uri(server.Endpoint)));
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
            progress: None,
            compile: args => FederatedDispatch.Compile(server, uri, args));

    static CapabilityDescriptor PromptDescriptor(FederatedServer server, string name) =>
        CapabilityDescriptor.Of(
            surface: $"federated.{server.Value}.prompt",
            op: name,
            effect: EffectClass.Pure,
            idempotency: Idempotency.Idempotent,
            cost: CostModel.Free,
            permission: server.Trust.Floor(EffectClass.Pure),
            progress: None,
            compile: args => FederatedDispatch.Compile(server, name, args));

    static CapabilityDescriptor TemplateDescriptor(FederatedServer server, string template) =>
        CapabilityDescriptor.Of(
            surface: $"federated.{server.Value}.template",
            op: template,
            effect: EffectClass.Read,
            idempotency: Idempotency.Idempotent,
            cost: new CostModel(new CostVector(HashMap((CostUnit.Calls, 1L))), static _ => CostVector.Zero),
            permission: server.Trust.Floor(EffectClass.Read),
            progress: None,
            compile: args => FederatedDispatch.Compile(server, template, args));
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
            // A resource-update notification carries IDENTITY and nothing measurable — the SDK hands the uri
            // and no body — so the numeric carrier is ABSENT rather than a literal zero, and the quality flag
            // reports what the notification actually proves: the peer said this resource changed. A fabricated
            // zero reading under a true quality flag is a measurement no instrument took, and it grades as one
            // everywhere downstream. The absent carrier is the lane's own shape: the MTConnect leg already
            // parses its text observations into an optional raw for the same reason, so the carrier column is
            // optional at the owner and every producer with nothing to measure writes nothing.
            (notification, ct) => {
                ignore(sink.TryWrite(new ExternalValue(
                    Raw: None,
                    Unit: $"federated.{server.Value}.resource.{notification.Uri}",
                    Good: true,
                    SourceAt: runtime.Clocks.Now,
                    Echo: EchoDiscriminator.None)));
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
// Boot is BOUNDED: the federation spine's token carries the composition deadline, so an unreachable peer
// refuses on it rather than holding the host at composition forever with no timeout on the leg that opens
// each session — the dispatch leg's own RequestOptions.Timeout covers only calls, never the handshake.
IServiceCollection federated = await catalog.Match(
    Succ: admitted => FederationProjection.Federate(federationRuntime with { Catalog = admitted }, services)
        .RunAsync(EnvIO.New(token: federationRuntime.Spine.Token)),
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
