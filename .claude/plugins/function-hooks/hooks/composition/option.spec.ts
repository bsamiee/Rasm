// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, expectTypeOf, it, vi } from 'vitest';
import {
    flatMap,
    forEach,
    fromBoolean,
    fromNullable,
    fromPredicate,
    getOrElse,
    liftPredicate,
    map,
    none,
    type Option,
    optional,
    some,
    struct,
    toArray,
    traverse,
} from './option.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Plain<A> = { readonly kind: 'some'; readonly value: A } | { readonly kind: 'none' };

// The record type the three-field shape below decodes
interface Row {
    readonly a: string;
    readonly b: number;
    readonly c: string | undefined;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NONE: Plain<unknown> = { kind: 'none' };

// --- [OPERATIONS] ----------------------------------------------------------------------

const _plain = <A>(option: Option<A>): Plain<A> => option.match<Plain<A>>({ some: (value) => ({ kind: 'some', value }), none: () => _NONE });

const _positive = liftPredicate<number>((n) => n > 0);

const _isString = (value: unknown): value is string => typeof value === 'string';

const _isNumber = (value: unknown): value is number => typeof value === 'number';

const _isRow = struct({ a: _isString, b: _isNumber, c: optional(_isString) });

// The value resolved after the given count of microtask ticks, a later element resolving first proves traverse keeps list order
const _after = <A>(ticks: number, value: A): Promise<A> =>
    Array.from({ length: ticks }).reduce<Promise<A>>((pending) => pending.then((settled) => settled), Promise.resolve(value));

// --- [TESTS] ---------------------------------------------------------------------------

describe('constructors', () => {
    it('lifts a refinement, a nullable, a boolean, and a predicate', () => {
        expect(_plain(fromPredicate(_isString)('a'))).toStrictEqual({ kind: 'some', value: 'a' });
        expect(_plain(fromPredicate(_isString)(1))).toStrictEqual(_NONE);
        expect(_plain(fromNullable('a'))).toStrictEqual({ kind: 'some', value: 'a' });
        expect(_plain(fromNullable(null))).toStrictEqual(_NONE);
        expect(_plain(fromNullable(undefined))).toStrictEqual(_NONE);
        expect(_plain(fromBoolean(true))).toStrictEqual({ kind: 'some', value: true });
        expect(_plain(fromBoolean(false))).toStrictEqual(_NONE);
        expect(_plain(liftPredicate<number>((value) => value > 0)(1))).toStrictEqual({ kind: 'some', value: 1 });
        expect(_plain(liftPredicate<number>((value) => value > 0)(0))).toStrictEqual(_NONE);
    });
});

describe('operations', () => {
    it('maps and binds under the some and keeps the none', () => {
        expect(_plain(map((value: number) => value + 1)(some(1)))).toStrictEqual({ kind: 'some', value: 2 });
        expect(_plain(map((value: number) => value + 1)(none<number>()))).toStrictEqual(_NONE);
        expect(_plain(flatMap((value: number) => fromNullable([1, 2][value]))(some(1)))).toStrictEqual({ kind: 'some', value: 2 });
        expect(_plain(flatMap((value: number) => fromNullable([1, 2][value]))(some(2)))).toStrictEqual(_NONE);
    });

    it('answers the value or the fallback and one element or none', () => {
        expect(getOrElse(() => 'b')(some('a'))).toBe('a');
        expect(getOrElse(() => 'b')(none<string>())).toBe('b');
        expect(toArray(some('a'))).toStrictEqual(['a']);
        expect(toArray(none<string>())).toStrictEqual([]);
    });
});

describe('refinements', () => {
    it('decodes a record whose fields pass and refuses every other value', () => {
        expect(_plain(fromPredicate(_isRow)({ a: 'a', b: 1, c: 'c' }))).toStrictEqual({ kind: 'some', value: { a: 'a', b: 1, c: 'c' } });
        expect(_plain(fromPredicate(_isRow)({ a: 'a', b: 1 }))).toStrictEqual({ kind: 'some', value: { a: 'a', b: 1 } });
        expect(_plain(fromPredicate(_isRow)({ a: 'a', b: 'b', c: 'c' }))).toStrictEqual(_NONE);
        expect(_plain(fromPredicate(_isRow)({ a: 'a', c: 'c' }))).toStrictEqual(_NONE);
        expect(_plain(fromPredicate(_isRow)({ a: 'a', b: 1, c: 1 }))).toStrictEqual(_NONE);
        expect(_plain(fromPredicate(_isRow)(null))).toStrictEqual(_NONE);
        expect(_plain(fromPredicate(_isRow)('a'))).toStrictEqual(_NONE);
        expect(_plain(fromPredicate(_isRow)(['a', 1]))).toStrictEqual(_NONE);
    });

    it('narrows to the record type of the shape', () => {
        const decodeRow: (value: unknown) => Option<Row> = fromPredicate(_isRow);
        expectTypeOf(_isRow).guards.toEqualTypeOf<Row>();
        expect(_plain(decodeRow({ a: 'a', b: 1 }))).toStrictEqual({ kind: 'some', value: { a: 'a', b: 1 } });
    });
});

describe('asynchronous operations', () => {
    it('traverses in list order, drops the nones, and calls once per element', async () => {
        const positive = vi.fn((value: number): Promise<Option<number>> => _after(2 - value, _positive(value)));
        await expect(traverse(positive)([0, 1, 2])).resolves.toStrictEqual([1, 2]);
        expect(positive.mock.calls.map(([value]) => value)).toStrictEqual([0, 1, 2]);
        await expect(traverse(positive)([])).resolves.toStrictEqual([]);
        expect(positive.mock.calls.map(([value]) => value)).toStrictEqual([0, 1, 2]);
    });

    it('runs the function under a some, skips it under a none, and propagates a rejection', async () => {
        const increment = vi.fn((value: number): Promise<number> => Promise.resolve(value + 1));
        await expect(forEach(increment)(some(1)).then(_plain)).resolves.toStrictEqual({ kind: 'some', value: 2 });
        await expect(forEach(increment)(none<number>()).then(_plain)).resolves.toStrictEqual(_NONE);
        expect(increment).toHaveBeenCalledTimes(1);
        await expect(forEach((): Promise<number> => Promise.reject(new Error('failed')))(some(1))).rejects.toThrow('failed');
    });
});
