// --- [OPERATIONS] ----------------------------------------------------------------------

fn async_lock_without_await(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: no `tokio::sync::Mutex` guard of `hits` is held across an `.await`
      --> tools/dylint/examples/async_lock_without_await.rs:23:5
       |
    23 |     hits: Mutex<u64>,
       |     ^^^^^^^^^^^^^^^^
       |
    note: `hits` is locked here
      --> tools/dylint/examples/async_lock_without_await.rs:29:24
       |
    29 |         let mut hits = self.hits.lock().await;
       |                        ^^^^^^^^^^^^^^^^
       = help: declare `std::sync::nonpoison::Mutex` in place of `tokio::sync::Mutex`
       = note: `#[warn(async_lock_without_await)]` on by default

    warning: no `tokio::sync::RwLock` guard of `samples` is held across an `.await`
      --> tools/dylint/examples/async_lock_without_await.rs:24:5
       |
    24 |     samples: RwLock<Vec<u64>>,
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: `samples` is locked here
      --> tools/dylint/examples/async_lock_without_await.rs:34:40
       |
    34 |     async fn add(&self, sample: u64) { self.samples.write().await.push(sample); }
       |                                        ^^^^^^^^^^^^^^^^^^^^
    35 |
    36 |     async fn sum(&self) -> u64 { self.samples.read().await.iter().sum() }
       |                                  ^^^^^^^^^^^^^^^^^^^
       = help: declare `std::sync::nonpoison::RwLock` in place of `tokio::sync::RwLock`

    warning: no `tokio::sync::Mutex` guard of `count` is held across an `.await`
      --> tools/dylint/examples/async_lock_without_await.rs:41:5
       |
    41 |     count: Arc<Mutex<u64>>,
       |     ^^^^^^^^^^^^^^^^^^^^^^
       |
    note: `count` is locked here
      --> tools/dylint/examples/async_lock_without_await.rs:51:25
       |
    51 |         let mut count = self.count.lock().await;
       |                         ^^^^^^^^^^^^^^^^^
       = help: declare `std::sync::nonpoison::Mutex` in place of `tokio::sync::Mutex`

    warning: no `tokio::sync::Mutex` guard of `total` is held across an `.await`
      --> tools/dylint/examples/async_lock_without_await.rs:42:5
       |
    42 |     total: Arc<Mutex<u64>>,
       |     ^^^^^^^^^^^^^^^^^^^^^^
       |
    note: `total` is locked here
      --> tools/dylint/examples/async_lock_without_await.rs:57:25
       |
    57 |         let mut total = self.total.lock().await;
       |                         ^^^^^^^^^^^^^^^^^
       = help: declare `std::sync::nonpoison::Mutex` in place of `tokio::sync::Mutex`

    warning: no `tokio::sync::Mutex` guard of `count` is held across an `.await`
      --> tools/dylint/examples/async_lock_without_await.rs:92:5
       |
    92 |     count: Mutex<u64>,
       |     ^^^^^^^^^^^^^^^^^
       |
    note: `count` is locked here
      --> tools/dylint/examples/async_lock_without_await.rs:98:25
       |
    98 |         let mut guard = count.lock().await;
       |                         ^^^^^^^^^^^^
       = help: declare `std::sync::nonpoison::Mutex` in place of `tokio::sync::Mutex`

    warning: no `tokio::sync::Mutex` guard of `total` is held across an `.await`
       --> tools/dylint/examples/async_lock_without_await.rs:157:9
        |
    157 |     let total = Mutex::new(0_u64);
        |         ^^^^^
        |
    note: `total` is locked here
       --> tools/dylint/examples/async_lock_without_await.rs:158:20
        |
    158 |     let mut held = total.lock().await;
        |                    ^^^^^^^^^^^^
        = help: declare `std::sync::nonpoison::Mutex` in place of `tokio::sync::Mutex`

    warning: no `tokio::sync::Mutex` guard of `TOTAL` is held across an `.await`
       --> tools/dylint/examples/async_lock_without_await.rs:19:1
        |
     19 | static TOTAL: Mutex<u64> = Mutex::const_new(0);
        | ^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `TOTAL` is locked here
       --> tools/dylint/examples/async_lock_without_await.rs:164:24
        |
    164 | fn initial() -> u64 { *TOTAL.blocking_lock() }
        |                        ^^^^^^^^^^^^^^^^^^^^^
    165 |
    166 | async fn current() -> u64 { *TOTAL.lock().await }
        |                              ^^^^^^^^^^^^
        = help: declare `std::sync::nonpoison::Mutex` in place of `tokio::sync::Mutex`
    ");
}

fn blocking_call_in_async(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `std::thread::sleep` blocks the worker thread polling its async body
      --> tools/dylint/examples/blocking_call_in_async.rs:22:20
       |
    22 | async fn slept() { thread::sleep(Duration::ZERO); }
       |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: await an async counterpart, or run the call in `tokio::task::spawn_blocking`
       = note: `#[warn(blocking_call_in_async)]` on by default

    warning: `fs_err::read_to_string` blocks the worker thread polling its async body
      --> tools/dylint/examples/blocking_call_in_async.rs:24:63
       |
    24 | async fn module_function(path: &Path) -> io::Result<String> { fs_err::read_to_string(path) }
       |                                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: await an async counterpart, or run the call in `tokio::task::spawn_blocking`

    warning: `fs_err::PathExt::fs_err_metadata` blocks the worker thread polling its async body
      --> tools/dylint/examples/blocking_call_in_async.rs:26:54
       |
    26 | async fn described(path: &Path) -> io::Result<u64> { path.fs_err_metadata().map(|metadata| metadata.len()) }
       |                                                      ^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: await an async counterpart, or run the call in `tokio::task::spawn_blocking`

    warning: `std::io::BufRead::read_line` blocks the worker thread polling its async body
      --> tools/dylint/examples/blocking_call_in_async.rs:30:5
       |
    30 |     BufReader::new(file?).read_line(&mut line)?;
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: await an async counterpart, or run the call in `tokio::task::spawn_blocking`

    warning: `fs_err::read` blocks the worker thread polling its async body
      --> tools/dylint/examples/blocking_call_in_async.rs:34:87
       |
    34 | async fn sizes(paths: &[PathBuf]) -> io::Result<Vec<usize>> { paths.iter().map(|path| fs_err::read(path).map(|bytes| bytes.len())).c...
       |                                                                                       ^^^^^^^^^^^^^^^^^^
       |
       = help: await an async counterpart, or run the call in `tokio::task::spawn_blocking`

    warning: `fs_err::read` blocks the worker thread polling its async body
      --> tools/dylint/examples/blocking_call_in_async.rs:36:56
       |
    36 | async fn doubled(path: &Path) -> io::Result<Vec<u8>> { both!(path) }
       |                                                        ^^^^^^^^^^^
       |
       = help: await an async counterpart, or run the call in `tokio::task::spawn_blocking`

    warning: `std::thread::scope` blocks the worker thread polling its async body
      --> tools/dylint/examples/blocking_call_in_async.rs:41:5
       |
    41 | /     thread::scope(|scope| {
    42 | |         scope.spawn(|| thread::sleep(Duration::ZERO));
    43 | |     });
       | |______^
       |
       = help: await an async counterpart, or run the call in `tokio::task::spawn_blocking`

    warning: `std::sync::mpsc::Receiver::iter` blocks the worker thread polling its async body
      --> tools/dylint/examples/blocking_call_in_async.rs:78:70
       |
    78 | async fn drained(received: std::sync::mpsc::Receiver<u8>) -> usize { received.iter().count() }
       |                                                                      ^^^^^^^^^^^^^^^
       |
       = help: await an async counterpart, or run the call in `tokio::task::spawn_blocking`

    warning: `std::net::TcpListener::incoming` blocks the worker thread polling its async body
      --> tools/dylint/examples/blocking_call_in_async.rs:80:61
       |
    80 | async fn served(listener: std::net::TcpListener) -> usize { listener.incoming().take(1).count() }
       |                                                             ^^^^^^^^^^^^^^^^^^^
       |
       = help: await an async counterpart, or run the call in `tokio::task::spawn_blocking`
    ");
}

fn detached_task(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: dropping `tokio::task::JoinHandle` detaches its task
      --> tools/dylint/examples/detached_task.rs:17:26
       |
    17 |     fn drop(&mut self) { tokio::spawn(async {}); }
       |                          ^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: spawn release work through `TaskTracker::spawn_on` with a `Handle` captured at construction, or hold a `DropGuard` with a token the release task awaits
       = note: `#[warn(detached_task)]` on by default

    warning: dropping `tokio::task::JoinHandle` detaches its task
      --> tools/dylint/examples/detached_task.rs:30:18
       |
    30 | fn statement() { tokio::spawn(async {}); }
       |                  ^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: spawn on a `TaskTracker` or `JoinSet` that its owner waits on at shutdown

    warning: dropping `std::thread::JoinHandle` detaches its thread
      --> tools/dylint/examples/detached_task.rs:32:15
       |
    32 | fn unread() { let _worker = thread::spawn(|| 1_i32); }
       |               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: spawn the thread in `std::thread::scope`

    warning: dropping `std::thread::JoinHandle` detaches its thread
      --> tools/dylint/examples/detached_task.rs:34:23
       |
    34 | async fn threaded() { let _worker = thread::spawn(|| 1_i32); }
       |                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: run the work in `tokio::task::spawn_blocking` and await its handle

    warning: dropping `tokio::task::JoinHandle` detaches its task
      --> tools/dylint/examples/detached_task.rs:36:16
       |
    36 | fn dropped() { drop(tokio::spawn(async {})); }
       |                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: spawn on a `TaskTracker` or `JoinSet` that its owner waits on at shutdown

    warning: dropping `tokio::task::JoinHandle` detaches its task
      --> tools/dylint/examples/detached_task.rs:38:25
       |
    38 | fn finished() -> bool { tokio::spawn(async {}).is_finished() }
       |                         ^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: spawn on a `TaskTracker` or `JoinSet` that its owner waits on at shutdown

    warning: assignment drops the previous `tokio::task::JoinHandle` and detaches its task
      --> tools/dylint/examples/detached_task.rs:40:35
       |
    40 | fn restart(worker: &mut Worker) { worker.task = tokio::spawn(async { 2 }); }
       |                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: spawn on a `TaskTracker` or `JoinSet` that its owner waits on at shutdown

    warning: `tokio::task::JoinSet::detach_all` detaches every task in the set
      --> tools/dylint/examples/detached_task.rs:42:43
       |
    42 | fn detached(set: &mut JoinSet<u64>) { set.detach_all(); }
       |                                           ^^^^^^^^^^^^
       |
       = help: await `JoinSet::join_all`, or call `JoinSet::shutdown` where its owner stops

    warning: dropping `tokio::task::JoinHandle` detaches its task
      --> tools/dylint/examples/detached_task.rs:45:5
       |
    45 |     spawned!(async {});
       |     ^^^^^^^^^^^^^^^^^^
       |
       = help: spawn on a `TaskTracker` or `JoinSet` that its owner waits on at shutdown
    ");
}

fn manual_abort_on_drop(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Drop::drop` calls `tokio::task::JoinHandle::abort` on field `task`
      --> tools/dylint/examples/manual_abort_on_drop.rs:20:9
       |
    20 |         self.task.abort();
       |         ^^^^^^^^^^^^^^^^^
       |
       = help: store the handle as `tokio_util::task::AbortOnDropHandle` to abort its task on drop
       = note: `#[warn(manual_abort_on_drop)]` on by default

    warning: `Drop::drop` calls `tokio::task::AbortHandle::abort` on field `abort`
      --> tools/dylint/examples/manual_abort_on_drop.rs:21:9
       |
    21 |         AbortHandle::abort(&self.abort);
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: store the handle as `tokio_util::task::AbortOnDrop` to abort its task on drop

    warning: `Drop::drop` calls `tokio_util::sync::CancellationToken::cancel` on field `token`
      --> tools/dylint/examples/manual_abort_on_drop.rs:22:9
       |
    22 |         self.token.cancel();
       |         ^^^^^^^^^^^^^^^^^^^
       |
       = help: store the `DropGuard` from `CancellationToken::drop_guard` to cancel its token on drop

    warning: `Drop::drop` calls `tokio::task::JoinHandle::abort` on field `task`
      --> tools/dylint/examples/manual_abort_on_drop.rs:57:13
       |
    57 |             task.abort();
       |             ^^^^^^^^^^^^
       |
       = help: store the handle as `tokio_util::task::AbortOnDropHandle` to abort its task on drop

    warning: `Drop::drop` calls `tokio::task::JoinSet::abort_all` on field `tasks`
      --> tools/dylint/examples/manual_abort_on_drop.rs:67:26
       |
    67 |     fn drop(&mut self) { self.tasks.abort_all(); }
       |                          ^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call, dropping `JoinSet` aborts every task
    ");
}

fn manual_arc_swap(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: every write to `snapshot` replaces its whole `Arc`
      --> tools/dylint/examples/manual_arc_swap.rs:20:5
       |
    20 |     snapshot: RwLock<Arc<Config>>,
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: replaced here
      --> tools/dylint/examples/manual_arc_swap.rs:81:38
       |
    81 |     fn update(&self, next: Config) { *self.snapshot.write() = Arc::new(next); }
       |                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: declare `arc_swap::ArcSwap`, read with `load_full()`, and write with `store(Arc::new(..))` or `rcu(..)`
       = note: `#[warn(manual_arc_swap)]` on by default

    warning: every write to `latest` replaces its whole `Arc`
      --> tools/dylint/examples/manual_arc_swap.rs:25:5
       |
    25 |     latest: Mutex<Arc<Config>>,
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: replaced here
      --> tools/dylint/examples/manual_arc_swap.rs:87:38
       |
    87 |     fn update(&self, next: Config) { self.latest.set(Arc::new(next)); }
       |                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: declare `arc_swap::ArcSwap`, read with `load_full()`, and write with `store(Arc::new(..))` or `rcu(..)`

    warning: every write to `current` replaces its whole value, and no receiver subscribes to it
       --> tools/dylint/examples/manual_arc_swap.rs:40:5
        |
     40 |     current: watch::Sender<Config>,
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: replaced here
       --> tools/dylint/examples/manual_arc_swap.rs:107:48
        |
    107 |     fn update(&self, next: Config) -> Config { self.current.send_replace(next) }
        |                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: declare `arc_swap::ArcSwap`, read with `load()`, and write with `store(Arc::new(..))`

    warning: every write to `current` replaces its whole value, and no receiver subscribes to it
       --> tools/dylint/examples/manual_arc_swap.rs:45:5
        |
     45 |     current: watch::Sender<Config>,
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: replaced here
       --> tools/dylint/examples/manual_arc_swap.rs:115:38
        |
    115 |     fn update(&self, next: Config) { self.current.send_modify(|current| *current = next); }
        |                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: declare `arc_swap::ArcSwap`, read with `load()`, and write with `store(Arc::new(..))`

    warning: every write to `snapshot` replaces its whole `Arc`
       --> tools/dylint/examples/manual_arc_swap.rs:75:5
        |
     75 |     snapshot: RwLock<Arc<Config>>,
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: replaced here
       --> tools/dylint/examples/manual_arc_swap.rs:157:38
        |
    157 |     fn update(&self, next: Config) { self.snapshot.set(Arc::new(next)); }
        |                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: declare `arc_swap::ArcSwap`, read with `load_full()`, and write with `store(Arc::new(..))` or `rcu(..)`

    warning: every write to `lock` replaces its whole `Arc`
       --> tools/dylint/examples/manual_arc_swap.rs:163:9
        |
    163 |     let lock = Arc::new(Mutex::new(Arc::new(Config::default())));
        |         ^^^^
        |
    note: replaced here
       --> tools/dylint/examples/manual_arc_swap.rs:165:5
        |
    165 |     writer.set(Arc::new(Config { port: 9 }));
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: declare `arc_swap::ArcSwap`, read with `load_full()`, and write with `store(Arc::new(..))` or `rcu(..)`

    warning: every write to `sender` replaces its whole value, and no receiver subscribes to it
       --> tools/dylint/examples/manual_arc_swap.rs:170:9
        |
    170 |     let sender = watch::Sender::new(Config::default());
        |         ^^^^^^
        |
    note: replaced here
       --> tools/dylint/examples/manual_arc_swap.rs:171:5
        |
    171 |     sender.send_replace(Config { port: 10 });
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: declare `arc_swap::ArcSwap`, read with `load()`, and write with `store(Arc::new(..))`
    ");
}

fn manual_cancellation_token(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: every write to `stop` stores `true`, and a loop polls it as a stop flag
      --> tools/dylint/examples/manual_cancellation_token.rs:12:5
       |
    12 |     stop: AtomicBool,
       |     ^^^^^^^^^^^^^^^^
       |
    note: set here
      --> tools/dylint/examples/manual_cancellation_token.rs:31:22
       |
    31 |     fn halt(&self) { self.stop.store(true, Ordering::Release); }
       |                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    note: polled here
      --> tools/dylint/examples/manual_cancellation_token.rs:35:16
       |
    35 |         while !self.stop.load(Ordering::Acquire) && steps < 8 {
       |                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: declare `tokio_util::sync::CancellationToken`, call `cancel()` in place of each write, and poll `is_cancelled()` in the loop condition
       = note: `#[warn(manual_cancellation_token)]` on by default

    warning: every write to `open` stores `true`, and a loop polls it as a stop flag
      --> tools/dylint/examples/manual_cancellation_token.rs:24:5
       |
    24 |     open: AtomicBool,
       |     ^^^^^^^^^^^^^^^^
       |
    note: set here
      --> tools/dylint/examples/manual_cancellation_token.rs:76:5
       |
    76 |     gate.open.store(true, Ordering::Release);
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    note: polled here
      --> tools/dylint/examples/manual_cancellation_token.rs:77:12
       |
    77 |     while !gate.open.load(Ordering::Acquire) {
       |            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: declare `tokio_util::sync::CancellationToken`, call `cancel()` in place of each write, and poll `is_cancelled()` in the loop condition

    warning: every write to `stop` stores `true`, and a loop polls it as a stop flag
      --> tools/dylint/examples/manual_cancellation_token.rs:83:9
       |
    83 |     let stop = Arc::new(AtomicBool::new(false));
       |         ^^^^
       |
    note: set here
      --> tools/dylint/examples/manual_cancellation_token.rs:93:5
       |
    93 |     stop.store(true, Ordering::Release);
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    note: polled here
      --> tools/dylint/examples/manual_cancellation_token.rs:87:16
       |
    87 |         while !flag.load(Ordering::Acquire) {
       |                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: declare `tokio_util::sync::CancellationToken`, call `cancel()` in place of each write, and poll `is_cancelled()` in the loop condition

    warning: every write to `stop` stores `true`, and a loop polls it as a stop flag
       --> tools/dylint/examples/manual_cancellation_token.rs:121:9
        |
    121 |     let stop = AtomicBool::new(false);
        |         ^^^^
        |
    note: set here
       --> tools/dylint/examples/manual_cancellation_token.rs:123:5
        |
    123 |     stop.store(true, Ordering::Release);
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    note: polled here
       --> tools/dylint/examples/manual_cancellation_token.rs:124:12
        |
    124 |     while !stop.load(Ordering::Acquire) && steps < 8 {
        |            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: declare `tokio_util::sync::CancellationToken`, call `cancel()` in place of each write, and await `cancelled()` or run the loop through `run_until_cancelled(..)`

    warning: every write to `stop` stores `true`, and a loop polls it as a stop flag
       --> tools/dylint/examples/manual_cancellation_token.rs:148:9
        |
    148 |     let stop = AtomicBool::new(false);
        |         ^^^^
        |
    note: set here
       --> tools/dylint/examples/manual_cancellation_token.rs:150:5
        |
    150 |     stop.store(true, Ordering::Release);
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    note: polled here
       --> tools/dylint/examples/manual_cancellation_token.rs:152:12
        |
    152 |         if stop.load(Ordering::Acquire) {
        |            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: declare `tokio_util::sync::CancellationToken`, call `cancel()` in place of each write, and poll `is_cancelled()` in the loop condition
    ");
}

fn manual_stream(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `while let` awaits `recv` on `received` one item at a time
      --> tools/dylint/examples/manual_stream.rs:23:11
       |
    23 |     while let Some(value) = received.recv().await {
       |           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: wrap `received` in `tokio_stream::wrappers::ReceiverStream` and replace the loop with `StreamExt` combinators
       = note: `#[warn(manual_stream)]` on by default

    warning: `while let` awaits `next_segment` on `lines` one item at a time
      --> tools/dylint/examples/manual_stream.rs:31:11
       |
    31 |     while let Some(line) = lines.next_segment().await.transpose() {
       |           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: wrap `lines` in `tokio_stream::wrappers::SplitStream` and replace the loop with `StreamExt` combinators

    warning: `while let` awaits `next_line` on `lines` one item at a time
      --> tools/dylint/examples/manual_stream.rs:39:11
       |
    39 |     while let Some(line) = lines.next_line().await? {
       |           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: wrap `lines` in `tokio_stream::wrappers::LinesStream` and replace the loop with `StreamExt` combinators

    warning: `while let` awaits `recv` on `received` one item at a time
      --> tools/dylint/examples/manual_stream.rs:46:11
       |
    46 |     while let Some(value) = received.recv().await {
       |           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: wrap `received` in `tokio_stream::wrappers::UnboundedReceiverStream` and replace the loop with `StreamExt` combinators

    warning: `while let` awaits `recv` on `received` one item at a time
      --> tools/dylint/examples/manual_stream.rs:56:11
       |
    56 |     while let Some(value) = received.recv().await {
       |           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: wrap `received` in `tokio_stream::wrappers::ReceiverStream` and replace the loop with `StreamExt` combinators
    ");
}

fn needless_boxed_future(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: associated type `IntoFuture` is `Pin<Box<dyn Future>>`
      --> tools/dylint/examples/needless_boxed_future.rs:78:23
       |
    78 |     type IntoFuture = Boxed;
       |                       ^^^^^
       |
       = help: declare `type IntoFuture = impl Future` with the same bounds under `#![feature(impl_trait_in_assoc_type)]`
       = note: `#[warn(needless_boxed_future)]` on by default

    warning: `Source::fetch` returns `Pin<Box<dyn Future>>` with no `dyn Source` use
      --> tools/dylint/examples/needless_boxed_future.rs:12:24
       |
    12 |     fn fetch(&self) -> Pin<Box<dyn Future<Output = u64> + '_>>;
       |                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: declare `async fn fetch`

    warning: `Relay::relay` returns `Pin<Box<dyn Future>>` with no `dyn Relay` use
      --> tools/dylint/examples/needless_boxed_future.rs:16:24
       |
    16 |     fn relay(&self) -> Pin<Box<dyn Future<Output = u64> + Send + '_>>;
       |                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: declare `async fn relay` and require `Send` at call sites with `T: Relay<relay(..): Send>`

    warning: `next` returns `Pin<Box<dyn Future>>` with no function pointer use
      --> tools/dylint/examples/needless_boxed_future.rs:34:23
       |
    34 |     fn next(&self) -> Pin<Box<dyn Future<Output = u64> + '_>> { Box::pin(async move { self.start.strict_add(1) }) }
       |                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return `impl Future` with the same bounds

    warning: `countdown` returns `Pin<Box<dyn Future>>` with no function pointer use
      --> tools/dylint/examples/needless_boxed_future.rs:36:40
       |
    36 |     fn countdown(&self, steps: u64) -> Pin<Box<dyn Future<Output = u64> + '_>> {
       |                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: declare `async fn` and box the recursive call

    warning: `boxed` returns `Pin<Box<dyn Future>>` with no function pointer use
      --> tools/dylint/examples/needless_boxed_future.rs:86:24
       |
    86 | fn boxed(work: u64) -> Boxed { Box::pin(async move { work }) }
       |                        ^^^^^
       |
       = help: return `impl Future` with the same bounds

    warning: `depth` returns `Pin<Box<dyn Future>>` with no function pointer use
      --> tools/dylint/examples/needless_boxed_future.rs:88:25
       |
    88 | fn depth(level: u64) -> Pin<Box<dyn Future<Output = u64>>> {
       |                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: declare `async fn` and box the recursive call

    warning: `chosen` returns `Pin<Box<dyn Future>>` with no function pointer use
      --> tools/dylint/examples/needless_boxed_future.rs:97:26
       |
    97 | fn chosen(fast: bool) -> Pin<Box<dyn Future<Output = u64>>> { if fast { Box::pin(async { 1 }) } else { Box::pin(future::ready(2)) } }
       |                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: move the branches into one `async move` block and return `impl Future`
    ");
}

fn optional_channel_sender(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: every use of `0` tests its `Option` sender with `if let Some` and no `else`
      --> tools/dylint/examples/optional_channel_sender.rs:8:17
       |
     8 | struct Progress(Option<watch::Sender<u32>>);
       |                 ^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: tested here
      --> tools/dylint/examples/optional_channel_sender.rs:28:9
       |
    28 | /         if let Some(sent) = &self.0 {
    29 | |             sent.send_modify(|step| *step = step.saturating_add(1));
    30 | |         }
       | |_________^
       = help: hold the sender without `Option`, a sender with no receiver blocks nothing
       = note: `#[warn(optional_channel_sender)]` on by default

    warning: every use of `sent` tests its `Option` sender with `if let Some` and no `else`
      --> tools/dylint/examples/optional_channel_sender.rs:59:9
       |
    59 |     let sent = tracked.then(|| watch::Sender::new(0_u32));
       |         ^^^^
       |
    note: tested here
      --> tools/dylint/examples/optional_channel_sender.rs:60:5
       |
    60 | /     if let Some(sent) = &sent {
    61 | |         sent.send_replace(1);
    62 | |     }
       | |_____^
       = help: hold the sender without `Option`, a sender with no receiver blocks nothing
    ");
}

fn redundant_spawn(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `tokio::spawn` spawns a task that only awaits `tokio::task::JoinHandle<u8>`
      --> tools/dylint/examples/redundant_spawn.rs:11:82
       |
    11 | fn rewrapped_direct(task: JoinHandle<u8>) -> JoinHandle<Result<u8, JoinError>> { tokio::spawn(task) }
       |                                                                                  ^^^^^^^^^^^^^^^^^^
       |
    note: `tokio::task::JoinHandle<u8>` awaited here
      --> tools/dylint/examples/redundant_spawn.rs:11:95
       |
    11 | fn rewrapped_direct(task: JoinHandle<u8>) -> JoinHandle<Result<u8, JoinError>> { tokio::spawn(task) }
       |                                                                                               ^^^^
       = help: await `tokio::task::JoinHandle<u8>` where its result is read, with no task around it
       = note: `#[warn(redundant_spawn)]` on by default

    warning: `tokio::spawn` spawns a task that only awaits `tokio::task::JoinHandle<std::result::Result<u8, tokio::task::JoinError>>`
      --> tools/dylint/examples/redundant_spawn.rs:13:100
       |
    13 | ...r>>) -> JoinHandle<Result<u8, JoinError>> { tokio::spawn(async move { task.await? }) }
       |                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: `tokio::task::JoinHandle<std::result::Result<u8, tokio::task::JoinError>>` awaited here
      --> tools/dylint/examples/redundant_spawn.rs:13:126
       |
    13 | fn rewrapped_tried(task: JoinHandle<Result<u8, JoinError>>) -> JoinHandle<Result<u8, JoinError>> { tokio::spawn(async move { task.aw...
       |                                                                                                                              ^^^^
       = help: await `tokio::task::JoinHandle<std::result::Result<u8, tokio::task::JoinError>>` where its result is read, with no task around it

    warning: `tokio::runtime::Handle::spawn` spawns a task that only awaits `tokio::task::JoinHandle<u8>`
      --> tools/dylint/examples/redundant_spawn.rs:15:96
       |
    15 | ...JoinHandle<Result<u16, JoinError>> { handle.spawn(async move { task.await.map(u16::from) }) }
       |                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: `tokio::task::JoinHandle<u8>` awaited here
      --> tools/dylint/examples/redundant_spawn.rs:15:122
       |
    15 | fn rewrapped_on(task: JoinHandle<u8>, handle: &Handle) -> JoinHandle<Result<u16, JoinError>> { handle.spawn(async move { task.await....
       |                                                                                                                          ^^^^
       = help: await `tokio::task::JoinHandle<u8>` where its result is read, with no task around it
    ");
}

fn sequential_await(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `for` loop awaits futures built before it in sequence
      --> tools/dylint/examples/sequential_await.rs:49:5
       |
    49 |     for receiver in receivers {
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: each future completes here before the next starts
      --> tools/dylint/examples/sequential_await.rs:50:22
       |
    50 |         replies.push(time::timeout(Duration::ZERO, receiver).await);
       |                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: await `futures::future::join_all` over the futures, or `futures::stream::iter(..).buffered(n)` to bound concurrency, both keep input order
       = note: `#[warn(sequential_await)]` on by default

    warning: `for` loop builds and awaits one independent future per item in sequence
      --> tools/dylint/examples/sequential_await.rs:62:5
       |
    62 |     for id in ids {
       |     ^^^^^^^^^^^^^
       |
    note: each future completes here before the next starts
      --> tools/dylint/examples/sequential_await.rs:63:18
       |
    63 |         out.push(fetched(id).await);
       |                  ^^^^^^^^^^^^^^^^^
       = help: await `futures::future::join_all` over the futures, or `futures::stream::iter(..).buffered(n)` to bound concurrency, both keep input order
    ");
}

fn short_circuited_join(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `futures::future::join` runs futures together, and `?` on its results keeps only the first error
      --> tools/dylint/examples/short_circuited_join.rs:18:5
       |
    18 |     let (left, right) = future::join(fetch(1), fetch(2)).await;
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: each `?` returns here before the remaining results are read
      --> tools/dylint/examples/short_circuited_join.rs:19:8
       |
    19 |     Ok(left?.strict_add(right?))
       |        ^^^^             ^^^^^
       = help: combine the results with `validated::Validated::from(..).map2(..)` and `.ok()`, or race a future that outlives its partner through `select`
       = note: `#[warn(short_circuited_join)]` on by default

    warning: `join!` runs futures together, and `?` on its results keeps only the first error
      --> tools/dylint/examples/short_circuited_join.rs:23:5
       |
    23 |     let (left, right) = tokio::join!(fetch(1), fetch(2));
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: each `?` returns here before the remaining results are read
      --> tools/dylint/examples/short_circuited_join.rs:24:8
       |
    24 |     Ok(left?.strict_add(right?))
       |        ^^^^             ^^^^^
       = help: combine the results with `validated::Validated::from(..).map2(..)` and `.ok()`, or race a future that outlives its partner through `select`

    warning: `try_collect` after `buffered` stops at the first error of the buffered futures
      --> tools/dylint/examples/short_circuited_join.rs:43:67
       |
    43 | async fn buffered(ids: Vec<u32>) -> Result<Vec<u32>, io::Error> { stream::iter(ids).map(fetch).buffered(4).try_collect().await }
       |                                                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: collect the stream into a `Vec`, then collect its results into `validated::Validated<_, _>` and call `.ok()`

    warning: `collect` over `join_all` keeps only the first error of the joined futures
      --> tools/dylint/examples/short_circuited_join.rs:47:65
       |
    47 | ...sult<Vec<u32>, io::Error> { future::join_all(ids.into_iter().map(fetch)).await.into_iter().collect() }
       |                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: collect the results into `validated::Validated<_, _>` and call `.ok()`

    warning: `try_collect` over `join_all` keeps only the first error of the joined futures
      --> tools/dylint/examples/short_circuited_join.rs:49:64
       |
    49 | ...lt<Vec<u32>, io::Error> { future::join_all(ids.into_iter().map(fetch)).await.into_iter().try_collect() }
       |                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: collect the results into `validated::Validated<_, _>` and call `.ok()`

    warning: `collect` over `buffered` keeps only the first error of the joined futures
      --> tools/dylint/examples/short_circuited_join.rs:85:77
       |
    85 | ...<u32>, io::Error> { stream::iter(ids).map(fetch).buffered(4).collect::<Vec<_>>().await.into_iter().collect() }
       |                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: collect the results into `validated::Validated<_, _>` and call `.ok()`

    warning: `collect` over `join_all` keeps only the first error of the joined futures
      --> tools/dylint/examples/short_circuited_join.rs:87:105
       |
    87 | ...:Error>>) -> Result<Vec<u32>, io::Error> { set.join_all().await.into_iter().collect() }
       |                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: collect the results into `validated::Validated<_, _>` and call `.ok()`
    ");
}

fn unbounded_pending_map(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `waiting` holds `tokio::sync::oneshot::Sender<u32>` entries that only a keyed `remove` deletes
      --> tools/dylint/examples/unbounded_pending_map.rs:20:5
       |
    20 |     waiting: Mutex<BTreeMap<u64, oneshot::Sender<u32>>>,
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: inserted here, an entry its caller never removes stays until the map drops
      --> tools/dylint/examples/unbounded_pending_map.rs:83:9
       |
    83 |         self.waiting.lock().insert(key, sender);
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: bound the map by age or count at insertion, evict with `retain` or `pop_first`, or remove each entry in `Drop` of a guard its caller holds
       = note: `#[warn(unbounded_pending_map)]` on by default

    warning: `calls` holds `(Call, u32)` entries that only a keyed `remove` deletes
      --> tools/dylint/examples/unbounded_pending_map.rs:25:5
       |
    25 |     calls: Mutex<BTreeMap<u64, (Call, u32)>>,
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: inserted here, an entry its caller never removes stays until the map drops
      --> tools/dylint/examples/unbounded_pending_map.rs:91:67
       |
    91 | ...) -> Option<(Call, u32)> { self.calls.lock().insert(key, (Call { future: async { 0 }.boxed() }, step)) }
       |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: bound the map by age or count at insertion, evict with `retain` or `pop_first`, or remove each entry in `Drop` of a guard its caller holds

    warning: `waiting` holds `tokio::sync::oneshot::Sender<u32>` entries that only a keyed `remove` deletes
      --> tools/dylint/examples/unbounded_pending_map.rs:30:5
       |
    30 |     waiting: Arc<Mutex<BTreeMap<u64, oneshot::Sender<u32>>>>,
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: inserted here, an entry its caller never removes stays until the map drops
      --> tools/dylint/examples/unbounded_pending_map.rs:99:9
       |
    99 |         waiting.insert(key, sender);
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: bound the map by age or count at insertion, evict with `retain` or `pop_first`, or remove each entry in `Drop` of a guard its caller holds

    warning: `waiting` holds `tokio::sync::oneshot::Sender<u32>` entries that only a keyed `remove` deletes
       --> tools/dylint/examples/unbounded_pending_map.rs:65:5
        |
     65 |     waiting: Mutex<BTreeMap<u64, oneshot::Sender<u32>>>,
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: inserted here, an entry its caller never removes stays until the map drops
       --> tools/dylint/examples/unbounded_pending_map.rs:144:126
        |
    144 | ...t::Sender<u32>> { self.waiting.with_mut(|waiting| waiting.insert(key, sender)) }
        |                                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: bound the map by age or count at insertion, evict with `retain` or `pop_first`, or remove each entry in `Drop` of a guard its caller holds
    ");
}

fn unending_join(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `for_each` drains a `WatchStream` that ends only when every sender drops, and a sender outlives the join
      --> tools/dylint/examples/unending_join.rs:21:13
       |
    21 | /             updates.for_each(|update| {
    22 | |                 black_box(update);
    23 | |                 ready(())
    24 | |             }),
       | |______________^
       |
    note: sender held past the join
      --> tools/dylint/examples/unending_join.rs:18:40
       |
    18 |         let updates = WatchStream::new(self.0.subscribe());
       |                                        ^^^^^^
       = help: race the drain against its partner through `future::select` to drop the drain when its partner completes
       = note: `#[warn(unending_join)]` on by default

    warning: `try_for_each` drains a `WatchStream` that ends only when every sender drops, and a sender outlives the join
      --> tools/dylint/examples/unending_join.rs:33:23
       |
    33 |   ...   let relayed = stream::iter(token.map(|token| updates.map(move |update| Ok::<_, Infallible>((token, update))))).flatten().try...
       |  _____________________^
    34 | | ...       black_box(sent);
    35 | | ...       ready(Ok(()))
    36 | | ...   });
       | |________^
       |
    note: sender held past the join
      --> tools/dylint/examples/unending_join.rs:32:49
       |
    32 |         let updates = WatchStream::from_changes(self.0.subscribe());
       |                                                 ^^^^^^
       = help: race the drain against its partner through `future::select` to drop the drain when its partner completes

    warning: `count` drains a `BroadcastStream` that ends only when every sender drops, and a sender outlives the join
      --> tools/dylint/examples/unending_join.rs:63:35
       |
    63 |     let (seen, ()) = tokio::join!(BroadcastStream::new(received).count(), task::yield_now());
       |                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: sender held past the join
      --> tools/dylint/examples/unending_join.rs:62:10
       |
    62 |     let (sender, received) = broadcast::channel::<u32>(4);
       |          ^^^^^^
       = help: race the drain against its partner through `future::select` to drop the drain when its partner completes
    ");
}

// --- [COMPOSITION] ---------------------------------------------------------------------

snapshots!(
    async_lock_without_await,
    blocking_call_in_async,
    detached_task,
    manual_abort_on_drop,
    manual_arc_swap,
    manual_cancellation_token,
    manual_stream,
    needless_boxed_future,
    optional_channel_sender,
    redundant_spawn,
    sequential_await,
    short_circuited_join,
    unbounded_pending_map,
    unending_join
);
