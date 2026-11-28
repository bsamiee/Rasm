use core::time::Duration;
use std::path::Path;
use std::process::Termination;

use backon::ConstantBuilder;
use claims::assert_ok;
use rasm_ipc::{Endpoint, Hello, Limits};
use rasm_testing::{Failure, Folder};

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROTOCOL: u32 = 3;
const BUILD: u64 = 7;
const LIMITS: Limits = Limits { line: 64 << 10, deadline: Duration::from_secs(5) };

// --- [OPERATIONS] ----------------------------------------------------------------------

const fn brief() -> ConstantBuilder { ConstantBuilder::new().with_delay(Duration::from_millis(10)).with_max_times(3) }

fn hello(body: &str) -> Hello<u64, String> { Hello { protocol: PROTOCOL, build: BUILD, body: body.to_owned() } }

#[cfg(test)]
async fn scoped<T>(test: impl AsyncFnOnce(Endpoint, &Path) -> T) -> Result<(), Failure>
where T: Termination {
    Folder::scoped(Path::new(env!("CARGO_TARGET_TMPDIR")), async |folder: &Folder| {
        let instance = folder.join("instance");
        test(assert_ok!(Endpoint::new(&instance, "s", None)), &instance).await
    })
    .await
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod connect;
mod instance;
mod opening;
