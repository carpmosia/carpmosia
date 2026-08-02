
namespace Content.Shared.Preferences
{
    public enum JobPriority
    {
        // These enum values HAVE to match the ones in DbJobPriority in Content.Server.Database
        Never = 0,
        // Carpmosia-start - More job priorities
        Lowest = 1,
        Lower = 2,
        Low = 3,
        Medium = 4,
        High = 5
        // Carpmosia-end - More job priorities
    }
}
