use core::iter;

use ironcalc::{
    base::{
        Model,
        expressions::token::Error,
        types::{Cell, FormulaValue, SpillValue},
    },
    import,
};
use umya_spreadsheet::{CellErrorType, CellRawValue, Workbook, writer};

use crate::WorkbooksError;

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn recalculate(book: &mut Workbook, locale: &str, timezone: &str) -> Result<(), WorkbooksError> {
    let result = |cell: &Cell| match cell {
        Cell::CellFormula { v: FormulaValue::Unevaluated, .. } | Cell::ArrayFormula { v: FormulaValue::Unevaluated, .. } => Some(Ok(CellRawValue::Empty)),
        Cell::CellFormula { v: FormulaValue::Boolean(boolean), .. } | Cell::ArrayFormula { v: FormulaValue::Boolean(boolean), .. } | Cell::SpillCell { v: SpillValue::Boolean(boolean), .. } =>
            Some(Ok(CellRawValue::Bool(*boolean))),
        Cell::CellFormula { v: FormulaValue::Number(number), .. } | Cell::ArrayFormula { v: FormulaValue::Number(number), .. } | Cell::SpillCell { v: SpillValue::Number(number), .. } =>
            Some(Ok(CellRawValue::Numeric(*number))),
        Cell::CellFormula { v: FormulaValue::Text(text), .. } | Cell::ArrayFormula { v: FormulaValue::Text(text), .. } | Cell::SpillCell { v: SpillValue::Text(text), .. } =>
            Some(Ok(CellRawValue::String(text.as_str().into()))),
        Cell::CellFormula { v: FormulaValue::Error { ei, .. }, .. } | Cell::ArrayFormula { v: FormulaValue::Error { ei, .. }, .. } | Cell::SpillCell { v: SpillValue::Error(ei), .. } => Some(
            match ei {
                Error::REF => Ok(CellErrorType::Ref),
                Error::NAME => Ok(CellErrorType::Name),
                Error::VALUE => Ok(CellErrorType::Value),
                Error::DIV => Ok(CellErrorType::Div0),
                Error::NA => Ok(CellErrorType::NA),
                Error::NUM => Ok(CellErrorType::Num),
                Error::NULL => Ok(CellErrorType::Null),
                Error::ERROR | Error::NIMPL | Error::SPILL | Error::CALC | Error::CIRC => Err(ei.clone()),
            }
            .map(CellRawValue::Error),
        ),
        Cell::EmptyCell { .. } | Cell::BooleanCell { .. } | Cell::NumberCell { .. } | Cell::ErrorCell { .. } | Cell::SharedString { .. } => None,
    };
    let mut bytes = Vec::new();
    writer::xlsx::write_writer(book, &mut bytes)?;
    let mut model = Model::from_workbook(import::load_from_xlsx_bytes(&bytes, book.properties().title(), locale, timezone)?, "en").map_err(WorkbooksError::Model)?;
    drop(bytes);
    model.evaluate();
    let results = iter::zip(&model.workbook.worksheets, 0_u32..)
        .map(|(worksheet, sheet)| {
            worksheet
                .sheet_data
                .cells()
                .filter_map(|(row, column, cell)| {
                    let at = (row.cast_unsigned(), column.cast_unsigned());
                    result(&cell).map(|outcome| outcome.map(|raw| (at, raw)).map_err(|kind| WorkbooksError::Cached { sheet, cell: at, kind }))
                })
                .try_collect::<Vec<_>>()
        })
        .try_collect::<Vec<_>>()?;
    drop(model);
    for (worksheet, cells) in iter::zip(book.sheet_collection_mut(), results) {
        for ((row, column), raw) in cells {
            let cell = worksheet.cell_mut((column, row));
            match raw {
                CellRawValue::String(text) | CellRawValue::Lazy(text) => cell.set_formula_result_string(text),
                CellRawValue::RichText(text) => cell.set_formula_result_string(text.text()),
                CellRawValue::Numeric(number) => cell.set_formula_result_number(number),
                CellRawValue::Bool(boolean) => cell.set_formula_result_bool(boolean),
                CellRawValue::Error(error) => cell.set_formula_result_error(error),
                CellRawValue::Empty => cell.set_formula_result_blank(),
            };
        }
    }
    Ok(())
}
