namespace Improbable.Behaviour
{
    /// <summary>
    /// Base tick behaviour interface. Tick behaviours implement logic periodically with each <code>Tick()</code>
    /// </summary>
    public interface ITickBehaviour
    {
        void Tick();
    }
}
