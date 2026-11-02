use std::{cmp, iter};

use geo::{BooleanOps as _, BoundingRect as _, MapCoords as _};
use geo_types::{LineString, MultiLineString};
use kurbo::{Affine, BezPath, ParamCurve as _, ParamCurveArclen as _, PathEl, Point, Shape as _, Vec2, dash, segments};
use nonempty_collections::NEVec;
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::{Family, LineworkError, Pattern, Placement, Region, Tolerance};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn expand<'pattern>(region: &Region, pattern: &'pattern Pattern, placement: Placement, tolerance: Tolerance) -> Result<BezPath, NEVec<LineworkError>> {
    let accuracy = tolerance.into_inner();
    let (polygons, extent) = (crate::polygons(region, tolerance), region.bounding_box());
    let framed = |(family, descriptor): (usize, &'pattern Family)| {
        let frame = *placement * Affine::rotate(descriptor.angle).then_translate(descriptor.origin.to_vec2());
        let [x, y, ..] = frame.as_coeffs();
        let spacing = (frame.determinant() * descriptor.delta.y).abs() / Vec2::new(x, y).hypot();
        (spacing >= accuracy).then_some((frame, descriptor)).ok_or(LineworkError::Spacing { family, spacing })
    };
    let lines = |(frame, family): (Affine, &Family)| {
        let (inverse, spacing) = (frame.inverse(), family.delta.y);
        let local = inverse.transform_rect_bbox(extent);
        let [first, last] = cmp::minmax_by((local.y0 / spacing).round(), (local.y1 / spacing).round(), f64::total_cmp);
        let rows = iter::successors(Some(first), |row| Some(row + 1.0_f64))
            .take_while(|&row| row <= last)
            .map(|row| LineString::new(vec![crate::coord(frame * Point::new(local.x0, row * spacing)), crate::coord(frame * Point::new(local.x1, row * spacing))]))
            .collect::<MultiLineString<f64>>();
        let clipped = polygons.clip(&rows, false);
        match &family.dashes {
            None => crate::path(clipped).into_elements(),
            Some(dashes) => {
                let dotted = dashes.contains(&0.0_f64);
                BezPath::from_path_segments(
                    clipped
                        .into_iter()
                        .filter_map(|line| line.map_coords(|world| crate::coord(inverse * Point::from(world.x_y()))).bounding_rect())
                        .flat_map(|span| {
                            let row = (span.center().y / spacing).round();
                            let ends = [Point::new(span.min().x, row * spacing), Point::new(span.max().x, row * spacing)];
                            segments(dash([PathEl::MoveTo(ends[0]), PathEl::LineTo(ends[1])].into_iter(), row.mul_add(-family.delta.x, span.min().x), dashes.as_ref().as_slice()))
                                .map(move |piece| (frame * piece, ends.map(|end| frame * end)))
                        })
                        .filter_map(|(piece, ends)| {
                            let length = piece.arclen(accuracy);
                            (length >= accuracy || (length == 0.0_f64 && dotted) || ends.iter().all(|end| piece.start().distance(*end) >= accuracy)).then_some(piece)
                        }),
                )
                .into_elements()
            }
        }
    };
    pattern.families.iter().enumerate().map(framed).collect::<Validated<Vec<_>, _>>().ok().map(|families| BezPath::from_vec(families.into_par_iter().flat_map_iter(lines).collect()))
}
