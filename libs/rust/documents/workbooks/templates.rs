use core::cmp;

use calamine::{Data, Range};
use jiff::{SignedDuration, civil};
use rust_xlsxwriter::IntoExcelDateTime as _;
use umya_spreadsheet::{
    Address, ColumnReference, DefinedName, RowReference, Workbook,
    helper::{
        address::{is_address, join_address},
        coordinate::coordinate_from_index_with_lock,
    },
};

use crate::{Refusal, Value, WorkbooksError};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn fill(book: &mut Workbook, name: &str, block: &Range<Data>) -> Result<(), WorkbooksError> {
    let refused = |refusal| WorkbooksError::Template { name: name.to_owned(), refusal };
    let values = crate::values(block)?;
    let (scope, index, defined) = book
        .defined_names()
        .iter()
        .enumerate()
        .map(|(index, defined)| (None, index, defined))
        .chain(book.sheet_collection().iter().enumerate().flat_map(|(sheet, worksheet)| worksheet.defined_names().iter().enumerate().map(move |(index, defined)| (Some(sheet), index, defined))))
        .find(|&(_, _, defined)| defined.name().chars().flat_map(char::to_lowercase).eq(name.chars().flat_map(char::to_lowercase)))
        .ok_or_else(|| refused(Refusal::Missing))?;
    let address = Some(defined.address()).filter(|text| is_address(text.as_str())).map(|text| Address::default().set_address(text).clone()).ok_or_else(|| refused(Refusal::Area))?;
    let range = address.range();
    let corner = |column: Option<&ColumnReference>, row: Option<&RowReference>| column.map(ColumnReference::num).zip(row.map(RowReference::num));
    let ((start_column, start_row), (end_column, end_row)) = corner(range.coordinate_start_col(), range.coordinate_start_row())
        .map(|start| (start, corner(range.coordinate_end_col(), range.coordinate_end_row()).unwrap_or(start)))
        .ok_or_else(|| refused(Refusal::Area))?;
    let ([first_column, last_column], [first_row, last_row]) = (cmp::minmax(start_column, end_column), cmp::minmax(start_row, end_row));
    let (row_span, column_span) = block.start().zip(block.end()).map_or((0, 0), |((top, left), (bottom, right))| (bottom.strict_sub(top), right.strict_sub(left)));
    if column_span > last_column.strict_sub(first_column) {
        return Err(refused(Refusal::Width));
    }
    let sheet_name = address.sheet_name().to_owned();
    let template = book.sheet_by_name(&sheet_name)?;
    let styles = (first_column..=last_column).filter_map(|column| template.cell((column, first_row)).map(|cell| (column, cell.style().clone()))).collect::<Vec<_>>();
    let extra = row_span.saturating_sub(last_row.strict_sub(first_row));
    let bottom = last_row.strict_add(extra);
    let grown = (extra > 0).then(|| {
        let mut grown = DefinedName::default();
        grown.set_name(defined.name()).set_address(join_address(
            &sheet_name,
            &format!("{}:{}", coordinate_from_index_with_lock(first_column, first_row, true, true), coordinate_from_index_with_lock(last_column, bottom, true, true)),
        ));
        grown.set_hidden(defined.hidden());
        if defined.has_local_sheet_id() {
            grown.set_local_sheet_id(defined.local_sheet_id());
        }
        grown
    });
    let epoch = if book.date1904() { civil::date(1904, 1, 1).to_excel_serial_date() } else { 0.0 };
    let sheet = book.sheet_by_name_mut(&sheet_name)?;
    sheet.insert_new_row(last_row, extra);
    for row in last_row..bottom {
        for (column, style) in &styles {
            sheet.cell_mut((*column, row)).set_style(style.clone());
        }
    }
    for (_, cell) in sheet.collection_to_hashmap_mut().iter_mut().filter(|&(&(row, column), _)| (first_row..=bottom).contains(&row) && (first_column..=last_column).contains(&column)) {
        cell.set_blank();
    }
    for (row, column, value) in values {
        let cell = sheet.cell_mut((first_column.strict_add(column), first_row.strict_add(row)));
        match value {
            Value::Integer(integer) => cell.set_value(integer.to_string()),
            Value::Number(number) => cell.set_value_number(number),
            Value::Text(text) => cell.set_value_string(text),
            Value::Boolean(boolean) => cell.set_value_bool(boolean),
            Value::DateTime(datetime) => cell.set_value_number(datetime.to_excel_serial_date() - epoch),
            Value::Duration(duration) => cell.set_value_number(duration.div_duration_f64(SignedDuration::from_hours(24))),
            Value::Error(error) => cell.set_value(error.to_string()),
            Value::Empty => cell.set_blank(),
        };
    }
    if let Some(grown) = grown
        && let Some(stored) = match scope {
            Some(local_sheet) => book.sheet_mut(local_sheet)?.defined_names_mut(),
            None => book.defined_names_mut(),
        }
        .get_mut(index)
    {
        *stored = grown;
    }
    Ok(())
}
