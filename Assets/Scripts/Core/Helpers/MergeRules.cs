using Core.GridPawns;

namespace Core.Helpers
{
    public static class MergeRules
    {
        public static bool CanMerge(GridPawn first, GridPawn second)
        {
            return first != null && second != null && first != second &&
                   first.Level < first.MaxLevel && first.Level == second.Level &&
                   first.Type.Equals(second.Type);
        }
    }
}
