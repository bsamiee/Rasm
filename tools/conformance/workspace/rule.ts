import { createConformanceRule } from '@nx/conformance';
import { readNxJson } from '@nx/devkit';
import { findMatchingProjects, readNxMigrateConfig } from '@nx/devkit/internal';
import { Ajv } from 'ajv';
import { Array, Record } from 'effect';
import nx from 'nx/package.json' with { type: 'json' };
import settings from 'nx/schemas/nx-schema.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const ALIGNED = new Set([nx.name, ...(readNxMigrateConfig({ 'nx-migrations': nx['nx-migrations'] }).packageGroup ?? []).filter(({ version }) => version === '*').map(({ package: name }) => name)]);

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createConformanceRule({
    name: 'workspace',
    category: 'reliability',
    description: "Holds nx.json under Nx's settings schema, installs nx and every package of its migration group at one version, and gives release configuration to exactly the projects nx.json release projects and groups select",
    implementation: ({ tree, projectGraph: { nodes, externalNodes = {} } }) => {
        const validator = new Ajv({ strict: false, allErrors: true });
        const valid = validator.compile(settings);
        const nxJson = readNxJson(tree) ?? {};
        const { projects = [], groups = {} } = nxJson.release ?? {};
        const selectors = [...Array.ensure(projects), ...Object.values(groups).flatMap((group) => Array.ensure(group.projects))];
        const selected = new Set(findMatchingProjects(Array.isReadonlyArrayNonEmpty(selectors) ? selectors : ['*'], nodes));
        const installed = Record.collect(
            Array.groupBy(
                Object.values(externalNodes).filter(({ type, data }) => type === 'npm' && ALIGNED.has(data.packageName)),
                ({ data }) => data.version,
            ),
            (version, held) => `${version} (${Array.dedupe(held.map(({ data }) => data.packageName)).join(', ')})`,
        );
        const violations = [
            ...(valid(nxJson) ? [] : [{ file: 'nx.json', message: `nx.json ${validator.errorsText(valid.errors)}` }]),
            ...(installed.length > 1 ? [{ workspaceViolation: true, message: `Installs nx and its migration group at ${installed.join('; ')}, move each to one version in the catalog and lock` }] : []),
            ...Object.values(nodes)
                .filter(({ name, data }) => selected.has(name) !== (data.release !== undefined))
                .map(({ name, data }) => ({ sourceProject: name, message: data.release === undefined ? `${name} is a release project without release configuration, give it both or neither` : `${name} holds release configuration outside nx.json release projects, give it both or neither` })),
        ];
        return Promise.resolve({ severity: 'high' as const, details: { violations } });
    },
});
