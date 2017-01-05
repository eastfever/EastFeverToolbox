using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

// 2017.01.05 by east_fever
// tk2d와 연동되어야 하는 스파인 애셋을 유니티에서 쓸 수 있는 형태로 데이터를 가공할 때,
// 필요한 설정 값을 확인/수정 할 수 있게 하는 툴.
//public class tk2dSpineImporterSetting : EditorWindow
//{
//    private static tk2dSpineImporterSetting s_instance = null;

//    [MenuItem( "Tools/tk2d스파인 애셋 가져오기 설정" )]
//    static public void ShowEditor()
//    {
//        tk2dSpineImporterSetting s_instance = EditorWindow.CreateInstance<tk2dSpineImporterSetting>();
//        s_instance.titleContent = new GUIContent( "tk2dSpineSetting" );
//        s_instance.position = new Rect( 50, 50, 400, 400 );
//        s_instance.Show();
//    }

//    private int _newGameObjectLayerNumber = 0;
//    private Vector2 _scaleOnImport = Vector2.one;
//    private AnimatorController _baseAnimationController = null;

//    private string _spriteCollectionName = "{Folder}_SpriteCollection";
//    private string _spineAnimatorControllerName = "{Folder}_SpineAniController";
//    private string _overrideAniControllerName = "{Folder}_OverrideAniController";
//    private string _spinePrefabObjectName = "{Folder}_GameObject";

//    void OnEnable()
//    {
//        // 설정창 위치 설정.
//        float posX = EditorPrefs.GetFloat( "tk2dSpineImporter_PosX" );
//        float posY = EditorPrefs.GetFloat( "tk2dSpineImporter_PosY" );
//        if( posX < 1.0f ) posX = 10.0f;
//        if( posY < 1.0f ) posY = 10.0f;

//        // 배율 설정 불러오기.
//        if( EditorPrefs.HasKey( "tk2dSpineImporter_Scale" ) )
//        {
//            _scaleOnImport = StringHelper.Vector2FromString( 
//                EditorPrefs.GetString( "tk2dSpineImporter_Scale" ) );
//        }

//        // 기준 애니메이션 컨트롤러 불러오기.
//        if( EditorPrefs.HasKey( "tk2dSpineImporter_BaseAniCon" ) )
//        {
//            string baseAniConPath = AssetDatabase.GUIDToAssetPath(
//                EditorPrefs.GetString( "tk2dSpineImporter_BaseAniCon" ) );
            
//            if( baseAniConPath.IsNullOrEmpty() )
//            {
//                EditorPrefs.DeleteKey( "tk2dSpineImporter_BaseAniCon" );
//            }
//            else
//            {
//                _baseAnimationController = AssetDatabase.LoadAssetAtPath<AnimatorController>( baseAniConPath );
//            }
//        }
//    }

//    void OnGUI()
//    {

//    }   
    
//    void OnDestroy()
//    {

//    } 
//}
