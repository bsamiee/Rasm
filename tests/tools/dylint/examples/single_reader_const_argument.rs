#![feature(adt_const_params)]

use core::hint::black_box;
use core::marker::ConstParamTy;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(PartialEq, Eq, ConstParamTy)]
enum Kind {
    App,
    Tool,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TOOL: Kind = Kind::Tool; //~ single_reader has one reader, `tools`

const APP: Kind = Kind::App;

// --- [MODELS] --------------------------------------------------------------------------

struct Id<const KIND: Kind> {
    raw: u32,
}

mod registry {
    use super::{Id, Kind};

    const LOCAL: Kind = Kind::App; //~ single_reader has one reader, `hidden`

    pub(crate) fn hidden(raws: Vec<u32>) -> u32 { raws.into_iter().map(|raw| Id { raw }).map(|id: Id<{ LOCAL }>| id.raw).sum() }

    pub(crate) fn owned(raws: Vec<u32>) -> u32 { raws.into_iter().map(|raw| Id { raw }).map(|id: Id<{ Kind::App }>| id.raw).sum() } //~ single_reader has one reader, `summary`

    pub(crate) fn borrowed(raws: &[u32]) -> u32 { raws.iter().map(|&raw| Id { raw }).collect::<Vec<_>>().iter().map(|id: &Id<{ Kind::App }>| id.raw).sum() } //~ single_reader has one reader, `summary`

    pub(crate) fn nested(groups: &[Vec<u32>]) -> u32 { groups.iter().map(|group| group.iter().map(|&raw| Id { raw }).map(|id: Id<{ Kind::App }>| id.raw).sum::<u32>()).sum() } //~ single_reader
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn offset(raw: u32) -> u32 { raw.strict_add(1) } //~ single_reader has one reader, `scaled`

fn raw(id: &Id<{ Kind::App }>) -> u32 { id.raw }

fn forwarded(id: &Id<{ Kind::App }>) -> u32 { raw(id) } //~ forwarding_function

fn scaled(ids: &[Id<{ Kind::App }>]) -> Vec<u32> { ids.iter().map(|id: &Id<{ Kind::App }>| offset(raw(id))).collect() }

fn tools(raws: Vec<u32>) -> Vec<u32> { raws.into_iter().map(|raw| Id::<{ Kind::Tool }> { raw }).map(|id: Id<{ TOOL }>| id.raw).collect() }

fn apps(raws: Vec<u32>) -> Vec<u32> { raws.into_iter().map(|raw| Id { raw }).map(|id: Id<{ APP }>| raw(&id)).collect() }

fn first(raws: &[u32]) -> Option<u32> { raws.first().map(|&raw| Id::<{ APP }> { raw }.raw) }

fn summary(raws: Vec<u32>) -> u32 { registry::owned(raws.clone()).strict_add(registry::borrowed(&raws)).strict_add(registry::nested(&[raws.clone()])).strict_add(registry::hidden(raws)) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let ids = [Id::<{ Kind::App }> { raw: 2 }, Id { raw: 5 }];
    black_box((forwarded(&Id { raw: 1 }), scaled(&ids), tools(vec![3]), apps(vec![4]), first(&[6]), summary(vec![7, 8])));
}
