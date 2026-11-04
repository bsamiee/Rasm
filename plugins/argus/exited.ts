import { Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class Exited extends Schema.TaggedError<Exited, { readonly brand: unique symbol }>()('Exited', { command: Schema.String, args: Schema.Array(Schema.String), code: Schema.Int }) {
    override get message(): string {
        return `${[this.command, ...this.args].join(' ')} exited with code ${this.code}`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Exited };
