using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace ThreeMatch
{
    public class ItemManager : IManager, ISaveModule
    {
        public Type ModuleType => typeof(ISaveModule);
        private ItemSaveData _saveData;

        private readonly CompositeDisposable _disposable = new();
        private readonly Dictionary<ItemType, ReactiveProperty<int>> _itemProperties = new();

        public ItemManager()
        {
            foreach(var item in _itemProperties.Values)
            {
                _disposable.AddTo(_disposable);
            }
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        public void InitializeSaveData(SaveData saveData)
        {
            saveData.ItemSaveData ??= new();

            var enumValues = Enum.GetValues(typeof(ItemType)).Cast<ItemType>().ToList();

            if (saveData.ItemSaveData.Items.Count == 0)
            {
                foreach (var enumValue in enumValues)
                    saveData.ItemSaveData.Items.Add(enumValue, 0);
            }

            foreach (var enumValue in enumValues)
                _itemProperties[enumValue] = new ReactiveProperty<int>(saveData.ItemSaveData.Items[enumValue]);

            _saveData = saveData.ItemSaveData;
        }

        public void SaveData()
        {

        }

        public void AddItem(ItemType itemType, int count)
        {
            if (_saveData.Items.ContainsKey(itemType))
            {
                _saveData.Items[itemType] += count;
                _itemProperties[itemType].Value += count;
            }

            Main.Instance.GetManager<SaveManager>().SaveData();
        }

        public void UseItem(ItemType itemType, int count)
        {
            if (_saveData.Items.ContainsKey(itemType))
            {
                _saveData.Items[itemType] -= count;

                if (_saveData.Items[itemType] < 0)
                    throw new ItemLessThanZeroExceoption($"{itemType} muse never be less than zero");
                _itemProperties[itemType].Value = _saveData.Items[itemType];
            }

            Main.Instance.GetManager<SaveManager>().SaveData();
        }

        public int GetItemCount(ItemType itemType)
        {
            if (_saveData.Items.ContainsKey(itemType))
                return _saveData.Items[itemType];

            return 0;
        }

        public IDisposable Bind(ItemType type, Action<int> onChanged)
        {
            if (onChanged == null)
                return null;

            IObservable<int> stream = _itemProperties[type];
            return stream.DistinctUntilChanged().Subscribe(onChanged);
        }
    }

    public class ItemLessThanZeroExceoption : Exception
    {
        public ItemLessThanZeroExceoption(string message) : base(message) { }
    }
}
