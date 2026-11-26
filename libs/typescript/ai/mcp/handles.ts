import { Array, Option, Result, Schema, Struct } from 'effect';
import { HttpApi, HttpApiEndpoint, HttpApiError, HttpApiGroup, HttpApiSchema } from 'effect/http-api';
import { Host, NetAddress } from 'effect/net';

// --- [MODELS] --------------------------------------------------------------------------

class Binding extends Schema.Opaque<Binding>()(Schema.Struct({ handle: Schema.String, server: Schema.String })) {}
class Event extends Schema.Opaque<Event>()(Schema.Struct({ sequence: Schema.Int, line: Schema.String })) {}
const params = { handle: Binding.fields.handle };

class Handles extends HttpApi.make('handles').add(
    HttpApiGroup.make('handles', { topLevel: true }).add(
        HttpApiEndpoint.post('bind', '/handles', { query: { agent: Schema.optionalKey(Schema.String.annotate({ description: 'Key of the agent the handle serves, the same key returning its live handle' })) }, success: Binding }),
        HttpApiEndpoint.get('events', '/handles/:handle/events', {
            params,
            query: { follow: Schema.Boolean.annotate({ description: 'Streams each later event as NDJSON when true, else answers the queued events once as a JSON array' }) },
            success: [Schema.Array(Event), HttpApiSchema.StreamNdjson({ schema: Event })],
            error: HttpApiError.NotFoundNoContent,
        }),
        HttpApiEndpoint.delete('ack', '/handles/:handle/events', { params, query: { through: Event.fields.sequence }, success: HttpApiSchema.NoContent, error: HttpApiError.NotFoundNoContent }),
    ),
) {
    static readonly Binding = Binding;
    static readonly Event = Event;
    static readonly report = (server: string, events: Array.NonEmptyReadonlyArray<Event>) => [`MCP server ${server} reports:`, ...Array.map(events, Struct.get('line'))].join('\n');
    static readonly local = (hosts: Host.HostsTable) => (origin: string) =>
        Option.exists(Option.fromNullishOr(URL.parse(origin)), ({ host }) =>
            Result.match(Host.hostPortFromString(host), {
                onFailure: () => false,
                onSuccess: (bound) => (Host.isDomainName(bound.host) ? (hosts.get(Host.toRelative(bound.host)) ?? []) : Array.filter([bound.host], NetAddress.isIpAddress)).some(NetAddress.isLoopback),
            }),
        );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Handles };
