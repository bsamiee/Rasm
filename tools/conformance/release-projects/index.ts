import { createConformanceRule } from '@nx/conformance';
import { readNxJson } from '@nx/devkit';
import { findMatchingProjects } from '@nx/devkit/internal';
import { Array } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createConformanceRule({
    name: 'release-projects',
    category: 'consistency',
    description: 'Selects through nx.json release projects and groups exactly the projects holding release configuration',
    implementation: ({ tree, projectGraph: { nodes } }) => {
        const { projects = [], groups = {} } = readNxJson(tree)?.release ?? {};
        const selectors = [...Array.ensure(projects), ...Object.values(groups).flatMap((group) => Array.ensure(group.projects))];
        const selected = new Set(findMatchingProjects(Array.isReadonlyArrayNonEmpty(selectors) ? selectors : ['*'], nodes));
        const violations = Object.values(nodes)
            .filter(({ name, data }) => selected.has(name) !== (data.release !== undefined))
            .map(({ name, data }) => ({ sourceProject: name, message: data.release === undefined ? `${name} is a release project without release configuration, give it both or neither` : `${name} holds release configuration outside nx.json release projects, give it both or neither` }));
        return Promise.resolve({ severity: 'high' as const, details: { violations } });
    },
});
