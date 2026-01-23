using System;

namespace ThreeMatch
{
    public interface IModuleRegistrar<T> : IManager where T : IModule
    {
        public void Register(T module);
    }
}
