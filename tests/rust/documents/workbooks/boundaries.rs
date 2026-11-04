use calamine::{Data, Range};
use rasm_workbooks::{Refusal, WorkbooksError, ods, templates};
use spreadsheet_ods::{Sheet, Value, WorkBook};
use umya_spreadsheet::{DefinedName, Style, helper::address::join_address};

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn ods_offsets_refuse_overflow_before_mutation() {
    let maximum = u32::MAX;
    let outside = u64::from(maximum).strict_add(1);
    for (end, origin, expected) in [((1, 0), (maximum, 0), (outside, 0)), ((0, 1), (0, maximum), (0, outside))] {
        let mut block = Range::new((0, 0), end);
        block.set_value((0, 0), Data::Int(1));
        block.set_value(end, Data::Int(2));
        let mut book = WorkBook::new_empty();
        let mut sheet = Sheet::new("Schedule");
        sheet.set_value(origin.0, origin.1, 17);

        core::assert_matches!(ods::block(&mut book, &mut sheet, origin, &block), Err(WorkbooksError::Coordinate { cell, source: _ }) if cell == expected);
        assert_eq!(sheet.value(origin.0, origin.1), &Value::from(17), "Refused offset preserves existing destination");
        assert_eq!(sheet.cell_count(), 1, "Refused offset creates no cells");
        assert_eq!(book.iter_cellstyles().count(), 0, "Refused offset creates no styles");
    }
}

#[test]
fn templates_refuse_zero_rows_before_mutation() -> Result<(), WorkbooksError> {
    let (name, at, content) = ("Area", "A1", "Original");
    for address in ["A0", "A0:A1", "A1:A0"] {
        let mut book = umya_spreadsheet::new_file();
        let sheet = book.sheet_mut(0)?;
        sheet.cell_mut(at).set_value_string(content);
        let address = join_address(sheet.name(), address);
        let mut defined = DefinedName::default();
        defined.set_name(name).set_address(address);
        book.defined_names_mut().push(defined);
        let mut block = Range::new((0, 0), (0, 0));
        block.set_value((0, 0), Data::Int(1));

        core::assert_matches!(templates::fill(&mut book, name, &block), Err(WorkbooksError::Template { name: _, refusal: Refusal::Area }));
        assert_eq!(book.sheet(0)?.cell(at).map(umya_spreadsheet::Cell::value), Some(content.into()), "Refused area preserves existing cells");
    }
    Ok(())
}

#[test]
fn template_growth_preserves_styles_and_other_sheet_positions() -> Result<(), WorkbooksError> {
    let (target, other, other_at, name, tail, untouched) = ("O'Brien", "Summary", "C7", "Area", "Tail", "Untouched");
    let mut book = umya_spreadsheet::new_file();
    book.set_sheet_name(0, target)?;
    let mut style = Style::default();
    style.number_format_mut().set_format_code("0.000");
    book.sheet_mut(0).map(|sheet| {
        sheet.cell_mut("A2").set_style(style.clone()).set_value_number(0);
        sheet.cell_mut("A3").set_value_string(tail);
    })?;
    book.new_sheet(other)?.cell_mut(other_at).set_value_string(untouched);
    let mut defined = DefinedName::default();
    defined.set_name(name).set_address(join_address(target, "$A$2")).set_hidden(true);
    book.defined_names_mut().push(defined);
    let mut block = Range::new((0, 0), (2, 0));
    for (row, number) in [(0, 11), (1, 22), (2, 33)] {
        block.set_value((row, 0), Data::Int(number));
    }

    templates::fill(&mut book, name, &block)?;
    let sheet = book.sheet(0)?;
    for (row, number) in [(2, 11.0), (3, 22.0), (4, 33.0)] {
        let cell = sheet.cell((1, row));
        assert_eq!(cell.and_then(umya_spreadsheet::Cell::value_number), Some(number), "Growth writes every input row");
        assert_eq!(cell.map(umya_spreadsheet::Cell::style), Some(&style), "Growth retains template style");
    }
    assert_eq!(sheet.cell("A5").map(umya_spreadsheet::Cell::value), Some(tail.into()), "Target rows move with insertion");
    assert_eq!(book.sheet_by_name(other)?.cell(other_at).map(umya_spreadsheet::Cell::value), Some(untouched.into()), "Other sheet positions remain unchanged");
    let grown = &book.defined_names()[0];
    assert_eq!(grown.address(), join_address(target, "$A$2:$A$4"), "Selected name grows in its quoted target sheet");
    assert!(grown.hidden(), "Selected name retains metadata");
    Ok(())
}
