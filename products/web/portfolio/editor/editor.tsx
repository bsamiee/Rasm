import { useAtom, useAtomMount, useAtomRefresh, useAtomSet, useAtomSubscribe, useAtomValue } from '@effect/atom-react';
import { Issues, ModalDialog } from '@rasm/ui';
import { Array, Filter, Function, Match, Option, Predicate, Schema, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { AsyncResult, Atom } from 'effect/reactivity';
import { ArrowLeft, X } from 'lucide-react';
import { lazy, type RefObject, Suspense, useId, useRef } from 'react';
import { Button, Heading, Tab as TabButton, TabList, TabPanel, Tabs, Text, Toolbar } from 'react-aria-components';
import { flushSync } from 'react-dom';
import { Compositions } from '#/editor/compositions.tsx';
import { EntryFields, EntryList } from '#/editor/entries.tsx';
import { Library } from '#/editor/library.tsx';
import { PlaceFile } from '#/editor/place-file.tsx';
import { Profile } from '#/editor/profile.tsx';
import { RequestAlert } from '#/editor/request-alert.tsx';
import { busy, deleteRequest, draft, portfolio, reloadRequest, saveRequest, Tab, tab, Workspace, workspace } from '#/editor/state';
import { UploadRecovery } from '#/editor/upload-recovery.tsx';
import '#/editor/editor.css';
import { Destination, nextDestination, pendingUploads } from '#/editor/uploader';
import { confirmation, entryTitle, format } from '#/media/labels';
import type { Assets } from '#/model/asset';
import type { Composition } from '#/model/placement';
import { Entry, type Portfolio } from '#/model/portfolio';
import { preview } from '#/site/state';

// --- [SERVICES] ------------------------------------------------------------------------

const unsaved = Atom.make((get) => {
    const leaving = new AbortController();
    if (Option.exists(AsyncResult.value(get(draft)), ({ portfolio: edited, saved }) => edited !== saved) || get(pendingUploads).length > 0) {
        globalThis.addEventListener('beforeunload', (event) => event.preventDefault(), { signal: leaving.signal });
    }
    get.addFinalizer(() => leaving.abort());
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const Uploads = lazy(() => import('#/editor/uploads.tsx').then((module) => ({ default: module.Uploads })));
const DraftEditor = (state: Atom.Success<typeof draft>) => {
    const formId = useId();
    const uploadsId = useId();
    const publish = useRef<HTMLButtonElement>(null);
    const save = useRef<HTMLButtonElement>(null);
    const create = useRef<HTMLButtonElement>(null);
    const heading = useRef<HTMLHeadingElement>(null);
    const title = useRef<HTMLInputElement>(null);
    const composing = useRef<HTMLDivElement>(null);
    const form = useRef<HTMLFormElement>(null);
    const hero = useRef<HTMLHeadingElement>(null);
    const setDraft = useAtomSet(draft);
    const setTarget = useAtomSet(nextDestination);
    const setPreview = useAtomSet(preview);
    const pending = useAtomValue(pendingUploads);
    const deletion = useAtomValue(deleteRequest);
    const working = useAtomValue(busy);
    const [operation, submit] = useAtom(saveRequest);
    const [reloading, reload] = useAtom(reloadRequest);
    const [view, setView] = useAtom(workspace);
    const [shown, setTab] = useAtom(tab);
    useAtomSubscribe(reloadRequest, (result) => {
        if (AsyncResult.isSuccess(result) && !result.waiting) {
            save.current?.focus();
        }
    });
    const show = (next: Workspace, target: RefObject<HTMLElement | null>) => {
        flushSync(() => {
            setView(next);
            setTab('entries');
        });
        target.current?.focus();
    };
    const open = (entry: Entry['id'], composition = Option.none<Composition['id']>(), target: RefObject<HTMLElement | null> = heading) => {
        setTarget(Destination.Entry({ id: entry }));
        show(Workspace.Entry({ entry, composition }), target);
    };
    const place = (entry: Entry['id'], asset: Option.Option<keyof Assets['Service']>) => {
        setTarget(Destination.Entry({ id: entry }));
        show(Workspace.Place({ entry, asset }), heading);
    };
    const add = (kind: Entry['kind']) => {
        const created = Entry.make({ id: Entry.fields.id.make(SecureRandom.uuidV7Unsafe()), kind });
        setDraft(portfolio.key('entries').modify(Array.append(created)));
        open(created.id, Option.none(), title);
    };
    const enter =
        (snapshot: Portfolio, index: number, target: RefObject<HTMLElement | null>, item = Option.none<number>()) =>
        () =>
            Option.match(Array.get(snapshot.entries, index), {
                onNone: () => show(Workspace.Entries({ last: Option.none() }), create),
                onSome: (found) =>
                    open(
                        found.id,
                        Option.flatMap(item, (at) => Option.map(Array.get(found.compositions, at), Struct.get('id'))),
                        target,
                    ),
            });
    const describe = (field: PropertyKey | undefined) => () => {
        flushSync(() => setTab('profile'));
        const control = field === 'hero' ? hero.current : form.current?.elements.namedItem(String(field));
        if (control instanceof HTMLElement) {
            control.focus();
        }
    };
    const locate = (path: readonly PropertyKey[], snapshot: Portfolio) =>
        Match.value(path).pipe(
            Match.when(['entries', Match.number, 'compositions', Match.number], ([, index, , item]) => enter(snapshot, index, composing, Option.some(item))),
            Match.when(['entries', Match.number, 'title'], ([, index]) => enter(snapshot, index, title)),
            Match.when(['entries', Match.number], ([, index]) => enter(snapshot, index, heading)),
            Match.when(['entries'], () => () => show(Workspace.Entries({ last: Option.none() }), create)),
            Match.orElse(([field]) => describe(field)),
        );
    const status = Match.value({ reloading: reloading.waiting, saving: operation.waiting, changed: state.portfolio !== state.saved }).pipe(
        Match.when({ reloading: true }, () => 'Loading latest draft…'),
        Match.when({ saving: true }, () => 'Saving…'),
        Match.when({ changed: true }, () => 'Unsaved draft'),
        Match.orElse(() => Option.match(AsyncResult.value(operation), { onNone: () => 'Private draft', onSome: (saved) => (saved.publish ? 'Draft saved and published' : 'Draft saved. Published work unchanged') })),
    );
    const current = (id: Entry['id']) => Array.findFirst(state.portfolio.entries, (item) => item.id === id);
    const idle = { last: Option.none<Entry['id']>(), placing: undefined, entering: undefined };
    const { last, placing, entering } = Workspace.$match(view, {
        Entries: (listing) => ({ ...idle, last: listing.last }),
        Place: ({ entry, asset }) => ({ ...idle, placing: Option.getOrUndefined(Option.map(current(entry), (found) => ({ entry: found, asset }))) }),
        Entry: ({ entry, composition }) => ({ ...idle, entering: Option.getOrUndefined(Option.map(current(entry), (found) => ({ entry: found, composition }))) }),
    });
    return (
        <>
            <div className="draft-bar">
                <Toolbar aria-label="Draft">
                    <button className="button button-ghost" disabled={working} form={formId} ref={save} type="submit">
                        Save draft
                    </button>
                    <Button className="button button-ghost" isDisabled={working} onPress={() => setPreview(Option.some(Struct.pick(state, ['portfolio', 'assets'])))} slot="close">
                        Preview draft
                    </Button>
                    <button aria-describedby={pending.length > 0 ? uploadsId : undefined} className="button" disabled={working || pending.length > 0} form={formId} ref={publish} type="submit">
                        Publish
                    </button>
                </Toolbar>
                <output>{status}</output>
                {AsyncResult.builder(operation)
                    .onError(({ error, portfolio: submitted }) => (
                        <div>
                            <RequestAlert error={error} onField={(path) => locate(path, submitted)()} />
                            {Predicate.isTagged(error, 'PreconditionFailed') && (
                                <div className="actions">
                                    <ModalDialog.Confirm {...confirmation} description="Discards unsaved edits. Uploaded files stay in library. Edits stay if reload fails" isDisabled={working || pending.length > 0} label="Reload latest draft" onConfirm={() => reload(undefined)} />
                                    <ModalDialog.Confirm {...confirmation} description={`Saves these edits over draft revision ${error.revision}. Published work changes on next publish`} isDisabled={working} label="Replace newer draft" onConfirm={() => submit({ ...state, revision: error.revision, publish: false })} />
                                </div>
                            )}
                        </div>
                    ))
                    .orNull()}
                {AsyncResult.builder(reloading)
                    .onError((error) => <RequestAlert error={error} />)
                    .orNull()}
                {AsyncResult.builder(deletion)
                    .onError((error) => <RequestAlert conflict="Saved or published work uses file. Remove its placements and publish before deleting" error={error} />)
                    .orNull()}
                {pending.length > 0 && (
                    <p id={uploadsId}>
                        {format.quantity(pending.length, { one: (count) => `${count} file not uploaded`, other: (count) => `${count} files not uploaded` })}. Upload or remove before publishing or reloading{' '}
                        <button className="quiet-link" onClick={() => setTab('files')} type="button">
                            View uploads
                        </button>
                    </p>
                )}
            </div>
            <fieldset disabled={working}>
                <Tabs
                    isDisabled={working}
                    onSelectionChange={(key) => {
                        Option.match(Schema.decodeUnknownOption(Tab)(key), { onNone: Function.constVoid, onSome: setTab });
                    }}
                    selectedKey={shown}
                >
                    <TabList aria-label="Portfolio editing">
                        <TabButton id="entries">Projects and studies</TabButton>
                        <TabButton id="files">Files{pending.length > 0 && ` (${pending.length})`}</TabButton>
                        <TabButton id="profile">Profile and hero</TabButton>
                    </TabList>
                    <TabPanel id="entries">
                        {placing !== undefined && (
                            <>
                                <div className="panel-heading">
                                    <h3 ref={heading} tabIndex={-1}>
                                        Add to {entryTitle(placing.entry)}
                                    </h3>
                                    <button className="button button-ghost" onClick={() => open(placing.entry.id)} type="button">
                                        <ArrowLeft size={16} />
                                        Back to entry
                                    </button>
                                </div>
                                <PlaceFile assetId={placing.asset} assets={state.assets} entryId={placing.entry.id} key={Option.getOrUndefined(placing.asset)} onAdd={(composition) => open(placing.entry.id, Option.some(composition))} onSelect={(asset) => setView(Workspace.Place({ entry: placing.entry.id, asset }))} />
                            </>
                        )}
                        {entering !== undefined && (
                            <div className="entry-editor" key={entering.entry.id}>
                                <div className="panel-heading">
                                    <h3 ref={heading} tabIndex={-1}>
                                        {entryTitle(entering.entry)}
                                    </h3>
                                    <button className="button button-ghost" onClick={() => setView(Workspace.Entries({ last: Option.some(entering.entry.id) }))} type="button">
                                        Back to entries
                                    </button>
                                </div>
                                <EntryFields entry={entering.entry} title={title} />
                                <Compositions
                                    assets={state.assets}
                                    editor={composing}
                                    entry={entering.entry}
                                    onPlace={() => place(entering.entry.id, Option.none())}
                                    onSelect={(composition) => setView(Workspace.Entry({ entry: entering.entry.id, composition: Option.some(composition) }))}
                                    onUpload={() => {
                                        setTarget(Destination.Entry({ id: entering.entry.id }));
                                        setTab('files');
                                    }}
                                    selected={entering.composition}
                                />
                            </div>
                        )}
                        {placing === undefined && entering === undefined && (
                            <>
                                <div className="actions">
                                    <button className="button button-outline" onClick={() => add('project')} ref={create} type="button">
                                        New project
                                    </button>
                                    <button className="button button-ghost" onClick={() => add('study')} type="button">
                                        New study
                                    </button>
                                </div>
                                <EntryList entries={state.portfolio.entries} onRemove={() => create.current?.focus()} onSelect={(id) => open(id)} selected={last} />
                                {state.portfolio.entries.length === 0 && <p className="panel-empty hint">Create a project or study to add files</p>}
                            </>
                        )}
                    </TabPanel>
                    <TabPanel id="files">
                        <Suspense fallback={<output>Loading files…</output>}>
                            <Uploads assets={state.assets} entries={state.portfolio.entries} onPlace={(entry, asset) => place(entry, Option.some(asset))} />
                        </Suspense>
                        <UploadRecovery />
                        <Library assets={state.assets} onPlace={(entry, asset) => place(entry, Option.some(asset))} portfolio={state.portfolio} />
                    </TabPanel>
                    <TabPanel id="profile" shouldForceMount>
                        <form
                            id={formId}
                            onInvalidCapture={(event) => {
                                event.preventDefault();
                                flushSync(() => setTab('profile'));
                                event.currentTarget.querySelector<HTMLElement>(':invalid')?.focus();
                            }}
                            onSubmit={(event) => {
                                event.preventDefault();
                                submit({ ...state, publish: event.submitter === publish.current });
                            }}
                            ref={form}
                        >
                            <Issues.Fields issue={AsyncResult.error(operation).pipe(Option.map(Struct.get('error')), Option.filterMap(Filter.tagged('SchemaError')), Option.map(Struct.get('issue')))}>
                                <Profile
                                    assets={state.assets}
                                    hero={hero}
                                    onChooseHero={() => {
                                        setTarget(Destination.Hero());
                                        setTab('files');
                                    }}
                                    portfolio={state.portfolio}
                                />
                            </Issues.Fields>
                        </form>
                    </TabPanel>
                </Tabs>
            </fieldset>
        </>
    );
};
const Editor = () => {
    const state = useAtomValue(draft);
    const reload = useAtomRefresh(draft);
    const working = useAtomValue(busy);
    useAtomMount(unsaved);
    return (
        <ModalDialog dialog={{ className: 'editor' }} isDismissable={!working} isKeyboardDismissDisabled={working}>
            <header>
                <Heading slot="title">
                    Portfolio editor<span className="accent">.</span>
                </Heading>
                {AsyncResult.builder(state)
                    .onInitial(() => (
                        <Text elementType="p" slot="description">
                            Loading draft…
                        </Text>
                    ))
                    .onError(() => (
                        <Text elementType="p" slot="description">
                            Draft failed to load
                        </Text>
                    ))
                    .orNull()}
                <Button aria-label="Close portfolio editor" isDisabled={working} slot="close">
                    <X size={20} />
                </Button>
            </header>
            {AsyncResult.builder(state)
                .onError((error) => (
                    <div className="editor-failure">
                        <RequestAlert error={error} />
                        <button className="button" onClick={reload} type="button">
                            Try again
                        </button>
                    </div>
                ))
                .onSuccess((value) => <DraftEditor {...value} />)
                .orNull()}
        </ModalDialog>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Editor };
