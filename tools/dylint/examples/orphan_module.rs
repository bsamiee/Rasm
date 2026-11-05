use core::hint::black_box;

#[path = "orphan_module/declared.rs"]
mod declared;

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(declared::counted()); }
