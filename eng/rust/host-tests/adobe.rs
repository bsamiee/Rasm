use std::{ffi::OsString, io, path::PathBuf};

use clap::{Args, ValueHint};
use futures::StreamExt as _;
use rasm_process::{macos::Launch, report::Run};
use tokio::{io::AsyncWriteExt as _, net::UnixListener, process::Command};
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

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn leases(listener: UnixListener, run: &Run, finished: &CancellationToken) -> io::Result<()> {
    loop {
        let (socket, _) = tokio::select! {
            biased;
            () = finished.cancelled() => return Ok(()),
            accepted = listener.accept() => accepted?,
        };
        let (reader, mut writer) = socket.into_split();
        let mut requests = FramedRead::new(reader, LinesCodec::new_with_max_length(4096));
        let bundle = requests
            .next()
            .await
            .transpose()
            .map_err(io::Error::other)?
            .and_then(|uri| Url::parse(&uri).ok()?.to_file_path().ok())
            .ok_or_else(|| io::Error::new(io::ErrorKind::InvalidInput, "Expected a file URL for an Adobe application bundle"))?;
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
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) async fn test(suite: Suite, run: &Run) -> io::Result<()> {
    let (_socket, path, listener) = socket(run.scratch())?;
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
        let result = run.command(command, "vitest", Some(&active)).await;
        finished.cancel();
        result.map(drop)
    };
    let (tests, desktop) = tokio::join!(tested, leases(listener, run, &finished));
    match (tests, desktop) {
        (Err(tests), Err(desktop)) => Err(io::Error::other(format!("{tests}; {desktop}"))),
        (tests, desktop) => tests.and(desktop),
    }
}
