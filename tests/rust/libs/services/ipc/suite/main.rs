#![feature(error_iter)]

use core::time::Duration;
use std::path::Path;
use std::process::Termination;

use backon::ConstantBuilder;
use claims::assert_ok;
use rasm_ipc::{Endpoint, Hello, Limits};
use rasm_testing::{Failure, Fixture as _, Folder};

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROTOCOL: u32 = 1;
const BUILD: u64 = 7;
const LIMITS: Limits = Limits { line: 64 << 10, deadline: Duration::from_secs(5) };

// --- [OPERATIONS] ----------------------------------------------------------------------

fn brief() -> ConstantBuilder { ConstantBuilder::new().with_delay(Duration::from_millis(10)).with_max_times(3) }

fn hello(body: &str) -> Hello<u64, String> { Hello { protocol: PROTOCOL, build: BUILD, body: body.to_owned() } }

#[cfg(test)]
async fn scoped<T>(test: impl AsyncFnOnce(Endpoint, &Path) -> T) -> Result<(), Failure<Folder>>
where T: Termination {
    Folder::scoped(Path::new(env!("CARGO_TARGET_TMPDIR")), async |folder: &Folder| test(assert_ok!(Endpoint::new(folder.path(), "s", None)), folder.path()).await).await
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod connect;
mod hello;
mod instance;
