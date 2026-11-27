import { Schema } from 'effect';
import { HttpApi, HttpApiEndpoint, HttpApiError, HttpApiGroup, HttpApiSchema } from 'effect/http-api';

// --- [MODELS] --------------------------------------------------------------------------

class Binding extends Schema.Opaque<Binding>()(Schema.Struct({ handle: Schema.String, server: Schema.String })) {}
const Kind = Schema.Literals(['asks', 'hung', 'crashed', 'exited']);
const Standing = Schema.TaggedUnion({ Waiting: { key: Schema.String }, Settled: { key: Schema.String } });
const Event = Schema.TaggedUnion({
    Line: { sequence: Schema.Int, line: Schema.String, kind: Kind },
    Waiting: { sequence: Schema.Int, key: Schema.String, line: Schema.String },
    Settled: { key: Schema.String },
});
const params = { handle: Binding.fields.handle };

class Handles extends HttpApi.make('handles').add(
    HttpApiGroup.make('handles', { topLevel: true }).add(
        HttpApiEndpoint.post('bind', '/handles', { success: Binding }),
        HttpApiEndpoint.get('follow', '/handles/:handle/events', { params, success: HttpApiSchema.StreamNdjson({ schema: Event }), error: HttpApiError.NotFoundNoContent }),
        HttpApiEndpoint.delete('ack', '/handles/:handle/events', { params, query: { through: Event.cases.Line.fields.sequence }, success: HttpApiSchema.NoContent, error: HttpApiError.NotFoundNoContent }),
    ),
) {
    static readonly Binding = Binding;
    static readonly Kind = Kind;
    static readonly Standing = Standing;
    static readonly Event = Event;
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Handles };
