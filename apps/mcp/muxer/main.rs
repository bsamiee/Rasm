#![feature(default_field_values, error_reporter, iterator_try_collect, lock_value_accessors, nonpoison_mutex, process_setsid, sync_nonpoison)]

use core::time::Duration;
use std::{io, process::ExitCode};

use camino::Utf8PathBuf;
use clap::Parser;
use jiff::fmt::friendly::SpanParser;
use nonempty_collections::NEVec;
use rasm_command::{CommandError, Process, telemetry::Defaults};
use rasm_mcp::McpError;
use serde::de::DeserializeOwned;
use serde_path_to_error::Track;
use tokio::runtime::Builder;
use tracing_subscriber::{filter::LevelFilter, layer::Identity};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Parser)]
#[command(version, about = "Shares one stdio MCP server process per server key across every harness session through a per-user daemon")]
enum Cli {
    #[command(about = "Serve a stdio MCP server to this session through the shared daemon, running it directly when the daemon cannot")]
    Run(run::Run),
    #[command(hide = true, about = "Hold the shared servers on the per-user socket until it stays unused for the idle window")]
    Daemon {
        #[arg(long, env = "MCP_MUXER_IDLE", value_name = "DURATION", default_value = "5m", value_parser = |text: &str| SpanParser::new().parse_unsigned_duration(text))]
        #[arg(help = "Time a server, and the daemon itself, stays up with no session")]
        idle: Duration,
    },
}

#[derive(Debug, derive_more::Display)]
enum Operation {
    #[display("find the per-user folders")]
    Locate,
    #[display("read the working directory")]
    Directory,
    #[display("find {_0} on PATH")]
    Resolve(String),
    #[display("read variable {_0}")]
    Variable(String),
    #[display("reach the daemon at {_0}")]
    Connect(Utf8PathBuf),
    #[display("start the daemon")]
    Start,
    #[display("listen on {_0}")]
    Listen(Utf8PathBuf),
    #[display("exchange the opening line")]
    Opening,
    #[display("relay MCP messages")]
    Relay,
    #[display("run {_0} directly")]
    Exec(String),
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum MuxerError {
    #[error("cannot {operation}")]
    Io { operation: Operation, source: io::Error },
    #[error("cannot encode the opening line")]
    Encode(#[from] serde_json::Error),
    #[error("cannot decode the opening line")]
    Decode(#[from] serde_path_to_error::Error<serde_json::Error>),
    #[error(transparent)]
    Mcp(#[from] McpError),
    #[error(transparent)]
    Command(#[from] CommandError),
}

impl From<MuxerError> for sysexits::ExitCode {
    fn from(value: MuxerError) -> Self {
        match value {
            MuxerError::Io { operation: _, source } => source.into(),
            MuxerError::Encode(_) => Self::Software,
            MuxerError::Decode(_) => Self::Protocol,
            MuxerError::Mcp(_) => Self::Unavailable,
            MuxerError::Command(source) => source.into(),
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn decoded<T>(line: &str) -> Result<T, serde_path_to_error::Error<serde_json::Error>>
where T: DeserializeOwned {
    let mut decoder = serde_json::Deserializer::from_str(line);
    let value = serde_path_to_error::deserialize(&mut decoder)?;
    decoder.end().map(|()| value).map_err(|source| serde_path_to_error::Error::new(Track::new().path(), source))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod daemon;
mod run;

fn main() -> ExitCode {
    rasm_command::run(
        Defaults { targets: vec![("rmcp", LevelFilter::WARN), ("rmcp::service", LevelFilter::ERROR)], .. },
        io::stderr,
        Identity::new(),
        |cli| match cli {
            Cli::Run(_) => Builder::new_current_thread(),
            Cli::Daemon { idle: _ } => Builder::new_multi_thread(),
        },
        async |Process { args, styles: _, signals, filter: _, level: _ }| {
            match args {
                Cli::Run(launch) => run::relayed(launch, signals).await,
                Cli::Daemon { idle } => daemon::served(idle, signals).await,
            }
            .map_err(NEVec::new)
        },
    )
}
