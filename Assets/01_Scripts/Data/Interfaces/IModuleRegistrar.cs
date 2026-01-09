using System;

namespace ThreeMatch
{
    public interface IModuleRegistrar
    {
        public Type ModuleType { get; }
        public void Register(IModule module);
    }

    public interface IModuleRegistrar<T> : IManager, IModuleRegistrar where T : IModule
    {
        public void RegisterModule(T module);
    }
}
