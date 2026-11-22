use core::time::Duration;
use std::io;

use claims::{assert_err, assert_matches, assert_ok};
use futures::future;
use rasm_ipc::{Hello, IpcError, IpcOperation, Limits, Opened, Refusal, accept, greet, reply};
use rstest::rstest;
use tokio::io::{AsyncBufReadExt as _, AsyncWriteExt as _, BufReader, DuplexStream};
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

async fn exchanged(protocol: u32, build: u64) -> (Result<(), IpcError>, Result<Opened<DuplexStream, String>, IpcError>) {
    let (near, far) = tokio::io::duplex(4096);
    let greeting = Hello { protocol, build, body: "client".to_owned() };
    let served = async {
        let Opened { body, reader: _, mut writer } = accept::<_, u64, String>(far, PROTOCOL, &BUILD, LIMITS).await?;
        reply(&mut writer, Ok::<_, Refusal>(body)).await
    };
    future::join(served, greet::<_, _, _, String>(near, &greeting, LIMITS)).await
}

async fn limited(length: usize) -> (Result<Opened<DuplexStream, String>, IpcError>, Duration) {
    let wire = answer(&"x".repeat(length.saturating_sub(r#"{"Welcome":""}"#.len() + 1))).await;
    assert_eq!(wire.len(), length, "the answer line, newline counted, is {length} bytes");
    let (near, mut far) = tokio::io::duplex(64 << 10);
    assert_ok!(far.write_all(&wire).await);
    let started = Instant::now();
    let greeted = greet::<_, _, _, String>(near, &hello("hello"), Limits { line: 256, ..LIMITS }).await;
    (greeted, started.elapsed())
}

// --- [EXCHANGE]

#[tokio::test]
async fn opened_halves_carry_the_lines_after_the_exchange() {
    let (near, far) = tokio::io::duplex(4096);
    let served = async {
        let Opened { body, reader, mut writer } = assert_ok!(accept::<_, u64, String>(far, PROTOCOL, &BUILD, LIMITS).await);
        assert_ok!(reply(&mut writer, Ok::<_, Refusal>(format!("welcome {body}"))).await);
        (reader, writer)
    };
    let greeting = hello("client");
    let ((mut inbound, mut outbound), opened) = future::join(served, greet::<_, _, _, String>(near, &greeting, LIMITS)).await;
    let Opened { body, mut reader, mut writer } = assert_ok!(opened);
    assert_eq!(body, "welcome client", "the client reads the body the service answered for its own body");
    let (mut received, mut returned) = (String::new(), String::new());
    assert_ok!(writer.write_all(b"up\n").await);
    assert_ok!(inbound.read_line(&mut received).await);
    assert_ok!(outbound.write_all(b"down\n").await);
    assert_ok!(reader.read_line(&mut returned).await);
    assert_eq!((received.as_str(), returned.as_str()), ("up\n", "down\n"), "the opened halves carry what follows the exchange");
}

#[rstest]
#[case::same_build(BUILD)]
#[case::older_build(BUILD - 1)]
#[tokio::test]
async fn opening_of_this_protocol_from_a_build_no_newer_is_served(#[case] build: u64) {
    let (accepted, greeted) = exchanged(PROTOCOL, build).await;
    assert_ok!(accepted, "offered build {build}: the service serves it");
    assert_eq!(assert_ok!(greeted).body, "client", "offered build {build}: the client reads its answer");
}

#[rstest]
#[case::newer_build(PROTOCOL, BUILD + 1, Refusal::Superseded)]
#[case::other_protocol(PROTOCOL + 1, BUILD, Refusal::Protocol { offered: PROTOCOL + 1, served: PROTOCOL })]
#[case::newer_build_of_other_protocol(PROTOCOL + 1, BUILD + 1, Refusal::Superseded)]
#[tokio::test]
async fn opening_from_a_newer_build_or_another_protocol_is_refused_at_both_ends(#[case] protocol: u32, #[case] build: u64, #[case] refusal: Refusal) {
    let (accepted, greeted) = exchanged(protocol, build).await;
    assert_matches!(assert_err!(accepted), IpcError::Refused(served) if served == refusal, "offered protocol {protocol} build {build}: the service refuses it");
    assert_matches!(assert_err!(greeted), IpcError::Refused(told) if told == refusal, "offered protocol {protocol} build {build}: the client is told why");
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
    assert_matches!(assert_err!(greeted), IpcError::Refused(told) if told == refusal, "a service refuses an opening for its own reason");
}

// --- [FRAMING]

#[tokio::test]
async fn message_split_across_reads_is_one_message() {
    let wire = answer("world").await;
    let (first, rest) = wire.split_at(wire.len() / 2);
    let peer = tokio::io::join(Builder::new().read(first).read(rest).build(), tokio::io::sink());
    assert_eq!(assert_ok!(greet::<_, _, _, String>(peer, &hello("hello"), LIMITS).await).body, "world", "an answer split across reads is one answer");
    let opened = opening("hello").await;
    let (head, tail) = opened.split_at(opened.len() / 2);
    let client = tokio::io::join(Builder::new().read(head).read(tail).build(), tokio::io::sink());
    assert_eq!(assert_ok!(accept::<_, u64, String>(client, PROTOCOL, &BUILD, LIMITS).await).body, "hello", "an opening split across reads is one message");
}

#[tokio::test]
async fn peer_closing_mid_message_is_a_closed_connection() {
    let wire = answer("world").await;
    let peer = tokio::io::join(Builder::new().read(&wire[..wire.len() / 2]).build(), tokio::io::sink());
    assert_matches!(assert_err!(greet::<_, _, _, String>(peer, &hello("hello"), LIMITS).await), IpcError::Closed, "an answer cut short by the service closing");
    let opened = opening("hello").await;
    let client = tokio::io::join(Builder::new().read(&opened[..opened.len() / 2]).build(), tokio::io::sink());
    assert_matches!(assert_err!(accept::<_, u64, String>(client, PROTOCOL, &BUILD, LIMITS).await), IpcError::Closed, "an opening cut short by the client closing");
}

#[tokio::test]
async fn peer_closing_before_any_byte_is_a_closed_connection() {
    let peer = tokio::io::join(Builder::new().build(), tokio::io::sink());
    assert_matches!(assert_err!(greet::<_, _, _, String>(peer, &hello("hello"), LIMITS).await), IpcError::Closed);
}

#[rstest]
#[case::trailing_value(r#"{"Welcome":"world"} {"Welcome":"again"}"#)]
#[case::unknown_answer(r#"{"Greeting":"world"}"#)]
#[case::unclosed_object(r#"{"Welcome":"world""#)]
#[tokio::test]
async fn answer_line_holding_other_than_one_answer_is_undecodable(#[case] line: &str) {
    let peer = tokio::io::join(Builder::new().read(format!("{line}\n").as_bytes()).build(), tokio::io::sink());
    assert_matches!(assert_err!(greet::<_, _, _, String>(peer, &hello("hello"), LIMITS).await), IpcError::Decode { source: _ }, "answer line {line}");
}

#[tokio::test]
async fn read_failure_names_the_receive() {
    let wire = answer("world").await;
    let peer = tokio::io::join(Builder::new().read(&wire[..wire.len() / 2]).read_error(io::Error::from(io::ErrorKind::ConnectionReset)).build(), tokio::io::sink());
    assert_matches!(assert_err!(greet::<_, _, _, String>(peer, &hello("hello"), LIMITS).await), IpcError::Io { operation: IpcOperation::Receive, source } if source.kind() == io::ErrorKind::ConnectionReset);
}

#[tokio::test(start_paused = true)]
async fn answer_line_of_exactly_the_limit_is_read() {
    let (greeted, waited) = limited(256).await;
    assert_eq!(assert_ok!(greeted).body.len(), 241, "a 256 byte line, newline counted, holds a 241 byte body");
    assert_eq!(waited, Duration::ZERO);
}

#[tokio::test(start_paused = true)]
async fn answer_line_one_byte_over_the_limit_is_refused_without_waiting_for_its_end() {
    let (greeted, waited) = limited(257).await;
    assert_matches!(assert_err!(greeted), IpcError::Oversized { limit: 256 });
    assert_eq!(waited, Duration::ZERO, "the refusal comes from the bytes read, with no wait for the line to end");
}

#[tokio::test(start_paused = true)]
async fn answer_inside_the_deadline_is_read_and_silence_ends_at_it() {
    let wire = answer("late").await;
    let peer = tokio::io::join(Builder::new().wait(LIMITS.deadline.saturating_sub(Duration::from_millis(1))).read(&wire).build(), tokio::io::sink());
    assert_eq!(assert_ok!(greet::<_, _, _, String>(peer, &hello("hello"), LIMITS).await).body, "late", "an answer one millisecond inside the deadline is read");
    let (near, _silent) = tokio::io::duplex(4096);
    let started = Instant::now();
    assert_matches!(assert_err!(greet::<_, _, _, String>(near, &hello("hello"), LIMITS).await), IpcError::Deadline { operation: IpcOperation::Receive, source: _ });
    assert_eq!(started.elapsed(), LIMITS.deadline, "a silent service is given up on at the deadline");
}
