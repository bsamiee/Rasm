using OfficeOpenXml;

namespace Rasm.Interop.Excel;

/// <summary>EPPlus license registration; executables call <see cref="Initialize"/> once at the composition root</summary>
/// <remarks>
/// Unlicensed use throws <see cref="LicenseNotSetException"/> from the <see cref="ExcelPackage.Workbook"/> getter, EPPlus caches no negative result, and
/// the getter succeeds once initialization runs; the environment variable EPPlusLicense with value NonCommercialOrganization:Rasm covers CI jobs and shell
/// sessions, but Finder-launched processes receive no shell environment and writing process environment from a plugin mutates state shared with every
/// other plugin, so the facade calls the license API instead
/// </remarks>
public static class ExcelInterop {
    /// <summary>Registers the EPPlus noncommercial organization license for Rasm</summary>
    public static void Initialize() => ExcelPackage.License.SetNonCommercialOrganization("Rasm");
}
