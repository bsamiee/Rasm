import { Array, Data, Option, pipe } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const radix = 10;
const decimal = /^\p{Nd}$/u;

// --- [MODELS] --------------------------------------------------------------------------

class Digits extends Data.Class<{ readonly system: string; readonly table: readonly string[] }> {
    static readonly #point = (digit: string): Option.Option<number> =>
        Option.flatMap(
            Option.liftPredicate(digit, (single) => decimal.test(single)),
            (single) => Option.fromUndefinedOr(single.codePointAt(0)),
        );
    static readonly #place = (code: number): number =>
        Array.unfold(code - 1, (previous) =>
            Option.map(
                Option.liftPredicate(previous, (candidate) => decimal.test(String.fromCodePoint(candidate))),
                (run) => [run, run - 1] as const,
            ),
        ).length % radix;
    static readonly #table = (system: string): readonly string[] => {
        const format = new Intl.NumberFormat('en', { numberingSystem: system });
        return Array.makeBy(radix, (digit) =>
            format
                .formatToParts(digit)
                .filter(({ type }) => type === 'integer')
                .map(({ value }) => value)
                .join(''),
        );
    };
    static readonly systems: readonly string[] = Intl.supportedValuesOf('numberingSystem').filter((system) =>
        pipe(
            Option.all(Array.map(Digits.#table(system), Digits.#point)),
            Option.exists((codes) => Array.dedupe(Array.map(codes, (code, digit) => code - digit)).length === 1),
        ),
    );
    static readonly of = (system: string): Digits => new Digits({ system, table: Digits.#table(system) });
    static readonly fold = (text: string): string => text.replace(/\p{Nd}/gu, (digit) => Option.match(Digits.#point(digit), { onNone: () => digit, onSome: (code) => `${Digits.#place(code)}` }));

    format(text: string): string {
        return text.replace(/[0-9]/gu, (digit) => Array.getUnsafe(this.table, Number(digit)));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Digits };
