use std::{ffi::OsString, io, path::PathBuf};

use clap::{Args, ValueHint};
use fs_err::tokio::canonicalize;
use futures::StreamExt as _;
use rasm_process::{Launch, Run};
use thiserror::Error;
use tokio::{io::AsyncWriteExt as _, net::UnixListener, process::Command};
use tokio_util::{
    codec::{FramedRead, LinesCodec},
    sync::CancellationToken,
};
use url::Url;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Args)]
pub(crate) struct Suite {
    #[arg(value_hint = ValueHint::FilePath)]
    config: PathBuf,
    #[arg(long, env, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, help = "SwiftPM scratch folder holding the `watch-apps` debug build")]
    swiftpm_build_dir: PathBuf,
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

pub(crate) async fn test(Suite { config, swiftpm_build_dir, selectors }: Suite, run: &Run) -> io::Result<()> {
    let path = run.scratch().join("socket");
    let listener = UnixListener::bind(&path)?;
    let watcher = canonicalize(swiftpm_build_dir.join("debug/watch-apps")).await?;
    let finished = CancellationToken::new();
    let active = run.directory().join("active-case");
    let mut command = Command::new("vitest");
    command
        .args(["run", "--config"])
        .arg(config)
        .args(selectors)
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
            let mut application = Command::new(&watcher);
            application.env("WATCH_APPS_LOG", run.directory().join("watch-apps.log"));
            run.application(application, launch, async move |pid| {
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
