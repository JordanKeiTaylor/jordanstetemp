namespace Improbable.Sandbox.Navigation.Api
{
    public class PathNode<T>
    {
        public long Id { get; set; }

        public Coordinates Coords { get; set; }
        
        public T Node { get; set; }
    }
    
    public class PathNode: PathNode<object> { }
}