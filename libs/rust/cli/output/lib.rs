#![feature(iterator_try_collect)]

use core::{error::Error, fmt, pin::pin};
use std::{io, panic, process};

use anstream::{AutoStream, ColorChoice};
use anstyle::{Ansi256Color, Color, Effects, Style};
use clap::{Args, CommandFactory, FromArgMatches, ValueEnum, builder::Styles};
use csv::ErrorKind;
use futures::{Stream, StreamExt as _, executor};
use inquire::{
    InquireError,
    error::InquireResult,
    ui::{self, Attributes, ErrorMessageRenderConfig, RenderConfig, StyleSheet},
};
use minus::MinusError;
use rasm_command::{Augmented, CommandError, Process};
use rasm_telemetry::Defaults;
use serde::Serialize;
use sysexits::ExitCode;
use tokio::task;
use tracing_indicatif::{
    IndicatifLayer,
    filter::{IndicatifFilter, hide_indicatif_span_fields},
    suspend_tracing_indicatif,
};
use tracing_subscriber::{Layer as _, fmt::format::DefaultFields};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, ValueEnum)]
enum Format {
    #[value(help = "Table sized to the terminal, paged when taller")]
    Grid,
    #[value(help = "Comma-separated records with a header")]
    Csv,
    #[value(help = "One JSON array")]
    Json,
    #[value(help = "One JSON object per line")]
    Jsonl,
}

#[derive(Clone, Copy, Default, ValueEnum)]
enum Paging {
    #[default]
    #[value(help = "Page when the grid is taller than the terminal")]
    Auto,
    #[value(help = "Page every grid")]
    Always,
    #[value(help = "Write the grid without paging")]
    Never,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Args)]
#[group(skip)]
#[command(next_help_heading = "Output")]
struct Options {
    #[arg(long, global = true, value_enum, help = "Row format, grid on a terminal and JSON lines elsewhere")]
    format: Option<Format>,
    #[arg(long, global = true, value_name = "WHEN", value_enum, default_value_t, help = "Page the grid through the in-process pager")]
    paging: Paging,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum OutputError {
    #[error("cannot write rows")]
    Write { source: io::Error },
    #[error("{}", fmt::from_fn(|formatter| match row {
        Some(row) => write!(formatter, "cannot encode row {row} as JSON"),
        None => formatter.write_str("cannot encode rows as JSON"),
    }))]
    Json { row: Option<usize>, source: serde_json::Error },
    #[error("cannot encode row {row} as CSV")]
    Records { row: usize, source: csv::Error },
    #[error(transparent)]
    Pager(#[from] MinusError),
    #[error(transparent)]
    Prompt(#[from] InquireError),
}

impl From<OutputError> for ExitCode {
    fn from(value: OutputError) -> Self {
        io_error_kind(&value).map_or_else(
            || match value {
                OutputError::Write { source: _ } | OutputError::Pager(_) | OutputError::Prompt(InquireError::IO(_)) => Self::IoErr,
                OutputError::Json { row: _, source: _ } | OutputError::Records { row: _, source: _ } | OutputError::Prompt(InquireError::Custom(_)) => Self::DataErr,
                OutputError::Prompt(InquireError::NotTTY) => Self::NoInput,
                OutputError::Prompt(InquireError::InvalidConfiguration(_)) => Self::Software,
                OutputError::Prompt(InquireError::OperationCanceled | InquireError::OperationInterrupted) => Self::TempFail,
            },
            Self::from,
        )
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn io_error_kind(error: &OutputError) -> Option<io::ErrorKind> {
    match error {
        OutputError::Write { source } | OutputError::Pager(MinusError::Draw(source)) | OutputError::Prompt(InquireError::IO(source)) => Some(source.kind()),
        OutputError::Json { row: _, source } => source.io_error_kind(),
        OutputError::Records { row: _, source } if let ErrorKind::Io(failure) = source.kind() => Some(failure.kind()),
        OutputError::Records { row: _, source: _ } | OutputError::Pager(_) | OutputError::Prompt(_) => None,
    }
}

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

pub fn run<A, R, E, S>(defaults: Defaults, work: impl FnOnce(A) -> S + Send + 'static) -> process::ExitCode
where
    A: CommandFactory + FromArgMatches + Send + 'static,
    R: Serialize,
    E: Error + From<CommandError> + From<OutputError> + Into<ExitCode> + Send + 'static,
    S: Stream<Item = Result<R, E>>,
{
    fn rendered(styles: &Styles) -> RenderConfig<'static> {
        fn sheet(style: &Style) -> StyleSheet {
            let colored = |color| painted(color, ui::Color::AnsiValue, ui::Color::rgb);
            StyleSheet {
                fg: style.get_fg_color().map(colored),
                bg: style.get_bg_color().map(colored),
                att: [(Effects::BOLD, Attributes::BOLD), (Effects::ITALIC, Attributes::ITALIC)]
                    .into_iter()
                    .filter_map(|(effect, attribute)| style.get_effects().contains(effect).then_some(attribute))
                    .collect(),
            }
        }
        let base = RenderConfig::default_colored();
        RenderConfig {
            prompt_prefix: base.prompt_prefix.with_style_sheet(sheet(styles.get_literal())),
            answered_prompt_prefix: base.answered_prompt_prefix.with_style_sheet(sheet(styles.get_valid())),
            prompt: sheet(styles.get_header()),
            default_value: sheet(styles.get_context()),
            placeholder: sheet(styles.get_placeholder()),
            help_message: sheet(styles.get_context()),
            answer: sheet(styles.get_valid()),
            canceled_prompt_indicator: base.canceled_prompt_indicator.with_style_sheet(sheet(styles.get_invalid())),
            error_message: ErrorMessageRenderConfig { message: sheet(styles.get_error()), ..base.error_message },
            highlighted_option_prefix: base.highlighted_option_prefix.with_style_sheet(sheet(styles.get_literal())),
            selected_option: Some(sheet(styles.get_literal())),
            ..base
        }
    }
    let progress = IndicatifLayer::new().with_span_field_formatter(hide_indicatif_span_fields(DefaultFields::new()));
    let stderr = progress.get_stderr_writer();
    rasm_command::run(
        defaults,
        stderr,
        progress.with_filter(IndicatifFilter::new(false)),
        async move |Process { args: Augmented { options: Options { format, paging }, args }, mut signals, filter: _ }| {
            let program = A::command();
            let styles = program.get_styles();
            inquire::set_global_render_config(match AutoStream::choice(&io::stderr()) {
                ColorChoice::Never => RenderConfig::empty(),
                ColorChoice::Auto | ColorChoice::AlwaysAnsi | ColorChoice::Always => rendered(styles),
            });
            let header = *styles.get_header();
            task::spawn_blocking(move || {
                let mut rows = pin!(work(args).take_until(signals.next()));
                formats::written(format, paging, header, executor::block_on_stream(rows.as_mut())).map(|()| if rows.is_stopped() { ExitCode::TempFail } else { ExitCode::Ok })
            })
            .await
            .unwrap_or_else(|failure| panic::resume_unwind(failure.into_panic()))
        },
    )
}

mod formats;
