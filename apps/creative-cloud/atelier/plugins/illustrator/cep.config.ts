import { cep } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, { host: '@rasm/illustrator/host', manifest: { host: { app: 'ILST', minVersion: '30.0' } } });
