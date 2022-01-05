using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class GameMain : MonoBehaviour
{
    public RawImage Raw;
    private List<string> _bundleCacheList = new List<string>();

    public string WWWProtoHead
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return "";
#else
            return "file://";
#endif
        }
    }


    // Start is called before the first frame update
    IEnumerator Start()
    {
        Debug.Log(Application.persistentDataPath);
        yield return Addressables.InitializeAsync();
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;
        if (checkHandle.Result.Count > 0)
        {
            yield return Addressables.UpdateCatalogs(checkHandle.Result);
        }

        Addressables.Release(checkHandle);

        //从StreamingAsset中获取bundle列表文件
        var bundleCacheFileURL = $"{Addressables.RuntimePath}/{GlobalFunc.Caching}/{GlobalFunc.Caching}.json";
        
        var url = bundleCacheFileURL;
#if UNITY_EDITOR
        //windows和mac平台上需要全路径，移动平台上是虚拟路径
        url = Path.GetFullPath(url);
#endif
        url = $"{WWWProtoHead}{url}";
        
        Debug.Log($"cache url :{url}");
        var request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        if (!string.IsNullOrEmpty(request.error))
        {
            Debug.LogError(request.error);
        }

        Debug.Log($"BundleCache:{request.downloadHandler.text}");
        _bundleCacheList = JsonConvert.DeserializeObject<List<string>>(request.downloadHandler.text);
        Addressables.InternalIdTransformFunc = Addressables_InternalIdTransformFunc;

        //开始业务逻辑
        StartLogic();
    }

    string Addressables_InternalIdTransformFunc(IResourceLocation location)
    {
        if (location.Data is AssetBundleRequestOptions)
        {
            if (_bundleCacheList.Contains(location.PrimaryKey))
            {
                var fileName = Path.GetFileName(location.PrimaryKey);
                Debug.LogError($"StreamingAssetCache:{location.PrimaryKey}");
                return $"{Addressables.RuntimePath}/{GlobalFunc.Caching}/{fileName}";
            }
        }

        return location.InternalId;
    }

    void StartLogic()
    {
        var h = Addressables.LoadAssetAsync<Texture>("pic/1.jpg");
        h.Completed += handle => { Raw.texture = h.Result; };
    }
}