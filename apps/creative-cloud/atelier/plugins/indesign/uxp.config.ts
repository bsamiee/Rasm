import { uxp } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, { host: { app: 'ID', minVersion: '18.5.0' }, entrypoints: [{ type: 'panel', id: 'typesetting', label: { default: 'Typesetting' } }] });
