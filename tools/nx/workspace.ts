// --- [IMPORTS] -------------------------------------------------------------------------

import { readFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import {
    type CreateNodes,
    type CreateNodesContext,
    type CreateNodesResult,
    type CreateNodesResultArray,
    createNodesFromFiles,
    type ProjectConfiguration,
    type TargetConfiguration,
} from '@nx/devkit';

// --- [TYPES] ---------------------------------------------------------------------------

type Language = 'dotnet' | 'python' | 'typescript';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NAME = /^\[project\][ \t]*(?:#.*)?$(?:\r?\n(?!\[).*)*?\r?\nname[ \t]*=[ \t]*(?:"(?<basic>[^"\\]*)"|'(?<literal>[^']*)')/mu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _language = (file: string): Language => {
    if (file.endsWith('.csproj')) {
        return 'dotnet';
    }
    return file.endsWith('tsconfig.json') ? 'typescript' : 'python';
};

const _pythonName = (file: string, context: CreateNodesContext): string => {
    const match = _NAME.exec(readFileSync(resolve(context.workspaceRoot, file), 'utf8'));
    const name = match?.groups?.['basic'] ?? match?.groups?.['literal'];
    if (name === undefined) {
        throw new Error(`${file} declares no string name under [project]`);
    }
    return name;
};

const _pythonTargets = (file: string): Record<string, TargetConfiguration> =>
    file.startsWith('tests/python/libs/') ? { typecheck: {}, test: {}, check: {} } : { typecheck: {}, check: {} };

const _configuration = (file: string, language: Language, context: CreateNodesContext): ProjectConfiguration =>
    language === 'python'
        ? { root: dirname(file), name: _pythonName(file, context), tags: ['language:python'], targets: _pythonTargets(file) }
        : { root: dirname(file), tags: [`language:${language}`], targets: { typecheck: {}, check: {} } };

const _project = (file: string, _options: unknown, context: CreateNodesContext): CreateNodesResult => {
    const configuration = _configuration(file, _language(file), context);
    return { projects: { [configuration.root]: configuration } };
};

// --- [REGISTRATION] --------------------------------------------------------------------

const createNodes: CreateNodes = [
    '{{apps,libs,tests,tools}/**/*.csproj,{apps,libs,tests}/**/tsconfig.json,.claude/plugins/*/tsconfig.json,{libs/python,apps/*,tests/python,tests/python/libs}/*/pyproject.toml}',
    (files, options, context): Promise<CreateNodesResultArray> => createNodesFromFiles(_project, files, options, context),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
