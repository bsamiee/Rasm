import { BrowserCrypto } from '@effect/platform-browser';
import { Crypto, Effect, Option, Record, Schema, Struct } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { AsyncResult, Atom, AtomHttpApi } from 'effect/reactivity';
import { Api, type PendingUpload, type Revision } from '../model/api.ts';
import { Portfolio, type PortfolioData } from '../model/document.ts';

// --- [SERVICES] ------------------------------------------------------------------------

class PortfolioClient extends AtomHttpApi.Service<PortfolioClient>()('PortfolioClient', { api: Api, httpClient: FetchHttpClient.layer }) {}
const draftRequest = PortfolioClient.runtime.atom(PortfolioClient.use((api) => api.content.draft({})));
const pendingRequest = PortfolioClient.runtime.atom(PortfolioClient.use((api) => api.media.pending({})));
const draft = Atom.writable(
    (get) => AsyncResult.map(get(draftRequest), Struct.get('body')),
    (context, value: AsyncResult.AsyncResult<typeof PortfolioData.Type, Atom.Failure<typeof draftRequest>>) => context.setSelf(value),
    (refresh) => refresh(draftRequest),
).pipe(Atom.keepAlive);
const saveRequest = PortfolioClient.runtime
    .fn(
        Effect.fnUntraced(function* ({ snapshot, publish, revision }: { readonly snapshot: typeof PortfolioData.Type; readonly publish: boolean; readonly revision: typeof Revision.Type }) {
            const api = yield* PortfolioClient;
            const response = yield* Schema.encodeEffect(Portfolio)(snapshot.portfolio).pipe(
                Effect.flatMap((payload) => (publish ? api.content.publish : api.content.save)({ payload, headers: { 'if-match': revision } })),
                Effect.mapError((error) => ({ error, portfolio: snapshot.portfolio })),
            );
            return { snapshot, publish, revision: response.headers['draft-etag'] };
        }),
    )
    .pipe(Atom.keepAlive);
const reloadRequest = PortfolioClient.runtime
    .fn((_: undefined, get) =>
        PortfolioClient.use((api) => api.content.draft({})).pipe(
            Effect.tap((response) =>
                Effect.sync(() => {
                    get.registry.set(draft, AsyncResult.success(response.body));
                    get.registry.set(saveRequest, Atom.Reset);
                }),
            ),
        ),
    )
    .pipe(Atom.keepAlive);
const draftState = Atom.make((get) => {
    const stored = ({ body, headers }: Atom.Success<typeof draftRequest>) => ({ portfolio: body.portfolio, revision: headers['draft-etag'] });
    return AsyncResult.all({
        data: get(draft),
        saved: Option.match(Option.firstSomeOf([Option.map(AsyncResult.value(get(saveRequest)), ({ snapshot, revision }) => ({ portfolio: snapshot.portfolio, revision })), Option.map(AsyncResult.value(get(reloadRequest)), stored)]), {
            onNone: () => AsyncResult.map(get(draftRequest), stored),
            onSome: AsyncResult.success,
        }),
    }).pipe(AsyncResult.map(({ data, saved }) => ({ data, revision: saved.revision, changed: data.portfolio !== saved.portfolio })));
});
const deleteRequest = PortfolioClient.runtime
    .fn((id: string, get) =>
        PortfolioClient.use((api) => api.media.delete({ params: { id } })).pipe(
            Effect.tapError(() => Effect.sync(() => get.registry.refresh(pendingRequest))),
            Effect.andThen(Effect.sync(() => get.registry.update(draft, AsyncResult.map(Struct.evolve({ assets: Record.remove(id) }))))),
        ),
    )
    .pipe(Atom.keepAlive);
const discardRequest = PortfolioClient.runtime
    .fn((upload: typeof PendingUpload.Type, get) =>
        PortfolioClient.use((api) => api.media.discard({ params: { id: upload.id } })).pipe(
            Effect.mapError((error) => ({ upload, error })),
            Effect.tap(() =>
                Effect.sync(() => {
                    if (upload.status === 'removing') {
                        get.registry.update(draft, AsyncResult.map(Struct.evolve({ assets: Record.remove(upload.id) })));
                    }
                    get.registry.refresh(pendingRequest);
                }),
            ),
        ),
    )
    .pipe(Atom.keepAlive);

// --- [OPERATIONS] ----------------------------------------------------------------------

const identifier = Crypto.Crypto.use((crypto) => crypto.randomUUIDv7()).pipe(Effect.provide(BrowserCrypto.layer));

// --- [EXPORTS] -------------------------------------------------------------------------

export { deleteRequest, discardRequest, draft, draftState, identifier, pendingRequest, reloadRequest, saveRequest };
