// --- [IMPORTS] -------------------------------------------------------------------------

import { App, applyDocumentTheme, applyHostStyleVariables, type McpUiHostContext } from '@modelcontextprotocol/ext-apps';
import Panzoom from '@panzoom/panzoom';
import { Array, Effect, flow, identity, Option, Schema, Struct } from 'effect';
import manifest from '../package.json' with { type: 'json' };
import { Bodies } from './jobs.ts';

// --- [PRESENTATION] --------------------------------------------------------------------

const output = document.createElement('output');
output.setAttribute('aria-live', 'polite');
output.value = 'Waiting for snapshot…';
document.title = 'InDesign snapshot';
document.documentElement.lang = 'en';
Object.assign(document.body.style, { margin: '0', display: 'grid', gap: '0.5em', fontFamily: 'system-ui', color: 'CanvasText', backgroundColor: 'Canvas' });

Effect.gen(function* () {
    const controller = yield* Effect.acquireRelease(
        Effect.sync(() => new AbortController()),
        (value) => Effect.sync(() => value.abort()),
    );
    const { signal } = controller;
    const app = yield* Effect.acquireRelease(
        Effect.sync(() => new App({ name: document.title, version: manifest.version }, {})),
        (value) => Effect.promise(() => value.close()),
    );
    const figure = document.createElement('figure');
    figure.inert = true;
    Object.assign(figure.style, { display: 'flex', alignItems: 'center', justifyContent: 'center', minWidth: '0', margin: '0' });
    const image = document.createElement('img');
    image.style.flexShrink = '0';
    const navigation = document.createElement('fieldset');
    Object.assign(navigation.style, { display: 'flex', flexWrap: 'wrap', gap: document.body.style.gap, margin: '0', padding: '0', border: '0' });
    navigation.setAttribute('aria-label', 'Snapshot view');
    navigation.disabled = true;
    image.alt = document.title;
    const caption = document.createElement('legend');
    caption.textContent = image.alt;
    navigation.append(caption);
    figure.append(image);
    document.body.append(navigation, figure, output);
    const camera = yield* Effect.acquireRelease(
        Effect.sync(() => Panzoom(image, { canvas: true, animate: false })),
        (value) => Effect.sync(() => value.destroy()),
    );
    const percentage = new Intl.NumberFormat(undefined, { style: 'percent', maximumFractionDigits: 0 });
    const fitImage = (): void => {
        if (image.naturalWidth === 0) {
            return;
        }
        const dimensions = app.getHostContext()?.containerDimensions;
        const maximum = dimensions && 'height' in dimensions ? dimensions.height : dimensions?.maxHeight;
        const chrome = document.body.scrollHeight - figure.clientHeight;
        figure.style.height = `${Math.max(0, Math.min(image.naturalHeight, (maximum ?? figure.clientWidth + chrome) - chrome))}px`;
        if (figure.clientWidth === 0 || figure.clientHeight === 0) {
            return;
        }
        const scale = Math.min(1, figure.clientWidth / image.naturalWidth, figure.clientHeight / image.naturalHeight);
        camera.setOptions({ minScale: scale, maxScale: Math.max(1, Math.min(figure.clientWidth, figure.clientHeight)), startScale: scale });
        camera.reset({ animate: false });
    };
    navigation.append(
        ...Array.map(
            [
                { label: 'Fit', action: fitImage },
                { label: 'Zoom out', action: () => camera.zoomOut({ animate: false }) },
                { label: 'Zoom in', action: () => camera.zoomIn({ animate: false }) },
                { label: '1:1', action: () => camera.reset({ startScale: 1, animate: false }) },
            ],
            ({ label, action }) => {
                const button = document.createElement('button');
                button.type = 'button';
                button.textContent = label;
                button.style.font = 'inherit';
                button.addEventListener('click', action, { signal });
                return button;
            },
        ),
    );
    figure.addEventListener('wheel', camera.zoomWithWheel, { passive: false, signal });
    image.addEventListener(
        'panzoomchange',
        () => {
            output.value = percentage.format(camera.getScale());
        },
        { signal },
    );
    image.addEventListener(
        'load',
        () => {
            navigation.disabled = false;
            figure.inert = false;
            fitImage();
        },
        { signal },
    );
    image.addEventListener(
        'error',
        () => {
            navigation.disabled = true;
            figure.inert = true;
            output.value = 'The snapshot image could not be decoded.';
        },
        { signal },
    );
    const observer = yield* Effect.acquireRelease(
        Effect.sync(() => new ResizeObserver(fitImage)),
        (value) => Effect.sync(() => value.disconnect()),
    );
    observer.observe(figure);
    const style = (context: McpUiHostContext): void => {
        if (context.theme) {
            applyDocumentTheme(context.theme);
        }
        if (context.styles?.variables) {
            applyHostStyleVariables(context.styles.variables);
        }
        fitImage();
    };
    app.addEventListener('hostcontextchanged', style);
    app.addEventListener(
        'toolinput',
        flow(
            Struct.get('arguments'),
            Schema.decodeUnknownOption(Schema.Struct({ target: Bodies.fields.snapshot.fields.target })),
            Option.match({
                onNone: () => 'Snapshot target unavailable.',
                onSome: ({ target }) => ('index' in target ? `${target.kind} index ${target.index}` : `${target.kind} ID ${target.itemId}`),
            }),
            (text) => {
                caption.textContent = text;
                image.alt = text;
            },
        ),
    );
    app.addEventListener('toolresult', (result) => {
        const content = result.content?.find((block) => block.type === 'image');
        navigation.disabled = true;
        figure.inert = true;
        if (result.isError || !content) {
            image.removeAttribute('src');
            output.value = 'The snapshot could not be rendered.';
            return;
        }
        output.value = 'Loading snapshot…';
        image.src = `data:${content.mimeType};base64,${content.data}`;
    });
    app.addEventListener('toolcancelled', () => {
        output.value = 'Snapshot cancelled.';
    });
    yield* Effect.tryPromise({ try: () => app.connect(), catch: identity });
    Option.map(Option.fromNullishOr(app.getHostContext()), style);
    yield* Effect.callback<void>((resume) => {
        window.addEventListener('pagehide', () => resume(Effect.void), { once: true, signal });
    });
}).pipe(
    Effect.scoped,
    Effect.catchCause(() =>
        Effect.sync(() => {
            output.value = 'The snapshot viewer could not connect.';
        }),
    ),
    Effect.runFork,
);
