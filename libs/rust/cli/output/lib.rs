#![feature(control_flow_into_value, try_blocks_heterogeneous)]

use core::pin::pin;
use std::{io, panic, process};

use anstream::{AutoStream, ColorChoice};
use anstyle::{Ansi256Color, Color, Effects, Style};
use clap::{CommandFactory, FromArgMatches};
use futures::{
    Stream, StreamExt as _, executor,
    future::{Either, select},
};
use inquire::{
    InquireError,
    error::InquireResult,
    ui::{self, Attributes, ErrorMessageRenderConfig, RenderConfig, StyleSheet},
};
use minus::MinusError;
use nonempty_collections::NEVec;
use rasm_command::{Augmented, CommandError, Process};
use rasm_telemetry::{Defaults, Failure};
use serde::Serialize;
use sysexits::ExitCode;
use tokio::task;
use tracing_indicatif::{
    IndicatifLayer,
    filter::{IndicatifFilter, hide_indicatif_span_fields},
    suspend_tracing_indicatif,
};
use tracing_subscriber::{Layer as _, fmt::format::DefaultFields};

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum OutputError {
    #[error("cannot write rows")]
    Write { source: io::Error },
    #[error("cannot encode row {row}")]
    Encode { row: usize, source: serde_json::Error },
    #[error("cannot write row {row} as CSV")]
    Csv { row: usize, source: csv::Error },
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
            OutputError::Encode { row: _, source: _ } | OutputError::Csv { row: _, source: _ } | OutputError::Column { row: _, name: _ } | OutputError::Prompt(InquireError::Custom(_)) =>
                Self::DataErr,
            OutputError::Pager(_) => Self::IoErr,
            OutputError::Prompt(InquireError::NotTTY) => Self::NoInput,
            OutputError::Prompt(InquireError::InvalidConfiguration(_)) => Self::Software,
            OutputError::Prompt(InquireError::OperationCanceled | InquireError::OperationInterrupted) => Self::TempFail,
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn painted<C>(color: Color, indexed: impl FnOnce(u8) -> C, rgb: impl FnOnce(u8, u8, u8) -> C) -> C {
    match color {
        Color::Ansi(color) => indexed(Ansi256Color::from(color).index()),
        Color::Ansi256(color) => indexed(color.index()),
        Color::Rgb(color) => rgb(color.r(), color.g(), color.b()),
    }
}

pub async fn prompt<T>(ask: impl FnOnce() -> InquireResult<T> + Send + 'static) -> Result<T, OutputError>
where T: Send + 'static {
    task::spawn_blocking(move || suspend_tracing_indicatif(ask)).await.unwrap_or_else(|failure| panic::resume_unwind(failure.into_panic())).map_err(OutputError::Prompt)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub fn run<A, R, E, S>(defaults: Defaults, work: impl AsyncFnOnce(A) -> Result<S, NEVec<E>> + Send + 'static) -> process::ExitCode
where
    A: CommandFactory + FromArgMatches + Send + 'static,
    R: Serialize,
    E: Failure + From<CommandError> + From<OutputError> + Send + 'static,
    S: Stream<Item = Result<R, E>>,
{
    let sheet = |style: &Style| {
        let colored = |color| painted(color, ui::Color::AnsiValue, ui::Color::rgb);
        StyleSheet {
            fg: style.get_fg_color().map(colored),
            bg: style.get_bg_color().map(colored),
            att: [(Effects::BOLD, Attributes::BOLD), (Effects::ITALIC, Attributes::ITALIC)]
                .into_iter()
                .filter_map(|(effect, attribute)| style.get_effects().contains(effect).then_some(attribute))
                .collect(),
        }
    };
    let progress = IndicatifLayer::new().with_span_field_formatter(hide_indicatif_span_fields(DefaultFields::new()));
    let (stderr, stdout) = (progress.get_stderr_writer(), progress.get_stdout_writer());
    rasm_command::run(defaults, stderr, progress.with_filter(IndicatifFilter::new(false)), async move |Process { args: Augmented { options, args }, styles, mut signals, filter: _ }| {
        let base = RenderConfig::default_colored();
        inquire::set_global_render_config(match AutoStream::choice(&io::stderr()) {
            ColorChoice::Never => RenderConfig::empty(),
            ColorChoice::Auto | ColorChoice::AlwaysAnsi | ColorChoice::Always => {
                let [literal, valid, context, error] = [styles.get_literal(), styles.get_valid(), styles.get_context(), styles.get_error()].map(sheet);
                RenderConfig {
                    prompt_prefix: base.prompt_prefix.with_style_sheet(literal),
                    answered_prompt_prefix: base.answered_prompt_prefix.with_style_sheet(valid),
                    prompt: sheet(styles.get_header()),
                    default_value: context,
                    placeholder: sheet(styles.get_placeholder()),
                    help_message: context,
                    answer: valid,
                    canceled_prompt_indicator: base.canceled_prompt_indicator.with_style_sheet(sheet(styles.get_invalid())),
                    error_message: ErrorMessageRenderConfig { prefix: base.error_message.prefix.with_style_sheet(error), message: error, ..base.error_message },
                    highlighted_option_prefix: base.highlighted_option_prefix.with_style_sheet(literal),
                    selected_checkbox: base.selected_checkbox.with_style_sheet(valid),
                    selected_option: Some(literal),
                    editor_prompt: context,
                    ..base
                }
            }
        });
        let header = *styles.get_header();
        task::spawn_blocking(move || {
            let Either::Left((started, next)) = executor::block_on(select(pin!(work(args)), signals.next())) else {
                return Ok(ExitCode::TempFail);
            };
            formats::written(options, header, stdout, Box::pin(started?).take_until(next))
        })
        .await
        .unwrap_or_else(|failure| panic::resume_unwind(failure.into_panic()))
    })
}

mod cells;
mod formats;
