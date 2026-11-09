#![feature(cmp_minmax, default_field_values)]

use core::iter;
use std::sync::Arc;

use kurbo::{Affine, Point, Rect};
use manifold_rust::cancel::{self, CancelToken};
use nalgebra::{Isometry3, Matrix3, Matrix4, Orthographic3, Perspective3, Point3, Quaternion, Translation3, UnitQuaternion, Vector2, Vector3, Vector4};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use parry3d_f64::{
    partitioning::BvhBuildStrategy,
    shape::{SharedShape, TopologyError, TriMesh},
};
use rasm_drawing::{Class, Deflection, EdgeKind, LineworkError, Tolerance};
use rasm_mesh::{MeshError, Solid, Source};
use rasm_proximity::{Assembly, ProximityError};
use rayon::iter::{IndexedParallelIterator, IntoParallelIterator, IntoParallelRefIterator as _, ParallelIterator as _};
use spade::InsertionError;
use validated::Validated;

use crate::visibility::Lines;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Projection {
    Parallel { volume: Orthographic3<f64>, receding: Option<Vector2<f64>> = None },
    Perspective { volume: Perspective3<f64>, shift: Option<Vector2<f64>> = None },
}

#[derive(Debug, Clone)]
pub enum Body {
    Solid(Solid),
    Surface(TriMesh),
}

#[derive(Debug, Clone, Copy)]
struct Frame {
    clip: Matrix4<f64>,
    inverse: Matrix4<f64>,
    picture: Affine,
    plane: Isometry3<f64>,
}

impl Frame {
    fn new(eye: Isometry3<f64>, projection: Projection) -> Result<Self, ViewsError> {
        let (forward, backward, along, [near, far], ahead) = match projection {
            Projection::Parallel { volume, receding } => (volume.to_homogeneous(), volume.inverse(), receding.map_or_else(Vector2::zeros, |receding| -receding), [volume.znear(), volume.zfar()], true),
            Projection::Perspective { volume, shift } => (volume.to_homogeneous(), volume.inverse(), shift.unwrap_or_else(Vector2::zeros), [volume.znear(), volume.zfar()], volume.znear() > 0.0),
        };
        let shear = |slope: Vector2<f64>| Matrix3::new(1.0, 0.0, slope.x, 0.0, 1.0, slope.y, 0.0, 0.0, 1.0).to_homogeneous();
        let clip = forward * shear(along) * eye.to_homogeneous();
        let unprojected = shear(-along) * backward;
        let inverse = eye.inverse().to_homogeneous() * unprojected;
        let [origin, right, up] = [(0.0, 0.0), (1.0, 0.0), (0.0, 1.0)].map(|(x, y)| unprojected.transform_point(&Point3::new(x, y, -1.0)));
        let picture = Affine::new([right.x - origin.x, right.y - origin.y, up.x - origin.x, up.y - origin.y, origin.x, origin.y]);
        (ahead && near < far && iter::chain(&clip, &inverse).all(|entry| entry.is_finite()) && picture.inverse().is_finite()).ok_or(ViewsError::Volume { near, far })?;
        Ok(Self { clip, inverse, picture, plane: eye.inv_mul(&Isometry3::from_parts(Translation3::new(0.0, 0.0, -near), UnitQuaternion::new_normalize(Quaternion::new(0.0, 1.0, 0.0, 0.0)))) })
    }
}

#[derive(Debug, Clone)]
struct Prepared {
    body: usize,
    mesh: Arc<TriMesh>,
    sources: Vec<Option<Source>>,
    surface: bool,
}

#[derive(Debug, Clone)]
struct Scene {
    bodies: Vec<Prepared>,
    assembly: Arc<Assembly>,
}

#[derive(Debug, Clone)]
pub struct View {
    frame: Frame,
    scene: Scene,
    lines: Lines,
    crease: Deflection,
    tolerance: Tolerance,
}

impl View {
    pub fn new(
        eye: Isometry3<f64>,
        projection: Projection,
        bodies: impl IntoParallelIterator<Item = Body, Iter: IndexedParallelIterator>,
        crease: Deflection,
        tolerance: Tolerance,
        token: Option<&CancelToken>,
    ) -> Result<Self, NEVec<ViewsError>> {
        let frame = Frame::new(eye, projection).map_err(NEVec::new)?;
        let bodies = bodies
            .into_par_iter()
            .enumerate()
            .filter(|_| !cancel::is_cancelled(token))
            .filter_map(|(index, body)| sections::prepared(&frame, index, body, token).transpose())
            .collect::<Validated<Vec<_>, _>>()
            .ok()?;
        let renumbered = |error: ProximityError| {
            let ProximityError::NonFinite { indices } = error else {
                return ViewsError::from(error);
            };
            ViewsError::from(ProximityError::NonFinite { indices: indices.into_nonempty_iter().map(|index| bodies[index].body).collect() })
        };
        let assembly = Assembly::new(BvhBuildStrategy::Ploc, bodies.iter().map(|prepared| (Isometry3::identity(), SharedShape(Arc::<TriMesh>::clone(&prepared.mesh)))))
            .map_err(|errors| errors.into_nonempty_iter().map(renumbered).collect::<NEVec<_>>())?;
        let scene = Scene { bodies, assembly: Arc::new(assembly) };
        (!cancel::is_cancelled(token)).ok_or(NEVec::new(ViewsError::Cancelled))?;
        outline(&frame, &scene, crease, tolerance).map(|lines| Self { frame, scene, lines, crease, tolerance }).map_err(NEVec::new)
    }
}

#[derive(Debug, Clone, Copy)]
struct Feature {
    ends: [Point3<f64>; 2],
    kind: Option<EdgeKind>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Operation {
    #[display("region of body {_0}")]
    Region(usize),
    #[display("stroke cleanup")]
    Cleanup,
}

#[derive(Debug, thiserror::Error)]
pub enum ViewsError {
    #[error("view from depth {near} to {far} is singular, not finite or out of order")]
    Volume { near: f64, far: f64 },
    #[error("body {body} yields no mesh")]
    Mesh { body: usize, source: MeshError },
    #[error("body {body} holds no half-edge topology")]
    Topology { body: usize, source: TopologyError },
    #[error(transparent)]
    Proximity(#[from] ProximityError),
    #[error(transparent)]
    Triangulation(#[from] InsertionError),
    #[error("cannot trace edge run")]
    Path { class: Class, source: linesweeper::Error },
    #[error("cannot complete {operation}")]
    Linework { operation: Operation, source: LineworkError },
    #[error("view cancelled before hidden-line classification")]
    Cancelled,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn project(view: &View, point: &Point3<f64>) -> Option<Point> { picture(&view.frame, clip(&view.frame, point)) }

#[must_use]
pub fn window(view: &View) -> Rect {
    let [corner, _, opposite, _] = visibility::window(&view.frame);
    Rect::from_points(corner, opposite)
}

fn clip(frame: &Frame, point: &Point3<f64>) -> Vector4<f64> { frame.clip * point.to_homogeneous() }

fn picture(frame: &Frame, clip: Vector4<f64>) -> Option<Point> {
    (clip.w > 0.0).then(|| clip.xy().unscale(clip.w)).map(|ndc| frame.picture * Point::new(ndc.x, ndc.y)).filter(|point| point.is_finite())
}

fn ray(frame: &Frame, ndc: Point) -> (Point3<f64>, Vector3<f64>) {
    let [near, far] = [-1.0, 1.0].map(|z| frame.inverse.transform_point(&Point3::new(ndc.x, ndc.y, z)));
    (near, far - near)
}

fn depth(frame: &Frame, point: &Point3<f64>) -> f64 { frame.plane.inverse_transform_point(point).z }

fn outline(frame: &Frame, scene: &Scene, crease: Deflection, tolerance: Tolerance) -> Result<Lines, ViewsError> {
    let features = scene
        .bodies
        .par_iter()
        .flat_map_iter(|prepared| iter::chain(edges::features(frame, prepared, crease), prepared.surface.then(|| sections::cuts(frame, prepared, tolerance)).into_iter().flatten()));
    visibility::lines(frame, scene, features.collect(), tolerance)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod edges;
mod faces;
mod sections;
mod shadows;
mod visibility;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use edges::{edges, strokes};
pub use faces::{Face, faces};
pub use sections::sections;
pub use shadows::shadows;
