use core::iter;
use core::pin::Pin;
use core::task::{Context, Poll};
use core::time::Duration;
use std::io;

use axum::Router;
use axum::middleware::{self, Next};
use bytes::Bytes;
use futures::Stream;
use futures::future::join_all;
use http::{HeaderMap, HeaderValue, Request, Response};
use http_body::{Body, Frame, SizeHint};
use pin_project_lite::pin_project;
use tokio::io::{AsyncRead, AsyncWrite};
use tokio::time::{self, Instant, Sleep};
use tokio_util::sync::{CancellationToken, WaitForCancellationFutureOwned};
use tonic::service::Routes;
use tonic::transport::Server;
use tonic::transport::server::Connected;
use tonic::{Code, Status};
#[cfg(feature = "reflection")]
use tonic_reflection::server::Builder;
use tower::{BoxError, Layer};

#[cfg(feature = "reflection")]
use crate::GrpcError;

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXPIRED: &str = "deadline expired before the server returned status";

// --- [MODELS] --------------------------------------------------------------------------

pin_project! {
    struct Ending<B> {
        #[pin]
        body: B,
        #[pin]
        expiry: Option<Sleep>,
        #[pin]
        stopped: WaitForCancellationFutureOwned,
        ended: bool,
    }
}

impl<B> Body for Ending<B>
where B: Body
{
    type Data = B::Data;
    type Error = B::Error;

    fn poll_frame(self: Pin<&mut Self>, cx: &mut Context<'_>) -> Poll<Option<Result<Frame<B::Data>, B::Error>>> {
        let this = self.project();
        match if *this.ended { Poll::Ready(None) } else { this.body.poll_frame(cx) } {
            Poll::Pending => {
                let ended = match this.expiry.as_pin_mut().map_or(Poll::Pending, |expiry| expiry.poll(cx)) {
                    Poll::Ready(()) => Poll::Ready((Code::DeadlineExceeded, EXPIRED)),
                    Poll::Pending => this.stopped.poll(cx).map(|()| (Code::Unavailable, "server is shutting down")),
                };
                ended.map(|(code, message)| {
                    *this.ended = true;
                    let status = [(Status::GRPC_STATUS, HeaderValue::from(i32::from(code))), (Status::GRPC_MESSAGE, HeaderValue::from_static(message))];
                    Some(Ok(Frame::trailers(HeaderMap::from_iter(status))))
                })
            }
            ready @ Poll::Ready(_) => ready,
        }
    }

    fn is_end_stream(&self) -> bool { self.ended || self.body.is_end_stream() }

    fn size_hint(&self) -> SizeHint { self.body.size_hint() }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn timeout(headers: &HeaderMap) -> Option<Duration> {
    let (unit, digits) = headers.get("grpc-timeout")?.as_bytes().split_last()?;
    let read = |total: u64, digit: &u8| char::from(*digit).to_digit(10).and_then(|digit| total.checked_mul(10)?.checked_add(u64::from(digit)));
    let amount = digits.iter().try_fold(0, read).filter(|_| (1..=8).contains(&digits.len()))?;
    match unit {
        b'H' => Some(Duration::from_hours(amount)),
        b'M' => Some(Duration::from_mins(amount)),
        b'S' => Some(Duration::from_secs(amount)),
        b'm' => Some(Duration::from_millis(amount)),
        b'u' => Some(Duration::from_micros(amount)),
        b'n' => Some(Duration::from_nanos(amount)),
        _ => None,
    }
}

#[cfg(feature = "reflection")]
pub fn reflection<'set>(descriptors: impl IntoIterator<Item = &'set [u8]>) -> Result<Routes, GrpcError> {
    let sets = Vec::from_iter(descriptors);
    let registered = || sets.iter().copied().fold(Builder::configure(), Builder::register_encoded_file_descriptor_set);
    registered().build_v1().and_then(|v1| Ok(Routes::new(v1).add_service(registered().build_v1alpha()?))).map_err(|source| GrpcError::Reflection { source })
}

pub async fn accept<L, B, I, IO, IE>(server: Server<L>, incoming: impl IntoIterator<Item = I>, router: Router, limit: Option<Duration>, stopping: CancellationToken) -> Vec<io::Result<()>>
where
    L: Layer<Routes> + Clone,
    L::Service: tower::Service<Request<tonic::body::Body>, Response = Response<B>> + Clone + Send + 'static,
    <L::Service as tower::Service<Request<tonic::body::Body>>>::Future: Send,
    <L::Service as tower::Service<Request<tonic::body::Body>>>::Error: Into<BoxError> + Send + 'static,
    B: Body<Data = Bytes> + Send + 'static,
    B::Error: Into<BoxError>,
    I: Stream<Item = Result<IO, IE>>,
    IO: AsyncRead + AsyncWrite + Connected + Unpin + Send + 'static,
    IE: Into<BoxError>,
{
    let ending = stopping.clone();
    let bounded = move |request: Request<axum::body::Body>, next: Next| {
        let (allowed, started, stopped) = (timeout(request.headers()), Instant::now(), ending.clone().cancelled_owned());
        let deadline = |allowed: Option<Duration>| allowed.and_then(|allowed| started.checked_add(allowed));
        let (whole, start) = (deadline(allowed), deadline(iter::chain(allowed, limit).min()));
        async move {
            let answered = match start {
                Some(start) => time::timeout_at(start, next.run(request)).await.ok(),
                None => Some(next.run(request).await),
            };
            answered.map_or_else(
                || Status::deadline_exceeded(EXPIRED).into_http(),
                |response| response.map(|body| axum::body::Body::new(Ending { body, expiry: whole.map(time::sleep_until), stopped, ended: false })),
            )
        }
    };
    let services = Routes::from(router.layer(middleware::from_fn(bounded)));
    let accepting = incoming.into_iter().map(|accepted| server.clone().serve_with_incoming_shutdown(services.clone(), accepted, stopping.clone().cancelled_owned()));
    join_all(accepting).await.into_iter().map(|closed| closed.map_err(io::Error::other)).collect()
}
