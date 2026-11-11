use core::sync::atomic::Ordering;
use core::time::Duration;
use std::path::PathBuf;

use claims::assert_ok;
use futures::future;
use mcp_muxer_tests::{Agent, Ask, Isolated, Slow, call, modern, session, spawned, text};
use rmcp::model::{CallToolRequest, ClientRequest};
use rmcp::service::{ClientLifecycleMode, PeerRequestOptions};
use tokio::time;

use crate::{HOME, echoed, scoped, servers, whoami};

// --- [KEYS]

#[tokio::test(flavor = "multi_thread")]
async fn sessions_share_one_server_per_key() {
    let passed = scoped(async |isolated: &Isolated| {
        let trace = "TRACEPARENT";
        let relay = |options: &[&str]| {
            let mut command = isolated.relay(options, &[]);
            command.env(trace, "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01");
            session(Agent::default(), command)
        };
        let (first, second, tagged, other, own) = (
            assert_ok!(relay(&[]).await),
            assert_ok!(relay(&[]).await),
            assert_ok!(relay(&["--env", "FIXTURE_TAG=one"]).await),
            assert_ok!(relay(&["--env", "FIXTURE_TAG=two"]).await),
            assert_ok!(relay(&["--per", "session"]).await),
        );
        let direct = assert_ok!(session(Agent::default(), isolated.direct(&[])).await);
        let (shared, same, one, two, private, unshared) =
            (whoami(&first, HOME).await, whoami(&second, HOME).await, whoami(&tagged, "FIXTURE_TAG").await, whoami(&other, "FIXTURE_TAG").await, whoami(&own, HOME).await, whoami(&direct, HOME).await);
        assert_eq!(shared.pid, same.pid, "two sessions with one key reach one server process");
        assert_eq!(shared.value, unshared.value, "the server receives the session environment an unshared run receives");
        assert_eq!(shared.directory, unshared.directory, "the server runs in the session working directory");
        assert_eq!((one.value.as_deref(), two.value.as_deref()), (Some("one"), Some("two")), "declared variables reach the server");
        assert_ne!(one.pid, two.pid, "sessions with differing declared values never share");
        assert_ne!(shared.pid, private.pid, "a per-session server is not shared");
        assert_eq!((whoami(&first, trace).await.value, whoami(&own, trace).await.value.is_some()), (None, true), "trace context reaches a per-session server and no shared one");
        let entries = |folder: PathBuf| assert_ok!(fs_err::read_dir(folder)).count();
        let state = assert_ok!(isolated.state(&[]).await);
        assert_eq!(state.cache, isolated.folder("cache"), "the state report names the overridden cache folder");
        assert!(entries(isolated.folder("cache")) >= 1, "stored handshakes sit in the overridden cache folder");
        assert!(entries(isolated.folder("logs")) >= 1, "log files sit in the overridden log folder");
        assert!(entries(isolated.folder("r")) >= 2, "socket and lock sit in the overridden runtime folder");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

#[tokio::test(flavor = "multi_thread")]
async fn racing_first_sessions_all_share() {
    let passed = scoped(async |isolated: &Isolated| {
        for round in 0..100 {
            let relay = || {
                let mut command = isolated.relay(&[], &[]);
                command.env("MCP_MUXER_RUNTIME_DIR", format!("r{round}")).env("MCP_MUXER_IDLE", "5s");
                session(Agent::default(), command)
            };
            let (first, second) = future::join(relay(), relay()).await;
            let (first, second) = (assert_ok!(first), assert_ok!(second));
            assert_eq!(whoami(&first, HOME).await.pid, whoami(&second, HOME).await.pid, "round {round}: neither racing session fell back to an unshared server");
        }
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

// --- [ERAS]

#[tokio::test(flavor = "multi_thread")]
async fn sessions_of_either_era_reach_servers_of_either_era() {
    let passed = scoped(async |isolated: &Isolated| {
        let agent = |marker: &str| Agent { marker: marker.to_owned(), ..Agent::default() };
        for (server, served) in [([].as_slice(), "modern"), (["--speaks", "legacy"].as_slice(), "legacy")] {
            let (old, new) = (agent("old"), agent("new"));
            let (legacy, _) = assert_ok!(spawned(old.clone(), isolated.relay(&[], server), ClientLifecycleMode::Initialize).await);
            let (current, _) = assert_ok!(spawned(new.clone(), isolated.relay(&[], server), modern()).await);
            assert_eq!(echoed(&legacy, "one").await, "one", "{served} server: a session that sends initialize is answered");
            assert_eq!(echoed(&current, "two").await, "two", "{served} server: a session that sends server/discover is answered");
            let asked = || call("ask", &Ask { question: "who".to_owned() });
            assert_eq!(text(&assert_ok!(legacy.call_tool(assert_ok!(asked())).await)), "old", "{served} server: the initialize session answers its own input request");
            assert_eq!(text(&assert_ok!(current.call_tool(assert_ok!(asked())).await)), "new", "{served} server: the discover session answers its own input request");
            assert_ok!(legacy.call_tool(assert_ok!(call("slow", &Slow { millis: 100, steps: 2 }))).await);
            assert_eq!(old.progress.load(Ordering::Relaxed), 2, "{served} server: progress reaches the initialize session");
            let eras = Vec::from_iter(servers(isolated, &[]).await.into_iter().filter_map(|held| held.era));
            assert!(eras.iter().any(|era| era == served), "the state report names a {served} server: {eras:?}");
            assert_eq!(whoami(&legacy, HOME).await.pid, whoami(&current, HOME).await.pid, "{served} server: sessions of both eras share one server process");
        }
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

// --- [ROUTING]

#[tokio::test(flavor = "multi_thread")]
async fn server_requests_progress_and_cancellation_reach_the_calling_session() {
    let passed = scoped(async |isolated: &Isolated| {
        let agent = |marker: &str| Agent { marker: marker.to_owned(), ..Agent::default() };
        let (left, right) = (agent("left"), agent("right"));
        let one = assert_ok!(session(left.clone(), isolated.relay(&[], &[])).await);
        let two = assert_ok!(session(right.clone(), isolated.relay(&[], &[])).await);
        let asked = || call("ask", &Ask { question: "who".to_owned() });
        assert_eq!(text(&assert_ok!(one.peer().call_tool(assert_ok!(asked())).await)), "left", "elicitation reaches the session whose call is in flight");
        assert_eq!(text(&assert_ok!(two.peer().call_tool(assert_ok!(asked())).await)), "right", "elicitation reaches the other session for its own call");
        assert_ok!(one.peer().call_tool(assert_ok!(call("slow", &Slow { millis: 200, steps: 4 }))).await);
        assert_eq!((left.progress.load(Ordering::Relaxed), right.progress.load(Ordering::Relaxed)), (4, 0), "progress reaches the calling session alone");
        let request = ClientRequest::CallToolRequest(CallToolRequest::new(assert_ok!(call("slow", &Slow { millis: 60_000, steps: 1 }))));
        let handle = assert_ok!(two.peer().send_cancellable_request(request, PeerRequestOptions::no_options()).await);
        time::sleep(Duration::from_millis(200)).await;
        assert_eq!(Vec::from_iter(servers(isolated, &[]).await.iter().map(|served| served.calls)), [1], "the slow call is in flight");
        assert_ok!(handle.cancel(None).await);
        time::sleep(Duration::from_millis(300)).await;
        assert_eq!(Vec::from_iter(servers(isolated, &[]).await.iter().map(|served| served.calls)), [0], "the cancelled call ends at the server");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}
