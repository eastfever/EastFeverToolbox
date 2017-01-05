using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace EastFever
{
    // 애셋 간의 의존 관계 확인을 돕는 툴.
    public class DependencyCheckEditor : EditorWindow
    {
        // private static DependencyCheckEditor s_instance = null;

        [MenuItem( "EAST_FEVER/의존 관계 확인" )]
        static public void ShowEditor()
        {
            DependencyCheckEditor s_instance = EditorWindow.CreateInstance<DependencyCheckEditor>();
            s_instance.titleContent = new GUIContent( "Dependency Checker" );
            s_instance.position = new Rect( 50, 50, 695, 695 );
            s_instance.Show();
        }

        private UnityEngine.Object _targetAsset = null;
        private string _targetFolderPath = "Assets";
        private Vector2 _scrollPivot = Vector2.zero;

        private List<string> _searchedPaths = new List<string>();
        private List<UnityEngine.Object> _searchedObjects = new List<UnityEngine.Object>();

        private enum eSearchTarget
        {
            ProjectFolder,
            CurrentScene,
        }
        private eSearchTarget _searchTarget = eSearchTarget.ProjectFolder;

        // UI 구현하기.
        void OnGUI()
        {
            GUILayout.Space( 10f );

            _targetAsset = EditorGUIHelper.ObjectField<UnityEngine.Object>( "검사할 애셋", 100f, _targetAsset, false, 10f );
            GUILayout.Space( 5f );

            _searchTarget = ( eSearchTarget )EditorGUIHelper.EnumField( "검색 대상", 100f, _searchTarget, 10f );

            if( eSearchTarget.ProjectFolder == _searchTarget )
            {
                _targetFolderPath = EditorGUIHelper.FolderField( "검색 폴더", 100f, _targetFolderPath, 10f );
            }
            GUILayout.Space( 10f );

            if( GUILayout.Button( "검색 시작", GUILayout.Height( 30f ) ) )
            {
                switch( _searchTarget )
                {
                    case eSearchTarget.CurrentScene:
                        CheckDependenciesInCurrentScene();
                        break;
                    case eSearchTarget.ProjectFolder:
                        CheckDependenciesInProjectFolder();
                        break;
                }
                return;
            }

            GUILayout.Space( 10f );

            EditorGUIHelper.Label( ">> 검색 결과( 검색된 경로를 클릭하면 해당 경로의 애셋을 선택 합니다 )", 500f, 10f );
            _scrollPivot =
                GUILayout.BeginScrollView(
                    _scrollPivot,
                    GUILayout.Width( 695f ),
                    GUILayout.Height( 540f ) );

            int searchedCount = 0;
            switch( _searchTarget )
            {
                case eSearchTarget.ProjectFolder:
                    searchedCount = _searchedPaths.Count;
                    DrawSearchResultsInProjectFolder();
                    break;
                case eSearchTarget.CurrentScene:
                    searchedCount = _searchedObjects.Count;
                    DrawSearchResultInCurrentScene();
                    break;
            }

            GUILayout.EndScrollView();

            GUILayout.Space( 10f );
            if( searchedCount > 0 )
            {
                string bottomTip = string.Format( "검색 결과, 총 {0}건의 의존 애셋을 발견 했습니다", searchedCount );
                EditorGUIHelper.Label( bottomTip, 400f, 10f );
            }
            else
            {
                EditorGUIHelper.Label( "검색 결과가 없습니다", 400f, 10f );
            }
        }

        void DrawSearchResultInCurrentScene()
        {
            foreach( GameObject selectedObject in _searchedObjects )
            {
                GUILayout.BeginHorizontal();
                if( GUILayout.Button( selectedObject.GetHierarchyPath() ) )
                {
                    Selection.activeObject = selectedObject;
                }
                GUILayout.EndHorizontal();
            }
        }

        void DrawSearchResultsInProjectFolder()
        {
            foreach( string path in _searchedPaths )
            {
                GUILayout.BeginHorizontal();
                if( GUILayout.Button( path ) )
                {
                    UnityEngine.Object objectToSelect = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( path );
                    Selection.activeObject = objectToSelect;
                }
                GUILayout.EndHorizontal();
            }
        }

        // 현재 씬 안의 있는 오브젝트들을 대상으로 검사.
        void CheckDependenciesInCurrentScene()
        {
            if( IsTargetAssetFieldEmpty() )
            {
                return;
            }

            _searchedObjects.Clear();

            EditorUtility.DisplayProgressBar( "확인중", "오브젝트 수집 중", 0f );

            UnityEngine.SceneManagement.Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            GameObject[] objectsInScenes = currentScene.GetAllObjectsInScene();

            int objectCount = objectsInScenes.Length;
            for( int i = 0; i < objectCount; i++ )
            {
                EditorUtility.DisplayProgressBar( "확인중", "의존 관계 확인 중", ( float )i / ( float )objectCount );

                // 자기 자신은 검색 대상에서 제외한다.
                if( objectsInScenes[ i ] == _targetAsset )
                {
                    continue;
                }

                if( IsDetectedDependency( objectsInScenes[ i ] ) )
                {
                    _searchedObjects.Add( objectsInScenes[ i ] );
                }
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog( "", "확인이 끝났습니다", "OK" );
        }

        // 주어진 폴더 안에 있는 애셋들을 대상으로 검사.
        void CheckDependenciesInProjectFolder()
        {
            if( IsTargetAssetFieldEmpty() )
            {
                return;
            }

            EditorUtility.DisplayProgressBar( "확인중", "경로 수집 중", 0f );
            List<string> paths = PathHelper.GetPathListByFileExtension( _targetFolderPath, null );

            _searchedPaths.Clear();
            for( int i = 0; i < paths.Count; i++ )
            {
                EditorUtility.DisplayProgressBar( "확인중", "의존 관계 확인 중", ( float )i / ( float )paths.Count );

                string path = paths[ i ];
                UnityEngine.Object assetAtPath = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( path );

                // 자기 자신은 검색 대상에서 제외한다.
                if( assetAtPath == _targetAsset )
                {
                    continue;
                }

                if( IsDetectedDependency( assetAtPath ) )
                {
                    _searchedPaths.Add( path );
                }
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog( "", "확인이 끝났습니다", "OK" );
        }

        // 검사할 애셋 필드가 비어 있는지 확인.
        bool IsTargetAssetFieldEmpty()
        {
            if( null == _targetAsset )
            {
                EditorUtility.DisplayDialog( "", "검사할 애셋을 먼저 선택해 주세요", "OK" );
                return true;
            }
            return false;
        }

        // 주어진 오브젝트와 연결된 애셋들 중에 검사 대상 애셋이 포함되어 있는지 검사.
        bool IsDetectedDependency( UnityEngine.Object targetObject )
        {
            UnityEngine.Object[] rootAsset = { targetObject };
            UnityEngine.Object[] dependencies = EditorUtility.CollectDependencies( rootAsset );

            foreach( UnityEngine.Object dependencyObject in dependencies )
            {
                if( dependencyObject == _targetAsset )
                {
                    Debug.Log( dependencyObject.name );
                    return true;
                }
            }
            return false;
        }
    }
}