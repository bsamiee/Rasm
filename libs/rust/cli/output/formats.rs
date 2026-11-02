use core::{
    fmt::{self, Write as _},
    iter,
    ops::ControlFlow,
};
use std::io::{self, IsTerminal as _, Write as _};

use anstream::{AutoStream, ColorChoice};
use anstyle::{Effects, Style};
use clap::{Args, ValueEnum};
use comfy_table::{Attribute, Cell, CellAlignment, Color, ContentArrangement, Table, presets};
use futures::{Stream, executor, stream::TakeUntil};
use indexmap::IndexSet;
use minus::{Pager, hooks::Hook};
use nonempty_collections::NEVec;
use serde::{Serialize, ser::Error as _};
use serde_json::{
    Serializer,
    ser::{Formatter as _, PrettyFormatter},
};
use sysexits::ExitCode;
use tracing_indicatif::{IndicatifWriter, suspend_tracing_indicatif, writer};
use tracing_subscriber::fmt::writer::EitherWriter;

use crate::{
    OutputError,
    cells::{self, Field, Row},
    painted,
};

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
pub(crate) struct Options {
    #[arg(long, global = true, value_enum, help = "Row format, grid on a terminal and JSON lines elsewhere")]
    format: Option<Format>,
    #[arg(long, global = true, value_name = "WHEN", value_enum, default_value_t, help = "Page the grid through the in-process pager")]
    paging: Paging,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn written<R, E, S, F>(Options { format, paging }: Options, header: Style, progress: IndicatifWriter<writer::Stdout>, items: TakeUntil<S, F>) -> Result<ExitCode, NEVec<E>>
where
    R: Serialize,
    E: From<OutputError>,
    S: Stream<Item = Result<R, E>> + Unpin,
    F: Future + Unpin,
{
    type Out = EitherWriter<IndicatifWriter<writer::Stdout>, io::Stdout>;
    enum Sink {
        Jsonl { out: Out, scratch: Vec<u8> },
        Json { out: Out, scratch: Vec<u8>, formatter: PrettyFormatter<'static>, first: bool },
        Csv { writer: Box<csv::Writer<Out>>, columns: Option<IndexSet<String>> },
        Grid { rows: Vec<Row> },
    }
    let terminal = io::stdout().is_terminal();
    let failed = |source| OutputError::Write { source };
    let flattened = |row: usize, value: &R| cells::flattened(value).map_err(|source| OutputError::Encode { row, source });
    let recorded = |row| {
        move |source: csv::Error| match source.kind() {
            csv::ErrorKind::Io(failure) => OutputError::Write { source: io::Error::new(failure.kind(), source) },
            _ => OutputError::Encode { row, source: serde_json::Error::custom(source) },
        }
    };
    let celled = |Field { text, numeric }: &Field| Cell::new(escaped(text)).set_alignment(if *numeric { CellAlignment::Right } else { CellAlignment::Left });
    let titled = |name: &str, style: Style| {
        let colored = |color| painted(color, Color::AnsiValue, |r, g, b| Color::Rgb { r, g, b });
        let cell = Cell::new(escaped(name)).add_attributes(
            [
                (Effects::BOLD, Attribute::Bold),
                (Effects::DIMMED, Attribute::Dim),
                (Effects::ITALIC, Attribute::Italic),
                (Effects::UNDERLINE, Attribute::Underlined),
                (Effects::DOUBLE_UNDERLINE, Attribute::DoubleUnderlined),
                (Effects::CURLY_UNDERLINE, Attribute::Undercurled),
                (Effects::DOTTED_UNDERLINE, Attribute::Underdotted),
                (Effects::DASHED_UNDERLINE, Attribute::Underdashed),
                (Effects::BLINK, Attribute::SlowBlink),
                (Effects::INVERT, Attribute::Reverse),
                (Effects::HIDDEN, Attribute::Hidden),
                (Effects::STRIKETHROUGH, Attribute::CrossedOut),
            ]
            .into_iter()
            .filter_map(|(effect, attribute)| style.get_effects().contains(effect).then_some(attribute))
            .collect(),
        );
        let cell = style.get_fg_color().map(colored).into_iter().fold(cell, Cell::fg);
        style.get_bg_color().map(colored).into_iter().fold(cell, Cell::bg)
    };
    let appended = |sink: &mut Sink, row: usize, value: R| -> Result<(), OutputError> {
        match sink {
            Sink::Jsonl { out, scratch } => {
                scratch.clear();
                serde_json::to_writer(&mut *scratch, &value).map_err(|source| OutputError::Encode { row, source })?;
                scratch.push(b'\n');
                out.write_all(scratch).map_err(failed)
            }
            Sink::Json { out, scratch, formatter, first } => {
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
                let lines = scratch.iter().rposition(|byte| *byte == b'\n').map_or_default(|end| end.strict_add(1));
                out.write_all(scratch.drain(..lines).as_slice()).map_err(failed)
            }
            Sink::Csv { writer, columns } => {
                let Row { named, fields } = flattened(row, &value)?;
                let fixed = columns.is_none();
                let columns = columns.get_or_insert_with(|| fields.keys().cloned().collect());
                if fixed && named {
                    writer.write_record(columns.iter()).map_err(recorded(row))?;
                }
                if let Some(name) = fields.keys().find(|name| !columns.contains(*name)) {
                    return Err(OutputError::Column { row, name: name.clone() });
                }
                writer.write_record(columns.iter().map(|name| fields.get(name).map_or("", |field| field.text.as_str()))).map_err(recorded(row))?;
                if terminal { writer.flush().map_err(failed) } else { Ok(()) }
            }
            Sink::Grid { rows } => {
                rows.push(flattened(row, &value)?);
                Ok(())
            }
        }
    };
    let finished = |sink: Sink, stopped: bool| -> Result<(), OutputError> {
        match sink {
            Sink::Jsonl { out: _, scratch: _ } => Ok(()),
            Sink::Csv { mut writer, columns: _ } => writer.flush().map_err(failed),
            Sink::Json { mut out, mut scratch, mut formatter, first } => (try bikeshed io::Result<()> {
                if first {
                    formatter.begin_array(&mut scratch)?;
                }
                formatter.end_array(&mut scratch)?;
                scratch.push(b'\n');
                out.write_all(&scratch)?
            })
            .map_err(failed),
            Sink::Grid { rows } if rows.is_empty() => Ok(()),
            Sink::Grid { rows } => {
                let columns = rows.iter().flat_map(|row| row.fields.keys().map(String::as_str)).collect::<IndexSet<&str>>();
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
                    table.set_header(columns.iter().map(|name| titled(name, style)));
                }
                table.add_rows(rows.iter().map(|row| columns.iter().map(|name| row.fields.get(*name).map_or_else(|| Cell::new(""), celled))));
                suspend_tracing_indicatif(|| match (paging, stopped) {
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
    };
    let out = || if terminal { EitherWriter::A(progress) } else { EitherWriter::B(io::stdout()) };
    let opened = match (format, terminal) {
        (Some(Format::Grid), _) | (None, true) => Sink::Grid { rows: Vec::new() },
        (Some(Format::Csv), _) => Sink::Csv { writer: Box::new(csv::Writer::from_writer(out())), columns: None },
        (Some(Format::Json), _) => Sink::Json { out: out(), scratch: Vec::new(), formatter: PrettyFormatter::new(), first: true },
        (Some(Format::Jsonl), _) | (None, false) => Sink::Jsonl { out: out(), scratch: Vec::new() },
    };
    let (flow, stopped) = {
        let mut items = items;
        let flow = executor::block_on_stream(&mut items).enumerate().try_fold((opened, Vec::new()), |(mut sink, mut failures), (row, item)| match item.map(|value| appended(&mut sink, row, value)) {
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
        });
        (flow, items.take_result().is_some())
    };
    let (failures, ended) = flow.map_continue(|(sink, failures)| (failures, finished(sink, stopped))).into_value();
    let closed = |error: &OutputError| matches!(error, OutputError::Write { source } if source.kind() == io::ErrorKind::BrokenPipe);
    NEVec::try_from_vec(iter::chain(failures, ended.err().filter(|error| !closed(error)).map(E::from)).collect()).map_or(Ok(if stopped { ExitCode::TempFail } else { ExitCode::Ok }), Err)
}

fn escaped(text: &str) -> impl fmt::Display {
    fmt::from_fn(move |formatter| {
        text.chars().try_for_each(|character| if character.is_control() && character != '\n' { fmt::Display::fmt(&character.escape_default(), formatter) } else { formatter.write_char(character) })
    })
}
