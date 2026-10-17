// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Settings {
    pub name: String,
    pub port: u16,
}

#[derive(Debug)]
pub struct Exposed<'settings> {
    pub settings: &'settings Settings,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub const fn port(exposed: &Exposed<'_>) -> u16 { exposed.settings.port }
