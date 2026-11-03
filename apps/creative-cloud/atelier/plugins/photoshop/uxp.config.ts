import { uxp } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, { host: { app: 'PS', minVersion: '27.0.0' }, entrypoints: [] });
