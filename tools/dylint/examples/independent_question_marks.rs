use core::{hint::black_box, num::ParseIntError, ops::Range};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn parse(text: &str) -> Result<u32, ParseIntError> { text.parse() }

fn summed(left: &str, right: &str) -> Result<u32, ParseIntError> {
    let first = parse(left)?;
    let second = parse(right)?;
    Ok(first.strict_add(second))
}

fn paired(left: &str, right: &str) -> Result<(u32, u32), ParseIntError> {
    let first = parse(left)?;
    let second = parse(right)?;
    Ok((first, second))
}

fn compared(left: &str, right: &str) -> Result<bool, ParseIntError> {
    let first = parse(left)?;
    let second = parse(right)?;
    Ok(first == second)
}

fn built(left: &str, right: &str) -> Result<Range<u32>, ParseIntError> {
    let start = parse(left)?;
    let end = parse(right)?;
    Ok(Range { start, end })
}

fn dependent(text: &str) -> Result<u32, ParseIntError> {
    let first = parse(text)?;
    let second = parse(&first.to_string())?;
    Ok(first.strict_add(second))
}

fn separate(left: &str, right: &str) -> Result<u32, ParseIntError> {
    let first = parse(left)?;
    let second = parse(right)?;
    black_box(first);
    Ok(second)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(summed("1", "2"), paired("3", "4"), compared("8", "9"), built("1", "9"), dependent("5"), separate("6", "7"))); }
