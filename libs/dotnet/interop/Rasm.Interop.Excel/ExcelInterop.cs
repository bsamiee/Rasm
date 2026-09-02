using OfficeOpenXml;

namespace Rasm.Interop.Excel;

/// <summary>EPPlus license registration for executables to call once at the composition root</summary>
/// <remarks>
/// Unlicensed use throws <see cref="LicenseNotSetException"/> from the <see cref="ExcelPackage.Workbook"/> getter until initialization runs, as EPPlus caches no negative result.
/// The facade calls the license API instead of the environment variable EPPlusLicense with value NonCommercialOrganization:Rasm, which covers CI jobs and shell sessions,
/// but Finder-launched processes receive no shell environment and writing it from a plugin mutates state shared with every other plugin
/// </remarks>
public static class ExcelInterop {
    /// <summary>Registers the EPPlus noncommercial organization license for Rasm</summary>
    public static void Initialize() => ExcelPackage.License.SetNonCommercialOrganization("Rasm");
}
