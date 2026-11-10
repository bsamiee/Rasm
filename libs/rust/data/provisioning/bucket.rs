use core::future::ready;
use std::error::Report;

use aws_sdk_s3::error::SdkError;
use aws_sdk_s3::operation::get_bucket_tagging::GetBucketTaggingOutput;
use aws_sdk_s3::operation::list_buckets::ListBucketsOutput;
use aws_sdk_s3::types::{Tag, Tagging};
use aws_sdk_sts::types::Credentials;
use data_encoding::BASE64URL_NOPAD;
use futures::future::join_all;
use http::{HeaderName, HeaderValue, StatusCode};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use serde::Serialize;

use crate::stores::{Create, Server, Stores, decoded, encoded, named};
use crate::{Kind, Label, Operation, ProvisioningError, Refusal, Removal, StoreId};

// --- [CONSTANTS] -----------------------------------------------------------------------

const LABEL: &str = env!("CARGO_PKG_NAME");

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Granted {
    pub id: StoreId,
    pub credentials: Credentials,
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
struct Policy {
    version: &'static str,
    statement: [Statement; 1],
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
struct Statement {
    effect: &'static str,
    action: [&'static str; 1],
    resource: [String; 2],
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Bucket {
    pub s3: aws_sdk_s3::Client,
    pub sts: aws_sdk_sts::Client,
    pub duration_seconds: i32,
    pub force: (HeaderName, HeaderValue),
}

impl Bucket {
    pub async fn issue(&self, id: StoreId) -> Result<Granted, ProvisioningError> {
        let name = id.to_string();
        let policy = Policy { version: "2012-10-17", statement: [Statement { effect: "Allow", action: ["s3:*"], resource: [format!("arn:aws:s3:::{name}"), format!("arn:aws:s3:::{name}/*")] }] };
        let assumed = self.sts.assume_role().role_arn(format!("arn:aws:iam:::role/{name}")).role_session_name(&name).duration_seconds(self.duration_seconds).policy(encoded(id, &policy)?);
        let issued = assumed.send().await.map_err(|source| ProvisioningError::Credentials { operation: Operation::Issue(id), source: Box::new(source.into()) })?;
        issued.credentials.map(|credentials| Granted { id, credentials }).ok_or(ProvisioningError::Store { id, refusal: Refusal::Unissued })
    }

    async fn tagged(&self, id: StoreId, label: &Label, operation: Operation) -> Result<(), ProvisioningError> {
        let refused = |source: aws_sdk_s3::Error| ProvisioningError::Bucket { operation, source: Box::new(source) };
        let tag = Tag::builder().key(LABEL).value(BASE64URL_NOPAD.encode(encoded(id, label)?.as_bytes())).build().map_err(|source| refused(source.into()))?;
        let tagging = Tagging::builder().tag_set(tag).build().map_err(|source| refused(source.into()))?;
        self.s3.put_bucket_tagging().bucket(id.to_string()).tagging(tagging).send().await.map(drop).map_err(|source| refused(source.into()))
    }
}

impl Server for Bucket {
    fn kind(&self) -> Kind { Kind::Bucket }

    fn limit(&self) -> impl Future<Output = Result<Option<u32>, ProvisioningError>> + Send { ready(Ok(None)) }

    async fn stores(&self) -> Result<Vec<(StoreId, Option<Label>)>, ProvisioningError> {
        let refused = |source: Box<aws_sdk_s3::Error>| ProvisioningError::Bucket { operation: Operation::List, source };
        let pages = self.s3.list_buckets().into_paginator().send().try_collect().await.map_err(|source| refused(Box::new(source.into())))?;
        let ids = pages.iter().flat_map(ListBucketsOutput::buckets).filter_map(|bucket| bucket.name().and_then(named));
        let labeled = join_all(ids.map(async |id| {
            let tagging = self.s3.get_bucket_tagging().bucket(id.to_string()).send().await.map(Some).or_else(|source| absent(source).map(|()| None))?;
            let held = tagging.iter().flat_map(GetBucketTaggingOutput::tag_set).find(|tag| tag.key() == LABEL);
            let record =
                held.and_then(|tag| BASE64URL_NOPAD.decode(tag.value().as_bytes()).inspect_err(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), %id, "store tag unreadable")).ok());
            Ok((id, record.and_then(|record| decoded(id, &record))))
        }));
        labeled.await.into_iter().try_collect().map_err(refused)
    }

    async fn label(&self, id: StoreId, label: &Label) -> Result<(), ProvisioningError> { self.tagged(id, label, Operation::Label(id)).await }

    async fn remove(&self, id: StoreId, _: Removal) -> Result<(), ProvisioningError> {
        let (name, value) = self.force.clone();
        let forced = self.s3.delete_bucket().bucket(id.to_string()).customize().mutate_request(move |request| drop(request.headers_mut().insert(name.clone(), value.clone())));
        forced.send().await.map(drop).or_else(absent).map_err(|source| ProvisioningError::Bucket { operation: Operation::Remove(id), source })
    }
}

impl Create for Bucket {
    type Access = Granted;

    async fn create(&self, id: StoreId, label: &Label) -> Result<Granted, ProvisioningError> {
        let created = self.s3.create_bucket().bucket(id.to_string()).send().await;
        created.map_err(|source| ProvisioningError::Bucket { operation: Operation::Create(id), source: Box::new(source.into()) })?;
        self.tagged(id, label, Operation::Create(id)).await?;
        self.issue(id).await
    }
}

impl Stores<Bucket> {
    pub async fn renew(&self, id: StoreId) -> Result<Granted, ProvisioningError> {
        self.registry.holds(Kind::Bucket, id).ok_or(ProvisioningError::Store { id, refusal: Refusal::Missing })?;
        self.server.issue(id).await
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn absent<E>(source: SdkError<E>) -> Result<(), Box<aws_sdk_s3::Error>>
where aws_sdk_s3::Error: From<SdkError<E>> {
    match source.raw_response().map(|response| response.status().as_u16()) {
        Some(status) if status == StatusCode::NOT_FOUND.as_u16() => Ok(()),
        Some(_) | None => Err(Box::new(source.into())),
    }
}
