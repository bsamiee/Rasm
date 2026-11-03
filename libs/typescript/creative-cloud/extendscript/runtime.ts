import type { Reply } from '@rasm/cep/contract';

// --- [TYPES] ---------------------------------------------------------------------------

type Replied<Tag extends (typeof Reply.Encoded)['_tag']> = Extract<typeof Reply.Encoded, { readonly _tag: Tag }>;
type Fields = Readonly<Record<string, unknown>>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const runtime = () => {
    const isRecord = (value: unknown): value is Fields => typeof value === 'object' && value !== null;
    const isList = (value: unknown): value is readonly unknown[] => Object.prototype.toString.call(value) === '[object Array]';
    const isPlain = (value: unknown): value is Fields => isRecord(value) && Object.prototype.toString.call(value) === '[object Object]';
    const finite = (value: unknown): number | undefined => (typeof value === 'number' && Math.abs(value) < Number.POSITIVE_INFINITY ? value : undefined);
    const message = (thrown: unknown): string => String(isRecord(thrown) ? thrown.message : thrown);
    const attempt = <A>(body: () => A): A | undefined => {
        try {
            return body();
        } catch {
            return undefined;
        }
    };
    const keys = (value: object): string[] => {
        const found: string[] = [];
        for (const key in value) {
            if (Object.prototype.hasOwnProperty.call(value, key)) {
                found.push(key);
            }
        }
        return found;
    };
    const read = (value: unknown, key: string): unknown => attempt(() => (isRecord(value) ? value[key] : undefined));
    const unmatched = (subject: string, query: string, candidates: readonly string[]): Replied<'Unmatched'> => ({ _tag: 'Unmatched', subject, query, candidates });
    const illegal = (text: string): Replied<'IllegalArgument'> => ({ _tag: 'IllegalArgument', message: text });
    const escaped = (text: string): string => {
        const control = 0x20;
        const hex = 16;
        const digits = -4;
        let out = '';
        for (let at = 0; at < text.length; at += 1) {
            const code = text.charCodeAt(at);
            const character = text.charAt(at);
            out += code < control || character === '"' || character === '\\' ? `\\u${`000${code.toString(hex)}`.slice(digits)}` : character;
        }
        return `"${out}"`;
    };
    const encoded = (value: unknown, specifier: (value: object) => string | undefined): string => {
        if (typeof value === 'string') {
            return escaped(value);
        }
        if (typeof value === 'boolean' || finite(value) !== undefined) {
            return String(value);
        }
        if (value === null || value === undefined || typeof value === 'number') {
            return 'null';
        }
        if (isList(value) || isPlain(value)) {
            return joined(value, specifier);
        }
        const at = isRecord(value) ? specifier(value) : undefined;
        const [type, name] = [read(read(value, 'reflect'), 'name'), read(value, 'name')];
        return at === undefined ? escaped(String(value)) : joined({ type: typeof type === 'string' ? type : String(value), specifier: at, name: typeof name === 'string' ? name : undefined, valid: true }, specifier);
    };
    const joined = (value: Fields | readonly unknown[], specifier: (value: object) => string | undefined): string => {
        const parts: string[] = [];
        if (isList(value)) {
            for (const item of value) {
                parts.push(encoded(item, specifier));
            }
            return `[${parts.join(',')}]`;
        }
        for (const key of keys(value)) {
            if (value[key] !== undefined) {
                parts.push(`${escaped(key)}:${encoded(value[key], specifier)}`);
            }
        }
        return `{${parts.join(',')}}`;
    };
    const reply = (answer: () => unknown, specifier: (value: object) => string | undefined, evaluated: (thrown: unknown) => boolean): string => {
        try {
            return encoded({ _tag: 'Value' satisfies Replied<'Value'>['_tag'], value: answer() }, specifier);
        } catch (thrown) {
            if (isRecord(thrown) && (thrown._tag === 'Unmatched' || thrown._tag === 'IllegalArgument')) {
                return encoded(thrown, specifier);
            }
            const line = evaluated(thrown) ? finite(read(thrown, 'line') ?? read(thrown, 'lineNumber')) : undefined;
            return encoded({ _tag: 'Thrown', message: message(thrown), number: finite(read(thrown, 'number')), line } satisfies Omit<Replied<'Thrown'>, 'number' | 'line'> & Record<'number' | 'line', number | undefined>, specifier);
        }
    };
    return { isRecord, isList, isPlain, finite, message, attempt, keys, read, unmatched, illegal, escaped, reply };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { runtime };
