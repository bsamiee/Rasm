use std::path::Path;

use claims::{assert_none, assert_ok, assert_some};
use futures::future;
use interprocess::local_socket::tokio::Stream;
use interprocess::local_socket::traits::tokio::{Listener as _, Stream as _};
use rasm_ipc::{Endpoint, Instance, Opened, Refusal, accept, greet, reply};
use rasm_testing::Failure;

use crate::{BUILD, LIMITS, PROTOCOL, brief, hello, scoped};

// --- [NAME]

#[tokio::test]
async fn live_name_is_taken_until_its_listener_drops() -> Result<(), Failure> {
    scoped(async |endpoint: Endpoint, _: &Path| {
        let first = assert_some!(assert_ok!(Instance::claim(&endpoint, brief()).await), "a free name is claimed");
        assert_none!(assert_ok!(Instance::claim(&endpoint, brief()).await), "a name with a live listener is taken");
        drop(first);
        assert_some!(assert_ok!(Instance::claim(&endpoint, brief()).await), "a dropped listener frees its name");
    })
    .await
}

#[tokio::test]
async fn name_left_by_a_dead_owner_is_reclaimed() -> Result<(), Failure> {
    scoped(async |endpoint: Endpoint, _: &Path| {
        let mut dead = assert_some!(assert_ok!(Instance::claim(&endpoint, brief()).await));
        dead.listener.do_not_reclaim_name_on_drop();
        drop(dead);
        let live = assert_some!(assert_ok!(Instance::claim(&endpoint, brief()).await), "a socket file with no listener behind it is replaced");
        let served = async {
            let Opened { body, reader: _, mut writer } = assert_ok!(accept::<_, u64, String>(assert_ok!(live.listener.accept().await), PROTOCOL, &BUILD, LIMITS).await);
            assert_ok!(reply(&mut writer, Ok::<_, Refusal>(body)).await);
        };
        let greeted = async { greet::<_, _, _, String>(assert_ok!(Stream::connect(endpoint.name.borrow()).await), &hello("after"), LIMITS).await };
        let ((), opened) = future::join(served, greeted).await;
        assert_eq!(assert_ok!(opened).body, "after", "the new listener answers on the reclaimed name");
    })
    .await
}

// --- [ACCESS]

#[cfg(unix)]
#[tokio::test]
async fn instance_folder_is_created_for_its_owner_alone() -> Result<(), Failure> {
    use std::os::unix::fs::PermissionsExt as _;
    scoped(async |endpoint: Endpoint, folder: &Path| {
        assert_none!(fs_err::tokio::metadata(folder).await.ok(), "the folder does not exist before the claim");
        let _held = assert_some!(assert_ok!(Instance::claim(&endpoint, brief()).await));
        let mode = assert_ok!(fs_err::tokio::metadata(folder).await).permissions().mode() & 0o777;
        assert_eq!(mode, 0o700, "the folder holding the socket is how a local socket authenticates, so only its owner enters it: {mode:o}");
    })
    .await
}
