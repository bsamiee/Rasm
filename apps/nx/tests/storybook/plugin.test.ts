import { execSync } from 'node:child_process';
import { existsSync, globSync, symlinkSync } from 'node:fs';
import { createRequire, findPackageJSON } from 'node:module';
import { dirname, join } from 'node:path';
import type { CreateNodesContext } from '@nx/devkit';
import { createNodes } from '@rasm/nx-storybook';
import { TempFs } from '@rasm/testing/temp-fs';
import framework from '@storybook/react-vite/package.json' with { type: 'json' };
import { Effect, Predicate, Schema } from 'effect';
import { getOutputsForTargetAndConfiguration } from 'nx/src/tasks-runner/utils';
import dom from 'react-dom/package.json' with { type: 'json' };
import react from 'react/package.json' with { type: 'json' };
import type { StorybookConfig } from 'storybook/internal/types';
import storybook from 'storybook/package.json' with { type: 'json' };
import vite from 'vite/package.json' with { type: 'json' };
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const [, createNodesFunction] = createNodes;
const check = createRequire(import.meta.resolve('@rasm/nx-storybook')).resolve('@nx/storybook/ready-check');

expect.addSnapshotSerializer({
    test: (value: unknown) => Predicate.isString(value) && value.includes(check),
    serialize: (value: string, config, indentation, depth, refs, printer) => printer(value.replaceAll(check, '@nx/storybook/ready-check'), config, indentation, depth, refs),
});

// --- [MODELS] --------------------------------------------------------------------------

const Planned = Schema.Struct({ command: Schema.String, options: Schema.Struct({ cwd: Schema.String }) }).annotate({ description: 'The command and working folder of a run-commands target' });

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const cwd = process.cwd();
        const tempFs = new TempFs('storybook-plugin');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        tempFs.createFilesSync({
            'package.json': JSON.stringify({ name: 'repo' }),
            'package-lock.json': '{}',
            'my-app/project.json': JSON.stringify({ name: 'my-app' }),
            'my-react-lib/project.json': JSON.stringify({ name: 'my-react-lib' }),
            'my-webpack-app/project.json': JSON.stringify({ name: 'my-webpack-app' }),
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
    }))
    .extend('installPackage', ({ tempFs }) => (projectRoot: string, packageName: string) => {
        tempFs.createFilesSync({
            [`${projectRoot}/node_modules/${packageName}/package.json`]: JSON.stringify({ name: packageName, main: 'index.js' }),
            [`${projectRoot}/node_modules/${packageName}/index.js`]: '',
        });
    })
    .extend('mockStorybookMainConfig', ({ tempFs }) => (mainTsPath: string, mainTsConfig: StorybookConfig) => {
        tempFs.createFileSync(mainTsPath, `export default ${JSON.stringify(mainTsConfig)};\n`);
    })
    .extend('myApp', ({ context, mockStorybookMainConfig }) => {
        mockStorybookMainConfig('my-app/.storybook/main.ts', {
            stories: ['../src/app/**/*.stories.@(js|jsx|ts|tsx|mdx)'],
            addons: ['@storybook/addon-essentials', '@storybook/addon-interactions'],
            framework: {
                name: '@storybook/react-vite',
                options: {},
            },
        });
        return createNodesFunction(['my-app/.storybook/main.ts'], {}, context);
    })
    .extend('myReactLib', ({ context, mockStorybookMainConfig }) => {
        mockStorybookMainConfig('my-react-lib/.storybook/main.js', {
            stories: ['../src/lib/**/*.stories.@(js|jsx|ts|tsx|mdx)'],
            addons: ['@storybook/addon-essentials'],
            framework: {
                name: '@storybook/react-vite',
                options: {
                    builder: {
                        viteConfigPath: 'vite.config.js',
                    },
                },
            },
        });
        return createNodesFunction(['my-react-lib/.storybook/main.js'], {}, context);
    });

describe("should create nodes, inputs derived from the config's stories, no build-deps or watch-deps as workspace packages export source", () => {
    it('should create one storybook project at the parent of .storybook', ({ myApp }) => {
        expect(myApp[0]?.[0]).toBe('my-app/.storybook/main.ts');
        expect(Object.keys({ ...myApp[0]?.[1].projects })).toStrictEqual(['my-app']);
        expect(myApp[0]?.[1].projects?.['my-app']?.root).toBe('my-app');
        expect(myApp[0]?.[1].projects?.['my-app']?.metadata).toStrictEqual({ technologies: ['storybook'] });
        expect(Object.keys({ ...myApp[0]?.[1].projects?.['my-app']?.targets })).toMatchInlineSnapshot(`
          [
            "build",
            "build--storybook",
            "preview",
            "preview--storybook",
            "serve",
            "serve--storybook",
            "clean",
            "clean--storybook",
          ]
        `);
    });

    it('should create build', ({ myApp }) => {
        expect(myApp[0]?.[1].projects?.['my-app']?.targets?.['build']).toMatchInlineSnapshot(`
          {
            "dependsOn": [
              "build--*",
            ],
            "inputs": [
              "...",
              "production",
              "{workspaceRoot}/{projectRoot}/.storybook/**/*",
              "{workspaceRoot}/my-app/src/app/**/*.stories.@(js|jsx|ts|tsx|mdx)",
              "^production",
              {
                "externalDependencies": [
                  "storybook",
                ],
              },
            ],
          }
        `);
    });

    it('should create build--storybook', ({ myApp }) => {
        expect(myApp[0]?.[1].projects?.['my-app']?.targets?.['build--storybook']).toMatchInlineSnapshot(`
          {
            "cache": true,
            "command": "storybook build",
            "dependsOn": [
              "codegen--*",
              "^codegen",
            ],
            "inputs": [
              "production",
              "{workspaceRoot}/{projectRoot}/.storybook/**/*",
              "{workspaceRoot}/my-app/src/app/**/*.stories.@(js|jsx|ts|tsx|mdx)",
              "^production",
              {
                "externalDependencies": [
                  "storybook",
                ],
              },
            ],
            "metadata": {
              "description": "Builds the static Storybook of the project stories with storybook build",
              "help": {
                "command": "npx storybook build --help",
                "example": {
                  "args": [
                    "--test",
                  ],
                },
              },
              "technologies": [
                "storybook",
              ],
            },
            "options": {
              "cwd": "my-app",
            },
            "outputs": [
              "{projectRoot}/storybook-static",
              "{projectRoot}/{options.output-dir}",
              "{projectRoot}/{options.outputDir}",
              "{projectRoot}/{options.o}",
            ],
          }
        `);
    });

    it('should create serve--storybook', ({ myApp }) => {
        expect(myApp[0]?.[1].projects?.['my-app']?.targets?.['serve--storybook']).toMatchInlineSnapshot(`
          {
            "command": "storybook dev",
            "continuous": true,
            "dependsOn": [
              "codegen--*",
              "^codegen",
            ],
            "inputs": [
              "production",
              "{workspaceRoot}/{projectRoot}/.storybook/**/*",
              "{workspaceRoot}/my-app/src/app/**/*.stories.@(js|jsx|ts|tsx|mdx)",
              "^production",
              {
                "externalDependencies": [
                  "storybook",
                ],
              },
            ],
            "metadata": {
              "description": "Serves the project stories with storybook dev",
              "help": {
                "command": "npx storybook dev --help",
                "example": {
                  "args": [
                    "--port=<port>",
                  ],
                },
              },
              "technologies": [
                "storybook",
              ],
            },
            "options": {
              "cwd": "my-app",
            },
            "readyWhen": {
              "command": "node -e "require('@nx/storybook/ready-check').waitForDevServer()"",
              "interval": 1000,
            },
          }
        `);
    });

    it('should create preview--storybook', ({ myApp }) => {
        expect(myApp[0]?.[1].projects?.['my-app']?.targets?.['preview--storybook']).toMatchInlineSnapshot(`
          {
            "continuous": true,
            "dependsOn": [
              "build--storybook",
            ],
            "executor": "@nx/web:file-server",
            "inputs": [
              "production",
              "{workspaceRoot}/{projectRoot}/.storybook/**/*",
              "{workspaceRoot}/my-app/src/app/**/*.stories.@(js|jsx|ts|tsx|mdx)",
              "^production",
              {
                "externalDependencies": [
                  "storybook",
                ],
              },
            ],
            "metadata": {
              "description": "Serves the static Storybook build--storybook writes with @nx/web:file-server",
              "help": {
                "command": "npx nx run --help",
                "example": {
                  "options": {
                    "port": 4400,
                  },
                },
              },
              "technologies": [
                "storybook",
              ],
            },
            "options": {
              "buildTarget": "build--storybook",
              "staticFilePath": "my-app/storybook-static",
            },
            "readyWhen": {
              "signal": true,
            },
          }
        `);
    });

    it('should create clean--storybook', ({ myApp }) => {
        expect(myApp[0]?.[1].projects?.['my-app']?.targets?.['clean--storybook']).toMatchInlineSnapshot(`
          {
            "executor": "@rasm/nx-distribution:clean",
            "inputs": [
              "default",
              "^default",
              {
                "externalDependencies": [],
              },
            ],
            "metadata": {
              "description": "Removes the outputs build--storybook declare",
              "help": {
                "command": "npx nx run --help",
                "example": {
                  "options": {
                    "targets": [
                      "build--storybook",
                    ],
                  },
                },
              },
              "technologies": [
                "storybook",
              ],
            },
            "options": {
              "targets": [
                "build--storybook",
              ],
            },
          }
        `);
    });
});

describe("should support main.js, inputs derived from the config's stories, no build-deps or watch-deps as workspace packages export source", () => {
    it('should create one storybook project from main.js', ({ myReactLib }) => {
        expect(myReactLib[0]?.[0]).toBe('my-react-lib/.storybook/main.js');
        expect(Object.keys({ ...myReactLib[0]?.[1].projects?.['my-react-lib']?.targets })).toMatchInlineSnapshot(`
          [
            "build",
            "build--storybook",
            "preview",
            "preview--storybook",
            "serve",
            "serve--storybook",
            "clean",
            "clean--storybook",
          ]
        `);
    });

    it('should create build--storybook', ({ myReactLib }) => {
        expect(myReactLib[0]?.[1].projects?.['my-react-lib']?.targets?.['build--storybook']).toMatchInlineSnapshot(`
          {
            "cache": true,
            "command": "storybook build",
            "dependsOn": [
              "codegen--*",
              "^codegen",
            ],
            "inputs": [
              "production",
              "{workspaceRoot}/{projectRoot}/.storybook/**/*",
              "{workspaceRoot}/my-react-lib/src/lib/**/*.stories.@(js|jsx|ts|tsx|mdx)",
              "^production",
              {
                "externalDependencies": [
                  "storybook",
                ],
              },
            ],
            "metadata": {
              "description": "Builds the static Storybook of the project stories with storybook build",
              "help": {
                "command": "npx storybook build --help",
                "example": {
                  "args": [
                    "--test",
                  ],
                },
              },
              "technologies": [
                "storybook",
              ],
            },
            "options": {
              "cwd": "my-react-lib",
            },
            "outputs": [
              "{projectRoot}/storybook-static",
              "{projectRoot}/{options.output-dir}",
              "{projectRoot}/{options.outputDir}",
              "{projectRoot}/{options.o}",
            ],
          }
        `);
    });

    it('should create serve--storybook', ({ myReactLib }) => {
        expect(myReactLib[0]?.[1].projects?.['my-react-lib']?.targets?.['serve--storybook']).toMatchInlineSnapshot(`
          {
            "command": "storybook dev",
            "continuous": true,
            "dependsOn": [
              "codegen--*",
              "^codegen",
            ],
            "inputs": [
              "production",
              "{workspaceRoot}/{projectRoot}/.storybook/**/*",
              "{workspaceRoot}/my-react-lib/src/lib/**/*.stories.@(js|jsx|ts|tsx|mdx)",
              "^production",
              {
                "externalDependencies": [
                  "storybook",
                ],
              },
            ],
            "metadata": {
              "description": "Serves the project stories with storybook dev",
              "help": {
                "command": "npx storybook dev --help",
                "example": {
                  "args": [
                    "--port=<port>",
                  ],
                },
              },
              "technologies": [
                "storybook",
              ],
            },
            "options": {
              "cwd": "my-react-lib",
            },
            "readyWhen": {
              "command": "node -e "require('@nx/storybook/ready-check').waitForDevServer()"",
              "interval": 1000,
            },
          }
        `);
    });

    it('should create preview--storybook', ({ myReactLib }) => {
        expect(myReactLib[0]?.[1].projects?.['my-react-lib']?.targets?.['preview--storybook']).toMatchInlineSnapshot(`
          {
            "continuous": true,
            "dependsOn": [
              "build--storybook",
            ],
            "executor": "@nx/web:file-server",
            "inputs": [
              "production",
              "{workspaceRoot}/{projectRoot}/.storybook/**/*",
              "{workspaceRoot}/my-react-lib/src/lib/**/*.stories.@(js|jsx|ts|tsx|mdx)",
              "^production",
              {
                "externalDependencies": [
                  "storybook",
                ],
              },
            ],
            "metadata": {
              "description": "Serves the static Storybook build--storybook writes with @nx/web:file-server",
              "help": {
                "command": "npx nx run --help",
                "example": {
                  "options": {
                    "port": 4400,
                  },
                },
              },
              "technologies": [
                "storybook",
              ],
            },
            "options": {
              "buildTarget": "build--storybook",
              "staticFilePath": "my-react-lib/storybook-static",
            },
            "readyWhen": {
              "signal": true,
            },
          }
        `);
    });
});

describe('@rasm/nx-storybook', () => {
    it('should hash an .mjs story under a stories folder, which the official hand glob misses', async ({ context, tempFs, mockStorybookMainConfig }) => {
        tempFs.createFileSync('my-app/src/button.stories.mjs', 'export default {};\n');
        mockStorybookMainConfig('my-app/.storybook/main.ts', {
            stories: ['../src'],
            framework: { name: '@storybook/react-vite', options: {} },
        });

        const nodes = await createNodesFunction(['my-app/.storybook/main.ts'], {}, context);

        expect(nodes[0]?.[1].projects?.['my-app']?.targets?.['build--storybook']?.inputs).toContain('{workspaceRoot}/my-app/src/**/*.@(mdx|stories.@(js|jsx|mjs|ts|tsx))');
    });

    it('should stop using production once nx.json removes it', async ({ context, mockStorybookMainConfig }) => {
        mockStorybookMainConfig('my-app/.storybook/main.ts', {
            stories: ['../src/**/*.stories.tsx'],
            framework: { name: '@storybook/react-vite', options: {} },
        });
        await createNodesFunction(['my-app/.storybook/main.ts'], {}, context);

        const nodes = await createNodesFunction(['my-app/.storybook/main.ts'], {}, { ...context, nxJsonConfiguration: { namedInputs: { default: ['{projectRoot}/**/*'] } } });

        expect(nodes[0]?.[1].projects?.['my-app']?.targets?.['build--storybook']?.inputs).toStrictEqual(['default', '^default', { externalDependencies: ['storybook'] }]);
    });

    it('should infer the test runner target when only it is installed', async ({ context, installPackage, mockStorybookMainConfig }) => {
        installPackage('my-webpack-app', '@storybook/test-runner');
        mockStorybookMainConfig('my-webpack-app/.storybook/main.ts', {
            stories: ['../src/app/**/*.stories.@(js|jsx|ts|tsx|mdx)'],
            addons: [],
            framework: { name: '@storybook/react-webpack5', options: {} },
        });

        const nodes = await createNodesFunction(['my-webpack-app/.storybook/main.ts'], {}, context);

        expect(nodes[0]?.[1].projects?.['my-webpack-app']?.targets?.['test--storybook']).toMatchObject({ command: 'test-storybook', dependsOn: [{ target: 'serve--storybook', waitFor: 'ready' }] });
    });

    it('should write every file of build--storybook under its declared outputs and every declared output', { timeout: 120_000 }, async ({ tempFs, context, mockStorybookMainConfig }) => {
        [storybook.name, framework.name, react.name, dom.name, vite.name].forEach((name) => {
            tempFs.createDirSync(dirname(join('node_modules', name)));
            symlinkSync(dirname(String(findPackageJSON(name, import.meta.url))), join(tempFs.tempDir, 'node_modules', name), 'dir');
        });
        tempFs.createDirSync('node_modules/.bin');
        symlinkSync(join(tempFs.tempDir, 'node_modules/storybook/dist/bin/dispatcher.js'), join(tempFs.tempDir, 'node_modules/.bin/storybook'));
        tempFs.createFileSync('my-app/label.stories.tsx', "export default { title: 'Label', component: () => 'label' };\n\nexport const Shown = {};\n");
        mockStorybookMainConfig('my-app/.storybook/main.ts', { stories: ['../*.stories.tsx'], framework: { name: '@storybook/react-vite', options: {} }, core: { disableTelemetry: true } });
        const nodes = await createNodesFunction(['my-app/.storybook/main.ts'], {}, context);
        const targets = { ...nodes[0]?.[1].projects?.['my-app']?.targets };
        const { command, options } = await Effect.runPromise(Schema.decodeUnknownEffect(Planned)(targets['build--storybook']));
        const outputs = getOutputsForTargetAndConfiguration({ project: 'my-app', target: 'build--storybook' }, {}, { name: 'my-app', type: 'app', data: { root: 'my-app', targets } });
        const before = new Set(globSync('**', { cwd: tempFs.tempDir, exclude: ['node_modules/**'] }));

        execSync(`PATH="${join(tempFs.tempDir, 'node_modules/.bin')}:$PATH" ${command}`, { cwd: join(tempFs.tempDir, options.cwd), stdio: 'ignore' });

        expect(globSync('**', { cwd: tempFs.tempDir, exclude: ['node_modules/**', ...outputs.flatMap((output) => [output, `${output}/**`])] }).filter((file) => !before.has(file))).toStrictEqual([]);
        expect(outputs.filter((output) => !existsSync(join(tempFs.tempDir, output)))).toStrictEqual([]);
    });
});
