use core::net::SocketAddr;

use claims::{assert_ok, assert_some};
use rasm_infra_tests::{GATEWAY, ran};
use rstest::rstest;

use crate::{GRACE, running};

// --- [CONSTANTS] -----------------------------------------------------------------------

const IMAGE: &str = include_str!("../Dockerfile");

// --- [CONFORMANCE]

#[rstest]
#[case::generic(&["generic"])]
#[case::hpack(&["hpack"])]
#[case::http2(&["http2/3.5/1", "http2/4", "http2/5", "http2/6", "http2/7", "http2/8"])]
#[tokio::test]
async fn h2spec(#[case] specs: &[&str]) {
    let running = running(&["0.0.0.0"], GRACE).await;
    let port = assert_some!(running.addresses.first().map(SocketAddr::port)).to_string();
    let outcome = assert_ok!(ran("rasm-h2spec", IMAGE, specs.iter().copied().chain(["--host", GATEWAY, "--port", port.as_str(), "--timeout", "5"])).await);
    assert_eq!(outcome.code, Some(0), "h2spec {specs:?} passed:\n{}", outcome.output);
}
