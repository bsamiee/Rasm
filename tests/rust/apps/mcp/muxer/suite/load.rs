use core::time::Duration;
use std::collections::BTreeMap;
use std::error::Report;
use std::io;

use backon::{ConstantBuilder, Retryable as _};
use claims::{assert_ok, assert_some};
use futures::future;
use hdrhistogram::Histogram;
use mcp_muxer_tests::{Agent, Ask, Echo, Isolated, Sized, Slow, call, exited, modern, spawned, text};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rmcp::RoleClient;
use rmcp::model::{CallToolRequest, CallToolRequestParams, ClientRequest};
use rmcp::service::{ClientLifecycleMode, PeerRequestOptions, RunningService};
use serde::Serialize;
use sysinfo::{Pid, ProcessRefreshKind, ProcessesToUpdate, Signal, System};
use tokio::io::AsyncWriteExt as _;
use tokio::time::{self, Instant};

use crate::scoped;

// --- [CONSTANTS] -----------------------------------------------------------------------

const LEGACY: [&str; 2] = ["--speaks", "legacy"];

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy)]
enum Through {
    Direct,
    Muxer(&'static [&'static str]),
}

#[derive(Debug)]
struct Attached {
    running: RunningService<RoleClient, Agent>,
    agent: Agent,
    process: Option<u32>,
}

#[derive(Debug, Clone, Copy, Default)]
struct Usage {
    cpu_millis: u64,
    memory: u64,
    descriptors: usize,
}

#[derive(Debug, Serialize)]
struct Latency {
    calls: u64,
    p50_us: u64,
    p99_us: u64,
    max_us: u64,
}

#[derive(Debug, Serialize)]
struct Crowd {
    connections: usize,
    latency: Latency,
    handshakes_ms: u128,
    script_ms: u128,
    calls_per_second: Option<u64>,
    asks_answered_by_own_session: usize,
}

#[derive(Debug, Serialize)]
struct Daemon {
    pid: u32,
    cpu_ms_under_load: Option<u64>,
    memory_bytes_after_load: Option<u64>,
    memory_bytes_idle: Option<u64>,
    descriptors: Option<usize>,
    cpu_ms_over_idle_minute: u64,
    exited_after_every_session_left: bool,
    sessions_after_every_session_left: usize,
    calls_after_every_session_left: usize,
}

#[derive(Debug, Serialize)]
struct Relays {
    count: usize,
    memory_bytes_mean: Option<u64>,
    memory_bytes_max: Option<u64>,
    cpu_ms_over_idle_minute_total: u64,
}

#[derive(Debug, Serialize)]
struct Drain {
    ms_with_calls_in_flight: u128,
    calls_answered_with_error: usize,
}

#[derive(Debug, Serialize)]
struct Measured {
    level: &'static str,
    sessions: usize,
    crowds: BTreeMap<&'static str, Crowd>,
    daemon: Daemon,
    relays: Relays,
    drain: Drain,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn summary(latency: &Histogram<u64>) -> Latency { Latency { calls: latency.len(), p50_us: latency.value_at_quantile(0.5), p99_us: latency.value_at_quantile(0.99), max_us: latency.max() } }

async fn printed(report: &impl Serialize) {
    let mut printed = assert_ok!(serde_json::to_vec_pretty(report));
    printed.push(b'\n');
    assert_ok!(tokio::io::stdout().write_all(&printed).await);
    assert_ok!(tokio::io::stdout().flush().await);
}

async fn attached(isolated: &Isolated, through: Through, server: &[&str], lifecycle: ClientLifecycleMode, index: usize) -> Attached {
    let agent = Agent { marker: format!("agent-{index}"), ..Agent::default() };
    let command = match through {
        Through::Direct => isolated.direct(server),
        Through::Muxer(options) => isolated.relay(options, server),
    };
    let (running, process) = assert_ok!(spawned(agent.clone(), command, lifecycle).await);
    Attached { running, agent, process }
}

async fn timed(attached: &Attached, latency: &mut Histogram<u64>, request: io::Result<CallToolRequestParams>) -> String {
    let (request, started) = (assert_ok!(request), Instant::now());
    let result = assert_ok!(attached.running.call_tool(request).await);
    assert_ok!(latency.record(assert_ok!(u64::try_from(started.elapsed().as_micros()))));
    text(&result)
}

async fn crowded(isolated: &Isolated, level: &'static str) {
    const SESSIONS: usize = 150;
    const BURST: usize = 20;
    let isolated = &Isolated { level, ..isolated.clone() };
    let legacy = || ClientLifecycleMode::Initialize;
    let usage = |pids: &[u32]| {
        let mut system = assert_ok!(System::new());
        let watched = Vec::from_iter(pids.iter().copied().map(Pid::from_u32));
        system.refresh_processes_specifics(ProcessesToUpdate::Some(&watched), true, ProcessRefreshKind::everything());
        let measured = pids.iter().filter_map(|pid| {
            let process = system.process(Pid::from_u32(*pid))?;
            Some((*pid, Usage { cpu_millis: process.accumulated_cpu_time(), memory: process.memory(), descriptors: process.open_files().unwrap_or_default() }))
        });
        BTreeMap::from_iter(measured)
    };
    let script = async |attached: &Attached| {
        let mut latency = assert_ok!(Histogram::<u64>::new(3));
        assert!(assert_ok!(attached.running.peer().list_all_tools().await).len() >= 6, "the session lists the fixture tools");
        for bytes in [64, 64 << 10, 64, 1 << 20].into_iter().cycle().take(BURST) {
            assert_eq!(timed(attached, &mut latency, call("sized", &Sized { bytes })).await.len(), bytes, "a result of {bytes} bytes arrives whole");
        }
        assert_eq!(timed(attached, &mut latency, call("slow", &Slow { millis: 200, steps: 4 })).await, "done");
        let request = ClientRequest::CallToolRequest(CallToolRequest::new(assert_ok!(call("slow", &Slow { millis: 60_000, steps: 1 }))));
        let handle = assert_ok!(attached.running.peer().send_cancellable_request(request, PeerRequestOptions::no_options()).await);
        time::sleep(Duration::from_millis(50)).await;
        assert_ok!(handle.cancel(None).await);
        let answered = timed(attached, &mut latency, call("ask", &Ask { question: "who".to_owned() })).await;
        (latency, answered == attached.agent.marker)
    };
    let (mut crowds, mut sessions) = (BTreeMap::new(), Vec::new());
    for (name, through, server, lifecycle, connections) in [
        ("1 direct server, 1 connection", Through::Direct, LEGACY.as_slice(), legacy as fn() -> ClientLifecycleMode, 1),
        ("150 direct servers", Through::Direct, LEGACY.as_slice(), legacy, SESSIONS),
        ("150 muxed servers, one per session", Through::Muxer(&["--per", "session"]), LEGACY.as_slice(), legacy, SESSIONS),
        ("1 muxed server, modern sessions, modern server", Through::Muxer(&[]), [].as_slice(), modern, SESSIONS),
        ("1 muxed server, legacy sessions, modern server", Through::Muxer(&[]), [].as_slice(), legacy, SESSIONS),
        ("1 muxed server, legacy sessions, legacy server", Through::Muxer(&[]), LEGACY.as_slice(), legacy, SESSIONS),
    ] {
        let started = Instant::now();
        sessions = future::join_all((0..connections).map(|index| attached(isolated, through, server, lifecycle(), index))).await;
        let (handshakes, started) = (started.elapsed(), Instant::now());
        let scripts = future::join_all((0..SESSIONS).filter_map(|index| sessions.get(index.checked_rem(connections)?)).map(&script)).await;
        let elapsed = started.elapsed();
        let mut latency = assert_ok!(Histogram::<u64>::new(3));
        for (session, _) in &scripts {
            assert_ok!(latency.add(session));
        }
        assert_eq!(usize::try_from(latency.len()).ok(), Some(SESSIONS.saturating_mul(BURST.saturating_add(2))), "{name}: every call of every session is answered");
        let measured = Crowd {
            connections,
            latency: summary(&latency),
            handshakes_ms: handshakes.as_millis(),
            script_ms: elapsed.as_millis(),
            calls_per_second: latency.len().saturating_mul(1000).checked_div(assert_ok!(u64::try_from(elapsed.as_millis()))),
            asks_answered_by_own_session: scripts.iter().filter(|(_, own)| *own).count(),
        };
        crowds.insert(name, measured);
    }
    let state = async || assert_ok!(isolated.state(&[]).await).daemon;
    let daemon = assert_some!(state().await).pid;
    let relays = Vec::from_iter(sessions.iter().filter_map(|session| session.process));
    let watched = Vec::from_iter(relays.iter().copied().chain([daemon]));
    let loaded = usage(&watched);
    printed(&BTreeMap::from([("daemon_after_load", daemon)])).await;
    time::sleep(Duration::from_mins(1)).await;
    let rested = usage(&watched);
    printed(&BTreeMap::from([("daemon_after_idle_minute", daemon)])).await;
    time::sleep(Duration::from_secs(20)).await;
    let spent = |pid: &u32| rested.get(pid).zip(loaded.get(pid)).map_or_default(|(after, before)| after.cpu_millis.saturating_sub(before.cpu_millis));
    let relay_memory = Vec::from_iter(relays.iter().filter_map(|pid| rested.get(pid)).map(|usage| usage.memory));
    future::join_all(sessions.into_iter().map(async |session| assert_ok!(session.running.cancel().await))).await;
    let emptied = (async || Some(state().await).filter(|held| held.as_ref().is_none_or(|held| held.sessions == 0 && held.calls == 0)).ok_or_else(|| io::Error::from(io::ErrorKind::WouldBlock)))
        .retry(ConstantBuilder::new().with_delay(Duration::from_millis(100)).with_max_times(50))
        .await;
    let left = match emptied {
        Ok(held) => held,
        Err(error) => {
            tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "daemon still holds sessions or calls");
            state().await
        }
    };
    sessions = future::join_all((0..SESSIONS).map(|index| attached(isolated, Through::Muxer(&[]), &LEGACY, legacy(), index))).await;
    let slow = future::join_all(sessions.iter().map(async |session| session.running.call_tool(assert_ok!(call("slow", &Slow { millis: 30_000, steps: 1 }))).await));
    let draining = assert_some!(state().await).pid;
    let drained = async {
        time::sleep(Duration::from_millis(500)).await;
        let mut system = assert_ok!(System::new());
        system.refresh_processes(ProcessesToUpdate::Some(&[Pid::from_u32(draining)]), true);
        let started = Instant::now();
        assert_eq!(assert_some!(system.process(Pid::from_u32(draining))).kill_with(Signal::Term), Some(true), "the daemon takes the terminate signal");
        assert_ok!(exited(draining).await);
        started.elapsed()
    };
    let (answers, drain) = future::join(slow, drained).await;
    let report = Measured {
        level,
        sessions: SESSIONS,
        crowds,
        daemon: Daemon {
            pid: daemon,
            cpu_ms_under_load: loaded.get(&daemon).map(|usage| usage.cpu_millis),
            memory_bytes_after_load: loaded.get(&daemon).map(|usage| usage.memory),
            memory_bytes_idle: rested.get(&daemon).map(|usage| usage.memory),
            descriptors: rested.get(&daemon).map(|usage| usage.descriptors),
            cpu_ms_over_idle_minute: spent(&daemon),
            exited_after_every_session_left: left.is_none(),
            sessions_after_every_session_left: left.as_ref().map_or_default(|held| held.sessions),
            calls_after_every_session_left: left.as_ref().map_or_default(|held| held.calls),
        },
        relays: Relays {
            count: relays.len(),
            memory_bytes_mean: relay_memory.iter().sum::<u64>().checked_div(assert_ok!(u64::try_from(relay_memory.len()))),
            memory_bytes_max: relay_memory.iter().copied().max(),
            cpu_ms_over_idle_minute_total: relays.iter().map(spent).sum(),
        },
        drain: Drain { ms_with_calls_in_flight: drain.as_millis(), calls_answered_with_error: answers.iter().filter(|answer| answer.is_err()).count() },
    };
    printed(&report).await;
    for (name, measured) in &report.crowds {
        assert_eq!(measured.asks_answered_by_own_session, SESSIONS, "{name}: every ask reaches the session that caused it");
    }
    assert_eq!((report.daemon.sessions_after_every_session_left, report.daemon.calls_after_every_session_left), (0, 0), "the daemon holds no session and no call once every session left");
}

// --- [LOAD]

#[tokio::test(flavor = "multi_thread", worker_threads = 4)]
#[ignore = "latency measurement, run by name"]
async fn one_session_two_thousand_calls() {
    let passed = scoped(async |isolated: &Isolated| {
        let mut measured = BTreeMap::new();
        for (name, through) in [("direct", Through::Direct), ("muxed", Through::Muxer(&[]))] {
            drop(attached(isolated, through, &LEGACY, ClientLifecycleMode::Initialize, 0).await);
            let started = Instant::now();
            let session = attached(isolated, through, &LEGACY, ClientLifecycleMode::Initialize, 0).await;
            let (start, mut latency) = (started.elapsed(), assert_ok!(Histogram::<u64>::new(3)));
            for _ in 0..2000 {
                timed(&session, &mut latency, call("echo", &Echo { text: "x".to_owned() })).await;
            }
            measured.insert(name, (start.as_micros(), summary(&latency)));
        }
        printed(&measured).await;
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

#[tokio::test(flavor = "multi_thread", worker_threads = 8)]
#[ignore = "load measurement at the default log level, run by name with a raised descriptor limit"]
async fn one_hundred_fifty_sessions() {
    assert_ok!(scoped(async |isolated: &Isolated| crowded(isolated, "info").await).await, "a failed test keeps its folder");
}

#[tokio::test(flavor = "multi_thread", worker_threads = 8)]
#[ignore = "load measurement at the debug log level, run by name with a raised descriptor limit"]
async fn one_hundred_fifty_sessions_at_debug() {
    assert_ok!(scoped(async |isolated: &Isolated| crowded(isolated, "debug").await).await, "a failed test keeps its folder");
}
