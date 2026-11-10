use itertools::Itertools as _;
use kurbo::{BezPath, Shape as _, Triangle};
use linesweeper::{BinaryOp, FillRule};
use nalgebra::Point3;
use nonempty_collections::NEVec;
use rasm_drawing::regions::boolean;
use rasm_drawing::{LineworkError, Region};
use rayon::iter::{IntoParallelRefIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::{Operation, Prepared, View, ViewsError, clip, picture, window};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn sections(view: &View) -> Result<Vec<(usize, Region)>, NEVec<ViewsError>> {
    let View { frame, scene, lines: _, crease: _, tolerance } = view;
    let border = window(view);
    let pictured = |vertex| picture(frame, clip(frame, &Point3::from(vertex))).ok_or(linesweeper::Error::Infinity);
    scene
        .bodies
        .par_iter()
        .filter(|prepared| prepared.sources.contains(&None))
        .filter_map(|Prepared { body, mesh, sources, surface: _ }| {
            mesh.triangles()
                .zip(sources)
                .filter(|&(_, source)| source.is_none())
                .map(|(triangle, _)| {
                    let [start, middle, end] = triangle.vertices().map(pictured);
                    Ok::<_, linesweeper::Error>(Triangle::new(start?, middle?, end?).path_elements(tolerance.into_inner()))
                })
                .flatten_ok()
                .collect::<Result<BezPath, _>>()
                .map_err(LineworkError::from)
                .and_then(|caps| Region::new(caps, FillRule::NonZero, *tolerance))
                .and_then(|capped| boolean(&capped, [&Region::new(border, FillRule::NonZero, *tolerance)?], BinaryOp::Intersection, *tolerance))
                .map(|region| (!region.contours().is_empty()).then_some((*body, region)))
                .map_err(|source| ViewsError::Linework { operation: Operation::Region(*body), source })
                .transpose()
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}
