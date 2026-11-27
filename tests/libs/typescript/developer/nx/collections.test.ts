import { dirname, join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';
import { readJsonFile, workspaceRoot } from '@nx/devkit';
import { getExecutorInformation, getGeneratorInformation, type PackageJson, parseExecutor } from '@nx/devkit/internal';
import { EXECUTORS } from '@rasm/nx/inference';
import { Array, Option, Order, Predicate, Record, Schema, SchemaAST, Struct } from 'effect';
import { describe, expect, it } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = fileURLToPath(import.meta.resolve('@rasm/nx/package.json'));
const { name, generators = '' } = readJsonFile<PackageJson>(MANIFEST);
const PROJECTS = { [name]: { name, root: relative(workspaceRoot, dirname(MANIFEST)) } };
const GENERATORS = Record.keys(readJsonFile<{ readonly generators: Readonly<Record<string, object>> }>(join(dirname(MANIFEST), generators)).generators);

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('executor collection', () => {
    it.for(Record.toEntries(EXECUTORS))('resolves %s through Nx to an implementation that loads and a schema of the options its module declares', async ([key, executor]) => {
        const [collection, member] = parseExecutor(executor);
        const { implementationFactory, schema } = getExecutorInformation(collection, member, workspaceRoot, PROJECTS);
        const module: unknown = await import(`@rasm/nx/executors/${key}`);
        const options = Option.liftPredicate(module, Predicate.isObject).pipe(
            Option.flatMap((loaded) => Array.findFirst(Object.values(loaded), Schema.isSchema)),
            Option.flatMap(({ ast }) => Option.liftPredicate(ast, SchemaAST.isObjects)),
            Option.map(({ propertySignatures }) => propertySignatures.map(Struct.get('name')).map(String).toSorted(Order.String)),
        );

        expect({ loads: Predicate.isFunction(implementationFactory()), properties: Option.some(Record.keys(schema.properties).toSorted(Order.String)) }).toStrictEqual({ loads: true, properties: options });
    });

    it.for(GENERATORS)('resolves generator %s through Nx to a factory that loads', (generator) => {
        const { implementationFactory } = getGeneratorInformation(name, generator, workspaceRoot, PROJECTS);

        expect(Predicate.isFunction(implementationFactory())).toBe(true);
    });
});
