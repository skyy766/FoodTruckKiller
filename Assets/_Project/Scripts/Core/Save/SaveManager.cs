using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FoodTruckKiller.Core.Singleton;

namespace FoodTruckKiller.Core.Save
{
    /// <summary>
    /// 存档管理器（单例）。聚合所有 ISaveable，统一进行 JSON 存读。
    /// <para>MVP 实现：各 ISaveable 返回/接收 JSON 字符串，SaveManager
    /// 组装为单个存档文件写入 persistentDataPath。</para>
    /// </summary>
    public class SaveManager : SingletonMono<SaveManager>
    {
        private const string SaveFileName = "savegame.json";

        private readonly Dictionary<string, ISaveable> saveables = new Dictionary<string, ISaveable>();

        /// <summary>注册可存档对象（通常在 OnEnable 调用）。</summary>
        public void Register(ISaveable saveable)
        {
            if (saveable == null) return;
            string key = saveable.GetSaveKey();
            if (!saveables.ContainsKey(key))
                saveables.Add(key, saveable);
        }

        /// <summary>注销可存档对象（通常在 OnDisable 调用）。</summary>
        public void Unregister(ISaveable saveable)
        {
            if (saveable == null) return;
            saveables.Remove(saveable.GetSaveKey());
        }

        /// <summary>保存所有已注册对象到存档文件。</summary>
        public void SaveAll()
        {
            var file = new SaveFile();
            foreach (var kvp in saveables)
            {
                string json = kvp.Value.Save() ?? "{}";
                file.entries.Add(new SaveEntry { key = kvp.Key, json = json });
            }

            string raw = JsonUtility.ToJson(file, true);
            File.WriteAllText(GetSavePath(), raw);
            Debug.Log($"[SaveManager] Saved {file.entries.Count} entries -> {GetSavePath()}");
        }

        /// <summary>从存档文件加载并恢复所有已注册对象。</summary>
        public void LoadAll()
        {
            string path = GetSavePath();
            if (!File.Exists(path))
            {
                Debug.Log("[SaveManager] No save file found, skip load.");
                return;
            }

            string raw = File.ReadAllText(path);
            var file = JsonUtility.FromJson<SaveFile>(raw);
            if (file == null || file.entries == null) return;

            int restored = 0;
            foreach (var entry in file.entries)
            {
                if (saveables.TryGetValue(entry.key, out var saveable))
                {
                    saveable.Load(entry.json);
                    restored++;
                }
            }
            Debug.Log($"[SaveManager] Load complete, restored {restored}/{file.entries.Count}.");
        }

        /// <summary>删除存档文件。</summary>
        public void DeleteSave()
        {
            string path = GetSavePath();
            if (File.Exists(path)) File.Delete(path);
        }

        private string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, SaveFileName);
        }

        /// <summary>存档文件结构（可被 JsonUtility 序列化）。</summary>
        [System.Serializable]
        public class SaveFile
        {
            public List<SaveEntry> entries = new List<SaveEntry>();
        }

        /// <summary>单条存档记录：键 + JSON 数据。</summary>
        [System.Serializable]
        public class SaveEntry
        {
            public string key;
            public string json;
        }
    }
}
