import { runMain } from '@effect/platform-node/NodeRuntime';
import { Cause, Config, Duration, Effect, flow, Redacted, Schedule, Schema } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientRequest, HttpClientResponse } from 'effect/http';

// --- [MODELS] --------------------------------------------------------------------------

const Job = Schema.Struct({ result: Schema.Struct({ id: Schema.String, status: Schema.String }) });

// --- [COMPOSITION] ---------------------------------------------------------------------

if (import.meta.main) {
    runMain(
        Effect.gen(function* () {
            const { account, token, bucket } = yield* Config.all({ account: Config.String('CLOUDFLARE_ACCOUNT_ID'), token: Config.Redacted('CLOUDFLARE_API_TOKEN'), bucket: Config.String('BUCKET') });
            const jobs = (yield* HttpClient.HttpClient).pipe(
                HttpClient.mapRequest(flow(HttpClientRequest.prependUrl(`https://api.cloudflare.com/client/v4/accounts/${account}/r2/buckets/${bucket}/jobs`), HttpClientRequest.bearerToken(Redacted.value(token)))),
                HttpClient.filterStatusOk,
                HttpClient.retryTransient({ schedule: Schedule.exponential(Duration.seconds(1)), times: 5 }),
            );
            const read = (request: HttpClientRequest.HttpClientRequest) => Effect.flatMap(jobs.execute(request), HttpClientResponse.schemaBodyJson(Job));
            const { result } = yield* read(HttpClientRequest.bodyJsonUnsafe(HttpClientRequest.post(''), { jobType: 'prefixDelete', prefix: '' }));
            const { result: ended } = yield* read(HttpClientRequest.get(`/${result.id}`)).pipe(Effect.repeat({ schedule: Schedule.spaced(Duration.seconds(2)), until: ({ result: { status } }) => ['COMPLETED', 'FAILED', 'CANCELLED'].includes(status) }));
            return yield* ended.status === 'COMPLETED' ? Effect.void : Effect.fail(new Cause.UnknownError(ended, `R2 job ${ended.id} emptying ${bucket} ended ${ended.status}, read its job in the Cloudflare dashboard`));
        }).pipe(Effect.provide(FetchHttpClient.layer)),
    );
}
