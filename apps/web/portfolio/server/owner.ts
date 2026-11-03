import { Relation } from '@rasm/database/relation';
import { unavailable } from '@rasm/hosting/unavailable';
import { User } from '@rasm/hosting/user';
import { Array, Context, Effect, Layer, Option } from 'effect';
import { HttpMethod, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { HttpApiError } from 'effect/http-api';
import { Owner } from '#/model/api.ts';
import { sites } from '#/model/sites.ts';
import { owner } from '#/server/store/catalog.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const address = new Intl.Collator(undefined, { usage: 'search', sensitivity: 'accent' });

// --- [SERVICES] ------------------------------------------------------------------------

class OwnerAccount extends Context.Service<OwnerAccount, { readonly email: string }>()('@rasm/portfolio/server/owner/OwnerAccount') {
    static readonly session = Effect.gen(function* () {
        const [{ email }, user] = yield* Effect.all([OwnerAccount, User.current(sites)]);
        return yield* Option.match(user, {
            onNone: () => Effect.succeed('signedOut' as const),
            onSome: Effect.fnUntraced(function* (signed) {
                const enrolled = Option.map(Array.head(yield* Relation.select(owner, { email: [email] })), ({ userId }) => userId);
                const ownerId = Option.isNone(enrolled) && Option.exists(signed.email, (value) => address.compare(value, email) === 0) ? Option.some((yield* Relation.ensure(owner, [{ email, userId: signed.id }]))[0].userId) : enrolled;
                return Option.contains(ownerId, signed.id) ? ('owner' as const) : ('signedIn' as const);
            }),
        });
    }).pipe(Effect.catchTag(['SqlError', 'SchemaError'], unavailable), Effect.withSpan('OwnerAccount.session'));
    static readonly authorize = Effect.filterOrFail(
        OwnerAccount.session,
        (current) => current === 'owner',
        () => HttpApiError.Forbidden.make(),
    );
    static readonly sessionScoped = HttpServerResponse.setHeader('Cache-Control', 'private, no-store');
    static readonly middleware = Layer.effect(
        Owner,
        Effect.map(
            Effect.context<Exclude<Effect.Services<typeof OwnerAccount.authorize>, HttpServerRequest.HttpServerRequest>>(),
            (context) =>
                (effect): ReturnType<typeof Owner.Service> =>
                    HttpServerRequest.HttpServerRequest.pipe(
                        Effect.filterOrFail(
                            ({ method, headers }) => !HttpMethod.hasBody(method) || headers['sec-fetch-site'] === 'same-origin',
                            () => HttpApiError.Forbidden.make(),
                        ),
                        Effect.andThen(OwnerAccount.authorize),
                        Effect.provideContext(context),
                        Effect.andThen(effect),
                        Effect.map(OwnerAccount.sessionScoped),
                    ),
        ),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { OwnerAccount };
