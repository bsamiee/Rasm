#![feature(cmp_minmax, control_flow_into_value, try_blocks_heterogeneous)]

use core::{cmp, iter, num::NonZero};

use faer::{
    ColRef, Mat, MatRef, RowRef, Side,
    sparse::{
        Argsort, CreationError, Pair, SparseColMat, SymbolicSparseColMat, Triplet,
        linalg::{LltError, solvers::SymbolicLlt},
    },
};
use itertools::Itertools as _;
use nalgebra::{DVector, DVectorView, Point3, Vector3};
use nonempty_collections::NEVec;
use petgraph::unionfind::UnionFind;
use rasm_optimization::OptimizeError;
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Input {
    Nodes,
    Edges,
    Densities,
    Members,
    Groups,
    Goals,
    Bounds,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Node {
    Support(Point3<f64>),
    Free { position: Point3<f64>, load: Vector3<f64> },
}

#[derive(Debug, Clone, derive_more::AsRef)]
pub struct Network {
    #[as_ref]
    nodes: Vec<Node>,
    #[as_ref]
    edges: Vec<[usize; 2]>,
    index: Vec<Option<usize>>,
    free: SparseColMat<usize, f64>,
    supports: SparseColMat<usize, f64>,
    anchors: Mat<f64>,
    loads: Mat<f64>,
    symbolic: SymbolicSparseColMat<usize>,
    argsort: Argsort<usize>,
    llt: SymbolicLlt<usize>,
}

impl Network {
    pub fn new(nodes: Vec<Node>, edges: Vec<[usize; 2]>) -> Result<Self, NEVec<FormError>> {
        iter::chain(
            nodes.iter().enumerate().filter_map(|(index, node)| {
                (!match node {
                    Node::Support(position) => position.iter().all(|coordinate| coordinate.is_finite()),
                    Node::Free { position, load } => iter::chain(position.iter(), load.iter()).all(|coordinate| coordinate.is_finite()),
                })
                .then_some(FormError::Entry { input: Input::Nodes, index })
            }),
            edges.iter().enumerate().filter_map(|(index, &[first, second])| (first == second || first.max(second) >= nodes.len()).then_some(FormError::Entry { input: Input::Edges, index })),
        )
        .map(Err)
        .collect::<Validated<(), _>>()
        .ok()?;
        let side = |project: fn(&Node) -> Option<Vector3<f64>>| {
            let ranks = nodes.iter().scan(0_usize.., |rank, node| Some(project(node).and_then(|_| rank.next()))).collect::<Vec<_>>();
            let rows = nodes.iter().filter_map(project).collect::<Vec<_>>();
            (Mat::from_fn(rows.len(), 3, |row, axis| rows[row][axis]), ranks)
        };
        let ((loads, index), (positions, supported)) = (
            side(|node| match *node {
                Node::Support(_) => None,
                Node::Free { position: _, load } => Some(load),
            }),
            side(|node| match *node {
                Node::Support(position) => Some(position.coords),
                Node::Free { position: _, load: _ } => None,
            }),
        );
        let mut components = edges.iter().fold(UnionFind::<usize>::new(nodes.len()), |mut components, &[first, second]| {
            components.union(first, second);
            components
        });
        let support = supported
            .iter()
            .positions(Option::is_some)
            .reduce(|first, second| {
                components.union(first, second);
                first
            })
            .map(|node| components.find_mut(node));
        (0..nodes.len()).filter(|&node| Some(components.find_mut(node)) != support).map(|node| Err(FormError::Floating { node })).collect::<Validated<(), _>>().ok()?;
        drop(components);
        let incidence = |ranks: &[Option<usize>]| {
            let entries = edges
                .iter()
                .enumerate()
                .flat_map(|(edge, ends)| iter::zip(ends, [1.0, -1.0]).filter_map(move |(&end, sign)| ranks[end].map(|column| Triplet::new(edge, column, sign))))
                .collect::<Vec<_>>();
            Validated::from(SparseColMat::<usize, f64>::try_new_from_triplets(edges.len(), ranks.iter().flatten().count(), &entries).map_err(FormError::from))
        };
        let (free, supports) = incidence(&index).map2(incidence(&supported), |free, supports| (free, supports)).ok()?;
        (try bikeshed Result<Self, FormError> {
            let anchors = supports.as_ref() * positions.as_ref();
            let pattern = grounded(&index, &edges, iter::repeat(1.0)).map(|(pair, _)| pair).collect::<Vec<_>>();
            let (symbolic, argsort) = SymbolicSparseColMat::<usize>::try_new_from_indices(loads.nrows(), loads.nrows(), &pattern)?;
            let llt = SymbolicLlt::try_new(symbolic.as_ref(), Side::Lower).map_err(LltError::Generic)?;
            Self { nodes, edges, index, free, supports, anchors, loads, symbolic, argsort, llt }
        })
        .map_err(NEVec::new)
    }
}

#[derive(Debug, Clone, PartialEq)]
pub struct Form {
    pub positions: Vec<Point3<f64>>,
    pub forces: DVector<f64>,
    pub lengths: DVector<f64>,
    pub reactions: Vec<Vector3<f64>>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum FormError {
    #[error("{input} holds {found} entries where the network holds {expected}")]
    Shape { input: Input, expected: usize, found: usize },
    #[error("{input} entry {index} lies outside its domain")]
    Entry { input: Input, index: usize },
    #[error("free node {node} reaches no support")]
    Floating { node: usize },
    #[error(transparent)]
    Assembly(#[from] CreationError),
    #[error(transparent)]
    Factorization(#[from] LltError),
    #[error(transparent)]
    Optimize(#[from] OptimizeError),
    #[error("relaxation left an out-of-balance force of {residual} after {steps} steps")]
    Unconverged { steps: NonZero<u32>, residual: f64 },
    #[error("relaxation turned non-finite at step {step}")]
    Diverged { step: u32 },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ASSEMBLY]

fn grounded<I>(index: &[Option<usize>], edges: &[[usize; 2]], weights: I) -> impl Iterator<Item = (Pair<usize, usize>, f64)>
where I: IntoIterator<Item = f64> {
    iter::zip(edges, weights).flat_map(move |(&[first, second], weight)| {
        let (start, end) = (index[first], index[second]);
        [
            start.map(|start| (Pair::new(start, start), weight)),
            end.map(|end| (Pair::new(end, end), weight)),
            start.zip(end).map(|(start, end)| {
                let [low, high] = cmp::minmax(start, end);
                (Pair::new(high, low), -weight)
            }),
        ]
        .into_iter()
        .flatten()
    })
}

fn laplacian(network: &Network, magnitudes: impl IntoIterator<Item = f64>) -> Result<SparseColMat<usize, f64>, CreationError> {
    SparseColMat::new_from_argsort(network.symbolic.clone(), &network.argsort, &grounded(&network.index, &network.edges, magnitudes).map(|(_, value)| value).collect::<Vec<_>>())
        .map_err(CreationError::from)
}

// --- [EQUILIBRIUM]

fn spans(network: &Network, free: MatRef<'_, f64>) -> Mat<f64> { &network.anchors + network.free.as_ref() * free }

fn balance<'network>(network: &'network Network, spans: MatRef<'_, f64>, densities: DVectorView<'_, f64>) -> (Mat<f64>, impl FnOnce() -> Mat<f64> + use<'network>) {
    let weighted = ColRef::from_slice(densities.as_slice()).as_diagonal() * spans;
    (&network.loads - network.free.as_ref().transpose() * &weighted, move || network.supports.as_ref().transpose() * &weighted)
}

fn vector(row: RowRef<'_, f64>) -> Vector3<f64> { Vector3::from_iterator(row.iter().copied()) }

fn form(network: &Network, free: MatRef<'_, f64>, spans: MatRef<'_, f64>, densities: DVectorView<'_, f64>, (residual, reactions): (Mat<f64>, impl FnOnce() -> Mat<f64>)) -> Form {
    let supports = reactions();
    let (mut moved, mut held) = (iter::zip(free.row_iter(), residual.row_iter()), supports.row_iter());
    let (positions, reactions) = network
        .nodes
        .iter()
        .filter_map(|node| match *node {
            Node::Support(position) => held.next().map(|reaction| (position, vector(reaction))),
            Node::Free { position: _, load: _ } => moved.next().map(|(position, imbalance)| (Point3::from(vector(position)), -vector(imbalance))),
        })
        .unzip();
    let lengths = DVector::from_iterator(spans.nrows(), spans.row_iter().map(|span| span.norm_l2()));
    Form { positions, forces: densities.component_mul(&lengths), lengths, reactions }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod densities;
mod goals;
pub mod relaxation;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use goals::{Goal, Measure, Optimum, optima};
