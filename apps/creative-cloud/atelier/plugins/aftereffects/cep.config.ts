import { cep } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, { host: '@rasm/aftereffects/host', manifest: { host: { app: 'AEFT', minVersion: '26.0' } } });
