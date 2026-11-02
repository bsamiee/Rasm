use core::{iter, ops::ControlFlow};
use std::io::{self, IsTerminal as _, StdoutLock, Write as _};

use anstream::{AutoStream, ColorChoice};
use anstyle::{Effects, Style};
use comfy_table::{Attribute, Cell, Color, ContentArrangement, Table, presets};
use csv::{ByteRecord, IntoInnerError, ReaderBuilder, Writer};
use minus::{Pager, hooks::Hook};
use nonempty_collections::NEVec;
use serde::Serialize;

use crate::{Format, OutputError, Paging, io_error_kind, painted};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn written<R, E>(format: Option<Format>, paging: Paging, header: Style, rows: impl Iterator<Item = Result<R, E>>) -> Result<(), NEVec<E>>
where
    R: Serialize,
    E: From<OutputError>,
{
    enum Sink<R> {
        Lines(StdoutLock<'static>),
        Array(Vec<R>),
        Records(Writer<StdoutLock<'static>>),
        Grid { table: Table, header: Style, paging: Paging },
    }
    fn titled(name: &[u8], style: Style) -> Cell {
        let colored = |color| painted(color, Color::AnsiValue, |r, g, b| Color::Rgb { r, g, b });
        let cell = Cell::new(String::from_utf8_lossy(name)).add_attributes(
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
    }
    let stepped = |sink: &mut Sink<R>, row: usize, value: R| -> Result<(), OutputError> {
        match sink {
            Sink::Lines(out) => {
                serde_json::to_writer(&mut *out, &value).map_err(|source| OutputError::Json { row: Some(row), source })?;
                writeln!(out).map_err(|source| OutputError::Write { source })
            }
            Sink::Array(kept) => {
                kept.push(value);
                Ok(())
            }
            Sink::Records(out) => out.serialize(&value).map_err(|source| OutputError::Records { row, source }),
            Sink::Grid { table, header: style, paging: _ } => {
                let mut record = Writer::from_writer(Vec::new());
                let records = try bikeshed Result<Vec<ByteRecord>, csv::Error> {
                    record.serialize(&value)?;
                    let bytes = record.into_inner().map_err(IntoInnerError::into_error)?;
                    ReaderBuilder::new().has_headers(false).from_reader(bytes.as_slice()).into_byte_records().try_collect::<Vec<ByteRecord>>()?
                }
                .map_err(|source| OutputError::Records { row, source })?;
                let Some((fields, names)) = records.split_last() else { return Ok(()) };
                if let Some(names) = names.first()
                    && table.header().is_none()
                {
                    table.set_header(names.iter().map(|name| titled(name, *style)));
                }
                table.add_row(fields.iter().map(String::from_utf8_lossy));
                Ok(())
            }
        }
    };
    let finished = |sink: Sink<R>| -> Result<(), OutputError> {
        match sink {
            Sink::Lines(_) => Ok(()),
            Sink::Array(kept) => {
                let mut out = io::stdout().lock();
                serde_json::to_writer_pretty(&mut out, &kept).map_err(|source| OutputError::Json { row: None, source })?;
                writeln!(out).map_err(|source| OutputError::Write { source })
            }
            Sink::Records(mut out) => out.flush().map_err(|source| OutputError::Write { source }),
            Sink::Grid { table, header: _, paging: _ } if table.is_empty() => Ok(()),
            Sink::Grid { table, header: _, paging: Paging::Never } => writeln!(anstream::stdout(), "{table}").map_err(|source| OutputError::Write { source }),
            Sink::Grid { table, header: _, paging: requested @ (Paging::Auto | Paging::Always) } => {
                let pager = Pager::new();
                try bikeshed Result<(), OutputError> {
                    pager.remove_hook(Hook::PostPagerExit, 1)?;
                    pager.set_run_no_overflow(matches!(requested, Paging::Always))?;
                    pager.set_text(table.to_string())?;
                    minus::page_all(pager)?
                }
            }
        }
    };
    let opened = match format.unwrap_or_else(|| if io::stdout().is_terminal() { Format::Grid } else { Format::Jsonl }) {
        Format::Grid => {
            let mut table = Table::new();
            table.load_style(presets::UTF8_FULL_CONDENSED).set_content_arrangement(ContentArrangement::Dynamic);
            let header = match AutoStream::choice(&io::stdout()) {
                ColorChoice::Never => Style::new(),
                ColorChoice::Auto | ColorChoice::AlwaysAnsi | ColorChoice::Always => {
                    table.enforce_styling();
                    header
                }
            };
            Sink::Grid { table, header, paging }
        }
        Format::Csv => Sink::Records(Writer::from_writer(io::stdout().lock())),
        Format::Json => Sink::Array(Vec::new()),
        Format::Jsonl => Sink::Lines(io::stdout().lock()),
    };
    let flow = rows.enumerate().try_fold((opened, Vec::new()), |(mut sink, mut failures), (row, item)| match item.map(|value| stepped(&mut sink, row, value)) {
        Ok(Ok(())) => ControlFlow::Continue((sink, failures)),
        Ok(Err(error)) => ControlFlow::Break((failures, error)),
        Err(failure) => {
            failures.push(failure);
            ControlFlow::Continue((sink, failures))
        }
    });
    let (failures, ended) = match flow {
        ControlFlow::Continue((sink, failures)) => (failures, finished(sink)),
        ControlFlow::Break((failures, error)) => (failures, Err(error)),
    };
    NEVec::try_from_vec(iter::chain(failures, ended.err().filter(|error| io_error_kind(error) != Some(io::ErrorKind::BrokenPipe)).map(E::from)).collect()).map_or(Ok(()), Err)
}
