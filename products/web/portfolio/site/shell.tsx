import { RegistryProvider } from '@effect/atom-react';
import { Shell } from '@rasm/hosting/shell';
import { Bootstrap } from '#/model/api';
import { Site } from '#/site/site.tsx';
import { bootstrap } from '#/site/state';

// --- [COMPOSITION] ---------------------------------------------------------------------

const shell = new Shell({
    name: 'portfolio',
    schema: Bootstrap,
    render: (value) => (
        <RegistryProvider initialValues={[[bootstrap, value]]}>
            <Site />
        </RegistryProvider>
    ),
    ...(import.meta.env.PROD && {
        policy: {
            'default-src': ["'self'"],
            'connect-src': ["'self'"],
            'object-src': ["'none'"],
            'base-uri': ["'self'"],
            'form-action': ["'self'"],
            'frame-ancestors': ["'none'"],
            'script-src': ["'self'", "'wasm-unsafe-eval'"],
            'style-src': ["'self'", "'unsafe-inline'"],
            'img-src': ["'self'", 'data:', 'blob:'],
            'media-src': ["'self'", 'blob:'],
            'worker-src': ["'self'", 'blob:'],
        },
    }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { shell };
