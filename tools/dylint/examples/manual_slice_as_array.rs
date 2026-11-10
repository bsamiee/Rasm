use core::array::TryFromSliceError;
use core::cell::Cell;
use core::hint::black_box;
use core::ops::{Deref, DerefMut};

// --- [TYPES] ---------------------------------------------------------------------------

trait Measured {
    fn len(&self) -> usize;
}

impl Measured for [u8; 4] {
    fn len(&self) -> usize { 2 }
}

// --- [MODELS] --------------------------------------------------------------------------

struct Projected {
    bytes: [u8; 4],
    reads: Cell<u32>,
}

impl Deref for Projected {
    type Target = [u8];
    fn deref(&self) -> &Self::Target {
        self.reads.update(|reads| reads + 1);
        &self.bytes
    }
}

impl DerefMut for Projected {
    fn deref_mut(&mut self) -> &mut Self::Target {
        self.reads.update(|reads| reads + 10);
        &mut self.bytes
    }
}

impl Projected {
    fn as_array<const N: usize>(&self) -> Option<&[u8; N]> { None }
}

struct FieldView {
    value: ([u16; 4],),
    reads: Cell<u32>,
}

impl Deref for FieldView {
    type Target = ([u16; 4],);
    fn deref(&self) -> &Self::Target {
        self.reads.update(|reads| reads + 1);
        &self.value
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn viewed(slice: &[u8]) -> Option<&[u8; 4]> { <&[u8; 4]>::try_from(slice).ok() }

fn viewed_mut(slice: &mut [u8]) -> Option<&mut [u8; 2]> { slice.try_into().ok() }

fn prefix(slice: &[u8]) -> Option<[u8; 2]> { slice[..2].try_into().ok() }

fn suffix(slice: &[u8]) -> Option<[u8; 2]> { slice[slice.len() - 2..].try_into().ok() }

fn array_prefix(array: &[u8; 4]) -> Option<[u8; 2]> { array[..2].try_into().ok() }

fn array_suffix(array: &[u16; 4]) -> Option<[u16; 2]> { array[array.len() - 2..].try_into().ok() }

fn generic<const N: usize>(slice: &[u8]) -> Option<[u8; N]> { slice.try_into().ok() }

fn generic_prefix<const r#N: usize>(array: &[u8; r#N]) -> Option<[u8; r#N]> { array[..r#N].try_into().ok() }

fn required(slice: &[u8]) -> Result<[u8; 2], TryFromSliceError> { slice.try_into() }

fn owned(bytes: Vec<u8>) -> Option<[u8; 4]> { <[u8; 4]>::try_from(bytes).ok() }

fn listed(slice: &[u8]) -> Option<Vec<u8>> { slice.try_into().ok() }

fn mismatched_prefix(slice: &[u8]) -> Option<[u8; 4]> { slice[..2].try_into().ok() }

fn mismatched_suffix(slice: &[u8]) -> Option<[u8; 4]> { slice[slice.len() - 2..].try_into().ok() }

fn dynamic_prefix(slice: &[u8], length: usize) -> Option<[u8; 4]> { slice[..length].try_into().ok() }

fn dereferenced(slice: &&[u8]) -> Option<[u8; 4]> { (*slice).try_into().ok() }

fn selected(left: &[u8], right: &[u8], flag: bool) -> Option<[u8; 4]> { (if flag { left } else { right }).try_into().ok() }

fn matched(left: &[u8], right: &[u8], flag: bool) -> Option<[u8; 4]> {
    (match flag {
        true => left,
        false => right,
    })
    .try_into()
    .ok()
}

fn qualified(slice: &[u8]) -> Option<[u8; 4]> { <&[u8] as TryInto<[u8; 4]>>::try_into(slice).ok() }

fn qualified_ok(slice: &[u8]) -> Option<[u8; 4]> { Result::ok(<[u8; 4]>::try_from(slice)) }

fn isolated(slice: &[u8]) -> bool { <&[u8; 4]>::try_from(slice).ok().is_some() }

fn projected(view: &Projected) -> Option<&[u8; 4]> { <&[u8; 4] as TryFrom<&[u8]>>::try_from(view).ok() }

fn projected_mut(view: &mut Projected) -> Option<[u8; 4]> { <[u8; 4] as TryFrom<&mut [u8]>>::try_from(view).ok() }

fn projected_suffix(view: &FieldView) -> Option<[u16; 2]> { view.0[view.0.len() - 2..].try_into().ok() }

fn overridden(array: &[u8; 4]) -> Option<[u8; 2]> { array[array.len() - 2..].try_into().ok() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let mut buffer = [1_u8, 2, 3, 4];
    black_box(&(viewed(&buffer).copied(), viewed_mut(&mut buffer).copied(), prefix(&buffer), suffix(&buffer), required(&buffer), owned(vec![1, 2, 3, 4]), listed(&buffer)));
    black_box((mismatched_prefix(&buffer), mismatched_suffix(&buffer), dynamic_prefix(&buffer, 2)));
    black_box((dereferenced(&buffer.as_slice()), selected(&buffer, &[], true), matched(&buffer, &[], false)));
    black_box(qualified(&buffer));
    black_box((qualified_ok(&buffer), isolated(&buffer), generic::<4>(&buffer), generic_prefix(&buffer)));
    black_box((array_prefix(&buffer), array_suffix(&[1, 2, 3, 4])));
    let mut view = Projected { bytes: buffer, reads: Cell::new(0) };
    black_box(projected(&view));
    black_box(projected_mut(&mut view));
    black_box((view.as_array::<4>(), view.reads.get()));
    let fields = FieldView { value: ([1, 2, 3, 4],), reads: Cell::new(0) };
    black_box((projected_suffix(&fields), fields.reads.get()));
    black_box(overridden(&buffer));
}
