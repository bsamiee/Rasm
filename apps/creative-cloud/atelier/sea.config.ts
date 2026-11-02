import { sea } from '../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default sea(import.meta.dirname, { main: 'main.ts', daemon: 'atelier', host: 'ATELIER_PATH' });
