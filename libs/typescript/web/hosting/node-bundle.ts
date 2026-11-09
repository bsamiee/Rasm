// --- [CONSTANTS] -----------------------------------------------------------------------

const nodeBundle = { environment: 'node', entry: 'index', assets: 'assets', portVariable: 'PORT', readiness: '/readyz' } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { nodeBundle };
