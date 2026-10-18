import { useAtom, useAtomRefresh, useAtomSet, useAtomSubscribe, useAtomValue } from '@effect/atom-react';
import { focus } from '@rasm/ui/focus.ts';
import { Array, Effect, Match, Optic, Option } from 'effect';
import { AsyncResult, type Atom } from 'effect/reactivity';
import { ArrowLeft, X } from 'lucide-react';
import { lazy, type ReactNode, Suspense, useEffect, useId, useRef, useState } from 'react';
import { Button, Dialog, Heading, type Key, Modal, ModalOverlay, Tab, TabList, TabPanel, Tabs, Text } from 'react-aria-components';
import { entryTitle, quantity } from '../media/display.ts';
import { createEntry, type Entry, type Portfolio, type PortfolioData } from '../model/document.ts';
import { type Placement, placementsFor } from '../model/placement.ts';
import { Compositions } from './compositions.tsx';
import { ConfirmDialog, RequestAlert } from './controls.tsx';
import { EntryFields, EntryList } from './entries.tsx';
import { Identity } from './identity.tsx';
import { Library } from './library.tsx';
import { ReuseFile } from './reuse.tsx';
import { deleteRequest, draft, draftState, identifier, reloadRequest, saveRequest } from './services.ts';
import { destination, pendingUploads } from './uploader.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Uploads = lazy(() => import('./uploads.tsx').then((module) => ({ default: module.Uploads })));
const EditorDialog = ({ description, busy, children }: { description?: string; busy: boolean; children?: ReactNode }) => (
    <ModalOverlay className="fixed inset-0 z-50 bg-foreground/55" isDismissable={!busy} isKeyboardDismissDisabled={busy}>
        <Modal>
            <Dialog className="fixed top-0 right-0 h-(--visual-viewport-height) w-full max-w-250 overflow-y-auto bg-background shadow-panel outline-none">
                <div className="relative px-9 pt-5 pr-18 pb-2 max-sm:px-5 max-sm:pr-15">
                    <Heading className="text-heading tracking-display max-sm:text-2xl" slot="title">
                        Portfolio editor<span className="text-accent">.</span>
                    </Heading>
                    {description === undefined ? null : (
                        <Text className="mt-2 text-muted text-sm" elementType="p" slot="description">
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
const DraftEditor = ({ data, revision, changed, published, onPreview }: Atom.Success<typeof draftState> & { published: typeof Portfolio.Type; onPreview: (data: typeof PortfolioData.Type) => void }) => {
    const ids = useId();
    const publish = useRef<HTMLButtonElement>(null);
    const save = useRef<HTMLButtonElement>(null);
    const setDraft = useAtomSet(draft);
    const setTarget = useAtomSet(destination);
    const pending = useAtomValue(pendingUploads);
    const [selected, setSelected] = useState<string>();
    const [workspace, setWorkspace] = useState<'projects' | 'entry' | 'sources'>('projects');
    const [composition, setComposition] = useState<string>();
    const [source, setSource] = useState(Option.none<string>());
    const [sheets, setSheets] = useState<readonly number[]>([]);
    const [tab, setTab] = useState<Key>(`${ids}entries`);
    const [operation, submit] = useAtom(saveRequest);
    const [deletion, deleteAsset] = useAtom(deleteRequest);
    const [reloading, reload] = useAtom(reloadRequest);
    useEffect(() => {
        if (AsyncResult.isSuccess(reloading) && !reloading.waiting) {
            save.current?.focus();
        }
    }, [reloading]);
    const portfolio = Optic.id<typeof PortfolioData.Type>().key('portfolio');
    const entries = portfolio.key('entries');
    const busy = operation.waiting || deletion.waiting || reloading.waiting;
    const current = data.portfolio.entries.find((item) => item.id === selected);
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
        setSheets([]);
        setWorkspace('sources');
        show('entries');
        focus(`${ids}source`);
    };
    const setHero = (hero: typeof Placement.Type | undefined) => setDraft(AsyncResult.map(Optic.replace(portfolio.optionalKey('hero'), hero)));
    const updateEntry = (update: (entry: typeof Entry.Type) => typeof Entry.Type) => setDraft(AsyncResult.map(entries.modify(Array.map((item) => (item.id === selected ? update(item) : item)))));
    const create = (kind: typeof Entry.Type.kind) => {
        const created = createEntry(Effect.runSync(identifier), kind);
        setDraft(AsyncResult.map(entries.modify(Array.append(created))));
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
    const status = Match.value({ reloading: reloading.waiting, saving: operation.waiting, changed, saved: AsyncResult.value(operation) }).pipe(
        Match.when({ reloading: true }, () => 'Loading the latest draft…'),
        Match.when({ saving: true }, () => 'Saving…'),
        Match.when({ changed: true }, () => 'Unsaved draft'),
        Match.when({ saved: { _tag: 'Some', value: { publish: true } } }, () => 'Published. Your site shows the saved draft.'),
        Match.when({ saved: { _tag: 'Some' } }, () => 'Draft saved. Published work is unchanged.'),
        Match.orElse(() => 'Private draft'),
    );
    return (
        <EditorDialog busy={busy}>
            <div className="flex flex-wrap items-center gap-2 border-line border-b px-9 pb-4 max-sm:px-5">
                <button className="button button-ghost" disabled={busy} form={`${ids}form`} ref={save} type="submit">
                    Save draft
                </button>
                <Button className="button button-ghost" isDisabled={busy} onPress={() => onPreview(data)} slot="close">
                    Preview draft
                </Button>
                <button aria-describedby={pending.length > 0 ? `${ids}uploads` : undefined} className="button" disabled={busy || pending.length > 0} form={`${ids}form`} ref={publish} type="submit">
                    Publish
                </button>
                <p className="min-h-4.5 w-full text-caption text-muted" role="status">
                    {status}
                </p>
                {AsyncResult.builder(operation)
                    .onError(({ error, portfolio: submitted }) => (
                        <div className="w-full">
                            <RequestAlert conflict="An asset in this draft is no longer available. Remove its placement or upload it again before saving." error={error} onField={(path) => locate(path, submitted)} />
                            {error._tag === 'PreconditionFailed' && (
                                <ConfirmDialog description="Discard your unsaved edits and load the latest saved draft. Uploaded files remain in the library. Your current draft stays here if loading fails." disabled={busy || pending.length > 0} label="Reload latest draft" onConfirm={() => reload(undefined)} />
                            )}
                        </div>
                    ))
                    .orNull()}
                {AsyncResult.builder(reloading)
                    .onError((error) => <RequestAlert error={error} />)
                    .orNull()}
                {AsyncResult.builder(deletion)
                    .onError((error) => <RequestAlert conflict="This file is still used by saved or published work. Remove its placements, save, and publish before deleting." error={error} />)
                    .orNull()}
                {pending.length > 0 && (
                    <p className="w-full text-caption text-muted" id={`${ids}uploads`}>
                        {quantity(pending.length, { one: 'file needs', other: 'files need' })} attention. Finish, retry, or remove queued files before publishing or reloading.{' '}
                        <button className="quiet-link" onClick={() => show('files')} type="button">
                            View uploads
                        </button>
                    </p>
                )}
            </div>
            <fieldset className="m-0 border-0 px-9 pt-3 pb-15 max-sm:px-5 max-sm:pb-safe" disabled={busy}>
                <Tabs isDisabled={busy} onSelectionChange={setTab} selectedKey={tab}>
                    <TabList aria-label="Portfolio editing" className="mb-6 flex gap-1.5 border-line border-b max-sm:w-full max-sm:flex-wrap">
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
                                        <h3 className="wrap-anywhere text-xl tracking-tight outline-none" id={`${ids}source`} tabIndex={-1}>
                                            Add to {entryTitle(entry)}
                                        </h3>
                                        <button className="button button-ghost" onClick={() => openEntry(entry.id)} type="button">
                                            <ArrowLeft className="size-4" />
                                            Back to entry
                                        </button>
                                    </div>
                                    <ReuseFile
                                        assets={data.assets}
                                        onAdd={(compositions) => {
                                            updateEntry((item) => ({ ...item, compositions: [...item.compositions, ...compositions] }));
                                            setComposition(compositions[0]?.id);
                                            openEntry(entry.id);
                                        }}
                                        onSelect={setSheets}
                                        onSource={setSource}
                                        selected={sheets}
                                        sourceId={source}
                                    />
                                </>
                            )),
                            Match.when({ workspace: 'entry', current: Match.defined }, ({ current: entry }) => (
                                <div className="flex flex-col gap-5" key={entry.id}>
                                    <div className="flex flex-wrap items-center justify-between gap-3 border-line border-b pb-3">
                                        <h3 className="wrap-anywhere min-w-0 flex-1 text-xl tracking-tight outline-none" id={`${ids}entry`} tabIndex={-1}>
                                            {entryTitle(entry)}
                                        </h3>
                                        <button className="button button-ghost" onClick={() => setWorkspace('projects')} type="button">
                                            Change project
                                        </button>
                                    </div>
                                    <EntryFields entry={entry} onChange={updateEntry} titleId={`${ids}title`} />
                                    <Compositions
                                        assets={data.assets}
                                        entry={entry}
                                        onChange={updateEntry}
                                        onHero={setHero}
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
                                        entries={data.portfolio.entries}
                                        locked={(item) => pending.some((target) => target.kind === 'entry' && target.id === item.id)}
                                        onChange={(items) => setDraft(AsyncResult.map(Optic.replace(entries, items)))}
                                        onRemove={(keys) => {
                                            setDraft(AsyncResult.map(entries.modify(Array.filter((candidate) => !keys.has(candidate.id)))));
                                            setSelected((value) => (value !== undefined && keys.has(value) ? undefined : value));
                                            setTarget((target) => (target.kind === 'entry' && keys.has(target.id) ? { kind: 'library' } : target));
                                            focus(`${ids}new`);
                                        }}
                                        onSelect={(item) => {
                                            openEntry(item.id);
                                            setTarget({ kind: 'entry', id: item.id });
                                        }}
                                        selected={selected}
                                    />
                                    {data.portfolio.entries.length === 0 && <p className="hint py-8">Create a project or an independent study, then add its files.</p>}
                                </>
                            )),
                        )}
                    </TabPanel>
                    <TabPanel className="editor-panel" id={`${ids}files`}>
                        <Suspense fallback={<p role="status">Loading files…</p>}>
                            <Uploads assets={data.assets} entries={data.portfolio.entries} onChoose={chooseSource} />
                        </Suspense>
                        <Library
                            assets={data.assets}
                            draft={data.portfolio}
                            onDelete={(asset) => deleteAsset(asset.id)}
                            onHero={(asset) => setHero(Array.headNonEmpty(placementsFor(asset)))}
                            onSelect={(id) => {
                                setSource(id);
                                setSheets([]);
                            }}
                            onUse={chooseSource}
                            published={published}
                            sourceId={source}
                        />
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
                                submit({ snapshot: data, publish: event.submitter === publish.current, revision });
                            }}
                        >
                            <Identity
                                assets={data.assets}
                                emailId={`${ids}email`}
                                onChange={(update) => setDraft(AsyncResult.map(portfolio.modify(update)))}
                                onChooseHero={() => {
                                    setTarget({ kind: 'hero' });
                                    show('files');
                                }}
                                onHero={setHero}
                                portfolio={data.portfolio}
                            />
                        </form>
                    </TabPanel>
                </Tabs>
            </fieldset>
        </EditorDialog>
    );
};
const Editor = ({ onPublish, ...props }: { published: typeof Portfolio.Type; onPreview: (data: typeof PortfolioData.Type) => void; onPublish: (data: typeof PortfolioData.Type) => void }) => {
    const state = useAtomValue(draftState);
    const reload = useAtomRefresh(draft);
    const pending = useAtomValue(pendingUploads);
    const unsaved = Option.exists(AsyncResult.value(state), ({ changed }) => changed) || pending.length > 0;
    useAtomSubscribe(saveRequest, (result) => {
        if (AsyncResult.isSuccess(result) && !result.waiting && result.value.publish) {
            onPublish(result.value.snapshot);
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
        .onInitial(() => <EditorDialog busy={false} description="Opening your private draft…" />)
        .onError((error) => (
            <EditorDialog busy={false} description="Your draft could not be loaded.">
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
