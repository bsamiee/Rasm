#![feature(cmp_minmax, control_flow_into_value, try_blocks_heterogeneous)]

use core::{cmp, iter, num::NonZero};

use faer::{
    Accum, MatMut, MatRef, Side, get_global_parallelism,
    sparse::{
        Argsort, CreationError, Pair, SparseColMat, SymbolicSparseColMat, Triplet,
        linalg::{LltError, matmul::sparse_dense_matmul, solvers::SymbolicLlt},
    },
};
use itertools::Itertools as _;
use nalgebra::{DVector, DVectorView, Matrix3xX, Point3, Vector3};
use nonempty_collections::NEVec;
use petgraph::unionfind::UnionFind;
use rasm_optimize::OptimizeError;
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
    anchors: Matrix3xX<f64>,
    loads: Matrix3xX<f64>,
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
            (Matrix3xX::from_iterator(ranks.iter().flatten().count(), nodes.iter().filter_map(project).flat_map(<[f64; 3]>::from)), ranks)
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
            let mut anchors = Matrix3xX::zeros(edges.len());
            sparse_dense_matmul(view_mut(&mut anchors), Accum::Replace, supports.as_ref(), view(&positions), 1.0, get_global_parallelism());
            let pattern = grounded(&index, &edges, iter::repeat(1.0)).map(|(pair, _)| pair).collect::<Vec<_>>();
            let (symbolic, argsort) = SymbolicSparseColMat::<usize>::try_new_from_indices(loads.ncols(), loads.ncols(), &pattern)?;
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

// --- [VIEWS]

fn view(matrix: &Matrix3xX<f64>) -> MatRef<'_, f64> { MatRef::from_column_major_slice(matrix.as_slice(), matrix.nrows(), matrix.ncols()).transpose() }

fn view_mut(matrix: &mut Matrix3xX<f64>) -> MatMut<'_, f64> {
    let (rows, columns) = matrix.shape();
    MatMut::from_column_major_slice_mut(matrix.as_mut_slice(), rows, columns).transpose_mut()
}

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

fn spans(network: &Network, free: &Matrix3xX<f64>) -> Matrix3xX<f64> {
    let mut spans = network.anchors.clone();
    sparse_dense_matmul(view_mut(&mut spans), Accum::Add, network.free.as_ref(), view(free), 1.0, get_global_parallelism());
    spans
}

fn balance<'network>(network: &'network Network, spans: &Matrix3xX<f64>, densities: DVectorView<'_, f64>) -> (Matrix3xX<f64>, impl FnOnce() -> Matrix3xX<f64> + use<'network>) {
    let weighted = Matrix3xX::from_iterator(spans.ncols(), iter::zip(spans.column_iter(), densities.iter()).flat_map(|(span, &density)| <[f64; 3]>::from(span * density)));
    let mut residual = network.loads.clone();
    sparse_dense_matmul(view_mut(&mut residual), Accum::Add, network.free.transpose(), view(&weighted), -1.0, get_global_parallelism());
    (residual, move || {
        let mut reactions = Matrix3xX::zeros(network.supports.ncols());
        sparse_dense_matmul(view_mut(&mut reactions), Accum::Replace, network.supports.transpose(), view(&weighted), 1.0, get_global_parallelism());
        reactions
    })
}

fn form(network: &Network, free: &Matrix3xX<f64>, spans: &Matrix3xX<f64>, densities: DVectorView<'_, f64>, (residual, reactions): (Matrix3xX<f64>, impl FnOnce() -> Matrix3xX<f64>)) -> Form {
    let (positions, reactions) = {
        let supports = reactions();
        let (mut moved, mut held) = (iter::zip(free.column_iter(), residual.column_iter()), supports.column_iter());
        network
            .nodes
            .iter()
            .filter_map(|node| match *node {
                Node::Support(position) => held.next().map(|reaction| (position, reaction.into_owned())),
                Node::Free { position: _, load: _ } => moved.next().map(|(position, imbalance)| (Point3::from(position.into_owned()), -imbalance)),
            })
            .unzip()
    };
    let lengths = DVector::from_iterator(spans.ncols(), spans.column_iter().map(|span| span.norm()));
    Form { positions, forces: densities.component_mul(&lengths), lengths, reactions }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod densities;
pub mod goals;
pub mod relaxation;
