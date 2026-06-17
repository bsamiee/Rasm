# [PLATFORM_TASKLOG]

Open work owned by this folder; closed items do not appear. `[STATUS]` is one of `QUEUED`, `ACTIVE`, `BLOCKED`, `SPIKE`; owner state is read at `ARCHITECTURE.md` `[OWNER_REGISTRY]`. Every `SPIKE` row names the probe that flips its owner registry cell to `FINALIZED`.

## [1]-[PROBES]

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | `ServiceWorkerHost` install/activate/skipWaiting lifecycle and offline-first navigation fallback: live-browser offline-queue redial-drain convergence probe | service-worker#SERVICE_WORKER | SPIKE |
| [2] | `CrashTelemetry` global capture: live-browser probe marshalling uncaught faults into the typed fault family with breadcrumb-ring + sanitized-envelope ship-through | error-boundary#ERROR_BOUNDARY | SPIKE |
| [3] | `PerformanceBudget` Core-Web-Vitals capture: live-browser PerformanceObserver probe feeding LCP/INP/CLS/TTFB/FCP attribution into the MetricRegistry rows | web-vitals#WEB_VITALS | SPIKE |

## [2]-[ADMISSION_GATES]

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | `vite-plugin-pwa` + `workbox-build` + `workbox-window` catalogue-pending: catalog rows admitted, catalogue page registration pending | service-worker#SERVICE_WORKER | QUEUED |
| [2] | `nuqs` catalogue-pending: catalog row admitted, catalogue page registration pending | routing-navigation#ROUTING_NAVIGATION | QUEUED |
| [3] | `react-error-boundary` catalogue-pending: catalog row admitted, catalogue page registration pending | error-boundary#ERROR_BOUNDARY | QUEUED |
| [4] | The single `web-vitals` catalog row activates only if native PerformanceObserver attribution is judged insufficient; default zero-package | web-vitals#WEB_VITALS | BLOCKED |
| [5] | Implementation-time root/folder `package.json` `./web` subpath `exports` authored at transcription | host-runtime#HOST_RUNTIME | QUEUED |

## [3]-[TRANSCRIPTION]

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Transcribe the build-order modules per `ARCHITECTURE.md` `[SOURCE_TREE]` (`platform-substrate.ts` through `browser.ts`); each module transcribes its page clusters verbatim | platform-substrate#PLATFORM_SUBSTRATE | QUEUED |
