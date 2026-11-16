import { RegistryProvider } from '@effect/atom-react';
import { Shell } from '@rasm/hosting/shell';
import { Option, String } from 'effect';
import { joinDetails } from '#/media/labels';
import { Bootstrap } from '#/model/api';
import { Site } from '#/site/site.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const shell = new Shell({
    root: 'portfolio',
    schema: Bootstrap,
    render: (value) => (
        <RegistryProvider>
            <Site {...value} />
        </RegistryProvider>
    ),
    head: ({ published: { portfolio } }) => ({ title: Option.map(Option.liftPredicate(portfolio.name, String.isNonEmpty), (name) => joinDetails([name, 'Architecture'])), meta: { name: { description: Option.liftPredicate(portfolio.introduction, String.isNonEmpty) }, property: {} } }),
    ...(import.meta.env.PROD && {
        policy: {
            'default-src': ["'self'"],
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
