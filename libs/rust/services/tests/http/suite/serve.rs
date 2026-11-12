use core::time::Duration;
use std::os::fd::AsFd as _;

use bytes::Bytes;
use claims::assert_ok;
use http::{Request, StatusCode, Version};
use http_body_util::{BodyExt as _, Empty};
use hyper_util::rt::{TokioExecutor, TokioIo};
use rlimit::Resource;
use rstest::rstest;
use tokio::io::{AsyncReadExt as _, AsyncWriteExt as _};
use tokio::net::{TcpSocket, TcpStream};
use tokio::time;
use tokio_util::task::AbortOnDropHandle;

use crate::{GRACE, GREETING, QUUX, fetched, read_until, running};

// --- [HTTP1]

#[tokio::test]
async fn keep_alive() {
    let running = running(&["127.0.0.1"], GRACE).await;
    let mut client = assert_ok!(TcpStream::connect(running.addresses[0]).await);
    assert_ok!(client.write_all(b"GET / HTTP/1.1\r\nHost: example.domain\r\n\r\n").await);
    assert_ok!(read_until(&mut client, |seen| seen.ends_with(GREETING.as_bytes())).await, "reading 1");
    assert_ok!(client.write_all(b"GET /quux HTTP/1.1\r\nHost: example.domain\r\nConnection: close\r\n\r\n").await);
    assert_ok!(read_until(&mut client, |seen| seen.ends_with(QUUX.as_bytes())).await, "reading 2 on the same connection");
}

#[tokio::test]
async fn expect_continue_sends_100() {
    const INTERIM: &[u8] = b"HTTP/1.1 100 Continue\r\n\r\n";
    let running = running(&["127.0.0.1"], GRACE).await;
    let mut client = assert_ok!(TcpStream::connect(running.addresses[0]).await);
    assert_ok!(client.write_all(b"POST /foo HTTP/1.1\r\nHost: example.domain\r\nExpect: 100-continue\r\nContent-Length: 5\r\nConnection: Close\r\n\r\n").await);
    let mut interim = vec![0; INTERIM.len()];
    assert_ok!(client.read_exact(&mut interim).await);
    assert_eq!(interim, INTERIM);
    assert_ok!(client.write_all(b"hello").await);
    let mut answer = String::new();
    assert_ok!(client.read_to_string(&mut answer).await);
    assert!(answer.starts_with("HTTP/1.1 200 OK\r\n") && answer.ends_with("hello"), "the body sent after the interim response reaches the handler: {answer}");
    assert!(!answer.contains("100 Continue"), "one interim response precedes the final one");
}

#[tokio::test]
async fn pipelined_requests_are_answered_in_order() {
    let running = running(&["127.0.0.1"], GRACE).await;
    let mut client = assert_ok!(TcpStream::connect(running.addresses[0]).await);
    assert_ok!(client.write_all(b"GET / HTTP/1.1\r\nHost: example.domain\r\n\r\nGET /quux HTTP/1.1\r\nHost: example.domain\r\nConnection: close\r\n\r\n").await);
    let mut answers = String::new();
    assert_ok!(client.read_to_string(&mut answers).await);
    let (first, second) = assert_ok!(answers.split_once(GREETING).ok_or(&answers));
    assert!(first.starts_with("HTTP/1.1 200 OK\r\n") && !first.contains("connection: close"), "the first response keeps the connection: {first}");
    assert!(second.starts_with("HTTP/1.1 200 OK\r\n") && second.contains("connection: close\r\n") && second.ends_with(QUUX), "the second response closes it: {second}");
}

#[rstest]
#[case::parse_errors_send_4xx_response(b"GE T / HTTP/1.1\r\n\r\n")]
#[case::illegal_request_length_returns_400_response(b"POST / HTTP/1.1\r\nHost: example.domain\r\nContent-Length: foo\r\n\r\n")]
#[tokio::test]
async fn malformed_request(#[case] request: &[u8]) {
    let running = running(&["127.0.0.1"], GRACE).await;
    let mut client = assert_ok!(TcpStream::connect(running.addresses[0]).await);
    assert_ok!(client.write_all(request).await);
    let mut answer = String::new();
    assert_ok!(client.read_to_string(&mut answer).await);
    assert!(answer.starts_with("HTTP/1.1 400 "), "{answer}");
}

#[tokio::test]
async fn http1() {
    let running = running(&["127.0.0.1"], GRACE).await;
    assert_eq!(fetched(running.addresses[0], "/").await, (StatusCode::OK, GREETING.into()));
}

// --- [HTTP2]

#[tokio::test]
async fn http2() {
    let running = running(&["127.0.0.1"], GRACE).await;
    let stream = TokioIo::new(assert_ok!(TcpStream::connect(running.addresses[0]).await));
    let (mut sender, connection) = assert_ok!(hyper::client::conn::http2::Builder::new(TokioExecutor::new()).handshake(stream).await);
    let _driven = AbortOnDropHandle::new(tokio::spawn(connection));
    let request = assert_ok!(Request::get(format!("http://{}/", running.addresses[0])).body(Empty::<Bytes>::new()));
    let response = assert_ok!(sender.send_request(request).await);
    assert_eq!((response.status(), response.version()), (StatusCode::OK, Version::HTTP_2), "a prior-knowledge connection is answered over HTTP/2");
    assert_eq!(assert_ok!(response.into_body().collect().await).to_bytes(), GREETING);
}

// --- [ACCEPT]

#[tokio::test]
async fn accept_survives_descriptor_exhaustion() {
    let running = running(&["127.0.0.1"], GRACE).await;
    let socket = assert_ok!(TcpSocket::new_v4());
    let (_, hard) = assert_ok!(Resource::NOFILE.get());
    assert_ok!(Resource::NOFILE.set(256.min(hard), hard));
    let input = std::io::stdin();
    let mut held = Vec::new();
    let exhausted = loop {
        match input.as_fd().try_clone_to_owned() {
            Ok(descriptor) => held.push(descriptor),
            Err(exhausted) => break exhausted,
        }
    };
    assert!(!held.is_empty() && exhausted.raw_os_error().is_some(), "descriptors run out under the lowered limit: {exhausted}");
    let _refused = assert_ok!(socket.connect(running.addresses[0]).await);
    time::sleep(Duration::from_millis(200)).await;
    drop(held);
    let answered = assert_ok!(time::timeout(Duration::from_secs(10), fetched(running.addresses[0], "/")).await, "the listener accepts again once descriptors are free");
    assert_eq!(answered, (StatusCode::OK, GREETING.into()));
}
