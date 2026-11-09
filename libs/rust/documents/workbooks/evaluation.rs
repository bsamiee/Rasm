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

use crate::{Uncached, WorkbooksError};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn recalculate(book: &mut Workbook, locale: &str, timezone: &str) -> Result<Vec<Uncached>, WorkbooksError> {
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
    let results = model
        .workbook
        .worksheets
        .iter()
        .map(|worksheet| worksheet.sheet_data.cells().filter_map(|(row, column, cell)| result(&cell).map(|outcome| ((row.cast_unsigned(), column.cast_unsigned()), outcome))).collect::<Vec<_>>())
        .collect::<Vec<_>>();
    drop(model);
    Ok(iter::zip(book.sheet_collection_mut(), iter::zip(results, 0_u32..))
        .flat_map(|(worksheet, (cells, sheet))| {
            cells.into_iter().filter_map(move |((row, column), outcome)| {
                let cell = worksheet.cell_mut((column, row));
                let uncached = outcome.as_ref().err().map(|kind| Uncached { sheet, cell: (row, column), kind: kind.clone() });
                match outcome {
                    Ok(CellRawValue::String(text) | CellRawValue::Lazy(text)) => cell.set_formula_result_string(text),
                    Ok(CellRawValue::RichText(text)) => cell.set_formula_result_string(text.text()),
                    Ok(CellRawValue::Numeric(number)) => cell.set_formula_result_number(number),
                    Ok(CellRawValue::Bool(boolean)) => cell.set_formula_result_bool(boolean),
                    Ok(CellRawValue::Error(error)) => cell.set_formula_result_error(error),
                    Ok(CellRawValue::Empty) | Err(Error::ERROR | Error::NIMPL | Error::SPILL | Error::CALC | Error::CIRC | Error::REF | Error::NAME | Error::VALUE | Error::DIV | Error::NA | Error::NUM | Error::NULL) =>
                        cell.set_formula_result_blank(),
                };
                uncached
            })
        })
        .collect())
}
