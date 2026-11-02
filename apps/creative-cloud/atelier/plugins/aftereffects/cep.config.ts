import npm from '#/package.json' with { type: 'json' };
import { cep } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, {
    host: '@rasm/aftereffects/host',
    daemon: 'atelier',
    npm,
    manifest: { host: { app: 'AEFT', minVersion: '26.0' } },
});
