import { type CreateNodesResult, type CreateNodesV2, createNodesFromFiles, type ProjectConfiguration } from '@nx/devkit';

const _project = (file: string): CreateNodesResult => {
    const root = file.slice(0, file.lastIndexOf('/'));
    const configuration: ProjectConfiguration = {
        root,
        name: root.slice(root.lastIndexOf('/') + 1),
        tags: ['language:python'],
        targets: { typecheck: {}, test: {}, check: {} },
    };
    return { projects: { [root]: configuration } };
};

const createNodesV2: CreateNodesV2 = [
    '{libs/python,apps/*}/*/pyproject.toml',
    (files, options, context) => createNodesFromFiles(_project, files, options, context),
];

export { createNodesV2 };
