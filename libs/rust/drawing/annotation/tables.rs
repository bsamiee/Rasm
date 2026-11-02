use core::{iter, num::NonZero, range::Range};
use std::sync::Arc;

use enum_map::EnumMap;
use itertools::Itertools as _;
use kurbo::{Affine, BezPath, Line, PathSeg, Point, Rect, Shape as _, Vec2};
use linesweeper::FillRule;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _, nev};
use rasm_linework::{Anchor, Baseline, Pen, Placement, Region, Width, WidthError, cleanup, transform};
use validated::Validated;

use crate::{
    AnnotationError, Graphics, Style, stroked,
    text::{Label, Typesetter, labelled},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, PartialOrd, Ord, enum_map::Enum)]
pub enum Rule {
    Frame,
    Header,
    Rows,
    Columns,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Column {
    pub width: Option<Width> = None,
    pub anchor: Anchor = Anchor::Start,
    pub vertical: Anchor = Anchor::Start,
}

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub struct Cell {
    pub content: Arc<str>,
    pub columns: NonZero<u16> = NonZero::<u16>::MIN,
    pub rows: NonZero<u16> = NonZero::<u16>::MIN,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Table {
    pub at: Point,
    pub columns: NEVec<Column>,
    pub header: Vec<Vec<Cell>>,
    pub body: Vec<Vec<Cell>>,
    pub rules: EnumMap<Rule, Option<Pen>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn draw(typesetter: &mut Typesetter, style: &Style, placement: Placement, table: &Table) -> Result<Graphics, NEVec<AnnotationError>> {
    fn spread(sizes: Vec<f64>, span: Range<usize>, open: impl Fn(usize) -> bool, need: f64) -> Vec<f64> {
        let grown = |index: usize| span.contains(&index) && open(index);
        let excess = need - sizes.get(span).map_or_default(|spanned| spanned.iter().sum::<f64>());
        let share = excess.max(0.0) / sizes.iter().enumerate().filter(|&(index, _)| grown(index)).map(|_| 1.0).sum::<f64>();
        sizes.into_iter().enumerate().map(|(index, size)| if grown(index) { size + share } else { size }).collect()
    }
    let Table { at, columns, header, body, rules } = table;
    let gap = style.gap.into_inner();
    let tolerance = style.tolerance.into_inner();
    let frame = Affine::translate((*placement * *at).to_vec2());
    let lines = iter::chain(header, body).collect::<Vec<_>>();
    let [count, depth] = [columns.len().get(), lines.len()];
    (depth > 0).ok_or_else(|| nev![AnnotationError::Degenerate])?;
    let mut free = vec![0_usize; count];
    let slots = lines
        .iter()
        .enumerate()
        .flat_map(|(row, cells)| {
            let closing = if row < header.len() { header.len() } else { depth };
            let mut cursor = 0_usize;
            cells
                .iter()
                .map(|cell| {
                    let column = (cursor..count).find(|&index| free.get(index).is_some_and(|&from| from <= row)).unwrap_or(count);
                    let end = column.saturating_add(NonZero::<usize>::from(cell.columns).get());
                    let bottom = row.saturating_add(NonZero::<usize>::from(cell.rows).get()).min(closing);
                    cursor = end;
                    let Some((&lead, spanned)) = columns.get(column).zip(free.get_mut(column..end).filter(|spanned| spanned.iter().all(|&from| from <= row))) else {
                        return Err(AnnotationError::Cell { row, column });
                    };
                    spanned.fill(bottom);
                    Ok((Range::from(row..bottom), Range::from(column..end), cell, lead))
                })
                .collect::<Vec<_>>()
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    let mut lettered = |cell: &Cell, max_advance: Option<Width>, anchor: Anchor| {
        labelled(typesetter, style, &Label { content: Arc::clone(&cell.content), max_advance, .. }, Point::ORIGIN, Vec2::new(1.0, 0.0), anchor, Baseline::Top)
    };
    let open = |index: usize| columns.get(index).is_some_and(|column| column.width.is_none());
    let measured = slots.iter().map(|&(_, span, cell, Column { width: _, anchor, vertical: _ })| span.into_iter().any(open).then(|| lettered(cell, None, anchor))).collect::<Vec<_>>();
    let widths = iter::zip(&slots, &measured)
        .filter_map(|(&(_, span, ..), set)| set.as_ref()?.as_ref().ok().map(|(_, mask)| (span, mask.bounding_box().width())))
        .sorted_by_key(|&(span, _)| span.end.strict_sub(span.start))
        .fold(columns.iter().map(|column| column.width.map_or(gap * 2.0, Width::into_inner)).collect::<Vec<_>>(), |sizes, (span, need)| spread(sizes, span, open, need));
    let sets = iter::zip(&slots, measured)
        .map(|(&(_, span, cell, Column { width: _, anchor, vertical: _ }), set)| {
            set.unwrap_or_else(|| {
                Width::try_new(gap.mul_add(-2.0, widths.get(span).map_or_default(|spanned| spanned.iter().sum::<f64>())))
                    .map_err(|(WidthError::FiniteViolated | WidthError::GreaterOrEqualViolated)| nev![AnnotationError::Degenerate])
                    .and_then(|advance| lettered(cell, Some(advance), anchor))
            })
            .map_or_else(Validated::Fail, |(elements, mask)| Validated::Good((elements, mask.bounding_box().height())))
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    let heights = iter::zip(&slots, &sets)
        .map(|(&(rows, ..), &(_, height))| (rows, height))
        .sorted_by_key(|&(rows, _)| rows.end.strict_sub(rows.start))
        .fold(vec![gap * 2.0; depth], |sizes, (rows, need)| spread(sizes, rows, |_| true, need));
    let edges = |sizes: &[f64]| {
        iter::once(0.0)
            .chain(sizes.iter().scan(0.0, |total: &mut f64, size| {
                *total += size;
                Some(*total)
            }))
            .collect::<Vec<_>>()
    };
    let [across, down] = [&widths, &heights].map(|sizes| edges(sizes));
    let [width, height] = [&widths, &heights].map(|sizes| sizes.iter().sum::<f64>());
    let lettering = iter::zip(&slots, sets)
        .map(|(&(rows, span, _, Column { width: _, anchor, vertical }), (elements, extent))| {
            let Some(((&left, &right), (&top, &bottom))) = across.get(span.start).zip(across.get(span.end)).zip(down.get(rows.start).zip(down.get(rows.end))) else {
                return Validated::Fail(nev![AnnotationError::Cell { row: rows.start, column: span.start }]);
            };
            let x = match anchor {
                Anchor::Start => left + gap,
                Anchor::Middle => left.midpoint(right),
                Anchor::End => right - gap,
            };
            let slack = match vertical {
                Anchor::Start => 0.0,
                Anchor::Middle => 0.5,
                Anchor::End => 1.0,
            };
            Placement::try_new(frame * Affine::translate((x, (bottom - top - extent).mul_add(slack, top + gap))))
                .map_err(|error| nev![error])
                .and_then(|shift| transform(elements, shift))
                .map_or_else(|errors| Validated::Fail(errors.into_nonempty_iter().map(AnnotationError::from).collect()), Validated::Good)
        })
        .collect::<Validated<Vec<_>, _>>();
    let grid = slots.as_slice();
    let crossed = |boundary: usize, span: Range<usize>| span.start < boundary && boundary < span.end;
    let ruled = |boundary: usize, y: f64| {
        across
            .array_windows()
            .enumerate()
            .filter(move |&(column, _)| !grid.iter().any(|&(rows, span, ..)| crossed(boundary, rows) && span.contains(&column)))
            .map(move |(_, &[left, right])| PathSeg::Line(Line::new((left, y), (right, y))))
    };
    let split = header.len();
    let [verticals, horizontals] = [&across, &down].map(|positions| positions.iter().copied().enumerate().skip(1).dropping_back(1).collect::<Vec<_>>());
    let segments = |rule: Rule| -> Vec<PathSeg> {
        match rule {
            Rule::Frame => Rect::new(0.0, 0.0, width, height).path_segments(tolerance).collect(),
            Rule::Header => horizontals.iter().filter(|&&(boundary, _)| boundary == split).flat_map(|&(boundary, y)| ruled(boundary, y)).collect(),
            Rule::Rows => horizontals.iter().filter(|&&(boundary, _)| boundary != split || rules[Rule::Header].is_none()).flat_map(|&(boundary, y)| ruled(boundary, y)).collect(),
            Rule::Columns => verticals
                .iter()
                .flat_map(|&(boundary, x)| {
                    down.array_windows()
                        .enumerate()
                        .filter(move |&(row, _)| !grid.iter().any(|&(rows, span, ..)| crossed(boundary, span) && rows.contains(&row)))
                        .map(move |(_, &[top, bottom])| PathSeg::Line(Line::new((x, top), (x, bottom))))
                })
                .collect(),
        }
    };
    let strokes = rules
        .iter()
        .filter_map(|(rule, pen)| pen.as_ref().map(|drawn| (segments(rule), drawn)))
        .filter(|(pieces, _)| !pieces.is_empty())
        .map(|(pieces, pen)| stroked(frame * cleanup::chain(pieces, style.tolerance).into_iter().flatten().collect::<BezPath>(), pen, None))
        .collect::<Validated<Vec<_>, _>>();
    let mask = Region::new(&frame.transform_rect_bbox(Rect::new(0.0, 0.0, width, height)), FillRule::NonZero, style.tolerance).map_err(AnnotationError::from);
    strokes.map3(lettering, Validated::from(mask), |rulings, glyphs, region| Graphics { elements: iter::chain(rulings, glyphs.into_iter().flatten()).collect(), mask: region }).ok()
}
