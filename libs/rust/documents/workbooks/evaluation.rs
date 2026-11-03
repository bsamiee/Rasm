use core::iter;

use ironcalc::{
    base::{
        Model,
        types::{Cell, FormulaValue, SpillValue},
    },
    import,
};
use umya_spreadsheet::{CellRawValue, Workbook, writer};

use crate::WorkbooksError;

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn recalculate(book: &mut Workbook, locale: &str, timezone: &str) -> Result<(), WorkbooksError> {
    let result = |cell: &Cell| match cell {
        Cell::CellFormula { f: _, s: _, v: FormulaValue::Unevaluated } | Cell::ArrayFormula { f: _, s: _, r: _, kind: _, v: FormulaValue::Unevaluated } => Some(Ok(CellRawValue::Empty)),
        Cell::CellFormula { f: _, s: _, v: FormulaValue::Boolean(boolean) }
        | Cell::ArrayFormula { f: _, s: _, r: _, kind: _, v: FormulaValue::Boolean(boolean) }
        | Cell::SpillCell { s: _, a: _, v: SpillValue::Boolean(boolean) } => Some(Ok(CellRawValue::Bool(*boolean))),
        Cell::CellFormula { f: _, s: _, v: FormulaValue::Number(number) }
        | Cell::ArrayFormula { f: _, s: _, r: _, kind: _, v: FormulaValue::Number(number) }
        | Cell::SpillCell { s: _, a: _, v: SpillValue::Number(number) } => Some(Ok(CellRawValue::Numeric(*number))),
        Cell::CellFormula { f: _, s: _, v: FormulaValue::Text(text) }
        | Cell::ArrayFormula { f: _, s: _, r: _, kind: _, v: FormulaValue::Text(text) }
        | Cell::SpillCell { s: _, a: _, v: SpillValue::Text(text) } => Some(Ok(CellRawValue::String(text.as_str().into()))),
        Cell::CellFormula { f: _, s: _, v: FormulaValue::Error { ei, d: _ } }
        | Cell::ArrayFormula { f: _, s: _, r: _, kind: _, v: FormulaValue::Error { ei, d: _ } }
        | Cell::SpillCell { s: _, a: _, v: SpillValue::Error(ei) } => Some(ei.to_string().parse().map(CellRawValue::Error)),
        Cell::EmptyCell { s: _ } | Cell::BooleanCell { v: _, s: _ } | Cell::NumberCell { v: _, s: _ } | Cell::ErrorCell { ei: _, s: _ } | Cell::SharedString { si: _, s: _ } => None,
    };
    let mut bytes = Vec::new();
    writer::xlsx::write_writer(book, &mut bytes)?;
    let mut model = Model::from_workbook(import::load_from_xlsx_bytes(&bytes, book.properties().title(), locale, timezone)?, "en").map_err(WorkbooksError::Model)?;
    model.evaluate();
    let results = iter::zip(&model.workbook.worksheets, 0_u32..)
        .map(|(worksheet, sheet)| {
            worksheet
                .sheet_data
                .cells()
                .filter_map(|(row, column, cell)| {
                    let at = (row.cast_unsigned(), column.cast_unsigned());
                    result(&cell).map(|outcome| outcome.map(|raw| (at, raw)).map_err(|source| WorkbooksError::Cached { sheet, cell: at, source }))
                })
                .try_collect::<Vec<_>>()
        })
        .try_collect::<Vec<_>>()?;
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
