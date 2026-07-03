using System.Reflection;
static class P {
    static void Main() {
        Assembly gg = Assembly.LoadFrom("/Users/bardiasamiee/.nuget/packages/geometrygymifc_core/25.7.30/lib/net8.0/GeometryGymIFCcore.dll");
        foreach (string name in new[] { "IfcElectricalCircuit", "IfcStructuralResultGroup" }) {
            Type t = gg.GetType($"GeometryGym.Ifc.{name}")!;
            bool obs = t.GetCustomAttributesData().Any(a => a.AttributeType.Name == "ObsoleteAttribute");
            string msg = obs ? t.GetCustomAttributesData().First(a => a.AttributeType.Name == "ObsoleteAttribute").ConstructorArguments.FirstOrDefault().Value?.ToString() ?? "" : "no";
            Console.WriteLine($"{name} : {t.BaseType?.Name} | obsolete={msg} | Predefined={t.GetProperty("PredefinedType")?.PropertyType.Name ?? "none"} | TheoryType={t.GetProperty("TheoryType")?.PropertyType.Name ?? "none"}");
        }
        // IfcResourceTime + BaseQuantity types on IfcConstructionResource
        Type cr = gg.GetType("GeometryGym.Ifc.IfcConstructionResource")!;
        Console.WriteLine($"IfcConstructionResource abstract={cr.IsAbstract} BaseQuantity={cr.GetProperty("BaseQuantity")?.PropertyType.Name} BaseCosts={cr.GetProperty("BaseCosts")?.PropertyType.Name} Usage={cr.GetProperty("Usage")?.PropertyType.Name}");
        // IfcCostItem members used by cost.md
        Type ci = gg.GetType("GeometryGym.Ifc.IfcCostItem")!;
        foreach (string p in new[] { "CostValues", "CostQuantities", "Controls", "Nests", "PredefinedType" })
            Console.WriteLine($"IfcCostItem.{p} = {ci.GetProperty(p)?.PropertyType.Name ?? "MISSING"}");
    }
}
