import { Effect, Record, Schema } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { AsyncResult, Atom, AtomHttpApi } from 'effect/reactivity';
import { Api } from '../model/api.ts';
import { Portfolio, type PortfolioData } from '../model/document.ts';

// --- [SERVICES] ------------------------------------------------------------------------

class PortfolioClient extends AtomHttpApi.Service<PortfolioClient>()('PortfolioClient', { api: Api, httpClient: FetchHttpClient.layer }) {}
const draftRequest = PortfolioClient.runtime.atom(PortfolioClient.use((api) => api.content.draft({})));
const draft = Atom.writable(
    (get) => get(draftRequest),
    (context, value: Atom.Type<typeof draftRequest>) => context.setSelf(value),
    (refresh) => refresh(draftRequest),
).pipe(Atom.keepAlive);
const saveRequest = PortfolioClient.runtime
    .fn(
        Effect.fnUntraced(function* (input: { readonly snapshot: typeof PortfolioData.Type; readonly publish: boolean }) {
            const api = yield* PortfolioClient;
            yield* Schema.encodeEffect(Portfolio)(input.snapshot.portfolio).pipe(
                Effect.flatMap((payload) => (input.publish ? api.content.publish({ payload }) : api.content.save({ payload }))),
                Effect.mapError((error) => ({ error, portfolio: input.snapshot.portfolio })),
            );
            return input;
        }),
    )
    .pipe(Atom.keepAlive);
const deleteRequest = PortfolioClient.runtime
    .fn((id: string, get) =>
        PortfolioClient.use((api) => api.media.delete({ params: { id } })).pipe(
            Effect.andThen(
                Effect.sync(() =>
                    get.registry.update(
                        draft,
                        AsyncResult.map((data) => ({ ...data, assets: Record.remove(data.assets, id) })),
                    ),
                ),
            ),
        ),
    )
    .pipe(Atom.keepAlive);

// --- [EXPORTS] -------------------------------------------------------------------------

export { deleteRequest, draft, draftRequest, saveRequest };
