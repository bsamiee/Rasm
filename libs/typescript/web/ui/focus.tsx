// --- [OPERATIONS] ----------------------------------------------------------------------

const focus = (id: string) => requestAnimationFrame(() => document.querySelector<HTMLElement>(`#${CSS.escape(id)}`)?.focus());

// --- [EXPORTS] -------------------------------------------------------------------------

export { focus };
