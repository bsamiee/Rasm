import type { Page } from '@playwright/test';
import { type Extension, hosted, media } from '@rasm/testing/playwright';
import { Effect } from 'effect';
import { isolate, STUB } from '#/fixtures.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const JOURNAL = {
    url: 'https://journal.example/instruments/plectrum',
    head: `<!doctype html><html lang="en"><title>The plectrum, a short history</title><link rel="stylesheet" href="${hosted(import.meta.resolve('@fontsource-variable/source-serif-4'))}"><link rel="stylesheet" href="${hosted(import.meta.resolve('@fontsource-variable/inter'))}"><style>:root { color-scheme: light; background: #fbfaf7; color: #3b372e; font: 15px / 1.65 'Source Serif 4 Variable' } body { margin: 0 } nav { display: flex; justify-content: space-between; padding: 14px 28px; border-bottom: 1px solid #e7e2d6; font: 600 12px / 1 'Inter Variable'; letter-spacing: 0.06em; text-transform: uppercase; color: #8a7f6a } article { max-width: 470px; margin: 0 auto; padding: 72px 26px 0 } small { font: 600 11px / 1 'Inter Variable'; letter-spacing: 0.08em; text-transform: uppercase; color: #8a7f6a } h1 { margin: 10px 0 14px; color: #1d1b16; font-size: 30px; line-height: 1.15; font-weight: 600 } p { margin: 0 0 12px } form { margin-top: 18px; border: 1px solid #e7e2d6; border-radius: 10px; background: white; font: 13px / 1.5 'Inter Variable'; color: #22211f } label { display: flex; gap: 10px; padding: 9px 14px; border-bottom: 1px solid #efebe2; color: #8a7f6a } label span { color: #22211f } [contenteditable] { min-height: 46px; padding: 10px 14px; outline: none; white-space: pre-wrap }</style><nav><span>The Journal</span><span>Instruments</span></nav>`,
    lede: '<small>Instruments</small><h1>The plectrum, a short history</h1><p>From the eagle quill of the medieval oud to the tortoiseshell picks of the baroque guitar, players have struck strings with whatever sounded one clean note.</p>',
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const shareable = (stage: { readonly page: Page; readonly extension: Extension }) =>
    Effect.runPromise(
        Effect.andThen(
            isolate(stage, STUB.offered),
            Effect.promise(() => stage.extension.act(stage.page)),
        ),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

media([
    { name: 'copy', shows: 'status', after: shareable, url: JOURNAL.url, body: `${JOURNAL.head}<article>${JOURNAL.lede}<p>The Persian mezrab still names that small striking tool, held between thumb and finger and drawn across the course in one motion.</p></article>` },
    {
        name: 'copy-dark',
        colorScheme: 'dark',
        shows: 'status',
        after: shareable,
        url: 'https://docs.example/guides/streams',
        body: `<!doctype html><html lang="en"><title>Streams</title><link rel="stylesheet" href="${hosted(import.meta.resolve('@fontsource-variable/inter'))}"><style>:root { color-scheme: light dark; background: Canvas; color: color-mix(in lab, CanvasText 82%, Canvas); font: 14px / 1.6 'Inter Variable' } body { display: grid; grid-template-columns: 150px 1fr; margin: 0; min-height: 100vh } aside { padding: 64px 18px; border-inline-end: 1px solid color-mix(in lab, CanvasText 12%, Canvas); font-size: 12.5px; line-height: 2.1; color: color-mix(in lab, CanvasText 55%, Canvas) } aside b { color: CanvasText; font-weight: 600 } main { padding: 64px 32px 0 } h1 { margin: 0 0 10px; color: CanvasText; font-size: 26px; font-weight: 650; letter-spacing: -0.01em }</style><aside><div>Overview</div><div>Install</div><div><b>Streams</b></div><div>Sinks</div><div>Schedules</div></aside><main><h1>Streams</h1><p>A stream describes values that arrive over time, with one failure channel and the services it reads.</p><p>Pull every value it holds into a collection, or fold it into a sink that keeps a running total.</p></main>`,
    },
    {
        name: 'paste',
        shows: 'status',
        url: JOURNAL.url,
        body: `${JOURNAL.head}<article>${JOURNAL.lede}<form aria-label="Email this article"><label>To <span>reading-group@example.com</span></label><div role="textbox" aria-label="Message" contenteditable>Worth a read before Thursday: </div></form></article>`,
        after: (stage) =>
            shareable(stage)
                .then(() => stage.page.getByRole('textbox', { name: 'Message' }).press('End'))
                .then(() => stage.page.keyboard.press('ControlOrMeta+V')),
    },
]);
