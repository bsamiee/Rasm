import { Manifest } from '@rasm/cep/manifest';
import npm from '#/package.json' with { type: 'json' };
import { cep } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, {
    main: npm.main,
    host: '@rasm/aftereffects/host',
    daemon: 'atelier',
    manifest: Manifest.make({ id: 'app.rasm.atelier.aftereffects', name: npm.description, version: npm.version, host: { app: 'AEFT', minVersion: '26.0' } }),
});
