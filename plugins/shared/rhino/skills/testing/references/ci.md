# [CI]

Rhino tests need Rhino installed and licensed on the runner, and only some runners allow both.

## [01]-[RUNNERS]

| [INDEX] | [RUNNER]                    | [INSTALL]                   | [LICENSE]       | [STATUS]                                       |
| :-----: | :-------------------------- | :-------------------------- | :-------------- | :--------------------------------------------- |
|  [01]   | GitHub-hosted Windows       | Silent installer            | `RHINO_TOKEN`   | McNeel's own harnesses test here               |
|  [02]   | Self-hosted macOS           | Machine owner               | Signed-in Rhino | Template host, runner in the signed-in session |
|  [03]   | Self-hosted Windows desktop | Machine owner               | Signed-in Rhino | Rhino processes and plug-in files persist      |
|  [04]   | Linux container             | `rhino3d` apt package       | `RHINO_TOKEN`   | Headless Rhino with defects, no Grasshopper 2  |
|  [05]   | GitHub-hosted macOS         | `setup-rhino3d@v4` dmg copy | None            | No license route for an ephemeral Mac          |
|  [06]   | GitHub-hosted Linux         | None                        | None            | Rhino-free projects                            |

- Self-hosted macOS runners run as a LaunchAgent (`svc.sh install`) of the user signed in to Rhino, a LaunchDaemon has no user session for Rhino
- Rhino-free projects run on every runner in a separate job, a Rhino outage blocks no other test

## [02]-[LICENSING]

- `RHINO_TOKEN` holds a core-hour billing token from a team with no licenses in it, read by Rhino from the test process environment
- Core-hour billing is required on Windows Server and the only route on Linux, with Rhino 9 enabled on the billing team, and macOS takes no token
- Billing runs per core while Rhino runs, tests boot Rhino once per process and the job ends with the run
- Missing or invalid tokens surface as `Rhino.Runtime.NotLicensedException` or `PAL_SEHException` on the first native call, not as a license message
- Linux keeps its license cache under `$HOME/.config/McNeel`, and containers run as a user with a writable `HOME`
- Pass the token as a secret to the test step alone, never into an image or a build step
- Rhino license keys install through the Windows installer (`LICENSE_METHOD=STANDALONE LICENSE_KEY=<key>`) on desktops alone, as Windows Server refuses single-computer keys
- Single-computer keys validate on one machine per key with a license file no other machine reads, ruling out ephemeral runners
- Email addresses go to McNeel's download form, read by `setup-rhino3d` input `email-address`

## [03]-[INSTALL]

`mcneel/setup-rhino3d@v4` installs Rhino on Windows and macOS runners, without cache, Linux support, or licensing:
- Inputs: `email-address`, `major-version` (`9` default, or `8`), `release-version` (`latest` default, or `rc`, `wip` refused while no WIP exists), `download-url`
- `download-url` overrides both versions and installs one build, its file name holding `rhino_<major>.`
- `v2` fails on macOS with `Unsupported platform`
- Windows runs the installer with `-passive -norestart`, installs to `C:\Program Files\Rhino <major>`, and records `HKLM:\SOFTWARE\McNeel\Rhinoceros\<major>.0\Install`
- On macOS it mounts the dmg with `hdiutil attach -nobrowse` and copies the `.app` to `/Applications`

Download URLs resolve by channel slug, the redirect names a versioned file (`rhino_9.1.<build>.exe`) that serves as cache key:

```bash
curl -fsS -o /dev/null -w '%{redirect_url}' --get --data-urlencode 'slug=rhino-for-windows/9/latest' --data-urlencode "email=${EMAIL}" https://www.rhino3d.com/www-api/download/direct
curl -fsS -o /dev/null -w '%{redirect_url}' --get --data-urlencode 'slug=rhino-for-mac/9/latest' --data-urlencode "email=${EMAIL}" https://www.rhino3d.com/www-api/download/direct
```

- Slugs take `latest`, `wip`, and `latest/rc` after the major

Linux installs from McNeel's apt repository:

```bash
wget -qO- https://mcneel-packages.s3.amazonaws.com/mcneel-packages.gpg.key | gpg --dearmor -o /usr/share/keyrings/mcneel-archive-keyring.gpg
echo "deb [signed-by=/usr/share/keyrings/mcneel-archive-keyring.gpg] https://mcneel-packages.s3.amazonaws.com/deb stable main" > /etc/apt/sources.list.d/mcneel.list
apt-get update && apt-get install -y "rhino3d=<version>" && apt-mark hold rhino3d
```

- `stable main` resolves to the newest nightly build, pin `rhino3d=<version>` and hold it
- Rhino installs at `/usr/lib/rhino3d`, Rhino.Inside finds it there, arm64 packages lag amd64, and McNeel ships Linux Rhino 9 as a beta feature
- Known Linux defects: block instance APIs throw `EntryPointNotFoundException`, `OpenHeadless` aborts on some large models with a native double free, and Grasshopper 2 stays off

## [04]-[SEQUENCE]

1. Checkout without persisted credentials
2. .NET SDK install, `global.json` selecting Microsoft.Testing.Platform for `dotnet test`
3. NuGet cache keyed on project files
4. Test run without Rhino, passing where each task hits a cache keyed on the installer URL
5. Rhino install where that run fails
6. Build once in Release
7. `dotnet test --no-build --results-directory <dir>` with `RHINO_TOKEN` on this step alone
8. Upload the results folder with `if: ${{ !cancelled() }}`, keeping dumps of failures and crashes
9. Report the TRX files to checks and the job summary
10. Package and publish in a separate job with `needs` on the test job

Job settings:
- `concurrency` with `cancel-in-progress` per pull request stops billing for superseded runs, and a queue (`queue: max`) keeps every push to the default branch
- `timeout-minutes` above the hang dump timeout, a hang ends in a dump before the job is killed
- `permissions: contents: read` at workflow level, `checks: write` on the job that reports
- `fail-fast: false` across runners, one platform's failure keeps the other's results

## [05]-[ARTEFACTS]

| [INDEX] | [FILE]                              | [WHEN]    | [USE]                                        |
| :-----: | :---------------------------------- | :-------- | :------------------------------------------- |
|  [01]   | `*.trx`                             | Every run | Results with each test's captured output     |
|  [02]   | `*_crash.sequence.log`              | Crash     | Test running at the crash                    |
|  [03]   | `*_crash.dmp`, `*.crashreport.json` | Crash     | Stacks, `dotnet-dump analyze` on any machine |
|  [04]   | Hang dump                           | Hang      | Stacks of the hung test                      |
|  [05]   | Installer log                       | Install   | Installed Rhino version and path             |

- Dumps of type `Mini` keep artefacts near 200 MB, type `Full` holds all of Rhino's memory
- `dorny/test-reporter` with `reporter: dotnet-trx` writes a check run and the job summary from TRX files

## [06]-[FAILURES]

| [INDEX] | [SYMPTOM]                                   | [CAUSE]                                             |
| :-----: | :------------------------------------------ | :-------------------------------------------------- |
|  [01]   | `ResolveRhinoApplication` build error       | Rhino or its .NET runtime absent at build           |
|  [02]   | `NotLicensedException`, `PAL_SEHException`  | `RHINO_TOKEN` absent from the test step             |
|  [03]   | `Unsupported platform` on macOS             | `setup-rhino3d@v2`                                  |
|  [04]   | Rhino 8 installed for a Rhino 9 project     | `setup-rhino3d` `major-version: 8`                  |
|  [05]   | Job killed at timeout without dumps         | Job timeout below hang dump timeout                 |
|  [06]   | Locked `.rhp` or `.gha` on self-hosted runs | Rhino process left from an earlier run              |
|  [07]   | Linux tests break after an image rebuild    | Unpinned `rhino3d` package moved to a newer nightly |
