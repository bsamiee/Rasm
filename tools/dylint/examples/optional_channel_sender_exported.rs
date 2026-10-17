use tokio::sync::watch;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Exported {
    pub sent: Option<watch::Sender<u32>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn exported(exported: &Exported) {
    if let Some(sent) = &exported.sent {
        sent.send_replace(1);
    }
}
