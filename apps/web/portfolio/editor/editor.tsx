import { useAtom, useAtomRefresh, useAtomSet, useAtomSubscribe, useAtomValue } from '@effect/atom-react';
import { Backdrop, focusElement, Issues } from '@rasm/ui';
import { Array, Function, Match, Option, Predicate, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { AsyncResult, type Atom } from 'effect/reactivity';
import { ArrowLeft, X } from 'lucide-react';
import { lazy, type ReactNode, Suspense, useEffect, useId, useRef, useState } from 'react';
import { Button, Dialog, Heading, type Key, Modal, Tab, TabList, TabPanel, Tabs, Text, Toolbar } from 'react-aria-components';
import { Compositions } from '#/editor/compositions.tsx';
import { ConfirmDialog } from '#/editor/confirm-dialog.tsx';
import { EntryFields, EntryList } from '#/editor/entries.tsx';
import { Identity } from '#/editor/identity.tsx';
import { Library } from '#/editor/library.tsx';
import { PlaceFile } from '#/editor/place-file.tsx';
import { RequestAlert } from '#/editor/request-alert.tsx';
import { deleteRequest, draft, portfolio, reloadRequest, saveRequest } from '#/editor/state.ts';
import { UploadRecovery } from '#/editor/upload-recovery.tsx';
import { Destination, nextDestination, pendingUploads } from '#/editor/uploader.ts';
import { entryTitle, format } from '#/media/labels.ts';
import type { AssetId } from '#/model/asset.ts';
import type { CompositionId } from '#/model/placement.ts';
import { Entry, EntryId, type Portfolio, type PortfolioData } from '#/model/portfolio.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Uploads = lazy(() => import('#/editor/uploads.tsx').then((module) => ({ default: module.Uploads })));
const EditorDialog = ({ description, busy, children }: { description?: string; busy: boolean; children?: ReactNode }) => (
    <Backdrop className="bg-foreground/55 fixed inset-0 z-50" isDismissable={!busy} isKeyboardDismissDisabled={busy}>
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
    </Backdrop>
);
const DraftEditor = ({ published, onPreview, ...state }: Atom.Success<typeof draft> & { published: typeof Portfolio.Type; onPreview: (data: typeof PortfolioData.Type) => void }) => {
    const idPrefix = useId();
    const publish = useRef<HTMLButtonElement>(null);
    const save = useRef<HTMLButtonElement>(null);
    const setDraft = useAtomSet(draft);
    const setTarget = useAtomSet(nextDestination);
    const pending = useAtomValue(pendingUploads);
    const deletion = useAtomValue(deleteRequest);
    const [operation, submit] = useAtom(saveRequest);
    const [reloading, reload] = useAtom(reloadRequest);
    const [selected, setSelected] = useState<typeof EntryId.Type>();
    const [workspace, setWorkspace] = useState<'entries' | 'entry' | 'place'>('entries');
    const [composition, setComposition] = useState<typeof CompositionId.Type>();
    const [assetId, setAssetId] = useState(Option.none<typeof AssetId.Type>());
    const [tab, setTab] = useState<Key>(`${idPrefix}entries`);
    useEffect(() => {
        if (AsyncResult.isSuccess(reloading) && !reloading.waiting) {
            save.current?.focus();
        }
    }, [reloading]);
    const busy = operation.waiting || deletion.waiting || reloading.waiting;
    const current = state.portfolio.entries.find((item) => item.id === selected);
    const show = (name: 'entries' | 'files' | 'identity') => setTab(`${idPrefix}${name}`);
    const open = (id: typeof EntryId.Type, view: 'entry' | 'place' = 'entry', target?: string) => {
        setSelected(id);
        setTarget(Destination.Entry({ id }));
        setWorkspace(view);
        show('entries');
        focusElement(target ?? `${idPrefix}${view}`);
    };
    const place = (entryId: typeof EntryId.Type, placed: typeof AssetId.Type) => {
        setAssetId(Option.some(placed));
        open(entryId, 'place');
    };
    const create = (kind: typeof Entry.Type.kind) => {
        const created = Entry.make({ id: EntryId.make(SecureRandom.uuidV7Unsafe()), kind });
        setDraft(portfolio.key('entries').modify(Array.append(created)));
        open(created.id, 'entry', `${idPrefix}title`);
    };
    const locate = ([head, entryIndex, part, compositionIndex]: readonly PropertyKey[], snapshot: typeof Portfolio.Type) => {
        const entry = head === 'entries' && Predicate.isNumber(entryIndex) ? Array.get(snapshot.entries, entryIndex) : Option.none();
        const failing = part === 'compositions' && Predicate.isNumber(compositionIndex) ? Option.flatMap(entry, ({ compositions }) => Array.get(compositions, compositionIndex)) : Option.none();
        const target = Option.match(failing, { onNone: () => `${idPrefix}${part === 'title' ? 'title' : 'entry'}`, onSome: Struct.get('id') });
        show(head === 'entries' ? 'entries' : 'identity');
        Match.value({ head, entry }).pipe(
            Match.when({ entry: Option.isSome }, ({ entry: found }) => open(found.value.id, 'entry', target)),
            Match.when({ head: 'entries' }, () => setWorkspace('entries')),
            Match.orElse(() => focusElement(`${idPrefix}${String(head)}`)),
        );
        Option.match(failing, { onNone: Function.constVoid, onSome: ({ id }) => setComposition(id) });
    };
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
                <Toolbar aria-label="Draft" className="flex flex-wrap items-center gap-2">
                    <button className="button button-ghost" disabled={busy} form={`${idPrefix}form`} ref={save} type="submit">
                        Save draft
                    </button>
                    <Button className="button button-ghost" isDisabled={busy} onPress={() => onPreview(state)} slot="close">
                        Preview draft
                    </Button>
                    <button aria-describedby={pending.length > 0 ? `${idPrefix}uploads` : undefined} className="button" disabled={busy || pending.length > 0} form={`${idPrefix}form`} ref={publish} type="submit">
                        Publish
                    </button>
                </Toolbar>
                <p className="text-caption text-muted min-h-4.5 w-full" role="status">
                    {status}
                </p>
                {AsyncResult.builder(operation)
                    .onError(({ error, portfolio: submitted }) => (
                        <div className="w-full">
                            <RequestAlert error={error} onField={(path) => locate(path, submitted)} />
                            {error._tag === 'PreconditionFailed' && (
                                <div className="actions">
                                    <ConfirmDialog description="Discards unsaved edits. Uploaded files stay in library. Edits stay if reload fails" disabled={busy || pending.length > 0} label="Reload latest draft" onConfirm={() => reload(undefined)} />
                                    <ConfirmDialog description={`Saves these edits over draft revision ${error.revision}. Published work changes on next publish`} disabled={busy} label="Replace newer draft" onConfirm={() => submit({ ...state, revision: error.revision, publish: false })} />
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
                    <p className="text-caption text-muted w-full" id={`${idPrefix}uploads`}>
                        {format.quantity(pending.length, { one: (shown) => `${shown} file not uploaded`, other: (shown) => `${shown} files not uploaded` })}. Upload or remove before publishing or reloading{' '}
                        <button className="quiet-link" onClick={() => show('files')} type="button">
                            View uploads
                        </button>
                    </p>
                )}
            </div>
            <fieldset className="max-sm:pb-safe m-0 border-0 px-9 pt-3 pb-15 max-sm:px-5" disabled={busy}>
                <Tabs isDisabled={busy} onSelectionChange={setTab} selectedKey={tab}>
                    <TabList aria-label="Portfolio editing" className="border-line mb-6 flex gap-1.5 border-b max-sm:w-full max-sm:flex-wrap">
                        <Tab className="editor-tab" id={`${idPrefix}entries`}>
                            Projects and studies
                        </Tab>
                        <Tab className="editor-tab" id={`${idPrefix}files`}>
                            Files{pending.length > 0 ? ` (${pending.length})` : ''}
                        </Tab>
                        <Tab className="editor-tab" id={`${idPrefix}identity`}>
                            Identity and hero
                        </Tab>
                    </TabList>
                    <TabPanel className="editor-panel" id={`${idPrefix}entries`}>
                        {Match.value({ workspace, current }).pipe(
                            Match.when({ workspace: 'place', current: Match.defined }, ({ current: entry }) => (
                                <>
                                    <div className="flex flex-wrap items-center justify-between gap-3">
                                        <h3 className="text-xl tracking-tight wrap-anywhere outline-none" id={`${idPrefix}place`} tabIndex={-1}>
                                            Add to {entryTitle(entry)}
                                        </h3>
                                        <button className="button button-ghost" onClick={() => open(entry.id)} type="button">
                                            <ArrowLeft className="size-4" />
                                            Back to entry
                                        </button>
                                    </div>
                                    <PlaceFile
                                        assetId={assetId}
                                        assets={state.assets}
                                        entryId={entry.id}
                                        key={Option.getOrUndefined(assetId)}
                                        onAdd={(id) => {
                                            setComposition(id);
                                            open(entry.id);
                                        }}
                                        onSelect={setAssetId}
                                    />
                                </>
                            )),
                            Match.when({ workspace: 'entry', current: Match.defined }, ({ current: entry }) => (
                                <div className="flex flex-col gap-5" key={entry.id}>
                                    <div className="border-line flex flex-wrap items-center justify-between gap-3 border-b pb-3">
                                        <h3 className="min-w-0 flex-1 text-xl tracking-tight wrap-anywhere outline-none" id={`${idPrefix}entry`} tabIndex={-1}>
                                            {entryTitle(entry)}
                                        </h3>
                                        <button className="button button-ghost" onClick={() => setWorkspace('entries')} type="button">
                                            Back to entries
                                        </button>
                                    </div>
                                    <EntryFields entry={entry} titleId={`${idPrefix}title`} />
                                    <Compositions
                                        assets={state.assets}
                                        entry={entry}
                                        onPlace={() => open(entry.id, 'place')}
                                        onSelect={setComposition}
                                        onUpload={() => {
                                            setTarget(Destination.Entry({ id: entry.id }));
                                            show('files');
                                        }}
                                        selected={composition}
                                    />
                                </div>
                            )),
                            Match.orElse(() => (
                                <>
                                    <div className="actions">
                                        <button className="button button-outline" id={`${idPrefix}new`} onClick={() => create('project')} type="button">
                                            New project
                                        </button>
                                        <button className="button button-ghost" onClick={() => create('study')} type="button">
                                            New study
                                        </button>
                                    </div>
                                    <EntryList entries={state.portfolio.entries} onRemove={() => focusElement(`${idPrefix}new`)} onSelect={({ id }) => open(id)} selected={selected} />
                                    {state.portfolio.entries.length === 0 && <p className="hint py-8">Create a project or study to add files</p>}
                                </>
                            )),
                        )}
                    </TabPanel>
                    <TabPanel className="editor-panel" id={`${idPrefix}files`}>
                        <Suspense fallback={<p role="status">Loading files…</p>}>
                            <Uploads assets={state.assets} entries={state.portfolio.entries} onPlace={place} />
                        </Suspense>
                        <UploadRecovery />
                        <Library assetId={assetId} assets={state.assets} onPlace={place} onSelect={setAssetId} portfolio={state.portfolio} published={published} />
                    </TabPanel>
                    <TabPanel className="editor-panel" id={`${idPrefix}identity`} shouldForceMount={true}>
                        <form
                            className="flex flex-col gap-5"
                            id={`${idPrefix}form`}
                            onInvalidCapture={(event) => {
                                event.preventDefault();
                                show('identity');
                                Option.match(Option.fromNullishOr(event.currentTarget.querySelector(':invalid')), { onNone: Function.constVoid, onSome: ({ id }) => focusElement(id) });
                            }}
                            onSubmit={(event) => {
                                event.preventDefault();
                                submit({ ...state, publish: event.submitter === publish.current });
                            }}
                        >
                            <Issues.Fields
                                issue={AsyncResult.error(operation).pipe(
                                    Option.flatMap(({ error }) =>
                                        Match.value(error).pipe(
                                            Match.tag('SchemaError', ({ issue }) => issue),
                                            Match.option,
                                        ),
                                    ),
                                )}
                            >
                                <Identity
                                    assets={state.assets}
                                    idPrefix={idPrefix}
                                    onChooseHero={() => {
                                        setTarget(Destination.Hero());
                                        show('files');
                                    }}
                                    portfolio={state.portfolio}
                                />
                            </Issues.Fields>
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
