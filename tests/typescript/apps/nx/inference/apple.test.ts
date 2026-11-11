import { assert } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-apple';
import manifest from '@rasm/nx-apple/package.json' with { type: 'json' };
import { Effect } from 'effect';
import { cases, edges, inferred, roots, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const subject: Subject = {
    plugin: { name: manifest.name, createNodes, createDependencies },
    options: { trustPackages: true },
    tools: ['swift'],
    state: ['.build'],
    files: {
        'package.json': '{ "name": "root" }\n',
        'Package.swift': '// swift-tools-version: 6.2\nimport PackageDescription\n\nlet package = Package(\n    name: "Atlas",\n    products: [.library(name: "Sheets", targets: ["Sheets"])],\n    targets: [\n        .target(name: "Sheets", dependencies: ["Paper"]),\n        .target(name: "Paper"),\n    ]\n)\n',
        'Sources/Sheets/Sheet.swift': 'import Paper\n\npublic struct Sheet {\n    public let paper: Paper\n}\n',
        'Sources/Paper/Paper.swift': 'public struct Paper {\n    public let name: String\n}\n',
        'kit/Package.swift': '// swift-tools-version: 6.2\nimport PackageDescription\n\nlet package = Package(name: "Kit", targets: [.target(name: "Ink")])\n',
        'kit/Sources/Ink/Ink.swift': 'public struct Ink {\n    public let tone: String\n}\n',
    },
    broken: ['Package.swift', '// swift-tools-version: 6.2\nimport PackageDescription\n\nlet package = Package(\n    name: "Atlas",\n'],
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const linked = (graph: ProjectGraph) => edges(graph).map(({ source, target }) => `${source} ${target}`);

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('names each module of a package by its target name under the folder SwiftPM gives it, reads every package manifest, and links a target dependency the manifest states', () =>
        Effect.map(inferred(subject), (graph) => assert.deepOwnInclude({ ...roots(graph), packages: Object.values(roots(graph)).includes('kit'), linked: linked(graph) }, { Sheets: 'Sources/Sheets', Paper: 'Sources/Paper', packages: true, linked: ['Sheets Paper'] })),
    );
});
