using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class BuildEditor
{
    [MenuItem("Tools/Build Resource #%&t")]
    public static void BuildResource()
    {
        var buildRootDir = GetAASRemoteBuildDir();
        //删除原有文件夹
        GlobalFunc.DeleteFolder(buildRootDir);
        //执行打包逻辑
        AddressableAssetSettings.BuildPlayerContent(out var result);
        BuildResource_CacheBundleList(result);
        BuildResource_CopyBundleDirToAAS();
        Debug.Log($"build resource finish");
    }

    /// <summary>
    /// 获取RemoteBuild文件夹路径
    /// </summary>
    /// <returns></returns>
    public static string GetAASRemoteBuildDir()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var profileSettings = settings.profileSettings;
        var propName =
            profileSettings.GetValueByName(settings.activeProfileId, AddressableAssetSettings.kRemoteBuildPath);
        var remoteBuildDir = profileSettings.EvaluateString(settings.activeProfileId, propName);
        return remoteBuildDir;
    }

    /// <summary>
    /// 生成BundleCache文件列表
    /// </summary>
    /// <param name="result"></param>
    static void BuildResource_CacheBundleList(AddressablesPlayerBuildResult result)
    {
        var buildRootDir = GetAASRemoteBuildDir();
        var buildRootDirLen = buildRootDir.Length;
        List<string> allBundles = new List<string>();

        var filePathList = result.FileRegistry.GetFilePaths().Where((s => s.EndsWith(".bundle")));
        foreach (var filePath in filePathList)
        {
            var bundlePath = filePath.Substring(buildRootDirLen + 1);
            allBundles.Add(bundlePath);
        }

        var json = JsonConvert.SerializeObject(allBundles);
        File.WriteAllText($"{buildRootDir}/{GlobalFunc.Caching}.json", json);
    }

    /// <summary>
    /// 拷贝Bundle到AAS，并随AAS进包
    /// </summary>
    static void BuildResource_CopyBundleDirToAAS()
    {
        var remoteBuildDir = GetAASRemoteBuildDir();
        var aaDestDir = $"{Addressables.BuildPath}/{GlobalFunc.Caching}";

        GlobalFunc.DeleteFolder(aaDestDir);
        Directory.CreateDirectory(aaDestDir);
        //拷贝bundle到aa文件夹
        var allSrcFile = Directory.EnumerateFiles(remoteBuildDir, "*.*", SearchOption.AllDirectories);
        foreach (var srcFile in allSrcFile)
        {
            var fileName = Path.GetFileName(srcFile);
            var destFile = $"{aaDestDir}/{fileName}";
            File.Copy(srcFile, destFile);
        }

        Debug.Log($"Bundle拷贝完毕:{remoteBuildDir}==>{aaDestDir}");
    }
}