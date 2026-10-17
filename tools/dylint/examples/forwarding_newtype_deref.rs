use core::{
    hint::black_box,
    ops::{Deref, DerefMut},
};
use std::sync::Arc;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Default)]
struct Names(Vec<String>);

impl Deref for Names {
    type Target = Vec<String>;

    fn deref(&self) -> &Self::Target { &self.0 }
}

impl DerefMut for Names {
    fn deref_mut(&mut self) -> &mut Self::Target { &mut self.0 }
}

#[derive(Debug, Default, derive_more::Deref)]
struct Scores(Vec<u32>);

#[derive(Debug)]
struct Shared(Arc<str>);

impl Deref for Shared {
    type Target = Arc<str>;

    fn deref(&self) -> &Self::Target { &self.0 }
}

#[derive(Debug, Default)]
struct Tracked(Vec<u8>);

impl Deref for Tracked {
    type Target = Vec<u8>;

    fn deref(&self) -> &Self::Target { &self.0 }
}

impl Drop for Tracked {
    fn drop(&mut self) { self.0.clear(); }
}

#[derive(Debug, Default)]
pub struct Exported(Vec<u8>);

impl Deref for Exported {
    type Target = Vec<u8>;

    fn deref(&self) -> &Self::Target { &self.0 }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let mut names = Names::default();
    names.push(String::from("name"));
    black_box(&(names.len(), Scores::default().len(), Shared(Arc::from("shared")).len(), Tracked::default().len(), Exported::default().len()));
}
