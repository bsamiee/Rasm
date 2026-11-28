use core::sync::atomic::{AtomicUsize, Ordering};
use std::io;
use std::path::Path;
use std::sync::Arc;

use claims::{assert_err, assert_matches, assert_none, assert_ok, assert_some};
use futures::future;
use interprocess::local_socket::tokio::Stream;
use interprocess::local_socket::traits::tokio::Listener as _;
use rasm_ipc::{Endpoint, Instance, IpcError, IpcOperation, Opened, Refusal, accept, attach, connect, reply};
use rasm_testing::Failure;
use tokio::io::AsyncWriteExt as _;
use tokio::sync::mpsc;
use tokio_util::task::{AbortOnDropHandle, TaskTracker};
use validated::Validated;

use crate::{BUILD, LIMITS, PROTOCOL, brief, hello, scoped};

// --- [ABSENT]

#[tokio::test]
async fn absent_service_is_no_connection_and_is_started_once() -> Result<(), Failure> {
    scoped(async |endpoint: Endpoint, _: &Path| {
        assert_none!(assert_ok!(attach::<_, _, String>(&endpoint, &hello("nobody"), LIMITS).await), "attaching where no service listens opens nothing");
        let starts = AtomicUsize::new(0);
        let start = || {
            starts.fetch_add(1, Ordering::Relaxed);
            Ok(())
        };
        let refused = assert_err!(connect::<_, _, _, String>(&endpoint, &hello("nobody"), LIMITS, brief(), start).await);
        assert_matches!(refused, IpcError::Io { operation: IpcOperation::Connect, source } if source.kind() == io::ErrorKind::NotFound);
        assert_eq!(starts.load(Ordering::Relaxed), 1, "a service that never comes up is started once across every retry");
    })
    .await
}

// --- [REPLACED]

#[tokio::test]
async fn answer_cut_short_by_a_closing_service_is_asked_again() -> Result<(), Failure> {
    scoped(async |endpoint: Endpoint, _: &Path| {
        let instance = assert_some!(assert_ok!(Instance::claim(&endpoint, brief()).await));
        let serving = AbortOnDropHandle::new(tokio::spawn(async move {
            let Opened { body: _, reader: _, writer: mut cut } = accept::<_, u64, String>(instance.listener.accept().await?, PROTOCOL, &BUILD, LIMITS).await.map_err(io::Error::other)?;
            cut.write_all(br#"{"Welcome":"cut"#).await?;
            drop(cut);
            let Opened { body, reader: _, mut writer } = accept::<_, u64, String>(instance.listener.accept().await?, PROTOCOL, &BUILD, LIMITS).await.map_err(io::Error::other)?;
            reply(&mut writer, Ok::<_, Refusal>(format!("whole {body}"))).await.map_err(io::Error::other)
        }));
        let opened =
            assert_ok!(connect::<_, _, _, String>(&endpoint, &hello("again"), LIMITS, brief(), || Ok(())).await, "a service that closes mid-answer, as one being replaced does, is reached again");
        assert_eq!(opened.body, "whole again");
        assert_ok!(assert_ok!(serving.await));
    })
    .await
}

#[tokio::test]
async fn refusal_from_a_superseded_service_is_asked_again() -> Result<(), Failure> {
    scoped(async |endpoint: Endpoint, _: &Path| {
        let instance = assert_some!(assert_ok!(Instance::claim(&endpoint, brief()).await));
        let serving = AbortOnDropHandle::new(tokio::spawn(async move {
            let superseded = accept::<_, u64, String>(instance.listener.accept().await?, PROTOCOL, &(BUILD - 1), LIMITS).await;
            let Opened { body, reader: _, mut writer } = accept::<_, u64, String>(instance.listener.accept().await?, PROTOCOL, &BUILD, LIMITS).await.map_err(io::Error::other)?;
            reply(&mut writer, Ok::<_, Refusal>(format!("whole {body}"))).await.map_err(io::Error::other)?;
            Ok::<_, io::Error>(superseded.map(|opened| opened.body))
        }));
        let opened = assert_ok!(connect::<_, _, _, String>(&endpoint, &hello("again"), LIMITS, brief(), || Ok(())).await, "a service older than its caller refuses it, and its replacement answers");
        assert_eq!(opened.body, "whole again");
        assert_matches!(assert_ok!(assert_ok!(serving.await)), Err(IpcError::Refused(Refusal::Superseded)), "the first service refused the newer caller");
    })
    .await
}

#[tokio::test]
async fn refusal_for_another_protocol_is_not_asked_again() -> Result<(), Failure> {
    scoped(async |endpoint: Endpoint, _: &Path| {
        let instance = assert_some!(assert_ok!(Instance::claim(&endpoint, brief()).await));
        let _serving = AbortOnDropHandle::new(tokio::spawn(async move {
            let stream = instance.listener.accept().await?;
            accept::<_, u64, String>(stream, PROTOCOL + 1, &BUILD, LIMITS).await.map_err(io::Error::other).map(|opened| opened.body)
        }));
        let refused = assert_err!(connect::<_, _, _, String>(&endpoint, &hello("other"), LIMITS, brief(), || Ok(())).await);
        assert_matches!(refused, IpcError::Refused(Refusal::Protocol { offered: PROTOCOL, served }) if served == PROTOCOL + 1, "a caller of another protocol is told so once, no retry reaches a service that answers no second opening");
    })
    .await
}

#[tokio::test]
async fn service_lost_after_its_start_is_started_again() -> Result<(), Failure> {
    async fn dying(named: Endpoint) -> io::Result<()> {
        let Some(instance) = Instance::claim(&named, brief()).await.map_err(io::Error::other)? else { return Ok(()) };
        let Opened { body: _, reader: _, writer: cut } = accept::<_, u64, String>(instance.listener.accept().await?, PROTOCOL, &BUILD, LIMITS).await.map_err(io::Error::other)?;
        drop((cut, instance));
        Ok(())
    }
    async fn serving(named: Endpoint) -> io::Result<()> {
        let instance = Instance::claim(&named, brief()).await.map_err(io::Error::other)?.ok_or_else(|| io::Error::from(io::ErrorKind::AddrInUse))?;
        let Opened { body, reader: _, mut writer } = accept::<_, u64, String>(instance.listener.accept().await?, PROTOCOL, &BUILD, LIMITS).await.map_err(io::Error::other)?;
        reply(&mut writer, Ok::<_, Refusal>(body)).await.map_err(io::Error::other)
    }
    scoped(async |endpoint: Endpoint, _: &Path| {
        let (starts, services) = (AtomicUsize::new(0), TaskTracker::new());
        let start = || {
            services.spawn(match starts.fetch_add(1, Ordering::Relaxed) {
                0 => future::Either::Left(dying(endpoint.clone())),
                _ => future::Either::Right(serving(endpoint.clone())),
            });
            Ok(())
        };
        let opened = assert_ok!(connect::<_, _, _, String>(&endpoint, &hello("restarted"), LIMITS, brief().with_max_times(500), start).await);
        assert_eq!((opened.body.as_str(), starts.load(Ordering::Relaxed)), ("restarted", 2), "a service that dies mid-answer after its start is started once more");
    })
    .await
}

// --- [RACE]

#[tokio::test(flavor = "multi_thread")]
async fn racing_first_callers_share_one_service() -> Result<(), Failure> {
    async fn answered(connected: io::Result<Stream>) -> Result<(), IpcError> {
        let Opened { body, reader: _, mut writer } = accept::<_, u64, String>(connected.map_err(|source| IpcError::Io { operation: IpcOperation::Listen, source })?, PROTOCOL, &BUILD, LIMITS).await?;
        reply(&mut writer, Ok::<_, Refusal>(body)).await
    }
    async fn serving(named: Endpoint, taken: Arc<AtomicUsize>, unopened: mpsc::UnboundedSender<IpcError>) -> Result<(), IpcError> {
        let Some(instance) = Instance::claim(&named, brief()).await? else { return Ok(()) };
        taken.fetch_add(1, Ordering::Relaxed);
        loop {
            let connected = instance.listener.accept().await;
            answered(connected).await.or_else(|probe| unopened.send(probe).map_err(|mpsc::error::SendError(unread)| unread))?;
        }
    }
    scoped(async |endpoint: Endpoint, _: &Path| {
        let (claimed, services, (unopened, mut probes)) = (Arc::new(AtomicUsize::new(0)), TaskTracker::new(), mpsc::unbounded_channel());
        let start = || {
            services.spawn(serving(endpoint.clone(), Arc::clone(&claimed), unopened.clone()));
            Ok(())
        };
        let callers =
            (0..8).map(async |caller| connect::<_, _, _, String>(&endpoint, &hello(&format!("caller {caller}")), LIMITS, brief().with_max_times(500), &start).await.map(|opened| opened.body));
        let answers = assert_ok!(future::join_all(callers).await.into_iter().collect::<Validated<Vec<_>, _>>().ok());
        assert_eq!(answers, Vec::from_iter((0..8).map(|caller| format!("caller {caller}"))), "every caller is answered with its own message");
        probes.close();
        assert_eq!(claimed.load(Ordering::Relaxed), 1, "one service took the name, connections it could not open: {:?}", probes.recv().await);
    })
    .await
}
