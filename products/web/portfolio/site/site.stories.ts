import preview from '#/.storybook/preview.ts';
import { Entry, EntryId, Portfolio } from '#/model/portfolio.ts';
import { shell } from '#/site/shell.tsx';
import { Site } from '#/site/site.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const meta = preview.meta({
    component: Site,
    render: shell.render,
    args: {
        session: 'signedOut',
        published: {
            assets: {},
            portfolio: Portfolio.make({
                name: 'Studio Rasm',
                introduction: 'Housing, civic buildings, and studies of timber construction.',
                email: 'studio@example.com',
                entries: [
                    Entry.make({ id: EntryId.make('7f1c2a4e-3b5d-4c6e-8f90-1a2b3c4d5e6f'), kind: 'project', title: 'Harbour Library', year: '2025', location: 'Bergen', description: 'A public library on a reclaimed quay.', role: 'Lead architect', credits: 'Structure by Rasm Engineering' }),
                    Entry.make({ id: EntryId.make('0a9b8c7d-6e5f-4a3b-9c2d-1e0f9a8b7c6d'), kind: 'study', title: 'Timber Joints', year: '2024', description: 'Full-scale studies of reversible joints.', role: 'Research' }),
                ],
            }),
        },
    },
});

const Visitor = meta.story();
const SignedIn = meta.story({ args: { session: 'signedIn' } });
const Empty = meta.story({ args: { published: { assets: {}, portfolio: Portfolio.make({}) } } });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Empty, SignedIn, Visitor };
