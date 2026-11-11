use core::error::Error;
use core::net::SocketAddr;
use std::collections::BTreeSet;
use std::io;

use claims::{assert_err, assert_ok};
use http::StatusCode;
use http::uri::Authority;
use rasm_http::bind;
use rstest::rstest;
use tokio::io::{AsyncReadExt as _, AsyncWriteExt as _};
use tokio::net::TcpStream;

use crate::{GRACE, GREETING, fetched, running};

// --- [ACCEPT]

#[rstest]
#[case::ip_str("127.0.0.1")]
#[case::host_str("localhost")]
#[case::ipv6_str("[::1]")]
#[tokio::test]
async fn listener_accepts_on_every_address_of(#[case] host: &str) {
    let running = running(&[host], GRACE).await;
    for address in &running.addresses {
        let mut client = assert_ok!(TcpStream::connect(address).await);
        let local = assert_ok!(client.local_addr());
        assert_ok!(client.write_all(b"GET /peer HTTP/1.1\r\nHost: example.domain\r\nConnection: close\r\n\r\n").await);
        let mut answer = String::new();
        assert_ok!(client.read_to_string(&mut answer).await);
        assert!(answer.ends_with(&local.to_string()), "the server's peer address is the client's local address: {answer}");
    }
}

#[tokio::test]
async fn wildcards_of_both_families_share_one_port() {
    let running = running(&["[::]", "0.0.0.0"], GRACE).await;
    let port = assert_ok!(running.addresses.first().map(SocketAddr::port).ok_or("no listener"));
    assert_eq!(
        running.addresses.iter().map(|address| (address.is_ipv6(), address.port())).collect::<BTreeSet<_>>(),
        BTreeSet::from([(false, port), (true, port)]),
        "an IPv6-only wildcard leaves the port to IPv4"
    );
    for loopback in ["127.0.0.1", "[::1]"] {
        let address = assert_ok!(format!("{loopback}:{port}").parse());
        assert_eq!(fetched(address, "/").await, (StatusCode::OK, GREETING.into()), "{loopback} answers on the shared port");
    }
}

#[tokio::test]
async fn taken_port_is_refused_as_in_use() {
    let held = assert_ok!(std::net::TcpListener::bind("127.0.0.1:0"));
    let port = assert_ok!(held.local_addr()).port();
    let refused = assert_err!(bind::<io::Error>([Authority::from_static("127.0.0.1")], port, 128).await);
    let kinds = refused.iter().flat_map(|error| (error as &dyn Error).sources()).filter_map(|cause| cause.downcast_ref::<io::Error>().map(io::Error::kind)).collect::<Vec<_>>();
    assert_eq!(kinds, [io::ErrorKind::AddrInUse], "a port another listener holds fails the bind");
}
