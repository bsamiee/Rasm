// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Settings {
    pub cache_dir: String,
    label_path: String,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn opened(log_file: &str, settings: &Settings) -> usize { log_file.len().saturating_add(settings.label_path.len()) }
