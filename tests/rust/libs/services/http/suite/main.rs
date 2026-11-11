#![feature(error_iter)]

use core::future::ready;
use core::net::SocketAddr;
use core::time::Duration;
use std::io;
use std::sync::Arc;

use axum::Router;
use axum::extract::ConnectInfo;
use axum::routing::{get, post};
use bytes::Bytes;
use claims::assert_ok;
use http::header::HOST;
use http::uri::Authority;
use http::{Request, StatusCode};
use http_body_util::{BodyExt as _, Empty};
use hyper_util::rt::TokioIo;
use nonempty_collections::NEVec;
use rasm_http::{HttpError, Service, accept, bind, serve};
use tokio::io::{AsyncRead, AsyncReadExt as _};
use tokio::net::TcpStream;
use tokio::sync::{Semaphore, mpsc};
use tokio::task::JoinHandle;
use tokio_stream::wrappers::UnboundedReceiverStream;
use tokio_util::sync::CancellationToken;
use tokio_util::task::AbortOnDropHandle;

// --- [CONSTANTS] -----------------------------------------------------------------------

const GREETING: &str = "foo bar baz";
const QUUX: &str = "zar quux";
const GRACE: Duration = Duration::from_secs(30);

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
struct Greeter {
    entered: Semaphore,
    release: Semaphore,
    drained: CancellationToken,
}

impl Service for Greeter {
    type Error = io::Error;

    fn router(self: Arc<Self>, _: CancellationToken) -> Router {
        let held = async move || {
            self.entered.add_permits(1);
            assert_ok!(self.release.acquire().await).forget();
            GREETING
        };
        Router::new()
            .route("/", get(async || GREETING).post(async || GREETING))
            .route("/quux", get(async || QUUX))
            .route("/foo", post(async |body: String| body))
            .route("/peer", get(async |ConnectInfo(peer): ConnectInfo<SocketAddr>| peer.to_string()))
            .route("/held", get(held))
    }

    fn lifetime(&self, stopping: CancellationToken) -> impl Future<Output = Result<(), io::Error>> + Send {
        let drained = self.drained.clone();
        async move {
            stopping.run_until_cancelled(drained.cancelled()).await;
            Ok(())
        }
    }

    fn drain(&self) -> impl Future<Output = Result<(), io::Error>> + Send {
        self.drained.cancel();
        ready(Ok(()))
    }
}

#[derive(Debug)]
struct Running {
    addresses: Vec<SocketAddr>,
    app: Arc<Greeter>,
    stop: mpsc::UnboundedSender<()>,
    served: JoinHandle<Result<(), NEVec<HttpError<io::Error>>>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn running(hosts: &[&str], grace: Duration) -> Running {
    let listen = hosts.iter().map(|host| assert_ok!(host.parse::<Authority>()));
    let bound = assert_ok!(bind::<io::Error>(listen, 0, 128).await);
    let (addresses, listeners) = bound.into_iter().flat_map(|(_, listeners)| listeners).unzip::<_, _, Vec<_>, Vec<_>>();
    let (stop, requests) = mpsc::unbounded_channel();
    let app = Arc::new(Greeter { entered: Semaphore::new(0), release: Semaphore::new(0), drained: CancellationToken::new() });
    let served = tokio::spawn(serve(Arc::clone(&app), async move |router, stopping| accept(listeners, router, stopping).await, UnboundedReceiverStream::new(requests), grace));
    Running { addresses, app, stop, served }
}

async fn read_until(stream: &mut (impl AsyncRead + Unpin), done: impl Fn(&[u8]) -> bool) -> io::Result<Vec<u8>> {
    let mut seen = Vec::new();
    while !done(&seen) {
        if stream.read_buf(&mut seen).await? == 0 {
            return Err(io::Error::new(io::ErrorKind::UnexpectedEof, String::from_utf8_lossy(&seen)));
        }
    }
    Ok(seen)
}

async fn fetched(address: SocketAddr, path: &str) -> (StatusCode, Bytes) {
    let stream = TokioIo::new(assert_ok!(TcpStream::connect(address).await));
    let (mut sender, connection) = assert_ok!(hyper::client::conn::http1::handshake(stream).await);
    let _driven = AbortOnDropHandle::new(tokio::spawn(connection));
    let request = assert_ok!(Request::get(path).header(HOST, "example.domain").body(Empty::<Bytes>::new()));
    let response = assert_ok!(sender.send_request(request).await);
    (response.status(), assert_ok!(response.into_body().collect().await).to_bytes())
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod bind;
mod h2spec;
mod serve;
mod shutdown;
