import { matchesGlob } from 'node:path';
import { createConformanceRule } from '@nx/conformance';
import { Array, Record } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createConformanceRule<{ readonly packages: readonly string[] }>({
    name: 'one-version',
    category: 'reliability',
    description: 'Installs every npm package a pattern of option `packages` matches at one version',
    implementation: ({ projectGraph, ruleOptions: { packages } }) => {
        const installed = Object.values(projectGraph.externalNodes ?? {}).filter(({ type, data }) => type === 'npm' && packages.some((pattern) => matchesGlob(data.packageName, pattern)));
        const listed = Record.collect(
            Array.groupBy(installed, ({ data }) => data.version),
            (version, held) => `${version} (${Array.dedupe(held.map(({ data }) => data.packageName)).join(', ')})`,
        );
        const violations = listed.length > 1 ? [{ workspaceViolation: true, message: `Installs ${listed.join('; ')}, move each to one version in the catalog and lock` }] : [];
        return Promise.resolve({ severity: 'high' as const, details: { violations } });
    },
});
