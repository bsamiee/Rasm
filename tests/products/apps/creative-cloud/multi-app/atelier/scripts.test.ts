import { assert, describe, it } from '@effect/vitest';
import { routed } from '@rasm/atelier/scripts';
import { Effect, Number, Option, Ref } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const command = (languages: readonly string[]) => Option.some({ name: 'do script', code: 'miscdosc', parameters: [{ name: 'language', code: 'kLng', list: false, enumerators: languages.map((code) => ({ name: code, code })) }] });
const route = Effect.fnUntraced(function* (product: string, version: string, script: ReturnType<typeof command> | Option.None<never>) {
    const reads = yield* Ref.make(0);
    const terms = () => Effect.as(Ref.update(reads, Number.increment), { script });
    const found = yield* routed(terms, { url: new URL(`file:///Applications/${product}.app/`), product, version });
    return { found, taken: { runtime: Option.match(found, { onNone: () => 'none', onSome: ({ _tag }) => _tag }), reads: yield* Ref.get(reads) } };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('script route', () => {
    it.effect.each([
        { named: 'refuses a UXP host that declares no UXP script command, without reading its dictionary', product: 'PHSP', version: '28.0.0', script: command([]), expected: { runtime: 'Unlinked', reads: 0 } },
        { named: 'runs do script uxpscript where the dictionary lists the UXP script language', product: 'IDSN', version: '22.0.0.104', script: command(['USLg', 'JScr']), expected: { runtime: 'Uxp', reads: 1 } },
        { named: 'refuses a UXP host whose dictionary lists no UXP script language', product: 'IDSN', version: '22.0.0.104', script: command(['JScr']), expected: { runtime: 'Unlinked', reads: 1 } },
        { named: 'runs ExtendScript in a host that runs no installed plugin', product: 'ILST', version: '31.1.0', script: command([]), expected: { runtime: 'Script', reads: 1 } },
        { named: 'runs ExtendScript in a release older than the first UXP one of its host', product: 'AEFT', version: '27.0.9', script: command([]), expected: { runtime: 'Script', reads: 1 } },
        { named: 'refuses the first UXP release of a host', product: 'AEFT', version: '27.1.0', script: command([]), expected: { runtime: 'Unlinked', reads: 0 } },
        { named: 'finds no runtime in an application that declares no script command', product: 'com.example.app', version: '26.003.22110', script: Option.none(), expected: { runtime: 'none', reads: 1 } },
    ])('$named', ({ product, version, script, expected }) => Effect.map(route(product, version, script), ({ taken }) => assert.deepStrictEqual(taken, expected)));

    it.effect('wraps an Illustrator script in the alert suppression its script host states', () => Effect.map(route('ILST', '31.0.0', command([])), ({ found }) => assert.match(Option.match(found, { onNone: () => '', onSome: (runtime) => (runtime._tag === 'Script' ? runtime.around : '') }), /userInteractionLevel/u)));
});
