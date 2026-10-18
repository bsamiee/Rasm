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

fn main() {
    black_box(&(
        summed("1", "2"),
        paired("3", "4"),
        compared("8", "9"),
        built("1", "9"),
        dependent("5"),
        separate("6", "7"),
        summed_inline("1", "2"),
        built_inline("1", "9"),
        nested("1", "2"),
        streamed(&mut "1\n2".lines()),
        optional(&[1, 2]),
    ));
}

fn summed_inline(left: &str, right: &str) -> Result<u32, ParseIntError> { Ok(parse(left)?.strict_add(parse(right)?)) }

fn built_inline(left: &str, right: &str) -> Result<Range<u32>, ParseIntError> { Ok(Range { start: parse(left)?, end: parse(right)? }) }

fn nested(left: &str, right: &str) -> Result<(usize, u32), ParseIntError> { Ok((parse(left.trim())?.to_string().len(), parse(right)?.strict_add(1))) }

fn streamed(lines: &mut core::str::Lines<'_>) -> Result<(u32, u32), ParseIntError> { Ok((parse(lines.next().unwrap_or_default())?, parse(lines.next().unwrap_or_default())?)) }

fn optional(items: &[u32]) -> Option<u32> { Some(items.first()?.strict_add(*items.last()?)) }
