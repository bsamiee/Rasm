// --- [OPERATIONS] ----------------------------------------------------------------------

const focus = (id: string): number => requestAnimationFrame(() => document.querySelector<HTMLElement>(`#${CSS.escape(id)}`)?.focus());

// --- [EXPORTS] -------------------------------------------------------------------------

export { focus };
