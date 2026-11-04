// --- [OPERATIONS] ----------------------------------------------------------------------

snapshots! {
fn blocking_call_in_async(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: blocking call to `std::thread::sleep` in async body
      --> tools/dylint/examples/blocking_call_in_async.rs:35:20
       |
    35 | async fn slept() { thread::sleep(Duration::ZERO); }
       |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = note: `#[warn(blocking_call_in_async)]` on by default

    warning: blocking call to `fs_err::read_to_string` in async body
      --> tools/dylint/examples/blocking_call_in_async.rs:41:63
       |
    41 | async fn module_function(path: &Path) -> io::Result<String> { fs_err::read_to_string(path) }
       |                                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    warning: blocking call to `std::fs::read` in async body
      --> tools/dylint/examples/blocking_call_in_async.rs:43:60
       |
    43 | async fn native_read(path: &Path) -> io::Result<Vec<u8>> { std::fs::read(path) }
       |                                                            ^^^^^^^^^^^^^^^^^^^

    warning: blocking call to `fs_err::PathExt::fs_err_metadata` in async body
      --> tools/dylint/examples/blocking_call_in_async.rs:45:54
       |
    45 | async fn described(path: &Path) -> io::Result<u64> { path.fs_err_metadata().map(|metadata| metadata.len()) }
       |                                                      ^^^^^^^^^^^^^^^^^^^^^^

    warning: blocking call to `std::io::BufRead::read_line` in async body
      --> tools/dylint/examples/blocking_call_in_async.rs:49:5
       |
    49 |     BufReader::new(file?).read_line(&mut line)?;
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    warning: blocking call to `fs_err::read` in async body
      --> tools/dylint/examples/blocking_call_in_async.rs:55:56
       |
    55 | async fn doubled(path: &Path) -> io::Result<Vec<u8>> { both!(path) }
       |                                                        ^^^^^^^^^^^

    warning: blocking call to `std::thread::scope` in async body
      --> tools/dylint/examples/blocking_call_in_async.rs:60:5
       |
    60 | /     thread::scope(|scope| {
    61 | |         scope.spawn(|| thread::sleep(Duration::ZERO));
    62 | |     });
       | |______^

    warning: blocking call to `std::io::Read::read` in async body
      --> tools/dylint/examples/blocking_call_in_async.rs:79:65
       |
    79 | async fn qualified_read(file: &mut File) -> io::Result<usize> { File::read(file, &mut [0; 1]) }
       |                                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    warning: blocking call to `std::sync::mpsc::Receiver::recv_timeout` in async body
      --> tools/dylint/examples/blocking_call_in_async.rs:91:118
       |
    91 | ...<u8, std::sync::mpsc::RecvTimeoutError> { received.recv_timeout(Duration::from_secs(1)) }
       |                                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    warning: blocking call to `std::net::TcpStream::connect_timeout` in async body
      --> tools/dylint/examples/blocking_call_in_async.rs:93:93
       |
    93 | ...Result<std::net::TcpStream> { std::net::TcpStream::connect_timeout(address, Duration::from_secs(1)) }
       |                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    warning: blocking call to `futures::executor::block_on` in async body
       --> tools/dylint/examples/blocking_call_in_async.rs:98:5
        |
     98 | /     futures::executor::block_on(async {
     99 | |         let state = std::rc::Rc::new(());
    100 | |         core::future::pending::<()>().await;
    101 | |         black_box(state);
    102 | |     });
        | |______^
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
       = note: `#[warn(detached_task)]` on by default

    warning: dropping `tokio::task::JoinHandle` detaches its task
      --> tools/dylint/examples/detached_task.rs:30:18
       |
    30 | fn statement() { tokio::spawn(async {}); }
       |                  ^^^^^^^^^^^^^^^^^^^^^^

    warning: dropping `tokio::task::JoinHandle` detaches its task
      --> tools/dylint/examples/detached_task.rs:34:5
       |
    34 |     tokio::task::spawn_local(async move { black_box(state) });
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    warning: dropping `tokio::task::JoinHandle` detaches its task
      --> tools/dylint/examples/detached_task.rs:37:59
       |
    37 | fn runtime_statement(runtime: &tokio::runtime::Runtime) { runtime.spawn(async {}); }
       |                                                           ^^^^^^^^^^^^^^^^^^^^^^^

    warning: dropping `tokio::task::JoinHandle` detaches its task
      --> tools/dylint/examples/detached_task.rs:43:16
       |
    43 | fn dropped() { drop(tokio::spawn(async {})); }
       |                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    warning: dropping `tokio::task::JoinHandle` detaches its task
      --> tools/dylint/examples/detached_task.rs:45:25
       |
    45 | fn finished() -> bool { tokio::spawn(async {}).is_finished() }
       |                         ^^^^^^^^^^^^^^^^^^^^^^

    warning: `tokio::task::JoinSet::detach_all` detaches every task in the set
      --> tools/dylint/examples/detached_task.rs:49:43
       |
    49 | fn detached(set: &mut JoinSet<u64>) { set.detach_all(); }
       |                                           ^^^^^^^^^^^^

    warning: dropping `tokio::task::JoinHandle` detaches its task
      --> tools/dylint/examples/detached_task.rs:52:5
       |
    52 |     spawned!(async {});
       |     ^^^^^^^^^^^^^^^^^^

    warning: dropping `tokio::task::JoinHandle` detaches its task
       --> tools/dylint/examples/detached_task.rs:100:5
        |
    100 |     let _ = tokio::spawn(async {});
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    warning: dropping `std::thread::JoinHandle` detaches its thread
       --> tools/dylint/examples/detached_task.rs:101:5
        |
    101 |     let _ = thread::spawn(|| 1_u8);
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    warning: dropping `tokio::task::JoinHandle` detaches its task
       --> tools/dylint/examples/detached_task.rs:117:5
        |
    117 |     ignored_tracker(tracker);
        |     ^^^^^^^^^^^^^^^^^^^^^^^^

    warning: dropping `tokio::task::JoinHandle` detaches its task
       --> tools/dylint/examples/detached_task.rs:118:5
        |
    118 |     ignored_future(tracker.track_future(async {}));
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    warning: dropping `std::thread::JoinHandle` detaches its thread
      --> tools/dylint/examples/detached_task.rs:39:15
       |
    39 | fn unread() { let _worker = thread::spawn(|| 1_i32); }
       |               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    warning: dropping `std::thread::JoinHandle` detaches its thread
      --> tools/dylint/examples/detached_task.rs:41:23
       |
    41 | async fn threaded() { let _worker = thread::spawn(|| 1_i32); }
       |                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    warning: dropping `std::thread::JoinHandle` detaches its thread
      --> tools/dylint/examples/detached_task.rs:94:13
       |
    94 |             let _worker = thread::spawn(|| 1_u8);
       |             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    ");
}

fn short_circuited_join(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `collect` over `join_all` keeps only the first error of the completed futures
      --> tools/dylint/examples/short_circuited_join.rs:47:65
       |
    47 | ...sult<Vec<u32>, io::Error> { future::join_all(ids.into_iter().map(fetch)).await.into_iter().collect() }
       |                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: collect the results into `validated::Validated<_, _>` and call `.ok()`
       = note: `#[warn(short_circuited_join)]` on by default

    warning: `try_collect` over `join_all` keeps only the first error of the completed futures
      --> tools/dylint/examples/short_circuited_join.rs:49:64
       |
    49 | ...lt<Vec<u32>, io::Error> { future::join_all(ids.into_iter().map(fetch)).await.into_iter().try_collect() }
       |                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: collect the results into `validated::Validated<_, _>` and call `.ok()`

    warning: `collect` over `buffered` keeps only the first error of the completed futures
      --> tools/dylint/examples/short_circuited_join.rs:62:77
       |
    62 | ...<u32>, io::Error> { stream::iter(ids).map(fetch).buffered(4).collect::<Vec<_>>().await.into_iter().collect() }
       |                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: collect the results into `validated::Validated<_, _>` and call `.ok()`

    warning: `collect` over `join_all` keeps only the first error of the completed futures
      --> tools/dylint/examples/short_circuited_join.rs:64:105
       |
    64 | ...:Error>>) -> Result<Vec<u32>, io::Error> { set.join_all().await.into_iter().collect() }
       |                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: collect the results into `validated::Validated<_, _>` and call `.ok()`
    ");
}

fn unending_join(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `for_each` over `WatchStream` can keep `futures::future::join` pending while a sender remains alive
      --> tools/dylint/examples/unending_join.rs:21:13
       |
    21 | /             updates.for_each(|update| {
    22 | |                 black_box(update);
    23 | |                 ready(())
    24 | |             }),
       | |______________^
       |
    note: sender held here
      --> tools/dylint/examples/unending_join.rs:18:40
       |
    18 |         let updates = WatchStream::new(self.0.subscribe());
       |                                        ^^^^^^
       = help: pin the futures, combine any remaining futures, and race `for_each` against them through `futures::future::select` in place of `futures::future::join`
       = note: `#[warn(unending_join)]` on by default

    warning: `count` over `BroadcastStream` can keep `join!` pending while a sender remains alive
      --> tools/dylint/examples/unending_join.rs:67:35
       |
    67 |     let (seen, ()) = tokio::join!(BroadcastStream::new(received).count(), task::yield_now());
       |                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: sender held here
      --> tools/dylint/examples/unending_join.rs:66:10
       |
    66 |     let (sender, received) = broadcast::channel::<u32>(4);
       |          ^^^^^^
       = help: pin the futures, combine any remaining futures, and race `count` against them through `futures::future::select` in place of `join!`

    warning: `count` over `BroadcastStream` can keep `futures::future::join` pending while a sender remains alive
      --> tools/dylint/examples/unending_join.rs:81:35
       |
    81 |     let (seen, ()) = future::join(BroadcastStream::new(received).count(), task::yield_now()).await;
       |                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: sender held here
      --> tools/dylint/examples/unending_join.rs:80:10
       |
    80 |     let (sender, received) = broadcast::channel::<u32>(4);
       |          ^^^^^^
       = help: pin the futures, combine any remaining futures, and race `count` against them through `futures::future::select` in place of `futures::future::join`

    warning: `count` over `BroadcastStream` can keep `futures::future::join` pending while a sender remains alive
       --> tools/dylint/examples/unending_join.rs:103:37
        |
    103 |     let (seen, sent) = future::join(BroadcastStream::new(received).count(), async { sender.send(1) }).await;
        |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: sender held here
       --> tools/dylint/examples/unending_join.rs:102:10
        |
    102 |     let (sender, received) = broadcast::channel::<u32>(4);
        |          ^^^^^^
        = help: pin the futures, combine any remaining futures, and race `count` against them through `futures::future::select` in place of `futures::future::join`

    warning: `count` over `BroadcastStream` can keep `futures::future::join` pending while a sender remains alive
       --> tools/dylint/examples/unending_join.rs:118:39
        |
    118 |         let (seen, ()) = future::join(BroadcastStream::new(received).count(), task::yield_now()).await;
        |                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: sender held here
       --> tools/dylint/examples/unending_join.rs:116:10
        |
    116 |     let (sender, received) = broadcast::channel::<u32>(4);
        |          ^^^^^^
        = help: pin the futures, combine any remaining futures, and race `count` against them through `futures::future::select` in place of `futures::future::join`

    warning: `count` over `WatchStream` can keep `futures::future::join` pending while a sender remains alive
       --> tools/dylint/examples/unending_join.rs:188:18
        |
    188 |     future::join(WatchStream::new(received).count(), task::yield_now()).await.0
        |                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: sender held here
       --> tools/dylint/examples/unending_join.rs:187:20
        |
    187 |     let received = sender.subscribe();
        |                    ^^^^^^
        = help: pin the futures, combine any remaining futures, and race `count` against them through `futures::future::select` in place of `futures::future::join`
    ");
}

}
