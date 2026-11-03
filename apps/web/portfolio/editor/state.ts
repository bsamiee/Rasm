import { Array, Effect, Optic, Record, Schema, Struct } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { AsyncResult, Atom, AtomHttpApi } from 'effect/reactivity';
import { Api, type Recovery } from '#/model/api.ts';
import type { AssetId } from '#/model/asset.ts';
import { type Entry, type EntryId, Portfolio } from '#/model/portfolio.ts';

// --- [SERVICES] ------------------------------------------------------------------------

class PortfolioClient extends AtomHttpApi.Service<PortfolioClient>()('@rasm/portfolio/editor/state/PortfolioClient', { api: Api, httpClient: FetchHttpClient.layer }) {}
const load = PortfolioClient.use((api) => api.documents.draft({})).pipe(Effect.map(({ body, headers }) => ({ ...body, saved: body.portfolio, revision: headers['draft-etag'] })));
const draftRequest = PortfolioClient.runtime.atom(load);
const recoveryRequest = PortfolioClient.runtime.atom(PortfolioClient.use((api) => api.assets.recovery({})));
const draft = Atom.writable(
    (get) => get(draftRequest),
    (context, value: Atom.Type<typeof draftRequest>) => context.setSelf(value),
    (refresh) => refresh(draftRequest),
).pipe(Atom.keepAlive);
const saveRequest = PortfolioClient.runtime
    .fn(
        Effect.fnUntraced(function* (submitted: Atom.Success<typeof draft> & { readonly publish: boolean }, get) {
            const api = yield* PortfolioClient;
            const response = yield* Schema.encodeEffect(Portfolio)(submitted.portfolio).pipe(
                Effect.flatMap((payload) => (submitted.publish ? api.documents.publish : api.documents.save)({ payload, headers: { 'if-match': submitted.revision } })),
                Effect.mapError((error) => ({ error, portfolio: submitted.portfolio })),
            );
            get.registry.update(draft, AsyncResult.map(Struct.evolve({ saved: () => submitted.portfolio, revision: () => response.headers['draft-etag'] })));
            return submitted;
        }),
    )
    .pipe(Atom.keepAlive);
const reloadRequest = PortfolioClient.runtime
    .fn((_: undefined, get) =>
        Effect.tap(load, (state) =>
            Effect.sync(() => {
                get.registry.set(draft, AsyncResult.success(state));
                get.registry.set(saveRequest, Atom.Reset);
            }),
        ),
    )
    .pipe(Atom.keepAlive);
const deleteRequest = PortfolioClient.runtime
    .fn((id: typeof AssetId.Type, get) =>
        PortfolioClient.use((api) => api.assets.delete({ params: { id } })).pipe(
            Effect.tapError(() => Effect.sync(() => get.registry.refresh(recoveryRequest))),
            Effect.andThen(Effect.sync(() => get.registry.update(draft, AsyncResult.map(assets.modify(Record.remove(id)))))),
        ),
    )
    .pipe(Atom.keepAlive);
const discardRequest = PortfolioClient.runtime
    .fn((upload: typeof Recovery.Type, get) =>
        PortfolioClient.use((api) => api.assets.discard({ params: { id: upload.id } })).pipe(
            Effect.mapError((error) => ({ upload, error })),
            Effect.tap(() =>
                Effect.sync(() => {
                    if (upload.status === 'removing') {
                        get.registry.update(draft, AsyncResult.map(assets.modify(Record.remove(upload.id))));
                    }
                    get.registry.refresh(recoveryRequest);
                }),
            ),
        ),
    )
    .pipe(Atom.keepAlive);

// --- [OPERATIONS] ----------------------------------------------------------------------

const assets = Optic.id<Atom.Success<typeof draft>>().key('assets');
const portfolio = Optic.id<Atom.Success<typeof draft>>().key('portfolio');
const hero = portfolio.optionalKey('hero');
const updateEntry = (id: typeof EntryId.Type, update: (entry: typeof Entry.Type) => typeof Entry.Type) => portfolio.key('entries').modify(Array.map((item) => (item.id === id ? update(item) : item)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { assets, deleteRequest, discardRequest, draft, hero, portfolio, recoveryRequest, reloadRequest, saveRequest, updateEntry };
