use core::{borrow::Borrow, iter};
use std::collections::{HashMap, HashSet};

use cavalier_contours::shape_algorithms::Shape;
use itertools::Itertools as _;
use nalgebra::Point2;
use nonempty_collections::NEVec;
use spade::{ConstrainedDelaunayTriangulation, RefinementParameters, RefinementResult, Triangulation as _, handles::FixedVertexHandle};

use crate::{PlanarError, Region, Tolerance, flattened};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, derive_more::AsRef)]
pub struct Triangulation {
    #[as_ref([Point2<f64>])]
    vertices: Box<[Point2<f64>]>,
    #[as_ref([[usize; 3]])]
    triangles: Box<[[usize; 3]]>,
    #[as_ref([Option<usize>])]
    sites: Box<[Option<usize>]>,
    #[as_ref(bool)]
    refinement_complete: bool,
}

impl Triangulation {
    pub fn new<I>(region: &Region, sites: I, tolerance: Tolerance, refinement: Option<RefinementParameters<f64>>) -> Result<Self, PlanarError>
    where I: IntoIterator<Item: Borrow<Point2<f64>>> {
        let Shape { ccw_plines: bodies, cw_plines: holes, plines_index: _ } = region.as_ref();
        let loops = iter::chain(bodies, holes).map(|outline| flattened(&outline.polyline, tolerance)).collect::<Result<Vec<_>, _>>()?;
        let edges = loops
            .iter()
            .scan(0_usize, |start, points| {
                let ring = *start..start.strict_add(points.len());
                *start = ring.end;
                Some(ring)
            })
            .flat_map(|ring| ring.circular_tuple_windows::<(_, _)>().map(<[usize; 2]>::from))
            .collect();
        let position = |point: Point2<f64>| spade::mitigate_underflow(<[f64; 2]>::from(point).into());
        let point = |coordinates: spade::Point2<f64>| Point2::from(<[f64; 2]>::from(coordinates));
        let sites = sites.into_iter().map(|site| position(*site.borrow())).collect::<Vec<_>>();
        let mut crossing = Vec::new();
        let positions = loops.into_iter().flatten().map(position).chain(sites.iter().copied()).collect();
        let loaded = ConstrainedDelaunayTriangulation::<spade::Point2<f64>>::try_bulk_load_cdt(positions, edges, |edge| crossing.push(edge))?;
        let mut cdt = NEVec::try_from_vec(crossing.into_iter().map(|edge| edge.map(|index| point(loaded.vertex(FixedVertexHandle::from_index(index)).position()))).collect())
            .map_or(Ok(loaded), |segments| Err(PlanarError::Crossing { segments }))?;
        let unrefined = refinement.is_none();
        let RefinementResult { excluded_faces, refinement_complete } = cdt.refine(refinement.unwrap_or_else(|| RefinementParameters::new().with_max_additional_vertices(0)).exclude_outer_faces(true));
        let excluded = excluded_faces.into_iter().collect::<HashSet<_>>();
        let mut vertices = Vec::new();
        let mut slots = HashMap::new();
        let triangles = cdt
            .inner_faces()
            .filter(|face| !excluded.contains(&face.fix()))
            .map(|face| {
                face.vertices().map(|vertex| {
                    *slots.entry(vertex.fix()).or_insert_with(|| {
                        let slot = vertices.len();
                        vertices.push(point(vertex.position()));
                        slot
                    })
                })
            })
            .collect::<Box<[_]>>();
        Ok(Self {
            vertices: vertices.into_boxed_slice(),
            triangles,
            sites: sites.into_iter().map(|site| cdt.locate_vertex(site).and_then(|vertex| slots.get(&vertex.fix()).copied())).collect(),
            refinement_complete: unrefined || refinement_complete,
        })
    }
}
