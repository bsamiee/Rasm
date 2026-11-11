use std::error::Report;
#[cfg(feature = "tls")]
use std::sync::Arc;

use backon::{BackoffBuilder, Retryable as _};
use opentelemetry_semantic_conventions::attribute::{EXCEPTION_MESSAGE, SERVER_ADDRESS, SERVER_PORT};
#[cfg(feature = "tls")]
use rustls::crypto::CryptoProvider;
#[cfg(feature = "tls")]
use rustls::pki_types::CertificateDer;
#[cfg(feature = "tls")]
use rustls_platform_verifier::Verifier;
#[cfg(feature = "tls")]
use tonic::transport::ClientTlsConfig;
use tonic::transport::{Channel, Endpoint};
#[cfg(feature = "health")]
use tonic_health::pb::HealthCheckRequest;
#[cfg(feature = "health")]
use tonic_health::pb::health_check_response::ServingStatus;
#[cfg(feature = "health")]
use tonic_health::pb::health_client::HealthClient;
use tracing::Instrument as _;

use crate::{GrpcError, GrpcOperation};

// --- [OPERATIONS] ----------------------------------------------------------------------

#[cfg(feature = "tls")]
pub fn verified(endpoint: Endpoint, tls: ClientTlsConfig, roots: impl IntoIterator<Item = CertificateDer<'static>>, provider: Arc<CryptoProvider>) -> Result<Endpoint, GrpcError> {
    let verifier = Verifier::new_with_extra_roots(roots, provider).map_err(|source| GrpcError::Verifier { source })?;
    let uri = endpoint.uri().clone();
    endpoint.tls_config_with_verifier(tls, Arc::new(verifier)).map_err(|source| GrpcError::Channel { operation: GrpcOperation::Secure, uri, source })
}

pub async fn connect<B>(endpoint: &Endpoint, retry: B) -> Result<Channel, GrpcError>
where B: BackoffBuilder {
    let uri = endpoint.uri();
    let span = tracing::info_span!("Endpoint.connect", otel.kind = "client", { SERVER_ADDRESS } = uri.host(), { SERVER_PORT } = uri.port_u16());
    (|| endpoint.connect())
        .retry(retry)
        .notify(|error, wait| tracing::debug!(wait = wait.as_secs_f64(), { EXCEPTION_MESSAGE } = %Report::new(error), "connection attempt failed"))
        .instrument(span)
        .await
        .map_err(|source| GrpcError::Channel { operation: GrpcOperation::Connect, uri: uri.clone(), source })
}

#[cfg(feature = "health")]
pub async fn probe(channel: Channel, service: &str) -> Result<(), GrpcError> {
    let answered = HealthClient::new(channel).check(HealthCheckRequest { service: service.to_owned() }).await;
    match answered.map_err(|source| GrpcError::Probe { service: service.to_owned(), source })?.into_inner().status() {
        ServingStatus::Serving => Ok(()),
        status @ (ServingStatus::Unknown | ServingStatus::NotServing | ServingStatus::ServiceUnknown) => Err(GrpcError::NotServing { service: service.to_owned(), status }),
    }
}
