
namespace Content.Shared.Preferences
{
    public enum JobPriority
    {
        // These enum values HAVE to match the ones in DbJobPriority in Content.Server.Database
        Never = 0,
        // Carpmosia-start - More job priorities
        Low6 = 1,
        Low5 = 2,
        Low4 = 3,
        Low3 = 4,
        Low2 = 5,
        Low = 6,
        Medium = 7,
        High = 8
        // Carpmosia-end - More job priorities
    }
}
