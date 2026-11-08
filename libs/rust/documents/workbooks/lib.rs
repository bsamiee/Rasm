#![feature(cmp_minmax, iterator_try_collect)]

use core::{iter, num::TryFromIntError};

use calamine::{CellErrorType, Data, DataType as _, Range};
use ironcalc::{base::expressions::token::Error, error::XlsxError};
use jiff::{SignedDuration, civil};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Value<'cell> {
    Integer(i64),
    Number(f64),
    Text(&'cell str),
    Boolean(bool),
    DateTime(civil::DateTime),
    Duration(SignedDuration),
    Error(&'cell CellErrorType),
    Empty,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Refusal {
    #[display("is not defined")]
    Missing,
    #[display("is not one cell area")]
    Area,
    #[display("is narrower than the block")]
    Width,
}

#[derive(Debug, thiserror::Error)]
pub enum WorkbooksError {
    #[error(transparent)]
    Decrypt(#[from] office_crypto::DecryptError),
    #[error(transparent)]
    Read(#[from] calamine::Error),
    #[error("date-time or duration cell at row {}, column {} refused", cell.0, cell.1)]
    Moment { cell: (u32, u32), source: jiff::Error },
    #[error("record {index} refused")]
    Record { index: usize, source: calamine::DeError },
    #[error("worksheet cell at row {}, column {} refused", cell.0, cell.1)]
    Write { cell: (rust_xlsxwriter::RowNum, rust_xlsxwriter::ColNum), source: rust_xlsxwriter::XlsxError },
    #[error("worksheet cell at row {}, column {} exceeds supported coordinates", cell.0, cell.1)]
    Coordinate { cell: (u64, u64), source: TryFromIntError },
    #[error("template name {name} {refusal}")]
    Template { name: String, refusal: Refusal },
    #[error(transparent)]
    Workbook(#[from] umya_spreadsheet::XlsxError),
    #[error("recalculated {kind} at sheet {sheet}, row {}, column {} has no umya error kind", cell.0, cell.1)]
    Cached { sheet: u32, cell: (u32, u32), kind: Error },
    #[error(transparent)]
    Load(#[from] XlsxError),
    #[error("ironcalc model refused: {0}")]
    Model(String),
    #[error("ODS cell at row {}, column {} refused", cell.0, cell.1)]
    Ods { cell: (u32, u32), source: spreadsheet_ods::OdsError },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn value(at: (u32, u32), cell: &Data) -> Result<Value<'_>, WorkbooksError> {
    match cell {
        Data::Int(integer) => Ok(Value::Integer(*integer)),
        Data::Float(number) => Ok(Value::Number(*number)),
        Data::String(text) => Ok(Value::Text(text)),
        Data::Bool(boolean) => Ok(Value::Boolean(*boolean)),
        Data::DateTime(serial) if serial.is_duration() => SignedDuration::try_from_secs_f64(serial.as_f64() * SignedDuration::from_hours(24).as_secs_f64()).map(Value::Duration),
        Data::DateTime(serial) => {
            let (year, month, day, hour, minute, second, milli) = serial.to_ymd_hms_milli();
            civil::DateTime::new(year.cast_signed(), month.cast_signed(), day.cast_signed(), hour.cast_signed(), minute.cast_signed(), second.cast_signed(), i32::from(milli).strict_mul(1_000_000))
                .map(Value::DateTime)
        }
        Data::DateTimeIso(text) => text.parse().map(Value::DateTime),
        Data::DurationIso(text) => text.parse().map(Value::Duration),
        Data::Error(error) => Ok(Value::Error(error)),
        Data::Empty => Ok(Value::Empty),
    }
    .map_err(|source| WorkbooksError::Moment { cell: at, source })
}

fn cells(block: &Range<Data>) -> impl Iterator<Item = ((u32, u32), (u32, u32), &Data)> {
    block.start().zip(block.end()).into_iter().flat_map(|((top, left), (bottom, right))| {
        iter::zip(block.rows(), top..=bottom)
            .flat_map(move |(row_cells, row)| iter::zip(row_cells, left..=right).map(move |(cell, column)| ((row.strict_sub(top), column.strict_sub(left)), (row, column), cell)))
    })
}

fn values<R, C>(block: &Range<Data>, (row, column): (R, C)) -> impl Iterator<Item = Result<((R, C), Value<'_>), WorkbooksError>>
where
    R: Into<u64> + TryFrom<u64, Error = TryFromIntError>,
    C: Into<u64> + TryFrom<u64, Error = TryFromIntError>,
{
    let (top, left): (u64, u64) = (row.into(), column.into());
    cells(block).filter(|&(_, _, cell)| !cell.is_empty()).map(move |((down, across), at, cell)| -> Result<_, WorkbooksError> {
        let target = (top.strict_add(u64::from(down)), left.strict_add(u64::from(across)));
        let placed = R::try_from(target.0).and_then(|placed_row| C::try_from(target.1).map(|placed_column| (placed_row, placed_column)));
        Ok((placed.map_err(|source| WorkbooksError::Coordinate { cell: target, source })?, value(at, cell)?))
    })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod evaluation;
pub mod ods;
mod reading;
mod templates;
pub mod writing;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use evaluation::recalculate;
pub use reading::{open, records};
pub use templates::fill;
