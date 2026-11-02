import { Manifest } from '@rasm/cep/manifest';
import npm from '#/package.json' with { type: 'json' };
import { cep } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, {
    host: '@rasm/illustrator/host',
    daemon: 'atelier',
    manifest: Manifest.make({ id: 'app.rasm.atelier.illustrator', name: npm.description, version: npm.version, host: { app: 'ILST', minVersion: '30.0' } }),
});
