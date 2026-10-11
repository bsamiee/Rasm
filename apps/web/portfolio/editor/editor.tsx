import { useAtom, useAtomRefresh, useAtomSet, useAtomSubscribe, useAtomValue } from '@effect/atom-react';
import { BrowserCrypto } from '@effect/platform-browser';
import { Array, Effect, Match, Optic, Option, Struct } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { ArrowLeft, X } from 'lucide-react';
import { lazy, type ReactNode, Suspense, useEffect, useId, useRef, useState } from 'react';
import { Button, Dialog, Heading, type Key, Modal, ModalOverlay, Tab, TabList, TabPanel, Tabs, Text } from 'react-aria-components';
import { entryTitle } from '../media/display.ts';
import { createEntry, type Entry, type Portfolio, type PortfolioData } from '../model/document.ts';
import { type Placement, placementsFor } from '../model/placement.ts';
import { Compositions } from './compositions.tsx';
import { ConfirmDialog, RequestAlert } from './controls.tsx';
import { EntryFields, EntryList } from './entries.tsx';
import { emailId, focus, titleId } from './focus.ts';
import { Identity } from './identity.tsx';
import { Library } from './library.tsx';
import { ReuseFile } from './reuse.tsx';
import { deleteRequest, draft, draftRequest, reloadRequest, saveRequest } from './services.ts';
import { type Destination, destination, pendingUploads } from './uploader.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const tab = 'min-h-11 cursor-pointer px-3.5 py-2.5 text-sm selected:text-accent-text selected:underline selected:decoration-2 selected:underline-offset-8 max-sm:p-2 max-sm:text-xs';
const tabPanel = 'flex flex-col gap-5 data-inert:hidden';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Uploads = lazy(() => import('./uploads.tsx').then((module) => ({ default: module.Uploads })));
function EditorDialog({ description, busy, children }: { description?: string; busy: boolean; children?: ReactNode }): ReactNode {
    return (
        <ModalOverlay className="fixed inset-0 z-50 bg-foreground/55" isDismissable={!busy} isKeyboardDismissDisabled={busy}>
            <Modal>
                <Dialog className="fixed top-0 right-0 z-[51] h-(--visual-viewport-height) w-[min(100%,1000px)] overflow-y-auto bg-background shadow-[-20px_0_70px] shadow-foreground/13 outline-none">
                    <div className="relative px-9 pt-5 pr-[72px] pb-2 max-sm:px-5 max-sm:pr-[60px]">
                        <Heading className="text-[26px] tracking-[-0.035em] max-sm:text-2xl" slot="title">
                            Portfolio editor<span className="text-accent">.</span>
                        </Heading>
                        {description && (
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
}
function DraftEditor({ data, published, changed, pending, etag, onPreview }: { data: typeof PortfolioData.Type; published: typeof Portfolio.Type; changed: boolean; pending: readonly Destination[]; etag: string; onPreview: (data: typeof PortfolioData.Type) => void }): ReactNode {
    const ids = useId();
    const publish = useRef<HTMLButtonElement>(null);
    const save = useRef<HTMLButtonElement>(null);
    const setDraft = useAtomSet(draft);
    const setTarget = useAtomSet(destination);
    const [selected, setSelected] = useState<string>();
    const [workspace, setWorkspace] = useState<'projects' | 'entry' | 'sources'>('projects');
    const [composition, setComposition] = useState<string>();
    const [source, setSource] = useState(Option.none<string>());
    const [sheets, setSheets] = useState<readonly number[]>([]);
    const [selectedTab, setSelectedTab] = useState<Key>(`${ids}entries`);
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
    const show = (name: 'entries' | 'files' | 'identity'): void => setSelectedTab(`${ids}${name}`);
    const openEntry = (id: string): void => {
        setSelected(id);
        setWorkspace('entry');
        focus(`${ids}entry`);
    };
    const chooseSource = (entryId: string, assetId: string): void => {
        setSelected(entryId);
        setTarget({ kind: 'entry', id: entryId });
        setSource(Option.some(assetId));
        setSheets([]);
        setWorkspace('sources');
        show('entries');
        focus(`${ids}source`);
    };
    const setHero = (hero: typeof Placement.Type | undefined): void => setDraft(AsyncResult.map(Optic.replace(portfolio.optionalKey('hero'), hero)));
    const updateEntry = (update: (entry: typeof Entry.Type) => typeof Entry.Type): void => setDraft(AsyncResult.map(entries.modify(Array.map((item) => (item.id === selected ? update(item) : item)))));
    const create = (kind: typeof Entry.Type.kind): void => {
        const created = createEntry(Effect.runSync(BrowserCrypto.WebCrypto).randomUUID(), kind);
        setDraft(AsyncResult.map(entries.modify(Array.append(created))));
        setSelected(created.id);
        setWorkspace('entry');
        setTarget({ kind: 'entry', id: created.id });
        focus(titleId(created));
    };
    const locate = (path: readonly PropertyKey[], snapshot: typeof Portfolio.Type): void =>
        Match.value({ head: path[0], index: path[1] }).pipe(
            Match.when({ head: 'entries', index: Match.number }, ({ index }) => {
                show('entries');
                Array.get(snapshot.entries, index).pipe(Option.map(Struct.get('id')), Option.map(openEntry));
            }),
            Match.orElse(() => show('files')),
        );
    const status = Match.value({ reloading: reloading.waiting, waiting: operation.waiting, changed, last: AsyncResult.value(operation) }).pipe(
        Match.when({ reloading: true }, () => 'Loading the latest draft…'),
        Match.when({ waiting: true }, () => 'Saving…'),
        Match.when({ changed: true }, () => 'Unsaved draft'),
        Match.orElse(({ last }) => Option.match(last, { onNone: () => 'Private draft', onSome: (result) => (result.publish ? 'Published. Your site shows the saved draft.' : 'Draft saved. Published work is unchanged.') })),
    );
    return (
        <EditorDialog busy={busy}>
            <div className="flex flex-wrap items-center gap-2 border-line border-b px-9 pb-4 max-sm:px-5">
                <button className="button button-ghost" disabled={busy} form={`${ids}form`} ref={save} type="submit">
                    Save draft
                </button>
                <Button className="button button-ghost" isDisabled={busy} onPress={(): void => onPreview(data)} slot="close">
                    Preview draft
                </Button>
                <button aria-describedby={pending.length > 0 ? `${ids}uploads` : undefined} className="button" disabled={busy || pending.length > 0} form={`${ids}form`} ref={publish} type="submit">
                    Publish
                </button>
                <p className="min-h-[18px] w-full text-[13px] text-muted" role="status">
                    {status}
                </p>
                {Option.match(AsyncResult.error(operation), {
                    onNone: (): ReactNode => null,
                    onSome: (failure) => (
                        <div className="w-full">
                            <RequestAlert conflict="An asset in this draft is no longer available. Remove its placement or upload it again before saving." error={failure.error} onField={(path): void => locate(path, failure.portfolio)} />
                            {failure.error._tag === 'PreconditionFailed' && (
                                <ConfirmDialog description="Discard your unsaved edits and load the latest saved draft. Uploaded files remain in the library. Your current draft stays here if loading fails." disabled={busy || pending.length > 0} label="Reload latest draft" onConfirm={(): void => reload(undefined)} />
                            )}
                        </div>
                    ),
                })}
                {Option.match(AsyncResult.error(reloading), { onNone: (): ReactNode => null, onSome: (failure) => <RequestAlert conflict="The latest draft could not be loaded." error={failure} /> })}
                {Option.match(AsyncResult.error(deletion), { onNone: (): ReactNode => null, onSome: (failure) => <RequestAlert conflict="This file is still used by saved or published work. Remove its placements, save, and publish before deleting." error={failure} /> })}
                {pending.length > 0 && (
                    <p className="w-full text-[13px] text-muted" id={`${ids}uploads`}>
                        {pending.length} {pending.length === 1 ? 'file needs' : 'files need'} attention. Finish, retry, or remove queued files before publishing or reloading.{' '}
                        <button className="quiet-link" onClick={(): void => show('files')} type="button">
                            View uploads
                        </button>
                    </p>
                )}
            </div>
            <fieldset className="m-0 border-0 px-9 pt-3 pb-[60px] max-sm:px-5 max-sm:pt-3 max-sm:pb-[max(36px,env(safe-area-inset-bottom))]" disabled={busy}>
                <Tabs isDisabled={busy} onSelectionChange={setSelectedTab} selectedKey={selectedTab}>
                    <TabList aria-label="Portfolio editing" className="mb-6 flex gap-1.5 border-line border-b max-sm:w-full max-sm:flex-wrap">
                        <Tab className={tab} id={`${ids}entries`}>
                            Projects and studies
                        </Tab>
                        <Tab className={tab} id={`${ids}files`}>
                            Files{pending.length > 0 ? ` (${pending.length})` : ''}
                        </Tab>
                        <Tab className={tab} id={`${ids}identity`}>
                            Identity and hero
                        </Tab>
                    </TabList>
                    <TabPanel className={tabPanel} id={`${ids}entries`}>
                        {(workspace === 'projects' || !current) && (
                            <>
                                <div className="actions">
                                    <button className="button button-outline" id={`${ids}new`} onClick={(): void => create('project')} type="button">
                                        New project
                                    </button>
                                    <button className="button button-ghost" onClick={(): void => create('study')} type="button">
                                        New study
                                    </button>
                                </div>
                                <EntryList
                                    entries={data.portfolio.entries}
                                    locked={(item): boolean => pending.some((target) => target.kind === 'entry' && target.id === item.id)}
                                    onChange={(update): void => setDraft(AsyncResult.map(entries.modify(update)))}
                                    onRemove={(keys): void => {
                                        setDraft(AsyncResult.map(entries.modify(Array.filter((candidate) => !keys.has(candidate.id)))));
                                        setSelected((value) => (value && keys.has(value) ? undefined : value));
                                        setTarget((target) => (target.kind === 'entry' && keys.has(target.id) ? { kind: 'library' } : target));
                                        focus(`${ids}new`);
                                    }}
                                    onSelect={(item): void => {
                                        openEntry(item.id);
                                        setTarget({ kind: 'entry', id: item.id });
                                    }}
                                    selected={selected}
                                />
                                {data.portfolio.entries.length === 0 && <p className="hint py-8">Create a project or an independent study, then add its files.</p>}
                            </>
                        )}
                        {current && workspace === 'sources' && (
                            <>
                                <div className="flex flex-wrap items-center justify-between gap-3">
                                    <h3 className="wrap-anywhere text-xl tracking-tight outline-none" id={`${ids}source`} tabIndex={-1}>
                                        Add to {entryTitle(current)}
                                    </h3>
                                    <button className="button button-ghost" onClick={(): void => openEntry(current.id)} type="button">
                                        <ArrowLeft className="size-4" />
                                        Back to entry
                                    </button>
                                </div>
                                <ReuseFile
                                    assets={data.assets}
                                    onAdd={(compositions): void => {
                                        updateEntry((entry) => ({ ...entry, compositions: [...entry.compositions, ...compositions] }));
                                        setComposition(compositions[0]?.id);
                                        openEntry(current.id);
                                    }}
                                    onSelect={setSheets}
                                    onSource={setSource}
                                    selected={sheets}
                                    sourceId={source}
                                />
                            </>
                        )}
                        {current && workspace === 'entry' && (
                            <div className="flex flex-col gap-5" key={current.id}>
                                <div className="flex flex-wrap items-center justify-between gap-3 border-line border-b pb-3">
                                    <h3 className="wrap-anywhere min-w-0 flex-1 text-xl tracking-tight outline-none" id={`${ids}entry`} tabIndex={-1}>
                                        {entryTitle(current)}
                                    </h3>
                                    <button className="button button-ghost" onClick={(): void => setWorkspace('projects')} type="button">
                                        Change project
                                    </button>
                                </div>
                                <EntryFields entry={current} onChange={updateEntry} />
                                <Compositions
                                    assets={data.assets}
                                    entry={current}
                                    onChange={updateEntry}
                                    onHero={setHero}
                                    onLibrary={(): void => {
                                        setWorkspace('sources');
                                        focus(`${ids}source`);
                                    }}
                                    onSelect={setComposition}
                                    onUpload={(): void => {
                                        setTarget({ kind: 'entry', id: current.id });
                                        show('files');
                                    }}
                                    selected={composition}
                                />
                            </div>
                        )}
                    </TabPanel>
                    <TabPanel className={tabPanel} id={`${ids}files`}>
                        <Suspense fallback={<p role="status">Loading files…</p>}>
                            <Uploads assets={data.assets} entries={data.portfolio.entries} onChoose={chooseSource} />
                        </Suspense>
                        <Library
                            assets={data.assets}
                            draft={data.portfolio}
                            onDelete={(asset): void => deleteAsset(asset.id)}
                            onHero={(asset): void => setHero(Array.headNonEmpty(placementsFor(asset, false)))}
                            onSelect={(id): void => {
                                setSource(id);
                                setSheets([]);
                            }}
                            onUse={chooseSource}
                            published={published}
                            sourceId={source}
                        />
                    </TabPanel>
                    <TabPanel className={tabPanel} id={`${ids}identity`} shouldForceMount={true}>
                        <form
                            className="flex flex-col gap-5"
                            id={`${ids}form`}
                            onInvalidCapture={(event): void => {
                                event.preventDefault();
                                show('identity');
                                focus(emailId);
                            }}
                            onSubmit={(event): void => {
                                event.preventDefault();
                                submit({ snapshot: data, publish: event.submitter === publish.current, etag });
                            }}
                        >
                            <Identity
                                assets={data.assets}
                                onChange={(update): void => setDraft(AsyncResult.map(portfolio.modify(update)))}
                                onChooseHero={(): void => {
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
}
function Editor({ onPublish, ...props }: { published: typeof Portfolio.Type; onPreview: (data: typeof PortfolioData.Type) => void; onPublish: (data: typeof PortfolioData.Type) => void }): ReactNode {
    const initial = useAtomValue(draftRequest);
    const loaded = useAtomValue(draft);
    const operation = useAtomValue(saveRequest);
    const reloaded = useAtomValue(reloadRequest);
    const reload = useAtomRefresh(draft);
    const pending = useAtomValue(pendingUploads);
    const latest = Option.match(AsyncResult.value(reloaded), { onNone: () => initial, onSome: AsyncResult.success });
    const baseline = Option.orElse(Option.map(AsyncResult.value(operation), Struct.get('snapshot')), () => Option.map(AsyncResult.value(latest), Struct.get('body')));
    const changed = Option.exists(Option.all({ data: AsyncResult.value(loaded), saved: baseline }), ({ data, saved }) => data.portfolio !== saved.portfolio);
    const unsaved = changed || pending.length > 0;
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
        return (): void => listeners.abort();
    }, [unsaved]);
    return AsyncResult.matchWithError(AsyncResult.all({ data: loaded, initial: latest }), {
        onInitial: () => <EditorDialog busy={false} description="Opening your private draft…" />,
        onError: (error) => (
            <EditorDialog busy={false} description="Your draft could not be loaded.">
                <div className="flex flex-col gap-5 px-9 py-6">
                    <RequestAlert conflict="Your draft changed. Try loading it again." error={error} />
                    <button className="button self-start" onClick={reload} type="button">
                        Try again
                    </button>
                </div>
            </EditorDialog>
        ),
        onDefect: (defect) => {
            throw defect;
        },
        onSuccess: ({ value }) => <DraftEditor {...props} changed={changed} data={value.data} etag={Option.match(AsyncResult.value(operation), { onNone: () => value.initial.headers['draft-etag'], onSome: Struct.get('etag') })} pending={pending} />,
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Editor };
