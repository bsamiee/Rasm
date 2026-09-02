using OfficeOpenXml;

namespace Rasm.Interop.Excel;

/// <summary>Provides the EPPlus license registration that executables run once at the composition root</summary>
/// <remarks>
/// <para>The <see cref="ExcelPackage.Workbook"/> getter throws <see cref="LicenseNotSetException"/> until registration runs, and EPPlus caches no negative result</para>
/// <para>The license API replaces the EPPlusLicense environment variable, which shells and CI inherit but Finder-launched processes do not, and which a plugin cannot set without changing every other plugin's environment</para>
/// </remarks>
public static class ExcelInterop {
    /// <summary>Registers the EPPlus noncommercial organization license</summary>
    public static void Initialize() => ExcelPackage.License.SetNonCommercialOrganization("Rasm");
}
