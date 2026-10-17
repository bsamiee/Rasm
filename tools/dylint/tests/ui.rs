#![feature(exit_status_error)]

use std::{io, process::ExitStatusError};

use thiserror::Error;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, derive_more::Display)]
enum Step {
    #[display("start")]
    Spawn,
    #[display("write a line a message decodes from")]
    Decode,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Error)]
enum Failure {
    #[error("cargo dylint did not {step}")]
    Io { step: Step, source: io::Error },
    #[error("cargo dylint failed")]
    Exit(#[source] ExitStatusError),
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[cfg(test)]
mod tests {
    use std::{
        collections::BTreeMap,
        process::{Command, Stdio},
    };

    use cargo_metadata::Message;

    use super::{Failure, Step};

    #[test]
    fn examples() -> Result<(), Failure> {
        let output = Command::new(env!("CARGO"))
            .args(["dylint", "--lib", env!("CARGO_PKG_NAME"), "--no-deps", "-p", env!("CARGO_PKG_NAME"), "--", "--examples", "--message-format=json", "--config", "build.warnings='warn'"])
            .stderr(Stdio::inherit())
            .output()
            .map_err(|source| Failure::Io { step: Step::Spawn, source })?;
        let findings = Message::parse_stream(output.stdout.as_slice()).try_fold(BTreeMap::<String, String>::new(), |mut findings, message| {
            if let Message::CompilerMessage(message) = message.map_err(|source| Failure::Io { step: Step::Decode, source })?
                && !message.message.spans.is_empty()
                && let Some(rendered) = message.message.rendered
            {
                let applicabilities = message
                    .message
                    .children
                    .iter()
                    .filter_map(|child| Some(format!("applicability {:?}: {}\n", child.spans.iter().find_map(|span| span.suggestion_applicability.as_ref())?, child.message)))
                    .collect::<String>();
                findings.entry(message.target.name).or_default().extend([rendered.trim_end(), "\n", &applicabilities, "\n"]);
            }
            Ok(findings)
        })?;
        insta::assert_snapshot!(findings.into_values().collect::<String>(), @r#"
        warning: `assert!` checks a fact of the program's own code at run time
          --> tools/dylint/examples/assertion.rs:20:5
           |
        20 |     assert!(values.len() <= LIMIT, "too many values");
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return a typed error for a failure the input causes, state the invariant in the types, or check it in a test or a `const` block
           = note: `#[warn(assertion)]` on by default

        warning: `debug_assert_eq!` checks a fact of the program's own code at run time
          --> tools/dylint/examples/assertion.rs:25:5
           |
        25 |     debug_assert_eq!(left.len(), right.len());
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return a typed error for a failure the input causes, state the invariant in the types, or check it in a test or a `const` block

        warning: `assert_ne!` checks a fact of the program's own code at run time
          --> tools/dylint/examples/assertion.rs:30:5
           |
        30 |     assert_ne!(left, right);
           |     ^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return a typed error for a failure the input causes, state the invariant in the types, or check it in a test or a `const` block

        warning: `assert_matches!` checks a fact of the program's own code at run time
          --> tools/dylint/examples/assertion.rs:35:5
           |
        35 |     core::assert_matches!(reply, Reply::Accepted(_));
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return a typed error for a failure the input causes, state the invariant in the types, or check it in a test or a `const` block

        warning: `assert_eq!` checks a fact of the program's own code at run time
          --> tools/dylint/examples/assertion.rs:40:5
           |
        40 |     assert_eq!(left, right, "lengths differ");
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return a typed error for a failure the input causes, state the invariant in the types, or check it in a test or a `const` block

        warning: `debug_assert!` checks a fact of the program's own code at run time
          --> tools/dylint/examples/assertion.rs:45:5
           |
        45 |     debug_assert!(!values.is_empty(), "no values");
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return a typed error for a failure the input causes, state the invariant in the types, or check it in a test or a `const` block

        warning: `debug_assert_ne!` checks a fact of the program's own code at run time
          --> tools/dylint/examples/assertion.rs:46:5
           |
        46 |     debug_assert_ne!(values.len(), LIMIT, "values fill the limit");
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return a typed error for a failure the input causes, state the invariant in the types, or check it in a test or a `const` block

        warning: `debug_assert_matches!` checks a fact of the program's own code at run time
          --> tools/dylint/examples/assertion.rs:51:5
           |
        51 |     core::debug_assert_matches!(reply, Reply::Rejected);
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return a typed error for a failure the input causes, state the invariant in the types, or check it in a test or a `const` block

        warning: this `tokio::sync::Mutex` never holds a guard across an `.await`
          --> tools/dylint/examples/async_mutex_without_await.rs:23:5
           |
        23 |     hits: Mutex<u64>,
           |     ^^^^^^^^^^^^^^^^
           |
        note: every lock site releases its guard before the next `.await`
          --> tools/dylint/examples/async_mutex_without_await.rs:29:24
           |
        29 |         let mut hits = self.hits.lock().await;
           |                        ^^^^^^^^^^^^^^^^
           = help: declare `std::sync::nonpoison::Mutex` in place of `tokio::sync::Mutex`
           = note: `#[warn(async_mutex_without_await)]` on by default

        warning: this `tokio::sync::RwLock` never holds a guard across an `.await`
          --> tools/dylint/examples/async_mutex_without_await.rs:24:5
           |
        24 |     samples: RwLock<Vec<u64>>,
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: every lock site releases its guard before the next `.await`
          --> tools/dylint/examples/async_mutex_without_await.rs:34:40
           |
        34 |     async fn add(&self, sample: u64) { self.samples.write().await.push(sample); }
           |                                        ^^^^^^^^^^^^^^^^^^^^
        35 |
        36 |     async fn sum(&self) -> u64 { self.samples.read().await.iter().sum() }
           |                                  ^^^^^^^^^^^^^^^^^^^
           = help: declare `std::sync::nonpoison::RwLock` in place of `tokio::sync::RwLock`

        warning: this `tokio::sync::Mutex` never holds a guard across an `.await`
          --> tools/dylint/examples/async_mutex_without_await.rs:41:5
           |
        41 |     count: Arc<Mutex<u64>>,
           |     ^^^^^^^^^^^^^^^^^^^^^^
           |
        note: every lock site releases its guard before the next `.await`
          --> tools/dylint/examples/async_mutex_without_await.rs:50:25
           |
        50 |         let mut count = self.count.lock().await;
           |                         ^^^^^^^^^^^^^^^^^
           = help: declare `std::sync::nonpoison::Mutex` in place of `tokio::sync::Mutex`

        warning: this `tokio::sync::Mutex` never holds a guard across an `.await`
          --> tools/dylint/examples/async_mutex_without_await.rs:90:5
           |
        90 |     count: Mutex<u64>,
           |     ^^^^^^^^^^^^^^^^^
           |
        note: every lock site releases its guard before the next `.await`
          --> tools/dylint/examples/async_mutex_without_await.rs:96:25
           |
        96 |         let mut guard = count.lock().await;
           |                         ^^^^^^^^^^^^
           = help: declare `std::sync::nonpoison::Mutex` in place of `tokio::sync::Mutex`

        warning: this `tokio::sync::Mutex` never holds a guard across an `.await`
           --> tools/dylint/examples/async_mutex_without_await.rs:155:9
            |
        155 |     let total = Mutex::new(0_u64);
            |         ^^^^^
            |
        note: every lock site releases its guard before the next `.await`
           --> tools/dylint/examples/async_mutex_without_await.rs:156:20
            |
        156 |     let mut held = total.lock().await;
            |                    ^^^^^^^^^^^^
            = help: declare `std::sync::nonpoison::Mutex` in place of `tokio::sync::Mutex`

        warning: this `tokio::sync::Mutex` never holds a guard across an `.await`
           --> tools/dylint/examples/async_mutex_without_await.rs:19:1
            |
         19 | static TOTAL: Mutex<u64> = Mutex::const_new(0);
            | ^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: every lock site releases its guard before the next `.await`
           --> tools/dylint/examples/async_mutex_without_await.rs:162:24
            |
        162 | fn initial() -> u64 { *TOTAL.blocking_lock() }
            |                        ^^^^^^^^^^^^^^^^^^^^^
        163 |
        164 | async fn current() -> u64 { *TOTAL.lock().await }
            |                              ^^^^^^^^^^^^
            = help: declare `std::sync::nonpoison::Mutex` in place of `tokio::sync::Mutex`

        warning: `static TOTAL` holds `Mutex<u64>`, which mutates through a shared reference
          --> tools/dylint/examples/async_mutex_without_await.rs:19:8
           |
        19 | static TOTAL: Mutex<u64> = Mutex::const_new(0);
           |        ^^^^^
           |
           = help: build the value in `main` and pass it to each operation that reads or writes it
           = note: `#[warn(interior_mutable_static)]` on by default

        warning: `fs_err::read_to_string` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:28:63
           |
        28 | async fn module_function(path: &Path) -> io::Result<String> { fs_err::read_to_string(path) }
           |                                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`
           = note: `#[warn(blocking_call_in_async)]` on by default

        warning: `fs_err::File::open` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:31:20
           |
        31 |     let mut file = File::open(path)?;
           |                    ^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `std::io::Read::read_exact` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:33:5
           |
        33 |     file.read_exact(&mut header)?;
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `fs_err::File::sync_all` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:37:52
           |
        37 | async fn appended(path: &Path) -> io::Result<()> { OpenOptions::new().append(true).open(path)?.sync_all() }
           |                                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `fs_err::OpenOptions::open` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:37:52
           |
        37 | async fn appended(path: &Path) -> io::Result<()> { OpenOptions::new().append(true).open(path)?.sync_all() }
           |                                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `std::io::BufRead::read_line` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:41:5
           |
        41 |     BufReader::new(File::create_new(path)?).read_line(&mut line)?;
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `fs_err::File::create_new` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:41:20
           |
        41 |     BufReader::new(File::create_new(path)?).read_line(&mut line)?;
           |                    ^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `std::io::Read::read_exact` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:47:5
           |
        47 |     io::Read::read_exact(&mut File::open(path)?, &mut header)?;
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `fs_err::File::open` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:47:31
           |
        47 |     io::Read::read_exact(&mut File::open(path)?, &mut header)?;
           |                               ^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `std::io::Write::write_all` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:52:5
           |
        52 |     io::stdout().write_all(b"name: ")?;
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `std::io::Stdin::read_line` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:54:5
           |
        54 |     io::stdin().read_line(&mut line)?;
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `std::thread::park_timeout` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:58:21
           |
        58 | async fn parked() { thread::park_timeout(Duration::ZERO); }
           |                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `tokio::sync::mpsc::Receiver::blocking_recv` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:60:83
           |
        60 | async fn received(receiver: &mut mpsc::Receiver<u64>, lock: &Mutex<u64>) -> u64 { receiver.blocking_recv().unwrap_or_default().stric...
           |                                                                                   ^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `tokio::sync::Mutex::blocking_lock` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:60:140
           |
        60 | ...eiver.blocking_recv().unwrap_or_default().strict_add(*lock.blocking_lock()) }
           |                                                          ^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `fs_err::read` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:62:87
           |
        62 | async fn sizes(paths: &[PathBuf]) -> io::Result<Vec<usize>> { paths.iter().map(|path| fs_err::read(path).map(|bytes| bytes.len())).c...
           |                                                                                       ^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `fs_err::read_to_string` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:64:90
           |
        64 | fn deferred(path: PathBuf) -> JoinHandle<io::Result<String>> { tokio::spawn(async move { fs_err::read_to_string(path) }) }
           |                                                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `std::thread::sleep` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:76:20
           |
        76 | async fn slept() { thread::sleep(Duration::ZERO); }
           |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `std::thread::JoinHandle::join` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:78:44
           |
        78 | async fn joined() -> thread::Result<u64> { thread::spawn(|| 1_u64).join() }
           |                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `std::sync::mpsc::SyncSender::send` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:80:62
           |
        80 | async fn queued(sender: &channel::SyncSender<u64>) -> bool { sender.send(1).is_ok() }
           |                                                              ^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `std::process::Command::output` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:82:44
           |
        82 | async fn listed() -> io::Result<Vec<u8>> { Command::new("ls").output().map(|output| output.stdout) }
           |                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `std::net::TcpStream::connect` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:84:49
           |
        84 | async fn connected() -> io::Result<TcpStream> { TcpStream::connect("127.0.0.1:80") }
           |                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `fs_err::read` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:86:56
           |
        86 | async fn doubled(path: &Path) -> io::Result<Vec<u8>> { both!(path) }
           |                                                        ^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `std::thread::scope` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:88:55
           |
        88 | ...ult<Vec<u8>> { thread::scope(|scope| scope.spawn(|| fs_err::read(path)).join().unwrap_or_else(|_| Ok(Vec::new()))) }
           |                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `std::thread::ScopedJoinHandle::join` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:88:77
           |
        88 | async fn scoped(path: &Path) -> io::Result<Vec<u8>> { thread::scope(|scope| scope.spawn(|| fs_err::read(path)).join().unwrap_or_else...
           |                                                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `fs_err::PathExt::fs_err_metadata` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:90:54
           |
        90 | async fn described(path: &Path) -> io::Result<u64> { path.fs_err_metadata().map(|metadata| metadata.len()) }
           |                                                      ^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `std::fs::set_times` blocks the worker thread polling this async body
          --> tools/dylint/examples/blocking_call_in_async.rs:92:51
           |
        92 | async fn touched(path: &Path) -> io::Result<()> { fs::set_times(path, FileTimes::new()) }
           |                                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: await the async counterpart, or move the call into `tokio::task::spawn_blocking`

        warning: `fetch` of trait `Source` returns a boxed `dyn Future`
          --> tools/dylint/examples/boxed_future_return.rs:14:24
           |
        14 |     fn fetch(&self) -> Pin<Box<dyn Future<Output = u64> + '_>>;
           |                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: declare the method `async fn`, no `dyn` use of the trait needs a boxed future
           = note: `#[warn(boxed_future_return)]` on by default

        warning: `relay` of trait `Relay` returns a boxed `dyn Future`
          --> tools/dylint/examples/boxed_future_return.rs:22:24
           |
        22 |     fn relay(&self) -> Pin<Box<dyn Future<Output = u64> + Send + '_>>;
           |                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: declare `async fn` and bound `Send` callers with `T: Relay<relay(..): Send>`

        warning: `next` returns a boxed `dyn Future`
          --> tools/dylint/examples/boxed_future_return.rs:34:23
           |
        34 |     fn next(&self) -> Pin<Box<dyn Future<Output = u64> + '_>> { Box::pin(async move { self.start.strict_add(1) }) }
           |                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return `impl Future` with the same bounds in place of `Pin<Box<dyn Future>>`

        warning: `countdown` returns a boxed `dyn Future`
          --> tools/dylint/examples/boxed_future_return.rs:36:40
           |
        36 |     fn countdown(&self, steps: u64) -> Pin<Box<dyn Future<Output = u64> + '_>> {
           |                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: declare `async fn` and box the recursive call

        warning: `boxed` returns a boxed `dyn Future`
          --> tools/dylint/examples/boxed_future_return.rs:79:24
           |
        79 | fn boxed(work: u64) -> Pin<Box<dyn Future<Output = u64> + Send>> { Box::pin(async move { work }) }
           |                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return `impl Future` with the same bounds in place of `Pin<Box<dyn Future>>`

        warning: `aliased` returns a boxed `dyn Future`
          --> tools/dylint/examples/boxed_future_return.rs:81:27
           |
        81 | fn aliased(work: &u64) -> Boxed<'_> { Box::pin(async move { *work }) }
           |                           ^^^^^^^^^
           |
           = help: return `impl Future` with the same bounds in place of `Pin<Box<dyn Future>>`

        warning: `depth` returns a boxed `dyn Future`
          --> tools/dylint/examples/boxed_future_return.rs:83:25
           |
        83 | fn depth(level: u64) -> Pin<Box<dyn Future<Output = u64>>> {
           |                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: declare `async fn` and box the recursive call

        warning: `chosen` returns a boxed `dyn Future`
          --> tools/dylint/examples/boxed_future_return.rs:92:26
           |
        92 | fn chosen(fast: bool) -> Pin<Box<dyn Future<Output = u64>>> { if fast { Box::pin(async { 1 }) } else { Box::pin(future::ready(2)) } }
           |                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: move the branch inside one `async move` block and return `impl Future`

        warning: closure clones `path`, which nothing reads after the closure
          --> tools/dylint/examples/captured_clone.rs:24:113
           |
        24 | ...se().map_err(|source| ReadError { path: path.clone(), source }) }
           |                                      ------^^^^^^^^^^^^
           |                                      |
           |                                      help: move `path` into the closure: `path`
           |
           = note: `#[warn(captured_clone)]` on by default
        applicability MachineApplicable: move `path` into the closure

        warning: closure clones `name`, which nothing reads after the closure
          --> tools/dylint/examples/captured_clone.rs:26:92
           |
        26 | fn fallback(name: String, given: Option<String>) -> Option<String> { given.or_else(|| Some(name.clone())) }
           |                                                                                            ^^^^^^^^^^^^ help: move `name` into the closure: `name`
        applicability MachineApplicable: move `name` into the closure

        warning: closure clones `name`, which nothing reads after the closure
          --> tools/dylint/examples/captured_clone.rs:28:56
           |
        28 | fn deferred(name: String) -> String { block_on(async { name.clone() }) }
           |                                                        ^^^^^^^^^^^^ help: move `name` into the closure: `name`
        applicability MachineApplicable: move `name` into the closure

        warning: closure clones `name`, which nothing reads after the closure
          --> tools/dylint/examples/captured_clone.rs:30:89
           |
        30 | fn owned(name: String, given: Option<String>) -> Option<String> { given.or_else(|| Some(name.to_owned())) }
           |                                                                                         ^^^^^^^^^^^^^^^ help: move `name` into the closure: `name`
        applicability MachineApplicable: move `name` into the closure

        warning: closure clones `name`, which nothing reads after the closure
          --> tools/dylint/examples/captured_clone.rs:32:90
           |
        32 | fn called(name: String, given: Option<String>) -> Option<String> { given.or_else(|| Some(Clone::clone(&name))) }
           |                                                                                          ^^^^^^^^^^^^^^^^^^^ help: move `name` into the closure: `name`
        applicability MachineApplicable: move `name` into the closure

        warning: field `0` lets code outside the module write `Port` past the `u16` check of `Port::try_from`
          --> tools/dylint/examples/construction_invariant.rs:12:28
           |
        12 |     pub(crate) struct Port(pub u16);
           |                            ^^^^^^^
           |
        note: `Port::try_from` checks the `u16` here
          --> tools/dylint/examples/construction_invariant.rs:17:9
           |
        17 | ...   fn try_from(value: u16) -> Result<Self, Self::Error> { if value < 1024 { Err(RangeError(u32::from(value))) } else { Ok(Self(va...
           |       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: make the field private and give code outside the module an accessor that reads it
           = note: `#[warn(construction_invariant)]` on by default

        warning: field `width` lets code outside the module write `Window` past the `u32` check of `Window::new`
          --> tools/dylint/examples/construction_invariant.rs:22:9
           |
        22 |         pub width: u32,
           |         ^^^^^^^^^^^^^^
           |
        note: `Window::new` checks the `u32` here
          --> tools/dylint/examples/construction_invariant.rs:27:9
           |
        27 | ...   pub(crate) const fn new(width: u32, height: u32) -> Option<Self> { if width == 0 || height == 0 { None } else { Some(Self { wi...
           |       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: make the field private and give code outside the module an accessor that reads it

        warning: field `height` lets code outside the module write `Window` past the `u32` check of `Window::new`
          --> tools/dylint/examples/construction_invariant.rs:23:9
           |
        23 |         pub height: u32,
           |         ^^^^^^^^^^^^^^^
           |
        note: `Window::new` checks the `u32` here
          --> tools/dylint/examples/construction_invariant.rs:27:9
           |
        27 | ...   pub(crate) const fn new(width: u32, height: u32) -> Option<Self> { if width == 0 || height == 0 { None } else { Some(Self { wi...
           |       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: make the field private and give code outside the module an accessor that reads it

        warning: `Weight` built from a `u32` parameter that skips `Weight::try_from`
          --> tools/dylint/examples/construction_invariant.rs:42:62
           |
        42 |         pub(crate) const fn unchecked(weight: u32) -> Self { Self(weight) }
           |                                                              ^^^^^^^^^^^^
           |
        note: `Weight::try_from` checks the `u32` here
          --> tools/dylint/examples/construction_invariant.rs:38:9
           |
        38 |         fn try_from(value: u32) -> Result<Self, Self::Error> { if value > 500 { Err(RangeError(value)) } else { Ok(Self(value)) } }
           |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: build `Weight` through `Weight::try_from`

        warning: field `value` lets code outside the module write `Level` past the `u8` check of `Level::new`
          --> tools/dylint/examples/construction_invariant.rs:67:9
           |
        67 |         pub value: u8,
           |         ^^^^^^^^^^^^^
           |
        note: `Level::new` checks the `u8` here
          --> tools/dylint/examples/construction_invariant.rs:72:9
           |
        72 |         pub(crate) fn new(value: u8) -> Option<Self> { (value <= 10).then_some(Self { value, label: "level" }) }
           |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: make the field private and give code outside the module an accessor that reads it

        warning: `Weight` built from a `u32` parameter that skips `Weight::try_from`
          --> tools/dylint/examples/construction_invariant.rs:85:57
           |
        85 |     pub(crate) const fn weighed(value: u32) -> Weight { Weight(value) }
           |                                                         ^^^^^^^^^^^^^
           |
        note: `Weight::try_from` checks the `u32` here
          --> tools/dylint/examples/construction_invariant.rs:38:9
           |
        38 |         fn try_from(value: u32) -> Result<Self, Self::Error> { if value > 500 { Err(RangeError(value)) } else { Ok(Self(value)) } }
           |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: build `Weight` through `Weight::try_from`

        warning: `io::Error` built from text holds a failure callers cannot match
          --> tools/dylint/examples/contextless_io_error.rs:10:108
           |
        10 | ...ffer.is_empty() { Ok(0) } else { Err(io::Error::new(io::ErrorKind::InvalidData, "bad magic")) } }
           |                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: pass a typed error as the payload: `io::Error::new(kind, <thiserror value>)`
           = note: `#[warn(contextless_io_error)]` on by default

        warning: `StoreError` takes every `io::Error` through `From` and keeps no operation or subject
          --> tools/dylint/examples/contextless_io_error.rs:18:5
           |
        18 |     Read(#[from] io::Error),
           |     ^^^^
           |
           = help: declare one variant holding a fieldless operation enum and its subject beside the `io::Error` source and map at the call

        warning: `JournalError` takes every `io::Error` through `From` and keeps no operation or subject
          --> tools/dylint/examples/contextless_io_error.rs:23:1
           |
        23 | struct JournalError {
           | ^^^^^^^^^^^^^^^^^^^
           |
           = help: declare one variant holding a fieldless operation enum and its subject beside the `io::Error` source and map at the call

        warning: `io::Error` built from text holds a failure callers cannot match
          --> tools/dylint/examples/contextless_io_error.rs:54:19
           |
        54 |         "" => Err(io::Error::new(io::ErrorKind::InvalidData, "port text is empty")),
           |                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: pass a typed error as the payload: `io::Error::new(kind, <thiserror value>)`

        warning: `io::Error` built from text holds a failure callers cannot match
          --> tools/dylint/examples/contextless_io_error.rs:55:20
           |
        55 |         "-" => Err(io::Error::other(format!("port text {text} is a dash"))),
           |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: pass a typed error as the payload: `io::Error::new(kind, <thiserror value>)`

        warning: `io::Error` built from text holds a failure callers cannot match
          --> tools/dylint/examples/contextless_io_error.rs:65:62
           |
        65 |         Err(LoadError::Read { path: path.to_owned(), source: io::Error::other("path is empty") })
           |                                                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return a `thiserror` variant holding the subject and its typed source

        warning: `main` returns `Failure` to `Termination`, which prints its derived `Debug`
          --> tools/dylint/examples/debug_main_error.rs:13:14
           |
        13 | fn main() -> Result<(), Failure> {
           |              ^^^^^^^^^^^^^^^^^^^
           |
           = help: return `Result<_, std::error::Report<Failure>>` under `#![feature(error_reporter)]`, its `Debug` prints the error's `Display` and `sources()` chain
           = note: `#[warn(debug_main_error)]` on by default

        warning: dropping `tokio::task::JoinHandle` detaches its work
          --> tools/dylint/examples/detached_task.rs:20:52
           |
        20 |     fn release(&mut self, task: JoinHandle<u64>) { drop(task); }
           |                                                    ^^^^^^^^^^
           |
           = help: spawn on the `TaskTracker` or `JoinSet` its owner waits on at shutdown
           = note: `#[warn(detached_task)]` on by default

        warning: dropping `tokio::task::JoinHandle` detaches its work
          --> tools/dylint/examples/detached_task.rs:37:26
           |
        37 |     fn drop(&mut self) { tokio::spawn(async {}); }
           |                          ^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: spawn the release through `TaskTracker::spawn_on` with a `Handle` captured at construction, or hold a `DropGuard` whose token the release task awaits

        warning: `Drop::drop` calls `tokio::task::JoinHandle::abort` by hand
          --> tools/dylint/examples/detached_task.rs:61:9
           |
        61 |         self.task.abort();
           |         ^^^^^^^^^^^^^^^^^
           |
           = help: hold the task in a `tokio_util::task::AbortOnDropHandle`, which aborts it when dropped

        warning: `Drop::drop` calls `tokio::task::AbortHandle::abort` by hand
          --> tools/dylint/examples/detached_task.rs:62:9
           |
        62 |         self.abort.abort();
           |         ^^^^^^^^^^^^^^^^^^
           |
           = help: hold the abort handle in a `tokio_util::task::AbortOnDrop`, which aborts its task when dropped

        warning: `Drop::drop` calls `tokio_util::sync::CancellationToken::cancel` by hand
          --> tools/dylint/examples/detached_task.rs:63:9
           |
        63 |         self.token.cancel();
           |         ^^^^^^^^^^^^^^^^^^^
           |
           = help: hold the `DropGuard` from `CancellationToken::drop_guard`, which cancels its token when dropped

        warning: assigning over `tokio::task::JoinHandle` drops the previous handle and detaches its work
          --> tools/dylint/examples/detached_task.rs:75:35
           |
        75 | fn restart(worker: &mut Worker) { worker.task = tokio::spawn(async { 2 }); }
           |                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: spawn on the `TaskTracker` or `JoinSet` its owner waits on at shutdown

        warning: dropping `tokio::task::JoinHandle` detaches its work
          --> tools/dylint/examples/detached_task.rs:91:18
           |
        91 | fn statement() { tokio::spawn(async {}); }
           |                  ^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: spawn on the `TaskTracker` or `JoinSet` its owner waits on at shutdown

        warning: dropping `tokio::task::JoinHandle` detaches its work
          --> tools/dylint/examples/detached_task.rs:93:33
           |
        93 | fn on_handle(handle: &Handle) { handle.spawn(async {}); }
           |                                 ^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: spawn on the `TaskTracker` or `JoinSet` its owner waits on at shutdown

        warning: dropping `std::thread::JoinHandle` detaches its work
          --> tools/dylint/examples/detached_task.rs:95:15
           |
        95 | fn unread() { let _worker = thread::spawn(|| 1_i32); }
           |               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: spawn the thread inside `std::thread::scope`

        warning: dropping `tokio::task::JoinHandle` detaches its work
          --> tools/dylint/examples/detached_task.rs:97:24
           |
        97 | fn through_wrapper() { start(1); }
           |                        ^^^^^^^^
           |
           = help: spawn on the `TaskTracker` or `JoinSet` its owner waits on at shutdown

        warning: dropping `tokio::task::JoinHandle` detaches its work
          --> tools/dylint/examples/detached_task.rs:99:16
           |
        99 | fn dropped() { drop(tokio::spawn(async {})); }
           |                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: spawn on the `TaskTracker` or `JoinSet` its owner waits on at shutdown

        warning: dropping `tokio::task::JoinHandle` detaches its work
           --> tools/dylint/examples/detached_task.rs:103:5
            |
        103 |     drop(task);
            |     ^^^^^^^^^^
            |
            = help: spawn on the `TaskTracker` or `JoinSet` its owner waits on at shutdown

        warning: dropping `tokio::task::JoinHandle` detaches its work
           --> tools/dylint/examples/detached_task.rs:106:34
            |
        106 | fn abort_handle() { let _abort = tokio::spawn(async {}).abort_handle(); }
            |                                  ^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: spawn on the `TaskTracker` or `JoinSet` its owner waits on at shutdown

        warning: dropping `tokio::task::JoinHandle` detaches its work
           --> tools/dylint/examples/detached_task.rs:108:25
            |
        108 | fn finished() -> bool { tokio::spawn(async {}).is_finished() }
            |                         ^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: spawn on the `TaskTracker` or `JoinSet` its owner waits on at shutdown

        warning: dropping `tokio::task::JoinHandle` detaches its work
           --> tools/dylint/examples/detached_task.rs:110:38
            |
        110 | fn returned_abort() -> AbortHandle { tokio::spawn(async {}).abort_handle() }
            |                                      ^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: spawn on the `TaskTracker` or `JoinSet` its owner waits on at shutdown

        warning: dropping `std::thread::JoinHandle` detaches its work
           --> tools/dylint/examples/detached_task.rs:122:23
            |
        122 | async fn threaded() { let _worker = thread::spawn(|| 1_i32); }
            |                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: run the work through `tokio::task::spawn_blocking` and await its handle

        warning: dropping `std::thread::JoinHandle` detaches its work
           --> tools/dylint/examples/detached_task.rs:126:9
            |
        126 |         thread::spawn(|| 1_i32);
            |         ^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: run the work through `tokio::task::spawn_blocking` and await its handle

        warning: dropping `std::thread::JoinHandle` detaches its work
           --> tools/dylint/examples/detached_task.rs:132:5
            |
        132 |     thread::Builder::new().spawn(|| 1_i32)?;
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: spawn the thread inside `std::thread::scope`

        warning: `Route::url_path` repeats the body and signature of `Route::path`
           --> tools/dylint/examples/duplicate_body.rs:110:5
            |
        110 |     fn url_path(&self) -> String { self.segments.join("/") }
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: `Route::path` holds the same body
           --> tools/dylint/examples/duplicate_body.rs:108:5
            |
        108 |     fn path(&self) -> String { self.segments.join("/") }
            |     ^^^^^^^^^^^^^^^^^^^^^^^^
            = help: delete `Route::url_path` and call `Route::path` at each site
            = note: `#[warn(duplicate_body)]` on by default

        warning: every `Shape` impl repeats one body for `doubled`
           --> tools/dylint/examples/duplicate_body.rs:13:5
            |
         13 |     fn doubled(&self) -> f64;
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: each impl holds the same body
           --> tools/dylint/examples/duplicate_body.rs:116:5
            |
        116 |     fn doubled(&self) -> f64 { self.area() * 2.0 }
            |     ^^^^^^^^^^^^^^^^^^^^^^^^
        ...
        122 |     fn doubled(&self) -> f64 { self.area() * 2.0 }
            |     ^^^^^^^^^^^^^^^^^^^^^^^^
            = help: move `doubled` into a default method of `Shape` and remove it from each impl

        warning: `Display` impls for `Square` and `Circle` repeat one body for `fmt`
           --> tools/dylint/examples/duplicate_body.rs:126:5
            |
        126 |     fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("shape") }
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: repeats the body here
           --> tools/dylint/examples/duplicate_body.rs:130:5
            |
        130 |     fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("shape") }
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: expand the `Display` impls that repeat `fmt` from one local `macro_rules!` taking the implementing type

        warning: `Display` impls for `Labeled<u8>` and `Labeled<u16>` repeat one body for `fmt`
           --> tools/dylint/examples/duplicate_body.rs:160:5
            |
        160 |     fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str(self.name) }
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: repeats the body here
           --> tools/dylint/examples/duplicate_body.rs:164:5
            |
        164 |     fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str(self.name) }
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: implement `Display` once as a generic impl over `Labeled` in place of the impls that repeat `fmt`

        warning: `Extend` impls for `Square` and `Circle` repeat one body for `extend`
           --> tools/dylint/examples/duplicate_body.rs:134:5
            |
        134 | /     fn extend<I>(&mut self, items: I)
        135 | |     where I: IntoIterator<Item = C> {
            | |___________________________________^
            |
        note: repeats the body here
           --> tools/dylint/examples/duplicate_body.rs:141:5
            |
        141 | /     fn extend<I>(&mut self, items: I)
        142 | |     where I: IntoIterator<Item = C> {
            | |___________________________________^
            = help: expand the `Extend` impls that repeat `extend` from one local `macro_rules!` taking the implementing type

        warning: `Neg` impls for `Box<Square>` and `Box<Circle>` repeat one body for `neg`
           --> tools/dylint/examples/duplicate_body.rs:150:5
            |
        150 |     fn neg(self) -> f64 { -1.0 }
            |     ^^^^^^^^^^^^^^^^^^^
            |
        note: repeats the body here
           --> tools/dylint/examples/duplicate_body.rs:156:5
            |
        156 |     fn neg(self) -> f64 { -1.0 }
            |     ^^^^^^^^^^^^^^^^^^^
            = help: expand the `Neg` impls that repeat `neg` from one local `macro_rules!` taking the implementing type

        warning: `Catalog::labels` repeats the body and signature of `Catalog::names`
           --> tools/dylint/examples/duplicate_body.rs:170:5
            |
        170 |     fn labels(&self) -> Vec<String> { self.items.iter().map(|entry| entry.name.clone()).collect() }
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: `Catalog::names` holds the same body
           --> tools/dylint/examples/duplicate_body.rs:168:5
            |
        168 |     fn names(&self) -> Vec<String> { self.items.iter().map(|item| item.name.clone()).collect() }
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: delete `Catalog::labels` and call `Catalog::names` at each site

        warning: `Weighted` impls for `Fixed` and `Spread` repeat one body for `weight`
           --> tools/dylint/examples/duplicate_body.rs:228:5
            |
        228 |     fn weight(&self) -> u32 { 1 }
            |     ^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: repeats the body here
           --> tools/dylint/examples/duplicate_body.rs:232:5
            |
        232 |     fn weight(&self) -> u32 { 1 }
            |     ^^^^^^^^^^^^^^^^^^^^^^^
            = help: move `weight` into a default method of `Weighted` and remove it from the impls that repeat it

        warning: `sum` repeats the body and signature of `total`
           --> tools/dylint/examples/duplicate_body.rs:213:1
            |
        213 | fn sum(numbers: &[u32]) -> u32 { numbers.iter().sum() }
            | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: `total` holds the same body
           --> tools/dylint/examples/duplicate_body.rs:211:1
            |
        211 | fn total(values: &[u32]) -> u32 { values.iter().sum() }
            | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: delete `sum` and call `total` at each site

        warning: `second` repeats the body and signature of `first`
           --> tools/dylint/examples/duplicate_body.rs:221:1
            |
        221 | async fn second(value: u32) -> u32 { value.strict_add(1) }
            | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: `first` holds the same body
           --> tools/dylint/examples/duplicate_body.rs:219:1
            |
        219 | async fn first(value: u32) -> u32 { value.strict_add(1) }
            | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: delete `second` and call `first` at each site

        warning: every `Arity` impl gives `ARITY` the same value
          --> tools/dylint/examples/duplicate_constant.rs:6:5
           |
         6 |     const ARITY: usize;
           |     ^^^^^^^^^^^^^^^^^^
           |
        note: each impl states the value here
          --> tools/dylint/examples/duplicate_constant.rs:35:5
           |
        35 |     const ARITY: usize = 2;
           |     ^^^^^^^^^^^^^^^^^^
        ...
        39 |     const ARITY: usize = 2;
           |     ^^^^^^^^^^^^^^^^^^
           = help: state `ARITY` once as the default value in `Arity` and remove it from each impl
           = note: `#[warn(duplicate_constant)]` on by default

        warning: `with_timeout` overrides the `OTEL_EXPORTER_OTLP_TIMEOUT` variables `opentelemetry_otlp` reads
         --> tools/dylint/examples/environment_override.rs:8:126
          |
        8 | ...1/traces").with_protocol(Protocol::HttpBinary).with_timeout(Duration::from_secs(5)).build()
          |                                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
          |
          = help: remove the call and set `OTEL_EXPORTER_OTLP_TIMEOUT` in the process environment
          = note: `#[warn(environment_override)]` on by default

        warning: `with_protocol` overrides the `OTEL_EXPORTER_OTLP_PROTOCOL` variables `opentelemetry_otlp` reads
         --> tools/dylint/examples/environment_override.rs:8:90
          |
        8 |     SpanExporter::builder().with_http().with_endpoint("http://127.0.0.1:4318/v1/traces").with_protocol(Protocol::HttpBinary).with_tim...
          |                                                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
          |
          = help: remove the call and set `OTEL_EXPORTER_OTLP_PROTOCOL` in the process environment

        warning: `with_endpoint` overrides the `OTEL_EXPORTER_OTLP_ENDPOINT` variables `opentelemetry_otlp` reads
         --> tools/dylint/examples/environment_override.rs:8:41
          |
        8 |     SpanExporter::builder().with_http().with_endpoint("http://127.0.0.1:4318/v1/traces").with_protocol(Protocol::HttpBinary).with_tim...
          |                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
          |
          = help: remove the call and set `OTEL_EXPORTER_OTLP_ENDPOINT` in the process environment

        warning: `with_endpoint` overrides the `OTEL_EXPORTER_OTLP_ENDPOINT` variables `opentelemetry_otlp` reads
          --> tools/dylint/examples/environment_override.rs:11:61
           |
        11 | ...> { opentelemetry_otlp::WithExportConfig::with_endpoint(LogExporter::builder().with_http(), "http://127.0.0.1:4318/v1/logs").buil...
           |        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: remove the call and set `OTEL_EXPORTER_OTLP_ENDPOINT` in the process environment

        warning: exported `load` returns `std::io::Error`, an error type another crate defines
          --> tools/dylint/examples/foreign_error_return.rs:28:23
           |
        28 |     fn load(&self) -> Result<u16, io::Error>;
           |                       ^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return an error enum this crate defines, with a variant holding the `std::io::Error` as its `#[source]`
           = note: `#[warn(foreign_error_return)]` on by default

        warning: exported `open` returns `std::io::Error`, an error type another crate defines
          --> tools/dylint/examples/foreign_error_return.rs:70:26
           |
        70 |     pub fn open(self) -> io::Result<u16> { self.load() }
           |                          ^^^^^^^^^^^^^^^
           |
           = help: return an error enum this crate defines, with a variant holding the `std::io::Error` as its `#[source]`

        warning: exported `parse` returns `std::num::ParseIntError`, an error type another crate defines
          --> tools/dylint/examples/foreign_error_return.rs:85:29
           |
        85 | pub fn parse(text: &str) -> Result<u16, ParseIntError> { text.parse() }
           |                             ^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return an error enum this crate defines, with a variant holding the `std::num::ParseIntError` as its `#[source]`

        warning: exported `fetch` returns `std::num::ParseIntError`, an error type another crate defines
          --> tools/dylint/examples/foreign_error_return.rs:87:35
           |
        87 | pub async fn fetch(text: &str) -> Result<u16, ParseIntError> {
           |                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return an error enum this crate defines, with a variant holding the `std::num::ParseIntError` as its `#[source]`

        warning: exported `shared` returns `std::num::ParseIntError`, an error type another crate defines
          --> tools/dylint/examples/foreign_error_return.rs:92:30
           |
        92 | pub fn shared(text: &str) -> Result<u16, Arc<ParseIntError>> { parse(text).map_err(Arc::new) }
           |                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return an error enum this crate defines, with a variant holding the `std::num::ParseIntError` as its `#[source]`

        warning: exported `expanded` returns `std::num::ParseIntError`, an error type another crate defines
           --> tools/dylint/examples/foreign_error_return.rs:101:1
            |
        101 | reader!(expanded);
            | ^^^^^^^^^^^^^^^^^
            |
            = help: return an error enum this crate defines, with a variant holding the `std::num::ParseIntError` as its `#[source]`

        warning: exported `narrowed` returns `std::num::TryFromIntError`, an error type another crate defines
           --> tools/dylint/examples/foreign_error_return.rs:109:32
            |
        109 | pub fn narrowed(value: u32) -> Result<Port, TryFromIntError> { Port::try_from(value) }
            |                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return an error enum this crate defines, with a variant holding the `std::num::TryFromIntError` as its `#[source]`

        warning: exported `joined` returns `tokio::task::JoinError`, an error type another crate defines
           --> tools/dylint/examples/foreign_error_return.rs:111:35
            |
        111 | pub async fn joined(port: u16) -> Result<u16, JoinError> { tokio::spawn(async move { port }).await }
            |                                   ^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return an error enum this crate defines, with a variant holding the `tokio::task::JoinError` as its `#[source]`

        warning: exported `load` returns `std::io::Error`, an error type another crate defines
           --> tools/dylint/examples/foreign_error_return.rs:113:29
            |
        113 | pub fn load(path: &Path) -> io::Result<String> { fs_err::read_to_string(path) }
            |                             ^^^^^^^^^^^^^^^^^^
            |
            = help: return an error enum this crate defines, with a variant holding the `std::io::Error` as its `#[source]`

        warning: exported `configured` returns `clap::error::Error`, an error type another crate defines
           --> tools/dylint/examples/foreign_error_return.rs:115:42
            |
        115 | pub fn configured(arguments: &[&str]) -> Result<Options, clap::Error> { Options::try_parse_from(arguments) }
            |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return an error enum this crate defines, with a variant holding the `clap::error::Error` as its `#[source]`

        warning: tracing records values interpolated into text
         --> tools/dylint/examples/formatted_event.rs:6:5
          |
        6 |     tracing::info!("loaded {} rows from {path}", rows);
          |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
          |
          = help: record each value as its own field (`name = %value` or `name = ?value`) beside a constant message
          = note: `#[warn(formatted_event)]` on by default

        warning: tracing records a value rendered to text
          --> tools/dylint/examples/formatted_event.rs:12:27
           |
        12 |     tracing::warn!(port = format!("{port}"), "listening");
           |                           ^^^^^^^^^^^^^^^^^
           |
           = help: record the value itself with `%value` or `?value`, the subscriber formats it when it records the field

        warning: tracing records a value rendered to text
          --> tools/dylint/examples/formatted_event.rs:13:28
           |
        13 |     tracing::debug!(port = port.to_string(), "listening");
           |                            ^^^^^^^^^^^^^^^^
           |
           = help: record the value itself with `%value` or `?value`, the subscriber formats it when it records the field

        warning: tracing records a value rendered to text
          --> tools/dylint/examples/formatted_event.rs:15:55
           |
        15 |     let _span = tracing::info_span!("request", port = format!("{port}"));
           |                                                       ^^^^^^^^^^^^^^^^^
           |
           = help: record the value itself with `%value` or `?value`, the subscriber formats it when it records the field

        warning: `static HITS` holds `Atomic<u32>`, which mutates through a shared reference
          --> tools/dylint/examples/interior_mutable_static.rs:19:8
           |
        19 | static HITS: AtomicU32 = AtomicU32::new(0);
           |        ^^^^
           |
           = help: build the value in `main` and pass it to each operation that reads or writes it
           = note: `#[warn(interior_mutable_static)]` on by default

        warning: `static ROOT` holds `OnceLock<String>`, which mutates through a shared reference
          --> tools/dylint/examples/interior_mutable_static.rs:21:8
           |
        21 | static ROOT: OnceLock<String> = OnceLock::new();
           |        ^^^^
           |
           = help: declare `static ROOT: LazyLock<String> = LazyLock::new(|| GREETING.to_owned())`, its one use initializes it from no input

        warning: `static PREFIX` holds `OnceLock<String>`, which mutates through a shared reference
          --> tools/dylint/examples/interior_mutable_static.rs:23:8
           |
        23 | static PREFIX: OnceLock<String> = OnceLock::new();
           |        ^^^^^^
           |
           = help: build the value in `main` and pass it to each operation that reads or writes it

        warning: `static LABEL` holds `OnceLock<String>`, which mutates through a shared reference
          --> tools/dylint/examples/interior_mutable_static.rs:25:8
           |
        25 | static LABEL: OnceLock<String> = OnceLock::new();
           |        ^^^^^
           |
           = help: build the value in `main` and pass it to each operation that reads or writes it

        warning: `static ORIGIN` holds `OnceLock<String>`, which mutates through a shared reference
          --> tools/dylint/examples/interior_mutable_static.rs:27:12
           |
        27 | pub static ORIGIN: OnceLock<String> = OnceLock::new();
           |            ^^^^^^
           |
           = help: build the value in `main` and pass it to each operation that reads or writes it

        warning: `static COUNTER` holds `Atomic<u32>`, which mutates through a shared reference
          --> tools/dylint/examples/interior_mutable_static.rs:31:8
           |
        31 | static COUNTER: LazyLock<AtomicU32> = LazyLock::new(|| AtomicU32::new(1));
           |        ^^^^^^^
           |
           = help: build the value in `main` and pass it to each operation that reads or writes it

        warning: `static SHARED` holds `Arc<Mutex<Vec<u8>>>`, whose `Mutex<Vec<u8>>` mutates through a shared reference
          --> tools/dylint/examples/interior_mutable_static.rs:33:8
           |
        33 | static SHARED: LazyLock<Arc<Mutex<Vec<u8>>>> = LazyLock::new(|| Arc::new(Mutex::new(Vec::new())));
           |        ^^^^^^
           |
           = help: build the value in `main` and pass it to each operation that reads or writes it

        warning: `static VISITS` holds `Cell<u32>`, which mutates through a shared reference
          --> tools/dylint/examples/interior_mutable_static.rs:40:12
           |
        40 |     static VISITS: Cell<u32> = const { Cell::new(0) };
           |            ^^^^^^
           |
           = help: build the value in `main` and pass it to each operation that reads or writes it

        warning: `static CALLS` holds `Atomic<u32>`, which mutates through a shared reference
          --> tools/dylint/examples/interior_mutable_static.rs:44:10
           |
        44 | counter!(CALLS);
           |          ^^^^^
           |
           = help: build the value in `main` and pass it to each operation that reads or writes it

        warning: `static SEEN` holds `Atomic<u32>`, which mutates through a shared reference
          --> tools/dylint/examples/interior_mutable_static.rs:58:12
           |
        58 |     static SEEN: AtomicU32 = AtomicU32::new(0);
           |            ^^^^
           |
           = help: build the value in `main` and pass it to each operation that reads or writes it

        warning: `applied!` calls this closure in its expansion
          --> tools/dylint/examples/macro_closure_call.rs:34:25
           |
         5 | macro_rules! applied {
        ...
        34 |     black_box([applied!(|value: u32| value + 1, 1), applied!(increment, 2), applied!(handler.apply, 3), bound!(|value| value + 1, 4)...
           |                         ^^^^^^^^^^^^^^^^^^^^^^
           |
        note: the expansion calls the closure here
          --> tools/dylint/examples/macro_closure_call.rs:7:9
           |
         7 |         ($transform)($value)
           |         ^^^^^^^^^^^^^^^^^^^^
        ...
        34 |     black_box([applied!(|value: u32| value + 1, 1), applied!(increment, 2), applied!(handler.apply, 3), bound!(|value| value + 1, 4)...
           |                ----------------------------------- in this macro invocation
           = help: match the closure in the macro's matcher as `|$<param>:pat_param| $<body>:expr` and transcribe the body with each parameter bound to its argument
           = note: `#[warn(macro_closure_call)]` on by default

        warning: `snapshot` locks an `Arc` that every write replaces whole
          --> tools/dylint/examples/manual_arc_swap.rs:20:5
           |
        20 |     snapshot: RwLock<Arc<Config>>,
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: the `Arc` is replaced here
          --> tools/dylint/examples/manual_arc_swap.rs:66:38
           |
        66 |     fn update(&self, next: Config) { *self.snapshot.write() = Arc::new(next); }
           |                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: hold an `arc_swap::ArcSwap`, read it with `load_full()`, and replace it with `store(Arc::new(..))` or `rcu(..)`
           = note: `#[warn(manual_arc_swap)]` on by default

        warning: `latest` locks an `Arc` that every write replaces whole
          --> tools/dylint/examples/manual_arc_swap.rs:25:5
           |
        25 |     latest: Mutex<Arc<Config>>,
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: the `Arc` is replaced here
          --> tools/dylint/examples/manual_arc_swap.rs:72:38
           |
        72 |     fn update(&self, next: Config) { self.latest.set(Arc::new(next)); }
           |                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: hold an `arc_swap::ArcSwap`, read it with `load_full()`, and replace it with `store(Arc::new(..))` or `rcu(..)`

        warning: `current` sends through a `watch::Sender` that no receiver subscribes to and every write replaces whole
          --> tools/dylint/examples/manual_arc_swap.rs:40:5
           |
        40 |     current: watch::Sender<Config>,
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: the watched value is replaced here
          --> tools/dylint/examples/manual_arc_swap.rs:92:48
           |
        92 |     fn update(&self, next: Config) -> Config { self.current.send_replace(next) }
           |                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: hold an `arc_swap::ArcSwap`, read it with `load()`, and replace it with `store(Arc::new(..))`

        warning: `current` sends through a `watch::Sender` that no receiver subscribes to and every write replaces whole
           --> tools/dylint/examples/manual_arc_swap.rs:45:5
            |
         45 |     current: watch::Sender<Config>,
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: the watched value is replaced here
           --> tools/dylint/examples/manual_arc_swap.rs:100:38
            |
        100 |     fn update(&self, next: Config) { self.current.send_modify(|current| *current = next); }
            |                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: hold an `arc_swap::ArcSwap`, read it with `load()`, and replace it with `store(Arc::new(..))`

        warning: `lock` locks an `Arc` that every write replaces whole
           --> tools/dylint/examples/manual_arc_swap.rs:127:9
            |
        127 |     let lock = Arc::new(Mutex::new(Arc::new(Config::default())));
            |         ^^^^
            |
        note: the `Arc` is replaced here
           --> tools/dylint/examples/manual_arc_swap.rs:129:5
            |
        129 |     writer.set(Arc::new(Config { port: 9 }));
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: hold an `arc_swap::ArcSwap`, read it with `load_full()`, and replace it with `store(Arc::new(..))` or `rcu(..)`

        warning: `sender` sends through a `watch::Sender` that no receiver subscribes to and every write replaces whole
           --> tools/dylint/examples/manual_arc_swap.rs:134:9
            |
        134 |     let sender = watch::Sender::new(Config::default());
            |         ^^^^^^
            |
        note: the watched value is replaced here
           --> tools/dylint/examples/manual_arc_swap.rs:135:5
            |
        135 |     sender.send_replace(Config { port: 10 });
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: hold an `arc_swap::ArcSwap`, read it with `load()`, and replace it with `store(Arc::new(..))`

        warning: `stop` is a one-way stop flag, every write sets `true` and a `while` loop polls it
          --> tools/dylint/examples/manual_cancellation_token.rs:11:5
           |
        11 |     stop: AtomicBool,
           |     ^^^^^^^^^^^^^^^^
           |
        note: the flag is set here
          --> tools/dylint/examples/manual_cancellation_token.rs:30:22
           |
        30 |     fn halt(&self) { self.stop.store(true, Ordering::Release); }
           |                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        note: the loop polls the flag here
          --> tools/dylint/examples/manual_cancellation_token.rs:34:16
           |
        34 |         while !self.stop.load(Ordering::Acquire) && steps < 8 {
           |                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: hold a `tokio_util::sync::CancellationToken`, call `cancel()` where the flag is set, and poll `token.is_cancelled()` in the loop condition
           = note: `#[warn(manual_cancellation_token)]` on by default

        warning: `open` is a one-way stop flag, every write sets `true` and a `while` loop polls it
          --> tools/dylint/examples/manual_cancellation_token.rs:23:5
           |
        23 |     open: AtomicBool,
           |     ^^^^^^^^^^^^^^^^
           |
        note: the flag is set here
          --> tools/dylint/examples/manual_cancellation_token.rs:73:5
           |
        73 |     gate.open.store(true, Ordering::Release);
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        note: the loop polls the flag here
          --> tools/dylint/examples/manual_cancellation_token.rs:74:12
           |
        74 |     while !gate.open.load(Ordering::Acquire) {
           |            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: hold a `tokio_util::sync::CancellationToken`, call `cancel()` where the flag is set, and poll `token.is_cancelled()` in the loop condition

        warning: `stop` is a one-way stop flag, every write sets `true` and a `while` loop polls it
          --> tools/dylint/examples/manual_cancellation_token.rs:80:9
           |
        80 |     let stop = Arc::new(AtomicBool::new(false));
           |         ^^^^
           |
        note: the flag is set here
          --> tools/dylint/examples/manual_cancellation_token.rs:90:5
           |
        90 |     stop.store(true, Ordering::Release);
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        note: the loop polls the flag here
          --> tools/dylint/examples/manual_cancellation_token.rs:84:16
           |
        84 |         while !flag.load(Ordering::Acquire) {
           |                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: hold a `tokio_util::sync::CancellationToken`, call `cancel()` where the flag is set, and poll `token.is_cancelled()` in the loop condition

        warning: `done` is a one-way stop flag, every write sets `true` and a `while` loop polls it
           --> tools/dylint/examples/manual_cancellation_token.rs:95:9
            |
         95 |     let done = AtomicBool::new(false);
            |         ^^^^
            |
        note: the flag is set here
           --> tools/dylint/examples/manual_cancellation_token.rs:102:9
            |
        102 |         done.store(true, Ordering::Release);
            |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        note: the loop polls the flag here
           --> tools/dylint/examples/manual_cancellation_token.rs:98:20
            |
         98 |             while !done.load(Ordering::Acquire) {
            |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: hold a `tokio_util::sync::CancellationToken`, call `cancel()` where the flag is set, and poll `token.is_cancelled()` in the loop condition

        warning: `stop` is a one-way stop flag, every write sets `true` and a `while` loop polls it
           --> tools/dylint/examples/manual_cancellation_token.rs:122:9
            |
        122 |     let stop = AtomicBool::new(false);
            |         ^^^^
            |
        note: the flag is set here
           --> tools/dylint/examples/manual_cancellation_token.rs:124:5
            |
        124 |     stop.store(true, Ordering::Release);
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        note: the loop polls the flag here
           --> tools/dylint/examples/manual_cancellation_token.rs:125:12
            |
        125 |     while !stop.load(Ordering::Acquire) && steps < 8 {
            |            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: hold a `tokio_util::sync::CancellationToken`, call `cancel()` where the flag is set, and await `token.cancelled()` or drive the loop through `token.run_until_cancelled(..)`

        warning: `stop` is a one-way stop flag, every write sets `true` and a `while` loop polls it
           --> tools/dylint/examples/manual_cancellation_token.rs:132:9
            |
        132 |     let stop = AtomicBool::new(false);
            |         ^^^^
            |
        note: the flag is set here
           --> tools/dylint/examples/manual_cancellation_token.rs:133:5
            |
        133 |     stop.store(true, Ordering::Release);
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        note: the loop polls the flag here
           --> tools/dylint/examples/manual_cancellation_token.rs:136:16
            |
        136 |         while !stop.load(Ordering::Acquire) && steps < 8 {
            |                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: hold a `tokio_util::sync::CancellationToken`, call `cancel()` where the flag is set, and await `token.cancelled()` or drive the loop through `token.run_until_cancelled(..)`

        warning: `Manual` implements `core::error::Error` by hand
          --> tools/dylint/examples/manual_error_impl.rs:12:1
           |
        12 | impl Error for Manual {}
           | ^^^^^^^^^^^^^^^^^^^^^
           |
           = help: derive `thiserror::Error` with an `#[error(..)]` message and `#[source]` on the wrapped error
           = note: `#[warn(manual_error_impl)]` on by default

        warning: `IntoIter` names an iterator adapter through a `fn` pointer in place of its closure
          --> tools/dylint/examples/manual_language_feature.rs:69:21
           |
        69 |     type IntoIter = Map<vec::IntoIter<u8>, fn(u8) -> u16>;
           |                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: declare the type as `impl Iterator<Item = ..>` and build it with a closure under `#![feature(impl_trait_in_assoc_type)]`
           = note: `#[warn(manual_language_feature)]` on by default

        warning: `Default` for `Retry` builds each field from a constant
          --> tools/dylint/examples/manual_language_feature.rs:89:28
           |
        89 |     fn default() -> Self { Self { attempts: 3, label: String::new() } }
           |                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: declare each value on its field as `field: T = value` and derive `Default` under `#![feature(default_field_values)]`

        warning: `Message` rebuilt field by field from `self` to change its type arguments
           --> tools/dylint/examples/manual_language_feature.rs:101:38
            |
        101 |     fn send(self) -> Message<Sent> { Message { state: PhantomData, body: self.body, id: self.id } }
            |                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: write the changed fields and the base under `#![feature(type_changing_struct_update)]`: `Message { state: PhantomData, ..self }`
        applicability MaybeIncorrect: write the changed fields and the base under `#![feature(type_changing_struct_update)]`

        warning: `..Default::default()` fills fields that declare their own default values
           --> tools/dylint/examples/manual_language_feature.rs:110:54
            |
        110 | fn limits(name: String) -> Limits { Limits { name, ..Default::default() } }
            |                                                      ^^^^^^^^^^^^^^^^^^ help: write `..` to take each field's declared default
        applicability MachineApplicable: write `..` to take each field's declared default

        warning: `as_str` derefs the scrutinee for its patterns
           --> tools/dylint/examples/manual_language_feature.rs:116:11
            |
        116 |     match owned.as_str() {
            |           ^^^^^^^^^^^^^^ help: match a reference to the value under `#![feature(deref_patterns)]`: `&owned`
        applicability MaybeIncorrect: match a reference to the value under `#![feature(deref_patterns)]`

        warning: `as_slice` derefs the scrutinee for its patterns
           --> tools/dylint/examples/manual_language_feature.rs:125:30
            |
        125 |     if let [first, second] = owned.as_slice() { first.strict_add(*second) } else { 0 }
            |                              ^^^^^^^^^^^^^^^^ help: match a reference to the value under `#![feature(deref_patterns)]`: `&owned`
        applicability MaybeIncorrect: match a reference to the value under `#![feature(deref_patterns)]`

        warning: `as_slice` derefs the scrutinee for its patterns
           --> tools/dylint/examples/manual_language_feature.rs:130:23
            |
        130 |     let [first, ..] = owned.as_slice() else { return 0 };
            |                       ^^^^^^^^^^^^^^^^ help: match a reference to the value under `#![feature(deref_patterns)]`: `&owned`
        applicability MaybeIncorrect: match a reference to the value under `#![feature(deref_patterns)]`

        warning: `as_deref` derefs the scrutinee for its patterns
           --> tools/dylint/examples/manual_language_feature.rs:135:11
            |
        135 |     match profile.name.as_deref() {
            |           ^^^^^^^^^^^^^^^^^^^^^^^ help: match a reference to the value under `#![feature(deref_patterns)]`: `&profile.name`
        applicability MaybeIncorrect: match a reference to the value under `#![feature(deref_patterns)]`

        warning: `&*` derefs the scrutinee for its patterns
           --> tools/dylint/examples/manual_language_feature.rs:143:11
            |
        143 |     match &**value {
            |           ^^^^^^^^ help: match a reference to the value under `#![feature(deref_patterns)]`: `value`
        applicability MaybeIncorrect: match a reference to the value under `#![feature(deref_patterns)]`

        warning: `&*` derefs the scrutinee for its patterns
           --> tools/dylint/examples/manual_language_feature.rs:152:11
            |
        152 |     match &*held {
            |           ^^^^^^ help: match a reference to the value under `#![feature(deref_patterns)]`: `&held`
        applicability MaybeIncorrect: match a reference to the value under `#![feature(deref_patterns)]`

        warning: associated type `IntoFuture` names a boxed `dyn Future`
           --> tools/dylint/examples/manual_language_feature.rs:163:23
            |
        163 |     type IntoFuture = Pin<Box<dyn Future<Output = u64> + Send>>;
            |                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: declare the type as `impl Future` with the same bounds under `#![feature(impl_trait_in_assoc_type)]`

        warning: `filter` and `find` split `self.words.iter()` on one predicate and its negation in two passes
          --> tools/dylint/examples/manual_partition.rs:35:133
           |
        35 | ...word| word.is_empty()).count(), self.words.iter().find(|word| !word.is_empty()).copied()) }
           |                                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: the other pass splits it here
          --> tools/dylint/examples/manual_partition.rs:35:75
           |
        35 |     fn split(&self) -> (usize, Option<&'static str>) { (self.words.iter().filter(|word| word.is_empty()).count(), self.words.iter()....
           |                                                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: split it once with `Iterator::partition` and read each side from its half
           = note: `#[warn(manual_partition)]` on by default

        warning: `filter` and `find` split `VERSIONS.iter().rev()` on one predicate and its negation in two passes
          --> tools/dylint/examples/manual_partition.rs:41:136
           |
        41 | ...oned().collect(), legacy: VERSIONS.iter().rev().find(|version| version.legacy()).cloned() }
           |                                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: the other pass splits it here
          --> tools/dylint/examples/manual_partition.rs:41:50
           |
        41 |     Lifecycle { preferred: VERSIONS.iter().rev().filter(|version| !version.legacy()).cloned().collect(), legacy: VERSIONS.iter().rev...
           |                                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: split it once with `Iterator::partition` and read each side from its half

        warning: `filter` and `filter` split `words.iter()` on one predicate and its negation in two passes
          --> tools/dylint/examples/manual_partition.rs:46:31
           |
        46 |     let filled = words.iter().filter(|text| !text.trim().is_empty()).copied().collect::<Vec<_>>();
           |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: the other pass splits it here
          --> tools/dylint/examples/manual_partition.rs:45:30
           |
        45 |     let blank = words.iter().filter(|word| word.trim().is_empty()).copied().collect::<Vec<_>>();
           |                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: split it once with `Iterator::partition` and read each side from its half

        warning: `filter` and `find` split `words.iter()` on one predicate and its negation in two passes
          --> tools/dylint/examples/manual_partition.rs:52:28
           |
        52 |     (counted, words.iter().find(|word| !short(word)).copied())
           |                            ^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: the other pass splits it here
          --> tools/dylint/examples/manual_partition.rs:51:32
           |
        51 |     let counted = words.iter().filter(short).count();
           |                                ^^^^^^^^^^^^^
           = help: split it once with `Iterator::partition` and read each side from its half

        warning: `ContentBlock::text` receives text `serde_json::to_string` writes
          --> tools/dylint/examples/manual_serialization.rs:15:71
           |
        15 | fn tried(port: &Port) -> Result<ContentBlock, serde_json::Error> { Ok(ContentBlock::text(serde_json::to_string(port)?)) }
           |                                                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: the value is serialized here
          --> tools/dylint/examples/manual_serialization.rs:15:90
           |
        15 | fn tried(port: &Port) -> Result<ContentBlock, serde_json::Error> { Ok(ContentBlock::text(serde_json::to_string(port)?)) }
           |                                                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: pass the value to `ContentBlock::json`, which serializes it and calls `ContentBlock::text`
           = note: `#[warn(manual_serialization)]` on by default

        warning: `ContentBlock::text` receives text `serde_json::to_string` writes
          --> tools/dylint/examples/manual_serialization.rs:19:8
           |
        19 |     Ok(ContentBlock::text(text))
           |        ^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: the value is serialized here
          --> tools/dylint/examples/manual_serialization.rs:18:16
           |
        18 |     let text = serde_json::to_string(port)?;
           |                ^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: pass the value to `ContentBlock::json`, which serializes it and calls `ContentBlock::text`

        warning: `ContentBlock::text` receives text `serde_json::to_string` writes
          --> tools/dylint/examples/manual_serialization.rs:22:45
           |
        22 | fn defaulted(port: &Port) -> ContentBlock { ContentBlock::text(serde_json::to_string(port).unwrap_or_default()) }
           |                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: the value is serialized here
          --> tools/dylint/examples/manual_serialization.rs:22:64
           |
        22 | fn defaulted(port: &Port) -> ContentBlock { ContentBlock::text(serde_json::to_string(port).unwrap_or_default()) }
           |                                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: pass the value to `ContentBlock::json`, which serializes it and calls `ContentBlock::text`

        warning: `Guard` runs its one closure on drop
         --> tools/dylint/examples/manual_std_method.rs:6:1
          |
        6 | struct Guard<F>(Option<F>)
          | ^^^^^^^^^^^^^^^
          |
          = help: hold `core::mem::DropGuard::new((), |()| ..)` and call `DropGuard::dismiss` where the guard is disarmed
          = note: `#[warn(manual_std_method)]` on by default

        warning: `Deferred` runs its one closure on drop
          --> tools/dylint/examples/manual_std_method.rs:19:1
           |
        19 | struct Deferred<F>(F)
           | ^^^^^^^^^^^^^^^^^^
           |
           = help: hold `core::mem::DropGuard::new((), |()| ..)` and call `DropGuard::dismiss` where the guard is disarmed

        warning: `Manual` runs its one closure on drop
          --> tools/dylint/examples/manual_std_method.rs:41:1
           |
        41 | struct Manual<F>(ManuallyDrop<F>)
           | ^^^^^^^^^^^^^^^^
           |
           = help: hold `core::mem::DropGuard::new((), |()| ..)` and call `DropGuard::dismiss` where the guard is disarmed

        warning: assignment negates the `bool` it writes
          --> tools/dylint/examples/manual_std_method.rs:60:31
           |
        60 | fn flipped(flag: &mut bool) { *flag = !*flag; }
           |                               ^^^^^^^^^^^^^^ help: call `bool::toggle`: `flag.toggle()`
        applicability MachineApplicable: call `bool::toggle`

        warning: assignment negates the `bool` it writes
          --> tools/dylint/examples/manual_std_method.rs:62:29
           |
        62 | fn xored(flag: &mut bool) { *flag ^= true; }
           |                             ^^^^^^^^^^^^^ help: call `bool::toggle`: `flag.toggle()`
        applicability MachineApplicable: call `bool::toggle`

        warning: `(0..n).map(f).collect()` builds a `Vec` from each index
          --> tools/dylint/examples/manual_std_method.rs:64:42
           |
        64 | fn squares(count: usize) -> Vec<usize> { (0..count).map(|index| index.strict_mul(index)).collect::<Vec<_>>() }
           |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `Vec::from_fn`: `Vec::from_fn(count, |index| index.strict_mul(index))`
        applicability MachineApplicable: call `Vec::from_fn`

        warning: `(0..n).map(f).collect()` builds a `Vec` from each index
          --> tools/dylint/examples/manual_std_method.rs:66:42
           |
        66 | fn labels(count: usize) -> Vec<String> { (0..count).map(|index| index.to_string()).collect() }
           |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `Vec::from_fn`: `Vec::from_fn(count, |index| index.to_string())`
        applicability MachineApplicable: call `Vec::from_fn`

        warning: a fallible conversion checks the length of a slice to view it as an array
          --> tools/dylint/examples/manual_std_method.rs:68:47
           |
        68 | fn viewed(slice: &[u8]) -> Option<&[u8; 4]> { <&[u8; 4]>::try_from(slice).ok() }
           |                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^-----
           |                                               |
           |                                               help: view the slice through the method returning `Option`: `slice.as_array()`
        applicability MachineApplicable: view the slice through the method returning `Option`

        warning: a fallible conversion checks the length of a slice to view it as an array
          --> tools/dylint/examples/manual_std_method.rs:70:59
           |
        70 | fn viewed_mut(slice: &mut [u8]) -> Option<&mut [u8; 2]> { slice.try_into().ok() }
           |                                                           ^^^^^^^^^^^^^^^^-----
           |                                                           |
           |                                                           help: view the slice through the method returning `Option`: `slice.as_mut_array()`
        applicability MachineApplicable: view the slice through the method returning `Option`

        warning: a fallible conversion checks the length of a slice to view it as an array
          --> tools/dylint/examples/manual_std_method.rs:72:46
           |
        72 | fn prefix(slice: &[u8]) -> Option<[u8; 2]> { slice[..2].try_into().ok() }
           |                                              ^^^^^^^^^^^^^^^^^^^^^-----
           |                                              |
           |                                              help: view the slice through the method returning `Option`: `slice.first_chunk().copied()`
        applicability MaybeIncorrect: view the slice through the method returning `Option`

        warning: a fallible conversion checks the length of a slice to view it as an array
          --> tools/dylint/examples/manual_std_method.rs:74:46
           |
        74 | fn suffix(slice: &[u8]) -> Option<[u8; 2]> { slice[slice.len() - 2..].try_into().ok() }
           |                                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^-----
           |                                              |
           |                                              help: view the slice through the method returning `Option`: `slice.last_chunk().copied()`
        applicability MaybeIncorrect: view the slice through the method returning `Option`

        warning: lossy decoding copies a byte buffer nothing reads afterwards
          --> tools/dylint/examples/manual_std_method.rs:76:40
           |
        76 | fn decoded(bytes: Vec<u8>) -> String { String::from_utf8_lossy(&bytes).into_owned() }
           |                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: decode the buffer in place: `String::from_utf8_lossy_owned(bytes)`
        applicability MachineApplicable: decode the buffer in place

        warning: lossy decoding copies a byte buffer nothing reads afterwards
          --> tools/dylint/examples/manual_std_method.rs:78:47
           |
        78 | fn decoded_output(output: Output) -> String { String::from_utf8_lossy(&output.stdout).into_owned() }
           |                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: decode the buffer in place: `String::from_utf8_lossy_owned(output.stdout)`
        applicability MachineApplicable: decode the buffer in place

        warning: `while let` reads `received` through `recv` one item at a time
          --> tools/dylint/examples/manual_stream.rs:23:11
           |
        23 |     while let Some(value) = received.recv().await {
           |           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: wrap it in `tokio_stream::wrappers::ReceiverStream` and state the loop with stream combinators
           = note: `#[warn(manual_stream)]` on by default

        warning: `while let` reads `lines` through `next_segment` one item at a time
          --> tools/dylint/examples/manual_stream.rs:31:11
           |
        31 |     while let Some(line) = lines.next_segment().await.inspect_err(|error| tracing::warn!(%error, "read failed")).ok().flatten() {
           |           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: wrap it in `tokio_stream::wrappers::SplitStream` and state the loop with stream combinators

        warning: `while let` reads `lines` through `next_line` one item at a time
          --> tools/dylint/examples/manual_stream.rs:38:11
           |
        38 |     while let Some(line) = lines.next_line().await? {
           |           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: wrap it in `tokio_stream::wrappers::LinesStream` and state the loop with stream combinators

        warning: `while let` reads `received` through `recv` one item at a time
          --> tools/dylint/examples/manual_stream.rs:45:11
           |
        45 |     while let Some(value) = received.recv().await {
           |           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: wrap it in `tokio_stream::wrappers::UnboundedReceiverStream` and state the loop with stream combinators

        warning: `while let` reads `set` through `join_next` one item at a time
          --> tools/dylint/examples/manual_stream.rs:53:11
           |
        53 |     while let Some(outcome) = set.join_next().await {
           |           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: wrap it in `tokio_stream::wrappers::JoinSetStream` and state the loop with stream combinators

        warning: `while let` reads `received` through `recv` one item at a time
           --> tools/dylint/examples/manual_stream.rs:121:11
            |
        121 |     while let Some(value) = received.recv().await {
            |           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: wrap it in `tokio_stream::wrappers::ReceiverStream` and state the loop with stream combinators

        warning: `as_display` returns `self` as `dyn Display`, which trait upcasting converts from `dyn Shape`
          --> tools/dylint/examples/manual_upcast.rs:11:29
           |
        11 |     fn as_display(&self) -> &dyn Display;
           |                             ^^^^^^^^^^^^
           |
           = help: delete the method and coerce `dyn Shape` to `dyn Display` where it is called, trait upcasting converts the object
           = note: `#[warn(manual_upcast)]` on by default

        warning: `as_named_display` returns `self` as `dyn Display`, which trait upcasting converts from `dyn Named`
          --> tools/dylint/examples/manual_upcast.rs:16:39
           |
        16 |     fn as_named_display(&mut self) -> &mut dyn Display;
           |                                       ^^^^^^^^^^^^^^^^
           |
           = help: delete the method and coerce `dyn Named` to `dyn Display` where it is called, trait upcasting converts the object

        warning: `into_display` returns `self` as `dyn Display`, which trait upcasting converts from `dyn Boxed`
          --> tools/dylint/examples/manual_upcast.rs:24:41
           |
        24 |     fn into_display(self: Box<Self>) -> Box<dyn Display>;
           |                                         ^^^^^^^^^^^^^^^^
           |
           = help: delete the method and coerce `dyn Boxed` to `dyn Display` where it is called, trait upcasting converts the object

        warning: `as_any` returns `self` as `dyn Any`, which trait upcasting converts from `dyn Plugin` once `Any` is a supertrait
          --> tools/dylint/examples/manual_upcast.rs:36:25
           |
        36 |     fn as_any(&self) -> &dyn Any;
           |                         ^^^^^^^^
           |
           = help: add `Any` to the supertraits of `Plugin`, delete the method, and coerce `dyn Plugin` to `dyn Any` where it is called

        warning: `as_shown` returns `self` as `dyn Display`, which trait upcasting converts from `dyn Defaulted`
          --> tools/dylint/examples/manual_upcast.rs:40:27
           |
        40 |     fn as_shown(&self) -> &dyn Display
           |                           ^^^^^^^^^^^^
           |
           = help: delete the method and coerce `dyn Defaulted` to `dyn Display` where it is called, trait upcasting converts the object

        warning: `Cell` renames `UnsafeCell`, which collides with no other item in scope
         --> tools/dylint/examples/needless_import_rename.rs:2:5
          |
        2 |     cell::UnsafeCell as Cell,
          |     ^^^^^^^^^^^^^^^^^^^^^^^^
          |
          = help: import `UnsafeCell` under its own name
          = note: `#[warn(needless_import_rename)]` on by default

        warning: `Flipped` renames `Reverse`, which collides with no other item in scope
         --> tools/dylint/examples/needless_import_rename.rs:3:11
          |
        3 |     cmp::{Reverse as Flipped, *},
          |           ^^^^^^^^^^^^^^^^^^
          |
          = help: import `Reverse` under its own name

        warning: `format` renames `fmt`, which collides with no other item in scope
         --> tools/dylint/examples/needless_import_rename.rs:4:11
          |
        4 |     fmt::{self as format, Error, Write as _},
          |           ^^^^^^^^^^^^^^
          |
          = help: import `fmt` under its own name

        warning: `taken` renames `take`, which collides with no other item in scope
         --> tools/dylint/examples/needless_import_rename.rs:6:5
          |
        6 |     mem::take as taken,
          |     ^^^^^^^^^^^^^^^^^^
          |
          = help: import `take` under its own name

        warning: `Map` renames `BTreeMap`, which collides with no other item in scope
          --> tools/dylint/examples/needless_import_rename.rs:14:5
           |
        14 |     collections::BTreeMap as Map,
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: import `BTreeMap` under its own name

        warning: `Shared` renames `Arc`, which collides with no other item in scope
          --> tools/dylint/examples/needless_import_rename.rs:18:5
           |
        18 |     sync::Arc as Shared,
           |     ^^^^^^^^^^^^^^^^^^^
           |
           = help: import `Arc` under its own name

        warning: `Span` renames `Duration`, which collides with no other item in scope
          --> tools/dylint/examples/needless_import_rename.rs:54:9
           |
        54 |     use core::time::Duration as Span;
           |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: import `Duration` under its own name

        warning: spawned task only awaits a `tokio::task::JoinHandle<u8>` it did not create
         --> tools/dylint/examples/nested_task.rs:8:82
          |
        8 | fn rewrapped_direct(task: JoinHandle<u8>) -> JoinHandle<Result<u8, JoinError>> { tokio::spawn(task) }
          |                                                                                  ^^^^^^^^^^^^^^^^^^
          |
        note: the task awaits the `tokio::task::JoinHandle<u8>` here
         --> tools/dylint/examples/nested_task.rs:8:95
          |
        8 | fn rewrapped_direct(task: JoinHandle<u8>) -> JoinHandle<Result<u8, JoinError>> { tokio::spawn(task) }
          |                                                                                               ^^^^
          = help: drop the task and await the `tokio::task::JoinHandle<u8>` where its result is read
          = note: `#[warn(nested_task)]` on by default

        warning: spawned task only awaits a `tokio::task::JoinHandle<std::result::Result<u8, tokio::task::JoinError>>` it did not create
          --> tools/dylint/examples/nested_task.rs:10:100
           |
        10 | ...r>>) -> JoinHandle<Result<u8, JoinError>> { tokio::spawn(async move { task.await? }) }
           |                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: the task awaits the `tokio::task::JoinHandle<std::result::Result<u8, tokio::task::JoinError>>` here
          --> tools/dylint/examples/nested_task.rs:10:126
           |
        10 | fn rewrapped_tried(task: JoinHandle<Result<u8, JoinError>>) -> JoinHandle<Result<u8, JoinError>> { tokio::spawn(async move { task.aw...
           |                                                                                                                              ^^^^
           = help: drop the task and await the `tokio::task::JoinHandle<std::result::Result<u8, tokio::task::JoinError>>` where its result is read

        warning: spawned task only awaits a `tokio::task::JoinHandle<u8>` it did not create
          --> tools/dylint/examples/nested_task.rs:12:96
           |
        12 | ...JoinHandle<Result<u16, JoinError>> { handle.spawn(async move { task.await.map(u16::from) }) }
           |                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: the task awaits the `tokio::task::JoinHandle<u8>` here
          --> tools/dylint/examples/nested_task.rs:12:122
           |
        12 | fn rewrapped_on(task: JoinHandle<u8>, handle: &Handle) -> JoinHandle<Result<u16, JoinError>> { handle.spawn(async move { task.await....
           |                                                                                                                          ^^^^
           = help: drop the task and await the `tokio::task::JoinHandle<u8>` where its result is read

        warning: `0` wraps a channel sender in `Option` though every use sends only when it is present
          --> tools/dylint/examples/optional_sender.rs:8:17
           |
         8 | struct Progress(Option<watch::Sender<u32>>);
           |                 ^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: the sender is used only when present here
          --> tools/dylint/examples/optional_sender.rs:30:9
           |
        30 | /         if let Some(sent) = &self.0 {
        31 | |             sent.send_modify(|step| *step = step.saturating_add(1));
        32 | |         }
           | |_________^
           = help: hold the sender always, a sender with no receiver accepts every send, and leave absence to the consumer that subscribes
           = note: `#[warn(optional_sender)]` on by default

        warning: `sent` wraps a channel sender in `Option` though every use sends only when it is present
          --> tools/dylint/examples/optional_sender.rs:12:5
           |
        12 |     sent: Option<broadcast::Sender<u32>>,
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: the sender is used only when present here
          --> tools/dylint/examples/optional_sender.rs:44:9
           |
        44 | /         if let Some(sent) = &self.sent {
        45 | |             black_box(sent.send(event).ok());
        46 | |         }
           | |_________^
           = help: hold the sender always, a sender with no receiver accepts every send, and leave absence to the consumer that subscribes

        warning: `sent` wraps a channel sender in `Option` though every use sends only when it is present
          --> tools/dylint/examples/optional_sender.rs:71:9
           |
        71 |     let sent = tracked.then(|| watch::Sender::new(0_u32));
           |         ^^^^
           |
        note: the sender is used only when present here
          --> tools/dylint/examples/optional_sender.rs:72:5
           |
        72 | /     if let Some(sent) = &sent {
        73 | |         sent.send_replace(1);
        74 | |     }
           | |_____^
           = help: hold the sender always, a sender with no receiver accepts every send, and leave absence to the consumer that subscribes

        warning: catch-all arm covers every `Signal` variant the arms above leave unnamed
          --> tools/dylint/examples/owned_catch_all.rs:47:9
           |
        47 |         _ => 1,
           |         ^
           |
           = help: name each variant this arm covers in an or-pattern, `name @ (A | B)` keeps the binding
           = note: `#[warn(owned_catch_all)]` on by default

        warning: catch-all arm covers every `Signal` variant the arms above leave unnamed
          --> tools/dylint/examples/owned_catch_all.rs:54:9
           |
        54 |         other => other,
           |         ^^^^^
           |
           = help: name each variant this arm covers in an or-pattern, `name @ (A | B)` keeps the binding

        warning: catch-all arm covers every `Signal` variant the arms above leave unnamed
          --> tools/dylint/examples/owned_catch_all.rs:61:9
           |
        61 |         Signal::Pause | _ => 2,
           |         ^^^^^^^^^^^^^^^^^
           |
           = help: name each variant this arm covers in an or-pattern, `name @ (A | B)` keeps the binding

        warning: catch-all arm covers every `Signal` variant the arms above leave unnamed
          --> tools/dylint/examples/owned_catch_all.rs:69:9
           |
        69 |         &_ => 5,
           |         ^^
           |
           = help: name each variant this arm covers in an or-pattern, `name @ (A | B)` keeps the binding

        warning: catch-all arm covers every `Invalid` variant the arms above leave unnamed
          --> tools/dylint/examples/owned_catch_all.rs:98:9
           |
        98 |         _ => 1,
           |         ^
           |
           = help: name each variant this arm covers in an or-pattern, `name @ (A | B)` keeps the binding

        warning: `..` covers `Window` fields the pattern leaves unnamed
           --> tools/dylint/examples/owned_catch_all.rs:110:9
            |
        110 |     let Window { start, .. } = window;
            |         ^^^^^^^^^^^^^^^^^^^^
            |
        help: name each field `..` covers as `field: _`
            |
        110 -     let Window { start, .. } = window;
        110 +     let Window { start, end: _, label: _ } = window;
            |
        applicability MachineApplicable: name each field `..` covers as `field: _`

        warning: `..` covers `Drag` fields the pattern leaves unnamed
           --> tools/dylint/examples/owned_catch_all.rs:121:9
            |
        121 |         Gesture::Drag { from, .. } => from,
            |         ^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        help: name each field `..` covers as `field: _`
            |
        121 -         Gesture::Drag { from, .. } => from,
        121 +         Gesture::Drag { from, to: _ } => from,
            |
        applicability MachineApplicable: name each field `..` covers as `field: _`

        warning: `..` covers `Pinch` fields the pattern leaves unnamed
           --> tools/dylint/examples/owned_catch_all.rs:122:9
            |
        122 |         Gesture::Pinch(first, ..) => first,
            |         ^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: write `_` in place of `..` for each field it covers

        warning: `..` covers `Token` fields the pattern leaves unnamed
           --> tools/dylint/examples/owned_catch_all.rs:136:9
            |
        136 |     let sealed::Token { id, .. } = token;
            |         ^^^^^^^^^^^^^^^^^^^^^^^^
            |
        help: name each field `..` covers as `field: _`
            |
        136 |     let sealed::Token { id, scope: _, .. } = token;
            |                             +++++++++
        applicability MachineApplicable: name each field `..` covers as `field: _`

        warning: `<Banner as Render>::render` and `<Footer as Render>::render` repeat the body of `<Page as Render>::render` with other values
          --> tools/dylint/examples/parameterizable_body.rs:76:5
           |
        76 |     fn render(&self) -> String { String::from("page") }
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: repeats the body here
          --> tools/dylint/examples/parameterizable_body.rs:80:5
           |
        80 |     fn render(&self) -> String { String::from("banner") }
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^
        ...
        84 |     fn render(&self) -> String { String::from("footer") }
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: replace `Page` and each other holder with one type holding the value and a `const` per former type
           = note: `#[warn(parameterizable_body)]` on by default

        warning: `retry_writes` repeats the body of `retry_reads` with other values
           --> tools/dylint/examples/parameterizable_body.rs:111:1
            |
        111 | const fn retry_reads(client: &Client) -> Policy { Policy::new(client, 3, TIMEOUT) }
            | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: repeats the body here
           --> tools/dylint/examples/parameterizable_body.rs:113:1
            |
        113 | const fn retry_writes(client: &Client) -> Policy { Policy::new(client, 5, TIMEOUT) }
            | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: call `Policy::new` with the differing value at each site of `retry_reads` and `retry_writes`

        warning: `total_circles` repeats the body of `total_squares` with another type in place of `Square`
           --> tools/dylint/examples/parameterizable_body.rs:115:1
            |
        115 | fn total_squares(shapes: &[Square]) -> f64 { shapes.iter().map(Shape::area).sum() }
            | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: repeats the body here
           --> tools/dylint/examples/parameterizable_body.rs:117:1
            |
        117 | fn total_circles(shapes: &[Circle]) -> f64 { shapes.iter().map(Shape::area).sum() }
            | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: make one function generic over `S: Shape` in place of `total_squares` and `total_circles`

        warning: `clock` repeats the body of `uptime` with another type in place of `Uptime`
           --> tools/dylint/examples/parameterizable_body.rs:153:1
            |
        153 | fn uptime() -> Uptime { Default::default() }
            | ^^^^^^^^^^^^^^^^^^^^^
            |
        note: repeats the body here
           --> tools/dylint/examples/parameterizable_body.rs:155:1
            |
        155 | fn clock() -> Clock { Default::default() }
            | ^^^^^^^^^^^^^^^^^^^
            = help: call `Default::default` directly in place of `uptime` and `clock`

        warning: `Interval` has no impl beyond `Debug` and every use builds or destructures it whole
         --> tools/dylint/examples/plain_record.rs:6:1
          |
        6 | struct Interval {
          | ^^^^^^^^^^^^^^^
          |
          = help: replace it with the tuple `(u32, u32)`, every use builds and destructures the tuple
          = note: `#[warn(plain_record)]` on by default

        warning: `Label` has no impl beyond `Debug` and every use builds or destructures it whole
          --> tools/dylint/examples/plain_record.rs:11:1
           |
        11 | struct Label {
           | ^^^^^^^^^^^^
           |
           = help: replace it with its field's type `std::string::String`, every use builds and destructures that value

        warning: `impl From<ParseFloatError> for ScaleError` wraps the source in `Parse` by hand
          --> tools/dylint/examples/redundant_error_conversion.rs:68:1
           |
        68 | impl From<ParseFloatError> for ScaleError {
           | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: this field holds the source
          --> tools/dylint/examples/redundant_error_conversion.rs:50:21
           |
        50 |     Parse(#[source] ParseFloatError),
           |                     ^^^^^^^^^^^^^^^
           = help: mark the field `#[from]` and remove the impl
           = note: `#[warn(redundant_error_conversion)]` on by default

        warning: `impl From<Utf8Error> for ScaleError` wraps the source in `Text` by hand
          --> tools/dylint/examples/redundant_error_conversion.rs:72:1
           |
        72 | impl From<Utf8Error> for ScaleError {
           | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: this field holds the source
          --> tools/dylint/examples/redundant_error_conversion.rs:54:9
           |
        54 |         source: Utf8Error,
           |         ^^^^^^^^^^^^^^^^^
           = help: mark the field `#[from]` and remove the impl

        warning: `ReadError` takes every `io::Error` through `From` and keeps no operation or subject
          --> tools/dylint/examples/redundant_error_conversion.rs:91:5
           |
        91 |     Io(#[source] io::Error),
           |     ^^
           |
           = help: declare one variant holding a fieldless operation enum and its subject beside the `io::Error` source and map at the call
           = note: `#[warn(contextless_io_error)]` on by default

        warning: `ManualError` implements `core::error::Error` by hand
           --> tools/dylint/examples/redundant_error_conversion.rs:105:1
            |
        105 | impl error::Error for ManualError {
            | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: derive `thiserror::Error` with an `#[error(..)]` message and `#[source]` on the wrapped error
            = note: `#[warn(manual_error_impl)]` on by default

        warning: `map_err` repeats the `From` conversion `?` applies
           --> tools/dylint/examples/redundant_error_conversion.rs:116:37
            |
        116 |     let text = str::from_utf8(bytes).map_err(Into::<PortError>::into)?;
            |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
        applicability MachineApplicable: remove the `map_err` call

        warning: `map_err` repeats the `From` conversion `?` applies
           --> tools/dylint/examples/redundant_error_conversion.rs:117:35
            |
        117 |     let port = text.trim().parse().map_err(PortError::from)?;
            |                                   ^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
        applicability MaybeIncorrect: remove the `map_err` call

        warning: `map_err` repeats the `From` conversion `?` applies
           --> tools/dylint/examples/redundant_error_conversion.rs:122:37
            |
        122 |     let text = str::from_utf8(bytes).map_err(PortError::Text)?;
            |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
        applicability MachineApplicable: remove the `map_err` call

        warning: `map_err` repeats the `From` conversion `?` applies
           --> tools/dylint/examples/redundant_error_conversion.rs:132:37
            |
        132 |     let text = str::from_utf8(bytes).map_err(LocatedError)?;
            |                                     ^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
        applicability MachineApplicable: remove the `map_err` call

        warning: `map_err` repeats the `From` conversion `?` applies
           --> tools/dylint/examples/redundant_error_conversion.rs:142:28
            |
        142 |     let text = text!(bytes).map_err(PortError::from)?;
            |                            ^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
        applicability MachineApplicable: remove the `map_err` call

        warning: `map_err` repeats the `From` conversion `?` applies
           --> tools/dylint/examples/redundant_error_conversion.rs:148:37
            |
        148 |     let text = str::from_utf8(bytes).map_err(E::from)?;
            |                                     ^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
        applicability MachineApplicable: remove the `map_err` call

        warning: `map_err` repeats the `From` conversion `?` applies
           --> tools/dylint/examples/redundant_error_conversion.rs:153:85
            |
        153 |     let ports = lines.iter().map(|line| line.parse()).collect::<Result<Vec<_>, _>>().map_err(PortError::Parse)?;
            |                                                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
        applicability MaybeIncorrect: remove the `map_err` call

        warning: `map_err` repeats the `From` conversion `?` applies
           --> tools/dylint/examples/redundant_error_conversion.rs:158:35
            |
        158 |     let port = text.parse::<u16>().map_err(ManualError)?;
            |                                   ^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
        applicability MaybeIncorrect: remove the `map_err` call

        warning: `map_err` repeats the `From` conversion `?` applies
           --> tools/dylint/examples/redundant_error_conversion.rs:164:117
            |
        164 | ...Error> { text.trim().parse::<u16>().map_err(PortError::from)? } }
            |                                       ^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
        applicability MaybeIncorrect: remove the `map_err` call

        warning: `map_err` repeats the `From` conversion `?` applies
           --> tools/dylint/examples/redundant_error_conversion.rs:169:36
            |
        169 |     let scale = text.trim().parse().map_err(ScaleError::Parse)?;
            |                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
        applicability MaybeIncorrect: remove the `map_err` call

        warning: `map_err` repeats the `From` conversion `?` applies
           --> tools/dylint/examples/redundant_error_conversion.rs:178:43
            |
        178 |             let port = text.trim().parse().map_err(<PortError as From<_>>::from)?;
            |                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
        applicability MaybeIncorrect: remove the `map_err` call

        warning: `map_err` repeats the `From` conversion `?` applies
           --> tools/dylint/examples/redundant_error_conversion.rs:182:48
            |
        182 |             let port: u16 = text.trim().parse().map_err(PortError::Parse)?;
            |                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
        applicability MaybeIncorrect: remove the `map_err` call

        warning: error variants `Retry`, `Text` of `PortError` restate the same fields
          --> tools/dylint/examples/redundant_error_conversion.rs:24:1
           |
        24 | enum PortError {
           | ^^^^^^^^^^^^^^
           |
        note: each variant holds the same fields
          --> tools/dylint/examples/redundant_error_conversion.rs:26:5
           |
        26 |     Text(#[from] Utf8Error),
           |     ^^^^
        ...
        30 |     Retry(#[source] Utf8Error),
           |     ^^^^^
           = help: declare one variant holding the shared fields and a fieldless kind whose message `derive_more::Display` derives
           = note: `#[warn(restated_type)]` on by default

        warning: trait `Store` has one implementor, `Memory`
           --> tools/dylint/examples/redundant_indirection.rs:6:1
            |
          6 | pub(crate) trait Store: Send + Sync {
            | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: `dyn Store` holds a `Memory` at each use
           --> tools/dylint/examples/redundant_indirection.rs:196:16
            |
        196 |     store: Arc<dyn Send + Store>,
            |                ^^^^^^^^^^^^^^^^
            = help: replace the trait with an inherent `impl Memory` block
            = note: `#[warn(redundant_indirection)]` on by default

        warning: trait `Clock` has one implementor, `Fixed`
          --> tools/dylint/examples/redundant_indirection.rs:10:1
           |
        10 | trait Clock {
           | ^^^^^^^^^^^
           |
           = help: replace the trait with an inherent `impl Fixed` block

        warning: trait `Lookup` has one implementor, `Borrowed<'_>`
           --> tools/dylint/examples/redundant_indirection.rs:38:1
            |
         38 | trait Lookup {
            | ^^^^^^^^^^^^
            |
        note: `dyn Lookup` holds a `Borrowed<'_>` at each use
           --> tools/dylint/examples/redundant_indirection.rs:227:19
            |
        227 | fn first(lookup: &dyn Lookup) -> Option<u32> { lookup.lookup(0) }
            |                   ^^^^^^^^^^
            = help: replace the trait with an inherent `impl Borrowed<'_>` block

        warning: trait `Doubled` has one implementor, `u32`
          --> tools/dylint/examples/redundant_indirection.rs:46:1
           |
        46 | trait Doubled {
           | ^^^^^^^^^^^^^
           |
           = help: delete the trait and state each method's body at its call sites on `u32`

        warning: newtype `Queue` wraps `Vec<u32>` and every method forwards to it
           --> tools/dylint/examples/redundant_indirection.rs:159:1
            |
        159 | struct Queue(Vec<u32>);
            | ^^^^^^^^^^^^
            |
        note: forwards to `Vec<u32>`
           --> tools/dylint/examples/redundant_indirection.rs:162:5
            |
        162 |     fn push(&mut self, value: u32) { self.0.push(value); }
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        163 |
        164 |     const fn len(&self) -> usize { self.0.len() }
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: use `Vec<u32>` in place of `Queue`

        warning: `Walker` holds `cx` by reference only to copy `cx.tcx`
          --> tools/dylint/examples/redundant_reference.rs:25:5
           |
        25 |     cx: &'cx Context<'tcx>,
           |     ^^^^^^^^^^^^^^^^^^^^^^
           |
        note: `cx.tcx` copied here
          --> tools/dylint/examples/redundant_reference.rs:95:55
           |
        95 |     const fn interner(&self) -> Interner<'tcx> { self.cx.tcx }
           |                                                       ^^^^^^
           = help: store the `Interner<'tcx>` of `cx.tcx` in place of `cx` and remove `'cx`
           = note: `#[warn(redundant_reference)]` on by default

        warning: `Shared` holds `settings` by reference only to copy `settings.port`
          --> tools/dylint/examples/redundant_reference.rs:29:5
           |
        29 |     settings: &'settings Settings,
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: `settings.port` copied here
          --> tools/dylint/examples/redundant_reference.rs:99:40
           |
        99 |     const fn port(&self) -> u16 { self.settings.port }
           |                                        ^^^^^^^^^^^^^
           = help: store the `u16` of `settings.port` in place of `settings`

        warning: `Derived` holds `settings` by reference only to copy `settings.port`
           --> tools/dylint/examples/redundant_reference.rs:68:5
            |
         68 |     settings: &'settings Settings,
            |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: `settings.port` copied here
           --> tools/dylint/examples/redundant_reference.rs:142:40
            |
        142 |     const fn port(&self) -> u16 { self.settings.port }
            |                                        ^^^^^^^^^^^^^
            = help: store the `u16` of `settings.port` in place of `settings` and remove `'settings`

        warning: `Tagged` holds `0` by reference only to copy `0.retries`
           --> tools/dylint/examples/redundant_reference.rs:88:26
            |
         88 | struct Tagged<'settings>(&'settings Settings);
            |                          ^^^^^^^^^^^^^^^^^^^
            |
        note: `0.retries` copied here
           --> tools/dylint/examples/redundant_reference.rs:158:42
            |
        158 |     const fn retries(&self) -> u8 { self.0.retries }
            |                                          ^^^^^^^^^
            = help: store the `u8` of `0.retries` in place of `0` and remove `'settings`

        warning: `scaled!` names the `Scale::scale` parameter `factor` as `by`
          --> tools/dylint/examples/renamed_macro_params.rs:33:15
           |
        33 |         scale(by) => u32::wrapping_mul;
           |               ^^
           |
        note: the method is expanded here
          --> tools/dylint/examples/renamed_macro_params.rs:32:5
           |
        32 | /     scaled! {
        33 | |         scale(by) => u32::wrapping_mul;
        34 | |         clamp(limit) => u32::min;
        35 | |     }
           | |_____^
           = help: name the parameter `factor` in the macro or its invocation, the trait owns each parameter name
           = note: `#[warn(renamed_macro_params)]` on by default

        warning: `offset!` names the `Scale::shift` parameter `offset` as `delta`
          --> tools/dylint/examples/renamed_macro_params.rs:27:25
           |
        25 | macro_rules! offset {
        26 |     () => {
        27 |         fn shift(&self, delta: u32) -> u32 { self.0 + delta }
           |                         ^^^^^
        ...
        37 |     offset!();
           |     --------- in this macro invocation
           |
        note: the method is expanded here
          --> tools/dylint/examples/renamed_macro_params.rs:37:5
           |
        37 |     offset!();
           |     ^^^^^^^^^
           = help: name the parameter `offset` in the macro or its invocation, the trait owns each parameter name

        warning: error `std::num::ParseIntError` rendered to text is stored where callers cannot match its cause
          --> tools/dylint/examples/rendered_error.rs:43:96
           |
        43 | fn field(text: &str) -> Result<u16, Failure> { parsed(text).map_err(|error| Failure { message: error.to_string() }) }
           |                                                                                                ^^^^^^^^^^^^^^^^^
           |
           = help: hold the error as the `#[source]` of a `thiserror` variant and render it where a log or view prints it
           = note: `#[warn(rendered_error)]` on by default

        warning: error `std::num::ParseIntError` rendered to text is stored where callers cannot match its cause
          --> tools/dylint/examples/rendered_error.rs:45:87
           |
        45 | fn wrapped(text: &str) -> Result<u16, Wrapped> { parsed(text).map_err(|error| Wrapped(format!("port: {error}"))) }
           |                                                                                       ^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: hold the error as the `#[source]` of a `thiserror` variant and render it where a log or view prints it

        warning: error `std::num::ParseIntError` rendered to text is stored where callers cannot match its cause
          --> tools/dylint/examples/rendered_error.rs:47:102
           |
        47 | fn converted(text: &str) -> Result<u16, Wrapped> { parsed(text).map_err(|error| Wrapped(String::from(error.to_string().as_str()))) }
           |                                                                                                      ^^^^^^^^^^^^^^^^^
           |
           = help: hold the error as the `#[source]` of a `thiserror` variant and render it where a log or view prints it

        warning: error `std::num::ParseIntError` rendered to text is stored where callers cannot match its cause
          --> tools/dylint/examples/rendered_error.rs:49:97
           |
        49 | fn constructed(text: &str) -> Result<u16, Failure> { parsed(text).map_err(|error| Failure::new(&error.to_string())) }
           |                                                                                                 ^^^^^^^^^^^^^^^^^
           |
           = help: hold the error as the `#[source]` of a `thiserror` variant and render it where a log or view prints it

        warning: `Result` error `String` is text that callers cannot match
          --> tools/dylint/examples/rendered_error.rs:52:37
           |
        52 |     let port = parsed(text).map_err(|error| error.to_string())?;
           |                                     ^^^^^^^
           |
           = help: return a `thiserror` enum with a variant per failure and its typed source
           = note: `#[warn(untyped_error)]` on by default

        warning: error `std::num::ParseIntError` rendered to text is stored where callers cannot match its cause
          --> tools/dylint/examples/rendered_error.rs:58:25
           |
        58 |         attempt.label = error.to_string();
           |                         ^^^^^^^^^^^^^^^^^
           |
           = help: hold the error as the `#[source]` of a `thiserror` variant and render it where a log or view prints it

        warning: error `std::num::ParseIntError` rendered to text is stored where callers cannot match its cause
          --> tools/dylint/examples/rendered_error.rs:65:23
           |
        65 |         let message = format!("{error:?}");
           |                       ^^^^^^^^^^^^^^^^^^^^
           |
           = help: hold the error as the `#[source]` of a `thiserror` variant and render it where a log or view prints it

        warning: error `std::num::ParseIntError` rendered to text is stored where callers cannot match its cause
          --> tools/dylint/examples/rendered_error.rs:70:78
           |
        70 | fn listed(error: &ParseIntError, messages: &mut Vec<String>) { messages.push(error.to_string()); }
           |                                                                              ^^^^^^^^^^^^^^^^^
           |
           = help: hold the error as the `#[source]` of a `thiserror` variant and render it where a log or view prints it

        warning: `Result` error `String` is text that callers cannot match
          --> tools/dylint/examples/rendered_error.rs:72:65
           |
        72 | fn closure(text: &str) -> Option<String> { parsed(text).map_err(|error| error.to_string()).err() }
           |                                                                 ^^^^^^^
           |
           = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `String` is text that callers cannot match
          --> tools/dylint/examples/rendered_error.rs:74:38
           |
        74 | fn errored(error: &ParseIntError) -> Result<u16, String> { Err(error.to_string()) }
           |                                      ^^^^^^^^^^^^^^^^^^^
           |
           = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: error `std::num::ParseIntError` rendered to text is returned where callers cannot match its cause
          --> tools/dylint/examples/rendered_error.rs:76:75
           |
        76 | fn collected(errors: &[ParseIntError]) -> Vec<String> { errors.iter().map(ToString::to_string).collect() }
           |                                                                           ^^^^^^^^^^^^^^^^^^^
           |
           = help: hold the error as the `#[source]` of a `thiserror` variant and render it where a log or view prints it

        warning: error `std::num::ParseIntError` rendered to text is returned where callers cannot match its cause
          --> tools/dylint/examples/rendered_error.rs:78:49
           |
        78 | fn described(error: &ParseIntError) -> String { error.to_string() }
           |                                                 ^^^^^^^^^^^^^^^^^
           |
           = help: hold the error as the `#[source]` of a `thiserror` variant and render it where a log or view prints it

        warning: error `std::num::ParseIntError` rendered to text is returned where callers cannot match its cause
          --> tools/dylint/examples/rendered_error.rs:83:19
           |
        83 |         message = error.to_string();
           |                   ^^^^^^^^^^^^^^^^^
           |
           = help: hold the error as the `#[source]` of a `thiserror` variant and render it where a log or view prints it

        warning: error `std::num::ParseIntError` rendered to text is compared by its wording
          --> tools/dylint/examples/rendered_error.rs:88:43
           |
        88 | fn equal(error: &ParseIntError) -> bool { error.to_string() == "invalid digit found in string" }
           |                                           ^^^^^^^^^^^^^^^^^
           |
           = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

        warning: error `std::io::Error` rendered to text is compared by its wording
          --> tools/dylint/examples/rendered_error.rs:90:43
           |
        90 | fn contained(error: &io::Error) -> bool { ToString::to_string(error).contains("denied") }
           |                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

        warning: error `std::num::ParseIntError` rendered to text is compared by its wording
          --> tools/dylint/examples/rendered_error.rs:93:11
           |
        93 |     match format!("{error}") {
           |           ^^^^^^^^^^^^^^^^^^
           |
           = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

        warning: error `std::num::ParseIntError` rendered to text is compared by its wording
          --> tools/dylint/examples/rendered_error.rs:99:77
           |
        99 | fn tested(error: &ParseIntError) -> usize { if let Some(("cannot", rest)) = error.to_string().split_once(' ') { rest.len() } else { ...
           |                                                                             ^^^^^^^^^^^^^^^^^
           |
           = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

        warning: error `std::num::ParseIntError` rendered to text is compared by its wording
           --> tools/dylint/examples/rendered_error.rs:102:19
            |
        102 |     let message = error.to_string();
            |                   ^^^^^^^^^^^^^^^^^
            |
            = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

        warning: error `std::num::ParseIntError` rendered to text is compared by its wording
           --> tools/dylint/examples/rendered_error.rs:106:84
            |
        106 | fn seen(error: &ParseIntError, known: &BTreeSet<String>) -> bool { known.contains(&error.to_string()) }
            |                                                                                    ^^^^^^^^^^^^^^^^^
            |
            = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

        warning: `Mode` rendered to text is compared by its wording
           --> tools/dylint/examples/rendered_error.rs:140:31
            |
        140 | fn fast(mode: Mode) -> bool { mode.to_string() == "fast" }
            |                               ^^^^^^^^^^^^^^^^
            |
            = help: compare or match the value itself in place of its text

        warning: error `std::num::ParseIntError` rendered to text is stored where callers cannot match its cause
           --> tools/dylint/examples/rendered_error.rs:144:62
            |
        144 | fn rejected(error: &ParseIntError) -> RefusedError { refused(error.to_string()) }
            |                                                              ^^^^^^^^^^^^^^^^^
            |
            = help: hold the error as the `#[source]` of a `thiserror` variant and render it where a log or view prints it

        warning: `io::Error` built from text holds a failure callers cannot match
           --> tools/dylint/examples/rendered_error.rs:146:49
            |
        146 | fn handed(error: &ParseIntError) -> io::Error { io::Error::other(error.to_string()) }
            |                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` variant holding the subject and its typed source
            = note: `#[warn(contextless_io_error)]` on by default

        warning: `Endpoint` restates `Origin`
          --> tools/dylint/examples/restated_type.rs:19:1
           |
        19 | struct Endpoint {
           | ^^^^^^^^^^^^^^^
           |
        note: `Origin` declares the same shape here
          --> tools/dylint/examples/restated_type.rs:13:1
           |
        13 | struct Origin {
           | ^^^^^^^^^^^^^
           = help: use `Origin` in place of `Endpoint`
           = note: `#[warn(restated_type)]` on by default

        warning: `Groups` restates `Users` with other field types
          --> tools/dylint/examples/restated_type.rs:45:1
           |
        45 | struct Groups {
           | ^^^^^^^^^^^^^
           |
        note: `Users` declares the fields here
          --> tools/dylint/examples/restated_type.rs:39:1
           |
        39 | struct Users {
           | ^^^^^^^^^^^^
           = help: make `Users` generic over the field types that differ and use it in place of `Groups`

        warning: `Members` restates `Paged<User>`
          --> tools/dylint/examples/restated_type.rs:57:1
           |
        57 | struct Members {
           | ^^^^^^^^^^^^^^
           |
        note: `Paged` declares the same shape here
          --> tools/dylint/examples/restated_type.rs:51:1
           |
        51 | struct Paged<T> {
           | ^^^^^^^^^^^^^^^
           = help: use `Paged<User>` in place of `Members`

        warning: every variant of `Event` restates the same fields
          --> tools/dylint/examples/restated_type.rs:63:1
           |
        63 | enum Event {
           | ^^^^^^^^^^
           |
           = help: hold the shared fields once beside a fieldless kind enum

        warning: variant `Fetch` restates `Origin`
          --> tools/dylint/examples/restated_type.rs:70:5
           |
        70 |     Fetch { host: String, port: u16 },
           |     ^^^^^
           |
           = help: hold `Origin` in `Fetch` in place of its fields

        warning: `Location` restates a subset of the fields of `Link`
           --> tools/dylint/examples/restated_type.rs:82:1
            |
         82 | struct Location {
            | ^^^^^^^^^^^^^^^
            |
        note: `Location` is built from the fields of `Link` here
           --> tools/dylint/examples/restated_type.rs:411:39
            |
        411 | fn located(link: &Link) -> Location { Location { host: link.host.clone(), path: link.path.clone() } }
            |                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: hold `Location` in `Link` in place of its fields

        warning: `AddressDto` restates `Address` member by member
           --> tools/dylint/examples/restated_type.rs:106:1
            |
        106 | struct AddressDto {
            | ^^^^^^^^^^^^^^^^^
            |
        note: `AddressDto` is converted from `Address` member by member here
           --> tools/dylint/examples/restated_type.rs:493:15
            |
        493 |     let dto = AddressDto { port: u16::from(address.port), timeout: u32::from(address.timeout) };
            |               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: make `Address` generic over the member types that differ and use it in place of `AddressDto`

        warning: `AccountRow` restates `Account` member by member
           --> tools/dylint/examples/restated_type.rs:118:1
            |
        118 | struct AccountRow {
            | ^^^^^^^^^^^^^^^^^
            |
        note: `AccountRow` is converted from `Account` member by member here
           --> tools/dylint/examples/restated_type.rs:374:15
            |
        374 |     let row = AccountRow { user: account.login.clone(), address: account.email.clone() };
            |               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: use `Account` in place of `AccountRow`

        warning: `Reading` restates `Celsius` member by member
           --> tools/dylint/examples/restated_type.rs:127:1
            |
        127 | struct Reading(f64);
            | ^^^^^^^^^^^^^^
            |
        note: `Reading` is converted from `Celsius` member by member here
           --> tools/dylint/examples/restated_type.rs:378:50
            |
        378 | const fn reading(celsius: &Celsius) -> Reading { Reading(celsius.0) }
            |                                                  ^^^^^^^^^^^^^^^^^^
            = help: use `Celsius` in place of `Reading`

        warning: `ShapeKind` restates the variants of `Shape` without their fields
           --> tools/dylint/examples/restated_type.rs:171:1
            |
        171 | enum ShapeKind {
            | ^^^^^^^^^^^^^^
            |
        note: `ShapeKind` mirrors `Shape` here
           --> tools/dylint/examples/restated_type.rs:355:9
            |
        355 | /         match self {
        356 | |             Self::Circle(_) => ShapeKind::Circle,
        357 | |             Self::Square(_) => ShapeKind::Square,
        358 | |         }
            | |_________^
            = help: match `Shape` in place of `ShapeKind`

        warning: `Order` restates `std::cmp::Ordering`
           --> tools/dylint/examples/restated_type.rs:183:1
            |
        183 | enum Order {
            | ^^^^^^^^^^
            |
            = help: use `std::cmp::Ordering` in place of `Order`

        warning: `Signal` restates `Phase` member by member
           --> tools/dylint/examples/restated_type.rs:196:1
            |
        196 | enum Signal {
            | ^^^^^^^^^^^
            |
        note: `Signal` is converted from `Phase` member by member here
           --> tools/dylint/examples/restated_type.rs:383:5
            |
        383 | /     match phase {
        384 | |         Phase::Start => Signal::Begin,
        385 | |         Phase::Stop => Signal::End,
        386 | |     }
            | |_____^
            = help: use `Phase` in place of `Signal`

        warning: `Invocation` restates `Notice<M, std::option::Option<P>>`
           --> tools/dylint/examples/restated_type.rs:220:1
            |
        220 | struct Invocation<M, P> {
            | ^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: `Notice` declares the same shape here
           --> tools/dylint/examples/restated_type.rs:214:1
            |
        214 | struct Notice<M, P> {
            | ^^^^^^^^^^^^^^^^^^^
            = help: use `Notice<M, std::option::Option<P>>` in place of `Invocation`

        warning: `Frame` restates `Message`
           --> tools/dylint/examples/restated_type.rs:239:1
            |
        239 | enum Frame {
            | ^^^^^^^^^^
            |
        note: `Message` declares the same shape here
           --> tools/dylint/examples/restated_type.rs:233:1
            |
        233 | enum Message {
            | ^^^^^^^^^^^^
            = help: use `Message` in place of `Frame`

        warning: `Bounds` restates `std::ops::Range<u32>`
           --> tools/dylint/examples/restated_type.rs:259:1
            |
        259 | struct Bounds {
            | ^^^^^^^^^^^^^
            |
            = help: use `std::ops::Range<u32>` in place of `Bounds`

        warning: `Captured` restates `std::process::Output`
           --> tools/dylint/examples/restated_type.rs:284:1
            |
        284 | struct Captured {
            | ^^^^^^^^^^^^^^^
            |
            = help: use `std::process::Output` in place of `Captured`

        warning: `Rejected` restates `std::sync::mpsc::TrySendError<T>`
           --> tools/dylint/examples/restated_type.rs:291:1
            |
        291 | enum Rejected<T> {
            | ^^^^^^^^^^^^^^^^
            |
            = help: use `std::sync::mpsc::TrySendError<T>` in place of `Rejected`

        warning: `Maybe` restates `std::option::Option<T>`
           --> tools/dylint/examples/restated_type.rs:297:1
            |
        297 | enum Maybe<T> {
            | ^^^^^^^^^^^^^
            |
            = help: use `std::option::Option<T>` in place of `Maybe`

        warning: `Outcome` restates `std::result::Result<T, E>`
           --> tools/dylint/examples/restated_type.rs:303:1
            |
        303 | enum Outcome<T, E> {
            | ^^^^^^^^^^^^^^^^^^
            |
            = help: use `std::result::Result<T, E>` in place of `Outcome`

        warning: error variants `Read`, `Write` of `ManifestError` restate the same fields
           --> tools/dylint/examples/restated_type.rs:318:1
            |
        318 | enum ManifestError {
            | ^^^^^^^^^^^^^^^^^^
            |
        note: each variant holds the same fields
           --> tools/dylint/examples/restated_type.rs:320:5
            |
        320 |     Read(#[source] io::Error),
            |     ^^^^
        321 |     #[error("cannot write the manifest")]
        322 |     Write(#[source] io::Error),
            |     ^^^^^
            = help: declare one variant holding the shared fields and a fieldless kind whose message `derive_more::Display` derives

        warning: error variants `Runtime`, `Serve` of `ServeError` restate the same fields
           --> tools/dylint/examples/restated_type.rs:328:1
            |
        328 | enum ServeError<X> {
            | ^^^^^^^^^^^^^^^^^^
            |
        note: each variant holds the same fields
           --> tools/dylint/examples/restated_type.rs:330:5
            |
        330 |     Runtime(#[source] io::Error),
            |     ^^^^^^^
        331 |     #[error("cannot serve the endpoint")]
        332 |     Serve(#[source] io::Error),
            |     ^^^^^
            = help: declare one variant holding the shared fields and a fieldless kind whose message `derive_more::Display` derives

        warning: error variants `Invalid`, `Truncated` of `SchemaError` restate the same fields
           --> tools/dylint/examples/restated_type.rs:338:1
            |
        338 | enum SchemaError {
            | ^^^^^^^^^^^^^^^^
            |
        note: each variant holds the same fields
           --> tools/dylint/examples/restated_type.rs:340:5
            |
        340 |     Invalid { path: String, line: u32 },
            |     ^^^^^^^
        341 |     #[error("{path} ends at line {line}")]
        342 |     Truncated { path: String, line: u32 },
            |     ^^^^^^^^^
            = help: declare one variant holding the shared fields and a fieldless kind whose message `derive_more::Display` derives

        warning: `Shim` restates `AddressDto`
           --> tools/dylint/examples/restated_type.rs:403:5
            |
        403 |     struct Shim {
            |     ^^^^^^^^^^^
            |
        note: `AddressDto` declares the same shape here
           --> tools/dylint/examples/restated_type.rs:106:1
            |
        106 | struct AddressDto {
            | ^^^^^^^^^^^^^^^^^
            = help: use `AddressDto` in place of `Shim`

        warning: `for` loop awaits futures built before it one at a time
          --> tools/dylint/examples/sequential_join.rs:77:5
           |
        77 |     for receiver in receivers {
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: each future is awaited here before the next one
          --> tools/dylint/examples/sequential_join.rs:78:17
           |
        78 |         total = receiver.await?.strict_add(total);
           |                 ^^^^^^^^^^^^^^
           = help: await `futures::future::join_all` over the futures, its results keep input order, or drive `futures::stream::iter(..).buffered(n)` to bound how many run at once
           = note: `#[warn(sequential_join)]` on by default

        warning: `while let` reads `set` through `join_next` one item at a time
           --> tools/dylint/examples/sequential_join.rs:120:11
            |
        120 |     while let Some(joined) = set.join_next().await {
            |           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: wrap it in `tokio_stream::wrappers::JoinSetStream` and state the loop with stream combinators
            = note: `#[warn(manual_stream)]` on by default

        warning: `bound` has one reader, `clamped`
          --> tools/dylint/examples/single_caller.rs:34:5
           |
        34 |     fn bound(&self) -> usize { self.width.strict_mul(SHARED) }
           |     ^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: read here
          --> tools/dylint/examples/single_caller.rs:32:49
           |
        32 |     fn clamped(&self) -> usize { self.width.min(self.bound()) }
           |                                                 ^^^^^^^^^^^^
           = help: move `bound` into `clamped` as a nested item
           = note: `#[warn(single_caller)]` on by default

        warning: `digits` has one reader, `parsed`
          --> tools/dylint/examples/single_caller.rs:55:1
           |
        55 | fn digits(text: &str) -> usize { text.chars().filter(char::is_ascii_digit).count() }
           | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: read here
          --> tools/dylint/examples/single_caller.rs:57:34
           |
        57 | fn parsed(text: &str) -> usize { digits(text).min(LIMIT) }
           |                                  ^^^^^^
           = help: move `digits` into `parsed` as a nested item

        warning: `LIMIT` has one reader, `parsed`
          --> tools/dylint/examples/single_caller.rs:7:1
           |
         7 | const LIMIT: usize = 8;
           | ^^^^^^^^^^^^^^^^^^
           |
        note: read here
          --> tools/dylint/examples/single_caller.rs:57:51
           |
        57 | fn parsed(text: &str) -> usize { digits(text).min(LIMIT) }
           |                                                   ^^^^^
           = help: move `LIMIT` into `parsed` as a nested item

        warning: `countdown` has one reader, `started`
          --> tools/dylint/examples/single_caller.rs:59:1
           |
        59 | fn countdown(steps: usize) -> usize { if steps == 0 { 0 } else { countdown(steps.strict_sub(1)) } }
           | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: read here
          --> tools/dylint/examples/single_caller.rs:61:37
           |
        61 | fn started(steps: usize) -> usize { countdown(steps) }
           |                                     ^^^^^^^^^
           = help: move `countdown` into `started` as a nested item

        warning: `BANNER` has one reader, `titled`
          --> tools/dylint/examples/single_caller.rs:11:1
           |
        11 | static BANNER: &str = "ports";
           | ^^^^^^^^^^^^^^^^^^^
           |
        note: read here
          --> tools/dylint/examples/single_caller.rs:69:35
           |
        69 | fn titled() -> String { format!("{BANNER}:") }
           |                                   ^^^^^^
           = help: move `BANNER` into `titled` as a nested item

        warning: `scaled` has one reader, `widened`
          --> tools/dylint/examples/single_caller.rs:83:1
           |
        83 | fn scaled(width: usize) -> usize { width.strict_add(1) }
           | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: read here
          --> tools/dylint/examples/single_caller.rs:91:18
           |
        91 | reader!(widened, scaled);
           |                  ^^^^^^
           = help: move `scaled` into `widened` as a nested item

        warning: `halved` has one reader, `narrowed`
           --> tools/dylint/examples/single_caller.rs:113:1
            |
        113 | fn halved(width: usize) -> usize { width.strict_div(2) }
            | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: read here
           --> tools/dylint/examples/single_caller.rs:119:42
            |
        119 |     fn narrowed(width: usize) -> usize { halved(width) }
            |                                          ^^^^^^
            = help: move `halved` into `narrowed` as a nested item

        warning: `Iterator` impls for `Countdown` and `Repeated` repeat one body for `next`
          --> tools/dylint/examples/single_use_wrapper.rs:42:5
           |
        42 |     fn next(&mut self) -> Option<u8> {
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: repeats the body here
          --> tools/dylint/examples/single_use_wrapper.rs:51:5
           |
        51 |     fn next(&mut self) -> Option<u8> {
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: expand the `Iterator` impls that repeat `next` from one local `macro_rules!` taking the implementing type
           = note: `#[warn(duplicate_body)]` on by default

        warning: `Joined` exists to format a value at its one construction
          --> tools/dylint/examples/single_use_wrapper.rs:5:8
           |
         5 | struct Joined<'items>(&'items [u8]);
           |        ^^^^^^
           |
        note: the one construction
          --> tools/dylint/examples/single_use_wrapper.rs:61:32
           |
        61 |     let joined = format!("{}", Joined(&values));
           |                                ^^^^^^
           = help: format at that site with `fmt::from_fn(move |formatter| ..)`
           = note: `#[warn(single_use_wrapper)]` on by default

        warning: `Countdown` holds iterator state for its one construction
          --> tools/dylint/examples/single_use_wrapper.rs:18:8
           |
        18 | struct Countdown {
           |        ^^^^^^^^^
           |
        note: the one construction
          --> tools/dylint/examples/single_use_wrapper.rs:65:23
           |
        65 |     let counted: u8 = Countdown { left: 3 }.sum();
           |                       ^^^^^^^^^^^^^^^^^^^^^
           = help: write the iteration at that site as a `gen move { .. }` block under `#![feature(gen_blocks)]`

        warning: `Lookup` holds flag `found` restating `value.is_some()`
          --> tools/dylint/examples/split_outcome.rs:52:5
           |
        52 |     found: bool,
           |     ^^^^^^^^^^^
           |
        note: every construction sets `found` from whether `value` holds a value
          --> tools/dylint/examples/split_outcome.rs:53:5
           |
        53 |     value: Option<u16>,
           |     ^^^^^^^^^^^^^^^^^^
           = help: delete `found` and read `value.is_some()` or match `value`
           = note: `#[warn(split_outcome)]` on by default

        warning: `Cached` holds flag `missing` restating `entry.is_none()`
          --> tools/dylint/examples/split_outcome.rs:58:5
           |
        58 |     missing: bool,
           |     ^^^^^^^^^^^^^
           |
        note: every construction sets `missing` from whether `entry` holds a value
          --> tools/dylint/examples/split_outcome.rs:59:5
           |
        59 |     entry: Option<String>,
           |     ^^^^^^^^^^^^^^^^^^^^^
           = help: delete `missing` and read `entry.is_none()` or match `entry`

        warning: `Parsed` holds failure `failure` beside success `port`
          --> tools/dylint/examples/split_outcome.rs:71:5
           |
        71 |     failure: Option<ParseIntError>,
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: every construction sets exactly one of `port` and `failure`
          --> tools/dylint/examples/split_outcome.rs:70:5
           |
        70 |     port: Option<u16>,
           |     ^^^^^^^^^^^^^^^^^
           = help: replace `port` and `failure` with one `Result`

        warning: `Reply` holds failure `refusal` beside success `value`
          --> tools/dylint/examples/split_outcome.rs:83:5
           |
        83 |     refusal: Option<Closed>,
           |     ^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: every construction sets exactly one of `value` and `refusal`
          --> tools/dylint/examples/split_outcome.rs:82:5
           |
        82 |     value: Option<u16>,
           |     ^^^^^^^^^^^^^^^^^^
           = help: replace `value` and `refusal` with one `Result`

        warning: array lists every variant of `Mode` in declaration order
          --> tools/dylint/examples/split_table.rs:32:24
           |
        32 | const ALL: [Mode; 2] = [Mode::Fast, Mode::Slow];
           |                        ^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: derive `strum::VariantArray` on `Mode` and read `Mode::VARIANTS`
           = note: `#[warn(split_table)]` on by default

        warning: `BTreeMap` holds one entry per variant of `Unit`
          --> tools/dylint/examples/split_table.rs:96:18
           |
        96 |     let counts = BTreeMap::from([(Unit::Meter, 0_u32), (Unit::Foot, 0)]);
           |                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: build the map with `enum_map::enum_map!` over `#[derive(enum_map::Enum)]` on `Unit`

        warning: array lists every variant of `Mode` in declaration order
           --> tools/dylint/examples/split_table.rs:101:117
            |
        101 | ...s, partial, [ALL.len(), HOT.len(), PRIORITY.len()], [Mode::Fast, Mode::Slow].contains(&Mode::Slow)));
            |                                                        ^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: derive `strum::VariantArray` on `Mode` and read `Mode::VARIANTS`

        warning: `factor` and `offset` each return one column of a table keyed by `Unit`
          --> tools/dylint/examples/split_table.rs:7:1
           |
         7 | enum Unit {
           | ^^^^^^^^^
           |
        note: each column matches every variant here
          --> tools/dylint/examples/split_table.rs:39:5
           |
        39 |     const fn factor(self) -> f64 {
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        ...
        46 |     const fn offset(self) -> f64 {
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: return every column from one match on `Unit`

        warning: field `kind` holds one of the tags `"build"`, `"test"` as text
          --> tools/dylint/examples/string_tag.rs:29:5
           |
        29 |     kind: String,
           |     ^^^^^^^^^^^^
           |
        note: branched on by its text here
          --> tools/dylint/examples/string_tag.rs:65:5
           |
        65 | /     match &job.kind {
        66 | |         "build" => build(job),
        67 | |         "test" => test(job),
        68 | |         _ => 0,
        69 | |     }
           | |_____^
           = help: declare an enum with a variant per tag deriving `strum::EnumString`, parse the text once where it enters, and match the variants
           = note: `#[warn(string_tag)]` on by default

        warning: field `state` holds one of the tags `"done"`, `"pending"` as text
          --> tools/dylint/examples/string_tag.rs:39:5
           |
        39 |     state: &'static str,
           |     ^^^^^^^^^^^^^^^^^^^
           |
        note: branched on by its text here
          --> tools/dylint/examples/string_tag.rs:74:67
           |
        74 | fn finished(steps: &[Step]) -> usize { steps.iter().filter(|step| step.state == DONE).count() }
           |                                                                   ^^^^^^^^^^^^^^^^^^
           = help: declare an enum with a variant per tag deriving `strum::EnumString`, parse the text once where it enters, and match the variants

        warning: `level` holds one of the tags `"warn"`, `"info"` as text
          --> tools/dylint/examples/string_tag.rs:78:11
           |
        78 | fn logged(level: &str, text: &str) -> String { if level == "warn" { text.to_uppercase() } else { text.to_owned() } }
           |           ^^^^^
           |
        note: branched on by its text here
          --> tools/dylint/examples/string_tag.rs:78:51
           |
        78 | fn logged(level: &str, text: &str) -> String { if level == "warn" { text.to_uppercase() } else { text.to_owned() } }
           |                                                   ^^^^^^^^^^^^^^^
           = help: declare an enum with a variant per tag deriving `strum::EnumString`, parse the text once where it enters, and match the variants

        warning: `mode` holds one of the tags `"fast"`, `"slow"`, `"pause"` as text
          --> tools/dylint/examples/string_tag.rs:91:9
           |
        91 |     let mode = "fast";
           |         ^^^^
           |
        note: branched on by its text here
          --> tools/dylint/examples/string_tag.rs:92:5
           |
        92 | /     match mode {
        93 | |         "fast" => input.len(),
        94 | |         "slow" => input.len().strict_mul(2),
        95 | |         PAUSE => input.len().strict_add(1),
        96 | |         _ => 0,
        97 | |     }
           | |_____^
           = help: declare an enum with a variant per tag deriving `strum::EnumString`, parse the text once where it enters, and match the variants

        warning: `tally` is keyed by text and addressed by the fixed keys `"ok"`, `"err"`
           --> tools/dylint/examples/string_tag.rs:148:9
            |
        148 |     let mut tally = BTreeMap::new();
            |         ^^^^^^^^^
            |
        note: addressed by a string literal here
           --> tools/dylint/examples/string_tag.rs:149:18
            |
        149 |     tally.insert("ok", words.len());
            |                  ^^^^
        150 |     tally.insert("err", 0);
            |                  ^^^^^
        151 |     tally.get("ok").copied().unwrap_or_default().strict_add(tally["err"])
            |               ^^^^                                                ^^^^^
            = help: key the collection by an enum with a variant per key, or hold a struct with a field per key

        warning: `waiting` holds `tokio::sync::oneshot::Sender<u32>` entries that only a keyed `remove` takes out
          --> tools/dylint/examples/unbounded_pending_map.rs:20:5
           |
        20 |     waiting: Mutex<BTreeMap<u64, oneshot::Sender<u32>>>,
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: entries enter here, and one whose caller never returns stays for the life of the map
          --> tools/dylint/examples/unbounded_pending_map.rs:68:9
           |
        68 |         self.waiting.lock().insert(key, sender);
           |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: bound the map by age or count where entries enter, evict with `retain` or `pop_first`, or remove each entry in the `Drop` of a guard its caller holds
           = note: `#[warn(unbounded_pending_map)]` on by default

        warning: `running` holds `tokio::task::JoinHandle<()>` entries that only a keyed `remove` takes out
          --> tools/dylint/examples/unbounded_pending_map.rs:25:5
           |
        25 |     running: Mutex<HashMap<u64, JoinHandle<()>>>,
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: entries enter here, and one whose caller never returns stays for the life of the map
          --> tools/dylint/examples/unbounded_pending_map.rs:76:81
           |
        76 |     fn start(&self, key: u64, task: JoinHandle<()>) -> Option<JoinHandle<()>> { self.running.lock().insert(key, task) }
           |                                                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: bound the map by age or count where entries enter, evict with `retain` or `pop_first`, or remove each entry in the `Drop` of a guard its caller holds

        warning: `calls` holds `(Call, u32)` entries that only a keyed `remove` takes out
          --> tools/dylint/examples/unbounded_pending_map.rs:30:5
           |
        30 |     calls: Mutex<BTreeMap<u64, (Call, u32)>>,
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
        note: entries enter here, and one whose caller never returns stays for the life of the map
          --> tools/dylint/examples/unbounded_pending_map.rs:82:67
           |
        82 | ...) -> Option<(Call, u32)> { self.calls.lock().insert(key, (Call { future: async { 0 }.boxed() }, step)) }
           |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           = help: bound the map by age or count where entries enter, evict with `retain` or `pop_first`, or remove each entry in the `Drop` of a guard its caller holds

        warning: field `level` holds text every read parses into `u8`
          --> tools/dylint/examples/unparsed_field.rs:15:5
           |
        15 |     level: String,
           |     ^^^^^^^^^^^^^
           |
        note: the text is parsed here
          --> tools/dylint/examples/unparsed_field.rs:31:64
           |
        31 | fn verbosity(options: &Options) -> Result<u8, ParseIntError> { options.level.parse() }
           |                                                                ^^^^^^^^^^^^^^^^^^^^^
           = help: declare the field `u8` and parse the text where it enters, through clap's `value_parser` or serde's `DisplayFromStr`
           = note: `#[warn(unparsed_field)]` on by default

        warning: `total` holds its value in a `Cell` that no closure captures and no call shares
         --> tools/dylint/examples/unshared_cell.rs:6:5
          |
        6 |     let total = Cell::new(0);
          |     ^^^^^^^^^^^^^^^^^^^^^^^^^
          |
          = help: bind the value with `let mut` and read and write it in place
          = note: `#[warn(unshared_cell)]` on by default

        warning: `listed` holds its value in a `RefCell` that no closure captures and no call shares
          --> tools/dylint/examples/unshared_cell.rs:14:5
           |
        14 |     let listed = RefCell::new(Vec::new());
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: bind the value with `let mut` and read and write it in place

        warning: `total` holds its value in a `Cell` that no closure captures and no call shares
          --> tools/dylint/examples/unshared_cell.rs:22:5
           |
        22 |     let total: Cell<u32> = Cell::default();
           |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: bind the value with `let mut` and read and write it in place

        warning: `Value::get` accesses an untyped JSON document by a fixed key
          --> tools/dylint/examples/untyped_document.rs:49:17
           |
        49 |     Ok(document.get("name").and_then(Value::as_str).map(str::to_owned))
           |                 ^^^^^^^^^^^
           |
           = help: declare a `#[derive(Deserialize, Serialize)]` record for the document and decode into it
           = note: `#[warn(untyped_document)]` on by default

        warning: `Value::as_str` reads an untyped JSON document by variant
          --> tools/dylint/examples/untyped_document.rs:49:38
           |
        49 |     Ok(document.get("name").and_then(Value::as_str).map(str::to_owned))
           |                                      ^^^^^^^^^^^^^
           |
           = help: declare a `#[derive(Deserialize, Serialize)]` record for the document and decode into it

        warning: `Value::as_u64` reads an untyped JSON document by variant
          --> tools/dylint/examples/untyped_document.rs:54:34
           |
        54 |     Ok(document["size"]["bytes"].as_u64().unwrap_or_default())
           |                                  ^^^^^^^^
           |
           = help: declare a `#[derive(Deserialize, Serialize)]` record for the document and decode into it

        warning: indexing accesses an untyped JSON document by a fixed key
          --> tools/dylint/examples/untyped_document.rs:54:8
           |
        54 |     Ok(document["size"]["bytes"].as_u64().unwrap_or_default())
           |        ^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: declare a `#[derive(Deserialize, Serialize)]` record for the document and decode into it

        warning: `Map::get` accesses an untyped JSON document by a fixed key
          --> tools/dylint/examples/untyped_document.rs:62:72
           |
        62 | fn object_name(object: &Map<String, Value>) -> Option<&Value> { object.get("name") }
           |                                                                        ^^^^^^^^^^^
           |
           = help: declare a `#[derive(Deserialize, Serialize)]` record for the document and decode into it

        warning: `Map::get` accesses an untyped JSON document by a fixed key
          --> tools/dylint/examples/untyped_document.rs:64:67
           |
        64 | fn constant_name(object: &Map<String, Value>) -> Option<&Value> { Map::get(object, NAME) }
           |                                                                   ^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: declare a `#[derive(Deserialize, Serialize)]` record for the document and decode into it

        warning: `Map::insert` accesses an untyped JSON document by a fixed key
          --> tools/dylint/examples/untyped_document.rs:66:71
           |
        66 | fn renamed(object: &mut Map<String, Value>) -> Option<Value> { object.insert("name".to_owned(), Value::Null) }
           |                                                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: declare a `#[derive(Deserialize, Serialize)]` record for the document and decode into it

        warning: `Value::pointer` accesses an untyped JSON document by a fixed key
          --> tools/dylint/examples/untyped_document.rs:68:57
           |
        68 | fn first(document: &Value) -> Option<&Value> { document.pointer("/items/0") }
           |                                                         ^^^^^^^^^^^^^^^^^^^
           |
           = help: declare a `#[derive(Deserialize, Serialize)]` record for the document and decode into it

        warning: `Value::String` pattern reads an untyped JSON document by variant
          --> tools/dylint/examples/untyped_document.rs:70:55
           |
        70 | fn textual(document: &Value) -> Option<&str> { if let Value::String(text) = document { Some(text) } else { None } }
           |                                                       ^^^^^^^^^^^^^^^^^^^
           |
           = help: declare a `#[derive(Deserialize, Serialize)]` record for the document and decode into it

        warning: `Value::as_str` reads an untyped JSON document by variant
          --> tools/dylint/examples/untyped_document.rs:76:124
           |
        76 | ...str) -> Option<&'document str> { object.get(key).and_then(Value::as_str) }
           |                                                              ^^^^^^^^^^^^^
           |
           = help: declare a `#[derive(Deserialize, Serialize)]` record for the document and decode into it

        warning: `Value::as_str` reads an untyped JSON document by variant
          --> tools/dylint/examples/untyped_document.rs:78:164
           |
        78 | ...iter().copied().filter_map(|key| object.get(key).and_then(Value::as_str)).collect() }
           |                                                              ^^^^^^^^^^^^^
           |
           = help: declare a `#[derive(Deserialize, Serialize)]` record for the document and decode into it

        warning: `Result` error `Box<dyn Error + Send + Sync>` is an erased `dyn Error` that callers cannot match
          --> tools/dylint/examples/untyped_error.rs:18:17
           |
        18 | type Boxed<T> = Result<T, Box<dyn Error + Send + Sync>>;
           |                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           |
           = help: return a `thiserror` enum with a variant per failure and its typed source
           = note: `#[warn(untyped_error)]` on by default

        warning: `Result` error `String` is text that callers cannot match
          --> tools/dylint/examples/untyped_error.rs:22:23
           |
        22 |     fn read(&self) -> Result<u16, String>;
           |                       ^^^^^^^^^^^^^^^^^^^
           |
           = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `()` is a unit that names no failure
          --> tools/dylint/examples/untyped_error.rs:35:14
           |
        35 |     outcome: Result<u16, ()>,
           |              ^^^^^^^^^^^^^^^
           |
           = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `String` is text that callers cannot match
          --> tools/dylint/examples/untyped_error.rs:42:18
           |
        42 |     type Error = String;
           |                  ^^^^^^
           |
           = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `String` is text that callers cannot match
          --> tools/dylint/examples/untyped_error.rs:58:18
           |
        58 |     type Error = String;
           |                  ^^^^^^
           |
           = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: error `Invalid` holds a collection of errors that can be empty, a failure holding no error
           --> tools/dylint/examples/untyped_error.rs:100:5
            |
        100 |     Invalid(Vec<PortError>),
            |     ^^^^^^^
            |
            = help: hold the errors in a `nonempty_collections::NEVec`

        warning: error `Unreachable` holds an erased `dyn Error` that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:136:5
            |
        136 |     Unreachable(#[source] Box<dyn Error + Send + Sync>),
            |     ^^^^^^^^^^^
            |
            = help: give the variant typed fields and the wrapped error's own type as its `#[source]`

        warning: error `Format` holds a source error with one value, which states nothing the variant lacks
           --> tools/dylint/examples/untyped_error.rs:160:5
            |
        160 |     Format(#[from] fmt::Error),
            |     ^^^^^^
            |
            = help: drop the field, state the source's cause in the variant's message, and convert through `map_err` with the source's one value as its pattern

        warning: error `Abandoned` holds a source error with one value, which states nothing the variant lacks
           --> tools/dylint/examples/untyped_error.rs:164:5
            |
        164 |     Abandoned(#[from] oneshot::error::RecvError),
            |     ^^^^^^^^^
            |
            = help: drop the field, state the source's cause in the variant's message, and convert through `map_err` with the source's one value as its pattern

        warning: `Result` error `String` is text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:183:27
            |
        183 | fn spelled(text: &str) -> Result<u16, String> { if text.is_empty() { Err(String::from("port text is empty")) } else { Digits::try_f...
            |                           ^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `&str` is text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:187:28
            |
        187 | fn borrowed(text: &str) -> Result<u16, &'static str> { text.parse().map_err(|_error: ParseIntError| "port is not a number") }
            |                            ^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `Box<dyn Error + Send + Sync>` is an erased `dyn Error` that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:189:25
            |
        189 | fn boxed(text: &str) -> Result<u16, Box<dyn Error + Send + Sync>> { Ok(text.trim().parse()?) }
            |                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `String` is text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:193:35
            |
        193 | fn generic_default(text: &str) -> Fallible<u16> { text.parse().map_err(|_error: ParseIntError| String::from("port is not a number")) }
            |                                   ^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `String` is text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:195:33
            |
        195 | async fn awaited(text: &str) -> Result<u16, String> {
            |                                 ^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `Code` is a `Display` type that implements no `core::error::Error`
           --> tools/dylint/examples/untyped_error.rs:200:25
            |
        200 | fn coded(text: &str) -> Result<u16, Code> { text.parse().map_err(|_error: ParseIntError| Code::Invalid) }
            |                         ^^^^^^^^^^^^^^^^^
            |
        note: the type is declared here
           --> tools/dylint/examples/untyped_error.rs:78:1
            |
         78 | enum Code {
            | ^^^^^^^^^
            = help: derive `thiserror::Error` on the type in place of its hand-written `Display`

        warning: `Result` error `Code` is a `Display` type that implements no `core::error::Error`
           --> tools/dylint/examples/untyped_error.rs:202:39
            |
        202 | fn retried(text: &str, code: Code) -> Result<u16, Code> { text.parse().map_err(|_error: ParseIntError| code) }
            |                                       ^^^^^^^^^^^^^^^^^
            |
        note: the type is declared here
           --> tools/dylint/examples/untyped_error.rs:78:1
            |
         78 | enum Code {
            | ^^^^^^^^^
            = help: derive `thiserror::Error` on the type in place of its hand-written `Display`

        warning: `Result` error `Box<str>` is text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:206:30
            |
        206 | fn boxed_text(text: &str) -> Result<u16, Box<str>> { text.parse().map_err(|_error: ParseIntError| Box::from("boxed port is not a nu...
            |                              ^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `Arc<str>` is text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:208:31
            |
        208 | fn shared_text(text: &str) -> Result<u16, Arc<str>> { text.parse().map_err(|_error: ParseIntError| Arc::from("shared port is not a ...
            |                               ^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `Rc<str>` is text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:210:32
            |
        210 | fn counted_text(text: &str) -> Result<u16, Rc<str>> { text.parse().map_err(|_error: ParseIntError| Rc::from("counted port is not a ...
            |                                ^^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `Cow<'static, str>` is text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:212:33
            |
        212 | fn borrowed_text(text: &str) -> Result<u16, Cow<'static, str>> { text.parse().map_err(|_error: ParseIntError| Cow::Borrowed("port i...
            |                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `Arc<dyn Error + Send + Sync>` is an erased `dyn Error` that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:214:32
            |
        214 | fn shared_error(text: &str) -> Result<u16, Arc<dyn Error + Send + Sync>> { text.parse().map_err(|error: ParseIntError| -> Arc<dyn E...
            |                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `Rc<dyn Error>` is an erased `dyn Error` that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:216:33
            |
        216 | fn counted_error(text: &str) -> Result<u16, Rc<dyn Error>> { text.parse().map_err(|error: ParseIntError| -> Rc<dyn Error> { Rc::new...
            |                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `String` is text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:218:30
            |
        218 | fn returned(text: String) -> Result<u16, String> { text.parse().map_err(|_error: ParseIntError| text) }
            |                              ^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `Vec<PortError>` is a collection of errors that can be empty, a failure holding no error
           --> tools/dylint/examples/untyped_error.rs:222:31
            |
        222 | fn planned(texts: &[&str]) -> Result<Vec<u16>, Vec<PortError>> {
            |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return `Result<T, NEVec<E>>` built by collecting into `validated::Validated`

        warning: function returns `Vec<PortError>`, where an empty collection stands for success
           --> tools/dylint/examples/untyped_error.rs:227:31
            |
        227 | fn audited(texts: &[&str]) -> Vec<PortError> { texts.iter().filter_map(|text| typed(text).err()).collect() }
            |                               ^^^^^^^^^^^^^^
            |
            = help: return `Result<(), NEVec<E>>` built by collecting into `validated::Validated`

        warning: `Result` error `Code` is a `Display` type that implements no `core::error::Error`
           --> tools/dylint/examples/untyped_error.rs:237:24
            |
        237 | fn kept(text: &str) -> Result<u16, Code> { text.trim().parse().map_err(|_error: ParseIntError| Code::Invalid) }
            |                        ^^^^^^^^^^^^^^^^^
            |
        note: the type is declared here
           --> tools/dylint/examples/untyped_error.rs:78:1
            |
         78 | enum Code {
            | ^^^^^^^^^
            = help: derive `thiserror::Error` on the type in place of its hand-written `Display`

        warning: `Result` error `String` is text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:239:77
            |
        239 | fn defaulted(port: Option<u16>) -> Result<u16, PortError> { port.ok_or_else(|| String::from("port is missing")).or_else(|_text| typ...
            |                                                                             ^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `&str` is text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:254:40
            |
        254 |     let present = if text.is_empty() { Err("port text is empty") } else { Ok(text) };
            |                                        ^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `String` is text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:255:19
            |
        255 |     let spelled = |digits: &str| if digits.is_empty() { Err(String::from("port text is empty")) } else { digits.parse::<u16>().map_...
            |                   ^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `anyhow::Error` is a report erasing each failure's type, which callers cannot match
           --> tools/dylint/examples/untyped_error.rs:276:28
            |
        276 | fn reported(path: &str) -> anyhow::Result<String> { Ok(fs_err::read_to_string(path)?) }
            |                            ^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `anyhow::Error` is a report erasing each failure's type, which callers cannot match
           --> tools/dylint/examples/untyped_error.rs:278:56
            |
        278 | fn contexted(text: &str) -> Result<u16, ReportError> { text.parse::<u16>().context("port").map_err(ReportError::Project) }
            |                                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `ErrorData` is built from written text beside `Unserved`, which converts into it
           --> tools/dylint/examples/untyped_error.rs:304:31
            |
        304 |     if port == reserved { Err(ErrorData::invalid_params(format!("{name} names a reserved port"), None)) } else { Ok(port) }
            |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: this construction writes the text
           --> tools/dylint/examples/untyped_error.rs:304:57
            |
        304 |     if port == reserved { Err(ErrorData::invalid_params(format!("{name} names a reserved port"), None)) } else { Ok(port) }
            |                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: add a `Unserved` variant holding the subject and raise it through the conversion

        warning: `ErrorData` is built from written text beside `Unserved`, which converts into it
           --> tools/dylint/examples/untyped_error.rs:308:78
            |
        308 | ...d_then(|port| if port == 0 { Err(ErrorData::internal_error("port zero cannot be served", None)) } else { Ok(port) })
            |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
        note: this construction writes the text
           --> tools/dylint/examples/untyped_error.rs:308:104
            |
        308 | ...ort| if port == 0 { Err(ErrorData::internal_error("port zero cannot be served", None)) } else { Ok(port) })
            |                                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: add a `Unserved` variant holding the subject and raise it through the conversion

        warning: `Result` error `Box<dyn Error + Send + Sync>` is an erased `dyn Error` that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:318:20
            |
        318 | async fn main() -> Result<(), Box<dyn Error + Send + Sync>> {
            |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: `Result` error `()` is a unit that names no failure
           --> tools/dylint/examples/untyped_error.rs:352:49
            |
        352 |         Pending { outcome: text.parse().map_err(drop) }.outcome.unwrap_or_default(),
            |                                                 ^^^^
            |
            = help: return a `thiserror` enum with a variant per failure and its typed source

        warning: error `Message` holds text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:110:5
            |
        110 |     Message(String),
            |     ^^^^^^^
            |
        note: this construction writes the text
           --> tools/dylint/examples/untyped_error.rs:263:60
            |
        263 |         "refused" => Err(ConfigError::Message(String::from("configuration refused the port"))),
            |                                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: give the variant typed fields and the wrapped error's own type as its `#[source]`

        warning: error `Detail` holds text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:112:5
            |
        112 |     Detail { detail: Box<str>, context: Cow<'static, str> },
            |     ^^^^^^
            |
        note: this construction writes the text
           --> tools/dylint/examples/untyped_error.rs:264:65
            |
        264 |         "detail" => Err(ConfigError::Detail { detail: Box::from("port detail"), context: Cow::Borrowed("port context") }),
            |                                                                 ^^^^^^^^^^^^^
            = help: give the variant typed fields and the wrapped error's own type as its `#[source]`

        warning: error `Reserved` holds text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:122:5
            |
        122 |     Reserved { port: String },
            |     ^^^^^^^^
            |
        note: this construction writes the text
           --> tools/dylint/examples/untyped_error.rs:265:57
            |
        265 |         "reserved" => Err(ConfigError::Reserved { port: 80_u16.to_string() }),
            |                                                         ^^^^^^^^^^^^^^^^^^
            = help: give the variant typed fields and the wrapped error's own type as its `#[source]`

        warning: error `Composed` holds text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:124:5
            |
        124 |     Composed(Box<str>),
            |     ^^^^^^^^
            |
        note: this construction writes the text
           --> tools/dylint/examples/untyped_error.rs:266:49
            |
        266 |         "composed" => Err(ConfigError::Composed(format!("{text} refused the port").into())),
            |                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            = help: give the variant typed fields and the wrapped error's own type as its `#[source]`

        warning: error `Refusal` holds text that callers cannot match
           --> tools/dylint/examples/untyped_error.rs:131:1
            |
        131 | struct Refusal(Arc<str>);
            | ^^^^^^^^^^^^^^
            |
        note: this construction writes the text
           --> tools/dylint/examples/untyped_error.rs:272:62
            |
        272 | fn refused() -> Result<u16, Refusal> { Err(Refusal(Arc::from("port refused"))) }
            |                                                              ^^^^^^^^^^^^^^
            = help: give the variant typed fields and the wrapped error's own type as its `#[source]`
        "#);
        output.status.exit_ok().map_err(Failure::Exit)
    }
}
