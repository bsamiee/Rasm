use std::path::PathBuf;

use claims::assert_ok;
use futures::{StreamExt as _, future, stream};
use mcp_muxer_tests::{Agent, Ask, Isolated, Slow, call, modern, reached, session, spawned, text};
use rasm_testing::{Failure, Folder};
use rmcp::model::{CallToolRequest, ClientRequest};
use rmcp::service::{ClientLifecycleMode, PeerRequestOptions};

use crate::{HOME, echoed, scoped, servers, settled, whoami};

// --- [KEYS]

#[tokio::test(flavor = "multi_thread")]
async fn sessions_share_a_server_only_under_one_key() -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let relay = |options: &[&str]| session(Agent::default(), isolated.relay(options, &[]));
        let (first, second) = (assert_ok!(relay(&[]).await), assert_ok!(relay(&[]).await));
        let (one, two, own) = (assert_ok!(relay(&["--env", "FIXTURE_TAG=one"]).await), assert_ok!(relay(&["--env", "FIXTURE_TAG=two"]).await), assert_ok!(relay(&["--per", "session"]).await));
        let shared = whoami(&first, HOME).await.pid;
        assert_eq!(whoami(&second, HOME).await.pid, shared, "two sessions with one key reach one server process");
        let (one, two) = (whoami(&one, HOME).await.pid, whoami(&two, HOME).await.pid);
        assert!(one != two && one != shared && two != shared, "a declared value is part of the key, so differing values never share: {shared} {one} {two}");
        assert_ne!(whoami(&own, HOME).await.pid, shared, "a per-session server is not shared");
    })
    .await
}

#[tokio::test(flavor = "multi_thread")]
async fn racing_first_sessions_all_share() -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let relay = |round: usize| {
            let mut command = isolated.relay(&[], &[]);
            command.env("MCP_MUXER_RUNTIME_DIR", format!("r{round}")).env("MCP_MUXER_IDLE", "5s");
            session(Agent::default(), command)
        };
        for round in 0..100 {
            let (first, second) = future::join(relay(round), relay(round)).await;
            let (first, second) = (assert_ok!(first), assert_ok!(second));
            assert_eq!(whoami(&first, HOME).await.pid, whoami(&second, HOME).await.pid, "round {round}: neither racing session fell back to an unshared server");
        }
    })
    .await
}

// --- [ENVIRONMENT]

#[tokio::test(flavor = "multi_thread")]
async fn shared_server_runs_with_the_environment_and_directory_an_unshared_run_gets_except_trace_context() -> Result<(), Failure<Folder>> {
    const TRACE: &str = "TRACEPARENT";
    scoped(async |isolated: &Isolated| {
        let traced = |mut command: tokio::process::Command| {
            command.env(TRACE, "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01");
            session(Agent::default(), command)
        };
        let (shared, tagged, own, direct) = (
            assert_ok!(traced(isolated.relay(&[], &[])).await),
            assert_ok!(traced(isolated.relay(&["--env", "FIXTURE_TAG=one"], &[])).await),
            assert_ok!(traced(isolated.relay(&["--per", "session"], &[])).await),
            assert_ok!(traced(isolated.direct(&[])).await),
        );
        let (relayed, unshared) = (whoami(&shared, HOME).await, whoami(&direct, HOME).await);
        assert_eq!((relayed.value, relayed.directory), (unshared.value, unshared.directory), "the server gets the session's variables and working directory an unshared run gets");
        assert_eq!(whoami(&tagged, "FIXTURE_TAG").await.value.as_deref(), Some("one"), "a declared variable reaches the server");
        assert_eq!((whoami(&shared, TRACE).await.value, whoami(&own, TRACE).await.value.is_some()), (None, true), "trace context reaches a per-session server and no shared one");
    })
    .await
}

// --- [FOLDERS]

#[tokio::test(flavor = "multi_thread")]
async fn overridden_folders_hold_the_handshakes_logs_socket_and_lock() -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let running = assert_ok!(session(Agent::default(), isolated.relay(&[], &[])).await);
        assert_eq!(echoed(&running, "stored").await, "stored", "the server answers, so its handshake is stored");
        let state = assert_ok!(isolated.state(&[]).await);
        assert_eq!(state.cache, isolated.folder("cache"), "the state report names the overridden cache folder");
        let listed = async |folder: PathBuf| {
            stream::unfold(assert_ok!(fs_err::tokio::read_dir(folder).await), async |mut entries| assert_ok!(entries.next_entry().await).map(|entry| (entry.path(), entries))).collect::<Vec<_>>().await
        };
        assert!(!listed(isolated.folder("cache").join("servers")).await.is_empty(), "stored handshakes sit under the overridden cache folder");
        assert!(!listed(isolated.folder("logs")).await.is_empty(), "per-process log files sit in the overridden output folder");
        let mut runtime = Vec::from_iter(listed(isolated.folder("r")).await.iter().filter_map(|path| Some(path.extension()?.to_str()?.to_owned())));
        runtime.sort_unstable();
        assert_eq!(runtime, ["lock", "sock"], "the daemon's lock and socket sit in the overridden runtime folder");
    })
    .await
}

#[tokio::test(flavor = "multi_thread")]
async fn sessions_share_one_server_when_runtime_and_cache_folders_coincide() -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let relay = || {
            let mut command = isolated.relay(&[], &[]);
            command.env("MCP_MUXER_RUNTIME_DIR", isolated.folder("cache"));
            session(Agent::default(), command)
        };
        let first = assert_ok!(relay().await);
        let held = whoami(&first, HOME).await;
        let second = assert_ok!(relay().await);
        assert_eq!(whoami(&second, HOME).await.pid, held.pid, "a later session reaches the server of the daemon that shares its folder with the stored handshakes");
    })
    .await
}

// --- [LIFECYCLES]

#[tokio::test(flavor = "multi_thread")]
async fn sessions_of_either_lifecycle_reach_servers_of_either_lifecycle() -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let agent = |marker: &str| Agent { marker: marker.to_owned(), ..Agent::default() };
        for (server, served) in [([].as_slice(), "discover"), (["--speaks", "legacy"].as_slice(), "initialize")] {
            let (old, new) = (agent("old"), agent("new"));
            let (legacy, _) = assert_ok!(spawned(old.clone(), isolated.relay(&[], server), ClientLifecycleMode::Initialize).await);
            let (current, _) = assert_ok!(spawned(new.clone(), isolated.relay(&[], server), modern()).await);
            assert_eq!(echoed(&legacy, "one").await, "one", "{served} server: a session that sends initialize is answered");
            assert_eq!(echoed(&current, "two").await, "two", "{served} server: a session that sends server/discover is answered");
            let asked = || call("ask", &Ask { question: "who".to_owned() });
            assert_eq!(text(&assert_ok!(legacy.call_tool(assert_ok!(asked())).await)), "old", "{served} server: the initialize session answers its own input request");
            assert_eq!(text(&assert_ok!(current.call_tool(assert_ok!(asked())).await)), "new", "{served} server: the discover session answers its own input request");
            assert_ok!(legacy.call_tool(assert_ok!(call("slow", &Slow { millis: 100, steps: 2 }))).await);
            assert_eq!(assert_ok!(reached(&old.progress, 2).await), 2, "{served} server: progress reaches the initialize session");
            let lifecycles = Vec::from_iter(servers(isolated, &[]).await.into_iter().filter_map(|held| held.lifecycle));
            assert!(lifecycles.iter().any(|lifecycle| lifecycle == served), "the state report names a {served} server: {lifecycles:?}");
            assert_eq!(whoami(&legacy, HOME).await.pid, whoami(&current, HOME).await.pid, "{served} server: sessions of both lifecycles share one server process");
        }
    })
    .await
}

// --- [ROUTING]

#[tokio::test(flavor = "multi_thread")]
async fn server_requests_progress_and_cancellation_reach_the_calling_session() -> Result<(), Failure<Folder>> {
    scoped(async |isolated: &Isolated| {
        let agent = |marker: &str| Agent { marker: marker.to_owned(), ..Agent::default() };
        let (left, right) = (agent("left"), agent("right"));
        let one = assert_ok!(session(left.clone(), isolated.relay(&[], &[])).await);
        let two = assert_ok!(session(right.clone(), isolated.relay(&[], &[])).await);
        let asked = || call("ask", &Ask { question: "who".to_owned() });
        assert_eq!(text(&assert_ok!(one.peer().call_tool(assert_ok!(asked())).await)), "left", "elicitation reaches the session whose call is in flight");
        assert_eq!(text(&assert_ok!(two.peer().call_tool(assert_ok!(asked())).await)), "right", "elicitation reaches the other session for its own call");
        assert_ok!(one.peer().call_tool(assert_ok!(call("slow", &Slow { millis: 200, steps: 4 }))).await);
        assert_eq!((assert_ok!(reached(&left.progress, 4).await), *right.progress.borrow()), (4, 0), "progress reaches the calling session alone");
        let request = ClientRequest::CallToolRequest(CallToolRequest::new(assert_ok!(call("slow", &Slow { millis: 60_000, steps: 1 }))));
        let handle = assert_ok!(two.peer().send_cancellable_request(request, PeerRequestOptions::no_options()).await);
        let flying = settled(isolated, |held| held.iter().map(|served| served.calls).eq([1])).await;
        assert_eq!(Vec::from_iter(flying.iter().map(|served| served.calls)), [1], "the slow call is in flight");
        assert_ok!(handle.cancel(None).await);
        let ended = settled(isolated, |held| held.iter().map(|served| served.calls).eq([0])).await;
        assert_eq!(Vec::from_iter(ended.iter().map(|served| served.calls)), [0], "the cancelled call ends at the server");
        assert_eq!(echoed(&two, "after").await, "after", "the cancelling session stays usable");
    })
    .await
}
