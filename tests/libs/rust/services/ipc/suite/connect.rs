use core::error::Error;
use core::sync::atomic::{AtomicUsize, Ordering};
use std::io;
use std::sync::Arc;

use claims::{assert_err, assert_ok};
use futures::future;
use interprocess::local_socket::tokio::Stream;
use interprocess::local_socket::traits::tokio::Listener as _;
use rasm_ipc::{Endpoint, Instance, IpcError, IpcOperation, Opened, Refusal, accept, attach, connect, reply};
use tokio::sync::mpsc;
use tokio_util::task::TaskTracker;
use validated::Validated;

use crate::{BUILD, LIMITS, PROTOCOL, brief, hello, scoped};

// --- [NO_SERVER]

#[tokio::test]
async fn no_server() {
    let passed = scoped(async |endpoint, _| {
        let attached = attach::<_, _, String>(&endpoint, &hello("nobody"), LIMITS).await;
        assert!(!matches!(attached, Ok(Some(_))), "client successfully connected to nonexistent server: {attached:?}");
        let refused = assert_err!(connect::<_, _, _, String>(&endpoint, &hello("nobody"), LIMITS, brief(), || Ok(())).await);
        let kinds = (&refused as &dyn Error).sources().filter_map(|cause| cause.downcast_ref::<io::Error>().map(io::Error::kind)).collect::<Vec<_>>();
        assert!(matches!(kinds[..], [io::ErrorKind::NotFound | io::ErrorKind::ConnectionRefused]), "expected error to be 'not found' or 'connection refused', received {refused:?}");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

// --- [RACE]

#[tokio::test(flavor = "multi_thread")]
async fn racing_first_callers_share_one_service() {
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
    let passed = scoped(async |endpoint, _| {
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
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}
