namespace Improbable.Context
{
    /// <summary>
    /// Context Status.
    /// <list type="bullet">
    /// <item>
    /// <term>Initialized</term>
    /// <description>The context has been fully initialized.</description>
    /// </item>
    /// <item>
    /// <term>Uninitialized</term>
    /// <description>The context remains uninitialized.</description>
    /// </item>
    /// </list>
    /// </summary>
    public enum Status
    {
        Initialized = 1,
        Uninitialized = 2
    }
}