import { RegistryProvider } from '@effect/atom-react';
import { Shell } from '@rasm/hosting/page/shell';
import { Option, String } from 'effect';
import { details } from '#/media/labels.ts';
import { Bootstrap } from '#/model/api.ts';
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
    head: ({ published: { portfolio } }) => ({ title: Option.map(Option.liftPredicate(portfolio.name, String.isNonEmpty), (name) => details([name, 'Architecture'])), meta: { name: { description: Option.liftPredicate(portfolio.introduction, String.isNonEmpty) }, property: {} } }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { shell };
