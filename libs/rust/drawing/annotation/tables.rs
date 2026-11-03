use core::{iter, num::NonZero, range::Range};
use std::{borrow::Cow, collections::HashSet};

use enum_map::EnumMap;
use itertools::Itertools as _;
use kurbo::{Affine, BezPath, Line, PathSeg, Point, Rect, Shape as _, Vec2};
use linesweeper::FillRule;
use nonempty_collections::NEVec;
use rasm_linework::{Anchor, Baseline, Pen, Placement, Region, Width, WidthError, cleanup};
use validated::Validated;

use crate::{
    AnnotationError, Content, Geometry, Graphics, Subject, bounds, drawn, edges, fraction, joined,
    legends::{self, Swatch},
    stroked,
    text::{Label, Typesetter},
    transformed, validated, within,
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, PartialOrd, Ord, enum_map::Enum)]
pub enum Rule {
    Border,
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

#[derive(Debug, Clone, PartialEq)]
pub struct Cell {
    pub content: Content,
    pub columns: NonZero<u16> = NonZero::<u16>::MIN,
    pub rows: NonZero<u16> = NonZero::<u16>::MIN,
    pub anchor: Option<Anchor> = None,
    pub vertical: Option<Anchor> = None,
    pub fill: Vec<Swatch> = Vec::new(),
}

#[derive(Debug, Clone, PartialEq)]
pub struct Table {
    pub at: Point,
    pub columns: NEVec<Column>,
    pub rows: NEVec<Vec<Cell>>,
    pub header: usize = 0,
    pub rules: EnumMap<Rule, Option<Pen>>,
    pub height: Option<Width> = None,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn draw(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, table: &Table) -> Result<Graphics, NEVec<AnnotationError>> {
    fn spread(sizes: Vec<f64>, span: Range<usize>, open: impl Fn(usize) -> bool, need: f64) -> Vec<f64> {
        let grown = |index: usize| span.contains(&index) && open(index);
        let excess = need - sizes.get(span).map_or_default(|spanned| spanned.iter().sum::<f64>());
        let share = excess.max(0.0) / sizes.iter().enumerate().filter(|&(index, _)| grown(index)).map(|_| 1.0).sum::<f64>();
        sizes.into_iter().enumerate().map(|(index, size)| if grown(index) { size + share } else { size }).collect()
    }

    fn merged<I>(cells: I) -> HashSet<(usize, usize)>
    where I: IntoIterator<Item = (Range<usize>, Range<usize>)> {
        cells.into_iter().flat_map(|(crossing, along)| (crossing.start.saturating_add(1)..crossing.end).flat_map(move |boundary| along.into_iter().map(move |index| (boundary, index)))).collect()
    }

    let Table { at, columns, rows: lines, header, rules, height } = table;
    let gap = style.gap.into_inner();
    let accuracy = style.tolerance.into_inner();
    let origin = *placement * *at;
    let depth = lines.len().get();
    let header = (*header).min(depth);
    let placed = {
        let count = columns.len().get();
        lines
            .iter()
            .enumerate()
            .flat_map(|(row, line)| line.iter().map(move |cell| (row, cell)))
            .scan((vec![0_usize; count], None::<(usize, usize)>), |(free, last), (row, cell)| {
                let cursor = last.filter(|&(previous, _)| previous == row).map_or_default(|(_, end)| end);
                let column = (cursor..count).find(|&index| free.get(index).is_some_and(|&from| from <= row)).unwrap_or(count);
                let end = column.saturating_add(NonZero::<usize>::from(cell.columns).get());
                let bottom = row.saturating_add(NonZero::<usize>::from(cell.rows).get()).min(if row < header { header } else { depth });
                *last = Some((row, end));
                Some(match (columns.get(column), free.get_mut(column..end).filter(|spanned| spanned.iter().all(|&from| from <= row))) {
                    (Some(&lead), Some(spanned)) => {
                        spanned.fill(bottom);
                        Validated::Good((Range::from(row..bottom), Range::from(column..end), cell, lead))
                    }
                    (None, None | Some(_)) | (Some(_), None) => within(&Subject::Cell { row, column }, Err(AnnotationError::Geometry(Geometry::Overlap).into())),
                })
            })
            .collect::<Validated<Vec<_>, _>>()
            .ok()?
    };
    let open = |index: usize| columns.get(index).is_some_and(|column| column.width.is_none());
    let mut set = |cell: &Cell, lead: &Column, max_advance: Option<Width>| {
        let content = match &cell.content {
            Content::Label(label) => Cow::Owned(Content::Label(Label { max_advance: max_advance.or(label.max_advance), ..label.clone() })),
            other @ (Content::Symbol(_) | Content::Control(_)) => Cow::Borrowed(other),
        };
        drawn(typesetter, style, placement, &content, Point::ORIGIN, cell.anchor.unwrap_or(lead.anchor), Baseline::Top)
    };
    let boxed = |cell: &Cell, graphics: &Graphics| match cell.content {
        Content::Label(_) => bounds(graphics, Affine::IDENTITY),
        Content::Symbol(_) | Content::Control(_) => bounds(graphics, Affine::IDENTITY).inflate(gap, gap),
    };
    let measured = placed.iter().map(|&(_, span, cell, lead)| span.into_iter().any(open).then(|| set(cell, &lead, None))).collect::<Vec<_>>();
    let widths = iter::zip(&placed, &measured)
        .filter_map(|(&(_, span, cell, _), sized)| sized.as_ref()?.as_ref().ok().map(|graphics| (span, boxed(cell, graphics).width())))
        .sorted_by_key(|&(span, _)| span.end.strict_sub(span.start))
        .fold(columns.iter().map(|column| column.width.map_or(gap * 2.0, Width::into_inner)).collect::<Vec<_>>(), |sizes, (span, need)| spread(sizes, span, open, need));
    let sets = iter::zip(&placed, measured)
        .map(|(&(rows, span, cell, lead), sized)| {
            within(
                &Subject::Cell { row: rows.start, column: span.start },
                sized.unwrap_or_else(|| {
                    Width::try_new(gap.mul_add(-2.0, widths.get(span).map_or_default(|spanned| spanned.iter().sum::<f64>())))
                        .map_err(|(WidthError::FiniteViolated | WidthError::GreaterOrEqualViolated)| AnnotationError::Geometry(Geometry::Gap).into())
                        .and_then(|advance| set(cell, &lead, Some(advance)))
                }),
            )
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    let heights = iter::zip(&placed, &sets)
        .map(|(&(rows, _, cell, _), graphics)| (rows, boxed(cell, graphics).height()))
        .sorted_by_key(|&(rows, _)| rows.end.strict_sub(rows.start))
        .fold(vec![gap * 2.0; depth], |sizes, (rows, need)| spread(sizes, rows, |_| true, need));
    let across = edges(widths.iter().copied()).collect::<Vec<_>>();
    let width = across.last().copied().unwrap_or_default();
    let segments = {
        let tops = edges(heights.iter().copied()).collect::<Vec<_>>();
        let total = |from: usize, to: usize| tops.get(to).zip(tops.get(from)).map_or_default(|(bottom, top)| bottom - top);
        let room = height.map(|bound| bound.into_inner() - total(0, header));
        let spanned = placed.iter().flat_map(|&(rows, ..)| rows.start.saturating_add(1)..rows.end).collect::<HashSet<_>>();
        let boundaries = (header.saturating_add(1)..=depth).filter(|boundary| *boundary == depth || !spanned.contains(boundary)).collect::<Vec<_>>();
        let end = |start: usize| {
            let mut after = boundaries.iter().copied().skip_while(|&boundary| boundary <= start);
            after.clone().take_while(|&boundary| room.is_none_or(|room| total(start, boundary) <= room)).last().or_else(|| after.next()).unwrap_or(depth)
        };
        iter::successors(Some(Range::from(header..end(header))), |page| (page.end < depth).then(|| Range::from(page.end..end(page.end)))).collect::<Vec<_>>()
    };
    let mut segment = |offset: f64, body: Range<usize>| -> Result<Graphics, NEVec<AnnotationError>> {
        let kept = (0..header).chain(body).collect::<Vec<_>>();
        let down = edges(kept.iter().map(|&row| heights.get(row).copied().unwrap_or_default())).collect::<Vec<_>>();
        let basis = Affine::translate((origin.x + offset, origin.y));
        let local = |row: usize| if row < header { Some(row) } else { body.contains(&row).then(|| header.strict_add(row.strict_sub(body.start))) };
        let grid = iter::zip(&placed, &sets)
            .filter_map(|(&(rows, span, cell, lead), graphics)| local(rows.start).map(|start| (Range::from(start..start.saturating_add(rows.end.strict_sub(rows.start))), span, cell, lead, graphics)))
            .collect::<Vec<_>>();
        let row_breaks = &merged(grid.iter().map(|&(rows, span, ..)| (rows, span)));
        let column_breaks = &merged(grid.iter().map(|&(rows, span, ..)| (span, rows)));
        let contents = grid
            .iter()
            .map(|&(rows, span, cell, lead, graphics)| {
                let Some(((&left, &right), (&top, &bottom))) = across.get(span.start).zip(across.get(span.end)).zip(down.get(rows.start).zip(down.get(rows.end))) else {
                    return within(&Subject::Cell { row: rows.start, column: span.start }, Err(AnnotationError::Geometry(Geometry::Overlap).into()));
                };
                let extent = boxed(cell, graphics);
                let [horizontal, vertical] = [cell.anchor.unwrap_or(lead.anchor), cell.vertical.unwrap_or(lead.vertical)].map(fraction);
                let shift = Vec2::new((right - left - extent.width()).mul_add(horizontal, left - extent.x0), (bottom - top - extent.height()).mul_add(vertical, top - extent.y0));
                let rect = basis.transform_rect_bbox(Rect::new(left, top, right, bottom));
                validated(joined([], cell.fill.iter().map(|sample| legends::swatched(typesetter, style, placement, sample, rect)).chain(iter::once(transformed(graphics.clone(), basis * Affine::translate(shift))))))
            })
            .collect::<Validated<Vec<_>, _>>();
        let ruled = |boundary: usize, y: f64| {
            across
                .array_windows()
                .enumerate()
                .filter(move |&(column, _)| !row_breaks.contains(&(boundary, column)))
                .map(move |(_, &[left, right])| PathSeg::Line(Line::new((left, y), (right, y))))
        };
        let reach = down.last().copied().unwrap_or_default();
        let [verticals, horizontals] = [&across, &down].map(|positions| positions.iter().copied().enumerate().skip(1).dropping_back(1).collect::<Vec<_>>());
        let pieces = |rule: Rule| -> Vec<PathSeg> {
            match rule {
                Rule::Border => Rect::new(0.0, 0.0, width, reach).path_segments(accuracy).collect(),
                Rule::Header => horizontals.iter().filter(|&&(boundary, _)| boundary == header).flat_map(|&(boundary, y)| ruled(boundary, y)).collect(),
                Rule::Rows => horizontals.iter().filter(|&&(boundary, _)| boundary != header || rules[Rule::Header].is_none()).flat_map(|&(boundary, y)| ruled(boundary, y)).collect(),
                Rule::Columns => verticals
                    .iter()
                    .flat_map(|&(boundary, x)| {
                        down.array_windows()
                            .enumerate()
                            .filter(move |&(row, _)| !column_breaks.contains(&(boundary, row)))
                            .map(move |(_, &[top, bottom])| PathSeg::Line(Line::new((x, top), (x, bottom))))
                    })
                    .collect(),
            }
        };
        let strokes = rules
            .iter()
            .filter_map(|(rule, pen)| pen.as_ref().map(|drawn| (pieces(rule), drawn)))
            .filter(|(pieces, _)| !pieces.is_empty())
            .map(|(pieces, pen)| stroked(basis * cleanup::chain(pieces, style.tolerance).into_iter().flatten().collect::<BezPath>(), pen, None))
            .collect::<Validated<Vec<_>, _>>();
        let mask = Region::new(&basis.transform_rect_bbox(Rect::new(0.0, 0.0, width, reach)), FillRule::NonZero, style.tolerance).map_err(AnnotationError::from);
        strokes
            .map3(contents, Validated::from(mask), |rulings, cells, region| iter::once(Graphics { elements: rulings, masks: vec![region] }).chain(cells).collect())
            .ok()
    };
    let offsets = edges(iter::repeat_n(width + gap, segments.len())).collect::<Vec<_>>();
    iter::zip(offsets, segments).map(|(offset, body)| validated(segment(offset, body))).collect::<Validated<Vec<_>, _>>().ok().map(Graphics::from_iter)
}
