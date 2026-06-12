# reactive-streams — bedrock

## admission law

- An observable pipeline earns admission only where time or combination operators change the design.
- The admitting operator classes: coalescing (`Throttle`, `Sample`, `Buffer`, `Window`); combination (`CombineLatest`, `WithLatestFrom`, `Switch`, `Merge`); shared lifetime (`Publish`/`RefCount`/`AutoConnect`); event-shaped sources (`FromEventPattern`).
- A chain of only `Select`/`Where` belongs on typed rails or lanes — projection alone never admits a push stream.
- The reason is structural: a projection-only stream buys the push model's hazards — unbounded delivery, exception-as-termination — and uses none of its algebra.
- The depth threshold is concrete: two or more time/combination operators, or one multicast lifetime, or time logic that must run under a virtualized clock.
- Below the threshold the stream is surface sprawl; above it, one declared chain replaces an event-handler mesh — handlers, re-entrancy flags, and hand-rolled debounce timers collapse into operators.
- The grammar is law: `OnNext* (OnError | OnCompleted)?`, serialized.
- `Subject<T>.OnNext` is unsynchronized — concurrent producers onto one subject violate the grammar and corrupt downstream operator state silently.
- The declared serialization point is `Synchronize()`, or `Synchronize(gate)` to share an external lock.
- Locking inside individual observers is the rejected form — it serializes one observer while leaving the grammar violated for the rest.
- Source construction is total over async shapes: `Observable.Create` spans `Func<IObserver<T>, IDisposable>`, `Func<IObserver<T>, Action>`, and async forms taking a `CancellationToken` and returning `Task`, `Task<IDisposable>`, or `Task<Action>`.
- The async `Create` forms mean async setup with structured teardown needs no manual bridging — the returned disposable or action is the teardown, the token is the abandon signal.
- `FromAsync` spans plain and token-accepting factories plus `TaskObservationOptions` overloads.
- `Defer` makes per-subscription construction declarative; `Return`, `Empty`, `Never` are the algebra's unit rows.
- Cold is the default: every subscription re-runs the factory's side effects — a side-effecting cold source fans out only behind a multicast row.
- `Switch` spans both `IObservable<IObservable<T>>` and `IObservable<Task<T>>` — latest-wins over inner streams or inner tasks with one operator.
- `Switch` unsubscribes superseded inners — the structural cancellation of stale async work, with no token plumbing.
- `RetryWhen(signalSelector)` exposes the failure stream itself as a composition surface — backoff, gating, and give-up become stream declarations.
- `TakeUntil` spans companion observables, value predicates, absolute time (`DateTimeOffset`, with scheduler), and `CancellationToken` directly — four lifetime-end vocabularies on one operator.
- `Catch` spans typed handlers (`Catch<TSource, TException>(handler)`), a second-stream fallback, and `params`/enumerable cascades — fallback chains are data, not nested try shapes.
- `FromEventPattern` converts an add/remove handler pair into a stream where subscription is the attach and disposal is the detach — the event-leak class is deleted structurally, and `EventPattern<TEventArgs>` carries sender and args as one value.
- `Interval` and `Timer` are cold per-subscriber clocks — every subscriber gets its own timer; shared ticks are a multicast row over one timer, and N raw subscriptions to one `Interval` are N timers, the quiet resource multiplication.
- Chains are constructed at composition time and subscribed at edges: a `Subscribe` in the middle of a pipeline severs the declarative chain into imperative halves — the operator that needed the value mid-stream is `Do`, not a subscription.
- Flattening is a three-row concurrency decision: `Concat` runs inners sequentially in order; `Merge` runs all inners unbounded; `Merge(maxConcurrent)` runs inners under a declared bound — the stream-side consumer of the process concurrency budget, and the row that deletes semaphore-wrapped subscription code.
- `GroupBy(keySelector, capacity, comparer)` demultiplexes one stream into keyed inner streams — the per-key pipeline primitive; the capacity overloads pre-size the key table for known cardinalities.
- `StartWith(params values)` seeds a stream; the load-bearing application: `CombineLatest` emits nothing until every source has emitted once, so joins over sometimes-quiet sources seed each leg with `StartWith` — the silent-dead-join defect and its one-line repair.
- `Do` is the tap row — receipts and counters ride `Do` without entering the value path; `Finally` is the termination hook regardless of exit class; `Using` scopes a resource to a subscription lifetime — acquisition at subscribe, disposal at terminate.

## scheduler law

- Defaults are structural facts: scheduler-less time-based overloads run on the platform default scheduler — timer-backed, pool-delivered.
- Iteration-shaped generators default to the current-thread scheduler; constant-time operations default to immediate execution.
- The law: every time-based operator names its `IScheduler` argument explicitly.
- The implicit default is rejected not for behavior but because it forecloses virtual-time substitution and hides the delivery thread from the declaration.
- Marshal-exactly-once: at most one `ObserveOn` per chain, placed at the consumption edge.
- `ObserveOn` affects only downstream notification delivery — operators above it run wherever their sources deliver.
- At most one `SubscribeOn` per chain; it affects only subscription and disposal side effects, and its position in the chain is irrelevant to where `OnNext` runs — the trap behind most misplaced-marshal defects.
- `SubscribeOn`'s one legitimate use is expensive attachment: a source whose subscribe path performs slow work moves that work off the caller's thread; using it to influence delivery is the misuse the placement law deletes.
- `ObserveOn` and `SubscribeOn` each accept either an `IScheduler` or a `SynchronizationContext`; the context overloads serve context-bound edges.
- The host-thread marshaling law itself is settled elsewhere; this lane owns operator placement only.
- The scheduler is the clock plane: `Sample`, `Throttle`, `Buffer`, `Timer`, `Interval`, `Timestamp`, `TimeInterval` all take the scheduler.
- A stream whose time logic rides injected schedulers is virtualizable end-to-end.
- One ambient wall-clock read inside an operator lambda breaks the plane; time evidence is stamped via `Timestamp(scheduler)`, never read ambiently.
- The virtual-time scheduler family substitutes through the same `IScheduler` seam — the delivery-policy table below doubles as the proof that all time logic is testable under virtual clocks.
- The immediate and current-thread schedulers differ on recursion, not speed: immediate executes scheduled work inline — recursive scheduling on it livelocks or overflows; current-thread trampolines recursion through a queue, which is why iteration-shaped operators default to it and why the immediate scheduler is never passed to time-based or recursive operators.
- `EventLoopScheduler` is the stream world's dedicated serial lane — one named thread, FIFO delivery.
- It is disposable and is disposed at scope end; it is the structural answer where consumers require affinity without a context.
- `ObserveOn` preserves per-observer order: notifications queue and deliver serially on the target scheduler — marshaling never reorders one observer's stream, so ordering defects after a hop indict the merge topology, not the hop.
- `Timestamp(scheduler)` and `TimeInterval(scheduler)` stamp absolute instants and inter-arrival gaps as values — latency and cadence measurement ride the value path under the virtualizable clock, never a side-channel stopwatch.

## cadence-gated delivery: declared coalescing

- Every delivery edge to a cadence-bound or slow observer declares its coalescing row.
- Raw push to such an observer is the rejected form — the edge without a declared row is the defect, not the slow observer.
- Row: conflate-to-latest = `Sample(period, scheduler)` — fixed-cadence emission of the latest value.
- Row: quiet-gap commit = `Throttle(dueTime, scheduler)` — emission only after the input goes quiet for the window.
- Row: bounded batch = `Buffer(timeSpan, count, scheduler)` — the composite overload emits on whichever bound trips first, covering burst and trickle with one declaration.
- Row: segmented analysis = `Window` — streams of streams with no materialized lists; `Buffer(count, skip)` and `Window(count, skip)` produce sliding or hopping frames by the skip/count relation; `Buffer(timeSpan, timeShift, scheduler)` is the time-shaped sliding form, and `Window(timeSpan, scheduler)` the time-shaped segment form.
- The Buffer/Window memory law: `Buffer` materializes a list per frame, `Window` streams elements as they arrive — large or unbounded frames take `Window` with an in-window fold, never a buffered list.
- `Throttle` is not a rate limiter: a producer steadier than the gap starves the output permanently; the steady-cadence row is `Sample` — choosing by name instead of by this distinction is the classic silent-starvation defect.
- Both coalescing rows have stream-driven generalizations: `Sample(sampler)` takes its cadence from another observable — any pulse source becomes the clock without scheduler coupling; `Throttle(item => IObservable<TThrottle>)` derives a per-item quiet window from the value itself — dynamic significance-weighted debounce as a selector, not a constant.
- Keyed coalescing composes from the demultiplexer: `GroupBy` then a per-group coalescing row, merged.
- The composition yields per-key conflation with one declaration per axis, replacing per-key timer dictionaries.
- `Scan` inside a window or group is the streaming fold — aggregation without the materialized frame the buffered forms would allocate.
- Loss accounting: `Sample` and `Throttle` drop intermediates silently.
- Where loss must be receipted, the spelling is the lossless `Buffer` folded to (latest value, dropped count) — conflation with evidence.
- Bare `Sample` is admissible only where intermediates are semantically void — continuous signals whose only meaningful value is current.
- This is the stream-side instance of the every-loss-receipted law; the hand-off-side instance lives with the lanes.
- `DistinctUntilChanged(keySelector, comparer)` sits immediately before the delivery edge — after coalescing, so cadence and significance compose: first conflate time, then suppress non-changes.
- `Scan(seed, accumulator)` threads running state inside the chain — the in-stream fold that replaces mutable accumulator fields beside a subscription.

## monotonic cell with CAS rank guard

- Multi-hop delivery — scheduler hops, merged sources — can reorder arrivals; published state must be rank-guarded.
- Every update carries a rank drawn from the source's own monotone — a version, a stamp, a sequence minted at origin.
- The cell admits an update through a compare-and-set loop only when its rank exceeds the cell's current rank.
- Stale arrivals fold to receipted skips — not silent overwrites, not exceptions; the skip receipt is the reorder gauge.
- Rank derives from the source's monotone, never from arrival order: arrival order is exactly what the hops destroyed.
- Where a chain is single-source, the rank threads inside the stream via `Scan` and the cell becomes a plain write.
- The CAS guard earns admission only at genuine multi-writer convergence points — guarding a single-writer cell is ceremony.
- This is the structural alternative to locking around `OnNext`: ordering by evidence instead of exclusion — the guard admits all writers concurrently and lets the rank algebra, not a mutex, decide precedence.
- Cell-primitive mechanics are settled rail law; this lane owns the rank-guard protocol and its skip receipt.

## activation edges: reference-counted scope

- `Publish().RefCount()` is the universal reference-counted activation edge: the zero-to-one subscriber transition fires connection, and the symmetric last-dispose fires disconnection.
- `RefCount(minObservers)` moves activation to the Nth subscriber — a quorum edge: expensive shared work does not start until demand is proven.
- `RefCount(disconnectDelay)` and `RefCount(disconnectDelay, scheduler)` linger the deactivation: re-subscription inside the window reuses the live connection.
- The linger window deletes teardown/setup churn on rapid detach/attach cycles — the disconnect cost is paid once per genuine idle period, not once per flicker.
- `RefCount(minObservers, disconnectDelay, scheduler)` composes both axes — quorum activation with lingering deactivation in one declaration.
- The activation edge is a universal scope primitive: any resource whose lifetime should equal net-consumer-count-above-zero rides this edge rather than a hand-rolled counter.
- `AutoConnect(minObservers = 1, onConnect)` is activation without symmetric deactivation: the connection survives subscriber count reaching zero.
- The connection handle escapes through `onConnect` for owner-controlled teardown — the scope, not the subscribers, ends it.
- Chooser: lifetime owned by consumers → the `RefCount` rows; lifetime owned by a scope → `AutoConnect` with the handle stored in the scope's disposal set.
- `Replay(bufferSize)` + `RefCount` composes the activation edge with bounded catch-up: late subscribers inside the active window receive the buffered tail.
- Unbounded replay is the rejected row — a memory leak declared as policy.
- `ReplaySubject` bounds by count, by time window, or both at once (`bufferSize, window, scheduler`) when the subject form is required.
- Subjects are state cells with declared semantics, three rows: `BehaviorSubject` = current value + replay-1, the observable property cell.
- `AsyncSubject` = terminal value only, the future cell — subscribers before completion wait, subscribers after receive immediately.
- `ReplaySubject` = bounded history — the catch-up cell.
- The relay subject — manually subscribing a `Subject` to a cold source to share it — is the rejected spelling of `Publish`: it loses disposal symmetry, replays nothing deterministically, and detaches error propagation from subscriber lifetime.
- A terminated multicast is sticky: once the upstream completes or errors, the subject inside `Publish()` is dead, and every later subscriber through `RefCount` receives the terminal event immediately — reconnection cannot revive it.
- Restart is therefore structural, not operator-level: a restartable shared source is a factory producing the `Publish().RefCount()` chain plus a current-instance cell swapped on restart.
- Wrapping the multicast in `Defer` is the tempting wrong fix — a deferred multicast constructs per subscriber and shares nothing.
- Whether stickiness is the intent is the design decision: terminal-once semantics (a completed bootstrap, a one-shot computation) want the sticky multicast as-is; long-lived feeds want the factory-and-cell restart shape.

## rx-rail bridges

- Error conversion happens at exactly one consumption boundary: `OnError` carries one exception and terminates the stream, so behind the boundary the rail's typed failure owns the outcome.
- `Materialize()` lifts the grammar into `Notification<T>` values for a total fold into typed outcomes; `Dematerialize()` is the inverse for re-entering operator space.
- `Catch` rows declare typed in-stream recovery; `Retry(count)` and `RetryWhen(signals)` are admissible only before the boundary — behind it, the rail's schedule policy is the one retry owner, and a stream-side retry stacked under a rail-side retry is the double-loop defect.
- Stream-to-lane crossing: observers never await, so the writer side of a bridge is a non-waiting write into a drop-mode lane row with receipts.
- A wait-mode lane behind an observer is structurally wrong — `OnNext` has no park position.
- The drop receipts are the bridge's loss evidence; a bridge without them is the unreceipted-loss defect at the seam.
- Lane-to-stream crossing: the async `Create` overload consumes the lane's async read stream, completing on lane completion, with the factory's token cancelling the read loop.
- Disposal symmetry at that seam: disposing the subscription IS cancelling the pull — no orphaned read loop survives an unsubscribed consumer.
- Disposal algebra as policy rows: `CompositeDisposable` per scope for bulk teardown.
- `SerialDisposable` is replace-on-reconfigure — assignment disposes the predecessor: the one-active-subscription cell.
- `SingleAssignmentDisposable` is set-once wiring; `RefCountDisposable` is shared teardown across owners; `CancellationDisposable` bridges token-world into disposal-world.
- Where a lifetime signal exists, `TakeUntil(signal)` is the declarative scope end and is preferred over imperative disposal — the chain states its own lifetime.
- `TakeUntil(CancellationToken)` makes a token the lifetime signal directly — no disposable bookkeeping at all for token-scoped chains.
- The block-family bridge is admitted at the lane page; the stream-side law here: a bridged block is a hot source — multicast it behind a `RefCount` row before fan-out, or every subscriber contends for the block's single output stream.
- Deadline escalation has a stream-altitude spelling: `TakeUntil(softDeadline, scheduler)` ends the stream cleanly at the cooperative deadline (consumers observe `OnCompleted` and flush), and scope disposal at the hard deadline is the forced phase — the generic two-phase pair instantiated as one operator plus one disposal, with both deadlines drawn from the drain budget.
- `Observer.Create` builds grammar-enforcing observers for hand-rolled sinks; `ObservableBase<T>` is the seam for authoring custom sources that inherit the auto-detach contract — both exist so bespoke endpoints never re-implement the grammar by hand.

## divergent

- Admission-table closure with deleted forms, each named: event-handler chains → `FromEventPattern` plus a declared chain; hand-rolled debounce and throttle timers → the coalescing rows; boolean re-entrancy flags → serialized grammar plus `Switch`; polling loops over mutable state → `BehaviorSubject` plus `DistinctUntilChanged`; callback-pyramid async setup → async `Create`.
- The chain is shorter than the mesh it replaces by roughly the operator count: one `CombineLatest` + `DistinctUntilChanged` + `Throttle` chain owns what a mesh spreads across handlers, flags, and timer fields.
- Delivery-policy table: streams join the process's declared-policy posture by naming, per edge, the triple { edge, scheduler, coalescing row } in one table.
- The table makes delivery cadence a policy value beside the lane table and the budget record; per-operator ad-hoc scheduler choice becomes the deleted form.
- The same table is the virtualization manifest, since every named scheduler is substitutable.
- Three-world crossing law: rails, lanes, and streams each cross at one canonical seam with one evidence carrier.
- The four crossings: rail→stream (`FromAsync`/`Defer` over the effect); stream→rail (the boundary fold over `Materialize`/`Catch`); stream→lane (non-waiting write plus drop receipt); lane→stream (async `Create` over the read stream).
- The rejected topology is the double bridge — stream→lane→stream inside one process hop — which launders backpressure evidence through a buffer and re-enters push-world having destroyed the ordering the lane provided.
- Quorum-and-linger as a lifecycle vocabulary: `minObservers` and `disconnectDelay` generalize beyond connections — any expensive shared computation behind an activation edge inherits quorum activation (do not start until N consumers prove demand) and linger deactivation (survive transient zero-crossings).
- The two knobs are the closed vocabulary for demand-driven lifetime; hand-rolled subscriber counting with timers is the form they foreclose.
- Sampling-direction chooser: `WithLatestFrom` versus `CombineLatest` is a causality decision, not a preference.
- `WithLatestFrom` fires only on the driver and samples the companion — the companion can never cause emission: configuration-at-event-time.
- `CombineLatest` fires on either source — true joint state, with re-fire on every leg.
- Choosing by which side may cause downstream effects eliminates the spurious-refire defect class; the whole-set re-resolution such root observations drive belongs to the change-set lane.
- Notification values as data: `Notification<T>`, `EventPattern<TEventArgs>`, `Timestamped<T>`, and `TimeInterval<T>` make grammar events, event-source pairs, and time evidence first-class values — folds over stream behavior (loss counting, latency measurement, terminal classification) operate on these carriers instead of on side-channel state, which is what keeps stream observability inside the expression spine.
- Terminal classification through `Materialize` lands in the same closed verdict family the budget primitives emit — completed, failed-with-evidence, canceled.
- One verdict vocabulary at the boundary means stream endings, lease failures, and lane refusals dispatch through one shape instead of three subsystem-specific ones.
- The full cadence-gated delivery law is a three-row composition, stated once: a declared coalescing row shapes time; a rank-guarded cell shapes concurrent publication; a significance gate (`DistinctUntilChanged`) shapes value noise.
- Every observer edge in a process composes some subset of exactly those three rows; an edge that hand-rolls any of them re-derives a row that already exists.
