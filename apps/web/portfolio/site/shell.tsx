import { RegistryProvider } from '@effect/atom-react';
import type { Shell } from '@rasm/hosting/page/render';
import { Option, String } from 'effect';
import { Bootstrap } from '#/model/bootstrap';
import { Site } from '#/site/site';

// --- [COMPOSITION] ---------------------------------------------------------------------

const shell = {
    root: 'portfolio',
    data: 'portfolio-data',
    schema: Bootstrap,
    render: (value: typeof Bootstrap.Type) => (
        <RegistryProvider>
            <Site {...value} />
        </RegistryProvider>
    ),
    head: ({ initial: { portfolio } }: typeof Bootstrap.Type) => ({ title: Option.map(Option.liftPredicate(portfolio.name, String.isNonEmpty), (name) => `${name} — Architecture`), meta: { description: Option.liftPredicate(portfolio.introduction, String.isNonEmpty) } }),
} as const satisfies Shell<typeof Bootstrap>;

// --- [EXPORTS] -------------------------------------------------------------------------

export { shell };
