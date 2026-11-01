# [ASYNC]

Covers effects at the boundary, from futures and concurrent fan-out to task scopes, streams, owned state, telemetry, and failure policies.

## [01]-[FUTURES]

Futures do nothing until polled, and dropping one cancels its work at the `.await` it waits on. Values held across an `.await` decide whether a future is `Send`, and a non-`Send` value (an `Rc`, a lock guard, a `papaya` `pin()` guard) sits in a block that ends before the `.await`. Combinators that race or bound a future (`select`, `timeout`, `run_until_cancelled`, `take_until`) drop the losing future, so each raced operation keeps its progress outside the future:
- Cancel-safe operations lose nothing when dropped: `mpsc::Receiver::recv`, `watch::Receiver::changed`, `TcpListener::accept`, `StreamExt::next`, `AsyncReadExt::read`
- `read_exact`, `read_to_end`, and `write_all` drop partial progress, a stream owning the buffer (`LinesStream`) races through its `next`
- `tokio::sync::Mutex::lock`, `Semaphore::acquire`, and `Notify::notified` lose their queue position when dropped
- Output more than one consumer awaits comes from one `FutureExt::shared` future, which runs its work once and clones the output to each

## [02]-[FAN_OUT]

Independent I/O over a collection runs concurrently on the current task through a stream, and a bound the caller passes states how many futures run at once:

```rust
pub async fn fetch_all<S>(source: &S, limit: NonZero<usize>, ids: impl IntoIterator<Item = ItemId>) -> Result<Vec<Item>, NEVec<SourceError>>
where S: Source {
    stream::iter(ids).map(|id| source.fetch(id)).buffered(limit.get()).collect::<Vec<_>>().await.into_iter().collect::<Validated<Vec<_>, _>>().ok()
}
```

`buffered(0)` never polls a fetch, so the bound is a `NonZero`. Every fetch runs to its outcome, and `Validated` keeps each failure for the caller. Concurrency on one task needs no `Send`, no `Arc`, and no `'static` capture, and fits I/O that waits more than it computes.

## [03]-[TASK_SCOPES]

Work that runs in parallel across runtime threads spawns into a scope that owns its tasks. Collecting futures into a `JoinSet` spawns each one, `join_all` awaits every output, and dropping the set aborts the tasks still running:

```rust
pub async fn upload_all<S>(store: Arc<S>, parts: Vec<Part>) -> Result<Vec<Receipt>, NEVec<UploadError>>
where S: Bucket<put(..): Send> + Send + Sync + 'static {
    parts
        .into_iter()
        .map(|part| {
            let store = Arc::clone(&store);
            async move { store.put(part).await }
        })
        .collect::<JoinSet<_>>()
        .join_all()
        .await
        .into_iter()
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}
```

Each task owns an `Arc` clone and its part, so its future is `'static`. Receipts arrive in completion order and each names its part, `tokio_util::task::JoinQueue` yields outputs in spawn order. A panicking task resumes its panic in the caller through `join_all`, so a defect stays a defect. Services hold a `TaskTracker` for tasks they start over their lifetime, and a `CancellationToken` stops the loop that starts them:

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

Accepting ends when `stop` fires or an accept fails, and `close` with `wait` runs on both paths, so every accepted connection finishes before `serve` returns. A token one scope owns becomes a `DropGuard` through `drop_guard`, and the drop cancels it on every exit path. Hosts drain on the first signal and abandon the drain on the next:

```rust
pub async fn supervised(serving: impl Future<Output = Result<(), ServiceError>>, stop: CancellationToken, mut signals: impl Stream + Unpin) -> Result<(), ServiceError> {
    let mut serving = pin!(future::maybe_done(serving));
    future::select(serving.as_mut(), signals.next()).await;
    stop.cancel();
    future::select(serving.as_mut(), signals.next()).await;
    serving.take_output().unwrap_or(Err(ServiceError::Abandoned))
}
```

`serve(listener, handler, stop.clone())` fills `serving`, and `maybe_done` holds its output across both races until `take_output` reads it once. `signals` is one `SelectAll` that `try_collect` builds over each `SignalStream` a `cfg_select!` value lists per platform.

## [04]-[STREAMS]

Values that arrive over time are a `Stream`, and each failure is one item the consumer handles. `tokio_stream::wrappers` adapt receivers, listeners, watches, intervals, line readers, and join sets into streams, and `futures::StreamExt` combinators transform them as iterator adapters transform sequences:

```rust
pub fn sample<S>(sensor: &S, every: Duration) -> impl Stream<Item = Result<Reading, SensorError>>
where S: Sensor {
    IntervalStream::new(interval(every)).then(move |_| sensor.read())
}
```

`sample` borrows the sensor and reads it once per tick, and a failed read is an `Err` item while the stream continues. `interval` ticks at once and then every period, and `set_missed_tick_behavior` decides the ticks after slow work: `Burst` by default, `Delay` for a full period, `Skip` for the original grid. `then` runs one read at a time, `try_for_each` on the consumer stops at the first `Err`, and `for_each` with a `match` records each failure and continues. `ready_chunks(n)` batches the items already waiting into one write. Streams that end only with their sender (`WatchStream`, `BroadcastStream`) end beside other work through `take_until(stop.cancelled())`. Channels take their delivery shape:
- `mpsc` queues values for one consumer, and a bounded `send` waits while the buffer is full (`ReceiverStream`)
- `broadcast` delivers each value to every subscriber, and a consumer answers a `Lagged(n)` item with a resync of the state
- `watch` holds the latest value, `WatchStream::new` yields it and each change, `from_changes` yields changes alone
- `oneshot` carries one reply

## [05]-[OWNED_STATE]

State that many tasks change belongs to one task, and other tasks send it requests. Its task folds the request stream through a pure transition and answers each request on its `oneshot` sender:

```rust
#[derive(Debug, Clone, Copy)]
pub enum Command {
    Deposit(Cents),
    Withdraw(Cents),
    Balance,
}

pub fn applied(balance: Cents, command: Command) -> Result<Cents, LedgerError> {
    match command {
        Command::Deposit(amount) => balance.checked_add(amount),
        Command::Withdraw(amount) => balance.checked_sub(amount),
        Command::Balance => Some(balance),
    }
    .ok_or(LedgerError::Refused { balance, command })
}

pub async fn ledger(opening: Cents, requests: mpsc::Receiver<(Command, oneshot::Sender<Result<Cents, LedgerError>>)>) -> Cents {
    ReceiverStream::new(requests)
        .fold(opening, async |balance, (command, reply)| {
            let outcome = applied(balance, command);
            let next = outcome.as_ref().copied().unwrap_or(balance);
            if let Err(unsent) = reply.send(outcome) {
                tracing::debug!(?unsent, "requester stopped waiting");
            }
            next
        })
        .await
}
```

Balance is the fold's accumulator, no lock guards it, and requests apply in arrival order. Requesters send a command with the sender half of a fresh `oneshot::channel` and await the receiver, and every command answers with the balance after it or the refusal. A failed `send` means its requester stopped waiting, and the command still applies. Once every sender drops, the fold returns the final balance. State with another access pattern sits in a shared structure whose API states that access:
- `ArcSwap::load` borrows a snapshot for a short read, `load_full` returns an `Arc` held across `.await`, and a reload `store`s the next whole value
- `watch::Sender<BTreeMap<K, V>>` holds state that changes together and owns both value and notification
- `send_if_modified` edits that value in place and notifies when its closure returns `true`, `send_replace` publishes a whole next value
- `nonpoison::Mutex` state one closure reads or writes goes through `with_mut`, `get_cloned`, `set`, or `replace`, and no guard outlives the call
- `papaya::HashMap::pin` returns a guard that drops before the next `.await`, `pin_owned` a `Send` guard held across it
- `moka::future::Cache::try_get_with(key, load)` runs one load per missing key while concurrent callers await it and caches only an `Ok`
- `try_get_with` returns `Result<V, Arc<E>>` since every waiter shares one failure, and a crate error variant holds the `Arc<E>` as its source

## [06]-[TELEMETRY]

Boundary operations open a span that names the operation and records its inputs as fields, and a value known after entry fills a field the span declared empty:

```rust
#[tracing::instrument(skip_all, fields({ ACCOUNT } = %command.account, { BALANCE } = field::Empty))]
pub async fn transfer<L>(ledger: &L, command: Transfer) -> Result<Receipt, LedgerError>
where L: Ledger {
    let receipt = ledger.apply(&command).await?;
    Span::current().record(BALANCE, field::display(receipt.balance));
    Ok(receipt)
}
```

`ACCOUNT` and `BALANCE` are crate key constants a span and a metric both read, and a key `opentelemetry_semantic_conventions::attribute` names comes from there. `skip_all` keeps arguments out of the span unless a field names them, and a returned error reaches the host, which records it once. Spawned tasks start outside the caller's span: `in_current_span()` on the future carries the span into the task, and `instrument(span)` gives the task its own span (a connection named by its peer). Pure functions emit nothing, and the boundary that calls them records what they returned.

## [07]-[POLICIES]

Retry takes its schedule as delays and a predicate naming the failures that repeat:

```rust
pub async fn retried<T, E>(schedule: impl IntoIterator<Item = Duration>, transient: impl Fn(&E) -> bool, attempt: impl AsyncFn() -> Result<T, E>) -> Result<T, E> {
    stream::iter(schedule)
        .fold(attempt().await, async |outcome, delay| match outcome {
            Err(failure) if transient(&failure) => {
                sleep(delay).await;
                attempt().await
            }
            settled => settled,
        })
        .await
}
```

`iter::successors(Some(base), |delay| delay.checked_mul(2)).take(attempts)` states a bounded exponential schedule, and the call site passes the error's own predicate (`SourceError::is_transient`). Waits run on the runtime clock, and cancellation drops each one:
- `timeout(limit, future)` returns `Err(Elapsed)` once the limit passes, one `timeout` around a composed future bounds every step inside it
- `timeout_at(deadline, future)` takes an `Instant`, steps in separate calls sharing one budget receive one deadline
- Crate error variants hold `Elapsed` as their source beside the subject that ran out of time
- `tokio::time::Instant` measures waits and deadlines, a `Timestamp` names the wall-clock instant a record stores

Fallback runs a lower-priority effect through `TryFutureExt::or_else`, and each policy reads only the result it wraps. Served routes take the same policies as `tower` layers on one `ServiceBuilder` (`load_shed`, `concurrency_limit`, `timeout`), outermost first.
