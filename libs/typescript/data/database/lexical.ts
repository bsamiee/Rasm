import { Hash, String } from 'effect';
import { Statement } from 'effect/sql';

// --- [CONSTANTS] -----------------------------------------------------------------------

const transforms = { transformQueryNames: String.camelToSnake, transformResultNames: String.snakeToCamel, transformJson: false };

// --- [OPERATIONS] ----------------------------------------------------------------------

const quote = Statement.defaultEscape('"');
const literal = (text: string) => `'${text.replaceAll("'", "''")}'`;
const identifier = (parts: readonly string[]) => {
    const name = parts.map(transforms.transformQueryNames).join('_');
    const limit = 63;
    const radix = 36;
    const digest = Math.abs(Hash.string(name)).toString(radix);
    const encoder = new TextEncoder();
    return encoder.encode(name).length <= limit ? name : `${name.slice(0, encoder.encodeInto(name, new Uint8Array(limit - digest.length - 1)).read)}_${digest}`;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { identifier, literal, quote, transforms };
