#region DoxygenMain
/// @mainpage VinylAssetCreateHelper
/// @section intro 개요 
/// - 라디오해머에서 사용되었던 AssetCreateHelper 기능을 분리 시켰습니다.
/// - 저장하고자 하는 객체 타입을 AssetCreateHelper 로 보내면 asset 파일을 생성합니다.
///
/// @section history 작업내역 
/// - Ver. 1.0 : 2014-09-22
///   - AssetCreateHelper 만 분리 시켰습니다.
///     - 담당자 : 김희철 
///
/// @section files 패키지 파일 
/// - AssetCreateHelper.package
///   - AssetCreateHelper 라이브러리 본체 
///
/// @section summary 요약 
/// - namespace 구성 
///   - 없음 
/// - AssetCreate 클래스 호출 예시 
/// @code
///     [MenuItem ("Assets/Create/Radiohammer/RH_FontHolderObject")]
///     public static void AssetCreateSample ()
///     {
///         AssetCreateHelper.CreateAsset<RH_SO_FontHolder>("FontHolder.asset");
///     }
/// @endcode
///
#endregion
 
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

/// <summary>
/// [Serializable] 객체면 어떤 타입이든 Asset 파일로 만들어주는 Helper 클래스.
/// </summary>
public static class AssetCreateHelper{

    public static string CreateAsset<T>(string assetName) where T : ScriptableObject
    {
        string path = "Assets";

        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
            }
            break;
        }
        
        return CreateAsset<T> (path, assetName);
    }


    public static string CreateAsset<T>(string targetFoler, string assetName) where T : ScriptableObject
    {
        var instance = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(instance, targetFoler + "/" + assetName);
        return targetFoler;
    }
}
