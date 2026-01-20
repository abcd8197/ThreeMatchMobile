using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    public class SaveManager : IModuleRegistrar<ISaveModule>
    {
        private readonly SaveData _saveData;
        private readonly Dictionary<Type, ISaveModule> _registeredModules = new();
        private readonly ISaveService _saveService;

        public SaveManager(SaveData saveData, ISaveService saveService)
        {
            _saveData = saveData ?? new SaveData();
            _saveService = saveService;
        }

        public void Register(ISaveModule module)
        {
            var type = module.GetType();
            if (_registeredModules.ContainsKey(type))
            {
                return;
            }
            _registeredModules[type] = module;
        }

        public void InitializeSaveData()
        {
            foreach (var module in _registeredModules.Values)
            {
                module.InitializeSaveData(_saveData);
            }
        }

        public void SaveData()
        {
            foreach (var module in _registeredModules.Values)
            {
                module.SaveData();
            }

            _saveService?.SaveData(_saveData);
        }

        public void Dispose()
        {
            foreach (var module in _registeredModules.Values)
            {
                module?.Dispose();
            }

            _registeredModules?.Clear();
        }
    }
}
