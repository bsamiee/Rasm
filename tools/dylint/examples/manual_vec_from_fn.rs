use core::hint::black_box;
use std::collections::VecDeque;

// --- [OPERATIONS] ----------------------------------------------------------------------

fn squares(count: usize) -> Vec<usize> { (0..count).map(|index| index.strict_mul(index)).collect::<Vec<_>>() }

fn labels(count: usize) -> Vec<String> { (0..count).map(|index| index.to_string()).collect() }

fn offset(count: usize) -> Vec<usize> { (1..count).map(|index| index.strict_mul(2)).collect() }

fn zeroes(count: usize) -> Vec<u8> { (0..count).map(|_| 0_u8).collect() }

fn narrow(count: u32) -> Vec<u32> { (0..count).map(|index| index.strict_mul(2)).collect() }

fn queued(count: usize) -> VecDeque<usize> { (0..count).map(|index| index.strict_mul(2)).collect() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(squares(3), labels(2), offset(3), zeroes(2), narrow(2), queued(2))); }
