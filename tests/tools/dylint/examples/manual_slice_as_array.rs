use core::array::TryFromSliceError;
use core::cell::Cell;
use core::hint::black_box;
use core::ops::Deref;

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

// --- [OPERATIONS] ----------------------------------------------------------------------

fn viewed(slice: &[u8]) -> Option<&[u8; 4]> { <&[u8; 4]>::try_from(slice).ok() } //~ manual_slice_as_array `<[_]>::as_array::<4>(slice)`

fn viewed_mut(slice: &mut [u8]) -> Option<&mut [u8; 2]> { slice.try_into().ok() } //~ manual_slice_as_array `<[_]>::as_mut_array::<2>(slice)`

fn prefix(slice: &[u8]) -> Option<[u8; 2]> { slice[..2].try_into().ok() } //~ manual_slice_as_array `<[_]>::as_array::<2>(&slice[..2]).copied()`

fn suffix(slice: &[u8]) -> Option<[u8; 2]> { slice[slice.len() - 2..].try_into().ok() } //~ manual_slice_as_array `<[_]>::as_array::<2>(&slice[slice.len() - 2..]).copied()`

fn array_prefix(array: &[u8; 4]) -> Option<[u8; 2]> { array[..2].try_into().ok() } //~ manual_slice_as_array `<[_]>::first_chunk::<2>(array).copied()`

fn array_suffix(array: &[u16; 4]) -> Option<[u16; 2]> { array[array.len() - 2..].try_into().ok() } //~ manual_slice_as_array `<[_]>::last_chunk::<2>(array).copied()`

fn generic<const N: usize>(slice: &[u8]) -> Option<[u8; N]> { slice.try_into().ok() } //~ manual_slice_as_array `<[_]>::as_array::<N>(slice).copied()`

fn generic_prefix<const r#N: usize>(array: &[u8; r#N]) -> Option<[u8; r#N]> { array[..r#N].try_into().ok() } //~ manual_slice_as_array `<[_]>::first_chunk::<r#N>(array).copied()`

fn required(slice: &[u8]) -> Result<u16, TryFromSliceError> {
    let pair: [u8; 2] = slice.try_into()?; //~ manual_slice_as_array call `<[T]>::as_array::<2>` and handle `None`
    Ok(u16::from_le_bytes(pair))
}

fn owned(bytes: Vec<u8>) -> Option<[u8; 4]> { <[u8; 4]>::try_from(bytes).ok() }

fn listed(slice: &[u8]) -> Option<Vec<u8>> { slice.try_into().ok() }

fn projected(view: &Projected) -> Option<&[u8; 4]> { <&[u8; 4] as TryFrom<&[u8]>>::try_from(view).ok() } //~ manual_slice_as_array `<[_]>::as_array::<4>(view)`

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let mut buffer = [1_u8, 2, 3, 4];
    black_box(&(viewed(&buffer).copied(), viewed_mut(&mut buffer).copied(), prefix(&buffer), suffix(&buffer), required(&buffer), owned(vec![1, 2, 3, 4]), listed(&buffer)));
    black_box((array_prefix(&buffer), array_suffix(&[1, 2, 3, 4]), generic::<4>(&buffer), generic_prefix(&buffer)));
    let view = Projected { bytes: buffer, reads: Cell::new(0) };
    black_box((projected(&view), view.reads.get()));
}
