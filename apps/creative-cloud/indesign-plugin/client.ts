// --- [IMPORTS] -------------------------------------------------------------------------

import { Application, app, DocumentEvent, type Event } from 'adobe:indesign';
import { host, versions } from 'adobe:uxp';
import { type Client, type Handler, handle, handler, type Settled, settle } from '@rasm/creative-cloud-server/client';
import type { Identity, Job, State } from '@rasm/creative-cloud-server/frames';
import { Bodies, type Kind, Results } from '@rasm/creative-cloud-server/indesign/jobs';
import { Array, Effect, Option, Stream, Struct } from 'effect';
import { active, scripting } from './host.ts';
import { editDocument } from './jobs/editorial.ts';
import { execute } from './jobs/execute.ts';
import { findKeyStrings } from './jobs/find-key-strings.ts';
import { fontSources } from './jobs/font-sources.ts';
import { getLayout } from './jobs/get-layout.ts';
import { applyGrid } from './jobs/grid.ts';
import { importStyles, placeLibraryAsset, placeSnippet, readLibrary, setExportTags, setLayers, writeLibrary, writeSnippet } from './jobs/library.ts';
import { listEnums } from './jobs/list-enums.ts';
import { getPreferences, setPreferences, setTextDefaults } from './jobs/preferences.ts';
import { publishing } from './jobs/publishing.ts';
import { snapshot } from './jobs/snapshot.ts';
import { buildTypography } from './jobs/templates.ts';
import { bridge } from './uxp.config.ts';

// --- [JOBS] ----------------------------------------------------------------------------

const _jobs: { readonly [K in Exclude<Kind, 'execute'>]: Handler } = {
    applyGrid: handler(Bodies.fields.applyGrid, Results.fields.applyGrid, applyGrid),
    editDocument: handler(Bodies.fields.editDocument, Results.fields.editDocument, editDocument),
    fontSources: handler(Bodies.fields.fontSources, Results.fields.fontSources, fontSources),
    importStyles: handler(Bodies.fields.importStyles, Results.fields.importStyles, importStyles),
    placeLibraryAsset: handler(Bodies.fields.placeLibraryAsset, Results.fields.placeLibraryAsset, placeLibraryAsset),
    placeSnippet: handler(Bodies.fields.placeSnippet, Results.fields.placeSnippet, placeSnippet),
    publishing: handler(Bodies.fields.publishing, Results.fields.publishing, publishing),
    readLibrary: handler(Bodies.fields.readLibrary, Results.fields.readLibrary, readLibrary),
    setExportTags: handler(Bodies.fields.setExportTags, Results.fields.setExportTags, setExportTags),
    setLayers: handler(Bodies.fields.setLayers, Results.fields.setLayers, setLayers),
    writeLibrary: handler(Bodies.fields.writeLibrary, Results.fields.writeLibrary, writeLibrary),
    writeSnippet: handler(Bodies.fields.writeSnippet, Results.fields.writeSnippet, writeSnippet),
    buildTypography: handler(Bodies.fields.buildTypography, Results.fields.buildTypography, buildTypography),
    listEnums: handler(Bodies.fields.listEnums, Results.fields.listEnums, listEnums),
    snapshot: handler(Bodies.fields.snapshot, Results.fields.snapshot, snapshot),
    getLayout: handler(Bodies.fields.getLayout, Results.fields.getLayout, getLayout),
    findKeyStrings: handler(Bodies.fields.findKeyStrings, Results.fields.findKeyStrings, findKeyStrings),
    getPreferences: handler(Bodies.fields.getPreferences, Results.fields.getPreferences, getPreferences),
    setPreferences: handler(Bodies.fields.setPreferences, Results.fields.setPreferences, setPreferences),
    setTextDefaults: handler(Bodies.fields.setTextDefaults, Results.fields.setTextDefaults, setTextDefaults),
};

// --- [HOST] ----------------------------------------------------------------------------

const _identity = (): Identity => ({
    plugin: bridge.manifest.id,
    version: versions.plugin,
    host: { name: host.name, version: host.version },
    uxp: versions.uxp,
    app: Option.some(app.version),
    dom: Option.some(app.scriptPreferences.version),
});

const _state = (): State => {
    const open = active();
    return { modalState: app.modalState, activeDocumentId: Option.map(open, Struct.get('id')), modified: Option.exists(open, Struct.get('modified')) };
};

const _states: Stream.Stream<State> = Stream.merge(
    Stream.fromEffect(Effect.sync(_state)),
    Stream.mergeAll(
        Array.map(
            [
                Application.AFTER_CONTEXT_CHANGED,
                Application.AFTER_SELECTION_CHANGED,
                Application.AFTER_SELECTION_ATTRIBUTE_CHANGED,
                DocumentEvent.AFTER_CLOSE,
                Application.AFTER_ACTIVATE,
                DocumentEvent.AFTER_OPEN,
                DocumentEvent.AFTER_NEW,
                DocumentEvent.AFTER_SAVE,
            ],
            (type) => Stream.fromEventListener<Event>(app, type),
        ),
        { concurrency: 'unbounded' },
    ).pipe(Stream.map(_state)),
);

const _perform = (job: Job): Effect.Effect<Settled> => (job.kind === 'execute' ? execute(job.body) : settle(scripting(handle(_jobs, job))));

// --- [CLIENT] --------------------------------------------------------------------------

const client: Client = { endpoint: bridge.endpoint, identity: _identity, states: _states, perform: _perform };

// --- [EXPORTS] -------------------------------------------------------------------------

export { client };
