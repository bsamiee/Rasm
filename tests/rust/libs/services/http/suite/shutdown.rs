use core::time::Duration;
use std::io;

use backon::{ConstantBuilder, Retryable as _};
use claims::{assert_err, assert_ok};
use futures::{FutureExt as _, future};
use h2::Reason;
use http::{Request, StatusCode};
use nonempty_collections::{IntoNonEmptyIterator as _, NonEmptyIterator as _};
use rasm_http::{HttpError, Stage, Timeout};
use rstest::rstest;
use sysexits::ExitCode;
use tokio::io::{AsyncReadExt as _, AsyncWriteExt as _};
use tokio::net::TcpStream;
use tokio::time;
use tokio_util::task::AbortOnDropHandle;

use crate::{GRACE, GREETING, fetched, read_until, running};

// --- [CONSTANTS] -----------------------------------------------------------------------

const SETTLE: Duration = Duration::from_millis(100);
const LIMIT: Duration = Duration::from_secs(5);

// --- [IDLE]

#[tokio::test]
async fn graceful_shutdown_before_first_request_no_block() {
    let running = running(&["127.0.0.1"], GRACE).await;
    let mut silent = assert_ok!(TcpStream::connect(running.addresses[0]).await);
    assert_eq!(fetched(running.addresses[0], "/").await.0, StatusCode::OK, "the server accepts before the stop");
    assert_ok!(running.stop.send(()));
    let mut rest = Vec::new();
    assert_ok!(assert_ok!(time::timeout(LIMIT, silent.read_to_end(&mut rest)).await, "timed out waiting for graceful shutdown"));
    assert_ok!(assert_ok!(assert_ok!(time::timeout(LIMIT, running.served).await, "a server with no request in flight stops at once")));
}

#[tokio::test]
async fn graceful_shutdown_ok() {
    let running = running(&["127.0.0.1"], GRACE).await;
    let mut idle = Vec::new();
    for _ in 0..3 {
        let mut client = assert_ok!(TcpStream::connect(running.addresses[0]).await);
        assert_ok!(client.write_all(b"GET / HTTP/1.1\r\nHost: example.domain\r\n\r\n").await);
        assert_ok!(read_until(&mut client, |seen| seen.ends_with(GREETING.as_bytes())).await);
        idle.push(client);
    }
    assert_ok!(running.stop.send(()));
    for mut client in idle {
        let mut rest = Vec::new();
        assert_ok!(assert_ok!(time::timeout(LIMIT, client.read_to_end(&mut rest)).await, "an idle keep-alive connection closes at the stop"));
    }
    assert_ok!(assert_ok!(assert_ok!(time::timeout(LIMIT, running.served).await)));
}

// --- [IN_FLIGHT]

#[tokio::test]
async fn shutdown_closes_listener_before_drain() {
    let running = running(&["127.0.0.1"], GRACE).await;
    let address = running.addresses[0];
    let call = tokio::spawn(fetched(address, "/held"));
    assert_ok!(running.app.entered.acquire().await).forget();
    assert_ok!(running.stop.send(()));
    let refused = || TcpStream::connect(address).map(|connected| connected.map_or_else(|error| Ok(error.kind()), |_accepted| Err(io::Error::from(io::ErrorKind::AlreadyExists))));
    let kind = assert_ok!(refused.retry(ConstantBuilder::new().with_delay(Duration::from_millis(10)).with_max_times(100)).await, "the listener closes before in-flight work ends");
    assert!(matches!(kind, io::ErrorKind::ConnectionRefused | io::ErrorKind::ConnectionReset), "{kind:?}");
    assert!(!call.is_finished(), "the held call is still in flight");
    running.app.release.add_permits(1);
    assert_eq!(assert_ok!(call.await), (StatusCode::OK, GREETING.into()), "the call in flight completes");
    assert_ok!(assert_ok!(assert_ok!(time::timeout(LIMIT, running.served).await)));
}

#[tokio::test]
async fn graceful_shutdown_delayed_ok() {
    let running = running(&["127.0.0.1"], GRACE).await;
    let calls = [(); 3].map(|()| tokio::spawn(fetched(running.addresses[0], "/held")));
    assert_ok!(running.app.entered.acquire_many(3).await).forget();
    assert_ok!(running.stop.send(()));
    for _ in 0..3 {
        time::sleep(Duration::from_millis(50)).await;
        assert!(!running.served.is_finished(), "the shutdown waits for every call in flight");
        running.app.release.add_permits(1);
    }
    for call in future::join_all(calls).await {
        assert_eq!(assert_ok!(call).0, StatusCode::OK);
    }
    assert_ok!(assert_ok!(assert_ok!(time::timeout(LIMIT, running.served).await)));
}

#[tokio::test]
async fn graceful_shutdown_timeout() {
    let mut running = running(&["127.0.0.1"], GRACE).await;
    let calls = [(); 3].map(|()| AbortOnDropHandle::new(tokio::spawn(fetched(running.addresses[0], "/held"))));
    assert_ok!(running.app.entered.acquire_many(3).await).forget();
    assert_ok!(running.stop.send(()));
    running.app.release.add_permits(2);
    assert_err!(time::timeout(SETTLE, &mut running.served).await, "shutdown should not be completed: as not all our conns finish");
    assert_eq!(calls.iter().filter(|call| call.is_finished()).count(), 2, "the calls that could finish did");
}

// --- [ABANDONED]

#[rstest]
#[case::grace_deadline(SETTLE, 1, Timeout::Deadline(SETTLE))]
#[case::second_shutdown_request(GRACE, 2, Timeout::Request)]
#[tokio::test]
async fn shutdown_past_its_bound_is_abandoned(#[case] grace: Duration, #[case] requests: usize, #[case] bound: Timeout) {
    let running = running(&["127.0.0.1"], grace).await;
    let _call = AbortOnDropHandle::new(tokio::spawn(fetched(running.addresses[0], "/held")));
    assert_ok!(running.app.entered.acquire().await).forget();
    for _ in 0..requests {
        assert_ok!(running.stop.send(()));
    }
    let failures = assert_err!(assert_ok!(assert_ok!(time::timeout(LIMIT, running.served).await, "a bounded shutdown ends with the call still held")));
    let (abandoned, rest) = failures.split_first();
    assert!(rest.is_empty() && matches!(abandoned, HttpError::Abandoned { reason, pending } if *reason == bound && pending.contains(&Stage::Server)), "{abandoned:?}");
    assert_eq!(ExitCode::from(failures.into_nonempty_iter().next().0), ExitCode::TempFail, "an abandoned shutdown is a temporary failure");
}

// --- [PROTOCOL]

#[tokio::test]
async fn disable_keep_alive_mid_request() {
    let running = running(&["127.0.0.1"], GRACE).await;
    let mut client = assert_ok!(TcpStream::connect(running.addresses[0]).await);
    assert_ok!(client.write_all(b"GET / HTTP/1.1\r\n").await);
    time::sleep(SETTLE).await;
    assert_ok!(running.stop.send(()));
    time::sleep(SETTLE).await;
    assert_ok!(client.write_all(b"Host: localhost\r\n\r\n").await);
    let mut answer = String::new();
    assert_ok!(assert_ok!(time::timeout(LIMIT, client.read_to_string(&mut answer)).await));
    assert!(answer.starts_with("HTTP/1.1 200 OK\r\n"), "should be a response over the wire: {answer}");
    assert!(answer.contains("connection: close\r\n"), "should have disabled keep-alive: {answer}");
}

#[tokio::test]
async fn http2_graceful_shutdown() {
    let running = running(&["127.0.0.1"], GRACE).await;
    let address = running.addresses[0];
    let (mut client, connection) = assert_ok!(h2::client::handshake(assert_ok!(TcpStream::connect(address).await)).await);
    let closed = tokio::spawn(connection);
    let request = |path: &str| Request::get(format!("http://{address}{path}")).body(());
    let (held, _) = assert_ok!(client.send_request(assert_ok!(request("/held")), true));
    assert_ok!(running.app.entered.acquire().await).forget();
    assert_ok!(running.stop.send(()));
    time::sleep(SETTLE).await;
    let late = match client.send_request(assert_ok!(request("/")), true) {
        Ok((response, _)) => assert_err!(response.await, "streams sent after GOAWAY receive no response"),
        Err(error) => error,
    };
    assert!(late.is_go_away() && late.is_remote(), "the peer announced the shutdown with GOAWAY: {late:?}");
    assert_eq!(late.reason(), Some(Reason::NO_ERROR));
    running.app.release.add_permits(1);
    assert_eq!(assert_ok!(held.await).status(), StatusCode::OK, "the stream opened before GOAWAY completes");
    assert_ok!(assert_ok!(assert_ok!(time::timeout(LIMIT, closed).await, "the connection closes after its last stream")));
    assert_ok!(assert_ok!(assert_ok!(time::timeout(LIMIT, running.served).await)));
}
