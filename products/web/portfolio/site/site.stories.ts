import { Effect, Option, Schema } from 'effect';
import { expect, waitFor } from 'storybook/test';
import preview from '#/.storybook/preview';
import { Bootstrap } from '#/model/api';
import { AssetId, ImageAsset } from '#/model/asset';
import { CompositionId } from '#/model/placement';
import { Entry, EntryId, Portfolio } from '#/model/portfolio';
import { shell } from '#/site/shell.tsx';

// --- [CONSTANTS] -----------------------------------------------------------------------

const missing = ImageAsset.make({ id: AssetId.make('5e0d6c1b-2a3f-4b8e-9c7d-6f5a4b3c2d1e'), name: 'missing-cover.png', size: 1000, mime: 'image/png', width: 1200, height: 900 });
const library = EntryId.make('7f1c2a4e-3b5d-4c6e-8f90-1a2b3c4d5e6f');
const joints = EntryId.make('0a9b8c7d-6e5f-4a3b-9c2d-1e0f9a8b7c6d');
const sheets = [CompositionId.make('3c1f0e52-8a47-4d19-b6c3-2e5d7a9f1b04'), CompositionId.make('9d2e4b61-5f38-4c07-a1d4-6b8c0e2f3a15')] as const;
const stored = {
    session: 'signedOut',
    published: {
        assets: {},
        portfolio: { name: '', introduction: '   ', entries: [{ id: library, kind: 'project', title: '', year: ' 2025 ', location: '', description: '', role: '', credits: 'Structure by Rasm Engineering  ' }] },
    },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const played = (steps: readonly (() => unknown)[]) => Effect.runPromise(Effect.forEach(steps, (step) => Effect.promise(() => Promise.resolve(step())), { discard: true }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const meta = preview.meta({
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
                    Entry.make({ id: library, kind: 'project', title: 'Harbour Library', year: '2025', location: 'Bergen', description: 'A public library on a reclaimed quay.', role: 'Lead architect', credits: 'Structure by Rasm Engineering' }),
                    Entry.make({ id: joints, kind: 'study', title: 'Timber Joints', year: '2024', description: 'Full-scale studies of reversible joints.', role: 'Research' }),
                ],
            }),
        },
    },
});

const Visitor = meta.story({
    play: () =>
        played([() => waitFor(() => expect(document.title).toBe('Studio Rasm · Architecture')), () => expect(Array.from(document.head.querySelectorAll('meta[name="description"]'), (described) => described.getAttribute('content'))).toStrictEqual(['Housing, civic buildings, and studies of timber construction.'])]),
});
const SignedIn = meta.story({ args: { session: 'signedIn' } });
const Empty = meta.story({ args: { published: { assets: {}, portfolio: Portfolio.make({}) } } });
const StoredBlanks = meta.story({
    args: Option.getOrThrow(Schema.decodeUnknownOption(Bootstrap)(stored)),
    play: ({ canvas }) =>
        played([
            () => waitFor(() => expect(document.title).toBe('Portfolio · Architecture')),
            () => expect(Array.from(canvas.getByRole('region', { name: 'Untitled entry' }).querySelectorAll('.chapter-meta span, dt, dd'), ({ textContent }) => textContent)).toStrictEqual(['Project', '2025', 'Credits', 'Structure by Rasm Engineering']),
            () =>
                expect(Effect.runPromise(Schema.encodeEffect(Portfolio)(Portfolio.make({ name: ' Studio Rasm ', introduction: '  ', entries: [Entry.make({ id: joints, kind: 'study', title: 'Timber Joints ' })] })))).resolves.toStrictEqual({
                    name: 'Studio Rasm',
                    entries: [{ id: joints, kind: 'study', title: 'Timber Joints', compositions: [] }],
                }),
        ]),
});
const IndexJump = meta.story({
    play: ({ canvas, userEvent }) =>
        played([
            () => userEvent.click(canvas.getByRole('link', { name: 'Timber Joints' })),
            () => waitFor(() => expect(location.hash).toBe(`#project=${joints}`)),
            () => expect(canvas.getByRole('heading', { name: 'Timber Joints' })).toHaveFocus(),
            () => userEvent.click(canvas.getByRole('link', { name: 'Harbour Library' })),
            () => waitFor(() => expect(location.hash).toBe(`#project=${library}`)),
            () => navigation.back().finished,
            () => waitFor(() => expect(location.hash).toBe(`#project=${joints}`)),
            () => waitFor(() => expect(canvas.getByRole('link', { name: 'Timber Joints' })).toHaveAttribute('aria-current', 'location')),
        ]),
});
const CarouselControls = meta.story({
    args: {
        published: {
            assets: { [missing.id]: missing },
            portfolio: Portfolio.make({ name: 'Studio Rasm', entries: [Entry.make({ id: library, kind: 'project', title: 'Harbour Library', compositions: sheets.map((id) => ({ id, scale: 'full', align: 'center', items: [{ kind: 'image', asset: missing, role: 'drawing' }] })) })] }),
        },
    },
    play: ({ canvas, userEvent }) =>
        played([
            () => canvas.getByRole('button', { name: 'Overview' }).focus(),
            () => userEvent.keyboard('{ArrowRight}{ArrowRight}'),
            () => expect(canvas.getByRole('button', { name: 'Next composition' })).toHaveFocus(),
            () => expect(canvas.getByRole('toolbar', { name: 'Harbour Library composition controls' })).toContainElement(canvas.getByRole('button', { name: 'Next composition' })),
            () => userEvent.keyboard('{Enter}'),
            () => waitFor(() => expect(location.hash).toBe(`#project=${library}&composition=${sheets[1]}`)),
            () => expect(canvas.getByRole('button', { name: 'Next composition' })).toHaveAttribute('aria-disabled', 'true'),
        ]),
});
const UnavailableCover = meta.story({
    args: { published: { assets: { [missing.id]: missing }, portfolio: Portfolio.make({ name: 'Studio Rasm', entries: [Entry.make({ id: library, kind: 'project', title: 'Harbour Library', cover: { kind: 'image', asset: missing, role: 'drawing' } })] }) } },
    play: ({ canvas }) => {
        canvas.getByRole('heading', { name: 'Harbour Library' }).scrollIntoView({ block: 'center' });
        return expect(canvas.findByRole('link', { name: 'Open original image' }, { timeout: 10_000 })).resolves.toBeVisible();
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { CarouselControls, Empty, IndexJump, SignedIn, StoredBlanks, UnavailableCover, Visitor };
