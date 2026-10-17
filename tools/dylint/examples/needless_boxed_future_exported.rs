use core::pin::Pin;

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn exported(work: u64) -> Pin<Box<dyn Future<Output = u64> + Send>> { Box::pin(async move { work.strict_add(1) }) }
