import npm from '#/package.json' with { type: 'json' };
import { cep } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, {
    host: '@rasm/illustrator/host',
    daemon: 'atelier',
    npm,
    manifest: { host: { app: 'ILST', minVersion: '30.0' } },
});
