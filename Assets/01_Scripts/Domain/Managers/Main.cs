using System;
using System.Collections.Generic;
using System.Reflection;

namespace ThreeMatch
{
    public class Main
    {
        public static Main Instance = _lazy.Value;
        private static readonly Lazy<Main> _lazy = new Lazy<Main>(() => new Main());
        private readonly Dictionary<Type, IManager> _globalManagers = new();

        internal Main()
        {

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
        }

    }
}