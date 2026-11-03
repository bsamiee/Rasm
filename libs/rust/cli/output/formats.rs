use core::{
    fmt::{self, Write as _},
    iter, mem,
    ops::ControlFlow,
};
use std::io::{self, IsTerminal as _, Write as _};

use anstream::{AutoStream, ColorChoice};
use anstyle::Style;
use clap::{Args, ValueEnum};
use comfy_table::{Cell, CellAlignment, ContentArrangement, Table, presets};
use csv::IntoInnerError;
use futures::{Stream, StreamExt as _, executor, stream::TakeUntil};
use minus::{Pager, hooks::Hook};
use nonempty_collections::NEVec;
use serde::Serialize;
use serde_json::{
    Serializer,
    ser::{Formatter as _, PrettyFormatter},
};
use sysexits::ExitCode;
use tracing_indicatif::{IndicatifWriter, suspend_tracing_indicatif, writer};
use tracing_subscriber::fmt::writer::EitherWriter;

use crate::{
    OutputError,
    cells::{self, Columns, Field, Row},
    theme,
};

// --- [TYPES] ---------------------------------------------------------------------------

type Out = EitherWriter<IndicatifWriter<writer::Stdout>, io::Stdout>;

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
pub(crate) struct Options {
    #[arg(long, global = true, value_enum, help = "Row format, grid on a terminal and JSON lines elsewhere")]
    format: Option<Format>,
    #[arg(long, global = true, value_name = "WHEN", value_enum, default_value_t, help = "Page the grid on a terminal through the in-process pager")]
    paging: Paging,
}

enum Sink {
    Jsonl { out: Out, scratch: Vec<u8> },
    Json { out: Out, scratch: Vec<u8>, formatter: PrettyFormatter<'static>, first: bool },
    Csv { out: Out, writer: Box<csv::Writer<Vec<u8>>>, columns: Columns, width: Option<usize> },
    Grid { rows: Vec<Row>, columns: Columns },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn failed(source: impl Into<io::Error>) -> OutputError { OutputError::Write { source: source.into() } }

fn delivered(out: &mut Out, scratch: &[u8]) -> io::Result<usize> {
    let lines = scratch.iter().rposition(|byte| *byte == b'\n').and_then(|end| scratch.get(..=end)).unwrap_or_default();
    out.write_all(lines)?;
    out.flush()?;
    Ok(lines.len())
}

fn appended(sink: &mut Sink, row: usize, value: &impl Serialize) -> Result<(), OutputError> {
    let flattened = |columns: &mut Columns| cells::flattened(value, columns).map_err(|source| OutputError::Encode { row, source });
    match sink {
        Sink::Jsonl { out: _, scratch } => {
            let mark = scratch.len();
            serde_json::to_writer(&mut *scratch, value).map(|()| scratch.push(b'\n')).map_err(|source| {
                scratch.truncate(mark);
                OutputError::Encode { row, source }
            })
        }
        Sink::Json { out: _, scratch, formatter, first } => {
            let (mark, mut next) = (scratch.len(), formatter.clone());
            let encoded = try bikeshed Result<(), serde_json::Error> {
                if *first {
                    next.begin_array(&mut *scratch).map_err(serde_json::Error::io)?;
                }
                next.begin_array_value(&mut *scratch, *first).map_err(serde_json::Error::io)?;
                value.serialize(&mut Serializer::with_formatter(&mut *scratch, next.clone()))?;
                next.end_array_value(&mut *scratch).map_err(serde_json::Error::io)?
            };
            if let Err(source) = encoded {
                scratch.truncate(mark);
                return Err(OutputError::Encode { row, source });
            }
            (*formatter, *first) = (next, false);
            Ok(())
        }
        Sink::Csv { out: _, writer, columns, width } => {
            let Row { named, fields } = flattened(columns)?;
            let first = width.is_none();
            let width = *width.get_or_insert(columns.names.len());
            if first && named {
                writer.write_record(columns.names.iter().map(Box::as_ref)).map_err(failed)?;
            }
            if let Some(name) = fields.iter().enumerate().skip(width).find_map(|(index, field)| field.as_ref().and_then(|_| columns.names.get_index(index))) {
                return Err(OutputError::Column { row, name: name.to_string() });
            }
            writer.write_record((0..width).map(|index| fields.get(index).and_then(Option::as_ref).map_or("", |field| field.text.as_str()))).map_err(failed)
        }
        Sink::Grid { rows, columns } => {
            rows.push(flattened(columns)?);
            Ok(())
        }
    }
}

fn flushed(sink: &mut Sink) -> Result<(), OutputError> {
    match sink {
        Sink::Jsonl { out, scratch } | Sink::Json { out, scratch, formatter: _, first: _ } => delivered(out, scratch).map(|lines| {
            scratch.drain(..lines);
        }),
        Sink::Csv { out, writer, columns: _, width: _ } =>
            mem::replace(&mut **writer, csv::Writer::from_writer(Vec::new())).into_inner().map_err(IntoInnerError::into_error).and_then(|scratch| delivered(out, &scratch)).map(|_lines| ()),
        Sink::Grid { rows: _, columns: _ } => Ok(()),
    }
    .map_err(failed)
}

fn finished(sink: Sink, header: Style, paging: Paging, unpaged: bool) -> Result<(), OutputError> {
    match sink {
        Sink::Jsonl { out: _, scratch: _ } | Sink::Csv { out: _, writer: _, columns: _, width: _ } => Ok(()),
        Sink::Json { mut out, mut scratch, mut formatter, first } => (try bikeshed io::Result<()> {
            if first {
                formatter.begin_array(&mut scratch)?;
            }
            formatter.end_array(&mut scratch)?;
            scratch.push(b'\n');
            delivered(&mut out, &scratch)?;
        })
        .map_err(failed),
        Sink::Grid { rows, columns: _ } if rows.is_empty() => Ok(()),
        Sink::Grid { rows, columns } => {
            let escaped = |text: &str, formatter: &mut fmt::Formatter<'_>| {
                text.chars()
                    .try_for_each(|character| if character.is_control() && character != '\n' { fmt::Display::fmt(&character.escape_default(), formatter) } else { formatter.write_char(character) })
            };
            let cell = |Field { text, numeric }: &Field| Cell::new(fmt::from_fn(|formatter| escaped(text, formatter))).set_alignment(if *numeric { CellAlignment::Right } else { CellAlignment::Left });
            let mut table = Table::new();
            table.load_style(presets::UTF8_FULL_CONDENSED).set_content_arrangement(ContentArrangement::Dynamic);
            let style = match AutoStream::choice(&io::stdout()) {
                ColorChoice::Never => Style::new(),
                ColorChoice::Auto | ColorChoice::AlwaysAnsi | ColorChoice::Always => {
                    table.enforce_styling();
                    header
                }
            };
            if rows.iter().any(|row| row.named) {
                table.set_header(columns.names.iter().map(|name| theme::styled(Cell::new(fmt::from_fn(|formatter| escaped(name, formatter))), &style)));
            }
            table.add_rows(rows.iter().map(|row| (0..columns.names.len()).map(|index| row.fields.get(index).and_then(Option::as_ref).map_or_else(|| Cell::new(""), cell))));
            suspend_tracing_indicatif(|| match (paging, unpaged) {
                (Paging::Never, _) | (Paging::Auto | Paging::Always, true) => writeln!(anstream::stdout(), "{table}").map_err(failed),
                (requested @ (Paging::Auto | Paging::Always), false) => {
                    let pager = Pager::new();
                    try bikeshed Result<(), OutputError> {
                        pager.remove_hook(Hook::PostPagerExit, 1)?;
                        pager.set_run_no_overflow(matches!(requested, Paging::Always))?;
                        pager.set_text(table.to_string())?;
                        minus::page_all(pager)?
                    }
                }
            })
        }
    }
}

pub(crate) fn written<R, E, S, F>(Options { format, paging }: Options, header: Style, progress: IndicatifWriter<writer::Stdout>, items: TakeUntil<S, F>) -> Result<ExitCode, NEVec<E>>
where
    R: Serialize,
    E: From<OutputError>,
    S: Stream<Item = Result<R, E>> + Unpin,
    F: Future + Unpin,
{
    const BATCH: usize = 256;
    let terminal = io::stdout().is_terminal();
    let out = || if terminal { EitherWriter::A(progress) } else { EitherWriter::B(io::stdout()) };
    let opened = match (format, terminal) {
        (Some(Format::Grid), _) | (None, true) => Sink::Grid { rows: Vec::new(), columns: Columns::default() },
        (Some(Format::Csv), _) => Sink::Csv { out: out(), writer: Box::new(csv::Writer::from_writer(Vec::new())), columns: Columns::default(), width: None },
        (Some(Format::Json), _) => Sink::Json { out: out(), scratch: Vec::new(), formatter: PrettyFormatter::new(), first: true },
        (Some(Format::Jsonl), _) | (None, false) => Sink::Jsonl { out: out(), scratch: Vec::new() },
    };
    let (flow, stopped) = {
        let mut items = items;
        let flow = executor::block_on_stream((&mut items).enumerate().ready_chunks(BATCH)).try_fold((opened, Vec::new()), |state, batch| {
            let (mut sink, failures) = batch.into_iter().try_fold(state, |(mut sink, mut failures), (row, item)| match item.map(|value| appended(&mut sink, row, &value)) {
                Ok(Ok(())) => ControlFlow::Continue((sink, failures)),
                Ok(Err(error @ OutputError::Write { source: _ })) => ControlFlow::Break((failures, Err(error))),
                Ok(Err(error @ (OutputError::Encode { row: _, source: _ } | OutputError::Column { row: _, name: _ } | OutputError::Pager(_) | OutputError::Prompt(_)))) => {
                    failures.push(E::from(error));
                    ControlFlow::Continue((sink, failures))
                }
                Err(failure) => {
                    failures.push(failure);
                    ControlFlow::Continue((sink, failures))
                }
            })?;
            match flushed(&mut sink) {
                Ok(()) => ControlFlow::Continue((sink, failures)),
                Err(error) => ControlFlow::Break((failures, Err(error))),
            }
        });
        (flow, items.take_result().is_some())
    };
    let (failures, ended) = flow.map_continue(|(sink, failures)| (failures, finished(sink, header, paging, stopped || !terminal))).into_value();
    let closed = |error: &OutputError| matches!(error, OutputError::Write { source } if source.kind() == io::ErrorKind::BrokenPipe);
    NEVec::try_from_vec(iter::chain(failures, ended.err().filter(|error| !closed(error)).map(E::from)).collect()).map_or(Ok(if stopped { ExitCode::TempFail } else { ExitCode::Ok }), Err)
}
