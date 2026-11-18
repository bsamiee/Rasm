import { execFileSync, execSync } from 'node:child_process';
import { globSync } from 'node:fs';
import { join } from 'node:path';
import { type CreateDependenciesContext, type CreateNodesContext, DependencyType } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-docker';
import { TempFs } from '@rasm/testing/temp-fs';
import { updateContextWithChangedFiles } from 'nx/src/utils/workspace-context';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

expect.addSnapshotSerializer({
    serialize: (str: string) => str.replaceAll(/test[a-z0-9]{6}/gu, '{rootName}'),
    test: (val: unknown) => typeof val === 'string',
});

const [, createNodesFunction] = createNodes;
const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const cwd = process.cwd();
        const tempFs = new TempFs('test');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            tempFs.cleanup();
            process.chdir(cwd);
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({
        nxJsonConfiguration: {
            namedInputs: {
                default: ['{projectRoot}/**/*'],
                production: ['!{projectRoot}/**/*.spec.ts'],
            },
        },
        workspaceRoot: tempFs.tempDir,
    }));

describe('@rasm/nx-docker', () => {
    it('should create nodes based on compose.yaml', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'proj/compose.yaml': 'services:\n    app:\n        build: .\n',
            'proj/Dockerfile': 'FROM node:18',
            'proj/project.json': '{}',
        });

        const results = await createNodesFunction(['proj/compose.yaml'], {}, context);

        expect(results[0]?.[0]).toBe('proj/compose.yaml');
        expect(Object.keys({ ...results[0]?.[1].projects })).toStrictEqual(['proj']);
        expect(results[0]?.[1].projects?.['proj']).toMatchObject({ root: 'proj', metadata: { technologies: ['compose'] } });
        expect(Object.keys({ ...results[0]?.[1].projects?.['proj']?.targets })).toStrictEqual(['build', 'build--compose', 'serve', 'serve--compose', 'check', 'check--compose', 'check--buildx', 'install', 'install--compose']);
    });

    it('should create check targets based on compose.yaml', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'proj/compose.yaml': 'services:\n    app:\n        build: .\n',
            'proj/Dockerfile': 'FROM node:18',
            'proj/project.json': '{}',
        });

        const results = await createNodesFunction(['proj/compose.yaml'], {}, context);

        expect(results[0]?.[1].projects?.['proj']?.targets?.['check']).toMatchInlineSnapshot(`
          {
            dependsOn: [
              {
                params: forward,
                target: check--*,
              },
            ],
            inputs: [
              ...,
              {workspaceRoot}/proj/.env,
              {workspaceRoot}/proj/Dockerfile,
              {workspaceRoot}/proj/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
          }
        `);
        expect(results[0]?.[1].projects?.['proj']?.targets?.['check--compose']).toMatchInlineSnapshot(`
          {
            cache: true,
            command: docker compose config --quiet,
            inputs: [
              {workspaceRoot}/proj/.env,
              {workspaceRoot}/proj/Dockerfile,
              {workspaceRoot}/proj/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
            metadata: {
              description: Validates every Compose service of proj, interpolation and env files included, with docker compose config,
              help: {
                command: docker compose config --help,
                example: {
                  args: [
                    --dry-run,
                  ],
                },
              },
              technologies: [
                compose,
              ],
            },
            options: {
              cwd: proj,
              env: {
                COMPOSE_PROFILES: *,
              },
            },
          }
        `);
        expect(results[0]?.[1].projects?.['proj']?.targets?.['check--buildx']).toMatchInlineSnapshot(`
          {
            cache: true,
            command: docker compose build --check,
            dependsOn: [
              build--!(compose|buildx),
              ^build,
            ],
            inputs: [
              {workspaceRoot}/proj/.env,
              {workspaceRoot}/proj/Dockerfile,
              {workspaceRoot}/proj/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
            metadata: {
              description: Lints every image proj builds, failing on any warning, with docker compose build --check,
              help: {
                command: docker compose build --help,
                example: {
                  args: [
                    --dry-run,
                  ],
                },
              },
              technologies: [
                compose,
              ],
            },
            options: {
              cwd: proj,
              env: {
                COMPOSE_PROFILES: *,
              },
            },
          }
        `);
    });

    it('should create build and serve targets based on compose.yaml', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'proj/compose.yaml': 'services:\n    app:\n        build: .\n',
            'proj/Dockerfile': 'FROM node:18',
            'proj/project.json': '{}',
        });

        const results = await createNodesFunction(['proj/compose.yaml'], {}, context);

        expect(results[0]?.[1].projects?.['proj']?.targets?.['build']).toMatchInlineSnapshot(`
          {
            dependsOn: [
              {
                params: forward,
                target: build--*,
              },
            ],
            inputs: [
              ...,
              default,
              ^default,
              {workspaceRoot}/proj/.env,
              {workspaceRoot}/proj/Dockerfile,
              {workspaceRoot}/proj/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
          }
        `);
        expect(results[0]?.[1].projects?.['proj']?.targets?.['build--compose']).toMatchInlineSnapshot(`
          {
            command: docker compose build,
            dependsOn: [
              build--!(compose|buildx),
              ^build,
            ],
            inputs: [
              default,
              ^default,
              {workspaceRoot}/proj/.env,
              {workspaceRoot}/proj/Dockerfile,
              {workspaceRoot}/proj/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
            metadata: {
              description: Builds the images of proj into the engine with docker compose build,
              help: {
                command: docker compose build --help,
                example: {
                  args: [
                    --dry-run,
                  ],
                },
              },
              technologies: [
                compose,
              ],
            },
            options: {
              cwd: proj,
            },
          }
        `);
        expect(results[0]?.[1].projects?.['proj']?.targets?.['serve']).toMatchInlineSnapshot(`
          {
            dependsOn: [
              {
                params: forward,
                target: serve--*,
              },
            ],
            inputs: [
              ...,
              default,
              ^default,
              {workspaceRoot}/proj/.env,
              {workspaceRoot}/proj/Dockerfile,
              {workspaceRoot}/proj/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
          }
        `);
        expect(results[0]?.[1].projects?.['proj']?.targets?.['serve--compose']).toMatchInlineSnapshot(`
          {
            command: docker compose up --detach --wait --no-build app && docker compose logs --follow app,
            continuous: true,
            dependsOn: [
              build--*,
            ],
            inputs: [
              default,
              ^default,
              {workspaceRoot}/proj/.env,
              {workspaceRoot}/proj/Dockerfile,
              {workspaceRoot}/proj/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
            metadata: {
              description: Starts app of proj with the services each depends on from built images with docker compose up and follows their logs with docker compose logs, the services staying up after the run,
              help: {
                command: docker compose up --help,
                example: {
                  args: [
                    --dry-run,
                  ],
                },
              },
              technologies: [
                compose,
              ],
            },
            options: {
              cwd: proj,
            },
            readyWhen: {
              command: node -e "require('@rasm/nx-docker/ready-check').waitForDevServer('app')",
              interval: 1000,
            },
          }
        `);
    });

    it('should create install targets based on compose.yaml', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'proj/compose.yaml': 'services:\n    app:\n        build: .\n',
            'proj/Dockerfile': 'FROM node:18',
            'proj/project.json': '{}',
        });

        const results = await createNodesFunction(['proj/compose.yaml'], {}, context);

        expect(results[0]?.[1].projects?.['proj']?.targets?.['install']).toMatchInlineSnapshot(`
          {
            dependsOn: [
              {
                params: forward,
                target: install--*,
              },
            ],
            inputs: [
              ...,
              default,
              ^default,
              {workspaceRoot}/proj/.env,
              {workspaceRoot}/proj/Dockerfile,
              {workspaceRoot}/proj/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
          }
        `);
        expect(results[0]?.[1].projects?.['proj']?.targets?.['install--compose']).toMatchInlineSnapshot(`
          {
            command: docker compose up --detach --wait --no-build --remove-orphans,
            dependsOn: [
              build--*,
            ],
            inputs: [
              default,
              ^default,
              {workspaceRoot}/proj/.env,
              {workspaceRoot}/proj/Dockerfile,
              {workspaceRoot}/proj/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
            metadata: {
              description: Starts the services of proj from built images and waits until each is healthy with docker compose up,
              help: {
                command: docker compose up --help,
                example: {
                  args: [
                    --dry-run,
                  ],
                },
              },
              technologies: [
                compose,
              ],
            },
            options: {
              cwd: proj,
            },
          }
        `);
    });

    it('should use default namedInputs whether production is available or not, since docker build reads every file of its context', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'proj/compose.yaml': 'services:\n    app:\n        build: .\n',
            'proj/Dockerfile': 'FROM node:18',
            'proj/project.json': '{}',
        });

        const results = await createNodesFunction(['proj/compose.yaml'], {}, { ...context, nxJsonConfiguration: { namedInputs: { default: ['{projectRoot}/**/*'] } } });

        expect(results).toStrictEqual(await createNodesFunction(['proj/compose.yaml'], {}, context));
        expect(results[0]?.[1].projects?.['proj']?.targets?.['build--compose']?.inputs).toStrictEqual(['default', '^default', '{workspaceRoot}/proj/.env', '{workspaceRoot}/proj/Dockerfile', '{workspaceRoot}/proj/compose.yaml', { externalDependencies: [] }]);
    });

    it('should handle nested project structures', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'apps/api/compose.yaml': 'services:\n    app:\n        build: .\n',
            'apps/api/Dockerfile': 'FROM node:18',
            'apps/api/project.json': '{}',
        });

        const results = await createNodesFunction(['apps/api/compose.yaml'], {}, context);

        expect(results[0]?.[0]).toBe('apps/api/compose.yaml');
        expect(results[0]?.[1].projects?.['apps/api']?.targets?.['check--compose']).toMatchInlineSnapshot(`
          {
            cache: true,
            command: docker compose config --quiet,
            inputs: [
              {workspaceRoot}/apps/api/.env,
              {workspaceRoot}/apps/api/Dockerfile,
              {workspaceRoot}/apps/api/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
            metadata: {
              description: Validates every Compose service of api, interpolation and env files included, with docker compose config,
              help: {
                command: docker compose config --help,
                example: {
                  args: [
                    --dry-run,
                  ],
                },
              },
              technologies: [
                compose,
              ],
            },
            options: {
              cwd: apps/api,
              env: {
                COMPOSE_PROFILES: *,
              },
            },
          }
        `);
        expect(results[0]?.[1].projects?.['apps/api']?.targets?.['build--compose']).toMatchInlineSnapshot(`
          {
            command: docker compose build,
            dependsOn: [
              build--!(compose|buildx),
              ^build,
            ],
            inputs: [
              default,
              ^default,
              {workspaceRoot}/apps/api/.env,
              {workspaceRoot}/apps/api/Dockerfile,
              {workspaceRoot}/apps/api/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
            metadata: {
              description: Builds the images of api into the engine with docker compose build,
              help: {
                command: docker compose build --help,
                example: {
                  args: [
                    --dry-run,
                  ],
                },
              },
              technologies: [
                compose,
              ],
            },
            options: {
              cwd: apps/api,
            },
          }
        `);
        expect(results[0]?.[1].projects?.['apps/api']?.targets?.['serve--compose']).toMatchInlineSnapshot(`
          {
            command: docker compose up --detach --wait --no-build app && docker compose logs --follow app,
            continuous: true,
            dependsOn: [
              build--*,
            ],
            inputs: [
              default,
              ^default,
              {workspaceRoot}/apps/api/.env,
              {workspaceRoot}/apps/api/Dockerfile,
              {workspaceRoot}/apps/api/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
            metadata: {
              description: Starts app of api with the services each depends on from built images with docker compose up and follows their logs with docker compose logs, the services staying up after the run,
              help: {
                command: docker compose up --help,
                example: {
                  args: [
                    --dry-run,
                  ],
                },
              },
              technologies: [
                compose,
              ],
            },
            options: {
              cwd: apps/api,
            },
            readyWhen: {
              command: node -e "require('@rasm/nx-docker/ready-check').waitForDevServer('app')",
              interval: 1000,
            },
          }
        `);
        expect(results[0]?.[1].projects?.['apps/api']?.targets?.['install--compose']).toMatchInlineSnapshot(`
          {
            command: docker compose up --detach --wait --no-build --remove-orphans,
            dependsOn: [
              build--*,
            ],
            inputs: [
              default,
              ^default,
              {workspaceRoot}/apps/api/.env,
              {workspaceRoot}/apps/api/Dockerfile,
              {workspaceRoot}/apps/api/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
            metadata: {
              description: Starts the services of api from built images and waits until each is healthy with docker compose up,
              help: {
                command: docker compose up --help,
                example: {
                  args: [
                    --dry-run,
                  ],
                },
              },
              technologies: [
                compose,
              ],
            },
            options: {
              cwd: apps/api,
            },
          }
        `);
    });

    it('should handle project names with special characters', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'apps/my-app/compose.yaml': 'services:\n    app:\n        build: .\n',
            'apps/my-app/Dockerfile': 'FROM node:18',
            'apps/my-app/project.json': '{}',
        });

        const results = await createNodesFunction(['apps/my-app/compose.yaml'], {}, context);

        expect(results[0]?.[1].projects?.['apps/my-app']?.targets?.['build--compose']).toMatchObject({ command: 'docker compose build', options: { cwd: 'apps/my-app' }, metadata: { description: 'Builds the images of my-app into the engine with docker compose build' } });
    });

    it('should handle multiple Compose files', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'app1/compose.yaml': 'services:\n    app:\n        image: node:18\n',
            'app1/compose.override.yaml': 'services:\n    app:\n        ports:\n            - "3000:3000"\n',
            'app1/project.json': '{}',
            'app2/compose.yaml': 'services:\n    app:\n        image: node:18\n',
            'app2/project.json': '{}',
        });

        const results = await createNodesFunction(['app1/compose.override.yaml', 'app1/compose.yaml', 'app2/compose.yaml'], {}, context);

        expect(results).toHaveLength(2);
        expect(results[0]?.[0]).toBe('app1/compose.override.yaml');
        expect(results[1]?.[0]).toBe('app2/compose.yaml');
        expect(Object.keys({ ...results[0]?.[1].projects })).toStrictEqual(['app1']);
        expect(Object.keys({ ...results[1]?.[1].projects })).toStrictEqual(['app2']);
    });

    it('should use cached targets when hash matches', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'proj/compose.yaml': 'services:\n    app:\n        build: .\n',
            'proj/Dockerfile': 'FROM node:18',
            'proj/project.json': '{}',
        });

        const results1 = await createNodesFunction(['proj/compose.yaml'], {}, context);

        const results2 = await createNodesFunction(['proj/compose.yaml'], {}, context);

        expect(results1).toStrictEqual(results2);
    });

    it('should re-read when a file the Compose model reads changes', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'shared/compose.base.yaml': 'services:\n    store:\n        image: valkey/valkey:8\n',
            'infra/compose.yaml': 'include:\n    - ../shared/compose.base.yaml\nservices:\n    api:\n        image: node:18\n',
            'infra/project.json': '{}',
        });

        const results1 = await createNodesFunction(['infra/compose.yaml'], {}, context);
        tempFs.writeFile('shared/compose.base.yaml', 'services:\n    store:\n        image: valkey/valkey:8\n        profiles:\n            - cached\n');
        await updateContextWithChangedFiles(tempFs.tempDir, [], ['shared/compose.base.yaml'], []);
        const results2 = await createNodesFunction(['infra/compose.yaml'], {}, context);

        expect(results1[0]?.[1].projects?.['infra']?.targets?.['install--compose']).not.toHaveProperty('configurations');
        expect(results2[0]?.[1].projects?.['infra']?.targets?.['install--compose']?.configurations).toStrictEqual({ cached: { env: { COMPOSE_PROFILES: 'cached' } } });
    });

    it('should handle root level compose.yaml', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'compose.yaml': 'services:\n    app:\n        build: .\n',
            Dockerfile: 'FROM node:18',
            'project.json': '{}',
        });

        const results = await createNodesFunction(['compose.yaml'], {}, context);

        expect(results[0]?.[0]).toBe('compose.yaml');
        expect(results[0]?.[1].projects?.['.']?.targets?.['check--compose']).toMatchInlineSnapshot(`
          {
            cache: true,
            command: docker compose config --quiet,
            inputs: [
              {workspaceRoot}/.env,
              {workspaceRoot}/Dockerfile,
              {workspaceRoot}/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
            metadata: {
              description: Validates every Compose service of {rootName}, interpolation and env files included, with docker compose config,
              help: {
                command: docker compose config --help,
                example: {
                  args: [
                    --dry-run,
                  ],
                },
              },
              technologies: [
                compose,
              ],
            },
            options: {
              cwd: .,
              env: {
                COMPOSE_PROFILES: *,
              },
            },
          }
        `);
        expect(results[0]?.[1].projects?.['.']?.targets?.['build--compose']).toMatchInlineSnapshot(`
          {
            command: docker compose build,
            dependsOn: [
              build--!(compose|buildx),
              ^build,
            ],
            inputs: [
              default,
              ^default,
              {workspaceRoot}/.env,
              {workspaceRoot}/Dockerfile,
              {workspaceRoot}/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
            metadata: {
              description: Builds the images of {rootName} into the engine with docker compose build,
              help: {
                command: docker compose build --help,
                example: {
                  args: [
                    --dry-run,
                  ],
                },
              },
              technologies: [
                compose,
              ],
            },
            options: {
              cwd: .,
            },
          }
        `);
        expect(results[0]?.[1].projects?.['.']?.targets?.['serve--compose']).toMatchInlineSnapshot(`
          {
            command: docker compose up --detach --wait --no-build app && docker compose logs --follow app,
            continuous: true,
            dependsOn: [
              build--*,
            ],
            inputs: [
              default,
              ^default,
              {workspaceRoot}/.env,
              {workspaceRoot}/Dockerfile,
              {workspaceRoot}/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
            metadata: {
              description: Starts app of {rootName} with the services each depends on from built images with docker compose up and follows their logs with docker compose logs, the services staying up after the run,
              help: {
                command: docker compose up --help,
                example: {
                  args: [
                    --dry-run,
                  ],
                },
              },
              technologies: [
                compose,
              ],
            },
            options: {
              cwd: .,
            },
            readyWhen: {
              command: node -e "require('@rasm/nx-docker/ready-check').waitForDevServer('app')",
              interval: 1000,
            },
          }
        `);
        expect(results[0]?.[1].projects?.['.']?.targets?.['install--compose']).toMatchInlineSnapshot(`
          {
            command: docker compose up --detach --wait --no-build --remove-orphans,
            dependsOn: [
              build--*,
            ],
            inputs: [
              default,
              ^default,
              {workspaceRoot}/.env,
              {workspaceRoot}/Dockerfile,
              {workspaceRoot}/compose.yaml,
              {
                externalDependencies: [],
              },
            ],
            metadata: {
              description: Starts the services of {rootName} from built images and waits until each is healthy with docker compose up,
              help: {
                command: docker compose up --help,
                example: {
                  args: [
                    --dry-run,
                  ],
                },
              },
              technologies: [
                compose,
              ],
            },
            options: {
              cwd: .,
            },
          }
        `);
    });

    it('should write nothing outside the declared outputs when build--compose runs', { timeout: 120_000 }, async ({ tempFs, context, onTestFinished }) => {
        await tempFs.createFiles({
            'proj/compose.yaml': 'services:\n    app:\n        build: .\n',
            'proj/Dockerfile': 'FROM scratch\nCOPY compose.yaml /compose.yaml\n',
            'proj/project.json': '{}',
        });
        const results = await createNodesFunction(['proj/compose.yaml'], {}, context);
        const target = results[0]?.[1].projects?.['proj']?.targets?.['build--compose'];
        const before = globSync('**', { cwd: tempFs.tempDir });
        onTestFinished(() => {
            execFileSync('docker', ['image', 'rm', '--force', 'proj-app'], { stdio: 'ignore' });
        });

        execSync(String(target?.command), { cwd: join(tempFs.tempDir, 'proj'), stdio: 'ignore' });

        expect(target).toMatchObject({ command: 'docker compose build', options: { cwd: 'proj' } });
        expect(target).not.toHaveProperty('outputs');
        expect(globSync('**', { cwd: tempFs.tempDir })).toStrictEqual(before);
    });

    describe('project roots', () => {
        it('should not create nodes for a Compose file outside a project root', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'package.json': '{ "name": "workspace", "workspaces": ["apps/*"] }',
                'deploy/compose.yaml': 'services:\n    app:\n        image: node:18\n',
                'tools/compose.yaml': 'services:\n    app:\n        image: node:18\n',
                'tools/package.json': '{ "name": "tools" }',
            });

            const results = await createNodesFunction(['deploy/compose.yaml', 'tools/compose.yaml'], {}, context);

            expect(results).toStrictEqual([]);
        });

        it('should create nodes for a package.json in the package-manager workspaces', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'package.json': '{ "name": "workspace", "workspaces": ["apps/*"] }',
                'apps/api/compose.yaml': 'services:\n    app:\n        build: .\n',
                'apps/api/Dockerfile': 'FROM node:18',
                'apps/api/package.json': '{ "name": "api" }',
            });

            const results = await createNodesFunction(['apps/api/compose.yaml'], {}, context);

            expect(Object.keys({ ...results[0]?.[1].projects })).toStrictEqual(['apps/api']);
        });

        it('should give no start to a Dockerfile folder outside a project root', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'infra/compose.yaml': 'services:\n    api:\n        build:\n            context: ../images/api\n',
                'infra/project.json': '{}',
                'images/api/Dockerfile': 'FROM node:18',
            });

            const results = await createNodesFunction(['infra/compose.yaml'], {}, context);

            expect(Object.keys({ ...results[0]?.[1].projects })).toStrictEqual(['infra']);
            expect(results[0]?.[1].projects?.['infra']?.targets).toHaveProperty('build--compose');
        });
    });

    describe('project name generation', () => {
        it('should handle leading slashes in project root', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'proj/compose.yaml': 'services:\n    app:\n        build: .\n',
                'proj/Dockerfile': 'FROM node:18',
                'proj/project.json': '{}',
            });

            const results = await createNodesFunction(['proj/compose.yaml'], {}, context);

            expect(results[0]?.[1].projects?.['proj']?.targets?.['build--compose']?.metadata?.description).toBe('Builds the images of proj into the engine with docker compose build');
        });

        it('should handle spaces in project path', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'my proj/compose.yaml': 'services:\n    app:\n        build: .\n',
                'my proj/Dockerfile': 'FROM node:18',
                'my proj/project.json': '{}',
            });

            const results = await createNodesFunction(['my proj/compose.yaml'], {}, context);

            expect(results[0]?.[1].projects?.['my proj']?.targets?.['build--compose']).toMatchObject({ command: 'docker compose build', options: { cwd: 'my proj' }, metadata: { description: 'Builds the images of myproj into the engine with docker compose build' } });
        });

        it('should replace shell metacharacters in project path', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'a;touch pwned;$(id)/compose.yaml': 'services:\n    app:\n        build: .\n',
                'a;touch pwned;$(id)/Dockerfile': 'FROM node:18',
                'a;touch pwned;$(id)/project.json': '{}',
            });

            const results = await createNodesFunction(['a;touch pwned;$(id)/compose.yaml'], {}, context);

            expect(results[0]?.[1].projects?.['a;touch pwned;$(id)']?.targets).toMatchObject({
                'build--compose': { command: 'docker compose build', options: { cwd: 'a;touch pwned;$(id)' }, metadata: { description: 'Builds the images of atouchpwnedid into the engine with docker compose build' } },
                'serve--compose': { command: 'docker compose up --detach --wait --no-build app && docker compose logs --follow app', options: { cwd: 'a;touch pwned;$(id)' } },
            });
        });
    });

    describe('dependency handling', () => {
        it('should set run target to depend on build target', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'proj/compose.yaml': 'services:\n    app:\n        build: .\n',
                'proj/Dockerfile': 'FROM node:18',
                'proj/project.json': '{}',
            });

            const results = await createNodesFunction(['proj/compose.yaml'], {}, context);

            expect(results[0]?.[1].projects?.['proj']?.targets?.['serve--compose']?.dependsOn).toStrictEqual(['build--*']);
            expect(results[0]?.[1].projects?.['proj']?.targets?.['install--compose']?.dependsOn).toStrictEqual(['build--*']);
        });

        it('should build a Dockerfile a Compose service of the project builds once, through build--compose', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'proj/compose.yaml': 'services:\n    app:\n        build: .\n    worker:\n        build:\n            context: ../apps/worker\n',
                'proj/Dockerfile': 'FROM node:18',
                'proj/project.json': '{}',
                'apps/worker/Dockerfile': 'FROM node:18',
                'apps/worker/project.json': '{}',
            });

            const results = await createNodesFunction(['proj/compose.yaml'], {}, context);

            expect(results[0]?.[1].projects?.['proj']?.targets).not.toHaveProperty('build--buildx');
            expect(results[0]?.[1].projects?.['proj']?.targets).toHaveProperty('build--compose');
            expect(Object.keys({ ...results[0]?.[1].projects?.['apps/worker']?.targets })).toStrictEqual(['build', 'build--buildx', 'serve', 'serve--compose']);
            expect(results[0]?.[1].projects?.['apps/worker']?.targets?.['build--buildx']?.command).toBe('docker compose build --with-dependencies worker');
        });

        it('should give the project holding the Dockerfile of a service its image build and start', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'infra/compose.yaml': 'services:\n    api:\n        build:\n            context: ../apps/api\n',
                'infra/project.json': '{}',
                'apps/api/Dockerfile': 'FROM node:18',
                'apps/api/project.json': '{}',
            });

            const results = await createNodesFunction(['infra/compose.yaml'], {}, context);

            expect(Object.keys({ ...results[0]?.[1].projects })).toStrictEqual(['apps/api', 'infra']);
            expect(Object.keys({ ...results[0]?.[1].projects?.['apps/api']?.targets })).toStrictEqual(['build', 'build--buildx', 'serve', 'serve--compose']);
            expect(results[0]?.[1].projects?.['apps/api']?.targets).toMatchObject({ 'build--buildx': { options: { cwd: 'infra' } }, 'serve--compose': { options: { cwd: 'infra' } } });
        });

        it('should name every file the Compose model reads as an input', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'shared/compose.base.yaml': 'services:\n    store:\n        image: valkey/valkey:8\n',
                'shared/proxy.base.yaml': 'services:\n    proxy:\n        image: traefik:v3\n',
                'infra/compose.yaml': 'include:\n    - ../shared/compose.base.yaml\nservices:\n    api:\n        build:\n            context: ../apps/api\n        env_file: api.env\n    proxy:\n        extends:\n            file: ../shared/proxy.base.yaml\n            service: proxy\n',
                'infra/api.env': 'PORT=8080\n',
                'infra/project.json': '{}',
                'apps/api/Dockerfile': 'FROM node:18',
            });

            const results = await createNodesFunction(['infra/compose.yaml'], {}, context);

            expect(results[0]?.[1].projects?.['infra']?.targets?.['check--compose']?.inputs).toStrictEqual([
                '{workspaceRoot}/apps/api/Dockerfile',
                '{workspaceRoot}/infra/.env',
                '{workspaceRoot}/infra/api.env',
                '{workspaceRoot}/infra/compose.yaml',
                '{workspaceRoot}/shared/compose.base.yaml',
                '{workspaceRoot}/shared/proxy.base.yaml',
                { externalDependencies: [] },
            ]);
        });

        it('should create a dependency on the project whose output holds a build context', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'ops/compose.yaml': 'services:\n    site:\n        build:\n            context: ../libs/site/dist\n            dockerfile: ../../../ops/site.Dockerfile\n',
                'ops/site.Dockerfile': 'FROM scratch\nCOPY . /site\n',
                'ops/project.json': '{}',
            });

            const results = await createNodesFunction(['ops/compose.yaml'], {}, context);
            const dependencies = await createDependencies({}, {
                externalNodes: {},
                fileMap: { projectFileMap: {}, nonProjectFiles: [] },
                filesToProcess: { projectFileMap: {}, nonProjectFiles: [] },
                nxJsonConfiguration: context.nxJsonConfiguration,
                projects: {
                    ops: { root: 'ops', ...results[0]?.[1].projects?.['ops'] },
                    site: { root: 'libs/site', targets: { build: { command: 'true', outputs: ['{projectRoot}/dist'] } } },
                },
                workspaceRoot: tempFs.tempDir,
            } satisfies CreateDependenciesContext);

            expect(dependencies).toStrictEqual([{ source: 'ops', target: 'site', type: DependencyType.static, sourceFile: 'ops/compose.yaml' }]);
            expect(results[0]?.[1].projects?.['ops']?.targets?.['build--compose']?.dependsOn).toStrictEqual(['build--!(compose|buildx)', '^build']);
        });
    });

    describe('target configurations', () => {
        it('should create configurations from Compose profiles', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'proj/compose.yaml': 'services:\n    app:\n        build: .\n    traces:\n        image: jaegertracing/jaeger:2\n        profiles:\n            - observed\n    debugger:\n        image: node:18\n        profiles:\n            - debug\n',
                'proj/Dockerfile': 'FROM node:18',
                'proj/project.json': '{}',
            });

            const results = await createNodesFunction(['proj/compose.yaml'], {}, context);

            const configurations = { debug: { env: { COMPOSE_PROFILES: 'debug' } }, observed: { env: { COMPOSE_PROFILES: 'observed' } } };
            expect(results[0]?.[1].projects?.['proj']?.targets).toMatchObject({ 'build--compose': { configurations }, 'install--compose': { configurations } });
        });

        it('should not include configurations if not defined', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'proj/compose.yaml': 'services:\n    app:\n        build: .\n',
                'proj/Dockerfile': 'FROM node:18',
                'proj/project.json': '{}',
            });

            const results = await createNodesFunction(['proj/compose.yaml'], {}, context);

            expect(results[0]?.[1].projects?.['proj']?.targets?.['build--compose']).not.toHaveProperty('configurations');
            expect(results[0]?.[1].projects?.['proj']?.targets?.['install--compose']).not.toHaveProperty('configurations');
        });
    });
});
