import { readFileSync } from "node:fs";
import { createRequire } from "node:module";
import { describe, expect, it } from "vitest";
import { PATCH_MARKER, patchWinTemplate } from "./patch-oclif-win-installer.js";

const require = createRequire(import.meta.url);

function readInstalledTemplate() {
  const oclifPkg = require.resolve("oclif/package.json");
  const winJs = oclifPkg.replace(/package\.json$/, "lib/commands/pack/win.js");
  return readFileSync(winJs, "utf8");
}

describe("patchWinTemplate", () => {
  it("replaces the AddToPath call with a PowerShell-based PATH append", () => {
    const patched = patchWinTemplate(readInstalledTemplate());

    expect(patched).toContain(PATCH_MARKER);
    expect(patched).toContain("nsExec::ExecToLog");
    // The install section must no longer route through the length-limited
    // NSIS AddToPath function.
    expect(patched).not.toMatch(/Call AddToPath/);
  });

  it("preserves REG_EXPAND_SZ semantics and avoids setx", () => {
    const patched = patchWinTemplate(readInstalledTemplate());

    // Reading raw (unexpanded) %VAR% entries and writing back as ExpandString
    // keeps entries like %USERPROFILE%\...\WindowsApps working.
    expect(patched).toContain("DoNotExpandEnvironmentNames");
    expect(patched).toContain("ExpandString");
    // setx silently truncates PATH at 1024 chars - never allowed here.
    expect(patched).not.toMatch(/\bsetx\b/);
  });

  it("keeps the environment-change broadcast for new terminals", () => {
    const patched = patchWinTemplate(readInstalledTemplate());
    const section = patched.slice(patched.indexOf(PATCH_MARKER));
    expect(section).toContain("SendMessage");
    expect(section).toContain("STR:Environment");
  });

  it("escapes PowerShell variables for NSIS ($$) but expands $INSTDIR", () => {
    const patched = patchWinTemplate(readInstalledTemplate());
    const section = patched.slice(
      patched.indexOf(PATCH_MARKER),
      patched.indexOf("SectionEnd", patched.indexOf(PATCH_MARKER))
    );
    // PowerShell variables must reach NSIS as $$name; a bare $k / $p / $n / $d
    // would be an undefined NSIS variable and corrupt the generated script.
    expect(section).toMatch(/\$\$k=/);
    expect(section).not.toMatch(/[^$]\$k=/);
    // $INSTDIR must stay single-$ so NSIS expands it into the command.
    expect(section).toMatch(/[^$]\$INSTDIR\\\\bin/);
  });

  it("is idempotent: patching twice returns the same output", () => {
    const once = patchWinTemplate(readInstalledTemplate());
    const twice = patchWinTemplate(once);
    expect(twice).toBe(once);
  });

  it("throws loudly when the expected template shape is missing", () => {
    expect(() => patchWinTemplate('Section "nothing here"\nSectionEnd')).toThrow(
      /oclif win\.js template/
    );
  });
});

describe("renderSmokeNsi", () => {
  it("renders a compilable standalone .nsi with the patched section", async () => {
    const { renderSmokeNsi } = await import("./render-nsis-path-smoke.js");
    const nsi = renderSmokeNsi(readInstalledTemplate());

    // Template-literal placeholders must be fully rendered away.
    expect(nsi).not.toContain("${config.name}");
    expect(nsi).toContain('Section "Set PATH to path-smoke"');
    expect(nsi).toContain("SectionEnd");
    // The defines the section relies on must be present in the standalone file.
    expect(nsi).toContain("!define HWND_BROADCAST");
    expect(nsi).toContain("!define WM_WININICHANGE");
    // PowerShell $-escapes survive rendering as NSIS $$ literals.
    expect(nsi).toContain("$$k=");
    expect(nsi).toContain("$INSTDIR\\bin");
  });
});
