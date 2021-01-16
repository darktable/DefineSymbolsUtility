using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DefineSymbolUtilities
{
    [System.Serializable]
    public class DefineSymbols
    {
        private static string uid => $"{nameof(DefineSymbols)}.{Application.companyName}.{Application.productName}";

        public List<string> add = new List<string>();
        public List<string> remove = new List<string>();
        private readonly HashSet<string> removeOnce = new HashSet<string>();

        public void SetSymbol(string symbol, bool set = true)
        {
            symbol = ClearSymbol(symbol);

            if (set)
            {
                add.Add(symbol);
            }
            else
            {
                removeOnce.Add(symbol);
            }
        }

        public void UnsetSymbol(string symbol)
        {
            SetSymbol(symbol, false);
        }

        public void RemoveSymbol(string symbol)
        {
            symbol = ClearSymbol(symbol);

            remove.Add(symbol);
        }

        private string ClearSymbol(string symbol)
        {
            symbol = symbol.Trim();

            bool RemovePredicate(string x)
            {
                return x.Equals(symbol);
            }

            add.RemoveAll(RemovePredicate);
            remove.RemoveAll(RemovePredicate);

            return symbol;
        }

        public void RemoveDuplicateSymbols()
        {
            var addHashSet = new HashSet<string>(add);
            var removeHashSet = new HashSet<string>(remove);

            add = new List<string>(addHashSet);
            remove = new List<string>(removeHashSet);
        }

        public void ApplySymbolsToProject()
        {
            RemoveDuplicateSymbols();

            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;

            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);

            var defined = new HashSet<string>(defineSymbols.Split(';'));

            foreach (var symbol in add)
            {
                defined.Add(symbol);
            }

            removeOnce.UnionWith(remove);
            foreach (var symbol in removeOnce)
            {
                defined.Remove(symbol);
            }
            removeOnce.Clear();

            var updatedDefineSymbols = string.Join(";", defined);

            if (defineSymbols != updatedDefineSymbols)
            {
                Debug.Log($"setting defines: {updatedDefineSymbols}");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, updatedDefineSymbols);
            }
        }

        public void SaveToEditorPrefs()
        {
            EditorPrefs.SetString(uid, ToString());
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }

        public static DefineSymbols LoadFromEditorPrefs()
        {
            var keys = EditorPrefs.GetString(uid, null);

            DefineSymbols list;

            if (string.IsNullOrEmpty(keys))
            {
                list = new DefineSymbols();
            }
            else
            {
                list = JsonUtility.FromJson<DefineSymbols>(keys);
            }

            return list;
        }
    }
}
