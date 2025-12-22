using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    public class Main : IDisposable
    {
        public static Main Instance = _lazy.Value;
        private static readonly Lazy<Main> _lazy = new Lazy<Main>(() => new Main());
        private readonly Dictionary<Type, IManager> _globalManagers = new();

        private Main()
        {
            Initialize();
        }

        private void Initialize()
        {

        }

        public void RegisterManager<T>(T manager) where T : IManager
        {
            var type = typeof(T);
            if (_globalManagers.ContainsKey(type))
            {
                throw new Exception($"Manager of type {type} is already registered.");
            }

            _globalManagers[type] = manager;

            if (manager is IModule module)
                RegisterModule(module);
        }

        private void RegisterModule<TModule>(TModule module) where TModule : IModule
        {
            foreach (var manager in _globalManagers.Values)
            {
                var registrarType = typeof(IModuleRegistrar<TModule>);
                if (registrarType.IsAssignableFrom(manager.GetType()))
                {
                    var registrar = manager as IModuleRegistrar<TModule>;
                    registrar.RegisterModule(module);
                    break;
                }
            }
        }


        public void Dispose()
        {
            foreach (var manager in _globalManagers.Values)
            {
                manager?.Dispose();
            }
        }
    }
}