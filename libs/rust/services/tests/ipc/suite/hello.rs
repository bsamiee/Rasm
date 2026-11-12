use core::error::Error;
use core::time::Duration;
use std::io;

use claims::{assert_err, assert_ok};
use futures::future;
use rasm_ipc::{Hello, IpcError, Limits, Opened, Refusal, accept, greet, reply};
use rstest::rstest;
use tokio::io::{AsyncBufReadExt as _, AsyncWriteExt as _, BufReader};
use tokio::time::Instant;
use tokio_test::io::Builder;

use crate::{BUILD, LIMITS, PROTOCOL, hello};

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn answer(body: &str) -> Vec<u8> {
    let mut wire = Vec::new();
    assert_ok!(reply(&mut wire, Ok::<_, Refusal>(body)).await);
    wire
}

async fn opening(body: &str) -> Vec<u8> {
    let (near, far) = tokio::io::duplex(4096);
    let greeting = hello(body);
    let greeted = tokio::spawn(async move { greet::<_, _, _, String>(near, &greeting, LIMITS).await });
    let mut wire = Vec::new();
    assert_ok!(BufReader::new(far).read_until(b'\n', &mut wire).await);
    greeted.abort();
    wire
}

// --- [EXCHANGE]

#[tokio::test]
async fn stream() {
    let (near, far) = tokio::io::duplex(4096);
    let served = async {
        let Opened { body, reader, mut writer } = assert_ok!(accept::<_, u64, String>(far, PROTOCOL, &BUILD, LIMITS).await);
        assert_ok!(reply(&mut writer, Ok::<_, Refusal>(format!("Hello from server, {body}!"))).await);
        (reader, writer)
    };
    let greeting = hello("client");
    let ((mut inbound, mut outbound), opened) = future::join(served, greet::<_, _, _, String>(near, &greeting, LIMITS)).await;
    let Opened { body, mut reader, mut writer } = assert_ok!(opened);
    assert_eq!(body, "Hello from server, client!");
    let (mut received, mut returned) = (String::new(), String::new());
    assert_ok!(writer.write_all(b"Hello from client!\n").await);
    assert_ok!(inbound.read_line(&mut received).await);
    assert_ok!(outbound.write_all(b"Hello from server!\n").await);
    assert_ok!(reader.read_line(&mut returned).await);
    assert_eq!((received.as_str(), returned.as_str()), ("Hello from client!\n", "Hello from server!\n"), "the opened halves carry what follows the exchange");
}

// --- [REFUSED]

#[rstest]
#[case::protocol(u32::MAX, BUILD, Refusal::Protocol { offered: u32::MAX, served: PROTOCOL })]
#[case::superseded(PROTOCOL, u64::MAX, Refusal::Superseded)]
#[tokio::test]
async fn refused_opening_reaches_both_ends(#[case] protocol: u32, #[case] build: u64, #[case] refusal: Refusal) {
    let (near, far) = tokio::io::duplex(4096);
    let greeting = Hello { protocol, build, body: "client".to_owned() };
    let (accepted, greeted) = future::join(accept::<_, u64, String>(far, PROTOCOL, &BUILD, LIMITS), greet::<_, _, _, String>(near, &greeting, LIMITS)).await;
    assert!(matches!(assert_err!(accepted), IpcError::Refused(served) if served == refusal), "the service refuses an opening it cannot serve");
    assert!(matches!(assert_err!(greeted), IpcError::Refused(told) if told == refusal), "the client is told why");
}

#[tokio::test]
async fn service_refusal_reaches_the_client() {
    let (near, far) = tokio::io::duplex(4096);
    let refusal = Refusal::Service { reason: "no room".to_owned() };
    let served = async {
        let Opened { body: _, reader: _, mut writer } = assert_ok!(accept::<_, u64, String>(far, PROTOCOL, &BUILD, LIMITS).await);
        assert_ok!(reply(&mut writer, Err::<String, _>(refusal.clone())).await);
    };
    let greeting = hello("client");
    let ((), greeted) = future::join(served, greet::<_, _, _, String>(near, &greeting, LIMITS)).await;
    assert!(matches!(assert_err!(greeted), IpcError::Refused(told) if told == refusal), "a service refuses an opening for its own reason");
}

// --- [SCRIPTED]

#[tokio::test]
async fn read() {
    let wire = answer("world!").await;
    let (first, rest) = wire.split_at(wire.len() / 2);
    let peer = tokio::io::join(Builder::new().read(first).read(rest).build(), tokio::io::sink());
    assert_eq!(assert_ok!(greet::<_, _, _, String>(peer, &hello("hello"), LIMITS).await).body, "world!", "an answer split across reads is one answer");
    let wire = opening("hello").await;
    let (first, rest) = wire.split_at(wire.len() / 2);
    let peer = tokio::io::join(Builder::new().read(first).read(rest).build(), tokio::io::sink());
    assert_eq!(assert_ok!(accept::<_, u64, String>(peer, PROTOCOL, &BUILD, LIMITS).await).body, "hello", "an opening message split across reads is one message");
}

#[tokio::test]
async fn read_error() {
    let wire = answer("world!").await;
    let peer = tokio::io::join(Builder::new().read(&wire[..wire.len() / 2]).read_error(io::Error::other("cruel")).build(), tokio::io::sink());
    let failed = assert_err!(greet::<_, _, _, String>(peer, &hello("hello"), LIMITS).await);
    let kinds = (&failed as &dyn Error).sources().filter_map(|cause| cause.downcast_ref::<io::Error>().map(io::Error::kind)).collect::<Vec<_>>();
    assert_eq!(kinds, [io::ErrorKind::Other], "the read failure is the cause: {failed:?}");
}

#[tokio::test]
async fn read_partial_then_end() {
    let wire = answer("world!").await;
    let peer = tokio::io::join(Builder::new().read(&wire[..wire.len() / 2]).build(), tokio::io::sink());
    assert_err!(greet::<_, _, _, String>(peer, &hello("hello"), LIMITS).await, "an answer cut short is no answer");
    let wire = opening("hello").await;
    let peer = tokio::io::join(Builder::new().read(&wire[..wire.len() / 2]).build(), tokio::io::sink());
    assert_err!(accept::<_, u64, String>(peer, PROTOCOL, &BUILD, LIMITS).await, "an opening message cut short is no message");
}

#[tokio::test(start_paused = true)]
async fn wait() {
    const FIRST_WAIT: Duration = Duration::from_secs(1);
    let wire = answer("world!").await;
    let peer = tokio::io::join(Builder::new().wait(FIRST_WAIT).read(&wire).build(), tokio::io::sink());
    let start = Instant::now();
    assert_eq!(assert_ok!(greet::<_, _, _, String>(peer, &hello("hello"), LIMITS).await).body, "world!");
    assert!(start.elapsed() >= FIRST_WAIT, "consumed {:?}", start.elapsed());
}

#[tokio::test(start_paused = true)]
async fn timeout() {
    let (near, _silent) = tokio::io::duplex(4096);
    let start = Instant::now();
    assert_err!(greet::<_, _, _, String>(near, &hello("hello"), LIMITS).await, "a peer that never answers ends the exchange");
    assert!(start.elapsed() < Duration::from_mins(1), "the wait for an answer is bounded: {:?}", start.elapsed());
}

#[tokio::test(start_paused = true)]
async fn max_length() {
    let limits = Limits { line: 256, ..LIMITS };
    let (near, mut flooding) = tokio::io::duplex(64 << 10);
    assert_ok!(flooding.write_all(&[b'a'; 4096]).await);
    let start = Instant::now();
    assert_err!(greet::<_, _, _, String>(near, &hello("hello"), limits).await, "a line over the maximum length is refused");
    assert!(start.elapsed() < limits.deadline, "the line is refused on its length, with no wait for its end: {:?}", start.elapsed());
}
