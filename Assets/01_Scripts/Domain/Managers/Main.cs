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

        public void Build()
        {
            foreach (var manager in _globalManagers)
            {
                if (manager.Value is IModule module)
                    RegisterModule(module);
            }

            (_globalManagers[typeof(SaveManager)] as SaveManager).InitializeSaveData();
        }

        public void RegisterManager<T>(T manager) where T : IManager
        {
            var type = typeof(T);
            if (_globalManagers.ContainsKey(type))
            {
                throw new Exception($"Manager of type {type} is already registered.");
            }

            _globalManagers[type] = manager;
        }

        private void RegisterModule(IModule module)
        {
            foreach (var manager in _globalManagers)
            {
                if (manager.Value is not IModuleRegistrar r) continue;
                if (!r.ModuleType.Equals(module.ModuleType)) continue;
                r.Register(module);
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