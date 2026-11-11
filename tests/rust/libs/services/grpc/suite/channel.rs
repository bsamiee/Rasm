use core::time::Duration;

use backon::ConstantBuilder;
use claims::{assert_err, assert_ok};
use futures::future;
use rasm_grpc::{accept, connect, probe};
use tokio::net::TcpListener;
use tokio::time;
use tokio_util::sync::CancellationToken;
use tonic::service::Routes;
use tonic::transport::server::TcpIncoming;
use tonic::transport::{Endpoint, Server};
use tonic_health::server::health_reporter;

// --- [CONNECT]

#[tokio::test]
async fn connect_returns_err() {
    let unreachable = Endpoint::from_static("http://thisdoesntexist.test");
    assert_err!(connect(&unreachable, ConstantBuilder::new().with_delay(Duration::from_millis(10)).with_max_times(2)).await);
}

#[tokio::test]
async fn connect_reaches_a_server_that_starts_late() {
    let address = assert_ok!(assert_ok!(TcpListener::bind("127.0.0.1:0").await).local_addr());
    let endpoint = assert_ok!(Endpoint::from_shared(format!("http://{address}")));
    let stop = CancellationToken::new();
    let late = async {
        time::sleep(Duration::from_millis(300)).await;
        let listener = assert_ok!(TcpListener::bind(address).await);
        let (_reporter, health) = health_reporter();
        accept(Server::builder(), [TcpIncoming::from(listener)], Routes::new(health).into_axum_router(), None, stop.clone()).await
    };
    let reached = async {
        let channel = assert_ok!(connect(&endpoint, ConstantBuilder::new().with_delay(Duration::from_millis(20)).with_max_times(200)).await, "the channel retries until the server listens");
        assert_ok!(probe(channel, "").await);
        stop.cancel();
    };
    let (ended, ()) = future::join(late, reached).await;
    assert!(ended.iter().all(Result::is_ok), "{ended:?}");
}
