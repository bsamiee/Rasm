import viteReact from '@vitejs/plugin-react';

// --- [COMPOSITION] ---------------------------------------------------------------------

const react = () => viteReact({ compiler: { panicThreshold: 'all_errors' } });

// --- [EXPORTS] -------------------------------------------------------------------------

export { react };
