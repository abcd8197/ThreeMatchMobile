using System;

namespace ThreeMatch
{
    public interface IModule : IDisposable
    {
        public abstract Type ModuleType { get; }
    }
}
