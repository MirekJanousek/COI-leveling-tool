using System;
using System.Collections.Generic;
using System.Reflection;

namespace COILevelingTool.Compatibility.V086;

public sealed class CapabilityProbeV086
{
    private readonly Assembly m_core;
    private readonly Assembly m_unity;

    public CapabilityProbeV086(Assembly core, Assembly unity)
    {
        m_core = core ?? throw new ArgumentNullException(nameof(core));
        m_unity = unity ?? throw new ArgumentNullException(nameof(unity));
    }

    public CapabilityProfileV086 Probe(string runtimeGameVersion)
    {
        var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        var terrain = m_core.GetType("Mafi.Core.Terrain.TerrainManager", false);
        var construction = m_core.GetType("Mafi.Core.Entities.Static.IConstructionManager", false);
        var entities = m_core.GetType("Mafi.Core.Entities.EntitiesManager", false);
        var occupancy = m_core.GetType("Mafi.Core.Terrain.TerrainOccupancyManager", false);
        var designations = m_core.GetType("Mafi.Core.Terrain.Designation.TerrainDesignationsManager", false);
        var massPlacer = m_unity.GetType("Mafi.Unity.Ui.Controllers.LayoutEntityPlacing.StaticEntityMassPlacer", false);

        bool Has(Type? type, string member) => type?.GetMember(member, flags).Length > 0;
        var capabilities = new Dictionary<string, bool>(StringComparer.Ordinal)
        {
            ["terrain.read"] = Has(terrain, "GetHeight"),
            ["terrain.write_no_physics"] = Has(terrain, "SetHeightPreserveRelativeLayersNoPhysics"),
            ["terrain.notify_changed"] = Has(terrain, "NotifyTileHeightLayersChanged"),
            ["terrain.bounds"] = Has(terrain, "IsValidCoord") && Has(terrain, "IsOffLimitsOrInvalid"),
            ["terrain.occupancy"] = Has(occupancy, "IsOccupiedAt"),
            ["designation.canonical_range"] = Has(designations, "GetCanonicalDesignationRange"),
            ["construction.completed_event"] = Has(construction, "EntityConstructed"),
            ["construction.progress"] = Has(construction, "GetConstructionProgress"),
            ["entity.normal_removal"] = Has(entities, "RemoveAndDestroyEntity") || Has(entities, "TryRemoveAndDestroyEntity"),
            ["placement.mass_placer"] = Has(massPlacer, "SetLayoutEntityToPlace"),
            ["placement.elevation"] = massPlacer != null,
        };

        return new CapabilityProfileV086(
            NormalizeGameVersion(runtimeGameVersion),
            m_core.GetName().Version?.ToString() ?? string.Empty,
            capabilities);
    }

    public static string NormalizeGameVersion(string value) =>
        (value ?? string.Empty).Trim().TrimStart('v');
}
