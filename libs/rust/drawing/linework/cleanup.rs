use core::{cell::Cell, cmp, iter, mem, ops::RangeInclusive};
use std::{borrow::Cow, collections::BTreeMap};

use geo::{BallTree, BooleanOps as _, Distance as _, Euclidean, InterpolatableLine as _, Intersections, Length as _, LineLocatePoint as _, PointWithData, sweep::Cross};
use geo_types::{Coord, Line, LineString, MultiLineString, Point};
use itertools::{Either, Itertools as _};
use kurbo::{BezPath, ParamCurve as _, PathEl, PathSeg, Shape as _, Vec2};
use linesweeper::BinaryOp;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _, iter::once, nev};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _, walk_tree_prefix};
use stacksafe::StackSafe;
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

#[stacksafe::stacksafe]
pub fn clip(elements: impl Into<StackSafe<Vec<Element>>>, region: &Region, keep: Side, tolerance: Tolerance) -> Result<Vec<Element>, NEVec<LineworkError>> {
    let elements = elements.into().into_inner();
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
    let painted = |grouped: Vec<Element>| Some(Ok(Element::Clip { region: region.clone(), keep, elements: grouped.into() }));
    let outcome = |run: Either<(usize, Element), Vec<Element>>| match run {
        Either::Right(run) => painted(run),
        Either::Left((index, element)) => {
            let indexed = |source: LineworkError| LineworkError::Element { index, errors: nev![source.into()] };
            match element {
                Element::Stroke { path, pen, linetype } => stroke(path).map(|outcome| outcome.map(|remaining| Element::Stroke { path: remaining, pen, linetype })).map_err(indexed).transpose(),
                Element::Fill { region: shape, color, opacity } => area(shape).map(|outcome| outcome.map(|remaining| Element::Fill { region: remaining, color, opacity })).map_err(indexed).transpose(),
                Element::Hatch { region: shape, pattern, placement, pen } =>
                    area(shape).map(|outcome| outcome.map(|remaining| Element::Hatch { region: remaining, pattern, placement, pen })).map_err(indexed).transpose(),
                Element::Clip { region: mask, keep: side, elements: grouped } => clip(grouped, region, keep, tolerance)
                    .map(|inner| (!inner.is_empty()).then_some(Element::Clip { region: mask, keep: side, elements: inner.into() }))
                    .map_err(|errors| LineworkError::Element { index, errors: errors.into_nonempty_iter().map(StackSafe::new).collect() })
                    .transpose(),
                element @ (Element::Text { .. } | Element::Glyphs { .. } | Element::Placed { .. }) => painted(vec![element]),
            }
        }
    };
    match (region.is_empty(), keep) {
        (true, Side::Inside) => Ok(Vec::new()),
        (true, Side::Outside) => Ok(elements),
        (false, Side::Inside | Side::Outside) => elements
            .into_iter()
            .enumerate()
            .chunk_by(|(_, element)| maskable(element))
            .into_iter()
            .flat_map(|(mask, run)| if mask { Either::Left(iter::once(Either::Right(run.map(|(_, element)| element).collect()))) } else { Either::Right(run.map(Either::Left)) })
            .collect::<Vec<_>>()
            .into_par_iter()
            .filter_map(outcome)
            .collect::<Vec<_>>()
            .into_iter()
            .collect::<Validated<Vec<_>, _>>()
            .ok(),
    }
}

#[stacksafe::stacksafe]
pub fn coincident<K>(elements: Vec<Element>, priority: impl Fn(&Element) -> K + Sync, tolerance: Tolerance) -> Result<Vec<Element>, NEVec<LineworkError>>
where K: Ord + Send {
    #[stacksafe::stacksafe]
    fn children<'elements>(roots: &'elements [Element], element: Option<&'elements Element>) -> &'elements [Element] {
        match element {
            None => roots,
            Some(Element::Clip { region: _, keep: _, elements }) => elements.as_slice(),
            Some(
                Element::Stroke { path: _, pen: _, linetype: _ }
                | Element::Fill { region: _, color: _, opacity: _ }
                | Element::Hatch { region: _, pattern: _, placement: _, pen: _ }
                | Element::Text { text: _, color: _, opacity: _ }
                | Element::Glyphs { run: _, color: _, opacity: _ }
                | Element::Placed { content: _, placement: _, opacity: _ },
            ) => &[],
        }
    }
    let grid = tolerance.into_inner();
    let (lines, ranks): (Vec<_>, Vec<_>) = walk_tree_prefix(None::<&Element>, |element| children(&elements, *element).iter().map(Some))
        .filter_map(|element| match element? {
            element @ Element::Stroke { path, pen: _, linetype: _ } => Some((element, path)),
            Element::Clip { region: _, keep: _, elements: _ }
            | Element::Fill { region: _, color: _, opacity: _ }
            | Element::Hatch { region: _, pattern: _, placement: _, pen: _ }
            | Element::Text { text: _, color: _, opacity: _ }
            | Element::Glyphs { run: _, color: _, opacity: _ }
            | Element::Placed { content: _, placement: _, opacity: _ } => None,
        })
        .map(|(element, path)| {
            (path.subpaths().map(|subpath| crate::flattened(subpath.iter().copied(), tolerance).iter().flat_map(LineString::lines).collect::<Vec<_>>()).collect::<Vec<_>>(), priority(element))
        })
        .unzip();
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
        .map(|(left, right, _)| cmp::minmax_by(left, right, |left, right| ranks[left.key[0]].cmp(&ranks[right.key[0]]).then(right.key.cmp(&left.key))))
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
    let reworked = |stroke: usize, path: Path| match touched(stroke, usize::MIN..=usize::MAX).then(|| &lines[stroke]) {
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
    let mut current = elements.into_iter().enumerate();
    let mut parents = Vec::new();
    let mut next = 0_usize;
    iter::from_fn(|| {
        loop {
            match current.next() {
                Some((index, Element::Stroke { path, pen, linetype })) => {
                    let stroke = next;
                    next = next.strict_add(1);
                    break Some(Either::Left((index, stroke, path, pen, linetype)));
                }
                Some((index, Element::Clip { region, keep, elements: grouped })) => {
                    let grouped = grouped.into_inner();
                    let count = grouped.len();
                    parents.push((index, region, keep, count, mem::replace(&mut current, grouped.into_iter().enumerate())));
                }
                Some((_, element @ (Element::Fill { .. } | Element::Hatch { .. } | Element::Text { .. } | Element::Glyphs { .. } | Element::Placed { .. }))) =>
                    break Some(Either::Right(Either::Left(element))),
                None => {
                    let Some((index, region, keep, count, parent)) = parents.pop() else { break None };
                    current = parent;
                    break Some(Either::Right(Either::Right((index, region, keep, count))));
                }
            }
        }
    })
    .collect::<Vec<_>>()
    .into_par_iter()
    .map(|event| match event {
        Either::Left((index, stroke, path, pen, linetype)) => Either::Left(
            reworked(stroke, path)
                .map(|outcome| outcome.map(|remaining| Element::Stroke { path: remaining, pen, linetype }))
                .map_err(|source| LineworkError::Element { index, errors: nev![source.into()] }),
        ),
        Either::Right(Either::Left(element)) => Either::Left(Ok(Some(element))),
        Either::Right(Either::Right(parent)) => Either::Right(parent),
    })
    .collect::<Vec<_>>()
    .into_iter()
    .fold(Vec::<Result<Option<Element>, LineworkError>>::new(), |mut completed, event| {
        let outcome = match event {
            Either::Left(outcome) => outcome,
            Either::Right((index, region, keep, count)) => completed
                .drain(completed.len().strict_sub(count)..)
                .filter_map(Result::transpose)
                .collect::<Validated<Vec<_>, _>>()
                .ok()
                .map(|grouped| (!grouped.is_empty()).then_some(Element::Clip { region, keep, elements: grouped.into() }))
                .map_err(|errors| LineworkError::Element { index, errors: errors.into_nonempty_iter().map(StackSafe::new).collect() }),
        };
        completed.push(outcome);
        completed
    })
    .into_iter()
    .filter_map(Result::transpose)
    .collect::<Validated<Vec<_>, _>>()
    .ok()
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
    let take = |index: usize| free[index].replace(false);
    let next = |(last, (_, incoming)): (PathSeg, (Vec2, Vec2))| {
        tree.within_radius(&vertex(last.end()), radius)
            .into_iter()
            .filter(|hit| free[hit.data.0].get())
            .map(|hit| (crate::deflection(incoming, hit.data.2.0), hit))
            .min_by(|(left, near), (right, far)| left.total_cmp(right).then(near.distance.total_cmp(&far.distance)).then(near.index.cmp(&far.index)))
            .inspect(|(_, hit)| free[hit.data.0].set(false))
            .map(|(_, hit)| (hit.data.1, hit.data.2))
    };
    segments
        .iter()
        .enumerate()
        .filter(|&(seed, _)| take(seed))
        .map(|(_, &(first, (ahead, behind)))| {
            let forward = once((first, ahead)).chain(iter::successors(next((first, ahead)), |&link| next(link))).map(|(link, _)| link).collect::<NEVec<_>>();
            let backward = iter::successors(next((first.reverse(), behind)), |&link| next(link)).map(|(link, _)| link.reverse()).collect::<Vec<_>>();
            let run = backward.iter().rev().chain(forward.iter());
            let origin = backward.last().unwrap_or(&first).start();
            let closed = backward.len().strict_add(forward.len().get()) > 1 && forward.last().end().distance(origin) <= radius;
            iter::once(PathEl::MoveTo(origin)).chain(run.map(PathSeg::as_path_el)).chain(closed.then_some(PathEl::ClosePath)).collect::<BezPath>()
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
