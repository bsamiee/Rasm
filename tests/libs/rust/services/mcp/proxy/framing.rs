use bytes::{Bytes, BytesMut};
use claims::{assert_ok, assert_some};
use proptest::prelude::*;
use proptest::test_runner::FileFailurePersistence;
use rasm_mcp::{Line, Lines, Method, Resumable, identity};
use rasm_mcp_tests::Id;
use rmcp::ErrorData;
use serde_json::json;
use tokio_util::codec::Decoder as _;

use super::{INITIALIZE, INITIALIZED, Peer, joined, padded, proxy};

// --- [CONSTANTS] -----------------------------------------------------------------------

const MAX: usize = 4 << 10;

// --- [OPERATIONS] ----------------------------------------------------------------------

fn decoded(limit: usize, chunks: impl IntoIterator<Item: AsRef<[u8]>>) -> Vec<Line> {
    let (mut lines, mut buffer, mut seen) = (Lines::new(limit), BytesMut::new(), Vec::new());
    for chunk in chunks {
        buffer.extend_from_slice(chunk.as_ref());
        while let Some(line) = assert_ok!(lines.decode(&mut buffer)) {
            seen.push(line);
        }
    }
    seen
}

// --- [DECODER]

proptest! {
    #![proptest_config(ProptestConfig { failure_persistence: Some(Box::new(FileFailurePersistence::WithSource("regressions"))), ..ProptestConfig::default() })]

    #[test]
    fn lines_are_yielded_whole_within_the_limit_and_as_head_and_tail_over_it(
        (limit, lines, unfinished, sizes) in (1_usize..48).prop_flat_map(|limit| {
            let line = |lengths: BoxedStrategy<usize>| lengths.prop_flat_map(|length| prop::collection::vec(any::<u8>().prop_filter("a line holds no newline", |byte| *byte != b'\n'), length));
            let lengths = prop_oneof![limit.saturating_sub(2)..=limit + 1, 0..limit * 3 + 512].boxed();
            (Just(limit), prop::collection::vec(line(lengths), 0..12), line((0..limit * 2).boxed()), prop::collection::vec(1_usize..97, 1..24))
        }),
    ) {
        const TAIL: usize = 256;
        let stream = [lines.iter().flat_map(|line| [line.as_slice(), b"\n"]).collect::<Vec<_>>().concat(), unfinished].concat();
        let mut rest = stream.as_slice();
        let chunks = Vec::from_iter(sizes.iter().cycle().map_while(|size| {
            let (chunk, after) = rest.split_at(rest.len().min(*size));
            rest = after;
            Some(chunk.to_vec()).filter(|chunk| !chunk.is_empty())
        }));
        let reference = Vec::from_iter(lines.iter().map(|line| {
            let framed = [line.as_slice(), b"\n"].concat();
            match framed.split_at_checked(limit) {
                Some((head, over)) if !over.is_empty() => Line::Oversized { head: head.to_vec().into(), tail: over[over.len().saturating_sub(TAIL)..].to_vec().into() },
                Some(_) | None => Line::Whole(framed.into()),
            }
        }));
        prop_assert_eq!(decoded(limit, chunks), reference, "limit {}, chunk sizes {:?}", limit, sizes);
    }

    #[test]
    fn oversized_request_names_its_id_and_method_from_whichever_end_holds_them(
        id in prop_oneof![any::<u64>().prop_map(|id| id.to_string()), "[a-z0-9:-]{0,12}".prop_map(|id| format!("\"{id}\""))],
        decoy in any::<u32>(),
        padding in 64_usize..2048,
        trailing in any::<bool>(),
    ) {
        let pad = "x".repeat(padding);
        let line = if trailing {
            format!(r#"{{"jsonrpc":"2.0","params":{{"pad":"{pad}","id":{decoy}}},"method":"tools/call","id":{id}}}"#)
        } else {
            format!(r#"{{"jsonrpc":"2.0","id":{id},"method":"tools/call","params":{{"pad":"{pad}","id":{decoy}}}}}"#)
        };
        let named = Vec::from_iter(decoded(64, [format!("{line}\n")]).iter().map(|seen| match seen {
            Line::Oversized { head, tail } => Some(identity(head, tail)),
            Line::Whole(_) => None,
        }));
        prop_assert_eq!(named, [Some((Some(Bytes::from(id)), Some(Method::CallTool)))], "the line over a 64 byte limit is one oversized line naming the top-level id and method");
    }
}

#[test]
fn line_of_exactly_the_limit_with_its_newline_is_whole_and_one_byte_more_is_not() {
    let line = |length: usize| [vec![b'x'; length], vec![b'\n']].concat();
    assert_eq!(decoded(MAX, [line(MAX - 1)]), [Line::Whole(line(MAX - 1).into())], "a line of the limit, newline counted, is whole");
    assert_eq!(
        decoded(MAX, [line(MAX), b"ok\n".to_vec()]),
        [Line::Oversized { head: vec![b'x'; MAX].into(), tail: Bytes::from_static(b"\n") }, Line::Whole(Bytes::from_static(b"ok\n"))],
        "a line one byte over is oversized, and the next line is whole"
    );
}

// --- [SESSION]

#[tokio::test(start_paused = true)]
async fn session_line_over_the_limit_is_refused_under_its_id_and_later_lines_reach_the_server() {
    let (proxy, mut starts, _actor) = proxy(16, MAX);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut client = joined(&proxy).await;
    client.send(&padded(1, MAX * 3)).await;
    client.send(&padded(2, MAX)).await;
    client.chunked(&padded(3, MAX - 1), 97).await;
    for id in [1, 2] {
        let refused = client.next().await;
        assert_eq!(refused.id, Some(Id::Number(id)), "the request over the limit is answered under its own id");
        let stated = json!({ "_tag": "Exceeded", "part": "request", "limit": MAX, "next": { "smaller": "request" } });
        assert_eq!((refused.error.code, refused.error.data), (Some(-32600), Some(stated)), "the refusal states the limit and a smaller request");
    }
    assert_eq!(server.wire.next().await.method.as_deref(), Some("tools/list"), "the line that fits, newline counted, reaches the server whole across many reads");
    server.wire.silent(core::time::Duration::from_millis(100)).await;
}

#[tokio::test(start_paused = true)]
async fn server_response_over_the_limit_fails_its_call_alone() {
    let (proxy, mut starts, _actor) = proxy(16, 1 << 10);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut caller = joined(&proxy).await;
    for (id, result) in [(7, format!(r#"{{"tools":[],"pad":"{}"}}"#, "x".repeat(2 << 10))), (8, r#"{"tools":[]}"#.to_owned())] {
        caller.send(&format!(r#"{{"jsonrpc":"2.0","id":{id},"method":"tools/list"}}"#)).await;
        let asked = server.wire.next().await;
        server.wire.send(&asked.answered(&result)).await;
    }
    let (wide, next) = (caller.next().await, caller.next().await);
    assert_eq!(wide.id, Some(Id::Number(7)));
    let stated = json!({ "_tag": "Exceeded", "part": "response", "limit": 1 << 10, "next": { "smaller": "response" } });
    assert_eq!((wide.error.code, wide.error.data), (Some(-32603), Some(stated)), "the caller is told the response was over the limit and to ask for a smaller one");
    assert_eq!((next.id, next.error.message), (Some(Id::Number(8)), None), "the next call is answered");
}

#[tokio::test(start_paused = true)]
async fn server_notification_or_request_over_the_limit_fails_no_call() {
    let (proxy, mut starts, _actor) = proxy(16, 1 << 10);
    let mut server = assert_some!(starts.recv().await).linked().await;
    let mut caller = joined(&proxy).await;
    let padding = "x".repeat(2 << 10);
    caller.send(r#"{"jsonrpc":"2.0","id":7,"method":"tools/call","params":{"name":"wide"}}"#).await;
    let call = server.wire.next().await;
    let issued = assert_ok!(serde_json::to_string(assert_some!(call.id.as_ref())));
    server.wire.send(&format!(r#"{{"jsonrpc":"2.0","method":"notifications/message","params":{{"level":"info","data":"{padding}"}}}}"#)).await;
    server.wire.send(&format!(r#"{{"jsonrpc":"2.0","id":{issued},"method":"elicitation/create","params":{{"message":"{padding}"}}}}"#)).await;
    server.wire.send(&call.answered("{}")).await;
    let answer = caller.next().await;
    assert_eq!((answer.id.clone(), answer.error.message.as_deref()), (Some(Id::Number(7)), None), "a notification or request over the line limit fails no call: {answer:?}");
    let refused = server.wire.next().await;
    assert_eq!(refused.id, call.id, "the request over the line limit is refused under its own id");
    assert!(refused.error.message.as_deref().is_some_and(|message| message.contains("exceeds")), "the refusal states the limit: {refused:?}");
}

// --- [REFUSAL]

#[tokio::test]
async fn refusing_session_answers_each_request_with_the_cause_until_its_input_ends() {
    let (client, near) = tokio::io::duplex(64 << 10);
    let (input, output) = tokio::io::split(near);
    let mut wire = Peer::new(client);
    for line in [INITIALIZED, r#"{"jsonrpc":"2.0","id":"probe","method":"server/discover"}"#, r#"{"jsonrpc":"2.0","id":5,"result":{}}"#, &padded(9, MAX * 4), INITIALIZE] {
        wire.send(line).await;
    }
    let refusing = tokio::spawn(async move { Resumable::new(input, output, MAX).refuse(&ErrorData::internal_error("cannot find tool on PATH", None)).await });
    let answers = [wire.next().await, wire.next().await, wire.next().await].map(|seen| (seen.id, seen.error.message));
    let cause = || Some("cannot find tool on PATH".to_owned());
    assert_eq!(
        answers,
        [(Some(Id::Text("probe".to_owned())), cause()), (Some(Id::Number(9)), cause()), (Some(Id::Number(0)), cause())],
        "each request, whole or over the line limit, is answered with the cause under its own id, and the notification and the response are not"
    );
    drop(wire);
    assert_ok!(assert_ok!(refusing.await), "the refusal ends with its client's input");
}
