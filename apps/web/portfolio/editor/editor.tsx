import { useAtom, useAtomRefresh, useAtomSet, useAtomSubscribe, useAtomValue } from '@effect/atom-react';
import { focus } from '@rasm/ui';
import { Array, Effect, Match, Option } from 'effect';
import { AsyncResult, type Atom } from 'effect/reactivity';
import { ArrowLeft, X } from 'lucide-react';
import { lazy, type ReactNode, Suspense, useEffect, useId, useRef, useState } from 'react';
import { Button, Dialog, Heading, type Key, Modal, ModalOverlay, Tab, TabList, TabPanel, Tabs, Text } from 'react-aria-components';
import { Compositions } from '#/editor/compositions';
import { ConfirmDialog } from '#/editor/confirm-dialog';
import { EntryFields, EntryList } from '#/editor/entries';
import { Identity } from '#/editor/identity';
import { Library } from '#/editor/library';
import { RequestAlert } from '#/editor/request-alert';
import { ReuseFile } from '#/editor/reuse-file';
import { deleteRequest, draft, identifier, portfolio, reloadRequest, saveRequest } from '#/editor/state';
import { UploadRecovery } from '#/editor/upload-recovery';
import { destination, pendingUploads } from '#/editor/uploader';
import { entryTitle, quantity } from '#/media/labels';
import { Entry, type Portfolio, type PortfolioData } from '#/model/portfolio';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Uploads = lazy(() => import('#/editor/uploads').then((module) => ({ default: module.Uploads })));
const EditorDialog = ({ description, busy, children }: { description?: string; busy: boolean; children?: ReactNode }) => (
    <ModalOverlay className="bg-foreground/55 fixed inset-0 z-50" isDismissable={!busy} isKeyboardDismissDisabled={busy}>
        <Modal>
            <Dialog className="bg-background shadow-panel fixed top-0 right-0 h-(--visual-viewport-height) w-full max-w-250 overflow-y-auto outline-none">
                <div className="relative px-9 pt-5 pr-18 pb-2 max-sm:px-5 max-sm:pr-15">
                    <Heading className="text-heading tracking-display max-sm:text-2xl" slot="title">
                        Portfolio editor<span className="text-accent">.</span>
                    </Heading>
                    {description === undefined ? null : (
                        <Text className="text-muted mt-2 text-sm" elementType="p" slot="description">
                            {description}
                        </Text>
                    )}
                    <Button aria-label="Close portfolio editor" className="absolute top-5 right-5 grid min-h-11 min-w-11 place-items-center" isDisabled={busy} slot="close">
                        <X className="size-5" />
                    </Button>
                </div>
                {children}
            </Dialog>
        </Modal>
    </ModalOverlay>
);
const DraftEditor = ({ published, onPreview, ...state }: Atom.Success<typeof draft> & { published: typeof Portfolio.Type; onPreview: (data: typeof PortfolioData.Type) => void }) => {
    const ids = useId();
    const publish = useRef<HTMLButtonElement>(null);
    const save = useRef<HTMLButtonElement>(null);
    const setDraft = useAtomSet(draft);
    const setTarget = useAtomSet(destination);
    const pending = useAtomValue(pendingUploads);
    const deletion = useAtomValue(deleteRequest);
    const [operation, submit] = useAtom(saveRequest);
    const [reloading, reload] = useAtom(reloadRequest);
    const [selected, setSelected] = useState<string>();
    const [workspace, setWorkspace] = useState<'projects' | 'entry' | 'sources'>('projects');
    const [composition, setComposition] = useState<string>();
    const [source, setSource] = useState(Option.none<string>());
    const [tab, setTab] = useState<Key>(`${ids}entries`);
    useEffect(() => {
        if (AsyncResult.isSuccess(reloading) && !reloading.waiting) {
            save.current?.focus();
        }
    }, [reloading]);
    const busy = operation.waiting || deletion.waiting || reloading.waiting;
    const current = state.portfolio.entries.find((item) => item.id === selected);
    const show = (name: 'entries' | 'files' | 'identity') => setTab(`${ids}${name}`);
    const openEntry = (id: string) => {
        setSelected(id);
        setWorkspace('entry');
        focus(`${ids}entry`);
    };
    const chooseSource = (entryId: string, assetId: string) => {
        setSelected(entryId);
        setTarget({ kind: 'entry', id: entryId });
        setSource(Option.some(assetId));
        setWorkspace('sources');
        show('entries');
        focus(`${ids}source`);
    };
    const create = (kind: typeof Entry.Type.kind) => {
        const created = Entry.make({ id: Effect.runSync(identifier), kind });
        setDraft(AsyncResult.map(portfolio.key('entries').modify(Array.append(created))));
        setSelected(created.id);
        setWorkspace('entry');
        setTarget({ kind: 'entry', id: created.id });
        focus(`${ids}title`);
    };
    const locate = (path: readonly PropertyKey[], snapshot: typeof Portfolio.Type) =>
        Match.value({ head: path[0], entry: snapshot.entries.find((_, index) => index === path[1]) }).pipe(
            Match.when({ head: 'email' }, () => {
                show('identity');
                focus(`${ids}email`);
            }),
            Match.when({ head: 'entries', entry: Match.defined }, ({ entry }) => {
                show('entries');
                openEntry(entry.id);
            }),
            Match.orElse(() => show('files')),
        );
    const status = Match.value({ reloading: reloading.waiting, saving: operation.waiting, changed: state.portfolio !== state.saved, saved: AsyncResult.value(operation) }).pipe(
        Match.when({ reloading: true }, () => 'Loading latest draft…'),
        Match.when({ saving: true }, () => 'Saving…'),
        Match.when({ changed: true }, () => 'Unsaved draft'),
        Match.when({ saved: { _tag: 'Some', value: { publish: true } } }, () => 'Draft saved and published'),
        Match.when({ saved: { _tag: 'Some' } }, () => 'Draft saved. Published work unchanged'),
        Match.orElse(() => 'Private draft'),
    );
    return (
        <EditorDialog busy={busy}>
            <div className="border-line flex flex-wrap items-center gap-2 border-b px-9 pb-4 max-sm:px-5">
                <button className="button button-ghost" disabled={busy} form={`${ids}form`} ref={save} type="submit">
                    Save draft
                </button>
                <Button className="button button-ghost" isDisabled={busy} onPress={() => onPreview(state)} slot="close">
                    Preview draft
                </Button>
                <button aria-describedby={pending.length > 0 ? `${ids}uploads` : undefined} className="button" disabled={busy || pending.length > 0} form={`${ids}form`} ref={publish} type="submit">
                    Publish
                </button>
                <p className="text-caption text-muted min-h-4.5 w-full" role="status">
                    {status}
                </p>
                {AsyncResult.builder(operation)
                    .onError(({ error, portfolio: submitted }) => (
                        <div className="w-full">
                            <RequestAlert conflict="Draft uses file missing from library. Remove its placement or upload it again" error={error} onField={(path) => locate(path, submitted)} />
                            {error._tag === 'PreconditionFailed' && <ConfirmDialog description="Discards unsaved edits. Uploaded files stay in library. Edits stay if reload fails" disabled={busy || pending.length > 0} label="Reload latest draft" onConfirm={() => reload(undefined)} />}
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
                    <p className="text-caption text-muted w-full" id={`${ids}uploads`}>
                        {quantity(pending.length, { one: 'file', other: 'files' })} not uploaded. Upload or remove before publishing or reloading{' '}
                        <button className="quiet-link" onClick={() => show('files')} type="button">
                            View uploads
                        </button>
                    </p>
                )}
            </div>
            <fieldset className="max-sm:pb-safe m-0 border-0 px-9 pt-3 pb-15 max-sm:px-5" disabled={busy}>
                <Tabs isDisabled={busy} onSelectionChange={setTab} selectedKey={tab}>
                    <TabList aria-label="Portfolio editing" className="border-line mb-6 flex gap-1.5 border-b max-sm:w-full max-sm:flex-wrap">
                        <Tab className="editor-tab" id={`${ids}entries`}>
                            Projects and studies
                        </Tab>
                        <Tab className="editor-tab" id={`${ids}files`}>
                            Files{pending.length > 0 ? ` (${pending.length})` : ''}
                        </Tab>
                        <Tab className="editor-tab" id={`${ids}identity`}>
                            Identity and hero
                        </Tab>
                    </TabList>
                    <TabPanel className="editor-panel" id={`${ids}entries`}>
                        {Match.value({ workspace, current }).pipe(
                            Match.when({ workspace: 'sources', current: Match.defined }, ({ current: entry }) => (
                                <>
                                    <div className="flex flex-wrap items-center justify-between gap-3">
                                        <h3 className="text-xl tracking-tight wrap-anywhere outline-none" id={`${ids}source`} tabIndex={-1}>
                                            Add to {entryTitle(entry)}
                                        </h3>
                                        <button className="button button-ghost" onClick={() => openEntry(entry.id)} type="button">
                                            <ArrowLeft className="size-4" />
                                            Back to entry
                                        </button>
                                    </div>
                                    <ReuseFile
                                        assets={state.assets}
                                        entryId={entry.id}
                                        key={Option.getOrUndefined(source)}
                                        onAdd={(id) => {
                                            setComposition(id);
                                            openEntry(entry.id);
                                        }}
                                        onSource={setSource}
                                        sourceId={source}
                                    />
                                </>
                            )),
                            Match.when({ workspace: 'entry', current: Match.defined }, ({ current: entry }) => (
                                <div className="flex flex-col gap-5" key={entry.id}>
                                    <div className="border-line flex flex-wrap items-center justify-between gap-3 border-b pb-3">
                                        <h3 className="min-w-0 flex-1 text-xl tracking-tight wrap-anywhere outline-none" id={`${ids}entry`} tabIndex={-1}>
                                            {entryTitle(entry)}
                                        </h3>
                                        <button className="button button-ghost" onClick={() => setWorkspace('projects')} type="button">
                                            Back to entries
                                        </button>
                                    </div>
                                    <EntryFields entry={entry} titleId={`${ids}title`} />
                                    <Compositions
                                        assets={state.assets}
                                        entry={entry}
                                        onLibrary={() => {
                                            setWorkspace('sources');
                                            focus(`${ids}source`);
                                        }}
                                        onSelect={setComposition}
                                        onUpload={() => {
                                            setTarget({ kind: 'entry', id: entry.id });
                                            show('files');
                                        }}
                                        selected={composition}
                                    />
                                </div>
                            )),
                            Match.orElse(() => (
                                <>
                                    <div className="actions">
                                        <button className="button button-outline" id={`${ids}new`} onClick={() => create('project')} type="button">
                                            New project
                                        </button>
                                        <button className="button button-ghost" onClick={() => create('study')} type="button">
                                            New study
                                        </button>
                                    </div>
                                    <EntryList
                                        entries={state.portfolio.entries}
                                        onRemove={() => focus(`${ids}new`)}
                                        onSelect={(item) => {
                                            openEntry(item.id);
                                            setTarget({ kind: 'entry', id: item.id });
                                        }}
                                        selected={selected}
                                    />
                                    {state.portfolio.entries.length === 0 && <p className="hint py-8">Create a project or study to add files</p>}
                                </>
                            )),
                        )}
                    </TabPanel>
                    <TabPanel className="editor-panel" id={`${ids}files`}>
                        <Suspense fallback={<p role="status">Loading files…</p>}>
                            <Uploads assets={state.assets} entries={state.portfolio.entries} onChoose={chooseSource} />
                        </Suspense>
                        <UploadRecovery />
                        <Library assets={state.assets} onSelect={setSource} onUse={chooseSource} portfolio={state.portfolio} published={published} sourceId={source} />
                    </TabPanel>
                    <TabPanel className="editor-panel" id={`${ids}identity`} shouldForceMount={true}>
                        <form
                            className="flex flex-col gap-5"
                            id={`${ids}form`}
                            onInvalidCapture={(event) => {
                                event.preventDefault();
                                show('identity');
                                focus(`${ids}email`);
                            }}
                            onSubmit={(event) => {
                                event.preventDefault();
                                submit({ ...state, publish: event.submitter === publish.current });
                            }}
                        >
                            <Identity
                                assets={state.assets}
                                emailId={`${ids}email`}
                                onChooseHero={() => {
                                    setTarget({ kind: 'hero' });
                                    show('files');
                                }}
                                portfolio={state.portfolio}
                            />
                        </form>
                    </TabPanel>
                </Tabs>
            </fieldset>
        </EditorDialog>
    );
};
const Editor = ({ onPublish, ...props }: { published: typeof Portfolio.Type; onPreview: (data: typeof PortfolioData.Type) => void; onPublish: (data: typeof PortfolioData.Type) => void }) => {
    const state = useAtomValue(draft);
    const reload = useAtomRefresh(draft);
    const pending = useAtomValue(pendingUploads);
    const unsaved = Option.exists(AsyncResult.value(state), ({ portfolio: edited, saved }) => edited !== saved) || pending.length > 0;
    useAtomSubscribe(saveRequest, (result) => {
        if (AsyncResult.isSuccess(result) && !result.waiting && result.value.publish) {
            onPublish(result.value);
        }
    });
    useEffect(() => {
        const listeners = new AbortController();
        if (unsaved) {
            globalThis.addEventListener('beforeunload', (event) => event.preventDefault(), { signal: listeners.signal });
        }
        return () => listeners.abort();
    }, [unsaved]);
    return AsyncResult.builder(state)
        .onInitial(() => <EditorDialog busy={false} description="Loading draft…" />)
        .onError((error) => (
            <EditorDialog busy={false} description="Draft failed to load">
                <div className="flex flex-col gap-5 px-9 py-6">
                    <RequestAlert error={error} />
                    <button className="button self-start" onClick={reload} type="button">
                        Try again
                    </button>
                </div>
            </EditorDialog>
        ))
        .onSuccess((value) => <DraftEditor {...props} {...value} />)
        .render();
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Editor };
