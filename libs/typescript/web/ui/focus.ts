// --- [OPERATIONS] ----------------------------------------------------------------------

const focus = (id: string) => requestAnimationFrame(() => document.getElementById(id)?.focus());

// --- [EXPORTS] -------------------------------------------------------------------------

export { focus };
