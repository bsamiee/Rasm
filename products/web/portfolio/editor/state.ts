import { Array, Data, Effect, Optic, Option, Record, Schema, String, Struct } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { AsyncResult, Atom, AtomHttpApi, Reactivity } from 'effect/reactivity';
import { Api, type Recovery } from '#/model/api';
import type { Assets } from '#/model/asset';
import type { Composition } from '#/model/placement';
import { type Entry, Portfolio } from '#/model/portfolio';
import { preview, revised } from '#/site/state';

// --- [TYPES] ---------------------------------------------------------------------------

type Workspace = Data.TaggedEnum<{
    Entries: { readonly last: Option.Option<Entry['id']> };
    Entry: { readonly entry: Entry['id']; readonly composition: Option.Option<Composition['id']> };
    Place: { readonly entry: Entry['id']; readonly asset: Option.Option<keyof Assets['Service']> };
}>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const recovery = ['recovery'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Workspace = Data.taggedEnum<Workspace>();
const Tab = Schema.Literals(['entries', 'files', 'profile']);

// --- [SERVICES] ------------------------------------------------------------------------

class PortfolioClient extends AtomHttpApi.Service<PortfolioClient>()('@rasm/portfolio/editor/state/PortfolioClient', { api: Api, httpClient: FetchHttpClient.layer }) {}
const load = PortfolioClient.use((api) => api.documents.draft({})).pipe(Effect.map(({ body, headers }) => ({ ...body, saved: body.portfolio, revision: headers['draft-etag'] })));
const draftRequest = PortfolioClient.runtime.atom(load);
const recoveryRequest = PortfolioClient.query('assets', 'recovery', { reactivityKeys: recovery });
const draft = Atom.writable(
    (get) => get(draftRequest),
    (context, value: Atom.Type<typeof draftRequest>) => context.setSelf(value),
    (refresh) => refresh(draftRequest),
).pipe(Atom.keepAlive);
const workspace = Atom.make<Workspace>(Workspace.Entries({ last: Option.none() }));
const tab = Atom.make(Tab.make('entries'));
const chosen = Atom.make(Option.none<keyof Assets['Service']>());
const saveRequest = PortfolioClient.runtime
    .fn(
        Effect.fnUntraced(function* (submitted: Atom.Success<typeof draft> & { readonly publish: boolean }, get: Atom.FnContext) {
            const api = yield* PortfolioClient;
            const response = yield* Schema.encodeEffect(Portfolio)(submitted.portfolio).pipe(
                Effect.flatMap((payload) => (submitted.publish ? api.documents.publish : api.documents.save)({ payload, headers: { 'if-match': submitted.revision } })),
                Effect.mapError((error) => ({ error, portfolio: submitted.portfolio })),
            );
            get.registry.update(draft, loaded.modify(Struct.evolve({ saved: () => submitted.portfolio, revision: () => response.headers['draft-etag'] })));
            if (submitted.publish) {
                get.set(revised, Option.some(Struct.pick(submitted, ['portfolio', 'assets'])));
                get.set(preview, Option.none());
            }
            return submitted;
        }),
    )
    .pipe(Atom.keepAlive);
const reloadRequest = PortfolioClient.runtime
    .fn(
        Effect.fnUntraced(function* (_: undefined, get: Atom.FnContext) {
            get.set(draft, AsyncResult.success(yield* load));
            get.set(saveRequest, Atom.Reset);
        }),
    )
    .pipe(Atom.keepAlive);
const deleteRequest = PortfolioClient.runtime
    .fn(
        Effect.fnUntraced(function* (id: keyof Assets['Service'], get: Atom.FnContext) {
            yield* PortfolioClient.use((api) => api.assets.delete({ params: { id } })).pipe(Effect.tapError(() => Reactivity.invalidate(recovery)));
            get.registry.update(draft, assets.modify(Record.remove(id)));
        }),
    )
    .pipe(Atom.keepAlive);
const discardRequest = PortfolioClient.runtime
    .fn(
        Effect.fnUntraced(function* (upload: Recovery, get: Atom.FnContext) {
            yield* PortfolioClient.use((api) => api.assets.discard({ params: { id: upload.id } })).pipe(Effect.mapError((error) => ({ upload, error })));
            if (upload.status === 'removing') {
                get.registry.update(draft, assets.modify(Record.remove(upload.id)));
            }
        }),
        { reactivityKeys: recovery },
    )
    .pipe(Atom.keepAlive);
const busy = Atom.make((get) => get(saveRequest).waiting || get(deleteRequest).waiting || get(reloadRequest).waiting);

// --- [OPERATIONS] ----------------------------------------------------------------------

const loaded = Optic.id<Atom.Type<typeof draft>>()
    .refine(AsyncResult.isSuccess)
    .compose(Optic.makeLens((result: AsyncResult.Success<Atom.Success<typeof draft>, Atom.Failure<typeof draft>>) => result.value, AsyncResult.success));
const assets = loaded.key('assets');
const portfolio = loaded.key('portfolio');
const hero = portfolio.optionalKey('hero');
const updateEntry = (id: Entry['id'], update: (entry: Entry) => Entry) => portfolio.key('entries').modify(Array.map((item) => (item.id === id ? update(item) : item)));
const entered = (input: string) => Option.getOrUndefined(Option.liftPredicate(input, String.isNonEmpty));

// --- [EXPORTS] -------------------------------------------------------------------------

export { assets, busy, chosen, deleteRequest, discardRequest, draft, entered, hero, PortfolioClient, portfolio, recoveryRequest, reloadRequest, saveRequest, Tab, tab, updateEntry, Workspace, workspace };
