use core::{iter, num::NonZero};

use indexmap::IndexMap;
use itertools::Itertools as _;
use nonempty_collections::{NESet, NEVec};
use petgraph::graphmap::DiGraphMap;
use serde::{Deserialize, Serialize};
use strum::VariantArray as _;
use uom::si::f64::{Angle, Length};
use url::Url;
use validated::Validated;

use crate::{
    Composed, IfcError, Model, Path,
    schemas::{Attribute, Target, attribute, record},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "UPPERCASE")]
pub enum FlowDirection {
    Source,
    Sink,
    SourceAndSink,
    NotDefined,
}

impl Attribute for FlowDirection {
    const ID: &'static str = "bsi::ifc::system::flowdirection";
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, strum::EnumString)]
pub enum Kind {
    Project,
    Site,
    Facility,
    Building,
    Bridge,
    MarineFacility,
    Railway,
    Road,
    FacilityPartCommon,
    BuildingStorey,
    BridgePart,
    MarinePart,
    RailwayPart,
    RoadPart,
    Space,
    SpatialZone,
    ExternalSpatialElement,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, strum::IntoStaticStr, strum::VariantArray)]
pub enum Link {
    #[strum(serialize = "bsi::ifc::system::connectsto")]
    ConnectsTo,
    #[strum(serialize = "bsi::ifc::system::partofsystem")]
    PartOfSystem,
    #[strum(serialize = "bsi::ifc::system::servicesfacility")]
    ServicesFacility,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct Class {
    pub code: Box<str>,
    pub uri: Url,
}

impl Attribute for Class {
    const ID: &'static str = "bsi::ifc::class";
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
#[serde(transparent)]
pub struct Material(pub Class);

impl Attribute for Material {
    const ID: &'static str = "bsi::ifc::material";
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Relation<'model> {
    Aggregates,
    Contains,
    Bounds { boundary: &'model Path },
    Link(Link),
}

#[derive(Debug, Clone, Copy, PartialEq, Deserialize)]
#[serde(untagged, deny_unknown_fields, expecting = "latitude and longitude or eastings and northings with an optional height, or a height alone")]
pub enum Coordinates {
    Geographic { latitude: Angle, longitude: Angle, height: Option<Length> },
    Projected { eastings: Length, northings: Length, height: Option<Length> },
    Vertical { height: Length },
}

#[derive(Debug, Deserialize)]
pub struct SpaceBoundary {
    #[serde(rename = "relatedelement")]
    related_element: Target,
    #[serde(rename = "relatingspace")]
    relating_space: Target,
}

impl Attribute for SpaceBoundary {
    const ID: &'static str = "bsi::ifc::spaceBoundary";
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CLASSIFICATION]

pub fn kind(node: &Composed) -> Result<Option<Kind>, IfcError> {
    attribute::<Class>(node).map(|class| class.and_then(|Class { code, uri: _ }| code.trim_end_matches(|letter: char| letter.is_ascii_uppercase() || letter == '_').strip_prefix("Ifc")?.parse().ok()))
}

pub fn classifications(node: &Composed) -> Result<IndexMap<&str, Class>, NEVec<IfcError>> {
    node.attributes
        .keys()
        .filter_map(|id| id.strip_suffix("::class").map(|namespace| (id, namespace)))
        .filter_map(|(id, namespace)| record::<Class>(node, id).transpose().map(|class| class.map(|class| (namespace, class))))
        .collect::<Validated<IndexMap<_, _>, _>>()
        .ok()
}

pub fn classified<'model>(model: &'model Model, code: &str) -> Result<Vec<&'model Composed>, NEVec<IfcError>> {
    model.holding(Class::ID).filter_map(|node| attribute::<Class>(node).map(|class| class.is_some_and(|class| &*class.code == code).then_some(node)).transpose()).collect::<Validated<Vec<_>, _>>().ok()
}

// --- [RELATIONS]

pub fn relations<'model>(model: &'model Model) -> Result<DiGraphMap<&'model Path, NESet<Relation<'model>>>, NEVec<IfcError>> {
    let resolved = |Target { reference }: Target| model.get(&reference).map(|target| &target.path);
    let bounded = |node: &'model Composed| {
        attribute::<SpaceBoundary>(node).map_or_else(Validated::fail, |boundary| {
            boundary
                .into_iter()
                .map(|SpaceBoundary { related_element, relating_space }| {
                    Validated::from(resolved(related_element)).map2(Validated::from(resolved(relating_space)), |element, space| (element, space, Relation::Bounds { boundary: &node.path }))
                })
                .collect::<Validated<Vec<_>, _>>()
        })
    };
    let linked = |node: &'model Composed, link: Link| {
        record::<Vec<Target>>(node, link.into())
            .map_or_else(Validated::fail, |targets| targets.into_iter().flatten().map(|target| resolved(target).map(|to| (&node.path, to, Relation::Link(link)))).collect::<Validated<Vec<_>, _>>())
    };
    let classified = model.holding(Class::ID).map(|node| kind(node).map(|spatial| (&node.path, (node, spatial)))).collect::<Validated<IndexMap<_, _>, _>>();
    let stated = model.nodes().flat_map(|node| iter::once(bounded(node)).chain(Link::VARIANTS.iter().map(move |&link| linked(node, link)))).collect::<Validated<Vec<Vec<_>>, _>>();
    classified
        .map2(stated, |classified, stated| {
            let decomposed = classified.values().flat_map(|&(node, parent)| node.children.values().map(move |child| (node, parent, child))).filter_map(|(node, parent, child)| {
                match (parent, classified.get(&child.path)) {
                    (_, None) => None,
                    (Some(_), Some(&(_, None))) => Some((&node.path, &child.path, Relation::Contains)),
                    (None, Some(_)) | (Some(_), Some(&(_, Some(_)))) => Some((&node.path, &child.path, Relation::Aggregates)),
                }
            });
            let placed = classified.keys().fold(DiGraphMap::<_, NESet<_>>::new(), |mut graph, &path| {
                graph.add_node(path);
                graph
            });
            decomposed.chain(stated.into_iter().flatten()).fold(placed, |mut graph, (from, to, relation)| {
                if let Some(relations) = graph.edge_weight_mut(from, to) {
                    relations.insert(relation);
                } else {
                    graph.add_edge(from, to, NESet::new(relation));
                }
                graph
            })
        })
        .ok()
}

pub fn network<'model>(model: &'model Model) -> Result<DiGraphMap<&'model Path, ()>, NEVec<IfcError>> {
    let owners = |port: &'model Composed| {
        let owners = model.parents(&port.path).map(|owner| &owner.path).collect::<Vec<_>>();
        if owners.is_empty() { Vec::from([&port.path]) } else { owners }
    };
    let oriented = |from: &'model Composed, to: &'model Composed| {
        use FlowDirection::{NotDefined, Sink, Source, SourceAndSink};
        attribute::<FlowDirection>(from).and_then(|upstream| {
            attribute::<FlowDirection>(to).map(|downstream| match (upstream, downstream) {
                (Some(Source), Some(Source | Sink | SourceAndSink | NotDefined) | None) | (Some(Sink | SourceAndSink | NotDefined) | None, Some(Sink)) => [Some((from, to)), None],
                (Some(Sink), Some(Source | SourceAndSink | NotDefined) | None) | (Some(SourceAndSink | NotDefined) | None, Some(Source)) => [Some((to, from)), None],
                (Some(SourceAndSink | NotDefined) | None, Some(SourceAndSink | NotDefined) | None) => [Some((from, to)), Some((to, from))],
            })
        })
    };
    model
        .holding(Link::ConnectsTo.into())
        .map(|port| {
            record::<Vec<Target>>(port, Link::ConnectsTo.into()).map_or_else(Validated::fail, |targets| {
                targets.into_iter().flatten().map(|Target { reference }| model.get(&reference).and_then(|target| oriented(port, target))).collect::<Validated<Vec<_>, _>>()
            })
        })
        .collect::<Validated<Vec<_>, _>>()
        .map(|ports| ports.into_iter().flatten().flatten().flatten().flat_map(|(from, to)| owners(from).into_iter().cartesian_product(owners(to))).collect())
        .ok()
}

// --- [GEOREFERENCE]

pub fn georeference(node: &Composed) -> Result<IndexMap<NonZero<u32>, Coordinates>, NEVec<IfcError>> {
    node.attributes
        .keys()
        .filter_map(|id| id.strip_prefix("epsg").map(|digits| (id, digits)))
        .map(|(id, digits)| {
            Validated::from(digits.parse::<NonZero<u32>>().map_err(|source| IfcError::Epsg { node: node.path.clone(), attribute: id.clone(), source }))
                .map2(Validated::from(record::<Coordinates>(node, id)), |code, coordinates| coordinates.map(|coordinates| (code, coordinates)))
        })
        .collect::<Validated<Vec<_>, _>>()
        .map(|georeferences| georeferences.into_iter().flatten().collect())
        .ok()
}
