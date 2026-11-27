import { Schema } from 'effect';
import { HttpApi, HttpApiEndpoint, HttpApiError, HttpApiGroup } from 'effect/http-api';

// --- [MODELS] --------------------------------------------------------------------------

class Binding extends Schema.Opaque<Binding>()(Schema.Struct({ handle: Schema.String, server: Schema.String })) {}
const Row = Schema.Struct({ key: Schema.String, text: Schema.String });
class Notice extends Schema.Class<Notice, { readonly brand: unique symbol }>('Notice')({
    sequence: Schema.Int,
    text: Schema.String,
    push: Schema.Boolean,
    wake: Schema.Boolean,
    row: Schema.OptionFromOptionalKey(Row),
    clears: Schema.OptionFromOptionalKey(Row.fields.key),
}) {}
const Rows = Schema.Struct({ revision: Schema.Int, items: Schema.Array(Row) });
const Polled = Schema.Struct({ notices: Schema.Array(Notice), rows: Rows });
const params = { handle: Binding.fields.handle };

class Handles extends HttpApi.make('handles').add(
    HttpApiGroup.make('handles', { topLevel: true }).add(
        HttpApiEndpoint.post('bind', '/handles', { success: Binding }),
        HttpApiEndpoint.get('poll', '/handles/:handle/notices', {
            params,
            query: { after: Notice.fields.sequence, rows: Rows.fields.revision, idle: Schema.Boolean, wait: Schema.Finite.check(Schema.isGreaterThan(0)) },
            success: Polled,
            error: HttpApiError.NotFoundNoContent,
        }),
        HttpApiEndpoint.post('press', '/handles/:handle/rows/:key', { params: { ...params, key: Row.fields.key }, error: [HttpApiError.NotFoundNoContent, HttpApiError.ServiceUnavailableNoContent] }),
        HttpApiEndpoint.delete('release', '/handles/:handle', { params, error: HttpApiError.NotFoundNoContent }),
    ),
) {
    static readonly Binding = Binding;
    static readonly Rows = Rows;
    static readonly Polled = Polled;
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Handles, Notice };
