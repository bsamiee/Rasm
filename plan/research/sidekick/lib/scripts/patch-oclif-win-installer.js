#!/usr/bin/env node

/**
 * Patch oclif's NSIS template for `oclif pack win` so the installer can add
 * itself to PATH regardless of how long the user's PATH already is.
 *
 * Why (issue #346): the stock template reads the user PATH with
 * RegQueryValueExW using NSIS_MAX_STRLEN (1024) as a BYTE count. With UTF-16
 * registry strings that halves the real limit: any user PATH over ~511
 * characters makes the installer pop "PATH not updated, original length
 * N > 1024" and skip the append - and because that MessageBox has no /SD
 * flag, a silent install (/S) hangs on it forever.
 *
 * Verified upstream-unfixed through oclif 5.0.1; Ubuntu's nsis package is a
 * standard 1024 build (Debian #942396, open since 2019), so rebuilding NSIS
 * in CI is not an option either.
 *
 * The fix: replace the AddToPath call with a PowerShell one-liner that reads
 * and appends the registry value entirely outside NSIS strings. PowerShell
 * 5.1 is a built-in Windows component, and this template already shells out
 * to PowerShell for the Defender exclusion. Notes:
 *  - GetValue(..., DoNotExpandEnvironmentNames) + SetValue(..., ExpandString)
 *    preserve REG_EXPAND_SZ entries like %USERPROFILE%\...\WindowsApps.
 *  - Never use `setx`: it truncates PATH at 1024 characters itself.
 *  - On a failure (e.g. hardened orgs with Constrained Language Mode) the
 *    step logs and moves on - same degraded behavior as the stock template,
 *    but it can never hang or clobber an existing PATH.
 */

import { readFileSync, writeFileSync } from "node:fs";
import { createRequire } from "node:module";

export const PATCH_MARKER = "; sidekick-path-patch (issue #346)";

// The exact install-section body we replace. Written against oclif 4.x/5.x
// (identical in both); if oclif ever changes this, the anchor mismatch makes
// the build fail loudly instead of silently shipping the broken PATH step.
const ANCHOR = `  Push "$INSTDIR\\\\bin"
  Call AddToPath`;

// PowerShell body, NSIS-escaped: PowerShell variables are written $$name so
// NSIS emits a literal $; $INSTDIR stays single-$ so NSIS expands it. \\bin
// keeps a literal backslash after JS template-literal unescaping.
const REPLACEMENT = `  ${PATCH_MARKER}
  ; Append $INSTDIR\\\\bin to the user PATH via PowerShell so the value never
  ; passes through a length-limited NSIS string.
  nsExec::ExecToLog \\\`powershell -NoProfile -ExecutionPolicy Bypass -Command "$$k=[Microsoft.Win32.Registry]::CurrentUser.CreateSubKey('Environment'); $$p=[string]$$k.GetValue('Path','',[Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames); $$d='$INSTDIR\\\\bin'; if(($$p -split ';') -notcontains $$d){ if([string]::IsNullOrEmpty($$p)){$$n=$$d}else{$$n=$$p.TrimEnd(';')+';'+$$d}; $$k.SetValue('Path',$$n,[Microsoft.Win32.RegistryValueKind]::ExpandString) }; $$k.Close()"\\\`
  Pop $0
  DetailPrint "PATH update via PowerShell exited: $0"
  SendMessage \\\${HWND_BROADCAST} \\\${WM_WININICHANGE} 0 "STR:Environment" /TIMEOUT=5000`;

export function patchWinTemplate(source) {
  if (source.includes(PATCH_MARKER)) {
    return source;
  }
  if (!source.includes(ANCHOR)) {
    throw new Error(
      "oclif win.js template changed shape: expected the AddToPath install " +
        "section from oclif 4.x/5.x. Re-verify issue #346 against the new " +
        "template before packing Windows installers."
    );
  }
  // Replacer function: a plain string here would re-interpret $$ sequences.
  return source.replace(ANCHOR, () => REPLACEMENT);
}

function main() {
  const require = createRequire(import.meta.url);
  const oclifPkg = require.resolve("oclif/package.json");
  const winJs = oclifPkg.replace(/package\.json$/, "lib/commands/pack/win.js");
  const source = readFileSync(winJs, "utf8");
  const patched = patchWinTemplate(source);
  if (patched === source) {
    console.log(`oclif win template already patched: ${winJs}`);
    return;
  }
  writeFileSync(winJs, patched);
  console.log(`Patched oclif win template for long-PATH support: ${winJs}`);
}

if (process.argv[1] && import.meta.url.endsWith(process.argv[1].split("/").pop())) {
  main();
}
