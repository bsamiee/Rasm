use core::{iter, mem};
use std::sync::Arc;

use krilla::{
    surface::Surface,
    tagging::{ContentTag, Identifier, Node, Tag, TagGroup},
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Default)]
pub(crate) struct Leaves {
    graphics: Vec<Node>,
    runs: Vec<Node>,
    open: bool,
}

impl Leaves {
    pub(crate) fn graphic(&mut self, surface: &mut Surface<'_>) {
        if !mem::replace(&mut self.open, true) {
            self.graphics.push(surface.start_tagged(ContentTag::Other).into());
        }
    }

    pub(crate) fn close(&mut self, surface: &mut Surface<'_>) {
        if mem::take(&mut self.open) {
            surface.end_tagged();
        }
    }

    pub(crate) fn run(&mut self, surface: &mut Surface<'_>, draw: impl FnOnce(&mut Surface<'_>)) {
        self.close(surface);
        let identifier = surface.start_tagged(ContentTag::Other);
        draw(surface);
        surface.end_tagged();
        self.runs.push(TagGroup::with_children(Tag::P, vec![identifier.into()]).into());
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn part<'layer>(layers: impl IntoIterator<Item = (&'layer Arc<str>, Leaves)>, links: impl IntoIterator<Item = Identifier>) -> Node {
    let drawn = layers
        .into_iter()
        .flat_map(|(name, Leaves { graphics, runs, open: _ })| iter::chain((!graphics.is_empty()).then(|| Node::from(TagGroup::with_children(Tag::Figure(Some(name.to_string())), graphics))), runs));
    let linked = links.into_iter().map(|identifier| Node::from(TagGroup::with_children(Tag::Link, vec![identifier.into()])));
    TagGroup::with_children(Tag::Part, iter::chain(drawn, linked).collect()).into()
}
