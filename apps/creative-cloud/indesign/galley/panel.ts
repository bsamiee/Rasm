import '@rasm/uxp/runtime';
import { served } from '@rasm/indesign/panel';
import type { Link } from '@rasm/mcp/bridge';
import { HostClock } from '@rasm/uxp/runtime';
import { Effect, Layer } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STYLE = [
    'body { padding: 12px; margin: 0; font-family: adobe-clean, sans-serif; font-size: 12px; line-height: 1.4; }',
    'p { margin: 0 0 8px; }',
    '[data-state="closed"], [data-state="unreachable"], [data-state="done"] { opacity: 0.6; }',
    '[data-state="running"] { font-weight: bold; }',
    '@media (prefers-color-scheme: dark), (prefers-color-scheme: darkest) { body { color: rgb(232 232 232); } }',
    '@media (prefers-color-scheme: light), (prefers-color-scheme: lightest) { body { color: rgb(50 50 50); } }',
].join('\n');

// --- [COMPOSITION] ---------------------------------------------------------------------

const [status, activity] = [document.createElement('p'), document.createElement('p')];
const show = (element: HTMLElement, state: string, text: string): Effect.Effect<void> =>
    Effect.sync(() => {
        element.setAttribute('data-state', state);
        element.replaceChildren(text);
    });
document.head.append(Object.assign(document.createElement('style'), { textContent: STYLE }));
document.body.append(status, activity);
const origin: string = import.meta.env.ATELIER;
const sentence = { linked: 'Linked to Atelier', closed: 'Atelier closed, redialing', unreachable: `No Atelier at ${origin}, redialing` } as const satisfies Record<Link, string>;
Layer.launch(
    served(origin, {
        link: (link) => Effect.all([show(status, link, sentence[link]), show(activity, link, '')], { discard: true }),
        working: (description, work) =>
            Effect.acquireUseRelease(
                show(activity, 'running', description),
                () => work,
                () => show(activity, 'done', description),
            ),
    }),
).pipe(Effect.provide(HostClock), Effect.runFork);
