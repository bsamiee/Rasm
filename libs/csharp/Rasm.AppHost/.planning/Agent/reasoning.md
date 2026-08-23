# [APPHOST_REASONING_RUNTIME]

Rasm.AppHost owns the in-process reasoning front door beside MCP server projection and client federation. `ReasoningSession` drives `IChatClient.GetStreamingResponseAsync` with the same brokered `CommandAIFunction` instances `Agent/mcp#METHOD_AXIS` mints, so model-invoked tools route through `CommandAlgebra.Run` and `GrantBroker`. `SemanticDiscovery` embeds each `CapabilityDescriptor` surface and ranks it by cosine similarity through `DiscoveryQuery.ByIntent`. Function transcripts retain exact `CommandReceipt` values only when `FunctionResultContent.Result` carries them, and the `FunctionInvoker` hook is where that contract is enforced rather than hoped for; a `ToolResult` never inflates into a fabricated commit. `ModelGovernance` composes routing, caching, tracing, redaction, the gated image modality, tool invocation, and the token-measured history bound into one draw owner over both the chat and embedding carriers, while the completed `ReasoningTranscript` rides the receipt sink under `ReceiptKind.Model`.

## [01]-[INDEX]

- [02]-[REASONING_LOOP]: `ReasoningSession` over `IChatClient` streaming; `ChatOptions.Tools` is the brokered `CommandAIFunction`.
- [03]-[SEMANTIC_DISCOVERY]: `IEmbeddingGenerator` cosine fold; the `DiscoveryQuery.ByIntent` case over the registry.
- [04]-[REPLAYABLE_TRANSCRIPT]: Exact function-result receipts chain into `EventLog`; absent joins remain explicit.
- [05]-[MODEL_GOVERNANCE]: One middleware fold over both carriers: routing above the cache, content filter below it, window-bounded history, gated image modality, receipt-carrying tool invocation, token-to-cost-to-ledger.
- [06]-[MODAL_INPUT]: `ModalKind` gates one pipeline arm and one intake entry over the same descriptor catalog.

## [02]-[REASONING_LOOP]

- Owner: `ReasoningPolicy` the ONE loop-bound and tool-mode authority — `MODEL_GOVERNANCE` reads its columns rather than restating them; `ReasoningTurn` `[Union]` the streamed-turn disposition; `ReasoningSession` the static in-process agent-loop surface over `IChatClient.GetStreamingResponseAsync`.
- Cases: `ReasoningTurn` = Thinking | ToolCalled | Message | Completed | Faulted — the disposition a streamed reasoning turn folds to as the chat client surfaces text, reasoning content, function calls, and the finish reason; `ToolCalled` carries the call id, descriptor, canonical argument element, and exact optional command receipt as one identity row; `Faulted` carries the app's bounded structured fault observation, never a code beside reminted message text.
- Entry: `Reason(ReasoningRuntime runtime, ReasoningPolicy policy, Seq<ChatMessage> conversation)` returns `IO<ReasoningTranscript>` — the loop streams `IChatClient.GetStreamingResponseAsync` with `ChatOptions.Tools` set to the brokered `CommandAIFunction` set, accumulates the `ChatResponseUpdate` stream into one `ChatResponse`, records each `FunctionCallContent`/`FunctionResultContent` pair as a transcript row, and terminates on the `ChatFinishReason` with the projected `ReasoningTranscript`.
- Auto: the `ChatOptions.Tools` list is the exact brokered `CommandAIFunction` set the `Agent/mcp#METHOD_AXIS` `ToolProjection.Adopt` minted, READ off the one `McpAdoption` product the `Agent/runtime#ADOPTION_SEAM` composition hands every front door — the loop holds the product rather than the registry and the degradation level, so it structurally cannot news up a second projection and a model tool call and an MCP tool call route through the identical brokered invoker over `CommandAlgebra.Run`; the function-invocation iteration is the `MODEL_GOVERNANCE` `FunctionInvokingChatClient` decorator, not a hand-rolled call-and-feed loop — `ReasoningSession` supplies the tool set and the conversation, the decorator runs the tool-call cycle, and the session folds the resulting stream into turns; `ChatOptions.ToolMode` is the policy's `AutoChatToolMode`/`RequiredChatToolMode`/`NoneChatToolMode` row so a session forces, permits, or forbids tool use without a parallel flag; the streaming accumulation uses the `ChatResponseUpdate` stream so a long reasoning turn surfaces incrementally and the host fans interim `Thinking`/`Message` turns to the session reporter exactly as `STREAM_PROGRESS` fans MCP progress; `ChatOptions.Seed` binds to the `DeterminismContext` RNG seed so a recorded reasoning turn replays under the same sampling seed, and `MaximumIterationsPerRequest`/`MaximumConsecutiveErrorsPerRequest` trace to the policy's `DeadlineClass`-derived loop bound, never a literal; the conversation those iterations grow is bounded by the `MODEL_GOVERNANCE` reducer against the resolved route's window at the policy's `WindowShare`, so the iteration bound and the context bound are two columns on one policy rather than one bound and one hope.
- Receipt: every reasoning run — completed or faulted — mints one `ReasoningTranscript` fanned under `ReceiptKind.Model`, which is where this page meets the hook rail: `AppHostHooks.Tap` decorates `ReceiptSinkPort.Emit`, so every transcript crosses `AppHostPoint.Receipt` without this page spelling a fire, and the loop's tool calls cross `AppHostPoint.Command` at the `Agent/runtime#DISPATCH_FRONT_DOOR` veto seat; the terminal turn carries the bounded structured fault observation while the exact `Error` parks on the composition's `FaultCell`; a tool-call row carries `Some(CommandReceipt)` only when the function result exposes the exact minted receipt, otherwise `None`; the per-turn fan is the streamed turn itself, not a separate receipt.
- Packages: Rasm (kernel `FaultCell`, `HookId`), Microsoft.Extensions.AI.Abstractions, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: one turn disposition is one `ReasoningTurn` case breaking every fold arm; a new loop bound is one field on `ReasoningPolicy` and every governance seat reads it with no second edit; a new tool front door is the SAME `CommandAIFunction` set adopted by a new caller, never a new projection; zero new surface.
- Boundary: the reasoning loop is the in-process model-driven command owner — it never executes an op itself, it routes every tool call through the brokered `CommandAIFunction` onto the command algebra, so the transaction, grant, and cost semantics are the command algebra's and the loop is the model-driven dispatch over them; a tool set divorced from the `Agent/mcp#METHOD_AXIS` adoption seam is the deleted form, so the in-process loop and the MCP server share one tool catalog; the `IChatClient` the loop drives is the `MODEL_GOVERNANCE`-wrapped client, never a raw provider client, so an unmetered un-ledgered model draw cannot reach the loop; the loop owns the turn vocabulary and the session-scoped conversation buffer, while `MODEL_GOVERNANCE` owns the metering, caching, tracing, and content-addressing — the two never merge, so the loop stays the orchestration and the middleware stays the policy; a model call that bypasses the function-invocation decorator to invoke a tool directly is the deleted form, because the decorator is the one seam where `ChatOptions.Tools` becomes executed calls.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------
// This record OWNS every loop bound: `MODEL_GOVERNANCE` reads these columns off its `Policy` slot and
// restates none. `WindowShare` is a FRACTION of the resolved `ModelRoute.Window`, not a message count, so
// the budget derives from the route the draw actually resolved.
public sealed record ReasoningPolicy(
    ChatToolMode ToolMode,
    int MaxIterations,
    int MaxConsecutiveErrors,
    double WindowShare,
    DeadlineClass Deadline,
    Option<float> Temperature,
    Option<long> Seed) {
    public static ReasoningPolicy Auto(DeterminismContext context, DeadlineClass deadline) =>
        new(ChatToolMode.Auto, MaxIterations: 16, MaxConsecutiveErrors: 3, WindowShare: 0.75d, deadline, None, Some(context.Seed));

    public ChatOptions Options(Seq<AITool> tools) =>
        new() {
            Tools = tools.ToList(),
            ToolMode = ToolMode,
            Temperature = Temperature.Match(Some: static t => (float?)t, None: static () => (float?)null),
            Seed = Seed.Match(Some: static s => (long?)s, None: static () => (long?)null),
            AllowMultipleToolCalls = true,
        };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReasoningTurn {
    private ReasoningTurn() { }
    public sealed record Thinking(string Reasoning) : ReasoningTurn;
    public sealed record ToolCalled(string CallId, string Descriptor, JsonElement Arguments, Option<CommandReceipt> Receipt) : ReasoningTurn;
    public sealed record Message(string Text) : ReasoningTurn;
    public sealed record Completed(Option<ChatFinishReason> Reason, Option<UsageDetails> Usage) : ReasoningTurn;
    public sealed record Faulted(Rasm.Contracts.Fault.V1.FaultObservation Fault) : ReasoningTurn;
}

// --- [SERVICES] -------------------------------------------------------------------------
// This capsule runs PER-RUN and holds the ADOPTED product, never the projection inputs, so this loop cannot
// mint a second adoption — the second function identity under one tool name the seam exists to foreclose.
// Degradation re-gates by re-composing the product.
public sealed record ReasoningRuntime(
    IChatClient Chat,
    McpAdoption Adopted,
    GovernanceLedger Ledger,
    ClockPolicy Clocks,
    FaultCell Faults,
    ReceiptSinkPort Sink,
    JsonSerializerOptions Wire);

// --- [OPERATIONS] -----------------------------------------------------------------------
public static class ReasoningSession {
    private static readonly HookId FaultPoint = HookId.Create("rasm.apphost.agent.reasoning-draw");

    public static IO<ReasoningTranscript> Reason(ReasoningRuntime runtime, ReasoningPolicy policy, Seq<ChatMessage> conversation) =>
        from tools in IO.lift(() => AdoptedTools(runtime))
        from started in IO.lift(() => runtime.Clocks.Now)
        // Faulted streams fold to a drawn PAIR rather than escaping: the transcript, its digest,
        // and its model fan are the evidence a failed run most needs, and an error thrown past this line
        // deletes all three.
        from drawn in IO.liftAsync(envIO => runtime.Chat
                .GetStreamingResponseAsync(conversation, policy.Options(tools), envIO.Token)
                .ToChatResponseAsync())
                .Map(static response => (Response: response, Fault: Option<Error>.None))
            | @catch<IO, (ChatResponse Response, Option<Error> Fault)>(static _ => true,
                error => IO.pure((new ChatResponse(), Some(error))))
        // Brokered typed faults already parked before their mandatory SDK throw. This outer seat parks only
        // the provider's foreign Error, so one failed draw creates one custody row rather than two.
        from _fault in IO.lift(() => drawn.Fault.Match(
            Some: error => error is Fault
                ? unit
                : ignore(runtime.Faults.Park(point: FaultPoint, cause: error)),
            None: static () => unit))
        from elapsed in IO.lift(() => runtime.Clocks.Now - started)
        let rows = TranscriptRows(drawn.Response, drawn.Fault, runtime.Wire)
        from transcript in IO.lift(() => ReasoningTranscript.Of(drawn.Response, rows, started, elapsed, runtime.Wire))
        from _ in runtime.Sink.Send(Correlation.Mint(), TenantContext.Current, TelemetrySource.AppHost, ReceiptKind.Model.Key, JsonSerializer.SerializeToElement(transcript, runtime.Wire))
        select transcript;

    // Re-projecting and re-adopting HERE is the deleted form: it mints a second function identity under the
    // same tool name, and a model tool call and an MCP tool call would then resolve two brokered instances.
    static Seq<AITool> AdoptedTools(ReasoningRuntime runtime) =>
        runtime.Adopted.Tools.Map(static adopted => (AITool)adopted.Function);

    // The terminal row is the disposition the run reached: a drawn response completes, a faulted draw
    // faults, and the union's Faulted case is reachable exactly here rather than declared and unbuilt.
    static Seq<ReasoningTurn> TranscriptRows(ChatResponse response, Option<Error> fault, JsonSerializerOptions wire) {
        var contents = response.Messages.AsIterable().Bind(static message => message.Contents.AsIterable()).ToSeq();
        var results = contents.OfType<FunctionResultContent>()
            .ToFrozenDictionary(static result => result.CallId, StringComparer.Ordinal);
        return contents.Choose(content => Row(content, results, wire)).ToSeq()
            .Add(fault.Match(
                Some: static error => new ReasoningTurn.Faulted(FaultWire.Observe(error)) as ReasoningTurn,
                None: () => new ReasoningTurn.Completed(Optional(response.FinishReason), Optional(response.Usage))));
    }

    static Option<ReasoningTurn> Row(
        AIContent content,
        FrozenDictionary<string, FunctionResultContent> results,
        JsonSerializerOptions wire) => content switch {
        TextReasoningContent reasoning => Some<ReasoningTurn>(new ReasoningTurn.Thinking(reasoning.Text)),
        TextContent text => Some<ReasoningTurn>(new ReasoningTurn.Message(text.Text)),
        FunctionCallContent call => Some<ReasoningTurn>(new ReasoningTurn.ToolCalled(
            call.CallId,
            call.Name,
            JsonSerializer.SerializeToElement(call.Arguments, wire),
            results.TryGetValue(call.CallId, out var result) ? ReceiptOf(result) : None)),
        FunctionResultContent => None,
        _ => None,
    };

    static Option<CommandReceipt> ReceiptOf(FunctionResultContent result) =>
        result.Result is CommandReceipt receipt ? Some(receipt) : None;
}
```

## [03]-[SEMANTIC_DISCOVERY]

- Owner: `IntentMatch` the ranked descriptor-to-intent projection; `EmbeddingIndex` the frozen descriptor-embedding cell; `SemanticDiscovery` the static embedding-rank fold; the new `DiscoveryQuery.ByIntent(string)` case extending `Agent/capability#DISCOVERY_FOLD`.
- Cases: `DiscoveryQuery` gains one case — `ByIntent(string Intent)` — alongside the settled `ById`/`BySurface`/`ByEffect`/`Permitting`/`All`, so the `Discover` switch is a total dispatch the new case breaks at compile time on every consumer arm; the registry's `Discover` fold gains the `byIntent` arm reading the embedding index.
- Entry: `Index(CapabilityRegistry registry, IEmbeddingGenerator<string, Embedding<float>> embedder)` returns `IO<EmbeddingIndex>` — embeds each descriptor's op-surface text into one frozen `Embedding<float>` per descriptor id at composition; `Rank(EmbeddingIndex index, IEmbeddingGenerator<string, Embedding<float>> embedder, string intent, int top)` returns `IO<Seq<IntentMatch>>` — embeds the intent string and ranks descriptors by cosine similarity over the frozen index, returning the top matches.
- Auto: the embedding index is a FROZEN projection over the registry built once at composition — `Index` folds `DiscoveryQuery.All` into the descriptor rows, embeds each row's `{surface}.{op}` text and its effect/idempotency keys through one batched `IEmbeddingGenerator.GenerateAsync`, and freezes the result into a `FrozenDictionary<string, ReadOnlyMemory<float>>`, so discovery is a read-only vector lookup, never a runtime mutation, mirroring the `CapabilityRegistry` composition-freeze law; the rank rides the BCL numerics primitives end to end and never a hand-rolled loop — `TensorPrimitives.Norm` and `Divide` unit-normalize each row ONCE at composition, so `TensorPrimitives.Dot` over two unit spans IS the cosine and the per-candidate norm a similarity call recomputes on every query is hoisted out of the whole catalog scan; `ByIntent` folds `Rank` to its top descriptors and projects them through the same `CapabilityMatch` projection the other query cases produce so an intent query and an id query return the identical result shape; the embedder is the `MODEL_GOVERNANCE`-composed `IEmbeddingGenerator` — the `Compose(GovernanceRuntime, IEmbeddingGenerator<string, Embedding<float>>)` overload folding `UseOpenTelemetry`/`UseDistributedCache` on the embedding builder and handing the built pipeline back as the one generator a composition root binds — so an intent embedding is content-cached and traced on the same source and store a chat draw rides, and an identical intent re-resolves from the cache without a fresh embedding draw.
- Receipt: `IntentMatch` carries the descriptor id, the cosine score, and the projected `CapabilityMatch`; the index build logs one `SpineLog` event; no parallel discovery receipt.
- Packages: Microsoft.Extensions.AI.Abstractions, LanguageExt.Core, Thinktecture.Runtime.Extensions, System.Numerics.Tensors, BCL inbox
- Growth: the `ByIntent` case is one `DiscoveryQuery` row breaking every consumer; a new ranking signal is one column on `IntentMatch`; a new embedding model is one `IEmbeddingGenerator` injection, never a second index; zero new surface.
- Boundary: `Index` and `Rank` take the governed generator alone — a raw provider generator reaching either is the deleted form, because an untraced uncached embedding draw leaves this card's own re-resolution claim with no mechanism; the semantic discovery is the only intent-resolution owner — a keyword-match heuristic, a hand-tuned synonym table, and a per-op intent annotation are the deleted forms, so an agent resolving "diffuse heat across this mesh" to `TensorOpFamily.HeatFlow` reads the one embedding rank; the `ByIntent` case extends the `Agent/capability#DISCOVERY_FOLD` `[Union]` rather than adding a parallel discovery surface, so the registry's `Discover` stays the single discovery entrypoint and the intent path is one fold arm; the embedding index is frozen at composition so a descriptor added after freeze is invisible to intent resolution until re-index, the same read-only-after-freeze contract the registry carries — a runtime descriptor-embedding mutation is the deleted form; the cosine rank is a similarity heuristic, not a guarantee, so an intent below the policy floor returns no match and the agent falls back to the exact-id path rather than dispatching a wrong tool; the embedded text is the op surface's self-description (`{surface}.{op}` and effect/classification), never the op's body or arguments, so the index is metadata-only and an op's payload never leaks into an embedding.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------
public sealed record IntentMatch(string Descriptor, float Score, CapabilityMatch Result);

public sealed record EmbeddingIndex(
    FrozenDictionary<string, ReadOnlyMemory<float>> Vectors,
    CapabilityRegistry Registry,
    float Floor) {
    public static readonly float DefaultFloor = 0.25f;
}

// --- [OPERATIONS] -----------------------------------------------------------------------
public static class SemanticDiscovery {
    // Freezing UNIT-NORMALIZED vectors makes each rank a DOT PRODUCT rather than a cosine: the
    // per-candidate norm is a property of the frozen row, computed once at composition.
    public static IO<EmbeddingIndex> Index(CapabilityRegistry registry, IEmbeddingGenerator<string, Embedding<float>> embedder) =>
        registry.Discover(new DiscoveryQuery.All()) is var rows && rows.IsEmpty
            ? IO.pure(new EmbeddingIndex(FrozenDictionary<string, ReadOnlyMemory<float>>.Empty, registry, EmbeddingIndex.DefaultFloor))
            : from embeddings in IO.liftAsync(async envIO => await embedder.GenerateAsync(
                  rows.Map(Surface).ToList(), options: null, cancellationToken: envIO.Token))
              let vectors = rows.Zip(embeddings.AsIterable().ToSeq())
                  .Map(static pair => KeyValuePair.Create(pair.First.Descriptor, Normalized(pair.Second.Vector)))
                  .ToFrozenDictionary(StringComparer.Ordinal)
              select new EmbeddingIndex(vectors, registry, EmbeddingIndex.DefaultFloor);

    // Divide is the package's own in-place scale over a destination span; a zero-norm row stays zero and
    // ranks below every floor rather than dividing by nothing.
    static ReadOnlyMemory<float> Normalized(ReadOnlyMemory<float> vector) {
        var unit = new float[vector.Length];
        var norm = TensorPrimitives.Norm(vector.Span);
        if (norm > 0f) TensorPrimitives.Divide(vector.Span, norm, unit);
        return unit;
    }

    public static IO<Seq<IntentMatch>> Rank(EmbeddingIndex index, IEmbeddingGenerator<string, Embedding<float>> embedder, string intent, int top) =>
        from drawn in IO.liftAsync(async envIO => await embedder.GenerateAsync(
            intent, options: null, cancellationToken: envIO.Token))
        let query = Normalized(drawn.Vector)
        let scored = toSeq(index.Registry.Discover(new DiscoveryQuery.All())
            .Choose(row => index.Vectors.TryGetValue(row.Descriptor, out var vector)
                ? Some(new IntentMatch(row.Descriptor, TensorPrimitives.Dot(query.Span, vector.Span), row))
                : Option<IntentMatch>.None)
            .Filter(match => match.Score >= index.Floor)
            .OrderByDescending(static match => match.Score)
            .Take(top))
        select scored;

    static string Surface(CapabilityMatch row) =>
        $"{row.Surface}.{row.Descriptor} effect={row.Effect.Key} idempotency={row.Idempotency.Key}";
}

// --- [COMPOSITION] -----------------------------------------------------------------------
// Composition BINDS `Agent/capability#DISCOVERY_FOLD`'s `byIntent` arm here — one union, one owner, one
// arm, and this page authors the ranking rather than a second query surface:
//
//   new CapabilityRegistry(rows, intentRank: Some<Func<string, Seq<string>>>(intent =>
//       SemanticDiscovery.Rank(index, embedder, intent, top: 8).Run()
//           .Map(static match => match.Descriptor).ToSeq()));
```

## [04]-[REPLAYABLE_TRANSCRIPT]

- Owner: `ReasoningTranscript` the function-invocation transcript record; `TranscriptDigest` the content-address of the whole reasoning turn; `TranscriptProjection` the exact-receipt-to-`LogEntry` fold over `Runtime/determinism#EVENT_LOG` and `#MACRO_ENGINE`.
- Entry: `Chain(ReasoningRuntime runtime, EventLog.Chain chain, ReasoningTranscript transcript, DeterminismContext context)` returns `IO<(EventLog.Chain Chain, Seq<LogEntry> Entries, Seq<string> Missing)>` — folds each exact tool-call `CommandReceipt` into the event-log chain through the owner's publish-free `EventLog.Project` — the dispatch append already fed the durable changefeed, so the projection re-chains without a second write — and carries the receiptless call ids beside the projected entries, so the chained slice and its completeness gap travel as one product; `AsMacro(string macroId, ReasoningTranscript transcript, Seq<LogEntry> entries, Seq<MacroParameter> parameters)` returns `Fin<Macro>` — records the chained slice through `Macro.Record` only when `transcript.MissingReceipts` is empty, refusing an incomplete transcript with the typed `CommandFault.MacroIncomplete` naming every receiptless call.
- Auto: each `ReasoningTurn.ToolCalled` carries `Some(CommandReceipt)` only when `FunctionResultContent.Result` exposes the exact value, and the carriage is a PROVEN path rather than a hoped one — `FunctionInvoker` is the seat whose return the invocation loop hands to `CreateResponseMessages`, whose `CreateFunctionResultContent` lifts that object verbatim onto `FunctionResultContent.Result` with no serialization and no wrapping, so a brokered function returning its receipt lands the exact instance; a foreign tool's result, a null, and every value crossing an MCP wire carry `None`, so projection never invents transaction, cost, dispatch, elapsed, tenant, or instant fields; `Chain` folds only exact receipts through the publish-free `EventLog.Project` while `Missing` names each call whose receipt never joined; the transcript digest composes kernel `ContentHash.Of` over ordered call identities and the model response digest; `AsMacro` gates on `ReasoningTranscript.MissingReceipts` before `Macro.Record` runs, so completeness is a structural refusal, never prose; the reasoning transcript itself rides the receipt sink and never masquerades as a model `CommandReceipt`.
- Receipt: each exact tool-call receipt becomes one `LogEntry`; the whole turn remains one `ReasoningTranscript` carrying its `TranscriptDigest`; absent receipt joins produce no fabricated log entry.
- Packages: System.IO.Hashing, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: one transcript column is one field on `ReasoningTranscript`; a new macro substitution point is one `MacroParameter` row on the recorded slice; a new digest input is one component on the kernel `ContentHash.Of` canonical bytes; zero new surface.
- Boundary: transcript projection never creates evidence absent from the function result; exact command receipts ride the existing event-log chain, while missing joins remain explicit and block macro completeness; `Macro.Record`/`MacroEngine.Play` reuse the command algebra for every captured receipt; `TranscriptDigest` addresses the observed response and call identities but makes no bit-identical model-replay claim beyond the cache owner's guarantee.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------
public sealed record ReasoningTranscript(
    string TranscriptId,
    TranscriptDigest Digest,
    Seq<ReasoningTurn> Turns,
    string ResponseDigest,
    MeterVector ModelCost,
    long InputTokens,
    long OutputTokens,
    Instant Started,
    Duration Elapsed) {
    public static ReasoningTranscript Of(
        ChatResponse response,
        Seq<ReasoningTurn> turns,
        Instant started,
        Duration elapsed,
        JsonSerializerOptions wire) {
        var responseDigest = ContentHash.Hex(ContentHash.Of(response,
            static (drawn, writer) => writer.Rows(
                drawn.Messages.AsIterable().ToSeq(), static (message, member) => member.String(message.Text))));
        var digest = TranscriptDigest.Of(turns, responseDigest, wire);
        return new(
            TranscriptId: digest.Value,
            Digest: digest,
            Turns: turns,
            ResponseDigest: responseDigest,
            ModelCost: ModelGovernance.Tokens(response.Usage),
            InputTokens: response.Usage?.InputTokenCount ?? 0L,
            OutputTokens: response.Usage?.OutputTokenCount ?? 0L,
            Started: started,
            Elapsed: elapsed);
    }

    public Seq<CommandReceipt> Receipts =>
        Turns.Choose(static turn => turn is ReasoningTurn.ToolCalled called ? called.Receipt : Option<CommandReceipt>.None);

    // Completeness is a transcript fact: every `ToolCalled` row whose exact receipt never joined is named by
    // call id, so the macro gate and the chain product read one roster.
    public Seq<string> MissingReceipts =>
        Turns.Choose(static turn => turn is ReasoningTurn.ToolCalled { Receipt.IsNone: true } called ? Some(called.CallId) : None);

    public bool Complete => MissingReceipts.IsEmpty;
}

[ValueObject<string>(
    KeyMemberName = "Value",
    ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit,
    ConversionFromKeyMemberType = ConversionOperatorsGeneration.None)]
public readonly partial struct TranscriptDigest {
    // Fixed field order and ordinal object-property order make call identity independent of dictionary insertion order.
    public static TranscriptDigest Of(Seq<ReasoningTurn> turns, string responseDigest, JsonSerializerOptions wire) {
        using var bytes = new MemoryStream();
        using (var json = new Utf8JsonWriter(bytes)) {
            json.WriteStartObject();
            json.WriteString("response", responseDigest);
            json.WritePropertyName("calls");
            json.WriteStartArray();
            turns.Choose(static turn => turn is ReasoningTurn.ToolCalled call ? Some(call) : Option<ReasoningTurn.ToolCalled>.None)
                .Iter(call => {
                    json.WriteStartObject();
                    json.WriteString("callId", call.CallId);
                    json.WriteString("descriptor", call.Descriptor);
                    json.WritePropertyName("arguments");
                    Canonical(json, call.Arguments);
                    json.WritePropertyName("receipt");
                    if (call.Receipt is { IsSome: true, Case: CommandReceipt receipt })
                        Canonical(json, JsonSerializer.SerializeToElement(receipt, wire));
                    else
                        json.WriteNullValue();
                    json.WriteEndObject();
                });
            json.WriteEndArray();
            json.WriteEndObject();
        }
        return TranscriptDigest.Create(ContentHash.Hex(ContentHash.Of(bytes.ToArray())));
    }

    static void Canonical(Utf8JsonWriter writer, JsonElement element) {
        switch (element.ValueKind) {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject().OrderBy(static property => property.Name, StringComparer.Ordinal)) {
                    writer.WritePropertyName(property.Name);
                    Canonical(writer, property.Value);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray()) Canonical(writer, item);
                writer.WriteEndArray();
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }
}

// --- [OPERATIONS] -----------------------------------------------------------------------
public static class TranscriptProjection {
    public static IO<(EventLog.Chain Chain, Seq<LogEntry> Entries, Seq<string> Missing)> Chain(ReasoningRuntime runtime, EventLog.Chain chain, ReasoningTranscript transcript, DeterminismContext context) =>
        from now in IO.lift(() => runtime.Clocks.Now)
        // Dispatch already fed the durable changefeed once per commit, so this projection
        // re-chains through the owner's publish-free `Project` rather than double-writing the feed.
        let calls = transcript.Turns.Bind(static turn =>
            turn is ReasoningTurn.ToolCalled { Receipt.IsSome: true } called
                ? called.Receipt.ToSeq().Map(receipt => (called.Arguments, Receipt: receipt))
                : Seq<(JsonElement Arguments, CommandReceipt Receipt)>())
        let folded = calls.Fold((Chain: chain, Entries: Seq<LogEntry>(), Logical: 0UL), (acc, call) => {
            var (next, entry) = EventLog.Project(acc.Chain,
                new LogBody.Command(call.Receipt.Descriptor,
                    new CommandArguments(call.Arguments, call.Receipt.Tenant, call.Receipt.Correlation).Digest),
                context, now, acc.Logical);
            return (next, acc.Entries.Add(entry), acc.Logical + 1UL);
        })
        select (folded.Chain, folded.Entries, transcript.MissingReceipts);

    // Macro recording demands the complete receipt slice: a receiptless call refuses with the typed
    // fault naming every gap, so no macro replays a turn whose command evidence never joined.
    public static Fin<Macro> AsMacro(string macroId, ReasoningTranscript transcript, Seq<LogEntry> entries, Seq<MacroParameter> parameters) =>
        transcript.Complete
            ? Fin.Succ(Macro.Record(macroId, entries, parameters))
            : Fin.Fail<Macro>(new CommandFault.MacroIncomplete(string.Join(',', transcript.MissingReceipts)));
}
```

## [05]-[MODEL_GOVERNANCE]

- Owner: `ModelRoute` `[SmartEnum<string>]` the model-selection row family discriminating target model by cost-tier/capability/variant under the `ComparerAccessors.StringOrdinal` accessor, each row carrying its provider model id, `EffectClass` ceiling, and context window; `WindowReducer` the token-measured `IChatReducer` bounding the conversation against that window; `BrokeredInvoker` the `FunctionInvoker` hook carrying the exact `CommandReceipt` onto the function result; `GovernanceLedger` the per-turn token-and-cost cell; `GovernedClient` the composed delegating-pipeline handle; `ModelGovernance` the static middleware-fold surface composing the `Microsoft.Extensions.AI` `ChatClientBuilder` decorators into the one model-governance owner — route, cache, trace, content filter, history bound, image modality, and tool invocation on one decorator chain over both the chat and embedding carriers.
- Cases: `ModelRoute` rows — `Economy`, `Balanced`, `Frontier`, `LongContext` — each carrying its provider model id, the `EffectClass` ceiling the brokered invoker refuses above, and the `Window` token budget the reducer bounds against, so a model draw routes to a target model by feature verdict rather than a fixed client while both the ceiling and the window are enforced columns rather than naming claims; the routing arm reads the `Runtime/features#VERDICT_PROJECTION` `FlagVerdict` variant and maps it to the row, and an absent or below-floor verdict falls to the policy default route, never a hard-coded model.
- Law: this section owns ONE capsule, the composition-time one. `GovernanceRuntime` decides which arms weave and what each reads; `ReasoningRuntime` at `#REASONING_LOOP` drives the client it built. Every loop bound reads `GovernanceRuntime.Policy` — the one `ReasoningPolicy` value — so no seat holds a second copy of an iteration ceiling, an error ceiling, or a window share; every modal admission reads `Modalities`, so a handle and a row cannot disagree; and the route resolves once per draw on `Turn`, the one bounded ambient slot, so three seats read one row. A `Func<>` column survives here only where the provider is a PER-CALL effect (`Verdict` and `Targeting` are evaluated per draw); a pure projection is a member and a handle is a typed port.
- Entry: `Compose(GovernanceRuntime runtime, IChatClient inner)` returns `GovernedClient` — folds the inner `IChatClient` through the one `ChatClientBuilder` chain, outermost first; `Compose(GovernanceRuntime runtime, IEmbeddingGenerator<string, Embedding<float>> embedder)` returns `IEmbeddingGenerator<string, Embedding<float>>` — the SAME owner's embedding arm folding `AsBuilder().UseOpenTelemetry(...).UseDistributedCache(...).Build(...)`, so chat and embedding draws share one governance owner and one store; `Charge(GovernanceRuntime runtime, UsageDetails? usage)` returns `Fin<MeterVector>` — projects a draw's usage onto a `MeterVector` charging `CostUnit.ModelTokens` through `GrantBroker.Admit` and recording it on the ledger, called by the governing client on both verbs rather than by any caller; `Route(GovernanceRuntime runtime, EvaluationContext targeting)` returns `ModelRoute` — resolves the feature verdict to the target row the routing decorator seats on `ChatOptions.ModelId`. DI registration is composition-root surface, never this owner's: the root registers `services.AddChatClient(sp => ModelGovernance.Compose(runtimeOf(sp), inner))` through the `Func<IServiceProvider, IChatClient>` factory overload (DI invokes `ChatClientBuilder.Build` with the root provider at first resolution), the factory's provider feeding `GovernanceRuntime.Services` — so both pipelines reach DI whole and `GovernanceRuntime` never carries `IServiceCollection`.
- Auto: `Build` composes decorators outermost-last, so the chain order IS the nesting law and each seat is placed by what it must observe — `UseOpenTelemetry` outermost spans the whole draw; `ConfigureOptions` seats the routed `ChatOptions.ModelId` on a per-call CLONE of the caller's options and is the ROUTING owner, sitting ABOVE the cache so `DistributedCachingChatClient.GetCacheKey` hashes options already carrying the routed model id and an `Economy` draw can never replay a `Frontier` answer over identical messages (a routing rewrite below the cache is the collision this order deletes, and it also mis-credits `UsageDetails.CachedInputTokenCount` against the wrong route); `CacheKeyAdditionalValues` carries the discriminants the messages and options cannot express — the governance cache epoch and the redaction key generation — so an HMAC key rotation or a taxonomy edit cannot replay a pre-rotation body; `GoverningChatClient` therefore owns REDACTION ALONE, rewriting only `TextContent.Text` and `TextReasoningContent.Text` through the shared classification owner while preserving every other `AIContent` value unchanged, and it sits BELOW the cache so a cached response is redacted exactly once (an HMAC redactor is not idempotent, so a redaction seat above the cache re-tokenizes every replay); `UseImageGeneration` weaves only where `ModalKind.Image` is enabled, substituting the `HostedImageGenerationTool` an intent carries with function tools the loop below it invokes, so the image draw rides the same span, cache, redaction, and broker charge a chat draw rides; `UseFunctionInvocation` runs the tool-call cycle and its `FunctionInvoker` hook is the seam where the exact `CommandReceipt` reaches `FunctionResultContent.Result` and where the route's `EffectClass` ceiling refuses a tool the routed model may not run — the one seat holding both the resolved route and the tool's declared class, so an `Economy` route can no longer carry an irreversible call; `UseChatReducer` sits innermost so every loop iteration re-bounds the conversation the tool cycle just grew. usage projects to `CostUnit.ModelTokens` through `GrantBroker` at the governing client itself, the one decorator that sees a completed draw on both verbs, so an unmetered draw is unreachable rather than merely forbidden and no caller has a charge entry to forget; function-invocation and window bounds come from `ReasoningPolicy` through the runtime record; the same governed client shape serves the reasoning and image front doors, and every model draw is the endpoint's own — the MCP revision election deletes the client-sampling bridge whole. Cache replay is a cache-owner guarantee and does not mint an event-log row.
- Receipt: the completed `ReasoningTranscript` carries `ModelCost` from usage and fans under `ReceiptKind.Model`; the OTel span carries the GenAI trace, selected route, and filter count; the cached-response hit is one `SpineLog` event; no fabricated `agent.reasoning` command receipt enters the event log.
- Packages: Rasm (kernel `AmbientSlot`, `CapabilitySet`, `Cell.Commit`, `FaultCell`, `HookId`), Microsoft.Extensions.AI, Microsoft.Extensions.AI.Abstractions, Microsoft.Extensions.Caching.Hybrid, Microsoft.Extensions.Compliance.Redaction, Microsoft.ML.Tokenizers, OpenFeature, System.IO.Hashing, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new decorator is one `ChatClientBuilder.Use` arm on the fold at its observation seat; a new model route is one `ModelRoute` row carrying its provider model id, effect ceiling, and window; a new content-filter classification is one `DataClassification` row the resolver reads; a new metered model resource rides the existing `CostUnit` axis; a new carrier is one `Compose` overload on this owner, never a second pipeline; zero new surface.
- Boundary: the middleware fold is the suite's only model-governance owner and it spans BOTH model carriers — a raw `IEmbeddingGenerator` reaching `SemanticDiscovery` is the deleted form, because an untraced uncached embedding draw makes the `#SEMANTIC_DISCOVERY` cache claim mechanismless; routing rewrites `ChatOptions.ModelId` through the one options-configuring decorator, redaction reuses `DataClassification`, metering charges `CostUnit.ModelTokens`, cache storage stays on `HybridCache`, and tracing stays on the GenAI source; the history bound is a TOKEN measurement against the route window, so a message-count literal is the deleted form and the shipped `SummarizingChatReducer` is composed for the summarization it owns rather than for a count it does not measure; `TranscriptProjection` chains exact command receipts only; model response cache identity and event-log identity remain distinct until an admitted response-log owner exists.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------
// `Window` is load-bearing rather than descriptive: `WindowReducer` measures the live conversation against
// it, so `LongContext` is a budget the pipeline enforces and not a name.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ModelRoute {
    public static readonly ModelRoute Economy = new("economy", target: "gpt-economy", ceiling: EffectClass.Read, window: 32_768);
    public static readonly ModelRoute Balanced = new("balanced", target: "gpt-balanced", ceiling: EffectClass.External, window: 131_072);
    public static readonly ModelRoute Frontier = new("frontier", target: "gpt-frontier", ceiling: EffectClass.Irreversible, window: 262_144);
    public static readonly ModelRoute LongContext = new("long-context", target: "gpt-long-context", ceiling: EffectClass.External, window: 1_048_576);

    public string Target { get; }
    public EffectClass Ceiling { get; }
    public int Window { get; }

    public static readonly ModelRoute Default = Balanced;

    // Feature verdict's variant string keys the route; an unknown or below-floor variant
    // resolves to the policy default, never a hard-coded provider default.
    public static ModelRoute From(FlagVerdict verdict) =>
        TryGet(verdict.Variant, out var row) ? row : Default;

}

// --- [MODELS] ---------------------------------------------------------------------------
public sealed record GovernanceLedger(
    Atom<HashMap<TenantId, MeterVector>> Tally) {
    public static GovernanceLedger Empty => new(Atom(HashMap<TenantId, MeterVector>()));

    // Folding over the CURRENT value is why a bounded commit makes the answer this call's own
    // total rather than whatever the winning contender left; `Contended` refuses instead of reporting a total
    // no swap recorded.
    public Fin<MeterVector> Record(TenantId tenant, MeterVector cost) =>
        Cell.Commit(Tally, held => held.AddOrUpdate(tenant, existing => existing.Add(cost), cost), Cell.SwapBudget) switch {
            Transition<HashMap<TenantId, MeterVector>>.Contended spent => Fin.Fail<MeterVector>(new GrantFault.Contended(spent.Attempts)),
            var landed => Fin.Succ(landed.Current.Find(tenant).IfNone(MeterVector.Zero)),
        };
}

public sealed record GovernedClient(IChatClient Client, GovernanceLedger Ledger);

// --- [SERVICES] -------------------------------------------------------------------------
// LIFETIME separates this composition-time capsule from `ReasoningRuntime`, never subject —
// this record builds a client, that one drives it. `Policy` is the ONE authority for the loop bounds (the
// three columns that used to sit here beside their `ReasoningPolicy` twins derived from nothing and drifted
// freely). `Modalities` is the ONE admission for which modal arms compose, and a handle absent while its row
// is admitted refuses at the `Fin` mint rather than weaving a dead arm. `CacheEpoch` carries what the request
// cannot express — the cache generation and the redaction key id — so a rotation never replays a body
// redacted under a retired generation. `Tokenizer` is the ONE air-gapped instance `DESCRIPTOR_AXIS` mints.
public sealed record GovernanceRuntime(
    IServiceProvider Services,
    IDistributedCache Cache,
    ILoggerFactory Loggers,
    string TelemetrySource,
    ReasoningPolicy Policy,
    CapabilitySet<ModalKind> Modalities,
    TiktokenTokenizer Tokenizer,
    Option<IImageGenerator> Images,
    Option<ISpeechToTextClient> Speech,
    AmbientSlot<ModelRoute> Turn,
    Seq<object> CacheEpoch,
    GovernanceLedger Ledger,
    Func<EvaluationContext, FlagVerdict> Verdict,
    Func<EvaluationContext> Targeting,
    GrantBroker Broker,
    DataClassificationSet FilterClassification,
    IRedactorProvider Redactors,
    FaultCell Faults);

// --- [COMPOSITION] ----------------------------------------------------------------------
// This scope seats ABOVE the options decorator so every seat below reads one resolution: the retired
// form was a bare `AsyncLocal<ModelRoute?>` seated with `??=` and never restored, so a pooled continuation
// carried the previous draw's route into the next one and a nested draw could not re-resolve at all. The
// kernel slot is declared ONE-LEVEL, so a nested draw refuses rather than silently shadowing, and disposal
// restores the frame it displaced. Named boundary capsule: `IChatClient` answers `Task`/`IAsyncEnumerable`,
// so the scope's lifetime is the framework's statement shape and the rail unwraps at that edge alone.
public sealed class TurnScope(IChatClient inner, GovernanceRuntime runtime) : DelegatingChatClient(inner) {
    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken token = default) {
        using IDisposable held = Entered();
        return await base.GetResponseAsync(messages, options, token).ConfigureAwait(false);
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken token = default) {
        using IDisposable held = Entered();
        await foreach (ChatResponseUpdate update in base.GetStreamingResponseAsync(messages, options, token).ConfigureAwait(false))
            yield return update;
    }

    IDisposable Entered() =>
        runtime.Turn.Enter(ModelRoute.From(runtime.Verdict(runtime.Targeting()))).ThrowIfFail();
}

// --- [OPERATIONS] -----------------------------------------------------------------------
// `AnonymousDelegatingChatClient` is internal and uninstantiable from this package, so redaction of both
// response verbs composes as ONE `DelegatingChatClient` subclass woven through the public `Use` seam.
// ROUTING IS NOT HERE: a rewrite below the cache leaves `GetCacheKey` hashing the caller's un-routed
// `ModelId` and two routes collide on one entry; redaction stays BELOW the cache because an HMAC redactor is
// not idempotent, so a replay would re-tokenize.
public sealed class GoverningChatClient(IChatClient inner, GovernanceRuntime runtime) : DelegatingChatClient(inner) {
    Seq<ChatMessage> Guard(Redactor redactor, IEnumerable<ChatMessage> messages) =>
        messages.AsIterable().Map(message => {
            var guarded = message.Clone();
            guarded.Contents = guarded.Contents.ToList();
            Redact(redactor, guarded.Contents);
            return guarded;
        }).ToSeq();

    static AIContent Redacted(Redactor redactor, AIContent content) => content switch {
        TextContent { Text: { Length: > 0 } text } body => new TextContent(redactor.Redact(text)) {
            Annotations = body.Annotations,
            RawRepresentation = body.RawRepresentation,
            AdditionalProperties = body.AdditionalProperties,
        },
        TextReasoningContent { Text: { Length: > 0 } text } reasoning => new TextReasoningContent(redactor.Redact(text)) {
            ProtectedData = reasoning.ProtectedData,
            Annotations = reasoning.Annotations,
            RawRepresentation = reasoning.RawRepresentation,
            AdditionalProperties = reasoning.AdditionalProperties,
        },
        _ => content,
    };

    static void Redact(Redactor redactor, IList<AIContent> contents) {
        for (var index = 0; index < contents.Count; index++)
            contents[index] = Redacted(redactor, contents[index]);
    }

    static ChatResponse Redact(Redactor redactor, ChatResponse response) {
        foreach (var message in response.Messages)
            Redact(redactor, message.Contents);
        return response;
    }

    // Metering seats HERE and nowhere else: this is the one decorator that sees a completed draw on BOTH
    // verbs, so an unmetered un-ledgered draw is structurally unreachable rather than forbidden by prose.
    public override async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) {
        var redactor = runtime.Redactors.GetRedactor(runtime.FilterClassification);
        var response = Redact(redactor, await base.GetResponseAsync(Guard(redactor, messages), options, cancellationToken).ConfigureAwait(false));
        ignore(ModelGovernance.Charge(runtime, response.Usage));
        return response;
    }

    // A streamed draw reports usage on the update that carries it, so the charge rides that content rather
    // than a re-accumulation of the whole stream this decorator has no reason to hold.
    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        var redactor = runtime.Redactors.GetRedactor(runtime.FilterClassification);
        await foreach (var update in base.GetStreamingResponseAsync(Guard(redactor, messages), options, cancellationToken).ConfigureAwait(false)) {
            Redact(redactor, update.Contents);
            foreach (var usage in update.Contents.OfType<UsageContent>()) ignore(ModelGovernance.Charge(runtime, usage.Details));
            yield return update;
        }
    }
}

// History bounds by TOKEN measurement, so the reducer measures rather than counts: the shipped
// SummarizingChatReducer retains a MESSAGE count, which no route window expresses, and a guessed count
// is a literal wearing a derivation. WindowReducer folds the conversation from the tail through the one
// composition-built TiktokenTokenizer, finds the largest suffix inside `route.Window * WindowShare`, and
// delegates the head to the shipped summarizer at exactly that measured retention with threshold 0 — the
// package owns summarization, this owner owns the measurement the package cannot make. Below-threshold
// conversations return unchanged, so the summarizer mints only on an overflow turn. Message granularity
// alone cannot hold the bound, so the newest turn trims in place before the fold reads it.
public sealed class WindowReducer(GovernanceRuntime runtime, IChatClient summarizer) : IChatReducer {
    public async Task<IEnumerable<ChatMessage>> ReduceAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken) {
        var budget = (int)(ModelGovernance.Route(runtime).Window * runtime.Policy.WindowShare);
        var raw = messages.AsIterable().ToSeq();
        var held = raw.Last.Match(Some: newest => raw.Init.Add(Bounded(newest, budget)), None: () => raw);
        var retained = Retained(held, budget);
        return retained >= held.Count
            ? held
            : await new SummarizingChatReducer(summarizer, targetCount: int.Max(retained, 1), threshold: 0)
                .ReduceAsync(held, cancellationToken).ConfigureAwait(false);
    }

    // A single turn wider than the whole budget defeats count retention: the tail fold keeps zero, the summarizer
    // is handed one message that still breaches, and the declared bound fails silently. The newest turn therefore
    // folds its OWN carriers from the tail — each text carrier admitted whole while the share holds, the first
    // breaching one trimmed, and every carrier past an exhausted share dropped — while non-text carriers pass
    // through untouched, so an over-window turn loses the head of its prose and none of its attachments.
    ChatMessage Bounded(ChatMessage message, int budget) {
        var bounded = message.Clone();
        bounded.Contents = [.. bounded.Contents.AsIterable().ToSeq().Rev()
            .Fold((Spent: 0, Kept: Seq<AIContent>()), (acc, content) => content switch {
                TextContent { Text: { Length: > 0 } text } body => (runtime.Tokenizer.CountTokens(text), budget - acc.Spent) switch {
                    (var cost, var share) when cost <= share => (Spent: acc.Spent + cost, Kept: acc.Kept.Add(body)),
                    (_, 0) => acc,
                    (_, var share) => (Spent: budget, Kept: acc.Kept.Add(Trimmed(body, share))),
                },
                _ => (Spent: acc.Spent, Kept: acc.Kept.Add(content)),
            }).Kept.Rev()];
        return bounded;
    }

    // GetIndexByTokenCountFromEnd is the package's own window primitive — one encoder pass answering the char
    // index whose suffix carries the last N tokens, never a re-encode loop. Its out values are the normalized
    // text and the exact retained count, neither of which this owner reads: the index is the whole answer.
    TextContent Trimmed(TextContent body, int share) =>
        new(body.Text[runtime.Tokenizer.GetIndexByTokenCountFromEnd(body.Text, share, out _, out _)..]) {
            Annotations = body.Annotations,
            RawRepresentation = body.RawRepresentation,
            AdditionalProperties = body.AdditionalProperties,
        };

    // Tail-first fold: the newest turn is the one a reduction must never drop, so the scan accumulates
    // backwards and stops at the first message that would breach the budget. CountTokens over the rendered
    // text is the same encoder the descriptor's pre-flight price reads, so bound and price never disagree.
    int Retained(Seq<ChatMessage> held, int budget) =>
        held.Rev().Fold((Spent: 0, Kept: 0), (acc, message) =>
            acc.Spent + runtime.Tokenizer.CountTokens(message.Text) is var next && next <= budget
                ? (next, acc.Kept + 1)
                : acc).Kept;
}

// FunctionInvoker is the seam the carrier question named: the delegate is a public settable property
// on the SAME configure lambda that bounds the loop — the loop dispatches
// `FunctionInvoker?.Invoke(context, ct) ?? context.Function.InvokeAsync(...)` — so no collector binds at client
// construction and FunctionInvocationServices being protected decides nothing. The returned object IS
// FunctionInvocationResult.Result, which CreateResponseMessages lifts verbatim into FunctionResultContent, so
// this is the one place the receipt contract is enforceable rather than hoped for: a brokered CommandAIFunction
// hands back the CommandReceipt its own OutputSchema declares, and one that hands back anything else refuses
// HERE, naming the tool, instead of producing a receiptless turn that TranscriptProjection.AsMacro later blames
// on a missing join.
//
// The brokered set is recognized through the SDK's approval wrapper, not around it: an irreversible effect is
// exactly the class whose receipt matters most, and testing the outer function alone exempts that one class
// from the assertion built for it. A genuinely foreign tool — an MCP client tool the SDK adopted, whose own
// invoke yields AIContent and never a CLR domain value — passes through untouched and carries None by
// structure, which is the honest read rather than a gap.
//
// The route ceiling lands here for the same reason the receipt does: this is the one seat holding both the
// resolved route and the tool's declared effect class, so a model routed to a read-ceilinged row cannot invoke
// an irreversible tool and the Ceiling column stops being a field nothing compares.
//
// Exemption: the raise is the delegate's OWN declared error channel — the loop folds it into
// FunctionInvocationResult.Status/Exception and MaximumConsecutiveErrorsPerRequest bounds it. The typed fault
// parks on the composition's bounded cell BEFORE its SDK-mandated exception projection crosses that ABI, so
// the host retains the original identity and cause even when the framework exposes only Exception.
public static class BrokeredInvoker {
    private static readonly HookId FaultPoint = HookId.Create("rasm.apphost.agent.function-invoker");

    public static async ValueTask<object?> Invoke(GovernanceRuntime runtime, FunctionInvocationContext context, CancellationToken cancellationToken) =>
        Brokered(context.Function) is { } command
            ? Admitted(runtime, command) switch {
                { IsFail: true } refusal => Raise(runtime, refusal.FailToError()),
                _ => await context.Function.InvokeAsync(context.Arguments, cancellationToken).ConfigureAwait(false) switch {
                    CommandReceipt receipt => receipt,
                    var drifted => Raise(runtime, new CommandFault.ExecutionFaulted(
                        $"<brokered-result-not-a-receipt:{command.Name}:{drifted?.GetType().Name ?? "null"}>")),
                },
            }
            : await context.Function.InvokeAsync(context.Arguments, cancellationToken).ConfigureAwait(false);

    static object? Raise(GovernanceRuntime runtime, Error fault) {
        ignore(runtime.Faults.Park(point: FaultPoint, cause: fault));
        throw fault.ToException();
    }

    // The approval wrapper is transparent to the brokered identity: it gates INVOCATION, never carriage.
    static CommandAIFunction? Brokered(AIFunction function) => function switch {
        CommandAIFunction command => command,
        ApprovalRequiredAIFunction { InnerFunction: CommandAIFunction command } => command,
        _ => null,
    };

    static Fin<Unit> Admitted(GovernanceRuntime runtime, CommandAIFunction command) =>
        command.Effect.Rank <= ModelGovernance.Route(runtime).Ceiling.Rank
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GrantFault.OutOfScope($"<effect-above-route-ceiling:{command.Name}:{command.Effect.Key}>"));
}

public static class ModelGovernance {
    // Build composes outermost-last, so this order IS the nesting law: span outermost, then the routing
    // rewrite, then the cache keyed on the routed options, then redaction, then the gated image arm, then the
    // tool loop, and the window bound innermost where every loop iteration re-enters it.
    public static GovernedClient Compose(GovernanceRuntime runtime, IChatClient inner) =>
        new(
            runtime.Images.Match(
                    Some: images => Chained(runtime, inner).UseImageGeneration(images,
                        static image => image.DataContentHandling = ImageGeneratingChatClient.DataContentHandling.GeneratedImages),
                    None: () => Chained(runtime, inner))
                .UseFunctionInvocation(runtime.Loggers, fi => {
                    fi.MaximumIterationsPerRequest = runtime.Policy.MaxIterations;
                    fi.MaximumConsecutiveErrorsPerRequest = runtime.Policy.MaxConsecutiveErrors;
                    fi.TerminateOnUnknownCalls = true;
                    // The invoker binds HERE, closed over this runtime: a delegate column on the record would
                    // be a knob whose only legal value is this line, and a root that set it to anything else
                    // would silently drop both the receipt assertion and the route ceiling.
                    fi.FunctionInvoker = (ctx, ct) => BrokeredInvoker.Invoke(runtime, ctx, ct);
                })
                .UseChatReducer(new WindowReducer(runtime, inner))
                .Build(runtime.Services),
            runtime.Ledger);

    // Embedding arm of the ONE owner: the same trace source and the same store, so an identical intent
    // re-resolves from the cache with no fresh draw and the #SEMANTIC_DISCOVERY cache claim has a mechanism.
    // Build hands back the composed pipeline as the ONE generator a composition root binds, so no consumer
    // resolves the raw generator and neither Compose overload reaches into a service collection.
    public static IEmbeddingGenerator<string, Embedding<float>> Compose(GovernanceRuntime runtime, IEmbeddingGenerator<string, Embedding<float>> embedder) =>
        embedder.AsBuilder()
            .UseOpenTelemetry(runtime.Loggers, runtime.TelemetrySource)
            .UseDistributedCache(runtime.Cache)
            .Build(runtime.Services);

    static ChatClientBuilder Chained(GovernanceRuntime runtime, IChatClient inner) =>
        inner.AsBuilder()
            .UseOpenTelemetry(runtime.Loggers, runtime.TelemetrySource)
            .Use(client => new TurnScope(client, runtime))
            .ConfigureOptions(options => options.ModelId = Route(runtime).Target)
            .UseDistributedCache(runtime.Cache, cache => cache.CacheKeyAdditionalValues = [.. runtime.CacheEpoch])
            .Use(client => new GoverningChatClient(client, runtime));

    // One resolution per draw, SEATED by the turn decorator and merely READ here: the options decorator takes
    // the target, the reducer measures against the window, and the invoker gates on the ceiling — three reads
    // of one row, so a mid-draw variant flip cannot seat a model whose window the reducer never bounded. The
    // slot is entered immediately above every reader in the same chain, so `None` is unreachable from a
    // composed client and the fallback names the declared policy default rather than a provider's.
    public static ModelRoute Route(GovernanceRuntime runtime) =>
        runtime.Turn.Current.IfNone(ModelRoute.Default);

    public static MeterVector Tokens(UsageDetails? usage) =>
        usage is { TotalTokenCount: { } total }
            ? new MeterVector(HashMap((CostUnit.ModelTokens, total)))
            : MeterVector.Zero;

    // The ambient tenancy is the charge's tenant because a draw has no caller-supplied arguments to carry
    // one, and the default payload element is safe by construction: the pricing shim's Variable arm answers
    // Zero without reading it, so the only estimator that ever sees this element never touches it.
    public static Fin<MeterVector> Charge(GovernanceRuntime runtime, UsageDetails? usage) =>
        Tokens(usage) is var cost && cost.Of(CostUnit.ModelTokens) == 0L
            ? Fin.Succ(MeterVector.Zero)
            : runtime.Broker.Admit(ModelDescriptor(cost), new CommandArguments(default, TenantContext.Current, Correlation.Mint()), DrawMode.Live)
                .Bind(charged => runtime.Ledger.Record(TenantContext.Current.TenantId, charged));

    static CapabilityDescriptor ModelDescriptor(MeterVector cost) =>
        CapabilityDescriptor.Of(
            surface: "agent",
            op: "reasoning",
            arguments: new ArgumentContract.Native(SuiteContracts.Host.GetTypeInfo(typeof(JsonElement))),
            effect: EffectClass.External,
            idempotency: Idempotency.NonIdempotent,
            cost: CostModel.Constant(cost),
            permission: new PermissionShape(FrozenSet<string>.Empty, EffectClass.External, DataClassification.Operational),
            // Pricing shim: the compile arm refuses by construction, so this row never becomes a dispatched
            // body and has no lane to report from — the draw's own streaming rides the chat client.
            progress: None,
            compile: static _ => Fin.Fail<CommandBody>(new KernelFault.InvalidResult(
                Op.Of(), Some("<model-draw-is-not-a-dispatched-body>"))));
}
```

`FlagVerdict` read by the `UseModelSelection` arm is the `Runtime/features#VERDICT_PROJECTION` seam shape the admitted `OpenFeature` provider projects — `(string FlagKey, string Variant, bool Enabled, string Reason)` over `FlagEvaluationDetails<Value>`. This page composes against that verdict at the seam and never owns the `OpenFeature` evaluator; the `Runtime/features.md` owner lands it as the `TARGETED_DELIVERY_EXPERIMENTATION` leg, so a host without the features rail seats the policy-default `ModelRoute.From` fallback and the routing arm is inert.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: One model-governance pipeline, two front doors
    accDescr: Reasoning and the image modality compose one ChatClientBuilder chain whose decorator order places routing above the cache and redaction below it; usage charges the broker and the completed transcript enters the receipt fan.
    Loop["ReasoningSession (in-process)"] --> Pipe
    Modal["ModalIntake.Intent (hosted image tool)"] --> Pipe
    Pipe["OpenTelemetry -> ConfigureOptions route -> DistributedCache -> Governing redaction -> ImageGeneration -> FunctionInvocation -> ChatReducer"] --> Usage["ChatResponse.Usage"]
    Usage --> Charge["GoverningChatClient charge: CostUnit.ModelTokens"]
    Pipe --> Cache["HybridCache content key: routed ModelId + cache epoch"]
    Charge --> Receipt["ReasoningTranscript: ModelCost"]
    Cache --> Receipt
```

## [06]-[MODAL_INPUT]

- Owner: `ModalKind` `[SmartEnum<string>]` realizing kernel `ICapability<ModalKind>` — the modal-capability row that decides which arms compose, admitted as the `CapabilitySet<ModalKind>` column `MODEL_GOVERNANCE` carries; `ModalIntake` the static modal-to-intent surface reading the same descriptor catalog.
- Cases: `ModalKind` rows — speech, image — each a COMPOSITION gate rather than a client carrier: speech transcribes an audio stream into the intent text the SEMANTIC_DISCOVERY fold resolves and needs its own entry because no chat pipeline consumes audio; image is woven INTO the governed pipeline as `UseImageGeneration`, so its row gates one decorator arm and no image entry exists here at all.
- Entry: `Transcribe(GovernanceRuntime runtime, Stream audio)` returns `IO<string>` — transcribes through `ISpeechToTextClient.GetTextAsync` to the intent text `SemanticDiscovery.Rank` resolves; `Intent(GovernanceRuntime runtime, Seq<AITool> tools, ImageGenerationOptions options)` returns `ChatOptions` — seats a `HostedImageGenerationTool` on the tool list so an image request enters the ONE governed client as a tool the pipeline's image arm substitutes and the function loop invokes, the generated `DataContent` arriving on the response contents.
- Auto: the image leg has NO client of its own — `ImageGeneratingChatClient` detects the `HostedImageGenerationTool` in `ChatOptions.Tools` and replaces it with the function tools the chat model invokes, so an image draw is a governed chat draw carrying an image tool and it therefore rides the OTel span, the routed cache key, the `GoverningChatClient` redaction, and the `GrantBroker` charge exactly as text does; the arm weaves only when `ModalKind.Image` is enabled, so a non-modal host resolves no `IImageGenerator` and pays nothing; `DataContentHandling.GeneratedImages` replaces only images this pipeline produced with identifiers on the way back down, so a caller-supplied image in the prompt survives intact; the speech leg stays an entry because audio is not a chat content the pipeline consumes, and it transcribes to intent text the SEMANTIC_DISCOVERY fold ranks so a spoken intent and a typed intent share one resolution path; both clients carry `[Experimental("MEAI001")]` and reach the runtime only through their gate row.
- Receipt: a modal-resolved command mints its `CommandReceipt` through the command algebra exactly as a typed command does; the image draw's tokens ride the enclosing `ChatResponse.Usage` the MODEL_GOVERNANCE charge already meters; no parallel modal receipt and no second metering seat.
- Packages: Microsoft.Extensions.AI, Microsoft.Extensions.AI.Abstractions, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new modality is one `ModalKind` row with either one pipeline decorator arm (when the modality is a chat content the governed client can carry) or one `ModalIntake` entry (when it is not); zero new surface.
- Boundary: the modal surface is the only multi-modal agent-intake owner, and it holds exactly one front door — a direct `IImageGenerator.GenerateAsync` call is the DELETED form, because that draw touches no span, no cache, no redaction, and no broker while the page claims it rides the meter "exactly as a chat draw does"; the collapse makes that claim structural, since the image tool cannot execute except inside the pipeline that carries all four; a `[Union]` over two client handles is likewise deleted — one modality is now a decorator arm and the other an entry, so a two-case carrier models a symmetry that does not exist; the modal output is data the descriptor catalog gates by effect class, never a privileged side channel.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------
// Each row gates COMPOSITION rather than carrying a client: `Image` decides whether `ModelGovernance.Compose`
// weaves its `UseImageGeneration` arm, `Speech` whether a transcriber resolves at all. The rows are a kernel
// capability set on the governance capsule, so a handle absent while its row is admitted refuses at the mint.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ModalKind : ICapability<ModalKind> {
    public static readonly ModalKind Speech = new("speech");
    public static readonly ModalKind Image = new("image");

    static IReadOnlyList<ModalKind> ICapability<ModalKind>.Items => Items;
}

// --- [OPERATIONS] -----------------------------------------------------------------------
[Experimental("MEAI001")]
public static class ModalIntake {
    public static IO<string> Transcribe(GovernanceRuntime runtime, Stream audio) =>
        runtime.Modalities.Admits(ModalKind.Speech)
            ? runtime.Speech.Match(
                Some: client => IO.liftAsync(async envIO =>
                    (await client.GetTextAsync(audio, options: null, cancellationToken: envIO.Token)).Text ?? string.Empty),
                None: () => IO.fail<string>(new FeatureFault.ProviderNotReady("modal-speech")))
            : IO.fail<string>(new FeatureFault.ProviderNotReady("modal-speech"));

    // Intent seats an image request as a TOOL on the one governed client, never a second front door: this
    // pipeline's image arm substitutes that hosted tool carrying its generation options, and produced
    // DataContent lands on response contents the session already folds into turns.
    public static ChatOptions Intent(GovernanceRuntime runtime, Seq<AITool> tools, ImageGenerationOptions options) =>
        runtime.Modalities.Admits(ModalKind.Image)
            ? runtime.Policy.Options(tools.Add(new HostedImageGenerationTool { Options = options }))
            : runtime.Policy.Options(tools);
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
