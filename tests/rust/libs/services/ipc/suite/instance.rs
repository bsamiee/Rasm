use claims::{assert_ok, assert_some};
use futures::future;
use interprocess::local_socket::tokio::Stream;
use interprocess::local_socket::traits::tokio::{Listener as _, Stream as _};
use rasm_ipc::{Instance, Opened, Refusal, accept, greet, reply};

use crate::{BUILD, LIMITS, PROTOCOL, brief, hello, scoped};

// --- [NAME]

#[tokio::test]
async fn live_name_is_taken_until_its_listener_drops() {
    let passed = scoped(async |endpoint, _| {
        let first = assert_some!(assert_ok!(Instance::claim(&endpoint, brief()).await), "a free name is claimed");
        let second = Instance::claim(&endpoint, brief()).await;
        assert!(!matches!(second, Ok(Some(_))), "a name with a live listener is taken: {second:?}");
        drop(first);
        assert_some!(assert_ok!(Instance::claim(&endpoint, brief()).await), "a dropped listener frees its name");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

#[tokio::test]
async fn name_left_by_a_dead_owner_is_overwritten() {
    let passed = scoped(async |endpoint, _| {
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
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

// --- [ACCESS]

#[cfg(unix)]
#[tokio::test]
async fn socket_file_is_writable_by_its_owner_alone() {
    use std::os::unix::fs::{FileTypeExt as _, PermissionsExt as _};
    let passed = scoped(async |endpoint, folder| {
        let _held = assert_some!(assert_ok!(Instance::claim(&endpoint, brief()).await));
        let mut entries = assert_ok!(fs_err::tokio::read_dir(folder).await);
        let mut modes = Vec::new();
        while let Some(entry) = assert_ok!(entries.next_entry().await) {
            let metadata = assert_ok!(entry.metadata().await);
            if metadata.file_type().is_socket() {
                modes.push(metadata.permissions().mode() & 0o777);
            }
        }
        assert_eq!(modes.is_empty(), !endpoint.name.is_path(), "a path name leaves one socket file in its folder: {modes:?}");
        assert!(modes.iter().all(|mode| mode & 0o022 == 0), "write access is how a local socket authenticates, so group and others hold none: {modes:?}");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}

#[cfg(unix)]
#[tokio::test]
async fn peer_credentials_are_the_callers() {
    use interprocess::local_socket::traits::StreamCommon as _;
    let passed = scoped(async |endpoint, _| {
        let instance = assert_some!(assert_ok!(Instance::claim(&endpoint, brief()).await));
        let (server, client) = future::join(instance.listener.accept(), Stream::connect(endpoint.name.borrow())).await;
        let (accepted, connected) = (assert_ok!(assert_ok!(server).peer_creds()), assert_ok!(assert_ok!(client).peer_creds()));
        let (user, group) = (rustix::process::geteuid().as_raw(), rustix::process::getegid().as_raw());
        assert_eq!((accepted.euid(), accepted.egid()), (Some(user), Some(group)), "the accepted stream names the connecting user");
        assert_eq!((connected.euid(), connected.egid()), (Some(user), Some(group)), "the connected stream names the listening user");
    });
    assert_ok!(passed.await, "a failed test keeps its folder");
}
