#![feature(control_flow_into_value, deref_patterns, try_blocks_heterogeneous)]

use core::pin::pin;
use std::{io, panic, process};

use clap::{CommandFactory, FromArgMatches};
use crossterm::terminal;
use futures::future::{Either, select};
use futures::{Stream, StreamExt as _, executor};
use inquire::InquireError;
use inquire::error::InquireResult;
use minus::MinusError;
use nonempty_collections::NEVec;
use rasm_cli::{Augmented, CommandError, Failure, Invocation};
use rasm_telemetry::process::{Layer, Program};
use serde::Serialize;
use sysexits::ExitCode;
use tokio::runtime::Builder;
use tokio::task;
use tracing_indicatif::filter::{IndicatifFilter, hide_indicatif_span_fields};
use tracing_indicatif::{IndicatifLayer, suspend_tracing_indicatif};
use tracing_subscriber::fmt::format::DefaultFields;
use validated::Validated;

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum EncodingError {
    #[error(transparent)]
    Json(#[from] serde_json::Error),
    #[error(transparent)]
    Value(#[from] serde_content::Error),
}

#[derive(Debug, thiserror::Error)]
pub enum OutputError {
    #[error("cannot write rows")]
    Write { source: io::Error },
    #[error("cannot encode row {row}")]
    Encode { row: usize, source: EncodingError },
    #[error("cannot write row {row}, column `{name}` is outside the CSV header")]
    Column { row: usize, name: String },
    #[error(transparent)]
    Pager(#[from] MinusError),
    #[error(transparent)]
    Prompt(#[from] InquireError),
}

impl From<OutputError> for ExitCode {
    fn from(value: OutputError) -> Self {
        match value {
            OutputError::Write { source } | OutputError::Pager(MinusError::Draw(source)) | OutputError::Prompt(InquireError::IO(source)) => source.into(),
            OutputError::Encode { row: _, source: _ } | OutputError::Column { row: _, name: _ } | OutputError::Prompt(InquireError::Custom(_)) => Self::DataErr,
            OutputError::Pager(_) => Self::IoErr,
            OutputError::Prompt(InquireError::NotTTY) => Self::NoInput,
            OutputError::Prompt(InquireError::InvalidConfiguration(_)) => Self::Software,
            OutputError::Prompt(InquireError::OperationCanceled | InquireError::OperationInterrupted) => Self::TempFail,
        }
    }
}

impl<E> From<OutputError> for NEVec<E>
where E: From<OutputError>
{
    fn from(value: OutputError) -> Self { Self::new(E::from(value)) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub async fn prompt<T>(ask: impl FnOnce() -> InquireResult<T> + Send + 'static) -> Result<T, OutputError>
where T: Send + 'static {
    task::spawn_blocking(move || suspend_tracing_indicatif(ask)).await.unwrap_or_else(|failure| panic::resume_unwind(failure.into_panic())).map_err(OutputError::Prompt)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub fn run<A, L, R, E, S>(program: Program, layer: L, runtime: impl FnOnce(&A) -> Builder, work: impl AsyncFnOnce(Invocation<A>) -> Result<S, NEVec<E>> + Send + 'static) -> process::ExitCode
where
    A: CommandFactory + FromArgMatches + Send + 'static,
    L: Layer,
    R: Serialize,
    E: Failure + From<CommandError> + From<OutputError> + Send + 'static,
    S: Stream<Item = Result<R, E>>,
{
    let progress = IndicatifLayer::new().with_span_field_formatter(hide_indicatif_span_fields(DefaultFields::new()));
    let (stderr, stdout) = (progress.get_stderr_writer(), progress.get_stdout_writer());
    rasm_cli::run(
        program,
        stderr,
        progress.with_filter(IndicatifFilter::new(false)).and_then(layer),
        |Augmented { options: _, args }| runtime(args),
        async move |Invocation { args: Augmented { options, args }, executable, styles, folders, filter, parent, settings, export }| {
            inquire::set_global_render_config(theme::prompts(&styles));
            let (header, mut signals) = (*styles.get_header(), rasm_cli::signals().map_err(|error| NEVec::new(E::from(error)))?);
            task::spawn_blocking(move || {
                let outcome = match executor::block_on(select(pin!(work(Invocation { args, executable, styles, folders, filter, parent, settings, export })), signals.next())) {
                    Either::Left((started, next)) => started.and_then(|items| formats::written(options, header, stdout, Box::pin(items).take_until(next))),
                    Either::Right(_) => Ok(ExitCode::TempFail),
                };
                let cooked = cfg_select! {
                    windows => terminal::is_raw_mode_enabled().and_then(|raw| if raw { terminal::disable_raw_mode() } else { Ok(()) }),
                    _ => terminal::disable_raw_mode(),
                };
                outcome.map_or_else(Validated::Fail, Validated::Good).map2(Validated::from(cooked.map_err(|source| E::from(OutputError::Prompt(InquireError::IO(source))))), |status, ()| status).ok()
            })
            .await
            .unwrap_or_else(|failure| panic::resume_unwind(failure.into_panic()))
        },
    )
}

mod cells;
mod formats;
mod theme;
