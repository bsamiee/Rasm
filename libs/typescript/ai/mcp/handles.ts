import { Schema } from 'effect';
import { HttpApi, HttpApiEndpoint, HttpApiError, HttpApiGroup, HttpApiSchema } from 'effect/http-api';

// --- [MODELS] --------------------------------------------------------------------------

const Binding = Schema.Struct({ handle: Schema.String, server: Schema.String });
const Event = Schema.Struct({ sequence: Schema.Int, line: Schema.String });
const params = { handle: Binding.fields.handle };

class Handles extends HttpApi.make('handles').add(
    HttpApiGroup.make('handles', { topLevel: true }).add(
        HttpApiEndpoint.post('bind', '/handles', { success: Binding }),
        HttpApiEndpoint.get('follow', '/handles/:handle/events', { params, success: HttpApiSchema.StreamNdjson({ schema: Event }), error: HttpApiError.NotFoundNoContent }),
        HttpApiEndpoint.delete('ack', '/handles/:handle/events', { params, query: { through: Event.fields.sequence }, success: HttpApiSchema.NoContent, error: HttpApiError.NotFoundNoContent }),
    ),
) {
    static readonly Binding = Binding;
    static readonly Event = Event;
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Handles };
