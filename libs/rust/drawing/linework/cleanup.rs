use std::{borrow::Cow, cell::Cell, cmp, collections::BTreeMap, iter, ops::RangeInclusive};

use geo::{BallTree, BooleanOps as _, Distance as _, Euclidean, InterpolatableLine as _, Intersections, Length as _, LineLocatePoint as _, PointWithData, sweep::Cross};
use geo_types::{Coord, Line, LineString, MultiLineString, Point};
use itertools::Itertools as _;
use kurbo::{BezPath, ParamCurve as _, PathEl, PathSeg, Shape as _, Vec2};
use linesweeper::BinaryOp;
use nonempty_collections::{NEVec, nev};
use rayon::iter::{IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::{Element, LineworkError, Path, Region, Side, Tolerance, regions};

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
    let (op, invert) = match keep {
        Side::Inside => (BinaryOp::Intersection, false),
        Side::Outside => (BinaryOp::Difference, true),
    };
    let bounds = region.bounding_box();
    let polygons = crate::polygons(region, tolerance);
    let clipped = |subpath: &[PathEl]| -> Option<Vec<PathEl>> {
        let lines = MultiLineString::new(crate::flattened(subpath.iter().copied(), tolerance));
        let remaining = polygons.clip(&lines, invert);
        match (remaining.0.is_empty(), Euclidean.length(&lines) - Euclidean.length(&remaining) <= tolerance.into_inner()) {
            (true, _) => Some(Vec::new()),
            (false, true) => None,
            (false, false) => Some(rebuilt(remaining.iter().flat_map(LineString::lines), tolerance).into_elements()),
        }
    };
    let kept = |subpath: &[PathEl]| match (bounds.overlaps(subpath.bounding_box()), invert) {
        (true, _) => clipped(subpath),
        (false, true) => None,
        (false, false) => Some(Vec::new()),
    };
    let stroke = |path: Path| -> Result<Option<Path>, LineworkError> {
        let reworked = {
            let parts = path.subpaths().map(|subpath| kept(subpath).map_or(Cow::Borrowed(subpath), Cow::Owned)).collect::<Vec<_>>();
            (!parts.iter().all(|part| matches!(part, Cow::Borrowed(_)))).then(|| parts.iter().flat_map(|part| part.iter().copied()).collect::<BezPath>())
        };
        Ok(Some(reworked.map(Path::try_new).transpose()?.unwrap_or(path)).filter(|remaining| !remaining.is_empty()))
    };
    let area = |shape: Region| -> Result<Option<Region>, LineworkError> {
        match (shape.is_empty(), bounds.overlaps(shape.bounding_box()), invert) {
            (true, _, _) | (false, false, false) => Ok(None),
            (false, false, true) => Ok(Some(shape)),
            (false, true, _) => regions::boolean(&shape, [region], op, tolerance).map(|remaining| Some(remaining).filter(|remaining| !remaining.is_empty())),
        }
    };
    let maskable = |element: &Element| match element {
        Element::Text { .. } | Element::Glyphs { .. } | Element::Placed { .. } => true,
        Element::Stroke { .. } | Element::Fill { .. } | Element::Hatch { .. } | Element::Clip { .. } => false,
    };
    let painted = |grouped: Vec<Element>| Some(Ok(Element::Clip { region: region.clone(), keep, elements: grouped }));
    let outcome = |(index, run): (usize, Vec<Element>)| {
        let indexed = |source: LineworkError| LineworkError::Element { index, errors: nev![source] };
        match <[Element; 1]>::try_from(run) {
            Ok([Element::Stroke { path, pen, linetype }]) => stroke(path).map(|outcome| outcome.map(|remaining| Element::Stroke { path: remaining, pen, linetype })).map_err(indexed).transpose(),
            Ok([Element::Fill { region: shape, color, opacity }]) =>
                area(shape).map(|outcome| outcome.map(|remaining| Element::Fill { region: remaining, color, opacity })).map_err(indexed).transpose(),
            Ok([Element::Hatch { region: shape, pattern, placement, pen }]) =>
                area(shape).map(|outcome| outcome.map(|remaining| Element::Hatch { region: remaining, pattern, placement, pen })).map_err(indexed).transpose(),
            Ok([Element::Clip { region: mask, keep: side, elements: grouped }]) => clip(grouped, region, keep, tolerance)
                .map(|inner| (!inner.is_empty()).then_some(Element::Clip { region: mask, keep: side, elements: inner }))
                .map_err(|errors| LineworkError::Element { index, errors })
                .transpose(),
            Ok([element @ (Element::Text { .. } | Element::Glyphs { .. } | Element::Placed { .. })]) => painted(vec![element]),
            Err(run) => painted(run),
        }
    };
    match (region.is_empty(), keep) {
        (true, Side::Inside) => Ok(Vec::new()),
        (true, Side::Outside) => Ok(elements),
        (false, Side::Inside | Side::Outside) => elements
            .into_iter()
            .enumerate()
            .map(|(index, element)| (index, vec![element]))
            .coalesce(|(index, mut left), (next, right)| {
                if iter::chain(&left, &right).all(maskable) {
                    left.extend(right);
                    Ok((index, left))
                } else {
                    Err(((index, left), (next, right)))
                }
            })
            .collect::<Vec<_>>()
            .into_par_iter()
            .filter_map(outcome)
            .collect::<Vec<_>>()
            .into_iter()
            .collect::<Validated<Vec<_>, _>>()
            .ok(),
    }
}

pub fn coincident<K>(elements: Vec<Element>, priority: impl Fn(&Element) -> K + Sync, tolerance: Tolerance) -> Result<Vec<Element>, NEVec<LineworkError>>
where K: Ord + Send {
    fn strokes(elements: &[Element]) -> Vec<(&Element, &Path)> {
        elements
            .iter()
            .flat_map(|element| match element {
                Element::Stroke { path, pen: _, linetype: _ } => vec![(element, path)],
                Element::Clip { region: _, keep: _, elements: grouped } => strokes(grouped),
                Element::Fill { region: _, color: _, opacity: _ }
                | Element::Hatch { region: _, pattern: _, placement: _, pen: _ }
                | Element::Text { text: _, color: _, opacity: _ }
                | Element::Glyphs { run: _, color: _, opacity: _ }
                | Element::Placed { content: _, placement: _, opacity: _ } => Vec::new(),
            })
            .collect()
    }
    fn reassembled<F>(elements: Vec<Element>, first: usize, stroke: &F) -> Result<Vec<Element>, NEVec<LineworkError>>
    where F: Fn(usize, Path) -> Result<Option<Path>, LineworkError> + Sync {
        elements
            .into_iter()
            .scan(first, |next, element| {
                let start = *next;
                *next = start.strict_add(match &element {
                    Element::Stroke { path: _, pen: _, linetype: _ } => 1,
                    Element::Clip { region: _, keep: _, elements: grouped } => strokes(grouped).len(),
                    Element::Fill { .. } | Element::Hatch { .. } | Element::Text { .. } | Element::Glyphs { .. } | Element::Placed { .. } => 0,
                });
                Some((start, element))
            })
            .enumerate()
            .collect::<Vec<_>>()
            .into_par_iter()
            .filter_map(|(index, (start, element))| {
                match element {
                    Element::Stroke { path, pen, linetype } =>
                        stroke(start, path).map(|outcome| outcome.map(|remaining| Element::Stroke { path: remaining, pen, linetype })).map_err(|source| nev![source]),
                    Element::Clip { region, keep, elements: grouped } =>
                        reassembled(grouped, start, stroke).map(|inner| (!inner.is_empty()).then_some(Element::Clip { region, keep, elements: inner })),
                    element @ (Element::Fill { .. } | Element::Hatch { .. } | Element::Text { .. } | Element::Glyphs { .. } | Element::Placed { .. }) => Ok(Some(element)),
                }
                .map_err(|errors| LineworkError::Element { index, errors })
                .transpose()
            })
            .collect::<Vec<_>>()
            .into_iter()
            .collect::<Validated<Vec<_>, _>>()
            .ok()
    }
    let grid = tolerance.into_inner();
    let drawn = strokes(&elements);
    let lines = drawn
        .par_iter()
        .map(|&(_, path)| path.subpaths().map(|subpath| crate::flattened(subpath.iter().copied(), tolerance).iter().flat_map(LineString::lines).collect::<Vec<_>>()).collect::<Vec<_>>())
        .collect::<Vec<_>>();
    let ranks = drawn.par_iter().map(|&(element, _)| priority(element)).collect::<Vec<_>>();
    let span = |upper: &Line<f64>, lower: &Line<f64>| {
        lower.line_locate_point(&upper.start_point()).zip(lower.line_locate_point(&upper.end_point())).map(|(start, end)| cmp::minmax_by(start, end, f64::total_cmp)).filter(|&[start, end]| {
            (end - start) * Euclidean.length(lower) > grid && [start, end].into_iter().all(|ratio| Euclidean.distance(&lower.point_at_ratio_from_start(&Euclidean, ratio), upper) <= grid)
        })
    };
    let removed = lines
        .iter()
        .enumerate()
        .flat_map(|(stroke, subpaths)| {
            subpaths.iter().enumerate().flat_map(move |(subpath, segments)| {
                segments.iter().enumerate().filter(|(_, original)| original.start != original.end).flat_map(move |(segment, &original)| {
                    let key = [stroke, subpath, segment];
                    let normal = {
                        let delta = original.delta();
                        Coord { x: -delta.y, y: delta.x } * (grid / Euclidean.length(&original))
                    };
                    iter::chain(iter::once(Piece { line: original, original, key }), [original.start, original.end].map(|end| Piece { line: Line::new(end - normal, end + normal), original, key }))
                })
            })
        })
        .collect::<Intersections<Piece>>()
        .filter(|(left, right, _)| left.key != right.key && (left.line == left.original || right.line == right.original))
        .map(|(left, right, _)| cmp::minmax_by(left, right, |left, right| ranks.get(left.key[0]).cmp(&ranks.get(right.key[0])).then(right.key.cmp(&left.key))))
        .unique_by(|[lower, upper]| [lower.key, upper.key])
        .filter_map(|[lower, upper]| span(&upper.original, &lower.original).map(|span| (lower.key, span)))
        .fold(BTreeMap::<[usize; 3], Vec<[f64; 2]>>::new(), |mut removed, (key, span)| {
            let spans = removed.entry(key).or_default();
            spans.insert(spans.partition_point(|probe| probe[0].total_cmp(&span[0]).is_lt()), span);
            removed
        });
    let touched = |stroke: usize, subpaths: RangeInclusive<usize>| removed.range([stroke, *subpaths.start(), 0]..=[stroke, *subpaths.end(), usize::MAX]).next().is_some();
    let kept = |stroke: usize, subpath: usize, segments: &[Line<f64>]| {
        rebuilt(
            segments.iter().enumerate().flat_map(|(segment, line)| {
                iter::chain(removed.get(&[stroke, subpath, segment]).into_iter().flatten().copied(), iter::once([1.0_f64, 1.0_f64]))
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
    let reworked = |stroke: usize, path: Path| match lines.get(stroke).filter(|_| touched(stroke, usize::MIN..=usize::MAX)) {
        None => Ok(Some(path)),
        Some(subpaths) => Path::try_new(
            path.subpaths()
                .zip(subpaths)
                .enumerate()
                .flat_map(|(subpath, (original, segments))| if touched(stroke, subpath..=subpath) { kept(stroke, subpath, segments) } else { original.to_vec() })
                .collect::<BezPath>(),
        )
        .map(|remaining| Some(remaining).filter(|remaining| !remaining.is_empty()))
        .map_err(LineworkError::from),
    };
    reassembled(elements, 0, &reworked)
}

#[must_use]
pub fn chain(segments: impl IntoIterator<Item = PathSeg>, tolerance: Tolerance) -> Vec<BezPath> {
    let segments = segments.into_iter().filter_map(|segment| crate::tangents(&segment).map(|(start, end)| (segment, ((start, end), (-end, -start))))).collect::<Vec<_>>();
    let radius = tolerance.into_inner();
    let vertex = |point: kurbo::Point| Point::from(crate::coord(point));
    let tree = BallTree::new(segments.iter().enumerate().flat_map(|(index, &(segment, (forward, backward)))| {
        [(segment, forward), (segment.reverse(), backward)].map(|(oriented, tangents)| PointWithData::new(vertex(oriented.start()), (index, oriented, tangents)))
    }));
    let free = vec![Cell::new(true); segments.len()];
    let take = |index: usize| free.get(index).is_some_and(|slot| slot.replace(false));
    let next = |(last, (_, incoming)): (PathSeg, (Vec2, Vec2))| {
        tree.within_radius(&vertex(last.end()), radius)
            .into_iter()
            .filter(|hit| free.get(hit.data.0).is_some_and(Cell::get))
            .map(|hit| (crate::deflection(incoming, hit.data.2.0), hit))
            .min_by(|(left, near), (right, far)| left.total_cmp(right).then(near.distance.total_cmp(&far.distance)))
            .filter(|(_, hit)| take(hit.data.0))
            .map(|(_, hit)| (hit.data.1, hit.data.2))
    };
    segments
        .iter()
        .enumerate()
        .filter(|&(seed, _)| take(seed))
        .map(|(_, &(first, (ahead, behind)))| {
            let forward = iter::successors(Some((first, ahead)), |&link| next(link)).map(|(link, _)| link).collect::<Vec<_>>();
            let backward = iter::successors(next((first.reverse(), behind)), |&link| next(link)).map(|(link, _)| link.reverse()).collect::<Vec<_>>();
            let run = backward.iter().rev().chain(&forward).copied().collect::<Vec<_>>();
            let origin = run.first().map_or_else(|| first.start(), PathSeg::start);
            let closed = run.len() > 1 && run.last().is_some_and(|tail| tail.end().distance(origin) <= radius);
            iter::once(PathEl::MoveTo(origin)).chain(run.iter().map(PathSeg::as_path_el)).chain(closed.then_some(PathEl::ClosePath)).collect::<BezPath>()
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
