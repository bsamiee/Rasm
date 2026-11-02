use core::num::NonZero;

use indexmap::IndexMap;
use nonempty_collections::{NESet, NEVec};
use petgraph::graphmap::DiGraphMap;
use serde::{Deserialize, Serialize};
use strum::VariantArray as _;
use uom::si::f64::{Angle, Length};
use validated::Validated;

use crate::{
    Composed, IfcError, Model, Path,
    schemas::{Attribute, attribute, record},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "UPPERCASE")]
pub enum Flow {
    Source,
    Sink,
    SourceAndSink,
    NotDefined,
}

impl Attribute for Flow {
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
    FacilityPart,
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
    pub uri: Box<str>,
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
pub enum Relation {
    Contains,
    Bounds,
    Link(Link),
}

#[derive(Debug, Clone, Copy, PartialEq, Deserialize)]
#[serde(untagged)]
pub enum Coordinates {
    Geographic { latitude: Angle, longitude: Angle },
    Projected { eastings: Length, northings: Length },
    Vertical { height: Length },
}

#[derive(Deserialize)]
struct Target {
    #[serde(rename = "ref")]
    reference: Path,
}

#[derive(Deserialize)]
struct SpaceBoundary {
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
    attribute::<Class>(node).map(|class| class.and_then(|Class { code, uri: _ }| code.strip_prefix("Ifc").and_then(|name| name.parse().ok())))
}

// --- [RELATIONS]

pub fn relations(model: &Model) -> Result<DiGraphMap<&Path, NESet<Relation>>, NEVec<IfcError>> {
    let resolved = move |Target { reference }: Target| model.get(&reference).map(|target| &target.node).ok_or(IfcError::Unknown { path: reference });
    model
        .nodes()
        .flat_map(|node| {
            let contained = kind(node).map(|spatial| {
                node.children.values().filter(|child| spatial.is_some() && child.attributes.contains_key(Class::ID)).map(|child| (&node.node, &child.node, Relation::Contains)).collect::<Vec<_>>()
            });
            let bounded = attribute::<SpaceBoundary>(node).map_or_else(Validated::fail, |boundary| {
                boundary
                    .into_iter()
                    .map(|SpaceBoundary { related_element, relating_space }| {
                        Validated::from(resolved(related_element)).map2(Validated::from(resolved(relating_space)), |element, space| (element, space, Relation::Bounds))
                    })
                    .collect::<Validated<Vec<_>, _>>()
            });
            let linked = Link::VARIANTS.iter().map(move |&link| {
                record::<Vec<Target>>(node, link.into()).map_or_else(Validated::fail, |targets| {
                    targets.into_iter().flatten().map(|target| resolved(target).map(|to| (&node.node, to, Relation::Link(link)))).collect::<Validated<Vec<_>, _>>()
                })
            });
            [Validated::from(contained), bounded].into_iter().chain(linked)
        })
        .collect::<Validated<Vec<Vec<_>>, _>>()
        .ok()
        .map(|edges| {
            edges.into_iter().flatten().fold(DiGraphMap::new(), |mut graph, (from, to, relation)| {
                if let Some(relations) = graph.edge_weight_mut(from, to) {
                    relations.insert(relation);
                } else {
                    graph.add_edge(from, to, NESet::new(relation));
                }
                graph
            })
        })
}

// --- [GEOREFERENCE]

pub fn georeference(node: &Composed) -> Result<IndexMap<NonZero<u32>, Coordinates>, NEVec<IfcError>> {
    node.attributes
        .keys()
        .filter_map(|id| id.strip_prefix("epsg").and_then(|code| code.parse::<NonZero<u32>>().ok()).map(|code| (code, id)))
        .filter_map(|(code, id)| record::<Coordinates>(node, id).transpose().map(|decoded| decoded.map(|coordinates| (code, coordinates))))
        .collect::<Validated<IndexMap<_, _>, _>>()
        .ok()
}
