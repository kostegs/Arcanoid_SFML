
namespace Arcanoid_SFML.Interfaces
{
    internal interface IDestroyable
    {
        bool AllowToDestroy { get; set; }
        void Destroy();
    }
}
