using System.Collections.Generic;
using Core.GridPawns;

namespace Core.Tasks
{
    public static class TaskRequirementMatcher
    {
        // Every goal consumes a distinct, live board item. Inventory items don't count.
        public static List<Appliance> Match(GridPawn[,] grid, IReadOnlyList<Goal> goals)
        {
            var matched = new List<Appliance>();
            foreach (var goal in goals)
            {
                Appliance found = null;
                foreach (var pawn in grid)
                {
                    if (pawn is Appliance appliance && appliance.ApplianceType == goal.ApplianceType &&
                        appliance.Level == goal.Level && !matched.Contains(appliance))
                    {
                        found = appliance;
                        break;
                    }
                }
                if (found == null) return null;
                matched.Add(found);
            }
            return matched;
        }
    }
}
