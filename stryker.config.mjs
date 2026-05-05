const config = {
    // --- [CHECKERS] --------------------------------------------------------------------
    checkerNodeArgs: ['--max-old-space-size=4096'],
    checkers: ['typescript'],
    coverageAnalysis: 'perTest',
    disableTypeChecks: false,
    tsconfigFile: 'tsconfig.base.json',
    typescriptChecker: {
        prioritizePerformanceOverAccuracy: false,
    },
    // --- [SANDBOX] ---------------------------------------------------------------------
    cleanTempDir: true,
    dryRunTimeoutMinutes: 10,
    ignorePatterns: [
        '.cache',
        '.git',
        '.nx',
        '.stryker-tmp',
        '.vite',
        'build',
        'coverage',
        'dist',
        'node_modules',
        'test-results',
    ],
    ignoreStatic: true,
    incremental: true,
    incrementalFile: 'test-results/mutation/stryker-incremental.json',
    logLevel: 'info',
    tempDirName: '.stryker-tmp',
    timeoutFactor: 1.5,
    timeoutMS: 10_000,
    warnings: {
        slow: false,
    },
    // --- [MUTATE] ----------------------------------------------------------------------
    mutate: [
        'apps/**/src/**/*.{ts,tsx,mts,cts}',
        '!apps/**/*.{test,spec}.{ts,tsx,mts,cts}',
        '!apps/**/*.{d,config}.{ts,tsx,mts,cts}',
        '!apps/**/{__fixtures__,__mocks__,__tests__,fixtures,mocks,tests}/**',
        '!apps/**/{generated,dist,build,coverage,test-results}/**',
    ],
    // --- [REPORTERS] -------------------------------------------------------------------
    clearTextReporter: {
        allowEmojis: false,
        logTests: false,
        maxTestsToLog: 0,
        reportMutants: false,
        reportScoreTable: true,
        skipFull: true,
    },
    htmlReporter: {
        fileName: 'test-results/mutation/index.html',
    },
    jsonReporter: {
        fileName: 'test-results/mutation/mutation.json',
    },
    plugins: ['@stryker-mutator/vitest-runner', '@stryker-mutator/typescript-checker'],
    reporters: ['clear-text', 'html', 'json', 'progress'],
    // --- [TEST_RUNNER] -----------------------------------------------------------------
    testFiles: [
        '{tests,apps}/**/*.{test,spec}.{ts,tsx,mts,cts}',
        '!{tests,apps}/**/{e2e,browser,playwright}/**',
        '!{tests,apps}/**/{dist,build,coverage,test-results,.stryker-tmp}/**',
    ],
    testRunner: 'vitest',
    testRunnerNodeArgs: ['--max-old-space-size=4096'],
    vitest: {
        related: true,
    },
    // --- [THRESHOLDS] ------------------------------------------------------------------
    thresholds: {
        break: 50,
        high: 80,
        low: 60,
    },
};

export default config;
