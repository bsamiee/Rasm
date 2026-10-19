import { Context, Option, Schema } from 'effect';
import type { RpcClient, RpcClientError, RpcGroup } from 'effect/rpc';
import type { Protocol } from './installation.ts';

// --- [ERRORS] --------------------------------------------------------------------------

class AutomationError extends Schema.TaggedError<AutomationError, { readonly brand: unique symbol }>()('AutomationError', {
    reason: Schema.TaggedUnion({
        Refused: { failures: Schema.NonEmptyArray(Schema.String) },
        Script: { text: Schema.String, error: Schema.OptionFromNullOr(Schema.Struct({ number: Schema.Int, line: Schema.Int })) },
    }),
}) {
    override get message(): string {
        return AutomationError.fields.reason.match(this.reason, {
            Refused: ({ failures }) => failures.join('\n'),
            Script: ({ text, error }) => Option.match(error, { onNone: () => text, onSome: ({ number, line }) => `Error ${number}: ${text}, line ${line}` }),
        });
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Helper extends Context.Service<Helper, RpcClient.RpcClient<RpcGroup.Rpcs<typeof Protocol>, RpcClientError.RpcClientError>>()('@rasm/automation/helper') {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { AutomationError, Helper };
