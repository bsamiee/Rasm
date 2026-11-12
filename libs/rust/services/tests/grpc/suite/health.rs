use core::error::Error;

use claims::{assert_err, assert_ok};
use rasm_grpc::probe;
use tonic::transport::Endpoint;
use tonic::{Code, Status};
use tonic_health::ServingStatus;
use tonic_health::server::health_reporter;

use crate::{Pace, running};

// --- [CHECK]

#[tokio::test]
async fn service_check() {
    let (reporter, health) = health_reporter();
    let running = running(None, Pace::Prompt, |_| tonic::service::Routes::new(health)).await;
    let channel = assert_ok!(assert_ok!(Endpoint::from_shared(format!("http://{}", running.address))).connect().await);
    assert_ok!(probe(channel.clone(), "").await, "the empty service name is the server's overall health, serving from the start");
    let unregistered = assert_err!(probe(channel.clone(), "Unregistered").await);
    let codes = (&unregistered as &dyn Error).sources().filter_map(|cause| cause.downcast_ref::<Status>().map(Status::code)).collect::<Vec<_>>();
    assert_eq!(codes, [Code::NotFound], "an unregistered service name answers NOT_FOUND: {unregistered:?}");
    reporter.set_service_status("TestService", ServingStatus::NotServing).await;
    assert_err!(probe(channel.clone(), "TestService").await, "a service that is not serving fails its check");
    reporter.set_service_status("TestService", ServingStatus::Serving).await;
    assert_ok!(probe(channel, "TestService").await);
}
