use claims::{assert_ok, assert_some};
use rasm_grpc::reflection;
use tonic::transport::Endpoint;
use tonic::{Code, Request, Status};
use tonic_reflection::pb::v1::ServerReflectionRequest;
use tonic_reflection::pb::v1::server_reflection_client::ServerReflectionClient;
use tonic_reflection::pb::v1::server_reflection_request::MessageRequest;
use tonic_reflection::pb::v1::server_reflection_response::MessageResponse;
use tonic_reflection::pb::v1alpha;

use crate::{Pace, running};

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn reflected(request: MessageRequest) -> Result<MessageResponse, Status> {
    let running = running(None, Pace::Prompt, |_| assert_ok!(reflection([tonic_health::pb::FILE_DESCRIPTOR_SET]))).await;
    let channel = assert_ok!(assert_ok!(Endpoint::from_shared(format!("http://{}", running.address))).connect().await);
    let asked = ServerReflectionRequest { host: String::new(), message_request: Some(request) };
    let mut inbound = assert_ok!(ServerReflectionClient::new(channel).server_reflection_info(Request::new(tokio_stream::once(asked))).await).into_inner();
    let answer = assert_some!(assert_some!(inbound.message().await?).message_response);
    assert!(assert_ok!(inbound.message().await).is_none(), "one answer per request");
    Ok(answer)
}

// --- [V1]

#[tokio::test]
async fn list_services() {
    let answer = assert_ok!(reflected(MessageRequest::ListServices(String::new())).await);
    let names = match &answer {
        MessageResponse::ListServicesResponse(listed) => Vec::from_iter(listed.service.iter().map(|service| service.name.as_str())),
        MessageResponse::FileDescriptorResponse(_) | MessageResponse::AllExtensionNumbersResponse(_) | MessageResponse::ErrorResponse(_) => Vec::new(),
    };
    assert!(names.contains(&"grpc.health.v1.Health") && names.contains(&"grpc.reflection.v1.ServerReflection"), "{answer:?}");
}

#[tokio::test]
async fn file_containing_symbol() {
    let answer = assert_ok!(reflected(MessageRequest::FileContainingSymbol("grpc.health.v1.Health".to_owned())).await);
    assert!(matches!(&answer, MessageResponse::FileDescriptorResponse(found) if found.file_descriptor_proto.first().is_some_and(|file| !file.is_empty())), "{answer:?}");
}

#[tokio::test]
async fn file_by_filename() {
    let answer = assert_ok!(reflected(MessageRequest::FileByFilename("reflection_v1.proto".to_owned())).await);
    assert!(matches!(&answer, MessageResponse::FileDescriptorResponse(found) if found.file_descriptor_proto.first().is_some_and(|file| !file.is_empty())), "{answer:?}");
}

#[tokio::test]
async fn unknown_file_is_not_found_in_the_response() {
    let answer = assert_ok!(reflected(MessageRequest::FileByFilename("no_such_file.proto".to_owned())).await, "an error is one response on the open stream");
    assert!(matches!(&answer, MessageResponse::ErrorResponse(missing) if Code::from_i32(missing.error_code) == Code::NotFound), "error code 5, NOT FOUND: {answer:?}");
}

// --- [V1ALPHA]

#[tokio::test]
async fn v1alpha() {
    let running = running(None, Pace::Prompt, |_| assert_ok!(reflection([tonic_health::pb::FILE_DESCRIPTOR_SET]))).await;
    let channel = assert_ok!(assert_ok!(Endpoint::from_shared(format!("http://{}", running.address))).connect().await);
    let asked = v1alpha::ServerReflectionRequest { host: String::new(), message_request: Some(v1alpha::server_reflection_request::MessageRequest::ListServices(String::new())) };
    let mut inbound = assert_ok!(v1alpha::server_reflection_client::ServerReflectionClient::new(channel).server_reflection_info(Request::new(tokio_stream::once(asked))).await).into_inner();
    let answer = assert_some!(assert_some!(assert_ok!(inbound.message().await)).message_response);
    assert!(
        matches!(&answer, v1alpha::server_reflection_response::MessageResponse::ListServicesResponse(listed) if listed.service.iter().any(|service| service.name == "grpc.reflection.v1alpha.ServerReflection")),
        "{answer:?}"
    );
}
