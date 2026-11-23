#![no_std]
#![cfg_attr(dylint_lib = "rasm_lints", allow(manual_retain_back, manual_vec_from_fn, single_reader, reason = "these lints fire incidentally here and are exercised by their own fixtures"))]

extern crate std as host;

use core::hint::black_box;

use host::collections::VecDeque;
use host::path::Path;
use host::vec::Vec;

// --- [OPERATIONS] ----------------------------------------------------------------------

fn trim(recent: &mut VecDeque<u8>) {
    while recent.len() > 1 {
        recent.pop_front();
    }
}

fn values(count: usize) -> Vec<usize> { (0..count).map(|index| index.strict_mul(2)).collect() }

// --- [COMPOSITION] ---------------------------------------------------------------------

pub fn main() {
    let mut recent = VecDeque::from([1, 2, 3]);
    trim(&mut recent);
    black_box((Path::new("").as_os_str().is_empty(), recent, values(2))); //~ manual_path_is_empty
}
