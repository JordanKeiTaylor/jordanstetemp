namespace Improbable.Behaviour
{
    /// <summary>
    /// Primary behaviour of <see cref="Improbable.Worker.GenericTickWorker"/>.
    /// </summary>
    public interface ITickBehaviour
    {
        void Tick();
    }
}
