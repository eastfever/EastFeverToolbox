using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace EastFever
{
    // 등록된 디파인 중에 어떤 디파인을 활성화할지 설정하고,
    // 설정이 끝난 후엔 유니티 PlayerSetting에 설정한 내용을 적용한다.
    public class DefineEditorWindow : EditorWindow
    {
        private static readonly string s_defineStringsPath = "Assets/Editor/DefineEditor/DefineStrings.cs";

        private static DefineEditorWindow s_instance = null;

        private bool _reservedCloseWindow = false;

        private Vector2 _scrollViewPivot = Vector2.zero;

        // 등록된 디파인과 그 주석
        private Dictionary<string, string> _defineCommentTable = new Dictionary<string, string>();

        // 등록된 디파인들과 활성화 여부( true/false )
        private Dictionary<string, bool> _defineToggleTable = new Dictionary<string, bool>();

        // 디파인이 적용될 플랫폼
        private BuildTargetGroup _selectedBuildTarget = BuildTargetGroup.Android;

        [MenuItem( "EAST_FEVER/디파인 관리 창 열기" )]
        static public void ShowEditor()
        {
            if( s_instance != null )
            {
                s_instance.ShowUtility();
                return;
            }

            float posX = EditorPrefs.GetFloat( "DefineEditorWindow_PositionX" );
            float posY = EditorPrefs.GetFloat( "DefineEditorWindow_PositionY" );
            if( posX < 10.0f ) posX = 10.0f;
            if( posY < 10.0f ) posY = 10.0f;

            s_instance = EditorWindow.GetWindow<DefineEditorWindow>( false, "#define 관리" );
            Rect windowRect = new Rect( posX, posY, 500, 500 );
            s_instance.position = windowRect;
            s_instance.ShowUtility();
        }

        void OnEnable()
        {
            // DefineStrings.cs에 등록된 디파인과 주석들을 가져온다.
            TextAsset defineStrings = AssetDatabase.LoadAssetAtPath( s_defineStringsPath, typeof( TextAsset ) ) as TextAsset;
            if( null == defineStrings )
            {
                return;
            }

            // static string[] DEFINE_STRINGS = new string[] { ~ } 사이의 문자열 추출해서,
            // 디파인과 그 주석을 가져 온다.
            string prefixOfDeclare = "static string[] DEFINE_STRINGS = new string[]";
            int startIndex = defineStrings.text.IndexOf( prefixOfDeclare ) + prefixOfDeclare.Length;
            int lastIndex = defineStrings.text.IndexOf( '}', startIndex ) - 1;
            string defineBodyString =
                defineStrings.text.Substring(
                    startIndex,
                    lastIndex - startIndex );

            defineBodyString = defineBodyString.Trim();
            defineBodyString = defineBodyString.Replace( "{", "" );

            string[] defineTokens =
                defineBodyString.Split(
                    new string[] { "\n" },
                    System.StringSplitOptions.RemoveEmptyEntries );

            foreach( string token in defineTokens )
            {
                if( !token.Contains( "," ) )
                {
                    continue;
                }
                string[] defineAndComment =
                    token.Split(
                        new string[] { "//" },
                        System.StringSplitOptions.RemoveEmptyEntries );

                string defineString = "";
                string commentString = "";
                if( defineAndComment.Length == 2 )
                {
                    defineString = defineAndComment[ 0 ].Replace( ",", "" );
                    defineString = defineString.Replace( "\"", "" );
                    defineString = defineString.Trim();
                    commentString = defineAndComment[ 1 ].Replace( "//", "" );
                    commentString = commentString.Trim();
                }

                if( defineString.Length > 0 && commentString.Length > 0 )
                {
                    _defineCommentTable.Add( defineString, commentString );
                }
                else if( defineString.Length > 0 )
                {
                    _defineCommentTable.Add( defineString, "" );
                }
            }

            // 디파인 활성화 상태 갱신
            this.RefreshInfoesByChangePlatform( _selectedBuildTarget );
        }

        void RefreshInfoesByChangePlatform( BuildTargetGroup buildTarget )
        {
            _defineToggleTable.Clear();
            foreach( string define in _defineCommentTable.Keys )
            {
                _defineToggleTable.Add( define, false );
            }

            string defineStrings = PlayerSettings.GetScriptingDefineSymbolsForGroup( buildTarget );
            if( null == defineStrings || defineStrings.Length == 0 )
            {
                return;
            }

            string[] enableDefines =
                defineStrings.Split(
                    new string[] { ";" },
                    System.StringSplitOptions.RemoveEmptyEntries );

            foreach( string enableDefine in enableDefines )
            {
                if( _defineToggleTable.ContainsKey( enableDefine ) )
                {
                    _defineToggleTable[ enableDefine ] = true;
                }
                else
                {
                    Debug.LogWarning( "[DefineEditorWindow]not registered define - " + enableDefine );
                }
            }
        }

        void OnGUI()
        {
            if( Event.current.keyCode == KeyCode.Escape )
            {
                _reservedCloseWindow = true;
            }

            GUILayout.Space( 10.0f );

            if( _defineToggleTable.Count == 0 )
            {
                GUILayout.Label( "등록된 디파인이 없습니다. 아래 경로의 스크립트에 디파인을 등록해 주세요" );
                GUILayout.Label( s_defineStringsPath );
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label( "적용 플랫폼 : " );
            BuildTargetGroup newBuildTarget = ( BuildTargetGroup )EditorGUILayout.EnumPopup( _selectedBuildTarget );
            if( newBuildTarget != _selectedBuildTarget )
            {
                _selectedBuildTarget = newBuildTarget;
                this.RefreshInfoesByChangePlatform( _selectedBuildTarget );

            }
            GUILayout.EndHorizontal();

            GUILayout.Space( 10.0f );

            _scrollViewPivot = GUILayout.BeginScrollView( _scrollViewPivot, GUILayout.Width( 500.0f ), GUILayout.Height( 410.0f ) );

            string toggledDefine = "";
            foreach( KeyValuePair<string, bool> pair in _defineToggleTable )
            {
                GUILayout.BeginHorizontal();

                // 체크 박스
                GUILayout.Space( 10.0f );
                bool toggleState = EditorGUILayout.Toggle( pair.Value, GUILayout.Width( 20.0f ) );
                if( toggleState != pair.Value )
                {
                    toggledDefine = pair.Key;
                }

                // 디파인
                GUILayout.Label( pair.Key, GUILayout.Width( 200.0f ) );

                // 주석
                if( _defineCommentTable.ContainsKey( pair.Key ) )
                {
                    GUILayout.Label( _defineCommentTable[ pair.Key ] );
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            if( toggledDefine.Length > 0 )
            {
                _defineToggleTable[ toggledDefine ] = !_defineToggleTable[ toggledDefine ];
            }

            GUILayout.Space( 20.0f );

            if( GUILayout.Button( "적용( 엔진 하단의 모래시계가 없을 때 눌러 주세요 )" ) )
            {
                this.SaveAndApplyDefineSetting();
            }
        }

        void SaveAndApplyDefineSetting()
        {
            string oldDefineString = PlayerSettings.GetScriptingDefineSymbolsForGroup( _selectedBuildTarget );
            string[] oldDefines = oldDefineString.Split( ';' );
            HashSet<string> defineSet = new HashSet<string>( oldDefines );

            foreach( KeyValuePair<string, bool> pair in _defineToggleTable )
            {
                if( pair.Value && !defineSet.Contains( pair.Key ) )
                {
                    // 디파인 관리 툴에서 체크 되었지만 아직 PlayerSetting에서 확인 안되는 #define은 추가.
                    defineSet.Add( pair.Key );
                }
                else if( !pair.Value && defineSet.Contains( pair.Key ) )
                {
                    // 디파인 관리 툴에 체크 안 되었지만 PlayerSetting에서 확인 되는 #define은 제거.
                    defineSet.Remove( pair.Key );
                }
            }

            System.Text.StringBuilder newDefineBuilder = new System.Text.StringBuilder( 256 );
            foreach( string defineString in defineSet )
            {
                newDefineBuilder.Append( defineString );
                newDefineBuilder.Append( ";" );
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                _selectedBuildTarget, newDefineBuilder.ToString() );
        }

        void Update()
        {
            if( _reservedCloseWindow )
            {
                EditorPrefs.SetFloat( "DefineEditorWindow_PositionX", s_instance.position.x );
                EditorPrefs.SetFloat( "DefineEditorWindow_PositionY", s_instance.position.y );
                this.Close();
            }
        }
    }
}