using OfficeOpenXml;

namespace Rasm.Interop.Excel;

/// <summary>EPPlus license registration for executables to call once at the composition root</summary>
/// <remarks>
/// The <see cref="ExcelPackage.Workbook"/> getter throws <see cref="LicenseNotSetException"/> until registration runs, and EPPlus caches no negative result.
/// The license API replaces the environment variable EPPlusLicense=NonCommercialOrganization:Rasm, which reaches CI jobs and shell sessions but not Finder-launched processes, and which a plugin cannot write without mutating state shared with every other plugin
/// </remarks>
public static class ExcelInterop {
    /// <summary>Registers the EPPlus noncommercial organization license</summary>
    public static void Initialize() => ExcelPackage.License.SetNonCommercialOrganization("Rasm");
}
