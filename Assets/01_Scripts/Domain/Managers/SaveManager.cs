using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch
{
    public class SaveManager : IModuleRegistrar<ISaveModule>
    {
        private readonly Dictionary<Type, ISaveModule> _registeredModules = new();

        public void RegisterModule(ISaveModule module)
        {
            var type = module.GetType();
            if (_registeredModules.ContainsKey(type))
            {
                Debug.LogWarning($"Save module of type {type} is already registered.");
                return;
            }
            _registeredModules[type] = module;
        }

        public void Dispose()
        {
            foreach(var module in _registeredModules.Values)
            {
                module?.Dispose();
            }
        }
    }
}
