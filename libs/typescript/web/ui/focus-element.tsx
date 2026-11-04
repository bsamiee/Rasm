// --- [OPERATIONS] ----------------------------------------------------------------------

const focusElement = (id: string, options?: FocusOptions) => requestAnimationFrame(() => document.getElementById(id)?.focus(options));

// --- [EXPORTS] -------------------------------------------------------------------------

export { focusElement };
