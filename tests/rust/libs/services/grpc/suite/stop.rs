use core::time::Duration;
use std::io;

use backon::{ConstantBuilder, Retryable as _};
use claims::{assert_err, assert_ok, assert_some};
use futures::FutureExt as _;
use tokio::net::TcpStream;
use tokio::time;
use tonic::Code;

use crate::{LIMIT, Pace, asked, running};

// --- [CALLS]

#[tokio::test]
async fn connect_returns_err_via_call_after_connected() {
    let running = running(None, Pace::Prompt, |routes| routes).await;
    let mut client = running.client().await;
    assert_ok!(client.check(asked("")).await);
    running.stop.cancel();
    time::sleep(Duration::from_millis(100)).await;
    assert_eq!(assert_err!(client.check(asked("")).await).code(), Code::Unavailable);
    assert!(assert_ok!(assert_ok!(time::timeout(LIMIT, running.served).await)).is_empty(), "every listener ends without an error");
}

#[tokio::test]
async fn shutdown_closes_listener_before_drain() {
    let running = running(None, Pace::Held, |routes| routes).await;
    let (mut client, address) = (running.client().await, running.address);
    let call = tokio::spawn(async move { client.check(asked("")).await });
    assert_ok!(running.probed.entered.acquire().await).forget();
    running.stop.cancel();
    let refused = || TcpStream::connect(address).map(|connected| connected.map_or_else(|error| Ok(error.kind()), |_accepted| Err(io::Error::from(io::ErrorKind::AlreadyExists))));
    let kind = assert_ok!(refused.retry(ConstantBuilder::new().with_delay(Duration::from_millis(10)).with_max_times(100)).await, "incoming was not dropped before drain");
    assert!(matches!(kind, io::ErrorKind::ConnectionRefused | io::ErrorKind::ConnectionReset), "{kind:?}");
    running.probed.release.add_permits(1);
    assert_ok!(assert_ok!(call.await), "the call in flight completes");
    assert!(assert_ok!(assert_ok!(time::timeout(LIMIT, running.served).await)).is_empty(), "every listener ends without an error");
}

#[tokio::test]
async fn client_cancellation_reaches_the_handler() {
    let running = running(None, Pace::Held, |routes| routes).await;
    let mut client = running.client().await;
    assert_err!(time::timeout(Duration::from_millis(200), client.check(asked(""))).await, "the held call outlives the wait");
    assert_ok!(assert_ok!(time::timeout(LIMIT, running.probed.left.acquire()).await, "a cancelled call ends its handler")).forget();
}

// --- [STREAMS]

#[tokio::test]
async fn shutdown_ends_an_open_stream_as_unavailable() {
    let running = running(None, Pace::Prompt, |routes| routes).await;
    let mut client = running.client().await;
    let mut watched = assert_ok!(client.watch(asked("")).await).into_inner();
    assert_some!(assert_ok!(watched.message().await), "the stream is open");
    running.stop.cancel();
    let ended = assert_err!(assert_ok!(time::timeout(LIMIT, watched.message()).await, "a stop ends the stream"));
    assert_eq!(ended.code(), Code::Unavailable, "a server shutting down answers UNAVAILABLE: {ended:?}");
    assert!(assert_ok!(assert_ok!(time::timeout(LIMIT, running.served).await)).is_empty(), "every listener ends without an error");
}
