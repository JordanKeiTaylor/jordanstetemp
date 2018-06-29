using System;
using Improbable.Sandbox.Navigation;

namespace ste_sdk_example
{
    class Program
    {
        static void Main(string[] args)
        {
            var navigator = new DefaultMeshNavigator(
                "../recast-wrapper/recast-csharp/Improbable.Recast.Tests/Resources/Tile_+007_+006_L21.obj.tiled.bin64");
            navigator.ToString();
            Console.WriteLine("Hello World!");
        }
    }
}
