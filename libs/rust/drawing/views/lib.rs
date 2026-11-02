#![feature(default_field_values)]

use core::{f64::consts::PI, iter};

use kurbo::{Affine, Point};
use manifold_rust::cancel::{self, CancelToken};
use nalgebra::{Isometry3, Matrix3, Matrix4, Orthographic3, Perspective3, Point3, Translation3, UnitQuaternion, Vector2, Vector3, Vector4};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use parry3d_f64::{
    partitioning::BvhBuildStrategy,
    shape::{SharedShape, TopologyError, TriMesh},
};
use rasm_linework::{Class, Deflection, EdgeKind, LineworkError, Tolerance};
use rasm_mesh::{MeshError, Solid, Source};
use rasm_proximity::{Assembly, ProximityError};
use rayon::iter::{IndexedParallelIterator, IntoParallelIterator, ParallelIterator as _};
use spade::InsertionError;
use validated::Validated;

use crate::visibility::Lines;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Projection {
    Parallel { volume: Orthographic3<f64>, receding: Option<Vector2<f64>> = None },
    Perspective(Perspective3<f64>),
}

#[derive(Debug, Clone)]
pub enum Body {
    Solid(Solid),
    Surface(TriMesh),
}

#[derive(Debug, Clone, Copy)]
struct Frame {
    eye: Isometry3<f64>,
    near: f64,
    clip: Matrix4<f64>,
    inverse: Matrix4<f64>,
    picture: Affine,
    plane: Isometry3<f64>,
}

impl Frame {
    fn new(eye: Isometry3<f64>, projection: Projection) -> Result<Self, ViewsError> {
        let (volume, [near, far], ordered) = match projection {
            Projection::Parallel { volume, receding } => (
                volume.to_homogeneous() * receding.map_or_else(Matrix4::identity, |receding| Matrix3::new(1.0, 0.0, -receding.x, 0.0, 1.0, -receding.y, 0.0, 0.0, 1.0).to_homogeneous()),
                [volume.znear(), volume.zfar()],
                volume.znear() < volume.zfar(),
            ),
            Projection::Perspective(volume) => (volume.to_homogeneous(), [volume.znear(), volume.zfar()], 0.0 < volume.znear() && volume.znear() < volume.zfar()),
        };
        let clip = volume * eye.to_homogeneous();
        let inverse = clip.try_inverse().filter(|inverse| ordered && iter::chain(&clip, inverse.iter()).all(|entry| entry.is_finite())).ok_or(ViewsError::Volume { near, far })?;
        let unprojected = eye.to_homogeneous() * inverse;
        let [origin, along, across] = [(0.0, 0.0), (1.0, 0.0), (0.0, 1.0)].map(|(right, up)| {
            let point = unprojected * Vector4::new(right, up, -1.0, 1.0);
            let picture = point.xy().unscale(point.w);
            Point::new(picture.x, picture.y)
        });
        Ok(Self {
            eye,
            near,
            clip,
            inverse,
            picture: Affine::new([along.x - origin.x, along.y - origin.y, across.x - origin.x, across.y - origin.y, origin.x, origin.y]),
            plane: eye.inv_mul(&Isometry3::from_parts(Translation3::new(0.0, 0.0, -near), UnitQuaternion::from_axis_angle(&Vector3::x_axis(), PI))),
        })
    }
}

#[derive(Debug, Clone)]
struct Prepared {
    body: usize,
    mesh: TriMesh,
    sources: Vec<Option<Source>>,
}

#[derive(Debug, Clone)]
struct Scene {
    bodies: Vec<Prepared>,
    assembly: Assembly,
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
        let (bodies, features) = bodies
            .into_par_iter()
            .enumerate()
            .map(|(index, body)| {
                let cut = match body {
                    Body::Solid(_) => None,
                    Body::Surface(_) => Some(sections::cuts),
                };
                sections::prepared(&frame, index, body, token).map(|prepared| {
                    prepared.map(|prepared| {
                        let features = iter::chain(edges::features(&frame, &prepared, crease), cut.map_or_default(|cuts| cuts(&frame, &prepared, tolerance))).collect::<Vec<_>>();
                        (prepared, features)
                    })
                })
            })
            .collect::<Validated<Vec<_>, _>>()
            .ok()?
            .into_iter()
            .flatten()
            .unzip::<_, _, Vec<_>, Vec<_>>();
        let assembly = Assembly::new(BvhBuildStrategy::Ploc, bodies.iter().map(|prepared| (Isometry3::identity(), SharedShape::new(prepared.mesh.clone()))))
            .map_err(|errors| errors.into_nonempty_iter().map(ViewsError::from).collect::<NEVec<_>>())?;
        let scene = Scene { bodies, assembly };
        (!cancel::is_cancelled(token)).ok_or(NEVec::new(ViewsError::Cancelled))?;
        visibility::lines(&frame, &scene, features.into_iter().flatten().collect(), tolerance).map(|lines| Self { frame, scene, lines, crease, tolerance }).map_err(NEVec::new)
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
    #[error("view volume from depth {near} to {far} is singular, not finite or out of order")]
    Volume { near: f64, far: f64 },
    #[error("body {body} yields no mesh")]
    Mesh { body: usize, source: MeshError },
    #[error("body {body} holds no half-edge topology")]
    Topology { body: usize, source: TopologyError },
    #[error(transparent)]
    Proximity(#[from] ProximityError),
    #[error(transparent)]
    Triangulation(#[from] InsertionError),
    #[error("edge run refused")]
    Path { class: Class, source: linesweeper::Error },
    #[error("{operation} refused")]
    Linework { operation: Operation, source: LineworkError },
    #[error("view cancelled before hidden-line classification")]
    Cancelled,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn project(view: &View, point: &Point3<f64>) -> Option<Point> { picture(&view.frame, clip(&view.frame, point)) }

fn clip(frame: &Frame, point: &Point3<f64>) -> Vector4<f64> { frame.clip * point.to_homogeneous() }

fn picture(frame: &Frame, clip: Vector4<f64>) -> Option<Point> {
    (clip.w > 0.0).then(|| clip.xy().unscale(clip.w)).map(|ndc| frame.picture * Point::new(ndc.x, ndc.y)).filter(|point| point.is_finite())
}

fn ray(frame: &Frame, ndc: Point) -> (Point3<f64>, Vector3<f64>) {
    let [near, far] = [-1.0, 1.0].map(|z| {
        let point = frame.inverse * Vector4::new(ndc.x, ndc.y, z, 1.0);
        Point3::from(point.xyz().unscale(point.w))
    });
    (near, far - near)
}

fn depth(frame: &Frame, point: &Point3<f64>) -> f64 { -(frame.eye * point).z - frame.near }

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod edges;
pub mod faces;
pub mod sections;
pub mod shadows;
mod visibility;
