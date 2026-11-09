use core::cell::LazyCell;

use calamine::{CellErrorType, Data, Range};
use jiff::SignedDuration;
use rust_xlsxwriter::{ColNum, Format, Formula, RowNum, Worksheet};

use crate::{Value, WorkbooksError};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn block(worksheet: &mut Worksheet, origin: (RowNum, ColNum), block: &Range<Data>) -> Result<(), WorkbooksError> {
    let duration_format = LazyCell::new(|| Format::new().set_num_format_index(46));
    crate::values(block, origin).try_collect::<Vec<_>>()?.into_iter().try_for_each(|(cell @ (row, column), value)| {
        match value {
            Value::Integer(integer) => worksheet.write(row, column, integer),
            Value::Number(number) => worksheet.write(row, column, number),
            Value::Text(text) => worksheet.write(row, column, text),
            Value::Boolean(boolean) => worksheet.write(row, column, boolean),
            Value::DateTime(datetime) => worksheet.write(row, column, &datetime),
            Value::Duration(duration) => worksheet.write_with_format(row, column, duration.div_duration_f64(SignedDuration::from_hours(24)), &duration_format),
            Value::Error(CellErrorType::GettingData) => worksheet.write(row, column, Formula::new("=#GETTING_DATA").set_result("#GETTING_DATA")),
            Value::Error(error) => {
                let text = error.to_string();
                worksheet.write(row, column, Formula::new(&text).set_result(text))
            }
            Value::Empty => Ok(&mut *worksheet),
        }
        .map(drop)
        .map_err(|source| WorkbooksError::Write { cell, source })
    })
}
