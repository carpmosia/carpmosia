
namespace Content.Shared.Preferences
{
    public enum JobPriority
    {
        // These enum values HAVE to match the ones in DbJobPriority in Content.Server.Database
        Never = 0,
        // Carpmosia-start - More job priorities
        Low5 = 1,
        Low4 = 2,
        Low3 = 3,
        Low2 = 4,
        Low = 5,
        Medium = 6,
        High = 7
        // Carpmosia-end - More job priorities
    }
}
