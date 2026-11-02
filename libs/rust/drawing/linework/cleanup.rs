use std::{cell::Cell, cmp, collections::BTreeMap, iter, ops::RangeInclusive};

use geo::{BallTree, BooleanOps as _, Distance as _, Euclidean, InterpolatableLine as _, Intersections, Length as _, LineLocatePoint as _, MapCoords as _, PointWithData, sweep::Cross};
use geo_types::{Coord, Line, LineString, MultiLineString, Point};
use kurbo::{BezPath, CubicBez, ParamCurve as _, PathEl, PathSeg, Rect, Shape as _};
use linesweeper::{BinaryOp, Error};
use nonempty_collections::{NEVec, nev};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::{Element, LineworkError, Region, Side, Tolerance, regions};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy)]
struct Piece {
    line: Line<f64>,
    original: Line<f64>,
    key: [usize; 3],
}

impl Cross for Piece {
    type Scalar = f64;

    fn line(&self) -> Line<f64> { self.line }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn clip(elements: Vec<Element>, region: &Region, keep: Side, tolerance: Tolerance) -> Result<Vec<Element>, NEVec<LineworkError>> {
    region.iter().try_for_each(|contour| crate::finite(&contour.path)).map_err(|source| nev![LineworkError::Sweep(source)])?;
    let (op, invert) = match keep {
        Side::Inside => (BinaryOp::Intersection, false),
        Side::Outside => (BinaryOp::Difference, true),
    };
    let bounds = crate::extent(region);
    let overlaps = |rect: Rect| bounds.is_some_and(|bounds| bounds.overlaps(rect));
    let polygons = regions::polygons(region, tolerance);
    let clipped = |subpath: &[PathEl]| {
        let lines = MultiLineString::new(crate::flattened(&BezPath::from_vec(subpath.to_vec()), tolerance));
        let remaining = polygons.clip(&lines, invert);
        match (remaining.0.is_empty(), Euclidean.length(&lines) - Euclidean.length(&remaining) <= tolerance.into_inner()) {
            (true, _) => Vec::new(),
            (false, true) => subpath.to_vec(),
            (false, false) => rebuilt(remaining.iter().flat_map(LineString::lines), tolerance).into_elements(),
        }
    };
    let kept = |subpath: &[PathEl]| match (overlaps(subpath.bounding_box()), invert) {
        (true, _) => clipped(subpath),
        (false, true) => subpath.to_vec(),
        (false, false) => Vec::new(),
    };
    let stroke = |path: &BezPath| crate::finite(path).map(|()| Some(path.subpaths().flat_map(kept).collect::<BezPath>()).filter(|remaining| !remaining.is_empty()));
    let area = |shape: Region| -> Result<Option<Region>, Error> {
        shape.iter().try_for_each(|contour| crate::finite(&contour.path))?;
        match (crate::extent(&shape).map(overlaps), invert) {
            (None, _) | (Some(false), false) => Ok(None),
            (Some(false), true) => Ok(Some(shape)),
            (Some(true), _) => regions::boolean(&shape, [region], op, tolerance)
                .map(|remaining| Some(remaining).filter(|remaining| !remaining.is_empty()))
                .map_err(|(LineworkError::Sweep(source) | LineworkError::Element { index: _, source })| source),
        }
    };
    elements
        .into_par_iter()
        .enumerate()
        .filter_map(|(index, element)| {
            let outcome = match element {
                Element::Stroke { path, pen, linetype } => stroke(&path).map(|outcome| outcome.map(|remaining| Element::Stroke { path: remaining, pen, linetype })).transpose(),
                Element::Fill { region: shape, color } => area(shape).map(|outcome| outcome.map(|remaining| Element::Fill { region: remaining, color })).transpose(),
                Element::Hatch { region: shape, pattern, placement, pen } =>
                    area(shape).map(|outcome| outcome.map(|remaining| Element::Hatch { region: remaining, pattern, placement, pen })).transpose(),
                element @ (Element::Text { .. } | Element::Glyphs { .. } | Element::Placed { .. }) => Some(Ok(element)),
            };
            outcome.map(|item| item.map_err(|source| LineworkError::Element { index, source }))
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}

pub fn coincident(elements: Vec<Element>, tolerance: Tolerance) -> Result<Vec<Element>, NEVec<LineworkError>> {
    let grid = tolerance.into_inner();
    let snap = |Coord { x, y }: Coord<f64>| Coord { x: (x / grid).round().mul_add(grid, 0.0), y: (y / grid).round().mul_add(grid, 0.0) };
    let lines = elements
        .par_iter()
        .enumerate()
        .map(|(index, element)| match element {
            Element::Stroke { path, pen: _, linetype: _ } => crate::finite(path)
                .map(|()| path.subpaths().map(|subpath| crate::flattened(&BezPath::from_vec(subpath.to_vec()), tolerance).iter().flat_map(LineString::lines).collect()).collect())
                .map_err(|source| LineworkError::Element { index, source }),
            Element::Fill { region: _, color: _ }
            | Element::Hatch { region: _, pattern: _, placement: _, pen: _ }
            | Element::Text { text: _, color: _ }
            | Element::Glyphs { run: _, color: _ }
            | Element::Placed { content: _, placement: _ } => Ok(Vec::new()),
        })
        .collect::<Validated<Vec<Vec<Vec<Line<f64>>>>, _>>()
        .ok()?;
    let span = |upper: &Line<f64>, lower: &Line<f64>| {
        lower.line_locate_point(&upper.start_point()).zip(lower.line_locate_point(&upper.end_point())).map(|(start, end)| cmp::minmax_by(start, end, f64::total_cmp)).filter(|&[start, end]| {
            (end - start) * Euclidean.length(lower) > grid && [start, end].into_iter().all(|ratio| Euclidean.distance(&lower.point_at_ratio_from_start(&Euclidean, ratio), upper) <= grid)
        })
    };
    let removed = lines
        .iter()
        .enumerate()
        .flat_map(|(element, subpaths)| {
            subpaths
                .iter()
                .enumerate()
                .flat_map(move |(subpath, segments)| segments.iter().enumerate().map(move |(segment, &original)| Piece { line: original.map_coords(snap), original, key: [element, subpath, segment] }))
        })
        .filter(|piece| piece.line.start != piece.line.end)
        .collect::<Intersections<Piece>>()
        .filter_map(|(left, right, _)| {
            let [upper, lower] = cmp::minmax_by_key(left, right, |piece| piece.key);
            span(&upper.original, &lower.original).map(|span| (lower.key, span))
        })
        .fold(BTreeMap::<[usize; 3], Vec<[f64; 2]>>::new(), |mut removed, (key, span)| {
            let spans = removed.entry(key).or_default();
            spans.insert(spans.partition_point(|probe| probe[0].total_cmp(&span[0]).is_lt()), span);
            removed
        });
    let touched = |element: usize, subpaths: RangeInclusive<usize>| removed.range([element, *subpaths.start(), 0]..=[element, *subpaths.end(), usize::MAX]).next().is_some();
    let kept = |element: usize, subpath: usize, segments: &[Line<f64>]| {
        rebuilt(
            segments.iter().enumerate().flat_map(|(segment, line)| {
                iter::chain(removed.get(&[element, subpath, segment]).into_iter().flatten().copied(), iter::once([1.0_f64, 1.0_f64]))
                    .scan(0.0_f64, |reach, [start, end]| {
                        let gap = (*reach, start);
                        *reach = reach.max(end);
                        Some(gap)
                    })
                    .filter(|&(start, end)| start < end)
                    .map(move |(start, end)| Line::new(line.point_at_ratio_from_start(&Euclidean, start), line.point_at_ratio_from_start(&Euclidean, end)))
            }),
            tolerance,
        )
        .into_elements()
    };
    let reworked = |element: usize, path: &BezPath, subpaths: &[Vec<Line<f64>>]| {
        Some(
            path.subpaths()
                .zip(subpaths)
                .enumerate()
                .flat_map(|(subpath, (original, segments))| if touched(element, subpath..=subpath) { kept(element, subpath, segments) } else { original.to_vec() })
                .collect::<BezPath>(),
        )
        .filter(|remaining| !remaining.is_empty())
    };
    Ok(elements
        .into_par_iter()
        .zip(lines)
        .enumerate()
        .filter_map(|(element, (item, subpaths))| match item {
            Element::Stroke { path, pen, linetype } if touched(element, usize::MIN..=usize::MAX) => reworked(element, &path, &subpaths).map(|path| Element::Stroke { path, pen, linetype }),
            item @ (Element::Stroke { .. } | Element::Fill { .. } | Element::Hatch { .. } | Element::Text { .. } | Element::Glyphs { .. } | Element::Placed { .. }) => Some(item),
        })
        .collect())
}

#[must_use]
pub fn chain(segments: impl IntoIterator<Item = PathSeg>, tolerance: Tolerance) -> Vec<BezPath> {
    let segments = segments
        .into_iter()
        .filter(|segment| {
            let CubicBez { p0, p1, p2, p3 } = segment.to_cubic();
            [p1, p2, p3] != [p0; 3]
        })
        .collect::<Vec<_>>();
    let radius = tolerance.into_inner();
    let vertex = |point: kurbo::Point| Point::from(crate::coord(point));
    let tree = BallTree::new(segments.iter().enumerate().flat_map(|(index, &segment)| [segment, segment.reverse()].map(|oriented| PointWithData::new(vertex(oriented.start()), (index, oriented)))));
    let free = segments.iter().map(|_| Cell::new(true)).collect::<Vec<_>>();
    let take = |index: usize| free.get(index).is_some_and(|slot| slot.replace(false));
    let next = |point: kurbo::Point| {
        tree.within_radius(&vertex(point), radius)
            .into_iter()
            .filter(|hit| free.get(hit.data.0).is_some_and(Cell::get))
            .min_by(|left, right| left.distance.total_cmp(&right.distance))
            .filter(|hit| take(hit.data.0))
            .map(|hit| hit.data.1)
    };
    segments
        .iter()
        .enumerate()
        .filter(|&(seed, _)| take(seed))
        .map(|(_, &first)| {
            let forward = iter::successors(Some(first), |last| next(last.end())).collect::<Vec<_>>();
            let backward = iter::successors(next(first.start()), |last| next(last.end())).map(|link| link.reverse()).collect::<Vec<_>>();
            let run = backward.iter().rev().chain(&forward).copied().collect::<Vec<_>>();
            let start = run.first().map_or_else(|| first.start(), PathSeg::start);
            let closed = run.len() > 1 && run.last().is_some_and(|tail| tail.end().distance(start) <= radius);
            iter::once(PathEl::MoveTo(start)).chain(run.iter().map(PathSeg::as_path_el)).chain(closed.then_some(PathEl::ClosePath)).collect::<BezPath>()
        })
        .collect()
}

fn rebuilt(lines: impl IntoIterator<Item = Line<f64>>, tolerance: Tolerance) -> BezPath {
    chain(lines.into_iter().map(|Line { start, end }| PathSeg::Line(kurbo::Line::new(start.x_y(), end.x_y()))), tolerance)
        .into_iter()
        .filter(|path| path.perimeter(tolerance.into_inner()) > tolerance.into_inner())
        .flatten()
        .collect()
}
