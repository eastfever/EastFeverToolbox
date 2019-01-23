using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace EastFever
{
    [CustomEditor( typeof( CommandExecuter ) )]
    public class CommandExecuterInspector : Editor
    {
        [UnityEditor.MenuItem( "EAST_FEVER/현재 씬에 커맨드 실행기 추가 &c" )]
        static public void AddCommanderToCurrentScene()
        {
            CommandExecuter commander = FindObjectOfType( typeof( CommandExecuter ) ) as CommandExecuter;
            if( null == commander )
            {
                GameObject commanderObject = new GameObject( "CommandExecuter" );
                commander = commanderObject.AddComponent<CommandExecuter>();
            }
            Selection.activeGameObject = commander.gameObject;
        }

        CommandExecuter m_targetExecuter = null;
        private string m_commandString = "";

        void OnEnable()
        {
            m_targetExecuter = target as CommandExecuter;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Space( 10.0f );
            GUILayout.BeginHorizontal();

            GUI.SetNextControlName( "CommandField" );
            m_commandString = EditorGUILayout.TextField( m_commandString );

            if( GUIUtility.keyboardControl != 0 )
            {
                if( Event.current.keyCode == KeyCode.Return )
                {
                    m_targetExecuter.ExecuteCommand( m_commandString );
                    GUIUtility.keyboardControl = 0;
                }
                else if( Event.current.keyCode == KeyCode.Escape )
                {
                    GUIUtility.keyboardControl = 0;
                    m_commandString = "";
                }
            }
            if( GUILayout.Button( "실행", GUILayout.Width( 60.0f ) ) )
            {
                m_targetExecuter.ExecuteCommand( m_commandString );
                EditorUtility.SetDirty( m_targetExecuter );
            }

            GUILayout.EndHorizontal();

            GUILayout.Space( 10.0f );
            GUILayout.Label( "등록된 명령어 ----------------------" );
            GUILayout.Space( 10.0f );

            List<string> commandList = m_targetExecuter.GetCommandList();
            foreach( string command in commandList )
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label( command );
                GUILayout.FlexibleSpace();
                if( GUILayout.Button( command + "실행", GUILayout.Width( 120f ) ) )
                {
                    m_targetExecuter.ExecuteCommand( command );
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }
}