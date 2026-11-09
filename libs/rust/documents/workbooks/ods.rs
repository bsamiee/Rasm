use calamine::{Data, Range};
use chrono::TimeDelta;
use spreadsheet_ods::{OdsError, Sheet, Value};

use crate::WorkbooksError;

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn block(sheet: &mut Sheet, origin: (u32, u32), block: &Range<Data>) -> Result<(), WorkbooksError> {
    crate::values(block, origin)
        .map(|value| {
            let (cell, value) = value?;
            match value {
                crate::Value::Integer(integer) => Ok((Value::from(integer), None)),
                crate::Value::Number(number) => Ok((Value::from(number), None)),
                crate::Value::Text(text) => Ok((Value::from(text), None)),
                crate::Value::Boolean(boolean) => Ok((Value::from(boolean), None)),
                crate::Value::DateTime(datetime) =>
                    datetime.to_string().parse().map(|parsed| (Value::DateTime(parsed), None)).map_err(|source| WorkbooksError::Ods { cell, source: OdsError::from(source) }),
                crate::Value::Duration(duration) => TimeDelta::try_seconds(duration.as_secs())
                    .and_then(|whole| whole.checked_add(&TimeDelta::nanoseconds(i64::from(duration.subsec_nanos()))))
                    .map(|span| (Value::TimeDuration(span), None))
                    .ok_or(WorkbooksError::Interval { cell }),
                crate::Value::Error(error) => Ok((Value::from(error.to_string()), Some(format!("of:={error}")))),
                crate::Value::Empty => Ok((Value::Empty, None)),
            }
            .map(|written| (cell, written))
        })
        .try_collect::<Vec<_>>()
        .map(|cells| {
            for ((row, column), (content, formula)) in cells {
                sheet.set_value(row, column, content);
                match formula {
                    Some(formula) => sheet.set_formula(row, column, formula),
                    None => sheet.clear_formula(row, column),
                }
            }
        })
}
