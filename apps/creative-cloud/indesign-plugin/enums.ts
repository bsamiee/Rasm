// --- [IMPORTS] -------------------------------------------------------------------------

import { enumerations, phrases } from '@rasm/creative-cloud-server/indesign';
import { Array, Option, Predicate, Record } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Live = Readonly<Record<string, Readonly<Record<string, unknown>>>>;

interface Constant {
    readonly enumeration: string;
    readonly constant: string;
}

// --- [TABLE] ---------------------------------------------------------------------------

const _constants = (enumeration: object): Readonly<Record<string, unknown>> =>
    Record.fromIterableWith(
        Array.filter(Object.getOwnPropertyNames(Object.getPrototypeOf(enumeration)), (spelling) => spelling === spelling.toUpperCase()),
        (spelling) => {
            const constant = Reflect.get(enumeration, spelling);
            return [String(constant), constant];
        },
    );

const _enumeration = (value: unknown): Option.Option<Readonly<Record<string, unknown>>> =>
    Option.map(
        Option.filter(Option.liftPredicate(value, Predicate.isObject), (candidate) => candidate.constructor.name === 'Enumeration'),
        _constants,
    );

const live = (registered: object): Live =>
    Record.fromEntries(Array.getSomes(Array.map(Object.getOwnPropertyNames(registered), (name) => Option.map(_enumeration(Reflect.get(registered, name)), (constants) => [name, constants] as const))));

// --- [MATCHING] ------------------------------------------------------------------------

const _folded = (text: string): string => text.replaceAll('_', '').replaceAll(' ', '').toLowerCase();

const _registered = (table: Live, candidates: readonly string[]): readonly Constant[] =>
    Array.flatMap(candidates, (enumeration) => Array.map(Option.match(Record.get(table, enumeration), { onNone: () => [], onSome: Record.keys }), (constant) => ({ enumeration, constant })));

const _one = (matches: readonly Constant[]): Option.Option<Constant> =>
    Array.match(
        Array.dedupeWith(matches, (left, right) => left.enumeration === right.enumeration && left.constant === right.constant),
        {
            onEmpty: Option.none,
            onNonEmpty: (found) => (found.length === 1 ? Option.some(Array.headNonEmpty(found)) : Option.none()),
        },
    );

const _phrased = ({ enumeration, constant }: Constant, text: string): boolean =>
    _folded(constant) === _folded(text) || Option.exists(Option.flatMap(Record.get(phrases, enumeration), Record.get(constant)), (phrase) => _folded(phrase) === _folded(text));

const named = (table: Live, candidates: readonly string[], text: string): Option.Option<Constant> => {
    const rows = _registered(table, candidates);
    return Option.orElse(_one(Array.filter(rows, ({ constant }) => constant === text)), () => _one(Array.filter(rows, (row) => _phrased(row, text))));
};

const coded = (table: Live, candidates: readonly string[], value: number): Option.Option<Constant> =>
    _one(Array.filter(_registered(table, candidates), ({ enumeration, constant }) => Option.contains(Option.flatMap(Record.get(enumerations, enumeration), Record.get(constant)), value)));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Constant, Live };
export { coded, live, named };
