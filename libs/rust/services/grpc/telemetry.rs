use core::any;
use core::pin::Pin;
use core::task::{Context, Poll};
use std::collections::HashSet;
use std::sync::{Arc, LazyLock};

use http::{Request, Response, Uri};
use http_body::{Body, Frame, SizeHint};
use nonempty_collections::NEVec;
use opentelemetry::metrics::Histogram;
use opentelemetry::{KeyValue, StringValue, global};
use opentelemetry_http::{HeaderExtractor, HeaderInjector};
use opentelemetry_semantic_conventions::attribute::{ERROR_TYPE, RPC_METHOD, RPC_METHOD_ORIGINAL, RPC_RESPONSE_STATUS_CODE, RPC_SYSTEM_NAME, SERVER_ADDRESS, SERVER_PORT};
use opentelemetry_semantic_conventions::metric::{RPC_CLIENT_CALL_DURATION, RPC_SERVER_CALL_DURATION};
use pin_project_lite::pin_project;
use prost::Message as _;
use prost_types::FileDescriptorSet;
use rasm_telemetry::Call;
use tonic::{Code, GrpcMethod, Status};
use tower::{Layer, Service};
use tracing::field::Empty;
use tracing_opentelemetry::OpenTelemetrySpanExt as _;
use validated::Validated;

use crate::GrpcError;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Debug)]
enum Role {
    Server { methods: Arc<HashSet<Arc<str>>> },
    Client { address: Option<Arc<str>>, port: Option<u16> },
}

#[derive(Debug, Clone, Copy)]
enum Outcome {
    Status(Code),
    Failure(&'static str),
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone, Debug)]
pub struct Telemetry {
    role: Role,
}

impl Telemetry {
    pub fn server<'set>(descriptors: impl IntoIterator<Item = &'set [u8]>) -> Result<Self, NEVec<GrpcError>> {
        let sets =
            descriptors.into_iter().enumerate().map(|(index, set)| FileDescriptorSet::decode(set).map_err(|source| GrpcError::Descriptor { index, source })).collect::<Validated<Vec<_>, _>>().ok()?;
        let services = sets.iter().flat_map(|set| &set.file).flat_map(|file| file.service.iter().map(move |service| (file.package(), service)));
        let methods = services.flat_map(|(package, service)| {
            let owner = if package.is_empty() { service.name().to_owned() } else { format!("{package}.{}", service.name()) };
            service.method.iter().map(move |method| Arc::from(format!("{owner}/{}", method.name())))
        });
        Ok(Self { role: Role::Server { methods: Arc::new(methods.collect()) } })
    }

    #[must_use]
    pub fn client(target: &Uri) -> Self { Self { role: Role::Client { address: target.host().map(Arc::from), port: target.port_u16() } } }
}

impl<S> Layer<S> for Telemetry {
    type Service = Traced<S>;

    fn layer(&self, inner: S) -> Traced<S> { Traced { service: inner, telemetry: self.clone() } }
}

#[derive(Clone, Debug)]
pub struct Traced<S> {
    service: S,
    telemetry: Telemetry,
}

impl<S, Q, B> Service<Request<Q>> for Traced<S>
where S: Service<Request<Q>, Response = Response<B>>
{
    type Error = S::Error;
    type Future = Observed<S::Future>;
    type Response = Response<Observed<B>>;

    fn poll_ready(&mut self, cx: &mut Context<'_>) -> Poll<Result<(), S::Error>> { self.service.poll_ready(cx) }

    fn call(&mut self, mut req: Request<Q>) -> Observed<S::Future> {
        const SYSTEM: &str = "grpc";
        const OTHER: &str = "_OTHER";
        let path = req.uri().path();
        let name = path.trim_prefix('/');
        let (client, recognized, address, port) = match &self.telemetry.role {
            Role::Server { methods } => (false, methods.get(name).cloned(), None, None),
            Role::Client { address, port } => (true, req.extensions().get::<GrpcMethod<'static>>().map(|_| Arc::from(name)), address.clone(), *port),
        };
        let original = recognized.is_none().then(|| name.to_owned());
        let method = recognized.map_or_else(|| StringValue::from(OTHER), StringValue::from);
        let span = tracing::info_span!(
            "call",
            otel.name = method.as_str(),
            otel.kind = if client { "client" } else { "server" },
            { RPC_SYSTEM_NAME } = SYSTEM,
            { RPC_METHOD } = method.as_str(),
            { RPC_METHOD_ORIGINAL } = original,
            { SERVER_ADDRESS } = address.as_deref(),
            { SERVER_PORT } = port,
            { RPC_RESPONSE_STATUS_CODE } = Empty,
            { ERROR_TYPE } = Empty,
        );
        let described = [
            Some(KeyValue::new(RPC_SYSTEM_NAME, SYSTEM)),
            Some(KeyValue::new(RPC_METHOD, method)),
            address.map(|host| KeyValue::new(SERVER_ADDRESS, host)),
            port.map(|port| KeyValue::new(SERVER_PORT, i64::from(port))),
        ];
        if client {
            global::get_text_map_propagator(|propagator| propagator.inject_context(&span.context(), &mut HeaderInjector(req.headers_mut())));
        }
        let call = Call::start(span, (!client).then(|| HeaderExtractor(req.headers())).as_ref(), described.into_iter().flatten());
        let inner = call.span.in_scope(|| self.service.call(req));
        Observed { inner, call: Some((call, client)) }
    }
}

pin_project! {
    #[derive(Debug)]
    pub struct Observed<T> {
        #[pin]
        inner: T,
        call: Option<(Call, bool)>,
    }

    impl<T> PinnedDrop for Observed<T> {
        fn drop(this: Pin<&mut Self>) {
            if let Some(call) = this.project().call.take() {
                finished(call, Outcome::Status(Code::Cancelled));
            }
        }
    }
}

impl<F, B, E> Future for Observed<F>
where F: Future<Output = Result<Response<B>, E>>
{
    type Output = Result<Response<Observed<B>>, E>;

    fn poll(self: Pin<&mut Self>, cx: &mut Context<'_>) -> Poll<Self::Output> {
        let this = self.project();
        let (inner, held) = (this.inner, this.call);
        let polled = match held.as_ref() {
            Some((call, _)) => call.span.in_scope(|| inner.poll(cx)),
            None => inner.poll(cx),
        };
        polled.map(|answered| match answered {
            Ok(response) => {
                let call = match (held.take(), response.headers().get(Status::GRPC_STATUS)) {
                    (Some(call), Some(status)) => {
                        finished(call, Outcome::Status(Code::from_bytes(status.as_bytes())));
                        None
                    }
                    (call, Some(_) | None) => call,
                };
                Ok(response.map(|inner| Observed { inner, call }))
            }
            Err(error) => {
                if let Some(call) = held.take() {
                    finished(call, Outcome::Failure(any::type_name::<E>()));
                }
                Err(error)
            }
        })
    }
}

impl<B> Body for Observed<B>
where B: Body
{
    type Data = B::Data;
    type Error = B::Error;

    fn poll_frame(self: Pin<&mut Self>, cx: &mut Context<'_>) -> Poll<Option<Result<Frame<B::Data>, B::Error>>> {
        let this = self.project();
        let (inner, held) = (this.inner, this.call);
        let polled = match held.as_ref() {
            Some((call, _)) => call.span.in_scope(|| inner.poll_frame(cx)),
            None => inner.poll_frame(cx),
        };
        let ended = match &polled {
            Poll::Ready(Some(Ok(frame))) => frame.trailers_ref().map(|trailers| Outcome::Status(trailers.get(Status::GRPC_STATUS).map_or(Code::Unknown, |status| Code::from_bytes(status.as_bytes())))),
            Poll::Ready(Some(Err(_))) => Some(Outcome::Failure(any::type_name::<B::Error>())),
            Poll::Ready(None) => Some(Outcome::Status(Code::Unknown)),
            Poll::Pending => None,
        };
        if let Some((outcome, call)) = ended.and_then(|outcome| Some((outcome, held.take()?))) {
            finished(call, outcome);
        }
        polled
    }

    fn is_end_stream(&self) -> bool { self.inner.is_end_stream() }

    fn size_hint(&self) -> SizeHint { self.inner.size_hint() }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn finished((call, client): (Call, bool), outcome: Outcome) {
    const fn token(code: Code) -> &'static str {
        match code {
            Code::Ok => "OK",
            Code::Cancelled => "CANCELLED",
            Code::Unknown => "UNKNOWN",
            Code::InvalidArgument => "INVALID_ARGUMENT",
            Code::DeadlineExceeded => "DEADLINE_EXCEEDED",
            Code::NotFound => "NOT_FOUND",
            Code::AlreadyExists => "ALREADY_EXISTS",
            Code::PermissionDenied => "PERMISSION_DENIED",
            Code::ResourceExhausted => "RESOURCE_EXHAUSTED",
            Code::FailedPrecondition => "FAILED_PRECONDITION",
            Code::Aborted => "ABORTED",
            Code::OutOfRange => "OUT_OF_RANGE",
            Code::Unimplemented => "UNIMPLEMENTED",
            Code::Internal => "INTERNAL",
            Code::Unavailable => "UNAVAILABLE",
            Code::DataLoss => "DATA_LOSS",
            Code::Unauthenticated => "UNAUTHENTICATED",
        }
    }
    const BOUNDARIES: [f64; 14] = [0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1.0, 2.5, 5.0, 7.5, 10.0];
    static DURATION: LazyLock<[Histogram<f64>; 2]> =
        LazyLock::new(|| [RPC_SERVER_CALL_DURATION, RPC_CLIENT_CALL_DURATION].map(|name| rasm_telemetry::duration(env!("CARGO_PKG_NAME"), name, BOUNDARIES.to_vec())));
    let (status, failure) = match outcome {
        Outcome::Status(code) => {
            let failed = if client { code != Code::Ok } else { matches!(code, Code::Unknown | Code::DeadlineExceeded | Code::Unimplemented | Code::Internal | Code::Unavailable | Code::DataLoss) };
            (Some(token(code)), failed.then(|| token(code)))
        }
        Outcome::Failure(kind) => (None, Some(kind)),
    };
    let [served, called] = &*DURATION;
    call.finish(if client { called } else { served }, status.map(StringValue::from), failure.map(|kind| (kind.into(), kind.into())));
}
