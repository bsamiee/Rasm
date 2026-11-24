# [ASYNC]

## [01]-[CANCELLATION]

`select!`, `timeout`, and `run_until_cancelled` drop each losing future, and effects it finished and tasks it spawned remain:
- `mpsc::Receiver::recv`, `watch::Receiver::changed`, `TcpListener::accept`, `StreamExt::next`, and `AsyncReadExt::read` lose nothing on drop
- `Lines::next_line` keeps a partial line in its own buffer and loses nothing on drop
- `read_exact`, `read_line`, and `write_all` lose partial progress on drop, and a loop racing one pins it outside the loop and polls `as_mut()`
- `mpsc::Sender::send` loses its message on drop, and a raced send awaits `reserve()` and sends through the `Permit` it returns
- `Mutex::lock`, `Semaphore::acquire`, `Notify::notified`, and `Sender::reserve` lose their queue position on drop

## [02]-[TASKS]

Services hold a `TaskTracker` for each task they start over their lifetime, and a `CancellationToken` stops the loop that starts them:

```rust
pub async fn serve<H>(listener: TcpListener, handler: Arc<H>, stop: CancellationToken) -> Result<(), ServiceError>
where H: Handler + Send + Sync + 'static {
    let tracker = TaskTracker::new();
    let connections = TcpListenerStream::new(listener).filter_map(async |accepted| match accepted {
        Err(error) if matches!(error.kind(), ErrorKind::ConnectionAborted | ErrorKind::ConnectionReset) => None,
        accepted => Some(accepted),
    });
    let accepted = connections
        .take_until(stop.cancelled())
        .try_for_each(async |stream| {
            let handler = Arc::clone(&handler);
            tracker.spawn(async move { handler.handle(stream).await });
            Ok(())
        })
        .await;
    tracker.close();
    tracker.wait().await;
    accepted.map_err(ServiceError::Accept)
}
```

Accepting ends when `stop` fires or an accept fails, and `close` with `wait` lets each accepted connection finish before `serve` returns:
- `filter_map` skips a connection its peer reset before `accept` returned it, and any other accept error ends `serve`
- `handle` returns `()` and records its connection's outcome itself
- `axum::serve(..).with_graceful_shutdown(stop.cancelled_owned())` stops accepting on the token and waits for open connections
- `drop_guard` (`drop_guard_ref` while a scope reads the token) cancels a scope's token and each child on every exit path
- `child_token` hands a consumer a token that cancels with its parent and never cancels the parent
- `JoinQueue` joins in spawn order and `JoinSet` in completion order, each spawns the futures it collects, and a drop aborts every task
- `join_all` resumes a task's panic, and `join_next` and `JoinHandle` results pass `JoinError::try_into_panic` to `panic::resume_unwind`
- Tasks many holders share go in an `Arc<AbortOnDropHandle<T>>`, and the last holder's drop aborts the task
- Blocking threads read a stream as an iterator through `executor::block_on_stream`

## [03]-[SHUTDOWN]

Programs drain on the first signal and abandon the drain on a second signal or at a deadline:

```rust
pub async fn drained<F>(serving: F, stop: &CancellationToken, mut signals: impl Stream + Unpin, grace: Duration) -> Option<F::Output>
where F: Future {
    let mut serving = pin!(future::maybe_done(serving));
    tokio::select! {
        _ = signals.next() => stop.cancel(),
        () = serving.as_mut() => {},
    }
    tokio::select! {
        biased;
        () = serving.as_mut() => {},
        _ = signals.next() => {},
        () = time::sleep(grace) => {},
    }
    serving.as_mut().take_output()
}
```

`drained(serve(listener, handler, stop.child_token()), &stop, signals, grace)` returns the serving output, or `None` once the drain is abandoned:
- `maybe_done` keeps the serving output across both races, and `take_output` reads it once
- Serving is a branch of both races, and a future no `select!` polls makes no progress (an accept loop accepts nothing)
- `biased;` polls serving first, and a drain finishing at the instant of a second signal or the deadline wins

## [04]-[FAN_OUT]

`removed` runs every removal to its outcome under the caller's bound and returns each failure:

```rust
pub async fn removed<S>(store: &S, keys: Vec<Key>, bound: NonZero<usize>) -> Result<(), NEVec<StoreError>>
where S: Store {
    let outcomes = stream::iter(keys).map(|key| store.remove(key)).buffer_unordered(bound.get());
    let failed = outcomes.filter_map(async |outcome| outcome.err()).collect::<Vec<_>>().await;
    NEVec::try_from_vec(failed).map_or(Ok(()), Err)
}
```

`Store::remove` deletes one key, and `NonZero` keeps out the bound that stalls or panics:
- `buffered(0)` and `buffer_unordered(0)` pull no future and never end, and `ready_chunks(0)` panics
- Phases acting on what an earlier phase left run one fan-out each, `?` ending the run after the first phase holding a failure

## [05]-[STREAMS]

Values arriving over time are a `Stream` with each failure one item, `tokio_stream::wrappers` adapting tokio's receivers, listeners, and intervals:

```rust
pub fn sample<S>(sensor: &S, ticks: Interval) -> impl Stream<Item = Result<Reading, SensorError>>
where S: Sensor {
    IntervalStream::new(ticks).then(move |_| sensor.read())
}
```

`sample` consumes an `Interval` holding its period and missed-tick policy, and each tick yields a reading or an `Err` item while the stream continues:
- `interval` ticks at once, then each period, and `set_missed_tick_behavior` replaces its default `Burst` of missed ticks with `Delay` or `Skip`
- `then` runs one read at a time
- Consumers stop at the first `Err` through `try_for_each`, or record each failure and continue through `for_each` over a `match`
- `ready_chunks(limit.get())` batches waiting items into one write or one commit per batch
- Sources of different item types merge in arrival order through `stream::select` into `Either` for a pair, `stream_select!` over an enum for more
- `stream_select!` polls its sources alike, where a nested `select` gives its outer source half the polls
- `chain(stream::iter([<Enum>::End]))` marks where one merged source ends
- Streams a program starts over time arrive on a channel and merge through `flatten_unordered(None)`
- Sources with a consumer each run as one `future::join` of their `for_each` calls
- `WatchStream::new` yields the held value and each change, `from_changes` the changes alone
- `watch` channels empty until their first value hold `Option<T>`
- Streams ending only with their sender (`WatchStream`, `BroadcastStream`) end beside other work through `take_until(stop.cancelled())`

Paged sources unfold into one stream of items:

```rust
pub fn listed<C>(catalog: &C) -> impl Stream<Item = Result<Item, SourceError>>
where C: Catalog {
    stream::try_unfold(Some(Cursor::default()), async |cursor| match cursor {
        Some(cursor) => catalog.page(cursor).await.map(|Page { items, next }| Some((stream::iter(items).map(Ok), next))),
        None => Ok(None),
    })
    .try_flatten()
}
```

`page` returns its items beside the `next` cursor, `None` after the last page:
- `try_unfold` fetches a page only when the consumer polls past the previous one, and `take(n)` stops paging
- Failed pages end the stream after their `Err`

## [06]-[SHARED_STATE]

Shared state takes the construct its access pattern names, and values changing together share one construct holding value and notification:

| [INDEX] | [ACCESS]                                      | [CONSTRUCT]                                     |
| :-----: | :-------------------------------------------- | :---------------------------------------------- |
|  [01]   | Read-mostly snapshot replaced whole           | `ArcSwap<T>`                                    |
|  [02]   | Latest value observed by many tasks           | `watch` channel                                 |
|  [03]   | Every event delivered to many tasks           | `broadcast` channel                             |
|  [04]   | Commands serialized against one owner         | Task owning the state, `mpsc` in, `oneshot` out |
|  [05]   | Short synchronous edits from any thread       | `std::sync::nonpoison::Mutex`                   |
|  [06]   | Concurrent keyed reads and writes             | `papaya::HashMap`                               |
|  [07]   | Keyed results with expiry and coalesced loads | `moka::future::Cache`                           |
|  [08]   | Reusable connections and handles              | `deadpool` pool                                 |

Owning tasks fold the request stream through a pure transition and answer each request on its `oneshot` sender:

```rust
pub async fn owned<S, C, R>(initial: S, transition: impl Fn(S, C) -> (S, R), requests: mpsc::Receiver<(C, oneshot::Sender<R>)>) -> S {
    ReceiverStream::new(requests)
        .fold(initial, async |state, (command, reply)| {
            let (next, answer) = transition(state, command);
            drop(reply.send(answer));
            next
        })
        .await
}
```

Transitions return the next state beside the reply, and a refused command keeps its state and answers with the refusal:
- State is the fold's accumulator, no lock guards it, and requests apply in arrival order
- Requesters send a command with the sender half of a fresh `oneshot::channel` and await the receiver
- Dropped receivers leave their command applied, and the fold returns the final state once every sender drops

Other constructs go through their own API:
- `ArcSwap::load` borrows a snapshot for a read with no `.await`, `load_full` returns an `Arc` held across one, and `store` replaces the whole value
- `ArcSwap::rcu` reruns its closure whenever another writer stores first, and the closure computes the next value alone
- `send_modify` edits a `watch` value in place, `send_if_modified` notifies only on a `true` from its closure, and `send_replace` sets a new value
- `watch::Sender::send` discards its value while no receiver exists, and `send_replace` or `send_modify` keeps it for a later `subscribe`
- `broadcast::Receiver::recv` returns `RecvError::Lagged(n)` once `n` values passed unread, and a consumer needing each value resyncs from the owner
- `nonpoison::Mutex` (`sync_nonpoison`, `nonpoison_mutex`) state goes through `with_mut`, `get_cloned`, `set`, or `replace` (`lock_value_accessors`)
- `papaya::HashMap::pin` returns a guard that drops before the next `.await`, `pin_owned` a `Send` guard held across it
- `moka::future::Cache::try_get_with(key, load)` runs one load for concurrent callers of a missing key and caches only an `Ok`
- `try_get_with` returns `Result<V, Arc<E>>` with one failure every waiter shares, and a crate error variant holds the `Arc<E>` as its source
- `get_with_by_ref` and `try_get_with_by_ref` take a borrowed key and build the owned key on insertion alone
- Cache keys hold every input the cached value depends on, and a value `get` clones on each hit is an `Arc<T>` when large

## [07]-[TELEMETRY]

Values known after span entry fill a field the span declared empty:

```rust
#[tracing::instrument(skip_all, fields({ ACCOUNT } = %command.account, { BALANCE } = field::Empty))]
pub async fn deposited(deposit: &impl AsyncFn(Deposit) -> Result<Account, LedgerError>, command: Deposit) -> Result<Account, LedgerError> {
    let account = deposit(command).await?;
    Span::current().record(BALANCE, field::display(account.balance));
    Ok(account)
}
```

Spans, events, metrics, and log fields read crate key constants (`ACCOUNT`, `BALANCE`):
- `opentelemetry_semantic_conventions::attribute` supplies each key it names (`{ SERVER_ADDRESS } = %address.ip()`)
- Keys the conventions lack or deprecate are crate constants beside the values a convention fixes (an operation name, bucket boundaries)
- Spans ending in failure take `OpenTelemetrySpanExt::set_status(Status::error(..))` and `ERROR_TYPE` from the error's classifier (`is_timeout`)
- Failures a program continues past record `%Report::new(error)` under `EXCEPTION_MESSAGE` through `unwrap_or_else(|error| tracing::debug!(..))`
- Trace context crosses a wire through a propagator over the carrier itself (`HeaderExtractor(&headers)`, an `Injector` over a JSON object)
- Forwarded requests inject the current `Context` into the outgoing carrier, never the inbound one
- Spans take a parent only where their carrier holds one
- `skip_all` keeps arguments out of a span unless a field names them
- Spawned tasks start outside the caller's span, `in_current_span()` entering it and `instrument(span)` giving the task its own
- Pure functions emit nothing, and the boundary calling them records what they returned

## [08]-[POLICIES]

Applications compose each failure policy around a library's own error, retry running through `backon` with its schedule a parameter:

```rust
pub async fn fetched<S>(source: &S, id: ItemId, schedule: impl BackoffBuilder) -> Result<Item, SourceError>
where S: Source {
    let attempts = (|| source.fetch(id)).retry(schedule).when(SourceError::is_transient);
    attempts.adjust(|error, planned| planned.map(|delay| error.retry_after.unwrap_or(delay))).await
}
```

`when` names the failures that repeat, `retry_after` holds a server's `Retry-After` delay, and `adjust` replaces only a planned delay:
- `adjust` returning `Some` where the schedule gave `None` retries past its end
- `ExponentialBuilder::default()` waits one second, doubles up to a 60-second cap, and stops after 3 retries
- `with_jitter`, `with_max_times`, `with_max_delay`, and `with_total_delay` reshape the schedule, and `with_jitter_seed` fixes a test's jitter
- `ExponentialBuilder` multiplies each delay as `f32` seconds, and a first delay `f32` cannot hold exactly (100 ms) drifts by nanoseconds each step
- `retry` takes any `Duration` iterator as schedule, and exact delays a test or protocol fixes step by `iter::successors` over `checked_mul`
- Successes and failures `when` refuses return at once, and the last failure returns once the schedule ends
- `notify` records each retried failure with its delay, waits run on the runtime clock, and cancellation drops each wait
- `TryFutureExt::or_else` runs a lower-priority fallback after the preferred effect fails, each policy reading only the result it wraps
- Recovery maps a final failure to a value at the entry point
- `timeout(limit, future)` returns `Err(Elapsed)` once the limit passes, and one `timeout` around a composed future bounds every step inside it
- `timeout_at(deadline, future)` takes an `Instant`, and steps in separate calls sharing one budget receive one deadline
- Served routes take the same policies as `tower` layers on one `ServiceBuilder` (`load_shed`, `concurrency_limit`, `timeout`), outermost first
