use core::hint::black_box;

// --- [OPERATIONS] ----------------------------------------------------------------------

fn flipped(flag: &mut bool) { *flag = !*flag; }

fn xored(flag: &mut bool) { *flag ^= true; }

fn inverted(bits: &mut u32) { *bits = !*bits; }

fn copied(flag: &mut bool, other: bool) { *flag = !other; }

fn kept() -> bool {
    let mut flag = true;
    flag ^= false;
    flag
}

fn set(flag: &mut bool) { *flag |= true; }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let (mut flag, mut bits) = (true, 1_u32);
    flipped(&mut flag);
    xored(&mut flag);
    inverted(&mut bits);
    copied(&mut flag, false);
    set(&mut flag);
    black_box(&(flag, bits, kept()));
}
