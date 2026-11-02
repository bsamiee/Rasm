use std::{borrow::Cow, iter, mem};

use itertools::Itertools as _;
use kurbo::{BezPath, PathEl, PathSeg, Shape as _, Stroke, StrokeOpts, Vec2, segments, stroke};

use crate::{Alignment, Linetype, Pen, Tolerance};

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn phases<'path>(path: &'path BezPath, linetype: &Linetype, tolerance: Tolerance) -> Vec<(Cow<'path, [PathEl]>, f64, kurbo::Dashes)> {
    let pattern = linetype.dashes.iter().copied().collect::<kurbo::Dashes>();
    let first = *linetype.dashes.first();
    let period = linetype.dashes.iter().sum::<f64>();
    let length = |elements: &[PathEl]| elements.perimeter(tolerance.into_inner() / f64::from(u32::try_from(segments(elements.iter().copied()).count()).unwrap_or(u32::MAX)));
    let fitted = |piece: Cow<'path, [PathEl]>| {
        let length = length(&piece);
        let scale = length / ((length / period).round().max(1.0_f64) * period);
        (length > 0.0_f64).then(|| (piece, scale * first / 2.0_f64, pattern.iter().map(|dash| dash * scale).collect()))
    };
    path.subpaths()
        .filter(|elements| segments(elements.iter().copied()).next().is_some())
        .flat_map(|elements| match linetype.alignment {
            Alignment::Start { dash_offset } => vec![(Cow::Borrowed(elements), dash_offset.into_inner(), pattern.clone())],
            Alignment::Center => vec![(Cow::Borrowed(elements), first.midpoint(-length(elements)), pattern.clone())],
            Alignment::Corners { angle } => {
                let corner = |(_, (_, incoming)): &(PathSeg, (Vec2, Vec2)), (_, (outgoing, _)): &(PathSeg, (Vec2, Vec2))| crate::deflection(*incoming, *outgoing) > angle.into_inner();
                let mut runs = segments(elements.iter().copied()).filter_map(|segment| crate::tangents(&segment).map(|tangents| (segment, tangents))).collect::<Vec<_>>();
                let seam = if matches!(elements.last(), Some(PathEl::ClosePath)) { iter::zip(iter::chain(runs.last(), &runs), &runs).position(|(left, right)| corner(left, right)) } else { Some(0) };
                let Some(seam) = seam else { return fitted(Cow::Borrowed(elements)).into_iter().collect() };
                runs.rotate_left(seam);
                runs.chunk_by(|left, right| !corner(left, right)).filter_map(|run| fitted(Cow::Owned(BezPath::from_path_segments(run.iter().map(|&(segment, _)| segment)).into_elements()))).collect()
            }
        })
        .collect()
}

#[must_use]
pub fn dash(path: &BezPath, linetype: &Linetype, tolerance: Tolerance) -> BezPath { joined(&phases(path, linetype, tolerance), tolerance).collect() }

#[must_use]
pub fn outline(path: &BezPath, pen: &Pen, linetype: Option<&Linetype>, tolerance: Tolerance) -> BezPath {
    let dotted = |pattern: &kurbo::Dashes| {
        let (pairs, _) = pattern.as_chunks::<2>();
        let dot = pairs.iter().filter_map(|&[on, off]| (on <= 0.0_f64).then_some(off)).reduce(f64::min).map_or_default(|gap| gap.min(tolerance.into_inner()));
        (dot / 2.0_f64, pairs.iter().flat_map(|&[on, off]| if on <= 0.0_f64 { [dot, off - dot] } else { [on, off] }).collect::<kurbo::Dashes>())
    };
    let centerline = linetype.map_or(Cow::Borrowed(path), |linetype| {
        let pieces = phases(path, linetype, tolerance)
            .into_iter()
            .map(|(piece, offset, pattern)| {
                let (shift, pattern) = dotted(&pattern);
                (piece, offset + shift, pattern)
            })
            .collect::<Vec<_>>();
        Cow::Owned(joined(&pieces, tolerance).collect())
    });
    stroke(
        centerline.iter(),
        &Stroke::new(pen.width.into_inner()).with_caps(pen.cap).with_join(pen.join).with_miter_limit(pen.miter_limit.into_inner()),
        &StrokeOpts::default(),
        tolerance.into_inner(),
    )
}

fn joined(pieces: &[(Cow<'_, [PathEl]>, f64, kurbo::Dashes)], tolerance: Tolerance) -> impl Iterator<Item = PathEl> {
    let touching = move |left: &[PathEl], right: &[PathEl]| {
        left.last().and_then(PathEl::end_point).zip(right.first().and_then(PathEl::end_point)).is_some_and(|(end, start)| end.distance(start) <= tolerance.into_inner())
    };
    let leaves =
        move |part: &[PathEl]| part.first().and_then(PathEl::end_point).is_some_and(|start| part.iter().filter_map(PathEl::end_point).any(|point| point.distance(start) > tolerance.into_inner()));
    pieces.chunk_by(|(left, _, _), (right, _, _)| left.last().and_then(PathEl::end_point).is_some_and(|end| right.first() == Some(&PathEl::MoveTo(end)))).flat_map(move |group| {
        let mut parts = group
            .iter()
            .flat_map(|(piece, offset, pattern)| {
                let mut parts = kurbo::dash(piece.iter().copied(), *offset, pattern).collect::<BezPath>().subpaths().map(<[PathEl]>::to_vec).collect::<Vec<_>>();
                let start = parts.iter().position(|part| part.first() == piece.first()).unwrap_or_default();
                parts.rotate_left(start);
                parts
            })
            .coalesce(|mut previous, part| {
                if touching(&previous, &part) {
                    previous.extend(part.into_iter().skip(1));
                    Ok(previous)
                } else {
                    Err((previous, part))
                }
            })
            .collect::<Vec<_>>();
        match parts.as_mut_slice() {
            [only] if leaves(only) && touching(only, only) => only.push(PathEl::ClosePath),
            [first, .., last] if touching(last, first) => last.extend(mem::take(first).into_iter().skip(1)),
            _ => {}
        }
        parts.into_iter().flatten()
    })
}
