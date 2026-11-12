#![feature(error_iter)]

use core::future::ready;
use core::net::SocketAddr;
use core::time::Duration;
use std::io;
use std::sync::Arc;

use claims::assert_ok;
use futures::future::BoxFuture;
use futures::stream::{self, BoxStream};
use futures::{FutureExt as _, StreamExt as _};
use rasm_grpc::accept;
use tokio::net::TcpListener;
use tokio::sync::Semaphore;
use tokio::task::JoinHandle;
use tokio::time;
use tokio_util::sync::CancellationToken;
use tonic::service::Routes;
use tonic::transport::server::TcpIncoming;
use tonic::transport::{Channel, Endpoint, Server};
use tonic::{Request, Response, Status};
use tonic_health::pb::health_check_response::ServingStatus;
use tonic_health::pb::health_client::HealthClient;
use tonic_health::pb::health_server::{Health, HealthServer};
use tonic_health::pb::{HealthCheckRequest, HealthCheckResponse};

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIMIT: Duration = Duration::from_secs(5);

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
struct Left(Arc<Semaphore>);

impl Drop for Left {
    fn drop(&mut self) { self.0.add_permits(1); }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum Pace {
    Prompt,
    Slow,
    Held,
}

#[derive(Debug)]
struct Probed {
    pace: Pace,
    entered: Semaphore,
    release: Semaphore,
    left: Arc<Semaphore>,
}

impl Health for Probed {
    type WatchStream = BoxStream<'static, Result<HealthCheckResponse, Status>>;

    fn check<'probed, 'call>(&'probed self, _: Request<HealthCheckRequest>) -> BoxFuture<'call, Result<Response<HealthCheckResponse>, Status>>
    where
        'probed: 'call,
        Self: 'call,
    {
        const LATENCY: Duration = Duration::from_secs(1);
        let paced = async move {
            match self.pace {
                Pace::Prompt => (),
                Pace::Slow => time::sleep(LATENCY).await,
                Pace::Held => {
                    let _left = Left(Arc::clone(&self.left));
                    self.entered.add_permits(1);
                    self.release.acquire().await.map_err(|closed| Status::internal(closed.to_string()))?.forget();
                }
            }
            Ok(Response::new(HealthCheckResponse { status: ServingStatus::Serving.into() }))
        };
        paced.boxed()
    }

    fn watch<'probed, 'call>(&'probed self, _: Request<HealthCheckRequest>) -> BoxFuture<'call, Result<Response<Self::WatchStream>, Status>>
    where
        'probed: 'call,
        Self: 'call,
    {
        let open = stream::once(ready(Ok(HealthCheckResponse { status: ServingStatus::Serving.into() }))).chain(stream::pending());
        ready(Ok(Response::new(open.boxed()))).boxed()
    }
}

#[derive(Debug)]
struct Running {
    address: SocketAddr,
    probed: Arc<Probed>,
    stop: CancellationToken,
    served: JoinHandle<Vec<io::Error>>,
}

impl Running {
    async fn client(&self) -> HealthClient<Channel> { HealthClient::new(assert_ok!(assert_ok!(Endpoint::from_shared(format!("http://{}", self.address))).connect().await)) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn running(limit: Option<Duration>, pace: Pace, mounted: impl FnOnce(Routes) -> Routes) -> Running {
    let listener = assert_ok!(TcpListener::bind("127.0.0.1:0").await);
    let address = assert_ok!(listener.local_addr());
    let probed = Arc::new(Probed { pace, entered: Semaphore::new(0), release: Semaphore::new(0), left: Arc::new(Semaphore::new(0)) });
    let axum = mounted(Routes::new(HealthServer::from_arc(Arc::clone(&probed)))).into_axum_router();
    let stop = CancellationToken::new();
    let accepting = accept(Server::builder(), [TcpIncoming::from(listener)], axum, limit, stop.clone());
    Running { address, probed, stop, served: tokio::spawn(accepting.map(|ended| ended.into_iter().filter_map(Result::err).collect())) }
}

fn asked(service: &str) -> Request<HealthCheckRequest> { Request::new(HealthCheckRequest { service: service.to_owned() }) }

// --- [COMPOSITION] ---------------------------------------------------------------------

mod channel;
mod deadline;
mod health;
mod reflection;
mod stop;
