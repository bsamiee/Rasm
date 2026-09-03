// Inline Pulumi program that registers the declared Doppler and GitHub resources and imports the live ones under --import

// --- [IMPORTS] -------------------------------------------------------------------------

import * as github from '@pulumi/github';
import type { CustomResourceOptions } from '@pulumi/pulumi';
import { secret } from '@pulumi/pulumi';
import type { PulumiFn } from '@pulumi/pulumi/automation/index.js';
import * as doppler from '@pulumiverse/doppler';
import { Redacted } from 'effect';
import { Resources } from './resources.ts';

// --- [TYPES] ---------------------------------------------------------------------------

declare namespace program {
    type Flags = { readonly import: boolean };
    type Credentials = { readonly dopplerToken: Redacted.Redacted<string>; readonly githubToken: Redacted.Redacted<string> };
}

// --- [PROGRAM] -------------------------------------------------------------------------

const program =
    (flags: program.Flags, credentials: program.Credentials): PulumiFn =>
    // BOUNDARY: the Pulumi engine registers each resource through its constructor inside the program context
    async () => {
        // The import option binds under the --import flag on imported rows alone, and a later up against imported state plans no change
        const importOption = (imported: boolean, importId: string): CustomResourceOptions => (flags.import && imported ? { import: importId } : {});
        const dopplerProvider = new doppler.Provider('doppler', { dopplerToken: secret(Redacted.value(credentials.dopplerToken)) });
        const githubProvider = new github.Provider('github', { owner: Resources.owner, token: secret(Redacted.value(credentials.githubToken)) });

        const project = new doppler.Project(
            Resources.project.name,
            { name: Resources.project.name, description: Resources.project.description },
            { provider: dopplerProvider, ...importOption(Resources.project.imported, Resources.project.name) },
        );

        // Each environment registers the branch configs declared under it, and every config passes its name on as an output that holds the dependency
        const configs = Resources.environments.flatMap((row) => {
            const environment = new doppler.Environment(
                row.slug,
                { project: project.name, slug: row.slug, name: row.name },
                { provider: dopplerProvider, ...importOption(row.imported, `${Resources.project.name}.${row.slug}`) },
            );
            return [
                { name: row.slug, output: environment.slug },
                ...Resources.branchConfigs
                    .filter((branch) => branch.environment === row.slug)
                    .map((branch) => ({
                        name: branch.name,
                        output: new doppler.BranchConfig(
                            branch.name,
                            { project: project.name, environment: environment.slug, name: branch.name },
                            {
                                provider: dopplerProvider,
                                ...importOption(branch.imported, `${Resources.project.name}.${branch.environment}.${branch.name}`),
                            },
                        ).name,
                    })),
            ];
        });

        const serviceTokens = configs.flatMap((config) =>
            Resources.serviceTokens
                .filter((row) => row.config === config.name)
                .map((row) => ({
                    row,
                    resource: new doppler.ServiceToken(
                        `${row.config}-${row.name}`,
                        { project: project.name, config: config.output, name: row.name, access: row.access },
                        { provider: dopplerProvider },
                    ),
                })),
        );

        // protect fails a row edit that would delete the repository, and archiveOnDestroy archives it on a destroy
        const repository = new github.Repository(
            Resources.repository.name,
            { name: Resources.repository.name, ...Resources.repository.settings },
            { provider: githubProvider, protect: true, ...importOption(Resources.repository.imported, Resources.repository.name) },
        );

        const actionsSecrets = serviceTokens.flatMap(({ row, resource }) =>
            Resources.actionsSecrets
                .filter((entry) => entry.serviceToken === row.name)
                .map(
                    (entry) =>
                        new github.ActionsSecret(
                            `${entry.repository}-${entry.secretName}`,
                            { repository: repository.name, secretName: entry.secretName, value: resource.key },
                            { provider: githubProvider },
                        ),
                ),
        );

        return {
            repository: repository.fullName,
            configs: configs.map((config) => config.name),
            serviceTokens: serviceTokens.map(({ row }) => row.name),
            actionsSecrets: actionsSecrets.map((actionsSecret) => actionsSecret.secretName),
        };
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { program };
