import type { ExecutorContext } from '@nx/devkit';
import { local } from '@pulumi/command';
import { ActionsEnvironmentSecret, ActionsRepositoryPermissions, ActionsSecret, IssueLabels, Repository, RepositoryDependabotSecurityUpdates, RepositoryDeployKey, RepositoryEnvironment, RepositoryRuleset, RepositoryVulnerabilityAlerts, WorkflowRepositoryPermissions } from '@pulumi/github';
import { interpolate, jsonStringify, type Output, secret } from '@pulumi/pulumi';
import { PrivateKey } from '@pulumi/tls';
import { Environment, Project, Provider, ServiceToken, secretssync } from '@pulumiverse/doppler';
import { Array, Cause, Config, Data, Effect, FileSystem, Path, Record, Schema } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientRequest, HttpClientResponse } from 'effect/http';
import { run } from '#/command';

// --- [MODELS] --------------------------------------------------------------------------

const Delivery = Data.taggedEnum<Data.TaggedEnum<{ Local: Record<never, never>; Repository: Record<never, never>; Environment: Record<never, never>; Token: Record<never, never> }>>();
const configs = { dev: Delivery.Local(), ci: Delivery.Repository(), release: Delivery.Environment(), deploy: Delivery.Token() };

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String, description: Schema.String, repository: Schema.Struct({ url: Schema.String }) }));
const Integrations = Schema.Struct({ integrations: Schema.Array(Schema.Struct({ slug: Schema.String, name: Schema.String, type: Schema.String })) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const repositoryStack = Effect.fnUntraced(function* (context: ExecutorContext) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { name: stackName, description, repository: origin } = yield* Effect.flatMap(fs.readFileString(path.join(context.root, 'package.json')), Schema.decodeEffect(Manifest));
    return {
        stackName,
        program: Effect.gen(function* () {
            const pattern = yield* Effect.fromNullishOr(context.nxJsonConfiguration.release?.releaseTag?.pattern).pipe(Effect.mapError(() => new Cause.NoSuchElementError('nx.json states no release.releaseTag.pattern for the release tag ruleset')));
            const [name, owner] = [path.basename(origin.url, '.git'), path.basename(path.dirname(origin.url))];
            const [project, login] = yield* Effect.all([Config.String('DOPPLER_PROJECT'), Effect.map(run('doppler', ['configure', 'get', 'token', '--plain']), (printed) => printed.trim())]);
            const { integrations } = yield* HttpClient.execute(HttpClientRequest.bearerToken(HttpClientRequest.get('https://api.doppler.com/v3/integrations'), login)).pipe(
                Effect.flatMap(HttpClientResponse.filterStatusOk),
                Effect.flatMap(HttpClientResponse.schemaBodyJson(Integrations)),
                Effect.provide(FetchHttpClient.layer),
            );
            const integration = yield* Effect.fromOption(
                Array.findFirst(integrations, ({ type, name: account }) => type === 'github-actions' && account === owner),
                () => new Cause.NoSuchElementError(`Doppler holds no github-actions integration named ${owner}`),
            );
            const { name: repository, fullName } = new Repository(
                name,
                {
                    name,
                    description,
                    visibility: 'public',
                    hasIssues: true,
                    allowMergeCommit: false,
                    allowUpdateBranch: true,
                    deleteBranchOnMerge: true,
                    squashMergeCommitTitle: 'PR_TITLE',
                    squashMergeCommitMessage: 'PR_BODY',
                    securityAndAnalysis: { secretScanning: { status: 'enabled' }, secretScanningPushProtection: { status: 'enabled' } },
                },
                { protect: true },
            );
            const provider = new Provider('doppler', { dopplerToken: secret(login) });
            const doppler = new Project(project, { name: project, description }, { provider, protect: true });
            const delivered = Record.map(configs, (delivery, config) => {
                const held = new Environment(`${project}-${config}`, { project: doppler.name, slug: config, name: config }, { provider, protect: true });
                const synced = (environmentName?: Output<string>) =>
                    new secretssync.GithubActions(`${project}-${config}`, { integration: integration.slug, project: doppler.name, config: held.slug, syncTarget: 'repo', repoName: repository, deleteBehavior: 'delete_from_target', ...(environmentName === undefined ? {} : { environmentName }) }, { provider }).config;
                return Delivery.$match(delivery, {
                    Local: () => held.slug,
                    Repository: () => synced(),
                    Environment: () => synced(new RepositoryEnvironment(`${name}-${config}`, { repository, environment: config }).environment),
                    Token: () =>
                        new ActionsEnvironmentSecret(`${name}-${config}-doppler`, {
                            repository,
                            environment: new RepositoryEnvironment(`${name}-${config}`, { repository, environment: config }).environment,
                            secretName: 'DOPPLER_TOKEN',
                            value: new ServiceToken(`${project}-${config}`, { project: doppler.name, config: held.slug, name, access: 'read' }, { provider }).key,
                        }).environment,
                });
            });
            const assets = new Repository(`${name}-Assets`, { name: `${name}-Assets`, description: `Binary assets ${name} holds as a private submodule`, visibility: 'private', allowMergeCommit: false, deleteBranchOnMerge: true }, { protect: true });
            const assetsKey = new PrivateKey(`${name}-assets-deploy`, { algorithm: 'ED25519' });
            return {
                repository: fullName,
                assets: assets.fullName,
                assetsDeployKey: new RepositoryDeployKey(`${name}-assets-deploy`, { repository: assets.name, title: `${name} CI`, key: assetsKey.publicKeyOpenssh, readOnly: true }).title,
                assetsSecret: new ActionsSecret(`${name}-assets-deploy-key`, { repository, secretName: 'ASSETS_DEPLOY_KEY', value: assetsKey.privateKeyOpenssh }).secretName,
                securityUpdates: new RepositoryDependabotSecurityUpdates(`${name}-security-updates`, { repository: new RepositoryVulnerabilityAlerts(`${name}-vulnerability-alerts`, { repository }).repository, enabled: false }).enabled,
                allowedActions: new ActionsRepositoryPermissions(`${name}-actions-permissions`, { repository, allowedActions: 'all' }).allowedActions,
                workflowPermissions: new WorkflowRepositoryPermissions(`${name}-workflow-permissions`, { repository, defaultWorkflowPermissions: 'read' }).defaultWorkflowPermissions,
                configs: delivered,
                codeScanning: new local.Command(`${name}-code-scanning`, {
                    create: interpolate`gh api --method PATCH repos/${fullName}/code-scanning/default-setup --input -`,
                    stdin: jsonStringify({ state: 'configured', languages: ['actions', 'csharp', 'javascript-typescript', 'python'] }),
                }).stdout,
                labels: new IssueLabels(`${name}-labels`, {
                    repository,
                    labels: [
                        { name: 'bug', color: 'd73a4a', description: 'Broken behavior' },
                        { name: 'enhancement', color: 'a2eeef', description: 'Feature request' },
                    ],
                }).labels,
                releaseTags: new RepositoryRuleset(`${name}-release-tags`, {
                    repository,
                    name: 'release tags',
                    target: 'tag',
                    enforcement: 'active',
                    conditions: { refName: { includes: [`refs/tags/${pattern.replaceAll(/\{\w+\}/gu, '**')}`], excludes: [] } },
                    rules: { update: true, deletion: true, nonFastForward: true },
                }).rulesetId,
            };
        }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { repositoryStack };
