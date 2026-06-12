# http-seams — bedrock

## standard handler composition

- `AddStandardResilienceHandler` composes exactly five strategies, outermost to innermost: rate limiter → total-request timeout → retry → circuit breaker → attempt timeout.
- All five strategies draw from one `HttpStandardResilienceOptions` record bound to the named options `"{clientName}-standard"`.
- The hop's entire posture is one options object — reloadable, config-bindable, and validated as a unit.
- Configuration section keys mirror the options property names (`RateLimiter`, `TotalRequestTimeout`, `Retry`, `CircuitBreaker`, `AttemptTimeout`) under strict binding — the config schema IS the options shape, never a parallel vocabulary.
- The handler sets `HttpClient.Timeout` to `Timeout.InfiniteTimeSpan`: the client's own default timeout is deleted so the pipeline is the only deadline owner on the seam.
- Restoring a finite client timeout re-creates a second, untyped deadline that surfaces as bare cancellation instead of typed rejection — the client timeout and the pipeline total timeout must never both be finite.
- Defaults: total-request timeout 30s; attempt timeout 10s; retry 3 attempts with exponential backoff, jitter, and `ShouldRetryAfterHeader` true; circuit breaker at 0.1 failure ratio, 100 minimum throughput, 30s sampling, 5s break; rate limiter as a 1000-permit, 0-queue concurrency limiter.
- Strategy names are fixed — `Standard-RateLimiter`, `Standard-TotalRequestTimeout`, `Standard-Retry`, `Standard-CircuitBreaker`, `Standard-AttemptTimeout` — so telemetry identity for the standard chain is stable across every client in a process.
- The pipeline name `"{clientName}-standard"` becomes the `pipeline.name` telemetry dimension; the instance dimension stays empty until selection is declared.
- Cross-field coherence is enforced at startup, not first request: options register through validate-on-start with a generated per-field validator plus a custom cross-field validator per options name.
- Per-options-name validation means two clients with different configurations validate independently — one client's misconfiguration names that client at boot.
- Cross-field rule one: attempt timeout must not exceed total timeout.
- Cross-field rule two: breaker sampling duration must be at least twice the attempt timeout — shorter sampling cannot accumulate meaningful statistics between timeouts.
- Cross-field rule three (hedging variant): cumulative hedging delay (`MaxHedgedAttempts × Delay`) must fit inside the total timeout when no delay generator is set — a hedging plan that cannot complete within its own budget fails boot.
- `Configure(IConfigurationSection)` binds with `ErrorOnUnknownConfiguration = true` — a typo in a resilience configuration key is a hard error, not a silently ignored setting.
- An empty configuration section is rejected with `ArgumentException` — binding against nothing is a wiring defect, not a defaults fallback.
- Standard and hedging options accept the same three `Configure` forms — configuration section, action, and service-aware action — so policy arrives from configuration, code, or container state under one named-options identity.
- Registration and configuration are separable verbs: the handler registers once, and `Configure` calls chain onto the returned builder — the section and action overloads of the registration call are just the fused spelling.
- The standard handler wires its own reload internally — options changes rebuild the seam pipeline with zero user reload wiring.
- Named options instances are per-name: each client's strategy options are unique instances constructed for that name, so mutating one client's policy never bleeds into another client.
- `ShouldRetryAfterHeader` is an active property: setting it true installs a delay generator that resolves the response's retry-after header; setting it false removes the generator.
- Retry-after honoring parses both the date and delta header forms; negative computed spans clamp to zero.
- A custom `DelayGenerator` assigned after construction silently replaces retry-after honoring — the header generator and a custom generator occupy one slot, and keeping both means composing the header parse inside the custom generator.
- `AddResilienceHandler(pipelineName, configure)` is the custom seam: the final pipeline name is `"{clientName}-{pipelineName}"`.
- The custom configure callback receives the typed builder plus a `ResilienceHandlerContext` exposing `ServiceProvider`, `BuilderName`, `InstanceName`, `GetOptions<TOptions>(name)`, `EnableReloads<TOptions>(name)`, and `OnPipelineDisposed`.
- Custom pipelines therefore get the same options, reload, and disposal rails as the standard one — the standard handler holds no privileged machinery.
- Each `EnableReloads` call adds a reload token to the current pipeline generation; tokens accumulate, and any one firing rebuilds the pipeline — multiple options types can all drive one seam's reloads.
- Options read through `GetOptions` inside the configure callback are read per generation: reload re-runs the callback and re-reads — configure-time reads are never stale beyond one generation.
- The builder return values expose `PipelineName` and `Services` — the composition points for selection, configuration, and further service wiring.
- Every HTTP resilience pipeline in a process lives in one keyed registry under the HTTP key type, with name and instance formatted into the two telemetry axes — custom and standard handlers share the registry, not parallel stores.
- Each `AddResilienceHandler` call appends one delegating handler wrapping one pipeline; multiple calls on one client stack pipelines multiplicatively in handler-chain order.
- Handler-chain position decides per-attempt versus per-call execution: handlers registered after the resilience handler run once per attempt, because the pipeline re-enters the inner chain on every retry and hedge.
- Handlers registered before the resilience handler run once per logical call — token-refresh handlers belong inside the resilience handler, single-shot logging belongs outside.
- `RemoveAllResilienceHandlers` strips every resilience handler from the client's additional-handlers list — the reset verb before declaring a replacement posture.
- `ResilienceHandler` is a public delegating handler with two constructors — a fixed `ResiliencePipeline<HttpResponseMessage>` or a per-request `Func<HttpRequestMessage, ResiliencePipeline<HttpResponseMessage>>` provider.
- The public constructors mean hand-built pipelines embed in any handler chain without the registration path — the handler is reusable infrastructure, not registration-locked.
- Several seam surfaces are experimental-gated under one diagnostic id: `RemoveAllResilienceHandlers`, the context request-message extensions, the method filters, and the token-aware transient predicates — consuming them requires acknowledging the diagnostic, and the acknowledgment is pinned once centrally, never per call site.

## transient classification

- The standard transient predicate is one closed set: status 408, status 429, any status ≥ 500, `HttpRequestException`, and `TimeoutRejectedException`.
- Treating `TimeoutRejectedException` as transient means retry observing the inner attempt-deadline is built into the predicate, not an accident of strategy ordering.
- Connection-establishment timeouts are classified transient by structure, not type: an `OperationCanceledException` sourced from the core runtime with an inner `TimeoutException`, while the caller's token has not fired.
- The token check is the entire distinction between a connect timeout and caller cancellation — the exception shapes are otherwise identical.
- The hedging predicate is the retry predicate plus `BrokenCircuitException`: a hedged attempt landing on an endpoint with an open breaker counts as transient and immediately routes the next attempt to the next endpoint.
- For plain retry a broken circuit is not transient — retrying into an open breaker spends budget on the same dead endpoint.
- The predicate split between the two families encodes the movement law: retry stays on one endpoint; hedging moves across endpoints — the same fault classifies differently because the remedies differ.
- `DisableForUnsafeHttpMethods` excludes POST, PATCH, PUT, DELETE, and CONNECT from retry; `DisableFor(params HttpMethod[])` is the general form.
- Both method filters decorate the existing `ShouldHandle` rather than replace it — transient classification stays intact under the filter.
- The filter recovers the request from the outcome's request message, falling back to the context's request when the outcome carries none — method filtering works even for exception outcomes that carry no response.
- The classification helpers are public composition material — `HttpClientResiliencePredicates.IsTransient` and `HttpClientHedgingResiliencePredicates.IsTransient`.
- The status and exception sub-tests inside them are not independently exposed: the public unit is the whole transient definition, not its parts — partial reuse means re-stating the parts, full reuse means composing the predicate.
- Custom pipelines reuse the standard fault vocabulary through those helpers instead of re-deriving status-code sets — one transient definition per process.

## per-hop selection

- One handler serves many pipeline instances: `SelectPipelineByAuthority` keys the pipeline per scheme+host+port.
- One named client fanning to N authorities gets N isolated breaker, limiter, and deadline states from one declaration — instance isolation without registration multiplication.
- Authority keys are cached in a concurrent (scheme, host, port)→string map — per-request key derivation costs one dictionary probe after first contact with an authority.
- The authority provider requires an absolute request URI and throws `InvalidOperationException` without one — relative-URI clients cannot ride authority selection.
- `SelectPipelineBy(selectorFactory)` is the general request→key projection; the selector output becomes the pipeline instance name.
- Selection exists uniformly on the custom, standard, and hedging builders — per-hop instance fan-out is not a hedging privilege.
- The key registry is two-axis: registration keys are (pipeline name, instance name) records whose builder comparer matches ordinally on name only.
- Every instance shares the single registered builder row and materializes its own pipeline lazily on first request to that instance — instance fan-out adds zero registrations.
- Key comparison is ordinal and case-sensitive — case differences in selector output create distinct pipeline instances with distinct breaker states.
- Builder name and instance name become the `pipeline.name`/`pipeline.instance` telemetry dimensions; selector outputs are emitted into metrics and logs.
- Selectors must never project secrets or unbounded cardinality — a per-user key is both a telemetry leak and a metric-space explosion.
- The selector is touched eagerly at handler construction with a synthetic request — a throwing selector fails at wiring time, not first call.
- Without a selector the handler resolves one pipeline at construction and reuses it for every request.
- With a selector resolution is per request — the selector is the only per-request cost of multi-instance seams.
- Registering any resilience handler installs an HTTP metrics enricher globally: outcomes carrying a response stamp `error.type` with the numeric status code on resilience metrics.
- Status-class dashboards therefore come from metric tags, not response logging — no log scraping for error-rate views.

## context flow

- The request↔context bridge is bidirectional and key-stable: the context rides the request under the request-options key `"Resilience.Http.ResilienceContext"` (`GetResilienceContext`/`SetResilienceContext` on `HttpRequestMessage`).
- The request rides the context under the resilience-property key `"Resilience.Http.RequestMessage"` (`GetRequestMessage`/`SetRequestMessage` on `ResilienceContext`).
- Strategies reach the live request through the context; callers reach the live context through the request — both directions are one typed property read.
- The handler reuses a caller-attached context when the request already carries one, otherwise it leases from the shared pool.
- On completion, leased contexts are returned to the pool and detached from the request; caller-attached contexts are re-attached — context lifetime ownership follows who created it.
- Pre-attaching a context is the sanctioned channel for threading caller properties through the pipeline and reading strategy-written properties afterward — the seam's typed side channel.
- The pipeline provider runs before context attachment, on the raw request — instance selection can depend only on the request, never on context properties.
- Execution is closure-free and outcome-first: the handler executes through the outcome-capturing entrypoint with (handler, request) as typed state, captures inner-handler exceptions into the outcome, and rethrows only at the outer edge.
- Strategies always see outcomes, never in-flight exceptions; the seam's final surface is throw-or-response, with outcome capture purely internal.
- Pipeline rejections propagate from the handler as their typed exceptions — deadline, open-circuit, and admission rejections are the seam's outbound fault vocabulary, distinguishable by type at every caller.
- The total timeout sits above hedging, so its child token fans into every attempt context — one budget expiry cancels all concurrent hedged attempts at once, never one-by-one.
- Request metadata present on the message propagates into context properties under the `"Extensions-RequestMetadata"` key before execution — downstream enrichment reads it from the context without re-touching the request.
- The handler implements both async and sync sends with the same context discipline — a fully synchronous pipeline path exists for sync HTTP stacks.
- Hedging publishes two further context properties — the request snapshot under `"Resilience.Http.Snapshot"` and the live routing strategy under `"Resilience.Http.RequestRoutingStrategy"`.
- Those two properties are how the per-attempt action generator and any custom strategy in the chain coordinate without shared fields — context properties are the chain's only shared state.

## hedging pipeline

- `AddStandardHedgingHandler` builds two chained pipelines, not one: the primary `"{clientName}-standard-hedging"` = routing strategy → request-snapshot strategy → total-request timeout → hedging.
- The per-endpoint pipeline `"{clientName}-standard-hedging-endpoint"`, selected by authority by default, = rate limiter → circuit breaker → attempt timeout.
- The endpoint triple lives below hedging so each endpoint owns isolated admission, health, and deadline state.
- The pool of per-endpoint breakers is what prevents hedging from re-hitting a dead endpoint — endpoint isolation is the hedging family's health memory.
- The endpoint options record carries exactly three slots — rate limiter, circuit breaker, attempt timeout — and no retry slot exists below hedging: hedging IS the family's only attempt loop, and the options shape forecloses a second one.
- Routing and snapshotting execute outside the total timeout — the budget starts after route resolution and snapshot capture, so route-table and clone costs are unbudgeted setup, not allotment spend.
- The hedging handler also sets `HttpClient.Timeout` to `Timeout.InfiniteTimeSpan` — the same single-deadline-owner law as the standard handler.
- Strategy names are fixed across the pair: `StandardHedging-TotalRequestTimeout`, `StandardHedging-Hedging`, `StandardHedging-RateLimiter`, `StandardHedging-CircuitBreaker`, `StandardHedging-AttemptTimeout`.
- Defaults: total timeout 30s; hedging delay 2s with one hedged attempt; endpoint attempt timeout 10s; endpoint limiter and breaker at the shared defaults.
- Naming asymmetry: hedging options bind under the client name itself, with no suffix, and routing options bind under the client name too — only the standard handler suffixes its options name.
- Configuration sections for the two handler families are therefore shaped differently, and section paths are not portable between them.
- Both hedging pipelines wire reload against the same options name — one options write rebuilds the pair coherently.
- `AddStandardHedgingHandler(Action<IRoutingStrategyBuilder>)` configures routes inline; the returned `IStandardHedgingHandlerBuilder` carries `RoutingStrategyBuilder` for later route configuration.
- The snapshot strategy captures method, URI, version, headers, and request options once before hedging.
- Each hedged attempt materializes a fresh request from the snapshot — concurrent clone-and-mutate races on one message are excluded by construction.
- Stream-bodied content is rejected at snapshot time with `InvalidOperationException` — hedging requires replayable bodies.
- The content instance is shared across clones, not deep-copied: hedged attempts must not consume content destructively, and content must tolerate concurrent reads.
- The hedging action generator pulls the next route from the routing strategy in context properties (`TryGetNextRoute`) and rewrites only the authority on the cloned request's URI — path and query preserved.
- The generator returns null when routes are exhausted: route exhaustion, not attempt count alone, stops hedging.
- The `ActionGenerator` slot is handler-owned: the handler installs its snapshot-and-routing generator through options post-configuration, which runs after user configuration.
- A user-assigned generator on the standard hedging options is therefore overwritten — custom action generation requires a custom handler declaration.
- The standard generator throws `InvalidOperationException` when the snapshot is absent from context properties — removing or reordering the snapshot strategy breaks the generator.
- The two standard-hedging pipelines are one organism; partial replacement of either is rejected by these structural couplings.
- The routing strategy runs as the outermost strategy and resolves the first route before anything else, throwing if the route table yields no first route.
- The primary attempt is routed exactly like hedged ones — route configuration governs all attempts uniformly, and an unrouted primary is impossible by construction.
- Routing options are two table shapes bound per client name, validate-on-start: `ConfigureOrderedGroups` and `ConfigureWeightedGroups`.
- Ordered groups walk by index in declaration order — pure failover; weighted groups draw by group weight — load-spread with failover.
- `WeightedGroupSelectionMode.EveryAttempt` (default) draws every group by weight; `InitialAttempt` draws only the first group by weight and walks the remaining groups in declaration order — load-spread versus primary-with-failover from one enum value.
- A tried group is removed from the candidate pool, so no group is offered twice within one logical call and the route supply is finite by construction.
- Both table shapes share the weighted endpoint draw: groups are lists of weighted endpoints in ordered and weighted tables alike — weight always operates at the endpoint axis; the table shape only changes group progression.
- Routing strategy instances are pooled — disposal returns them to the pool.
- The per-call routing instance initializes from an options-monitor-backed cache that live-updates on change: route table edits apply to new calls without any pipeline rebuild.
- The public routing surface is the two group tables; a custom route source is expressed through a custom action generator on a custom handler, not through strategy injection.
- Hedging multiplies in-flight work: per-endpoint rate limiters under the hedging strategy charge each attempt against its endpoint's admission.
- Losers cancel when a winner lands — concurrency cost is bounded per endpoint, not per logical call.

## divergent

- standard-handler-internals — replacement is per-slot, not per-handler: each of the five standard strategies is one property on the options record, so hardening a seam edits rows while preserving the validated chain shape.
- standard-handler-internals — the slot-edit vocabulary covers the real hardening moves: swap the retry predicate, retune the breaker, point the limiter at an externally shared instance — none of them re-opens the chain.
- standard-handler-internals — the chain order itself is not an options value: a seam needing a different order is a custom handler declaration, and the startup validators are the boundary conditions any replacement must re-satisfy (attempt ≤ total, sampling ≥ 2× attempt).
- standard-handler-internals — the two-owner trap is structural and scan-detectable: a client carrying both a standard handler and a custom resilience handler, or a typed client whose inner SDK retries beneath the handler chain, stacks attempt loops multiplicatively — 3 × 3 = 9 sends per logical call.
- standard-handler-internals — the additional-handlers list is inspectable for multiple resilience handlers, and the repair shape is `RemoveAllResilienceHandlers` followed by one declaration: at most one resilience handler per client seam.
- standard-handler-internals — reload semantics at the seam: `EnableReloads` accumulates monitor-driven reload tokens per pipeline generation, so an options change rebuilds the seam pipeline in place while in-flight requests finish on the old generation.
- standard-handler-internals — authority-keyed instances rebuild independently because reload tokens attach at materialization: one options write, N isolated rebuilds, zero dropped requests.
- hedging-idempotency — hedging is concurrent duplication, strictly stronger than retry's sequential duplication: overlapping attempts can both commit on the server, so hedging admission is a property of the operation, not of the failure.
- hedging-idempotency — the seam's row declares idempotency, and the hedging handler is admitted only on idempotent rows; failure-driven hedging onto a non-idempotent operation is a double-commit generator.
- hedging-idempotency — retry has a per-method filter slot but hedging does not: for mixed-method clients the method filter handles retry admission, while hedging admission can only be decided at the seam.
- hedging-idempotency — non-idempotent traffic routes through a separate non-hedged client registration, which is why hedged and unhedged seams split at client registration rather than inside one pipeline.
- hedging-idempotency — replayability is the second admission gate: snapshot rejection of stream content means a hedged seam must produce buffered, re-sendable bodies; a seam that streams uploads is structurally non-hedgeable regardless of semantic idempotency.
- hedging-idempotency — the typed seam row therefore carries two independent columns — semantic idempotency and body replayability — and hedging requires both; either column alone admits only retry.
- hedging-idempotency — endpoint-state asymmetry is the payoff: because an open breaker is hedging-transient, a regional outage converts to one fast-failed attempt plus immediate re-route while the per-endpoint breaker keeps absorbing probes.
- hedging-idempotency — tail-latency hedging (small positive delay) and failover hedging (delay tuned near the attempt deadline) are the same declaration at different delay values, with zero delay reserved for read-only racing where double execution is free.
