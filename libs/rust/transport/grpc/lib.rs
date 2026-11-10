#![feature(error_reporter)]

#[cfg(feature = "health")]
use core::fmt;

use http::Uri;
#[cfg(feature = "health")]
use tonic::Status;
#[cfg(feature = "health")]
use tonic_health::pb::health_check_response::ServingStatus;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum GrpcOperation {
    #[cfg(feature = "tls")]
    #[display("configure TLS for")]
    Secure,
    #[display("connect to")]
    Connect,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum GrpcError {
    #[cfg(feature = "telemetry")]
    #[error("cannot decode descriptor set {index}")]
    Descriptor { index: usize, source: prost::DecodeError },
    #[cfg(feature = "reflection")]
    #[error("cannot build reflection services")]
    Reflection { source: tonic_reflection::server::Error },
    #[cfg(feature = "tls")]
    #[error("cannot build platform certificate verifier")]
    Verifier { source: rustls::Error },
    #[error("cannot {operation} {uri}")]
    Channel { operation: GrpcOperation, uri: Uri, source: tonic::transport::Error },
    #[cfg(feature = "health")]
    #[error("cannot check health of {}", served(service))]
    Probe { service: String, source: Status },
    #[cfg(feature = "health")]
    #[error("{} reports {}", served(service), status.as_str_name())]
    NotServing { service: String, status: ServingStatus },
}

impl From<GrpcError> for sysexits::ExitCode {
    fn from(value: GrpcError) -> Self {
        match value {
            #[cfg(feature = "telemetry")]
            GrpcError::Descriptor { index: _, source: _ } => Self::DataErr,
            #[cfg(feature = "reflection")]
            GrpcError::Reflection { source: _ } => Self::DataErr,
            #[cfg(feature = "tls")]
            GrpcError::Verifier { source: _ } => Self::OsErr,
            #[cfg(feature = "tls")]
            GrpcError::Channel { operation: GrpcOperation::Secure, uri: _, source: _ } => Self::Config,
            GrpcError::Channel { operation: GrpcOperation::Connect, uri: _, source: _ } => Self::Unavailable,
            #[cfg(feature = "health")]
            GrpcError::Probe { service: _, source: _ } | GrpcError::NotServing { service: _, status: _ } => Self::Unavailable,
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[cfg(feature = "health")]
fn served(service: &str) -> impl fmt::Display { fmt::from_fn(move |formatter| formatter.write_str(if service.is_empty() { "server" } else { service })) }

// --- [COMPOSITION] ---------------------------------------------------------------------

mod client;
mod server;
#[cfg(feature = "telemetry")]
mod telemetry;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use client::connect;
#[cfg(feature = "health")]
pub use client::probe;
#[cfg(feature = "tls")]
pub use client::verified;
#[cfg(feature = "reflection")]
pub use server::reflection;
pub use server::{accept, timeout};
#[cfg(feature = "telemetry")]
pub use telemetry::{Observed, Telemetry, Traced};
