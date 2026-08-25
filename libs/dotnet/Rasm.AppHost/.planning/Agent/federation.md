# [APPHOST_TOOL_FEDERATION]

Rasm.AppHost inverts `Agent/mcp#METHOD_AXIS` into an MCP-CLIENT federation surface: the official `ModelContextProtocol` SDK owns the peer session — JSON-RPC framing, the `IClientTransport` connection, the initialize handshake, and `McpClientTool : AIFunction` adoption — and this surface folds each attached external server's tools, resources, and prompts inward as brokered `CapabilityDescriptor` rows under a `federated.{server}.{tool}` key, so one registry, one broker, and one codegen source serve both directions.

A federated call compiles to the same `CommandBody` a native op compiles to and rides `Agent/capability#COMMAND_ALGEBRA` `CommandAlgebra.Run` through the composition-bound dispatch seam, whose federated arm is the peer call below. The grant admission, dry-run cost preview, front-door veto, and event-log chain remain the native command path; `Wire/outbound#HOP_AXIS` `OutboundHop` carries the peer bytes under the existing retry, breaker, and deadline; the peer-tool JSON Schema becomes that descriptor's argument contract for MCP projection.

`CapabilityDescriptor`/`DescriptorSurface.Describe`, `CapabilityRegistry`, `CommandAlgebra`/`CommandRuntime`/`CommandBody`/`DispatchResult`/`GrantBroker`, `McpDispatch.Project`/`ToolResult`, `SubscriptionLane`/`ExternalValue`, `OutboundHop`/`OutboundSurface`, `TenantContext`, and `CancelScope` arrive settled.

## [01]-[INDEX]

- [02]-[FEDERATION_AXIS]: Transport-kind taxonomy with external-server admission rows, trust scope, and fault bands.
- [03]-[FEDERATION_PROJECTION]: Peer tool-to-descriptor inversion fold; the reused `CommandAIFunction` wrap.
- [04]-[FEDERATED_DISPATCH]: Brokered peer dispatch closing the `PeerVerb` grammar over the SDK's call members on the command algebra.
- [05]-[RESOURCE_PROMPT_FOLD]: Peer resource, prompt, and template projection with resource-update subscription drain.
- [06]-[APP_ROOT_COMPOSITION]: Catalog admission ahead of the registry freeze, refusal logging, and the census mount.

## [02]-[FEDERATION_AXIS]

- Owner: `TransportKind` `[SmartEnum<string>]` the closed three-row transport taxonomy under the `ComparerAccessors.StringOrdinal` accessor, each row carrying its `IClientTransport` factory delegate; `TrustScope` the per-server operating envelope the federated descriptors inherit; `FederationFault` `[Union]` fault family in the fresh 4800 band; `FederatedServer` `[ValueObject]` the admitted external-server row carrying its transport kind, its constructed `IClientTransport`, and its trust scope; `FederationCatalog` the frozen admitted-server set.
- Cases: 3 transport rows — stdio, http, streamable — the closed `IClientTransport` selection the SDK serves; `FederationFault` = TransportRejected | HandshakeFailed | PeerUnavailable | ToolCallFaulted | UntrustedScope; server identity is open at composition so `FederatedServer` is a `[ValueObject]` admitted dynamically, never a `[SmartEnum]` row.
- Entry: `TransportKind.Transport(string endpoint, StdioClientTransportOptions? stdio)` returns `IClientTransport` — the row's factory delegate constructs the SDK transport from the endpoint and the per-kind options; `FederatedServer.Admit(string server, TransportKind kind, string endpoint, TrustScope trust, StdioClientTransportOptions? stdio)` returns `Validation<Error, FederatedServer>` — the admission rail validates the server id, constructs the transport through the kind's factory, and admits the row, mirroring the `CapabilityDescriptor` admission through `DescriptorSurface.Describe`.
- Auto: the `TransportKind` row owns the `IClientTransport` construction so `Stdio` news a `StdioClientTransport(StdioClientTransportOptions)` over the spawned peer process command, `Http` news a `HttpClientTransport(HttpClientTransportOptions)` at `HttpTransportMode.AutoDetect` (streamable-first, SSE-fallback) over the peer endpoint uri, and `Streamable` news the SAME `HttpClientTransport` pinned to `HttpTransportMode.StreamableHttp` over the resumable HTTP session — the streamable session transport is the SDK's internal `TransportBase` the `HttpClientTransport` selects by mode at connect, never a directly-constructed type, and the three public `IClientTransport` implementors are exactly `StdioClientTransport`/`HttpClientTransport`/`StreamClientTransport`; the kind is the closed vocabulary the admission reads to gate the transport, never a per-server transport reimplementation; the `TrustScope` carries the `PermissionShape` floor every federated descriptor from that server inherits so an untrusted server's tools admit only as `read`-effect descriptors and a trusted server's tools admit at their declared effect class, the trust decision made once at admission and never re-evaluated per call; the admission folds through `Validation<Error, T>` so a malformed server id, an unreachable endpoint, or a scope violation accumulates rather than aborting on the first, and the frozen `FederationCatalog` is the composition-time admitted set the projection reads.
- Output: `FederatedServer` is its own value-object evidence carrying the server key, the transport kind key, the endpoint, and the trust-scope hash; the admission transition logs through one `SpineLog` event inside the `FaultBand.SpineEvents` stride — no parallel admission result.
- Packages: ModelContextProtocol.Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new transport kind is one `TransportKind` row carrying its `IClientTransport` factory the SDK already serves; a new server is one `FederatedServer.Admit` call, never a parallel client; a new fault is one `FederationFault` case; zero new surface.
- Boundary: the federation axis is the only external-MCP-server admission owner — a per-server client, a server-specific connection manager, and a second tool catalog are the deleted forms, so every external server rides one `FederatedServer` row admitted through one rail; the three `IClientTransport` cases are the SDK's transport selection — a hand-rolled JSON-RPC client transport beside the official SDK is the named drift defect at `ARCHITECTURE.md#[05]-[BOUNDARIES]`, so `TransportKind` reads the closed SDK transport vocabulary and never a bespoke socket; `FederatedServer` is a `[ValueObject]` not a `[SmartEnum]` because server identity is composition-open — the admitted set is dynamic config/discovery data, distinct from the closed `TransportKind` taxonomy that IS a smart enum; `FederationFault` rides the kernel `[FaultCase]`/`Fault` floor — `[FaultCase]` realizes the registry over `FaultBand.HostFederation`, and band disjointness is the kernel `Rasm/Domain/rails#FAULT_BAND` registry's type-enforced fact (a duplicate range fails at type initialization), so NO prose census exists here or anywhere; a consumer touching two fault families references each through its namespace-qualified path per `docs/stacks/csharp/language#FORM_CHOOSER`; the `TrustScope` is the federated descriptor's permission floor so a federated tool can never declare a wider effect class than its server's trust admits, the broker reading the inherited `PermissionShape` exactly as it reads a native descriptor's.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TrustScope(
    EffectClass Ceiling,
    DataClassification Classification,
    FrozenSet<string> ObjectSet) {
    public static readonly TrustScope ReadOnly = new(EffectClass.Read, DataClassification.Operational, FrozenSet<string>.Empty);

    public PermissionShape Floor(EffectClass declared) =>
        new(ObjectSet, declared.Rank <= Ceiling.Rank ? declared : Ceiling, Classification);

    public string Hash => Floor(Ceiling).ScopeHash;
}

[ValueObject<string>]
[ValidationError]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FederatedServer {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim().ToLowerInvariant();
        validationError = string.IsNullOrWhiteSpace(value) || value.Contains('.', StringComparison.Ordinal)
            ? new ValidationError(string.Join(" | ", new object?[] { $"federated server id must be non-empty and dot-free: {value}" }))
            : null;
    }

    public TransportKind Kind { get; private init; } = TransportKind.Stdio;
    public IClientTransport Transport { get; private init; } = default!;
    public TrustScope Trust { get; private init; } = TrustScope.ReadOnly;
    public string Endpoint { get; private init; } = string.Empty;

    public static Validation<Error, FederatedServer> Admit(string server, TransportKind kind, string endpoint, TrustScope trust, StdioClientTransportOptions? stdio = null) =>
        Op.Of().AcceptValidated<FederatedServer>(
                fault: Validate(server, provider: null, out FederatedServer? admitted),
                admitted: admitted)
            .Map(row => row with {
                Kind = kind,
                Transport = kind.Transport(endpoint, stdio),
                Trust = trust,
                Endpoint = endpoint,
            })
            .ToValidation();
}

public sealed record FederationCatalog(FrozenDictionary<string, FederatedServer> Servers) {
    public static readonly FederationCatalog Empty = new(FrozenDictionary<string, FederatedServer>.Empty);

    public static Validation<Error, FederationCatalog> Admit(Seq<(string Server, TransportKind Kind, string Endpoint, TrustScope Trust, StdioClientTransportOptions? Stdio)> rows) =>
        rows.Traverse(row => FederatedServer.Admit(row.Server, row.Kind, row.Endpoint, row.Trust, row.Stdio))
            .Map(servers => new FederationCatalog(servers.ToFrozenDictionary(static s => s.Value, StringComparer.Ordinal)))
            .As();

    public Option<FederatedServer> Resolve(string server) =>
        Servers.TryGetValue(server, out var row) ? Optional(row) : None;
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FederationFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.HostFederation;
    private FederationFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record TransportRejected : FederationFault { public TransportRejected(string detail) : base(detail) { } }
    [FaultCase(1)]
    public sealed partial record HandshakeFailed : FederationFault { public HandshakeFailed(string detail) : base(detail) { } }
    [FaultCase(2)]
    public sealed partial record PeerUnavailable : FederationFault { public PeerUnavailable(string detail) : base(detail) { } }
    [FaultCase(3)]
    public sealed partial record ToolCallFaulted : FederationFault { public ToolCallFaulted(string detail) : base(detail) { } }
    [FaultCase(4)]
    public sealed partial record UntrustedScope : FederationFault { public UntrustedScope(string detail) : base(detail) { } }
}
```

## [03]-[FEDERATION_PROJECTION]

- Owner: `FederationProjection` the static peer-to-descriptor inversion fold; `PeerVerb` `[SmartEnum<string>]` the closed four-row federated surface grammar keyed BY suffix — verb-to-suffix is the ONE correspondence the surface keys, the listed projection posture, and the dispatch decode all derive from; the `Sessions` cell the held `McpClient` seats in per admitted server; `FederationRuntime` the held composition state the fold reads — the `FederationCatalog`, the session accessor, and the `McpRuntime` the reused `CommandAIFunction` closes over.
- Cases: the projection folds each `McpClientTool` the peer's `McpClient.ListToolsAsync` enumerates into one `CapabilityDescriptor` under `federated.{server}.{tool}`, and the row's model-facing adoption is the one `CommandAIFunction : AIFunction` subclass the server projection mints — never a second `AIFunction` subclass, and never the peer's own `McpClientTool` handed to a model directly, which routes the call around the broker AND carries no `CommandResult` at all.
- Entry: `Project(FederationRuntime runtime, FederatedServer server)` returns `IO<Seq<CapabilityDescriptor>>` — opens the peer session through `McpClient.CreateAsync(server.Transport, ...)`, lists the peer's tools, and folds each `McpClientTool` into one brokered descriptor whose `Compile` projects to the federated-call `CommandBody`, returning the descriptor set the composition admits through `DescriptorSurface.Describe`; `Federate(FederationRuntime runtime, IServiceCollection services)` returns `IO<IServiceCollection>` — folds the tool projection beside the `#RESOURCE_PROMPT_FOLD` resource, prompt, and template projection over every admitted server and lands each peer's whole federated census as one `DescriptorSurface.Describe` snapshot across the four surface keys `Surfaces` derives.
- Auto: the peer session is constructed once per server through `McpClient.CreateAsync(IClientTransport, McpClientOptions?, ILoggerFactory?, CancellationToken)` so the SDK owns the initialize handshake and the session lifecycle, the federation holding one `McpClient` per server in the `Sessions` cell and never re-initializing per call; each `McpClientTool : AIFunction` the peer exposes carries its `JsonSchema` (the `JsonElement` parameter schema on `AIFunctionDeclaration`) which enters that row as `ArgumentContract.Published`, so the descriptor itself carries the peer's exact published contract and neither a mutable schema map nor a descriptor-id resolver can diverge from it; the descriptor's `EffectClass` derives from the peer-tool `ToolAnnotations` alone — a `DestructiveHint` tool is `External` (the saga/compensation path), a `ReadOnlyHint` tool is `Read`, and an unannotated tool defaults to `External` because the host cannot prove a remote tool side-effect-free — and the SAME derived value feeds both the declared effect and the `TrustScope.Floor` lowering (never a `ReturnJsonSchema` gate under-declaring a schema-less destructive tool as `Read`), so a peer tool's declared effect class never exceeds the server's trust ceiling; the `Idempotency` reads the `IdempotentHint` (`Idempotent` when the peer asserts it, else `NonIdempotent` for a tool the host cannot prove repeat-safe), and the `CostModel` carries a fixed `CostUnit.Calls` beside a `CostUnit.BytesEgress` variable the broker meters; the descriptor's `Compile` projects the `CommandArguments` payload into the same `CommandBody` a native op compiles to — surface, op, payload and nothing else — so the federated row enters the registry as a real descriptor the command algebra dispatches, and the composition's dispatch seam routes it to the peer call by that body's surface key, never through an unbrokered side channel and never through a sentinel that only ever refuses; the `federated.{server}.{tool}` surface key namespaces every federated descriptor under its server so two peers exposing a same-named tool never collide and the catalog stays one flat registry.
- Output: `Project` returns the `CapabilityDescriptor` rows that `Federate` admits as one per-peer snapshot through `DescriptorSurface.Describe`.
- Packages: Rasm (kernel `Cell.Claim`/`Cell.Commit`, `Transition`), ModelContextProtocol.Core, Microsoft.Extensions.AI.Abstractions, LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox
- Growth: a new federated server is one `FederationProjection.Project` fold over its session; a new peer tool is one descriptor row the fold absorbs at composition with no per-tool edit; the projection is the one inversion seam, so a second descriptor-from-peer fold is the deleted form; zero new surface.
- Boundary: the projection is the only peer-tool-to-descriptor owner — a per-server descriptor table, a hand-mirrored federated tool list, and a second tool catalog are the deleted forms, so every federated tool is a real registry descriptor adopted as the one `CommandAIFunction`; a peer tool the SDK adopted reaches a model as `McpClientTool` ONLY when nothing brokered it, and such a call carries no `CommandResult` BY STRUCTURE — that function's own invoke answers `AIContent`, an `AIContent` array, or the serialized `CallToolResult`, never a CLR domain value, so the brokered invoker's foreign arm passes it through and the turn's result is `None`, which is the honest read of an unbrokered peer call rather than a gap to close; the inversion is the exact mirror of `Agent/mcp#METHOD_AXIS` — that page folds `CapabilityMatch` outward to an `McpTool` adopted as `AIFunction`, this page folds `McpClientTool` inward to a `CapabilityDescriptor`, so one tool-adoption seam serves both front doors and a federated call and a native call share one `CommandAIFunction` invoker; the peer-tool `JsonSchema` crosses verbatim as the descriptor's `ArgumentContract.Published` case, so the MCP projection reads the peer's published schema with no cache or host-fabricated fallback; the `McpClient` session is the SDK's — the federation never re-implements the JSON-RPC client, the initialize handshake, or the tool enumeration, it composes `ListToolsAsync` and holds the session; the `TrustScope` ceiling is the trust boundary so a federated descriptor's effect class is the lesser of the peer's declared class and the server's trust, the broker reading the inherited `PermissionShape` with no federation-specific permission path.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PeerVerb {
    public static readonly PeerVerb Tool = new("");
    public static readonly PeerVerb Resource = new(".resource");
    public static readonly PeerVerb Prompt = new(".prompt");
    public static readonly PeerVerb Template = new(".template");
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record FederationRuntime(
    FederationCatalog Catalog,
    Atom<HashMap<FederatedServer, McpClient>> Sessions,
    McpRuntime Mcp,
    OutboundRuntime Outbound,
    Implementation Identity,
    TimeSpan CallTimeout,
    ClockPolicy Clocks,
    Func<ILatencyContext> Latency,
    CancelScope Spine) {
    public IO<McpClient> SessionOf(FederatedServer server) =>
        Sessions.Value.Find(server).Match(
            Some: IO.pure,
            None: () => FederationProjection.Open(this, server).Bind(opened => Claimed(server, opened)));

    IO<McpClient> Claimed(FederatedServer server, McpClient opened) =>
        Cell.Claim(Sessions, server, () => opened) switch {
            Transition<HashMap<FederatedServer, McpClient>>.Committed => IO.pure(opened),
            var ceded => IO.liftAsync(async () => {
                await opened.DisposeAsync().ConfigureAwait(false);
                return ceded.Current[server];
            }),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class FederationProjection {
    public static IO<Seq<CapabilityDescriptor>> Project(FederationRuntime runtime, FederatedServer server) =>
        from client in runtime.SessionOf(server)
        from tools in IO.liftAsync(() => client.ListToolsAsync(null, runtime.Spine.Token).AsTask())
        select toSeq(tools).Map(tool => Descriptor(server, tool));

    public static IO<IServiceCollection> Federate(FederationRuntime runtime, IServiceCollection services) =>
        runtime.Catalog.Servers.Values.AsIterable().ToSeq()
            .FoldM(services, (current, server) =>
                from tools in Project(runtime, server)
                from bound in ProjectResources(runtime, server)
                select DescriptorSurface.Describe(current, Surfaces(server), [.. tools + bound]))
            .As();

    static Seq<string> Surfaces(FederatedServer server) =>
        toSeq(PeerVerb.Items).Map(verb => $"federated.{server.Value}{verb.Key}");

    static CapabilityDescriptor Descriptor(FederatedServer server, McpClientTool tool) {
        var declared = EffectOf(tool.ProtocolTool.Annotations);
        return CapabilityDescriptor.Of(
            surface: $"federated.{server.Value}",
            op: tool.Name,
            arguments: new ArgumentContract.Published(tool.JsonSchema),
            effect: declared,
            idempotency: tool.ProtocolTool.Annotations?.IdempotentHint == true ? Idempotency.Idempotent : Idempotency.NonIdempotent,
            cost: CostModel.Of(
                new MeterVector(HashMap((CostUnit.Calls, 1L))),
                CostModel.Per(CostUnit.BytesEgress, static args => args.Payload.GetRawText().Length)),
            permission: server.Trust.Floor(declared),
            progress: None,
            compile: args => FederatedDispatch.Compile(server, PeerVerb.Tool, tool.Name, args));
    }

    static EffectClass EffectOf(ToolAnnotations? annotations) =>
        annotations?.DestructiveHint == true ? EffectClass.External
        : annotations?.ReadOnlyHint == true ? EffectClass.Read
        : EffectClass.External;

    public static IO<McpClient> Open(FederationRuntime runtime, FederatedServer server) =>
        IO.liftAsync(() => McpClient.CreateAsync(
            server.Transport,
            new McpClientOptions { ClientInfo = runtime.Identity },
            loggerFactory: null,
            cancellationToken: runtime.Spine.Token).AsTask());
}
```

## [04]-[FEDERATED_DISPATCH]

- Owner: `FederatedDispatch` the static peer-call surface the composition binds as the dispatch seam's federated arm; `FederatedCall` the call-intent record decoded from the compiled `CommandBody`, carrying the `PeerVerb` the surface key spells; `PeerAnswer` `[Union]` the typed peer product keyed on the SDK result shape.
- Cases: a federated descriptor's `Compile` projects to a `CommandBody` under the descriptor's OWN `federated.{server}{verb}` surface key carrying the op name and the payload; the seam's federated arm decodes it, resolves the peer session, closes the decoded `PeerVerb` over the SDK's verb member — `CallToolAsync` for tools, `ReadResourceAsync` for resources and (through its RFC-6570 overload) templates, `GetPromptAsync` for prompts — over the server's `OutboundHop`, and answers the `DispatchResult` the command algebra commits — the agent-facing structured result is the reused `Agent/mcp#TOOL_DISPATCH` `ToolResult` off `McpDispatch.Project`, never a branch-side mint.
- Entry: `Compile(FederatedServer server, PeerVerb verb, string op, CommandArguments arguments)` returns `Fin<CommandBody>` — the same body shape a native op compiles to under the verb's own surface key, so one algebra dispatches both; `Call(FederationRuntime runtime, FederatedCall call)` returns `IO<Fin<DispatchResult>>` — resolves the peer session, dispatches the verb-closed peer call through `OutboundSurface.Carry<PeerAnswer>` on the server's `OutboundHop` under the composition root's `ILatencyContext`, and answers the decoded execution evidence; `Decode(CommandBody body, CorrelationId correlation)` returns `FederatedCall` — the seam's one body-to-peer-call read, recovering server and verb from the surface key the dot-free server id makes unambiguous.
- Auto: the command algebra admits and prices the descriptor before the peer call; a refused or faulted call lands `Fin.Fail`, and the algebra refunds its non-committed charge. The decoded `PeerVerb` selects the SDK member under one linked deadline token inside `OutboundSurface.Carry`; the hop accounts an SDK `IsError` as `HopOutcome.Refused`, and a typed peer fault becomes the command transaction's refusal. The front door appends the returned `CommandResult` through its existing event-log path.
- Output: the federated call returns the command algebra's `CommandResult` carrying the decoded `DispatchResult`; federation owns no parallel result or direct chain advance.
- Packages: ModelContextProtocol.Core, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a federated call is one descriptor `Compile` projection over one verb-closed peer call on the hop; a new peer-result content kind is one column the reused `ToolResult` already carries; a new verb is one `PeerVerb` row breaking every dispatch arm at compile; zero new surface.
- Boundary: the federated dispatch is the only federated-call owner — a direct SDK peer call (`CallToolAsync`, `ReadResourceAsync`, `GetPromptAsync`) outside the algebra, a per-server call helper, and an unbrokered peer call are the deleted forms, so every federated call routes through the ONE front door and the ONE transaction `ARCHITECTURE.md#[05]-[BOUNDARIES]` mandates, the federation binding a dispatch ARM and never a second admission path; the dispatch never invokes the compute rail — a federated call is externally executed, so the seam's federated arm is where the body lands and no executing-stratum type appears anywhere on this page; the peer call rides the server's `OutboundHop` so the external server's bytes inherit the existing resilience and the federation owns no transport retry; the `ToolResult` is the reused `Agent/mcp#TOOL_DISPATCH` record carried as its own arm beside the `TS_PROJECTION` `ResultHeaderWire` — a branch-side `ToolResultWire` mint is the named drift defect both this page and the server projection delete; the federated `EffectClass.External` forces the command algebra onto the saga path because no rollback restores a peer's side effect, so a federated write declares a compensation descriptor on the runtime or admits as a single-shot non-compensatable op, never a phantom undo of a remote side effect.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PeerAnswer {
    private PeerAnswer() { }
    public sealed record Tool(CallToolResult Result) : PeerAnswer;
    public sealed record Resource(ReadResourceResult Result) : PeerAnswer;
    public sealed record Prompt(GetPromptResult Result) : PeerAnswer;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FederatedCall(
    string Server,
    PeerVerb Verb,
    string Op,
    JsonElement Payload,
    CorrelationId Correlation);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FederatedDispatch {
    public static Fin<CommandBody> Compile(FederatedServer server, PeerVerb verb, string op, CommandArguments arguments) =>
        Fin.Succ(new CommandBody($"federated.{server.Value}{verb.Key}", op, arguments.Payload));

    public static FederatedCall Decode(CommandBody body, CorrelationId correlation) =>
        body.Surface["federated.".Length..] switch {
            var keyed => keyed.IndexOf('.') switch {
                < 0 => new FederatedCall(keyed, PeerVerb.Tool, body.Op, body.Payload, correlation),
                var dot => new FederatedCall(keyed[..dot], PeerVerb.Get(keyed[dot..]), body.Op, body.Payload, correlation),
            },
        };

    public static IO<Fin<DispatchResult>> Call(FederationRuntime runtime, FederatedCall call) =>
        runtime.Catalog.Resolve(call.Server).Match(
            Some: server => Hopped(runtime, server, call)
                | @catch<IO, Fin<DispatchResult>>(static _ => true, error => IO.pure(Fin.Fail<DispatchResult>(error))),
            None: () => IO.pure(Fin.Fail<DispatchResult>(new FederationFault.PeerUnavailable(call.Server))));

    static IO<Fin<DispatchResult>> Hopped(FederationRuntime runtime, FederatedServer server, FederatedCall call) =>
        from mark in runtime.Clocks.Line.Capture().Match(Succ: IO.pure, Fail: IO.fail<MonotonicStamp>)
        from client in runtime.SessionOf(server)
        from latency in IO.lift(runtime.Latency)
        from _peer in OutboundSurface.Carry(runtime.Outbound, HopOf(server), ct => Peer(runtime, client, call, ct), Some(latency))
        from settled in runtime.Clocks.Line.Capture().Match(Succ: IO.pure, Fail: IO.fail<MonotonicStamp>)
        from span in runtime.Clocks.Line.Elapsed(mark, settled).Match(Succ: IO.pure, Fail: IO.fail<TimeSpan>)
        select Fin.Succ(new DispatchResult($"federated.{call.Server}{call.Verb.Key}", call.Op, Duration.FromTimeSpan(span)));

    static async Task<(HopOutcome Outcome, PeerAnswer Value)> Peer(FederationRuntime runtime, McpClient client, FederatedCall call, CancellationToken ct) {
        using CancellationTokenSource deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(runtime.CallTimeout);
        return await call.Verb.Switch(
            tool: async () => {
                CallToolResult peer = await client.CallToolAsync(
                    call.Op, Arguments(call.Payload), progress: null, options: null,
                    cancellationToken: deadline.Token).ConfigureAwait(false);
                return (peer.IsError is true
                    ? new HopOutcome.Refused(new FederationFault.ToolCallFaulted($"{call.Server}.{call.Op}"))
                    : (HopOutcome)new HopOutcome.Delivered(), (PeerAnswer)new PeerAnswer.Tool(peer));
            },
            resource: async () => {
                ReadResourceResult peer = await client.ReadResourceAsync(
                    call.Op, options: null, cancellationToken: deadline.Token).ConfigureAwait(false);
                return ((HopOutcome)new HopOutcome.Delivered(), (PeerAnswer)new PeerAnswer.Resource(peer));
            },
            prompt: async () => {
                GetPromptResult peer = await client.GetPromptAsync(
                    call.Op, Arguments(call.Payload), options: null, cancellationToken: deadline.Token).ConfigureAwait(false);
                return ((HopOutcome)new HopOutcome.Delivered(), (PeerAnswer)new PeerAnswer.Prompt(peer));
            },
            template: async () => {
                ReadResourceResult peer = await client.ReadResourceAsync(
                    call.Op, Arguments(call.Payload), options: null, cancellationToken: deadline.Token).ConfigureAwait(false);
                return ((HopOutcome)new HopOutcome.Delivered(), (PeerAnswer)new PeerAnswer.Resource(peer));
            }).ConfigureAwait(false);
    }

    static IReadOnlyDictionary<string, object?> Arguments(JsonElement payload) =>
        payload.ValueKind is JsonValueKind.Object
            ? payload.EnumerateObject().ToDictionary(static p => p.Name, static p => (object?)p.Value)
            : new Dictionary<string, object?>();

    static OutboundHop HopOf(FederatedServer server) => server.Kind.Switch(
        stdio: () => new OutboundHop.CompanionSpawn(new ProcessStartInfo(server.Endpoint)),
        http: () => new OutboundHop.HttpApi(new Uri(server.Endpoint)),
        streamable: () => new OutboundHop.ServerStream(new Uri(server.Endpoint)));
}
```

## [05]-[RESOURCE_PROMPT_FOLD]

- Owner: the `FederationProjection` fold EXTENSION — `Resources`/`Prompts`/`Templates` projection arms added to the tool fold; `FederationSubscription` the `McpClient.SubscribeToResourceAsync` per-uri handler seam draining a peer resource-update into the one bounded `Wire/livewire#LANE_SUBSTRATE` `SubscriptionLane` as one `ExternalValue` (the reused at-edge carrier, never a federation-local value type).
- Cases: a peer resource projects to a `read`-effect descriptor under `federated.{server}.resource.{uri}`; a peer prompt projects to a `pure`-effect descriptor under `federated.{server}.prompt.{name}`; a peer resource template projects to a `read`-effect descriptor under `federated.{server}.template.{uri}`, mirroring the server projection's effect-class filter where a `read` descriptor projects as both a tool and a resource and a `pure` template-shaped descriptor projects as a prompt; a peer resource-update notification drains into the same bounded lane the OPC-UA and MQTT subscriptions drain into.
- Entry: `ProjectResources(FederationRuntime runtime, FederatedServer server)` returns `IO<Seq<CapabilityDescriptor>>` — lists the peer's resources, prompts, and templates and folds each through the ONE `Listed` factory reading its `PeerVerb` row's posture, the same fold the tool projection runs extended with three more list arms; `Subscribe(FederationRuntime runtime, FederatedServer server, string uri, BindingSpec spec, ChannelWriter<ExternalValue> writer)` returns `IO<IAsyncDisposable>` — binds `McpClient.SubscribeToResourceAsync(uri, handler, options, ct)` registering the per-uri update handler at subscribe and returning the SDK unsubscribe handle, so a peer resource-change drains into the bounded lane as one `ExternalValue`.
- Auto: the resource fold lists the peer through `McpClient.ListResourcesAsync(RequestOptions?, CancellationToken)` and the prompt fold through `McpClient.ListPromptsAsync(RequestOptions?, CancellationToken)`, each `McpClientResource`/`McpClientPrompt` wrapping into one descriptor whose `Compile` projects the read or prompt-get call exactly as the tool fold projects a `CallToolAsync`; the resource-template fold lists through `McpClient.ListResourceTemplatesAsync(RequestOptions?, CT)` (catalogued at `.api/api-mcp.md` row [9]) so a parameterized peer resource projects as a `read`-effect descriptor carrying the `McpClientResourceTemplate.UriTemplate` RFC-6570 template the SDK evaluates; the subscription binds the `McpClient.SubscribeToResourceAsync(string uri, Func<ResourceUpdatedNotificationParams, CancellationToken, ValueTask> handler, RequestOptions?, CT)` per-uri overload — the SDK registers the handler at subscribe and returns one `IAsyncDisposable` unsubscribe handle — and the handler, on a peer resource-update, `TryWrite`s one `ExternalValue` (the `federated.{server}.resource.{uri}` descriptor key the `ResourceUpdatedNotificationParams.Uri` composes as the unit, the good flag, the `ClockPolicy` instant) into the same bounded `Channel<ExternalValue>` under `BoundedChannelFullMode.DropOldest` the live-wire subscriptions drain into — the foreign notification thread never runs the interior, the bounded lane's drop policy is the producer back-pressure, and the reactive consumer resolves that key and drains the changed resource as one brokered inbound command, so a peer resource-change re-projects through the federated descriptor exactly as a native binding's inbound value re-projects; the subscription is a read-shape variant on the federation fold, never a parallel notification handler — `SubscribeToResourceAsync` with its per-uri handler is the one subscription seam, never a mutated `McpClientHandlers` bag (the registry exposes no `ResourceUpdatedHandler` slot).
- Output: each projected resource/prompt/template descriptor is one `CapabilityDescriptor` the `#FEDERATION_PROJECTION` `Federate` fold lands in the same per-peer snapshot the tool rows ride; each drained resource-update is one `ExternalValue` carrying its federated resource-descriptor key as the unit, which the `Wire/livewire#LANE_SUBSTRATE` lane consumer resolves, brokers, and mints one `CommandResult` from — this page writes the keyed value and mints no result of its own; no parallel federation-resource result.
- Packages: ModelContextProtocol.Core, LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox
- Growth: a peer resource is one fold arm; a peer prompt is one fold arm; a peer template is one fold arm; the subscription drain reuses the one bounded lane the live-wire subscriptions own; zero new surface.
- Boundary: `FederatedServerWire` and `FederatedDescriptorWire` stay withdrawn: federated rows already project through the generated `capability.DiscoverResponse`, so a second peer catalog has no producer or consumer.
- Boundary: the resource/prompt fold is the same `FederationProjection` extended with three list-and-wrap arms, never a second projection — a peer resource/prompt enters only as a brokered descriptor through the one registry, a second resource catalog being the named drift defect; the effect-class filter mirrors the server projection so a federated resource is `read`, a federated prompt is `pure`, and the trust-scope ceiling lowers each exactly as the tool fold lowers a tool's effect class; only `McpClientTool` publishes `AIFunction.JsonSchema`, so resource, prompt, and template rows carry the source-generated open `JsonElement` contract and their SDK call admits the protocol-native argument rows — fabricating JSON-Schema properties from prompt arguments or URI-template variables is refused; the subscription drains into the ONE bounded `SubscriptionLane` the OPC-UA, MQTT, and OPC-UA-PubSub subscriptions drain into — a parallel notification handler, a federation-specific subscription buffer, and a second bounded channel are the deleted forms, so a peer resource-update and an industrial sensor value ride one inbound contract; the `McpClient.SubscribeToResourceAsync` per-uri handler overload is the SDK's resource-update seam so the federation never re-implements the JSON-RPC notification dispatch, it passes the handler at subscribe and holds the returned `IAsyncDisposable`; the resource-update re-projects through the federated descriptor so the changed resource lands as a brokered, metered, audited inbound command, the same brokerage a live-wire inbound value rides, never a privileged write path.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class FederationProjection {
    public static IO<Seq<CapabilityDescriptor>> ProjectResources(FederationRuntime runtime, FederatedServer server) =>
        from client in runtime.SessionOf(server)
        from resources in IO.liftAsync(() => client.ListResourcesAsync(null, runtime.Spine.Token).AsTask())
        from prompts in IO.liftAsync(() => client.ListPromptsAsync(null, runtime.Spine.Token).AsTask())
        from templates in IO.liftAsync(() => client.ListResourceTemplatesAsync(null, runtime.Spine.Token).AsTask())
        select toSeq(resources).Map(resource => Listed(server, PeerVerb.Resource, resource.Uri))
            + toSeq(prompts).Map(prompt => Listed(server, PeerVerb.Prompt, prompt.Name))
            + toSeq(templates).Map(template => Listed(server, PeerVerb.Template, template.UriTemplate));

    static CapabilityDescriptor Listed(FederatedServer server, PeerVerb verb, string op) {
        (EffectClass effect, CostModel cost) = verb.Switch(
            tool: static () => (EffectClass.Read, CostModel.Free),
            resource: static () => (EffectClass.Read, CostModel.Constant(new MeterVector(HashMap((CostUnit.Calls, 1L))))),
            prompt: static () => (EffectClass.Pure, CostModel.Free),
            template: static () => (EffectClass.Read, CostModel.Constant(new MeterVector(HashMap((CostUnit.Calls, 1L))))));
        return CapabilityDescriptor.Of(
            surface: $"federated.{server.Value}{verb.Key}",
            op: op,
            arguments: new ArgumentContract.Native(SuiteContracts.Host.GetTypeInfo(typeof(JsonElement))),
            effect: effect,
            idempotency: Idempotency.Idempotent,
            cost: cost,
            permission: server.Trust.Floor(effect),
            progress: None,
            compile: args => FederatedDispatch.Compile(server, verb, op, args));
    }
}

public static class FederationSubscription {
    public static IO<IAsyncDisposable> Subscribe(FederationRuntime runtime, FederatedServer server, string uri, BindingSpec spec, ChannelWriter<ExternalValue> writer) =>
        from client in runtime.SessionOf(server)
        from handle in IO.liftAsync(() => client.SubscribeToResourceAsync(
            uri,
            (notification, ct) => {
                ignore(writer.TryWrite(ExternalValue.Parsed(
                    reading: None,
                    spec: spec,
                    sourceAt: runtime.Clocks.Now,
                    absent: WireReason.Unavailable,
                    echo: EchoDiscriminator.None,
                    unit: Some($"federated.{server.Value}.resource.{notification.Uri}"))));
                return ValueTask.CompletedTask;
            },
            options: null,
            cancellationToken: runtime.Spine.Token))
        select handle;
}
```

## [06]-[APP_ROOT_COMPOSITION]

App-root composition admits the federation catalog before the registry freeze: `FederationCatalog.Admit(rows)` validates the configured servers, `FederationProjection.Federate(runtime, services)` folds each server's tools, resources, prompts, and templates into the descriptor fan-in, the registry freezes the combined native-and-federated set into one `FrozenDictionary`, and `CapabilityRegistry.Mount` projects that frozen set's surface index onto the keyed roster gauge.

Admitting at composition time seats the additive `federated.{server}.*` rows in the same frozen registry the native descriptors ride, so the combined catalog lands under ONE `DescriptorPin`, and the cross-language SDK codegen reads the combined catalog so a federated tool emits a typed command method in all three languages off the one descriptor source.

```csharp
Validation<Error, FederationCatalog> catalog =
    FederationCatalog.Admit(Seq(
        ("filesystem", TransportKind.Stdio, "rasm-mcp-fs", TrustScope.ReadOnly, default(StdioClientTransportOptions)),
        ("database", TransportKind.Http, "https://peer.local/mcp", new TrustScope(EffectClass.External, DataClassification.Operational, FrozenSet<string>.Empty), null)));

IServiceCollection federated = await catalog.Match(
    Succ: admitted => FederationProjection.Federate(federationRuntime with { Catalog = admitted }, services)
        .RunAsync(EnvIO.New(token: federationRuntime.Spine.Token)),
    Fail: faults => (SpineLog.PeersRefused(logger, faults.Count, faults.Head.Message), services).Item2);

federated.AddSingleton(sp => new CapabilityRegistry(sp.GetServices<CapabilityDescriptor>()));

ServiceProvider provider = federated.BuildServiceProvider();
InstrumentSet instruments = provider.GetRequiredService<InstrumentSet>();
Fin<Unit> roster = provider.GetRequiredService<CapabilityRegistry>().Mount(instruments);
```


## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
