namespace Improbable.Navigation.Api
{
    /// <summary>
    /// Path Status.
    /// <list type="bullet">
    /// <item>
    /// <term>Success</term>
    /// <description>A path has been found.</description>
    /// </item>
    /// <item>
    /// <term>Error</term>
    /// <description>An error occurred during path finding.</description>
    /// </item>
    /// <item>
    /// <term>NotFound</term>
    /// <description>An attempt was not made or a path does not exist between the specified locations.</description>
    /// </item>
    /// </list>
    /// </summary>
    public enum PathStatus
    {
        Success,
        Error,
        NotFound
    }
}