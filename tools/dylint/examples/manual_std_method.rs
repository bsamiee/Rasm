use core::{hint::black_box, mem::ManuallyDrop};
use std::process::{ExitStatus, Output};

// --- [MODELS] --------------------------------------------------------------------------

struct Guard<F>(Option<F>)
where F: FnOnce();

impl<F> Drop for Guard<F>
where F: FnOnce()
{
    fn drop(&mut self) {
        if let Some(run) = self.0.take() {
            run();
        }
    }
}

struct Deferred<F>(F)
where F: FnMut();

impl<F> Drop for Deferred<F>
where F: FnMut()
{
    fn drop(&mut self) { (self.0)(); }
}

struct Draining<F>
where F: FnMut(u8) -> bool
{
    keep: F,
    items: Vec<u8>,
}

impl<F> Drop for Draining<F>
where F: FnMut(u8) -> bool
{
    fn drop(&mut self) { self.items.retain(|&item| (self.keep)(item)); }
}

struct Manual<F>(ManuallyDrop<F>)
where F: FnMut();

impl<F> Drop for Manual<F>
where F: FnMut()
{
    fn drop(&mut self) { (*self.0)(); }
}

struct Buffer {
    bytes: Vec<u8>,
}

impl Drop for Buffer {
    fn drop(&mut self) { self.bytes.clear(); }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn flipped(flag: &mut bool) { *flag = !*flag; }

fn xored(flag: &mut bool) { *flag ^= true; }

fn squares(count: usize) -> Vec<usize> { (0..count).map(|index| index.strict_mul(index)).collect::<Vec<_>>() }

fn labels(count: usize) -> Vec<String> { (0..count).map(|index| index.to_string()).collect() }

fn viewed(slice: &[u8]) -> Option<&[u8; 4]> { <&[u8; 4]>::try_from(slice).ok() }

fn viewed_mut(slice: &mut [u8]) -> Option<&mut [u8; 2]> { slice.try_into().ok() }

fn prefix(slice: &[u8]) -> Option<[u8; 2]> { slice[..2].try_into().ok() }

fn suffix(slice: &[u8]) -> Option<[u8; 2]> { slice[slice.len() - 2..].try_into().ok() }

fn decoded(bytes: Vec<u8>) -> String { String::from_utf8_lossy(&bytes).into_owned() }

fn decoded_output(output: Output) -> String { String::from_utf8_lossy(&output.stdout).into_owned() }

fn inverted(bits: &mut u32) { *bits = !*bits; }

fn copied(flag: &mut bool, other: bool) { *flag = !other; }

fn offset(count: usize) -> Vec<usize> { (1..count).map(|index| index.strict_mul(2)).collect() }

fn zeroes(count: usize) -> Vec<u8> { (0..count).map(|_| 0_u8).collect() }

fn narrow(count: u32) -> Vec<u32> { (0..count).map(|index| index.strict_mul(2)).collect() }

fn owned(bytes: Vec<u8>) -> Option<[u8; 4]> { <[u8; 4]>::try_from(bytes).ok() }

fn reused(bytes: Vec<u8>) -> (String, usize) { (String::from_utf8_lossy(&bytes).into_owned(), bytes.len()) }

fn lent(bytes: &[u8]) -> String { String::from_utf8_lossy(bytes).into_owned() }

fn captured(bytes: Vec<u8>) -> String {
    let decode = || String::from_utf8_lossy(&bytes).into_owned();
    decode()
}

fn drained(buffer: Buffer) -> String { String::from_utf8_lossy(&buffer.bytes).into_owned() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (mut flag, mut bits, mut buffer) = (true, 1_u32, [1_u8, 2, 3, 4]);
    flipped(&mut flag);
    xored(&mut flag);
    inverted(&mut bits);
    copied(&mut flag, false);
    drop(Guard(Some(|| ())));
    drop(Deferred(|| ()));
    drop(Draining { keep: |item: u8| item > 1, items: vec![1, 2] });
    drop(Manual(ManuallyDrop::new(|| ())));
    black_box((
        (squares(3), labels(2), viewed(&buffer).copied(), viewed_mut(&mut buffer).copied(), prefix(&buffer), suffix(&buffer)),
        (decoded(vec![104]), decoded_output(Output { status: ExitStatus::default(), stdout: vec![105], stderr: Vec::new() }), offset(3), zeroes(2), narrow(2)),
        (owned(vec![1, 2, 3, 4]), reused(vec![106]), lent(&[107]), flag, bits),
        (captured(vec![108]), drained(Buffer { bytes: vec![109] })),
    ));
}
