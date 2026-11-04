use std::io::Cursor;

use calamine::{Data, Range, RangeDeserializerBuilder, Sheets, open_workbook_auto_from_rs};
use secrecy::{ExposeSecret as _, SecretString};
use serde::de::DeserializeOwned;

use crate::{Value, WorkbooksError, value};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn open(bytes: Vec<u8>, password: Option<&SecretString>) -> Result<Sheets<Cursor<Vec<u8>>>, WorkbooksError> {
    password
        .into_iter()
        .try_fold(bytes, |bytes, secret| office_crypto::decrypt_from_bytes(bytes, secret.expose_secret()))
        .map_err(WorkbooksError::Decrypt)
        .and_then(|decrypted| open_workbook_auto_from_rs(Cursor::new(decrypted)).map_err(WorkbooksError::Read))
}

pub fn records<D, H>(mut range: Range<Data>, headers: &RangeDeserializerBuilder<'_, H>) -> Result<Vec<D>, WorkbooksError>
where
    D: DeserializeOwned,
    H: AsRef<str> + Clone,
{
    let moments = crate::cells(&range)
        .filter(|&(_, _, cell)| matches!(cell, Data::DateTime(_)))
        .map(|(_, at, cell)| {
            value(at, cell).map(|moment| match moment {
                Value::DateTime(datetime) => Some((at, Data::DateTimeIso(datetime.to_string()))),
                Value::Duration(duration) => Some((at, Data::DurationIso(duration.to_string()))),
                Value::Integer(_) | Value::Number(_) | Value::Text(_) | Value::Boolean(_) | Value::Error(_) | Value::Empty => None,
            })
        })
        .filter_map(Result::transpose)
        .try_collect::<Vec<_>>()?;
    for (at, moment) in moments {
        range.set_value(at, moment);
    }
    headers.from_range(&range).map_err(calamine::Error::De)?.enumerate().map(|(index, record)| record.map_err(|source| WorkbooksError::Record { index, source })).collect()
}
