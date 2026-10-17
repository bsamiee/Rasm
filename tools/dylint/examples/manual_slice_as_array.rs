use core::{array::TryFromSliceError, hint::black_box};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn viewed(slice: &[u8]) -> Option<&[u8; 4]> { <&[u8; 4]>::try_from(slice).ok() }

fn viewed_mut(slice: &mut [u8]) -> Option<&mut [u8; 2]> { slice.try_into().ok() }

fn prefix(slice: &[u8]) -> Option<[u8; 2]> { slice[..2].try_into().ok() }

fn suffix(slice: &[u8]) -> Option<[u8; 2]> { slice[slice.len() - 2..].try_into().ok() }

fn required(slice: &[u8]) -> Result<[u8; 2], TryFromSliceError> { slice.try_into() }

fn owned(bytes: Vec<u8>) -> Option<[u8; 4]> { <[u8; 4]>::try_from(bytes).ok() }

fn listed(slice: &[u8]) -> Option<Vec<u8>> { slice.try_into().ok() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let mut buffer = [1_u8, 2, 3, 4];
    black_box(&(viewed(&buffer).copied(), viewed_mut(&mut buffer).copied(), prefix(&buffer), suffix(&buffer), required(&buffer), owned(vec![1, 2, 3, 4]), listed(&buffer)));
}
