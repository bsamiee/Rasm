use core::cmp;
use core::f64::consts::TAU;
use core::num::NonZero;
use std::collections::BTreeSet;

use fidget::context::Tree;
use fidget::shapes::Blend;
use fidget::var::Var;
use itertools::{Itertools as _, iproduct};
use nalgebra::{Affine3, Matrix3, Point3, Vector3};
use statrs::statistics::{Data, OrderStatistics as _};

use crate::{Evaluation, Field, ImplicitError, Shape, sample};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Lattice {
    Gyroid,
    Primitive,
    Diamond,
    Neovius,
    Iwp,
    FischerKochS,
    Lidinoid,
    SplitP,
    Frd,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Form {
    Network,
    Sheet,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Truss {
    BodyCenteredCubic,
    FaceCenteredCubic,
    Octet,
    Kelvin,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct UnitCell {
    phase: Affine3<f32>,
    world: Affine3<f32>,
}

impl UnitCell {
    #[must_use]
    pub fn new(edges: Matrix3<f64>) -> Option<Self> {
        let affine = |linear: Matrix3<f64>| Some(linear.cast::<f32>()).filter(|cast| cast.determinant().is_normal()).map(|cast| Affine3::from_matrix_unchecked(cast.to_homogeneous()));
        edges.try_inverse().and_then(|inverse| Some(Self { phase: affine(inverse * TAU)?, world: affine(edges / TAU)? }))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn lattice(kind: Lattice, cell: UnitCell) -> Tree { nodal(kind).remap_affine(cell.phase) }

pub fn solid(evaluation: &Evaluation<'_>, kind: Lattice, cell: UnitCell, form: Form, density: impl Into<Tree>, resolution: NonZero<u16>) -> Result<Tree, ImplicitError> {
    let tree = match form {
        Form::Network => nodal(kind),
        Form::Sheet => nodal(kind).abs(),
    };
    let count = NonZero::<u32>::from(resolution).get();
    let step = TAU / f64::from(count);
    let grid = iproduct!(0..count, 0..count, 0..count).map(|(x, y, z)| Point3::new(x, y, z).map(|index| (f64::from(index) + 0.5) * step));
    let mut levels = Data::new(sample::<f64>(&Shape::from(Field { origin: Point3::origin(), tree: tree.clone() }), evaluation, grid)?);
    let base = levels.quantile(0.0).cast::<f32>();
    let scaled = Tree::constant(f32::from(resolution.get())) * density;
    Ok(tree.remap_affine(cell.phase)
        - (0..=resolution.get())
            .map(|knot| (knot, levels.quantile(f64::from(knot) / f64::from(count)).cast::<f32>()))
            .tuple_windows()
            .fold(Tree::constant(base), |graded, ((knot, low), (_, high))| graded + (high - low) * (scaled.clone() - f32::from(knot)).max(0.0_f32).min(1.0_f32)))
}

#[must_use]
pub fn truss(kind: Truss, cell: UnitCell, radius: impl Into<Tree>, fillet: f64) -> Tree {
    fn joined(starts: &[Vector3<i32>], ends: &[Vector3<i32>], length: i32) -> impl Iterator<Item = ([i32; 3], [i32; 3])> {
        iproduct!(starts, ends)
            .filter(move |&(start, end)| (end - start).dot(&(end - start)) == length)
            .map(|(start, end)| ((start + end).map(|twice| twice.rem_euclid(8)).into(), cmp::max((end - start).into(), (start - end).into())))
    }
    let sides = [0, 4];
    let center = Vector3::repeat(2);
    let corners = iproduct!(sides, sides, sides).map(|(x, y, z)| Vector3::new(x, y, z)).collect::<Vec<Vector3<i32>>>();
    let faces = iproduct!(0..3, [-2, 2]).map(|(axis, offset)| center + Vector3::ith(axis, offset)).collect::<Vec<_>>();
    let struts = match kind {
        Truss::BodyCenteredCubic => joined(&corners, &[center], 12).collect::<BTreeSet<_>>(),
        Truss::FaceCenteredCubic => joined(&corners, &faces, 8).collect(),
        Truss::Octet => joined(&corners, &faces, 8).chain(joined(&faces, &faces, 8)).collect(),
        Truss::Kelvin => {
            let nodes = iproduct!(0..3, 0..3, [-2, 2], [-1, 1])
                .filter(|&(long, short, _, _)| long != short)
                .map(|(long, short, across, along)| center + Vector3::ith(long, across) + Vector3::ith(short, along))
                .collect::<Vec<_>>();
            joined(&nodes, &nodes, 2).collect()
        }
    };
    let scale = (TAU / 8.0).cast::<f32>();
    let (turn, half_turn) = (TAU.cast::<f32>(), (TAU / 2.0).cast::<f32>());
    let (x, y, z) = Tree::axes();
    let segment = |(twice, direction): ([i32; 3], [i32; 3])| {
        let middle = Vector3::from(twice).cast::<f32>() * scale;
        let half = cell.world * (Vector3::from(direction).cast::<f32>() * scale);
        let along = ((x.clone() * half.x + y.clone() * half.y + z.clone() * half.z) / half.norm_squared()).max(-1.0_f32).min(1.0_f32);
        let [wrapped_x, wrapped_y, wrapped_z] = [(&x, middle.x), (&y, middle.y), (&z, middle.z)].map(|(axis, at)| (axis.clone() + (half_turn - at)).modulo(turn) - half_turn);
        norm([x.clone() - along.clone() * half.x, y.clone() - along.clone() * half.y, z.clone() - along * half.z]).remap_affine(cell.world).remap_xyz(wrapped_x, wrapped_y, wrapped_z)
    };
    struts.into_iter().map(segment).fold(Tree::constant(f32::INFINITY), |nearest, strut| Tree::from(Blend { a: nearest, b: strut, radius: fillet.cast::<f32>() })).remap_affine(cell.phase) - radius
}

#[must_use]
pub fn distance(tree: Tree, floor: impl Into<Tree>) -> Tree {
    let gradient = [Var::X, Var::Y, Var::Z].map(|axis| tree.deriv(axis));
    tree / norm(gradient).max(floor)
}

#[must_use]
pub fn shell(tree: Tree, inner: impl Into<Tree>, outer: impl Into<Tree>) -> Tree {
    let (inner, outer) = (inner.into(), outer.into());
    (tree.clone() - inner.max(outer.clone())).max(inner.min(outer) - tree)
}

fn norm([x, y, z]: [Tree; 3]) -> Tree { (x.square() + y.square() + z.square()).sqrt() }

fn nodal(kind: Lattice) -> Tree {
    let (x, y, z) = Tree::axes();
    let [x2, y2, z2] = [&x, &y, &z].map(|axis| axis.clone() * 2.0);
    let cyclic = |term: Tree| term.remap_xyz(y.clone(), z.clone(), x.clone()) + term.remap_xyz(z.clone(), x.clone(), y.clone()) + term;
    match kind {
        Lattice::Gyroid => cyclic(x.sin() * y.cos()),
        Lattice::Primitive => cyclic(x.cos()),
        Lattice::Diamond => x.sin() * y.sin() * z.sin() + cyclic(x.sin() * y.cos() * z.cos()),
        Lattice::Neovius => 3.0 * cyclic(x.cos()) + 4.0 * x.cos() * y.cos() * z.cos(),
        Lattice::Iwp => 2.0 * cyclic(x.cos() * y.cos()) - cyclic(x2.cos()),
        Lattice::FischerKochS => cyclic(x2.cos() * y.sin() * z.cos()),
        Lattice::Lidinoid => 0.5 * cyclic(x2.sin() * y.cos() * z.sin()) - 0.5 * cyclic(x2.cos() * y2.cos()) + 0.15,
        Lattice::SplitP => 1.1 * cyclic(x2.sin() * y.cos() * z.sin()) - 0.2 * cyclic(x2.cos() * y2.cos()) - 0.4 * cyclic(x2.cos()),
        Lattice::Frd => 8.0 * x.cos() * y.cos() * z.cos() + x2.cos() * y2.cos() * z2.cos() - cyclic(x2.cos() * y2.cos()),
    }
}
