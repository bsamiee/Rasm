use core::borrow::Borrow;
use core::iter;

use cavalier_contours::shape_algorithms::Shape;
use itertools::Itertools as _;
use nalgebra::Point2;
use nonempty_collections::{IntoIteratorExt as _, NEVec, NonEmptyIterator};
use spade::handles::FixedVertexHandle;
use spade::{ConstrainedDelaunayTriangulation, RefinementParameters, RefinementResult, Triangulation as _};
use validated::Validated;

use crate::{PlanarError, Region, Tolerance, flattened, position};

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
        let point = |coordinates: spade::Point2<f64>| Point2::from(<[f64; 2]>::from(coordinates));
        let mut crossing = Vec::new();
        let vertices = loops.into_iter().flatten().map(position).collect();
        let loaded = ConstrainedDelaunayTriangulation::<spade::Point2<f64>>::try_bulk_load_cdt(vertices, edges, |edge| crossing.push(edge)).map_err(|source| PlanarError::Extent { source })?;
        let mut cdt = crossing
            .into_iter()
            .map(|edge| edge.map(|index| point(loaded.vertex(FixedVertexHandle::from_index(index)).position())))
            .try_into_nonempty_iter()
            .map(NonEmptyIterator::collect::<NEVec<_>>)
            .map_or(Ok(loaded), |segments| Err(PlanarError::Crossing { segments }))?;
        let sites = sites
            .into_iter()
            .enumerate()
            .map(|(site, location)| cdt.insert(position(*location.borrow())).map_err(|source| (site, source)))
            .collect::<Validated<Vec<_>, _>>()
            .ok()
            .map_err(|sites| PlanarError::Insertion { sites })?;
        let unrefined = refinement.is_none();
        let RefinementResult { excluded_faces, refinement_complete } = cdt.refine(refinement.unwrap_or_else(|| RefinementParameters::new().with_max_additional_vertices(0)).exclude_outer_faces(true));
        let mut excluded = vec![false; cdt.num_all_faces()];
        for face in excluded_faces {
            excluded[face.index()] = true;
        }
        let kept = cdt.inner_faces().filter(|face| !excluded[face.fix().index()]).map(|face| face.vertices().map(|vertex| vertex.fix().index())).collect_vec();
        let mut used = vec![false; cdt.num_vertices()];
        for vertex in kept.iter().flatten() {
            used[*vertex] = true;
        }
        let slots = used
            .iter()
            .scan(0_usize, |next, used| {
                let slot = *next;
                *next = next.strict_add(usize::from(*used));
                Some(slot)
            })
            .collect_vec();
        Ok(Self {
            vertices: cdt.vertices().filter(|vertex| used[vertex.fix().index()]).map(|vertex| point(vertex.position())).collect(),
            triangles: kept.into_iter().map(|corners| corners.map(|vertex| slots[vertex])).collect(),
            sites: sites.into_iter().map(|vertex| used[vertex.index()].then_some(slots[vertex.index()])).collect(),
            refinement_complete: unrefined || refinement_complete,
        })
    }
}
