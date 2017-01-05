using UnityEngine;
using System.Collections.Generic;

namespace EastFever
{
    // 개발 테스트 용으로 인스펙터에 특정 커맨드를 입력하고 실행 버튼을 누르면 특정 기능이 실행되도록 함패
    // 커맨드는 명령어(공백)인수1, 인수2, 인수3, .... 와 같은 형식으로 구성된다. ex) someCommand arg1, arg2, arg3
    [ExecuteInEditMode]
    public class CommandExecuter : MonoBehaviour
    {
        public delegate void CommandDelegate( string[] arguments );

        private Dictionary<string, CommandDelegate> _commandTable = new Dictionary<string, CommandDelegate>();

        public static CommandExecuter GetCommandExecuter()
        {
            CommandExecuter commander =
                FindObjectOfType( typeof( CommandExecuter ) ) as CommandExecuter;

            if( null == commander )
            {
                GameObject commanderObject = new GameObject( "CommandExecuter" );
                commander = commanderObject.AddComponent<CommandExecuter>();
            }
            return commander;
        }

        void OnEnable()
        {
            // 명령어 등록        
            _commandTable.Add( "delete_all_pref", OnDeleteAllPref );
        }

        // 등록된 명령어 리스트 반환
        public List<string> GetCommandList()
        {
            List<string> commandList = new List<string>();
            foreach( KeyValuePair<string, CommandDelegate> pair in _commandTable )
            {
                commandList.Add( pair.Key );
            }
            return commandList;
        }

        // 커맨드 실행
        public void ExecuteCommand( string command )
        {
            // 입력받은 커맨드에 문제 없는지 검사
            if( command.Length <= 0 )
            {
                return;
            }

            int firstSpaceIndex = command.IndexOf( ' ' );
            if( firstSpaceIndex >= 0 )
            {
                // 인수와 함께 전달됨
                string commandName = command.Substring( 0, firstSpaceIndex );
                if( !_commandTable.ContainsKey( commandName ) )
                {
                    Debug.Log( "[CommandExecuter]Unregisterd command - " + commandName );
                    return;
                }

                // 커맨드의 인수 추출
                string arguments = command.Substring( firstSpaceIndex );
                arguments = arguments.Trim();

                // 커맨드와 연결된 기능에 인수 전달과 함께 실행
                string[] argTokens = arguments.Split( ',' );
                for( int i = 0; i < argTokens.Length; i++ )
                {
                    argTokens[ i ] = argTokens[ i ].TrimStart( ' ' );
                }

                _commandTable[ commandName ]( argTokens );
            }
            else
            {
                // 커맨드만 전달됨
                if( !_commandTable.ContainsKey( command ) )
                {
                    Debug.Log( "[CommandExecuter]Unregisterd command - " + command );
                    return;
                }

                _commandTable[ command ]( null );
            }
        }

        void OnDeleteAllPref( string[] arguments )
        {
            PlayerPrefs.DeleteAll();
            Debug.Log( "[CommandExecuter/delete_all_pref]Executed" );
        }
    }
}