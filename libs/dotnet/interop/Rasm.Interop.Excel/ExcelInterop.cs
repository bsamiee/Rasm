using OfficeOpenXml;

namespace Rasm.Interop.Excel;

// --- [OPERATIONS] ----------------------------------------------------------------------
/// <summary>Provides the EPPlus license registration that executables run once at the composition root</summary>
/// <remarks>
/// <para>The <see cref="ExcelPackage.Workbook"/> getter throws <see cref="LicenseNotSetException"/> until registration runs, and EPPlus caches no negative result</para>
/// <para>The API call replaces the EPPlusLicense environment variable. Finder-launched processes lack it, and a plugin cannot set it without changing every other plugin's environment</para>
/// </remarks>
public static class ExcelInterop {
    /// <summary>Registers the EPPlus noncommercial organization license</summary>
    public static void Initialize() => ExcelPackage.License.SetNonCommercialOrganization("Rasm");
}
