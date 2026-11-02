use core::f64::consts::TAU;

use fidget::{context::Tree, shapes::Difference};
use nalgebra::{Affine3, Matrix3};

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

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct UnitCell(Affine3<f32>);

impl UnitCell {
    #[must_use]
    pub fn new(edges: Matrix3<f64>) -> Option<Self> {
        edges
            .try_inverse()
            .map(|inverse| (inverse * TAU).cast::<f32>())
            .filter(|phase| phase.iter().copied().all(f32::is_finite) && phase.cast::<f64>().try_inverse().is_some())
            .map(|phase| Self(Affine3::from_matrix_unchecked(phase.to_homogeneous())))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn lattice(kind: Lattice, cell: UnitCell) -> Tree {
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
    .remap_affine(cell.0)
}

#[must_use]
pub fn shell(tree: Tree, inner: impl Into<Tree>, outer: impl Into<Tree>) -> Tree { Difference { shape: tree.clone() - outer, cutout: tree - inner }.into() }
