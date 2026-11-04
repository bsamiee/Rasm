use core::cell::LazyCell;

use calamine::{Data, Range};
use jiff::SignedDuration;
use spreadsheet_ods::{CellStyle, OdsError, Sheet, Value, WorkBook, format};

use crate::WorkbooksError;

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn block(book: &mut WorkBook, sheet: &mut Sheet, origin: (u32, u32), block: &Range<Data>) -> Result<(), WorkbooksError> {
    let cells = crate::values(block)
        .map(|value| {
            let (offset, value) = value?;
            let at = (u64::from(origin.0).strict_add(u64::from(offset.0)), u64::from(origin.1).strict_add(u64::from(offset.1)));
            let cell = u32::try_from(at.0).and_then(|row| u32::try_from(at.1).map(|column| (row, column))).map_err(|source| WorkbooksError::Coordinate { cell: at, source })?;
            match value {
                crate::Value::Integer(integer) => Ok((Value::from(integer), None, false)),
                crate::Value::Number(number) => Ok((Value::from(number), None, false)),
                crate::Value::Text(text) => Ok((Value::from(text), None, false)),
                crate::Value::Boolean(boolean) => Ok((Value::from(boolean), None, false)),
                crate::Value::DateTime(datetime) =>
                    datetime.to_string().parse().map(|parsed| (Value::DateTime(parsed), None, false)).map_err(|source| WorkbooksError::Ods { cell, source: OdsError::from(source) }),
                crate::Value::Duration(duration) => Ok((Value::from(duration.div_duration_f64(SignedDuration::from_hours(24))), None, true)),
                crate::Value::Error(error) => Ok((Value::from(error.to_string()), Some(format!("of:={error}")), false)),
                crate::Value::Empty => Ok((Value::Empty, None, false)),
            }
            .map(|written| (cell, written))
        })
        .try_collect::<Vec<_>>()?;
    let style = LazyCell::new(|| {
        let interval = book.add_timeduration_format(format::create_time_interval_format(""));
        book.add_cellstyle(CellStyle::new("", &interval))
    });
    for ((row, column), (content, formula, timed)) in cells {
        if timed {
            sheet.set_styled_value(row, column, content, &style);
        } else {
            sheet.set_value(row, column, content);
        }
        match formula {
            Some(formula) => sheet.set_formula(row, column, formula),
            None => sheet.clear_formula(row, column),
        }
    }
    Ok(())
}
