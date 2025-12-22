using System;

namespace ThreeMatch
{
    public interface IModuleRegistrar<T> : IManager where T : IModule
    {
        public Type GetModuleType => typeof(T);
        public void RegisterModule(T module);
    }
}
