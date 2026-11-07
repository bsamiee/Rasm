use std::{ffi::OsString, io, path::PathBuf};

use clap::{Args, ValueHint};
use futures::StreamExt as _;
use rasm_process::{Launch, Run};
use thiserror::Error;
use tokio::{io::AsyncWriteExt as _, process::Command};
use tokio_util::{
    codec::{FramedRead, LinesCodec},
    sync::CancellationToken,
};
use url::Url;

use crate::desktop::{socket, with_app};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Args)]
pub(crate) struct Suite {
    #[arg(value_hint = ValueHint::FilePath)]
    config: PathBuf,
    #[arg(trailing_var_arg = true, allow_hyphen_values = true)]
    selectors: Vec<OsString>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Error)]
#[error("tests failed beside a failed application lease: {leases}")]
struct Joined {
    #[source]
    tests: io::Error,
    leases: io::Error,
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) async fn test(suite: Suite, run: &Run) -> io::Result<()> {
    let (path, listener) = socket(run)?;
    let finished = CancellationToken::new();
    let active = run.directory().join("active-case");
    let mut command = Command::new("vitest");
    command
        .args(["run", "--config"])
        .arg(suite.config)
        .args(suite.selectors)
        .env("RASM_HOST_RUN_DIRECTORY", run.directory())
        .env("RASM_HOST_RUN_SCRATCH", run.scratch())
        .env("RASM_HOST_ACTIVE_CASE", &active)
        .env("RASM_ADOBE_SOCKET", path);
    let tested = async {
        let result = run.command(command, Some(&active)).await;
        finished.cancel();
        result.map(drop)
    };
    let leases = async {
        while let Some(accepted) = finished.run_until_cancelled(listener.accept()).await {
            let (reader, mut writer) = accepted?.0.into_split();
            let mut requests = FramedRead::new(reader, LinesCodec::new_with_max_length(4096));
            let uri = requests.next().await.transpose().map_err(io::Error::other)?.ok_or_else(|| io::Error::new(io::ErrorKind::UnexpectedEof, "Adobe test lease closed before naming a bundle"))?;
            let bundle = Url::parse(&uri)
                .map_err(|source| io::Error::new(io::ErrorKind::InvalidInput, source))?
                .to_file_path()
                .map_err(|()| io::Error::new(io::ErrorKind::InvalidInput, "Expected a file URL for an Adobe application bundle"))?;
            let launch = Launch { bundle: &bundle, documents: &[], arguments: &[], environment: &[] };
            with_app(run, launch, async move |pid| {
                writer.write_all(format!("{pid}\n").as_bytes()).await?;
                match requests.next().await {
                    None => Ok(()),
                    Some(_) => Err(io::Error::new(io::ErrorKind::InvalidData, "Adobe test lease accepts one bundle URL")),
                }
            })
            .await?;
        }
        Ok(())
    };
    match tokio::join!(tested, leases) {
        (Err(tests), Err(leases)) => Err(io::Error::other(Joined { tests, leases })),
        (tests, leases) => tests.and(leases),
    }
}
