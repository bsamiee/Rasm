use std::iter;

use geo::{BooleanOps as _, BoundingRect as _, MapCoords as _};
use geo_types::{LineString, MultiLineString};
use kurbo::{Affine, BezPath, ParamCurve as _, ParamCurveArclen as _, PathEl, Point, dash, segments};

use crate::{Pattern, Region, Tolerance, regions};

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn expand(region: &Region, pattern: &Pattern, placement: Affine, tolerance: Tolerance) -> BezPath {
    crate::extent(region).map_or_default(|extent| {
        let polygons = regions::polygons(region, tolerance);
        pattern
            .families
            .iter()
            .filter_map(|family| {
                let frame = placement * Affine::rotate(family.angle).then_translate(family.origin.to_vec2());
                (frame.is_finite() && frame.determinant().is_normal() && family.delta.is_finite()).then_some((family, frame))
            })
            .flat_map(|(family, frame)| {
                let (inverse, spacing) = (frame.inverse(), family.delta.y);
                let (local, step) = (inverse.transform_rect_bbox(extent), |y: f64| if spacing == 0.0_f64 { 0.0_f64 } else { (y / spacing).round() });
                let (first, last) = (step(local.y0), step(local.y1));
                let rows = iter::successors(Some(first.min(last)), |row| Some(row + 1.0_f64))
                    .take_while(|&row| row <= first.max(last))
                    .map(|row| LineString::new(vec![crate::coord(frame * Point::new(local.x0, row * spacing)), crate::coord(frame * Point::new(local.x1, row * spacing))]))
                    .collect::<MultiLineString<f64>>();
                let clipped = polygons.clip(&rows, false);
                match &family.dashes {
                    None => crate::path(clipped),
                    Some(dashes) => {
                        let dotted = dashes.as_ref().contains(&0.0_f64);
                        BezPath::from_path_segments(
                            clipped
                                .into_iter()
                                .filter_map(|line| line.map_coords(|world| crate::coord(inverse * Point::from(world.x_y()))).bounding_rect())
                                .flat_map(|span| {
                                    let row = step(span.center().y);
                                    let ends = [Point::new(span.min().x, row * spacing), Point::new(span.max().x, row * spacing)];
                                    segments(dash([PathEl::MoveTo(ends[0]), PathEl::LineTo(ends[1])].into_iter(), row.mul_add(-family.delta.x, span.min().x), dashes.as_ref().as_slice()))
                                        .map(move |piece| (frame * piece, ends.map(|end| frame * end)))
                                })
                                .filter_map(|(piece, ends)| {
                                    let length = piece.arclen(tolerance.into_inner());
                                    (length >= tolerance.into_inner() || (length == 0.0_f64 && dotted) || ends.iter().all(|end| piece.start().distance(*end) >= tolerance.into_inner()))
                                        .then_some(piece)
                                }),
                        )
                    }
                }
            })
            .collect()
    })
}
