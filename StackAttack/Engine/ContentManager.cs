using StackAttack.Engine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine
{
    internal static class ContentManager
    {
        private static Dictionary<Type, Dictionary<string, object>> dictionaries = new();

        public static T? Load<T>(string id, string path)
        {
            if (typeof(T).IsAssignableFrom(typeof(ILoadable<T>)))
            {
                Logger.Log(Logger.Levels.Error, $"{typeof(T).Name}_{id} is Not Loadable.");
                return default; // hm? ten warning :D "Possible null reference return"... JA CHCEM NULL :D.. aj tak s dá :D
            }

            if (Activator.CreateInstance<T>() is not ILoadable<T> obj)
            {
                Logger.Log(Logger.Levels.Error, $"{typeof(T).Name}_{id} - Type needs constructor with no arguments.");
                return default;
            }

            if (!dictionaries.ContainsKey(typeof(T)))
            {
                dictionaries.Add(typeof(T), new Dictionary<string, object>());
            }

            if (dictionaries[typeof(T)].ContainsKey(id))
            {
                Logger.Log(Logger.Levels.Error, $"{typeof(T).Name}_{id} is already loaded.");
                return default;
            }
            T? newObject = obj.Load(path);
            if (newObject is null)
            {
                Logger.Log(Logger.Levels.Warn, $"{typeof(T).Name}_{id} failed to load.");
                return default;
            }
            dictionaries[typeof(T)].Add(id, newObject);
            return newObject;
        }

        public static void Remove<T>(string id)
        {
            if (!dictionaries.ContainsKey(typeof(T)) || !dictionaries[typeof(T)].ContainsKey(id))
            {
                Logger.Log(Logger.Levels.Error, $"{typeof(T).Name}_{id} is not loaded.");
                return;
            }
            if (typeof(T).IsAssignableFrom(typeof(IDisposable)))
            {
                ((IDisposable)dictionaries[typeof(T)][id]).Dispose();
            }
            dictionaries[typeof(T)].Remove(id);
        }

        public static void RemoveAll()
        {
            for (int typeI = dictionaries.Keys.Count-1; typeI >= 0; typeI--)
            {
                Type type = dictionaries.Keys.ElementAt(typeI);
                for (int keyI = dictionaries[type].Count -1; keyI >= 0; keyI--)
                {
                    string key = dictionaries[type].Keys.ElementAt(keyI);
                    if (type.IsAssignableFrom(typeof(IDisposable)))
                    {
                        ((IDisposable)dictionaries[type][key]).Dispose();
                        dictionaries[type].Remove(key);
                    }
                    dictionaries.Remove(type);
                }
            }
        }

        public static void Update<T>(string id, T content)
        {
            if (!dictionaries.ContainsKey(typeof(T)) || !dictionaries[typeof(T)].ContainsKey(id))
            {
                Logger.Log(Logger.Levels.Error, $"{typeof(T).Name}_{id} is not loaded.");
                return;
            }
            if (typeof(T).IsAssignableFrom(typeof(IDisposable)))
            {
                ((IDisposable)dictionaries[typeof(T)][id]).Dispose();
            }
            if (content is null)
            {
                Logger.Log(Logger.Levels.Error, $"{typeof(T).Name}_{id} is null..");
                return;
            }
            dictionaries[typeof(T)][id] = content;
        }

        public static void Add<T>(string id, T content)
        {
            if (!dictionaries.ContainsKey(typeof(T)))
            {
                dictionaries.Add(typeof(T), new Dictionary<string, object>());
            }
            if (dictionaries[typeof(T)].ContainsKey(id))
            {
                Logger.Log(Logger.Levels.Error, $"{typeof(T).Name}_{id} is already loaded.");
                return;
            }
            if (content is null)
            {
                Logger.Log(Logger.Levels.Error, $"{typeof(T).Name}_{id} is null..");
                return;
            }
            dictionaries[typeof(T)].Add(id, content);
        }

        public static List<string> GetKeys<T>()
        {
            List<string> keys = new();

            if (dictionaries.ContainsKey(typeof(T)))
            {
                foreach (string key in dictionaries[typeof(T)].Keys)
                {
                    keys.Add(key);
                }
            }

            return keys;
        }

        public static bool ContainsKey<T>(string key)
        {
            List<string> keys = new();

            return dictionaries.ContainsKey(typeof(T)) && dictionaries[typeof(T)].ContainsKey(key);
        }

        public static T Get<T>(string id)
        {
            if (!dictionaries.ContainsKey(typeof(T)) || !dictionaries[typeof(T)].ContainsKey(id))
            {
                T? empty = Activator.CreateInstance<T>();
                if (typeof(T) == typeof(Texture))
                {
                    Logger.Log(Logger.Levels.Warn, $"{typeof(T).Name}_{id} is not loaded.");
                    if (!dictionaries.ContainsKey(typeof(T)) || !dictionaries[typeof(T)].ContainsKey("Error"))
                    {
                        Logger.Log(Logger.Levels.Error, $"{typeof(T).Name}_Empty is not loaded.");
                        if (empty is null)
                        {
                            Logger.Log(Logger.Levels.Fatal, $"{typeof(T).Name}_{id} - cannot default.");
                            throw new Exception("This will never happen.");
                        }
                        return empty;
                    }
                    return (T)dictionaries[typeof(T)]["Error"];
                }
                Logger.Log(Logger.Levels.Error, $"{typeof(T).Name}_{id} is not loaded.");
                if (empty is null)
                {
                    Logger.Log(Logger.Levels.Fatal, $"{typeof(T).Name}_{id} - cannot default.");
                    throw new Exception("This will never happen.");
                }
                return empty;
            }
            return (T)dictionaries[typeof(T)][id];
        }
    }
}
