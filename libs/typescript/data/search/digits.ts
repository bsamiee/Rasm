import { Array, Data, Option, pipe } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ASCII = '0123456789';
const DECIMAL = /\p{Nd}/gu;

// --- [MODELS] --------------------------------------------------------------------------

class Digits extends Data.Class<{ readonly system: string; readonly table: readonly string[] }> {
    static readonly #value = (digit: string): number =>
        Array.unfold((digit.codePointAt(0) ?? 0) - 1, (previous) =>
            Option.map(
                Option.liftPredicate(previous, (candidate) => String.fromCodePoint(candidate).search(DECIMAL) === 0),
                (codePoint) => [codePoint, codePoint - 1] as const,
            ),
        ).length % ASCII.length;
    static readonly fold = (text: string): string => text.replace(DECIMAL, (digit) => `${Digits.#value(digit)}`);
    static readonly of = (system: string): Option.Option<Digits> =>
        pipe(
            system,
            Option.liftPredicate((given: string) => Array.contains(Intl.supportedValuesOf('numberingSystem'), given)),
            Option.map((known) => {
                const { format } = new Intl.NumberFormat('en', { numberingSystem: known });
                return new Digits({ system: known, table: Array.makeBy(ASCII.length, format) });
            }),
            Option.filter(({ table }) => Digits.fold(table.join('')) === ASCII),
        );
    static get systems(): readonly string[] {
        return Intl.supportedValuesOf('numberingSystem').filter((system) => Option.isSome(Digits.of(system)));
    }

    format(text: string): string {
        return text.replace(/\d/gu, (digit) => Array.getUnsafe(this.table, Number(digit)));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Digits };
