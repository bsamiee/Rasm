use core::time::Duration;

use bytes::Bytes;
use claims::{assert_ok, assert_some};
use http::Request;
use http::header::{CONTENT_TYPE, TE};
use rasm_grpc::timeout;
use rstest::rstest;
use tokio::net::TcpStream;
use tokio::time;
use tokio_util::task::AbortOnDropHandle;
use tonic::Code;
use tonic::metadata::MetadataMap;

use crate::{LIMIT, Pace, running};

// --- [EXPIRY]

#[rstest]
#[case::cancelation_on_timeout(Duration::from_secs(100), "500m")]
#[case::picks_server_timeout_if_thats_sorter(Duration::from_millis(100), "10H")]
#[case::picks_client_timeout_if_thats_sorter(Duration::from_secs(100), "100m")]
#[tokio::test]
async fn deadline_expires_before_the_reply(#[case] server: Duration, #[case] header: &str) {
    let running = running(Some(server), Pace::Slow, |routes| routes).await;
    let (mut client, connection) = assert_ok!(h2::client::handshake(assert_ok!(TcpStream::connect(running.address).await)).await);
    let _driven = AbortOnDropHandle::new(tokio::spawn(connection));
    let call = Request::post(format!("http://{}/grpc.health.v1.Health/Check", running.address)).header(CONTENT_TYPE, "application/grpc").header(TE, "trailers").header("grpc-timeout", header);
    let (answered, mut message) = assert_ok!(client.send_request(assert_ok!(call.body(())), false));
    assert_ok!(message.send_data(Bytes::from_static(&[0; 5]), true));
    let answer = assert_ok!(assert_ok!(time::timeout(LIMIT, answered).await, "the server answers at the deadline"));
    let status = assert_some!(answer.headers().get("grpc-status"), "an expired call ends in a status with no message: {answer:?}");
    assert_eq!(Code::from_bytes(status.as_bytes()), Code::DeadlineExceeded, "a deadline that expires before the server returns status is DEADLINE_EXCEEDED: {answer:?}");
}

// --- [GRAMMAR]

#[rstest]
#[case::hours("3H", Some(Duration::from_hours(3)))]
#[case::minutes("1M", Some(Duration::from_mins(1)))]
#[case::seconds("42S", Some(Duration::from_secs(42)))]
#[case::milliseconds("13m", Some(Duration::from_millis(13)))]
#[case::microseconds("2u", Some(Duration::from_micros(2)))]
#[case::nanoseconds("82n", Some(Duration::from_nanos(82)))]
#[case::eight_digits("99999999S", Some(Duration::from_secs(99_999_999)))]
#[case::invalid_unit("82f", None)]
#[case::nine_digits("123456789H", None)]
#[case::not_a_number("oneH", None)]
#[case::empty("", None)]
fn timeout_header(#[case] header: &str, #[case] parsed: Option<Duration>) {
    let mut metadata = MetadataMap::new();
    metadata.insert("grpc-timeout", assert_ok!(header.parse()));
    assert_eq!(timeout(metadata.as_ref()), parsed, "{header}");
    assert_eq!(timeout(MetadataMap::new().as_ref()), None, "an omitted timeout is an infinite one");
}
