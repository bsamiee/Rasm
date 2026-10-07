import { Effect, Record, Struct } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { AsyncResult, Atom, AtomHttpApi } from 'effect/reactivity';
import { ClientApi, type PendingUpload } from '../model/api.ts';
import type { PortfolioData } from '../model/document.ts';

// --- [SERVICES] ------------------------------------------------------------------------

class PortfolioClient extends AtomHttpApi.Service<PortfolioClient>()('PortfolioClient', { api: ClientApi, httpClient: FetchHttpClient.layer }) {}
const draftRequest = PortfolioClient.runtime.atom(PortfolioClient.use((api) => api.content.draft({})));
const pendingRequest = PortfolioClient.runtime.atom(PortfolioClient.use((api) => api.media.pending({})));
const draft = Atom.writable(
    (get) => AsyncResult.map(get(draftRequest), Struct.get('body')),
    (context, value: AsyncResult.AsyncResult<typeof PortfolioData.Type, Atom.Failure<typeof draftRequest>>) => context.setSelf(value),
    (refresh) => refresh(draftRequest),
).pipe(Atom.keepAlive);
const saveRequest = PortfolioClient.runtime
    .fn(
        Effect.fnUntraced(function* (input: { readonly snapshot: typeof PortfolioData.Type; readonly publish: boolean; readonly etag: string }) {
            const api = yield* PortfolioClient;
            const request = { payload: input.snapshot.portfolio, headers: { 'if-match': input.etag } };
            const response = yield* (input.publish ? api.content.publish(request) : api.content.save(request)).pipe(Effect.mapError((error) => ({ error, portfolio: input.snapshot.portfolio })));
            return { ...input, etag: response.headers['draft-etag'] };
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { deleteRequest, discardRequest, draft, draftRequest, pendingRequest, reloadRequest, saveRequest };
