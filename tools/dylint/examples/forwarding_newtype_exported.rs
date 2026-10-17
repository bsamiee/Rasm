use core::ops::Deref;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Default)]
pub struct Public(Vec<u32>);

impl Public {
    #[must_use]
    pub const fn len(&self) -> usize { self.0.len() }
}

impl AsRef<Vec<u32>> for Public {
    fn as_ref(&self) -> &Vec<u32> { &self.0 }
}

#[derive(Debug, Default)]
pub struct Exported(Vec<u8>);

impl Deref for Exported {
    type Target = Vec<u8>;

    fn deref(&self) -> &Self::Target { &self.0 }
}
