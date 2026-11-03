// --- [OPERATIONS] ----------------------------------------------------------------------

const focus = (id: string, options?: FocusOptions) => requestAnimationFrame(() => document.getElementById(id)?.focus(options));

// --- [EXPORTS] -------------------------------------------------------------------------

export { focus };
