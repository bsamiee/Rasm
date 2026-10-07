import { useAtom, useAtomRefresh, useAtomSet, useAtomSubscribe, useAtomValue } from '@effect/atom-react';
import { BrowserCrypto } from '@effect/platform-browser';
import { Array, Effect, Match, Optic, Option, Struct } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { X } from 'lucide-react';
import { lazy, type ReactNode, Suspense, useEffect, useId, useRef, useState } from 'react';
import { Button, Dialog, Heading, type Key, Modal, ModalOverlay, Tab, TabList, TabPanel, Tabs, Text } from 'react-aria-components';
import { createEntry, type Entry, type Portfolio, type PortfolioData } from '../model/document.ts';
import { type Placement, placementsFor } from '../model/placement.ts';
import { Compositions } from './compositions.tsx';
import { RequestAlert } from './controls.tsx';
import { EntryFields, EntryList } from './entries.tsx';
import { emailId, focus, titleId } from './focus.ts';
import { Identity } from './identity.tsx';
import { Library } from './library.tsx';
import { ReuseFile } from './reuse.tsx';
import { deleteRequest, draft, draftRequest, saveRequest } from './services.ts';
import { type Destination, destination, pendingUploads } from './uploader.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const tab = 'min-h-11 cursor-pointer px-3.5 py-2.5 text-sm selected:text-accent-text selected:shadow-[inset_0_-2px_var(--color-accent-text)] max-sm:p-2 max-sm:text-xs';
const tabPanel = 'flex flex-col gap-5 data-inert:hidden';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Uploads = lazy(() => import('./uploads.tsx').then((module) => ({ default: module.Uploads })));
function EditorDialog({ description, busy, children }: { description: string; busy: boolean; children?: ReactNode }): ReactNode {
    return (
        <ModalOverlay className="fixed inset-0 z-50 bg-foreground/55" isDismissable={!busy} isKeyboardDismissDisabled={busy}>
            <Modal>
                <Dialog className="fixed inset-y-0 right-0 z-[51] flex w-[min(100%,1000px)] flex-col bg-background shadow-[-20px_0_70px_rgb(24_24_22/0.13)] outline-none">
                    <div className="relative border-line border-b py-[30px] pr-[72px] pb-[22px] pl-9 max-sm:py-6 max-sm:pr-[60px] max-sm:pb-[18px] max-sm:pl-5">
                        <Heading className="text-4xl tracking-[-0.045em] max-sm:text-[30px]" slot="title">
                            Portfolio editor<span className="text-accent">.</span>
                        </Heading>
                        <Text className="mt-2 text-muted text-sm" elementType="p" slot="description">
                            {description}
                        </Text>
                        <Button aria-label="Close portfolio editor" className="absolute top-5 right-5 grid min-h-11 min-w-11 place-items-center" isDisabled={busy} slot="close">
                            <X className="size-6" />
                        </Button>
                    </div>
                    {children}
                </Dialog>
            </Modal>
        </ModalOverlay>
    );
}
function DraftEditor({ data, published, changed, pending, onPreview }: { data: typeof PortfolioData.Type; published: typeof Portfolio.Type; changed: boolean; pending: readonly Destination[]; onPreview: (data: typeof PortfolioData.Type) => void }): ReactNode {
    const ids = useId();
    const publish = useRef<HTMLButtonElement>(null);
    const setDraft = useAtomSet(draft);
    const setTarget = useAtomSet(destination);
    const [selected, setSelected] = useState<string>();
    const [selectedTab, setSelectedTab] = useState<Key>(`${ids}entries`);
    const [operation, submit] = useAtom(saveRequest);
    const [deletion, deleteAsset] = useAtom(deleteRequest);
    const portfolio = Optic.id<typeof PortfolioData.Type>().key('portfolio');
    const entries = portfolio.key('entries');
    const busy = operation.waiting || deletion.waiting;
    const current = data.portfolio.entries.find((item) => item.id === selected);
    const show = (name: 'entries' | 'files' | 'identity'): void => setSelectedTab(`${ids}${name}`);
    const setHero = (hero: typeof Placement.Type | undefined): void => setDraft(AsyncResult.map(Optic.replace(portfolio.optionalKey('hero'), hero)));
    const updateEntry = (update: (entry: typeof Entry.Type) => typeof Entry.Type): void => setDraft(AsyncResult.map(entries.modify(Array.map((item) => (item.id === selected ? update(item) : item)))));
    const create = (kind: typeof Entry.Type.kind): void => {
        const created = createEntry(Effect.runSync(BrowserCrypto.WebCrypto).randomUUID(), kind);
        setDraft(AsyncResult.map(entries.modify(Array.append(created))));
        setSelected(created.id);
        setTarget({ kind: 'entry', id: created.id });
        focus(titleId(created));
    };
    const locate = (path: readonly PropertyKey[], snapshot: typeof Portfolio.Type): void =>
        Match.value({ head: path[0], index: path[1] }).pipe(
            Match.when({ head: 'entries', index: Match.number }, ({ index }) => {
                show('entries');
                Option.map(Array.get(snapshot.entries, index), (item) => {
                    setSelected(item.id);
                    focus(titleId(item));
                });
            }),
            Match.when({ head: 'email' }, () => {
                show('identity');
                focus(emailId);
            }),
            Match.orElse(() => show('files')),
        );
    const status = Match.value({ waiting: operation.waiting, changed, last: AsyncResult.value(operation) }).pipe(
        Match.when({ waiting: true }, () => 'Saving…'),
        Match.when({ changed: true }, () => 'Unsaved draft'),
        Match.orElse(({ last }) => Option.match(last, { onNone: () => 'Only you can edit. Files remain private until publication.', onSome: (result) => (result.publish ? 'Published. Your site shows the saved draft.' : 'Draft saved. Published work is unchanged.') })),
    );
    return (
        <EditorDialog busy={busy} description="Create an entry, add files, compose, preview, publish.">
            <div className="flex flex-wrap gap-2.5 border-line border-b px-9 py-[18px] max-sm:gap-2 max-sm:px-5 max-sm:py-3.5">
                <button className="button button-outline" disabled={busy} form={`${ids}form`} type="submit">
                    Save draft
                </button>
                <Button className="button button-outline" isDisabled={busy} onPress={(): void => onPreview(data)} slot="close">
                    Preview
                </Button>
                <button aria-describedby={pending.length > 0 ? `${ids}uploads` : undefined} className="button" disabled={busy || pending.length > 0} form={`${ids}form`} ref={publish} type="submit">
                    Publish
                </button>
                <p className="min-h-[18px] w-full text-[13px] text-muted" role="status">
                    {status}
                </p>
                {Option.match(AsyncResult.error(operation), {
                    onNone: (): ReactNode => null,
                    onSome: (failure) => <RequestAlert conflict="An asset in this draft is no longer available. Remove its placement or upload it again before saving." error={failure.error} onField={(path): void => locate(path, failure.portfolio)} />,
                })}
                {Option.match(AsyncResult.error(deletion), { onNone: (): ReactNode => null, onSome: (failure) => <RequestAlert conflict="This file is still used by saved or published work. Remove its placements, save, and publish before deleting." error={failure} /> })}
                {pending.length > 0 && (
                    <p className="w-full text-[13px] text-muted" id={`${ids}uploads`}>
                        {pending.length} {pending.length === 1 ? 'file needs' : 'files need'} attention. Finish, retry, or remove queued files before publishing.{' '}
                        <button className="quiet-link" onClick={(): void => show('files')} type="button">
                            View uploads
                        </button>
                    </p>
                )}
            </div>
            <fieldset className="m-0 min-h-0 flex-1 overflow-auto border-0 px-9 pt-6 pb-[60px] max-sm:px-5 max-sm:pt-5 max-sm:pb-[max(36px,env(safe-area-inset-bottom))]" disabled={busy}>
                <Tabs isDisabled={busy} onSelectionChange={setSelectedTab} selectedKey={selectedTab}>
                    <TabList aria-label="Portfolio editing" className="mb-6 flex gap-1.5 border-line border-b max-sm:w-full max-sm:flex-wrap">
                        <Tab className={tab} id={`${ids}entries`}>
                            Projects &amp; studies
                        </Tab>
                        <Tab className={tab} id={`${ids}files`}>
                            Files{pending.length > 0 ? ` (${pending.length})` : ''}
                        </Tab>
                        <Tab className={tab} id={`${ids}identity`}>
                            Identity &amp; hero
                        </Tab>
                    </TabList>
                    <TabPanel className={tabPanel} id={`${ids}entries`} shouldForceMount={true}>
                        <div className="actions">
                            <button className="button" id={`${ids}new`} onClick={(): void => create('project')} type="button">
                                New project
                            </button>
                            <button className="button button-outline" onClick={(): void => create('study')} type="button">
                                New study
                            </button>
                        </div>
                        <EntryList
                            entries={data.portfolio.entries}
                            locked={(item): boolean => pending.some((target) => target.kind === 'entry' && target.id === item.id)}
                            onChange={(update): void => setDraft(AsyncResult.map(entries.modify(update)))}
                            onRemove={(item): void => {
                                setDraft(AsyncResult.map(entries.modify(Array.filter((candidate) => candidate.id !== item.id))));
                                setSelected((value) => (value === item.id ? undefined : value));
                                setTarget((target) => (target.kind === 'entry' && target.id === item.id ? { kind: 'library' } : target));
                                focus(`${ids}new`);
                            }}
                            onSelect={(item): void => {
                                setSelected(item.id);
                                setTarget({ kind: 'entry', id: item.id });
                            }}
                            selected={selected}
                        />
                        {data.portfolio.entries.length === 0 && (
                            <div className="flex flex-col items-center justify-center gap-5 px-5 py-10 text-center">
                                <h3 className="mb-2 text-xl">Your first chapter starts here</h3>
                                <p className="hint">Create a project or an independent study, then add its files.</p>
                            </div>
                        )}
                        {current && (
                            <div className="flex flex-col gap-5 border-foreground border-t pt-[25px]" key={current.id}>
                                <EntryFields entry={current} onChange={updateEntry} />
                                <Compositions
                                    entry={current}
                                    onChange={updateEntry}
                                    onHero={setHero}
                                    onUpload={(): void => {
                                        setTarget({ kind: 'entry', id: current.id });
                                        show('files');
                                    }}
                                />
                                <ReuseFile assets={data.assets} onAdd={(compositions): void => updateEntry((entry) => ({ ...entry, compositions: [...entry.compositions, ...compositions] }))} />
                            </div>
                        )}
                    </TabPanel>
                    <TabPanel className={tabPanel} id={`${ids}files`}>
                        <Suspense fallback={<p role="status">Loading files…</p>}>
                            <Uploads entries={data.portfolio.entries} />
                        </Suspense>
                        <Library assets={data.assets} draft={data.portfolio} onDelete={(asset): void => deleteAsset(asset.id)} onHero={(asset): void => setHero(Array.headNonEmpty(placementsFor(asset)))} published={published} />
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
                                submit({ snapshot: data, publish: event.submitter === publish.current });
                            }}
                        >
                            <Identity
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
    const reload = useAtomRefresh(draft);
    const pending = useAtomValue(pendingUploads);
    const baseline = Option.orElse(Option.map(AsyncResult.value(operation), Struct.get('snapshot')), () => AsyncResult.value(initial));
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
    return AsyncResult.matchWithError(loaded, {
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
        onSuccess: ({ value }) => <DraftEditor {...props} changed={changed} data={value} pending={pending} />,
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Editor };
