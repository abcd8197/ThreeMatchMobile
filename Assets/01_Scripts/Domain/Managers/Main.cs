using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    public class Main : IDisposable
    {
        private static readonly Lazy<Main> _lazy = new(() => new Main());
        public static Main Instance = _lazy.Value;
        private readonly Dictionary<Type, IManager> _globalManagers = new();

        public void Initialize()
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

        public T GetManager<T>() where T : IManager
        {
            var type = typeof(T);
            if (_globalManagers.TryGetValue(type, out var manager))
            {
                return (T)manager;
            }

            throw new Exception($"Manager of type {type} is not registered.");
        }
        public void Dispose()
        {
            foreach (var manager in _globalManagers.Values)
            {
                manager?.Dispose();
            }

            _globalManagers?.Clear();
        }
    }
}