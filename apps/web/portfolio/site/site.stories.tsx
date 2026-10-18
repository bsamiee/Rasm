import preview from '../.storybook/preview.tsx';
import { Site } from './site.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const meta = preview.meta({
    component: Site,
    args: {
        session: { kind: 'visitor', signedIn: false },
        initial: {
            assets: {},
            portfolio: {
                name: 'Studio Rasm',
                introduction: 'Housing, civic buildings, and studies of timber construction.',
                email: 'studio@example.com',
                entries: [
                    { id: '7f1c2a4e-3b5d-4c6e-8f90-1a2b3c4d5e6f', kind: 'project', title: 'Harbour Library', year: '2025', location: 'Bergen', description: 'A public library on a reclaimed quay.', role: 'Lead architect', credits: 'Structure by Rasm Engineering', compositions: [] },
                    { id: '0a9b8c7d-6e5f-4a3b-9c2d-1e0f9a8b7c6d', kind: 'study', title: 'Timber Joints', year: '2024', location: '', description: 'Full-scale studies of reversible joints.', role: 'Research', credits: '', compositions: [] },
                ],
            },
        },
    },
});

const Visitor = meta.story();
const SignedIn = meta.story({ args: { session: { kind: 'visitor', signedIn: true } } });
const Empty = meta.story({ args: { initial: { assets: {}, portfolio: { name: '', introduction: '', email: '', entries: [] } } } });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Empty, SignedIn, Visitor };
