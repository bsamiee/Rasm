import { Schema } from 'effect';
import { HttpApi, HttpApiEndpoint, HttpApiError, HttpApiGroup, HttpApiSchema } from 'effect/http-api';

// --- [MODELS] --------------------------------------------------------------------------

const Bound = Schema.Struct({ handle: Schema.String, server: Schema.String });
const Followed = Schema.Struct({ sequence: Schema.Int, line: Schema.String });
const params = { handle: Bound.fields.handle };

class Handles extends HttpApi.make('handles').add(
    HttpApiGroup.make('handles', { topLevel: true }).add(
        HttpApiEndpoint.post('bind', '/handles', { success: Bound }),
        HttpApiEndpoint.get('follow', '/handles/:handle/events', { params, success: HttpApiSchema.StreamNdjson({ schema: Followed }), error: HttpApiError.NotFoundNoContent }),
        HttpApiEndpoint.delete('ack', '/handles/:handle/events', { params, query: { through: Followed.fields.sequence }, success: HttpApiSchema.NoContent, error: HttpApiError.NotFoundNoContent }),
    ),
) {
    static readonly Bound = Bound;
    static readonly Followed = Followed;
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Handles };
