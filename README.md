# BIG
Library used for translating spredsheet documents into runtime structure.
Base purpose is to provide a way to translate games based on spreadsheets.

## How to use it for translation [WIP]

You need to define a class that will represent a collection:

```csharp
    public abstract class ScriptableCollection<T> : ScriptableObject, IInitializableCollection, IEnumerable<T>
    {
        private const string BALANCE_PATH = "Assets/Resources/Balance/";
        [SerializeField] public T[] Entities;
        public void Initialize(KeyValuePair<Type, IList> data)
        {
            Entities = new T[data.Value.Count];
            for (int i = 0; i < data.Value.Count; i++)
            {
                Entities[i] = (T)data.Value[i];
            }
        }
        public string OutputPath() => BALANCE_PATH + ConcretePath;

        /// <summary>
        /// Final path that be used after Assets/Resources/Balance/{ConcretePath}.
        /// It should include .asset extension at the end.
        /// </summary>
        protected abstract string ConcretePath { get; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T entity in Entities)
            {
                yield return entity;
            }
        }
    }
```

Than you need a class that will implement it:

```csharp
public class ScriptableLanguageDictionary : ScriptableCollection<DictionaryRecord>
{
    protected override string ConcretePath => "LanguageDictionary";

    public Dictionary<string, string> GetDictionary(SpookedLanguage language)
    {
        try
        {
            Dictionary<string, string> dict = new Dictionary<string, string>(Entities.Length);
            switch (language)
            {
                case SpookedLanguage.Polish:
                    for (int i = 0; i < Entities.Length; i++)
                        dict.Add(Entities[i].Tag, Entities[i].Polish);
                    return dict;
                default:
                    for (int i = 0; i < Entities.Length; i++)
                        dict.Add(Entities[i].Tag, Entities[i].English);
                    return dict;
            }
        }
        catch (Exception e)
        {
            this.Log("Exception occur during creating dictionary: " + e, LogLevel.Error);
            return new Dictionary<string, string>(0);
        }
    }
}
```

Than you need to define a class that will represents keys inside your spredsheet. This class will tell to our next class [ExcelAssetsPostprocessor] that ScriptableLanguageDictionary should 
be created. 
To create it it need to look into path: Assets/Editor/EditorResources/Dictionary.xlsx
Sheet name inside the spreadsheet found in this path is "Dictionary"

```csharp
[SerializeField]
[Serializable]
[ClassMapping(true, typeof(ScriptableLanguageDictionary), "Assets/Editor/EditorResources/Dictionary.xlsx", "Dictionary")]
public struct DictionaryRecord
{
    public string Tag;
    public string English;
    public string Polish;
}
```


Final step is to put this script in Editor.Workbook directory inside your Unity project.
This class will trigger every time you add/delete/move file in your project.
It check for all newly imported excel files.

Than it looks for all the classes that have ClassMappingAttribute and create ScriptableObjects for them.


```csharp
namespace Editor.Workbook
{
    public class ExcelAssetsPostprocessor : AssetPostprocessor
    {
        private static async void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
                return;

            importedAssets = importedAssets.Where(ExcelFileLoader.IsExcelFile).ToArray();
            if (importedAssets.Length == 0)
            {
                return;
            }

            UnityEngine.Debug.Log("Postprocessing excel files...");
            try
            {
                WorkbookClassMapperResult result = await new WorkbookClassMapper().MapTypes(importedAssets).ConfigureAwait(true);
                foreach (KeyValuePair<Type, IList> keyValuePair in result)
                {
                    CreateAsset(keyValuePair);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Excel balance files export failed: " + e);
            }
            
            AssetDatabase.SaveAssets();
        }

        private static void CreateAsset(KeyValuePair<Type, IList> output)
        {
            ClassMappingAttribute attr = output.Key.GetCustomAttribute<ClassMappingAttribute>(true);
            var asset = ScriptableObject.CreateInstance(attr.ScriptableType);
            IInitializableCollection collection = (IInitializableCollection)asset;
            if (collection == null) throw new Exception("ClassMappingAttribute scriptable type must be a ScriptableCollection implementation.");
            AssetDatabase.DeleteAsset(collection.OutputPath());

            collection.Initialize(output);

            AssetDatabase.CreateAsset(asset, collection.OutputPath() + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("<color=blue> [BIG] </color> Created scriptable collection: " + collection.OutputPath());
        }
    }
}
```
