# [ASYNC]

## [01]-[FUTURES]

Dropping a future cancels its work at the `.await` it waits on. Values held across an `.await` decide a future's `Send`, and a non-`Send` value (an `Rc`, a lock guard) sits in a block ending before it. Racing and bounding combinators (`select!`, `timeout`, `run_until_cancelled`, `take_until`) drop the losing future. Raced operations keep their progress outside it:
- Cancel-safe, lossless on drop: `mpsc::Receiver::recv`, `watch::Receiver::changed`, `TcpListener::accept`, `StreamExt::next`, `AsyncReadExt::read`
- `read_exact` and `write_all` lose partial progress, a `LinesStream` keeps its partial line in its own buffer
- `tokio::sync::Mutex::lock`, `Semaphore::acquire`, and `Notify::notified` lose their queue position when dropped

## [02]-[TASKS]

`JoinQueue` collection spawns each future on runtime threads, `join_all` awaits every output, and dropping the queue aborts running tasks:

```rust
pub async fn upload_all<S>(store: Arc<S>, parts: impl IntoIterator<Item = Part>) -> Result<Vec<Receipt>, NEVec<UploadError>>
where S: Bucket<put(..): Send> + Send + Sync + 'static {
    iter::repeat(store).zip(parts).map(async |(bucket, part)| bucket.put(part).await).collect::<JoinQueue<_>>().join_all().await.into_iter().collect::<Validated<Vec<_>, _>>().ok()
}
```

`iter::repeat` clones the `Arc` per part, and each future owns its inputs as `'static`. `join_all` resumes a task panic in the caller. Services hold a `TaskTracker` for tasks they start over their lifetime, and a `CancellationToken` stops the loop that starts them:

```rust
pub async fn serve<H>(listener: TcpListener, handler: Arc<H>, stop: CancellationToken) -> Result<(), ServiceError>
where H: Handler<handle(..): Send> + Send + Sync + 'static {
    let tracker = TaskTracker::new();
    let accepted = TcpListenerStream::new(listener)
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

Accepting ends when `stop` fires or an accept fails. `close` with `wait` runs on both paths, and every accepted connection finishes before `serve` returns. `handle` returns `()` and records its connection's outcome itself. A token one scope owns becomes a `DropGuard` through `drop_guard`, and the drop cancels it on every exit path. Subscribers to one task each hold an `Arc<AbortOnDropHandle<()>>`, a map of its `Weak` hands it to later subscribers, and the last drop aborts it. Programs drain on the first signal and abandon the drain on the next:

```rust
pub async fn supervised(serving: impl AsyncFnOnce(CancellationToken) -> Result<(), ServiceError>, mut signals: impl Stream + Unpin) -> Result<(), ServiceError> {
    let stop = CancellationToken::new();
    let mut serving = pin!(future::maybe_done(serving(stop.clone())));
    future::select(serving.as_mut(), signals.next()).await;
    stop.cancel();
    future::select(serving.as_mut(), signals.next()).await;
    serving.take_output().unwrap_or(Err(ServiceError::Abandoned))
}
```

`async move |stop| serve(listener, handler, stop).await` fills `serving`. `maybe_done` holds its output across both races until `take_output` reads it once. An output more than one future reads is `.shared()`. `signals` is one `SelectAll` that `try_collect` builds over each `SignalStream` a `cfg_select!` value lists per platform.

## [03]-[STREAMS]

Values arriving over time are a `Stream` with each failure one item. `tokio_stream::wrappers` adapt receivers, listeners, watches, intervals, line readers, and join sets:

```rust
pub fn sample<S>(sensor: &S, every: Duration) -> impl Stream<Item = Result<Reading, SensorError>>
where S: Sensor {
    IntervalStream::new(interval(every)).then(move |_| sensor.read())
}
```

`sample` borrows the sensor and reads it once per tick, and a failed read is an `Err` item while the stream continues. `interval` ticks at once, then every period. `set_missed_tick_behavior` sets ticks after slow work: `Burst` by default, `Delay` for a full period, `Skip` for the original grid. `then` runs one read at a time. Consumers stop at the first `Err` through `try_for_each`, or record each failure and continue through `for_each` with a `match`. `ready_chunks(n)` batches waiting items into one write. `WatchStream::new` yields the held value and each change, `from_changes` the changes alone. A `watch` empty until its first value holds `Option<T>`. Streams that end only with their sender (`WatchStream`, `BroadcastStream`) end beside other work through `take_until(stop.cancelled())`. Paged sources unfold into one stream of items:

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

`page` returns its items beside the `next` cursor, `None` after the last page. `try_unfold` threads the cursor and fetches a page only when the consumer polls past the previous one, and `take(n)` stops paging. A failed page ends the stream after its `Err`.

## [04]-[SHARED_STATE]

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

Transitions return the next state beside the reply, and a refused command keeps its state and answers with the refusal. State is the fold's accumulator, no lock guards it, and requests apply in arrival order. Requesters send a command with the sender half of a fresh `oneshot::channel` and await the receiver. A dropped receiver leaves its command applied, and once every sender drops, the fold returns the final state. Other access patterns go through a shared structure's own API:
- `ArcSwap::load` borrows a snapshot for a short read, `load_full` returns an `Arc` held across `.await`, and a reload `store`s the next whole value
- `watch::Sender::send_if_modified` edits the value in place and notifies when its closure returns `true`, `send_replace` publishes a whole next value
- `nonpoison::Mutex` state one closure reads or writes goes through `with_mut`, `get_cloned`, `set`, or `replace`, and no guard outlives the call
- `papaya::HashMap::pin` returns a guard that drops before the next `.await`, `pin_owned` a `Send` guard held across it
- `moka::future::Cache::try_get_with(key, load)` runs one load per missing key while concurrent callers await it and caches only an `Ok`
- `try_get_with` returns `Result<V, Arc<E>>` with one failure every waiter shares, and a crate error variant holds the `Arc<E>` as its source

## [05]-[TELEMETRY]

Values known after span entry fill a field the span declared empty:

```rust
#[tracing::instrument(skip_all, fields({ ACCOUNT } = %command.account, { BALANCE } = field::Empty))]
pub async fn deposited(deposit: &impl AsyncFn(Deposit) -> Result<Account, LedgerError>, command: Deposit) -> Result<Account, LedgerError> {
    let account = deposit(command).await?;
    Span::current().record(BALANCE, field::display(account.balance));
    Ok(account)
}
```

Spans and metrics read crate key constants (`ACCOUNT`, `BALANCE`), and `opentelemetry_semantic_conventions::attribute` supplies each key it names. `skip_all` keeps arguments out of the span unless a field names them. Spawned tasks start outside the caller's span. `in_current_span()` attaches the caller's span to a task future, `instrument(span)` gives it its own (a connection named by its peer). Pure functions emit nothing, and the boundary that calls them records what they returned.

## [06]-[POLICIES]

Retry is `backon` over the operation, its schedule a parameter and `when` naming the failures that repeat:

```rust
pub async fn fetched<S>(source: &S, id: ItemId, schedule: impl BackoffBuilder) -> Result<Item, SourceError>
where S: Source {
    (|| source.fetch(id)).retry(schedule).when(SourceError::is_transient).await
}
```

`ExponentialBuilder::default()` waits one second and doubles for 3 retries under a 60-second cap. `with_jitter`, `with_max_times`, `with_max_delay`, and `with_total_delay` reshape it. Any `Duration` iterator is a schedule. A success or a failure `when` rejects returns at once, and the last failure returns when the schedule ends. `notify` records each retried failure with its delay, and `adjust` replaces a delay with one the failure holds (a `Retry-After`). Waits run on the runtime clock, and cancellation drops each one:
- `timeout(limit, future)` returns `Err(Elapsed)` once the limit passes, one `timeout` around a composed future bounds every step inside it
- `timeout_at(deadline, future)` takes an `Instant`, steps in separate calls sharing one budget receive one deadline
- Crate error variants hold `Elapsed` as their source beside the subject that ran out of time
- `tokio::time::Instant` measures waits and deadlines, a `Timestamp` names the wall-clock instant a record stores

`TryFutureExt::or_else` composes a fallback, and each policy reads only the result it wraps. Served routes take the same policies as `tower` layers on one `ServiceBuilder` (`load_shed`, `concurrency_limit`, `timeout`), outermost first.
