// --- [CONSTANTS] -----------------------------------------------------------------------

pub const FILE_DESCRIPTOR_SET: &[u8] = include_bytes!(concat!(env!("OUT_DIR"), "/", env!("CARGO_PKG_NAME"), ".bin"));

// --- [MODELS] --------------------------------------------------------------------------

tonic::include_proto!("rasm.store");
