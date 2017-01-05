using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace EastFever
{
    // 게임 정보 편집 기능 제공.
    public class GameInfoEditor : EditorWindow
    {
        static GameInfoEditor s_instance;
        public readonly static byte s_recordCountPerPage = 27;
        protected bool m_bReservedToClose = false;

        // 처리중인 테이블들.
        private GameInfoTable _selectedTable = null;
        private string _tableNameBeforeSettingMode = "";
        private Dictionary<string, GameDataTableInfo> _tableInfoes = new Dictionary<string, GameDataTableInfo>();

        // 테이블 설정 모드 여부.
        private bool _tableInfoesettingMode = false;

        // 테이블 스크롤 위치.
        private Vector2 _tableInfoescrollPivot = Vector2.zero;

        // 테이블 이름 검색에 사용되는 필터.
        private string m_tableInfoesearchFilter = "";

        // 현재 체크 박스에 체크되어진 항목들.
        private bool[] m_selectionFlags = new bool[ s_recordCountPerPage ];

        // 현재 편집중인 정보의 인덱스.
        private int _selectedRecordIndex = 0;

        // 페이지 텍스트 필드에 쓰여져 있는 숫자.
        bool m_bFailedMoveToPage = false;        

        private Rect _rectForUI = new Rect();
        private GUIStyle _warningMessageStyle = new GUIStyle();

        private Color _processingColorInDataField = Color.white;

        // 외부에서 정해준 테이블/레코드를 선택한 상태에서 툴을 띄울때 사용.
        private string _reservedTableNameOnEnable = "";
        private int _reservedRecordIDOnEnable = 0;

        private Vector2 _recordListScrollViewPosition = Vector2.zero;
        private Vector2 _tableSettingScrollViewPosition = Vector2.zero;

        // 레코드 ID 입력 직전 값.
        private int _recordIDBeforeTyping = 0;
        private int _typingRecordID = 0;

        // 현재 처리중인 테이블의 ID정렬 리스트. 
        // 테이블이 변경 되거나 테이블에 레코드 추가/삭제 등이 발생하면 갱신해 주어야 한다.
        private List<GameInfoRecord> _sortedRecordList = null;

        // 현재 입력되어 있는 검색 타입과 검색어.   
        private string m_currentSearchKeyword = "";

        // 검색어 필터링으로 찾아낸 레코드 리스트.
        private List<GameInfoRecord> _filteredRecordList = new List<GameInfoRecord>();

        [UnityEditor.MenuItem( "EAST_FEVER/게임 정보 편집기 &i" )]
        static public void ShowEditor()
        {
            GameInfoEditor.ShowEditor( "", 0 );
        }

        static public void ShowEditor( string tableNameToShow = "", int recordIDToShow = 0 )
        {
            if( s_instance != null )
            {
                if( tableNameToShow.Length > 0 && recordIDToShow > 0 )
                {
                    s_instance.ReserveShowRecord( tableNameToShow, recordIDToShow );
                }
                s_instance.ShowUtility();
                return;
            }

            float posX = EditorPrefs.GetFloat( "GameInfoEditorX" );
            float posY = EditorPrefs.GetFloat( "GameInfoEditorY" );
            if( posX < 1.0f ) posX = 10.0f;
            if( posY < 1.0f ) posY = 10.0f;
            //Debug.Log( posX.ToString() + "," + posY.ToString() );        

            s_instance = EditorWindow.GetWindow<GameInfoEditor>( false, "GameInfo Editor" );
            Rect windowRect = new Rect( posX, posY, 1100, 700 );
            s_instance.position = windowRect;

            if( tableNameToShow.Length > 0 && recordIDToShow > 0 )
            {
                s_instance.ReserveShowRecord( tableNameToShow, recordIDToShow );
            }
            s_instance.ShowUtility();
        }

        // 특정 테이블의 레코드가 시작과 함께 표시되도록 한다.
        public void ReserveShowRecord( string tableName, int recordID )
        {
            _reservedTableNameOnEnable = tableName;
            _reservedRecordIDOnEnable = recordID;
        }

        // 창이 열릴 때 호출됨.
        void OnEnable()
        {
            ClearCheckBoxes();

            _warningMessageStyle.alignment = TextAnchor.MiddleCenter;
            _warningMessageStyle.fontSize = 24;

            this.LoadTableInfoes();

            string lastTableName = EditorPrefs.GetString( "GameInfoEditor_LastTable" );
            if( lastTableName != null && _tableInfoes.ContainsKey( lastTableName ) )
            {
                this.ChangeDataTable( lastTableName );
            }
        }

        // 창이 닫혀질 때 호출됨.
        void OnDestroy()
        {
            if( s_instance != null )
            {
                EditorPrefs.SetFloat( "GameInfoEditorX", s_instance.position.x );
                EditorPrefs.SetFloat( "GameInfoEditorY", s_instance.position.y );
            }

            if( _selectedTable != null )
            {
                EditorPrefs.SetString( "GameInfoEditor_LastTable", _selectedTable.TableName );
            }
        }

        void SetRectTo( ref Rect rect, float leftTop, float rightBottom, float width, float height )
        {
            rect.x = leftTop;
            rect.y = rightBottom;
            rect.width = width;
            rect.height = height;
        }

        // 열람/편집하는 데이터 테이블 변경.
        void ChangeDataTable( string tableName )
        {
            if( _selectedTable != null && _selectedTable.TableName == tableName )
            {
                return;
            }

            if( !_tableInfoes.ContainsKey( tableName ) )
            {
                Debug.LogError( "[ChangeDataTable]table not found - " + tableName );
                return;
            }

            GameDataTableInfo targetTableInfo = _tableInfoes[ tableName ];

            _selectedTable = null;
            _selectedTable = new GameInfoTable( tableName, targetTableInfo.FieldList() );

            if( _selectedTable.RecordCount() > 0 )
            {
                RefreshSortedRecordList();                
            }
        }

        // 매 프레임 처리.
        void Update()
        {
            if( _reservedTableNameOnEnable != null && _reservedTableNameOnEnable.Length > 0 )
            {
                this.ChangeDataTable( _reservedTableNameOnEnable );
                _reservedTableNameOnEnable = "";
            }

            if( _reservedRecordIDOnEnable != 0 )
            {
                _selectedRecordIndex = _reservedRecordIDOnEnable;
                _reservedRecordIDOnEnable = 0;
            }

            if( m_bFailedMoveToPage )
            {
                //EditorUtility.DisplayDialog(
                //    "페이지 이동 실패",
                //    "해당 페이지는 존재 하지 않습니다",
                //    "예" );
                string message = "해당 페이지는 존재 하지 않습니다";
                this.ShowNotification( new GUIContent( message ) );
                m_bFailedMoveToPage = false;
            }

            if( m_bReservedToClose )
            {
                this.Close();
                return;
            }
        }

        // UI 구현하기.
        void OnGUI()
        {
            // 단축키 처리.
            this.HandleShortCut();

            // 현재 편집중인 게임 정보 테이블과 테이블 리스트 필터.
            Rect uiRect = new Rect( 10, 10, 190, 120 );
            ProcessSelectedGameInfoRegion( uiRect );

            // 게임 정보 테이블 리스트 표시.
            SetRectTo( ref uiRect, 10, 140, 190, 370 );
            ProcessGameInfoTableList( uiRect );

            // 레코드 설정 영역.
            if( !_tableInfoesettingMode )
            {
                SetRectTo( ref uiRect, 10, 520, 190, 80 );
                ProcessRecordSection( uiRect );
            }

            // 테이블 설정 영역.
            SetRectTo( ref uiRect, 10, 610, 190, 60 );
            ProcessTableSection( uiRect );

            // 게임 정보 항목 리스트.
            SetRectTo( ref uiRect, 210, 10, 400, 550 );
            ProcessRecordList( uiRect );

            // 게임 정보 항목 리스트 컨트롤러.
            SetRectTo( ref uiRect, 210, 570, 400, 120 );
            ProcessRecordListController( uiRect );

            // 게임 정보 상세 항목 컨트롤러.
            SetRectTo( ref uiRect, 620, 10, 470, 680 );
            ProcessDetailInfoController( uiRect );

        }

        // 현재 편집중인 게임 정보 테이블과 테이블 리스트 필터.
        void ProcessSelectedGameInfoRegion( Rect uiRect )
        {
            Color oldGUIContentColor = GUI.contentColor;
            GUI.contentColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
            GUILayout.BeginArea( uiRect, GUI.skin.box );

            string selectedTableLabel = "선택된 테이블 : ";
            if( _selectedTable != null )
            {
                selectedTableLabel += _selectedTable.TableName;
            }
            Rect rect = new Rect( 10, 10, 170, 20 );
            GUI.Label( rect, selectedTableLabel );

            this.SetRectTo( ref rect, 10, 50, 170, 20 );
            GUI.Label( rect, "테이블 리스트 필터" );

            this.SetRectTo( ref rect, 10, 70, 170, 20 );
            m_tableInfoesearchFilter = GUI.TextField( rect, m_tableInfoesearchFilter );

            this.SetRectTo( ref rect, 10, 95, 170, 20 );
            if( GUI.Button( rect, "필터 없애기" ) )
            {
                m_tableInfoesearchFilter = "";
            }

            GUILayout.EndArea();
            GUI.contentColor = oldGUIContentColor;
        }

        // 게임 정보 테이블 리스트를 보여준다.
        void ProcessGameInfoTableList( Rect uiRect )
        {
            Color oldColor = GUI.color;
            GUILayout.BeginArea( uiRect, GUI.skin.box );

            GUI.skin.horizontalScrollbar = null;
            int scrollHeight = _tableInfoes.Count * 20;
            _tableInfoescrollPivot = GUI.BeginScrollView(
                                        new Rect( 0, 0, uiRect.width, uiRect.height ),
                                        _tableInfoescrollPivot,
                                        new Rect( 0, 0, uiRect.width, scrollHeight ) );

            int tableButtonVerticalOrder = 0;
            foreach( string tableName in _tableInfoes.Keys )
            {
                if( !tableName.Contains( m_tableInfoesearchFilter ) )
                {
                    continue;
                }

                if( _selectedTable != null && tableName == _selectedTable.TableName )
                {
                    GUI.color = Color.yellow;
                }
                else
                {
                    GUI.color = Color.white;
                }

                if( GUI.Button(
                            new Rect( 0, tableButtonVerticalOrder * 20, 189, 20 ),
                            tableName ) )
                {
                    GUIUtility.keyboardControl = 0;
                    this.ChangeDataTable( tableName );
                    break;
                }
                tableButtonVerticalOrder++;
            }

            GUI.EndScrollView();
            GUILayout.EndArea();
            GUI.color = oldColor;
        }

        // 테이블 설정 영역 처리.
        void ProcessTableSection( Rect uiRect )
        {
            Color oldGUIContentColor = GUI.contentColor;
            GUI.contentColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
            GUILayout.BeginArea( uiRect, GUI.skin.box );

            float functionKeyPosY = 0;
            if( !_tableInfoesettingMode )
            {
                if( GUI.Button( new Rect( 0, functionKeyPosY, 190, 20 ), "테이블 추가" ) )
                {
                    AddTable();
                }
                functionKeyPosY += 20;
                if( GUI.Button( new Rect( 0, functionKeyPosY, 190, 20 ), "테이블 삭제" ) )
                {
                    if( null == _selectedTable )
                    {
                        EditorUtility.DisplayDialog(
                            "테이블 삭제 실패",
                            "삭제를 위해 선택된 테이블이 없습니다",
                            "확인" );
                        return;
                    }

                    if( EditorUtility.DisplayDialog(
                        "테이블 삭제",
                        "선택 되어 있는 테이블을 삭제 합니다\n\n" + _selectedTable.TableName,
                        "NO", "YES" ) )
                    {
                        return;
                    }
                    DeleteTable( _selectedTable.TableName );
                }
            }

            functionKeyPosY += 20;
            string infoString = "테이블 설정 모드 켜기";
            if( _tableInfoesettingMode )
            {
                infoString = "설정 정보 저장 후 설정 모드 끄기";
            }
            if( GUI.Button( new Rect( 0, functionKeyPosY, 190, 20 ), infoString ) )
            {
                if( _tableInfoesettingMode )
                {
                    this.SaveTableInfoes();
                    if( _tableInfoes.ContainsKey( _selectedTable.TableName ) )
                    {
                        _selectedTable.ChangeSchema( _tableInfoes[ _selectedTable.TableName ] );
                        SaveTable( _selectedTable.TableName );
                    }

                    // 이전에 쓰던 테이블 정보 제거.
                    if( _tableNameBeforeSettingMode != null
                        && _tableNameBeforeSettingMode.Length > 0
                        && _tableNameBeforeSettingMode != _selectedTable.TableName )
                    {
                        AssetDatabase.DeleteAsset( "Assets/Resources/GameInfoTable/" + _tableNameBeforeSettingMode + ".txt" );
                        AssetDatabase.Refresh();
                    }
                    GameInfoTableFactory.Instance.Reset();
                    GameInfoTableFactory.Instance.Initialize();
                }
                else
                {
                    _tableNameBeforeSettingMode = _selectedTable.TableName;
                }
                _tableInfoesettingMode = !_tableInfoesettingMode;
            }

            GUILayout.EndArea();
            GUI.contentColor = oldGUIContentColor;
        }

        // 레코드 설정 영역 처리.
        void ProcessRecordSection( Rect uiRect )
        {
            Color oldGUIContentColor = GUI.contentColor;
            GUI.contentColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
            GUILayout.BeginArea( uiRect, GUI.skin.box );

            float functionKeyPosY = 0;
            if( GUI.Button( new Rect( 0, functionKeyPosY, 190, 20 ), "레코드 추가" ) )
            {
                if( null == _selectedTable )
                {
                    EditorUtility.DisplayDialog(
                        "레코드 초기화 실패",
                        "선택되어 있는 테이블이 없어서\n레코드를 추가할 수 없습니다",
                        "확인" );
                    return;
                }

                int newID = _selectedTable.AddRecord();
                RefreshSortedRecordList();

                this.ProcessOnRecordSelection( newID );
            }

            functionKeyPosY += 20;
            if( GUI.Button( new Rect( 0, functionKeyPosY, 190, 20 ), "레코드 복사" ) )
            {
                GameInfoRecord recordToCopy = null;
                for( int i = 0; i < s_recordCountPerPage; i++ )
                {
                    if( m_selectionFlags[ i ] )
                    {
                        recordToCopy = _sortedRecordList[ i ];
                        m_selectionFlags[ i ] = false;
                        break;
                    }
                }

                if( null == recordToCopy )
                {
                    recordToCopy = _selectedTable.GetRecord( _selectedRecordIndex );
                }

                int newID = _selectedTable.CopyRecord( recordToCopy.ID );

                RefreshSortedRecordList();
                ProcessOnRecordSelection( newID );
                ClearCheckBoxes();
            }

            functionKeyPosY += 20;
            if( GUI.Button( new Rect( 0, functionKeyPosY, 190, 20 ), "레코드 삭제" ) )
            {
                bool useCheckBox = false;
                for( int i = 0; i < s_recordCountPerPage; i++ )
                {
                    if( m_selectionFlags[ i ] )
                    {
                        useCheckBox = true;
                        break;
                    }
                }

                GameInfoRecord selectedRecord = null;
                string popupTitle = "레코드 삭제";
                string popupMessage = "체크 박스에 체크되어 있는\n레코드의 정보들을 삭제 합니다";
                if( !useCheckBox )
                {
                    selectedRecord = _selectedTable.GetRecord( _selectedRecordIndex );
                    popupMessage = string.Format( "다음 레코드를 삭제 합니다\n\n{0}. {1}", selectedRecord.ID, selectedRecord.Name );
                }
                if( EditorUtility.DisplayDialog( popupTitle, popupMessage, "NO", "YES" ) )
                {
                    return;
                }

                if( useCheckBox )
                {
                    for( int i = 0; i < m_selectionFlags.Length; i++ )
                    {
                        if( m_selectionFlags[ i ] )
                        {
                            _selectedTable.DeleteRecord( _sortedRecordList[ i ].ID );
                            m_selectionFlags[ i ] = false;
                        }
                    }                    
                }
                else
                {
                    _selectedTable.DeleteRecord( selectedRecord.ID );
                }

                RefreshSortedRecordList();
                ClearCheckBoxes();
            }

            functionKeyPosY += 20;
            if( GUI.Button( new Rect( 0, functionKeyPosY, 190, 20 ), "테이블 저장" ) )
            {
                if( null == _selectedTable )
                {
                    EditorUtility.DisplayDialog(
                        "테이블 저장 실패",
                        "저장할 테이블이 선택되지 않았습니다",
                        "확인" );
                    return;
                }
                SaveTable( _selectedTable.TableName );
            }

            GUILayout.EndArea();
            GUI.contentColor = oldGUIContentColor;
        }        

        // 좌측의 정보 리스트.
        void ProcessRecordList( Rect uiRect )
        {
            Color oldGUIColor = GUI.color;
            GUILayout.BeginArea( uiRect, GUI.skin.box );
            _recordListScrollViewPosition =
                EditorGUILayout.BeginScrollView(
                    _recordListScrollViewPosition,
                    GUILayout.Width( uiRect.width ),
                    GUILayout.Height( uiRect.height ) );

            List<GameInfoRecord> recordList = null;
            if( _filteredRecordList.Count > 0 )
            {
                recordList = _filteredRecordList;
            }
            else
            {
                recordList = _sortedRecordList;
            }

            int selectedOnThisUpdate = -1;
            if( recordList != null && recordList.Count > 0 )
            {                
                for( int i = 0; i < recordList.Count; i++ )
                {
                    EditorGUILayout.BeginHorizontal();
                    GameInfoRecord record = recordList[ i ];
                    
                    // 현재 선택된 항목에 대해서는 녹색으로 표시.
                    if( record.ID == _selectedRecordIndex )
                    {
                        GUI.color = Color.yellow;
                    }
                    else
                    {
                        GUI.color = Color.white;
                    }

                    // 체크 박스.                    
                    m_selectionFlags[ i ] = 
                        EditorGUILayout.Toggle( 
                            m_selectionFlags[ i ], 
                            GUILayout.Width( 18f ), 
                            GUILayout.Height( 20f ) );                    

                    // 레코드 ID.                 
                    if( GUILayout.Button( record.ID.ToString(), GUILayout.Width( 125 ), GUILayout.Height( 18f ) ) )
                    {
                        selectedOnThisUpdate = record.ID;
                        GUIUtility.keyboardControl = 0;
                        this.Repaint();
                    }

                    // 레코드 이름.                    
                    if( GUILayout.Button( record.Name, GUILayout.Width( 230 ), GUILayout.Height( 18f ) ) )
                    {
                        selectedOnThisUpdate = record.ID;
                        GUIUtility.keyboardControl = 0;
                        this.Repaint();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                // for( int i = 0; i < recordList.Count; i++ )
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
            GUI.color = oldGUIColor;

            // 레코드 선택 됐을때 처리.
            if( selectedOnThisUpdate > -1 )
            {
                this.ProcessOnRecordSelection( selectedOnThisUpdate );
            }
        }

        // 레코드가 선택 됐을 때 처리를 실행.
        void ProcessOnRecordSelection( int newRecordIndex )
        {
            if( null == _selectedTable )
            {
                return;
            }

            int refinedIndex = newRecordIndex;
            if( refinedIndex <= 0 )
            {
                refinedIndex = 0;
            }

            _selectedRecordIndex = refinedIndex;            
        }

        // 레코드 이동 버튼을 사용할 수 있는지 여부를 반환한다.
        bool IsEnableMoveRecord( int firstSelectionIndex, bool multiSelected )
        {
            if( multiSelected )
            {
                string message = "한 번에 하나의 레코드만\n이동시킬 수 있습니다";
                this.ShowNotification( new GUIContent( message ) );
                return false;
            }

            if( -1 == firstSelectionIndex )
            {
                string message = "이동 시킬 레코드를 선택해 주세요";
                this.ShowNotification( new GUIContent( message ) );
                return false;
            }

            return true;
        }

        // 좌측 하단의 정보 리스트 컨트롤러.
        void ProcessRecordListController( Rect uiRect )
        {
            Color oldGUIContentColor = GUI.contentColor;
            GUI.contentColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
            GUILayout.BeginArea( uiRect, GUI.skin.box );

            Rect rectForControls = new Rect();
            GUI.contentColor = Color.white;

            int selectedCheckBoxIndex = -1;
            //bool bMultiSelected = false;
            for( int i = 0; i < s_recordCountPerPage; i++ )
            {
                if( m_selectionFlags[ i ] )
                {
                    if( -1 == selectedCheckBoxIndex )
                    {
                        selectedCheckBoxIndex = i;
                    }
                    else
                    {
                        //bMultiSelected = true;
                        break;
                    }
                }
            }

            // 검색할 대상.
            this.SetRectTo( ref rectForControls, 7, 32, 80, 20 );
            GUI.Label( rectForControls, "검색어 : " );            

            // 검색하기.
            this.SetRectTo( ref rectForControls, 192, 32, 133, 20 );
            m_currentSearchKeyword = EditorGUI.TextField( rectForControls, m_currentSearchKeyword );
            this.SetRectTo( ref rectForControls, 330, 32, 60, 20 );
            if( GUI.Button( rectForControls, "검색" ) )
            {
                if( m_currentSearchKeyword != null && m_currentSearchKeyword.Length > 0 )
                {
                    FilterRecords();
                }
            }            

            this.SetRectTo( ref rectForControls, 7, 82, 386, 25 );
            if( GUI.Button( rectForControls, "처음 화면으로" ) )
            {
                m_currentSearchKeyword = "";
                if( _selectedTable.RecordCount() > 0 )
                {
                    _filteredRecordList.Clear();
                }
                GUIUtility.keyboardControl = 0;
                this.Repaint();
            }

            GUILayout.EndArea();
            GUI.contentColor = oldGUIContentColor;
        }

        // 게임 정보 상세 항목 컨트롤러.
        void ProcessDetailInfoController( Rect uiRect )
        {
            Color oldGUIContentColor = GUI.contentColor;
            GUI.contentColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
            GUILayout.BeginArea( uiRect, GUI.skin.box );

            // 테이블이 하나도 없을 때.
            Rect rect = new Rect( 0, 0, uiRect.width, uiRect.height );
            if( 0 == _tableInfoes.Count )
            {
                GUI.contentColor = Color.red;
                GUI.Label(
                    rect,
                    "등록된 테이블이 하나도 없습니다.\n테이블을 새로 추가해 주세요.",
                    _warningMessageStyle );
                GUI.contentColor = oldGUIContentColor;
                GUILayout.EndArea();
                return;
            }

            if( _tableInfoesettingMode )
            {
                this.ProcessTableSettingGUI( uiRect.width, uiRect.height );
            }
            else
            {
                this.ProcessTableRecordGUI( uiRect.width, uiRect.height );
            }
            GUILayout.EndArea();
            GUI.contentColor = oldGUIContentColor;
        }

        // 새로 테이블을 추가한다.
        void AddTable()
        {
            GameDataTableInfo newGameDataTableInfo = new GameDataTableInfo();

            string newTableName = "New Table";
            while( _tableInfoes.ContainsKey( newTableName ) )
            {
                newTableName += "+";
            }

            newGameDataTableInfo.SetTableName( newTableName );
            _tableInfoes.Add( newTableName, newGameDataTableInfo );

            _selectedTable = new GameInfoTable( newTableName, _tableInfoes[ newTableName ].FieldList(), true );
            _selectedTable.SaveTable( _tableInfoes[ newTableName ].FieldList() );
            _tableInfoesettingMode = true;

            _tableNameBeforeSettingMode = newTableName;

            RefreshSortedRecordList();
            AssetDatabase.Refresh();
        }

        // 테이블을 저장.
        void SaveTable( string tableName )
        {
            if( !_tableInfoes.ContainsKey( tableName ) )
            {
                EditorUtility.DisplayDialog(
                        "테이블 저장 실패 - " + tableName,
                        "해당 테이블을 찾지 못해\n테이블을 지우지 못했습니다",
                        "확인" );
                return;
            }

            GameDataTableInfo targetTableInfo = _tableInfoes[ tableName ];
            _selectedTable.SaveTable( targetTableInfo.FieldList() );

            GameInfoTableFactory.Instance.ClearCaches();

            this.ShowNotification( new GUIContent( "테이블 저장이 완료 되었습니다" ) );
            AssetDatabase.Refresh();
        }

        // 선택된 테이블을 삭제한다.
        void DeleteTable( string tableName )
        {
            if( !_tableInfoes.ContainsKey( tableName ) )
            {
                EditorUtility.DisplayDialog(
                        "테이블 삭제 실패 - " + tableName,
                        "해당 테이블을 찾지 못해\n테이블을 지우지 못했습니다",
                        "확인" );
                return;
            }

            _tableInfoes.Remove( tableName );
            _tableInfoescrollPivot = Vector2.zero;

            AssetDatabase.DeleteAsset( "Assets/Resources/GameInfoTable/" + tableName + ".txt" );
            SaveTableInfoes();
            AssetDatabase.Refresh();

            if( _tableInfoes.Count <= 0 )
            {
                _selectedTable = null;
                return;
            }

            string remainTableName = "";
            foreach( string key in _tableInfoes.Keys )
            {
                remainTableName = key;
                break;
            }

            this.ChangeDataTable( remainTableName );
        }

        // 테이블 레코드의 정렬 리스트를 갱신한다.
        void RefreshSortedRecordList()
        {
            _filteredRecordList.Clear();
            if( _selectedTable != null )
            {
                _sortedRecordList = _selectedTable.SortedRecordList;
                if( _sortedRecordList.Count > 0 )
                {
                    _selectedRecordIndex = _sortedRecordList[ 0 ].ID;
                    if( _sortedRecordList.Count > m_selectionFlags.Length )
                    {
                        bool[] oldFlags = m_selectionFlags;
                        m_selectionFlags = new bool[ _sortedRecordList.Count ];
                        for( int i = 0; i < oldFlags.Length; i++ )
                        {
                            m_selectionFlags[ i ] = oldFlags[ i ];
                        }
                        oldFlags = null;
                    }

                    // 검색어 필터링 상태 갱신.
                    FilterRecords();
                }
            }
            else
            {
                _sortedRecordList = null;
                _selectedRecordIndex = -1;
            }            
        }

        void FilterRecords()
        {
            _filteredRecordList.Clear();
            foreach( GameInfoRecord record in _sortedRecordList )
            {
                if( record.ID.ToString().Contains( m_currentSearchKeyword )
                    || record.Name.Contains( m_currentSearchKeyword ) )
                {
                    _filteredRecordList.Add( record );
                }
            }
            Repaint();
        }

        // 테이블 설정 모드의 GUI를 표시한다.
        void ProcessTableSettingGUI( float uiRegionWidth, float uiRegionHeight )
        {
            Rect rect = new Rect( 10, 10, 450, 20 );
            if( null == _selectedTable || !_tableInfoes.ContainsKey( _selectedTable.TableName ) )
            {
                this.SetRectTo( ref rect, 0, 0, uiRegionWidth, uiRegionHeight );
                GUI.Label(
                        rect,
                        "선택한 테이블을 찾을 수 없습니다.\n다른 테이블을 선택해 보세요.",
                        _warningMessageStyle );
                return;
            }

            GameDataTableInfo targetTableInfo = _tableInfoes[ _selectedTable.TableName ];

            _tableSettingScrollViewPosition = EditorGUILayout.BeginScrollView(
                                    _tableSettingScrollViewPosition,
                                    GUILayout.Width( uiRegionWidth - 7f ),
                                    GUILayout.Height( uiRegionHeight - 25f ) );

            EditorGUILayout.Space();
            string oldName = targetTableInfo.TableName;
            string newName = EditorGUILayout.TextField( "테이블 이름 : ", oldName );
            if( oldName != newName )
            {
                // 테이블 이름 변경.
                if( !_tableInfoes.ContainsKey( newName ) )
                {
                    _tableInfoes.Add( newName, targetTableInfo );
                    _tableInfoes.Remove( oldName );

                    targetTableInfo.SetTableName( newName );
                    _selectedTable.SetTableName( newName );
                }
            }

            GameDataTableInfo selectedTableInfo = null;
            if( _tableInfoes.ContainsKey( _selectedTable.TableName ) )
            {
                selectedTableInfo = _tableInfoes[ _selectedTable.TableName ];
            }

            // 등록된 필드 표시.
            int idToDelete = -1;
            EditorGUILayout.Space();
            foreach( int fieldID in selectedTableInfo.FieldIDs() )
            {
                GUILayout.BeginHorizontal();
                GameInfoTableField dataForm = selectedTableInfo.Field( fieldID );
                GUILayout.Label( "필드명 : ", GUILayout.MaxWidth( 50.0f ) );
                string newFieldName = GUILayout.TextField( dataForm.Name, GUILayout.MaxWidth( 100.0f ) );
                if( newFieldName != dataForm.Name )
                {
                    if( newFieldName == "ID" || newFieldName == "Name" )
                    {
                        EditorUtility.DisplayDialog(
                            "필드명 변경 실패",
                            "ID, Name이라는 필드명을 사용할 수 없습니다",
                            "확인" );
                        return;
                    }
                    //else if( newFieldName == "" )
                    //{
                    //	EditorUtility.DisplayDialog(
                    //		"필드명 변경 실패",
                    //		"빈 문자열은 사용할 수 없습니다",
                    //		"확인" );
                    //	return;
                    //}
                    dataForm.Name = newFieldName;
                }

                dataForm.DataType = ( eGameDataType )EditorGUILayout.EnumPopup( dataForm.DataType, GUILayout.MaxWidth( 100.0f ) );
                if( dataForm.DataType == eGameDataType.Enum
                    || dataForm.DataType == eGameDataType.EnumFlag )
                {
                    GUILayout.Label( "이넘명 : ", GUILayout.Width( 50f ) );
                    dataForm.Additive = EditorGUILayout.TextField( dataForm.Additive, GUILayout.Width( 100f ) );
                }
                else
                {
                    dataForm.Additive = "";
                }

                // 필드 삭제 버튼
                if( GUILayout.Button( "필드 삭제", GUILayout.MaxWidth( 60.0f ) ) )
                {
                    idToDelete = fieldID;
                }
                GUILayout.EndHorizontal();
            }

            // 필드 삭제 버튼 탭에 의한 필드 삭제
            if( idToDelete > -1 )
            {
                selectedTableInfo.RemoveField( idToDelete );
            }

            // 필드 추가 버튼
            EditorGUILayout.Space();
            if( GUILayout.Button( "데이터 필드 추가" ) )
            {
                if( _tableInfoes.ContainsKey( _selectedTable.TableName ) )
                {
                    selectedTableInfo = _tableInfoes[ _selectedTable.TableName ];
                }

                if( null == selectedTableInfo )
                {
                    Debug.LogWarning( "table not found - " + _selectedTable.TableName );
                    return;
                }

                selectedTableInfo.AddField( eGameDataType.Int, "New Field", "" );
            }

            EditorGUILayout.EndScrollView();
        }

        // 테이블의 레코드에 저장된 정보들을 표시한다.
        void ProcessTableRecordGUI( float uiRegionWidth, float uiRegionHeight )
        {
            if( null == _selectedTable || !_tableInfoes.ContainsKey( _selectedTable.TableName ) )
            {
                this.SetRectTo( ref _rectForUI, 0, 0, uiRegionWidth, uiRegionHeight );
                GUI.Label(
                        _rectForUI,
                        "선택한 테이블을 찾을 수 없습니다\n다른 테이블을 선택해 보세요",
                        _warningMessageStyle );
                return;
            }

            if( _selectedTable.RecordCount() <= 0 )
            {
                this.SetRectTo( ref _rectForUI, 0, 0, uiRegionWidth, uiRegionHeight );
                GUI.Label(
                        _rectForUI,
                        "선택한 테이블에 등록된\n레코드가 하나도 없습니다\n레코드를 추가해 주세요",
                        _warningMessageStyle );
                return;
            }

            // 선택한 레코드 인덱스와 테이블 내 레코드 인덱스 불일치 문제( 테이블 내에는 1로 저장되지만 에디터는 0으로 셋팅된 상태 )
            GameInfoRecord selectedRecord = _selectedTable.GetRecord( _selectedRecordIndex );
            if( null == selectedRecord )
            {
                return;
            }

            EditorGUILayout.Space();
            GameDataTableInfo targetTableInfo = _tableInfoes[ _selectedTable.TableName ];

            const float widthNameField = 120.0f;
            const float widthTypeField = 50.0f;
            const float widthBetweenNameAndType = 5.0f;
            float heightBetweenRecord = 8.0f;

            _tableSettingScrollViewPosition = EditorGUILayout.BeginScrollView(
                                    _tableSettingScrollViewPosition,
                                    GUILayout.Width( uiRegionWidth - 7f ),
                                    GUILayout.Height( uiRegionHeight - 25f ) );
            // 레코드 ID 표시.
            GUILayout.BeginHorizontal();
            GUILayout.Label( "레코드ID", GUILayout.MaxWidth( widthNameField ) );
            GUILayout.Space( widthBetweenNameAndType );
            GUILayout.Label( "(정수)", GUILayout.MaxWidth( widthTypeField ) );

            int newID = 0;
            if( GUIUtility.keyboardControl != 0 )
            {
                newID = EditorGUILayout.IntField( _typingRecordID );
            }
            else
            {
                newID = EditorGUILayout.IntField( selectedRecord.ID );
            }

            if( newID != selectedRecord.ID && 0 == _recordIDBeforeTyping )
            {
                // ID 타이핑 중에 키 중복 검사 안하도록 막기.
                _typingRecordID = newID;
                _recordIDBeforeTyping = selectedRecord.ID;
            }
            else if( _recordIDBeforeTyping != 0 && GUIUtility.keyboardControl == 0 )
            {
                if( _selectedTable.ExchangeID( _recordIDBeforeTyping, _typingRecordID ) )
                {
                    RefreshSortedRecordList();
                    this.ProcessOnRecordSelection( _typingRecordID );
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "ID 변경 실패",
                        "이미 존재 하거나 사용할 수 없는 ID 입니다 - " + _typingRecordID,
                        "OK" );
                }
                _recordIDBeforeTyping = 0;
                _typingRecordID = 0;
                Repaint();
            }
            else
            {
                _typingRecordID = newID;
            }

            GUILayout.EndHorizontal();

            GUILayout.Space( heightBetweenRecord );

            // 레코드 이름 표시.
            GUILayout.BeginHorizontal();
            GUILayout.Label( "레코드 이름", GUILayout.MaxWidth( widthNameField ) );
            GUILayout.Space( widthBetweenNameAndType );
            GUILayout.Label( "(문자열)", GUILayout.MaxWidth( widthTypeField ) );
            selectedRecord.SetName( EditorGUILayout.TextField( selectedRecord.Name ) );
            GUILayout.EndHorizontal();

            // 레코드 ID/이름과 다른 필드 정보 필드 간의 간격 유지.
            GUILayout.Space( 20.0f );

            // 각 필드별 데이터 표시.
            float defaultHeightBetweenRecord = heightBetweenRecord;
            foreach( int fieldID in targetTableInfo.FieldIDs() )
            {
                GUILayout.BeginHorizontal();

                // 데이터형에 관한 정보 얻기.
                GameInfoTableField dataForm = targetTableInfo.Field( fieldID );

                System.Type enumType = null;
                if( dataForm.DataType == eGameDataType.Enum
                    || dataForm.DataType == eGameDataType.EnumFlag )
                {
                    enumType = GetTypeIncludeSearchingAssembly( dataForm.Additive );
                }

                GUILayout.Label( dataForm.Name, GUILayout.MaxWidth( widthNameField ) ); // 필드명 표시.
                GUILayout.Space( widthBetweenNameAndType );
                switch( dataForm.DataType )
                {
                    case eGameDataType.Int:
                        GUILayout.Label( "(정수)", GUILayout.MaxWidth( widthTypeField ) );
                        selectedRecord.SetFieldData(
                            dataForm.Name,
                            EditorGUILayout.IntField( selectedRecord.GetFieldData<int>( dataForm.Name ) ) );
                        break;
                    case eGameDataType.Float:
                        GUILayout.Label( "(실수)", GUILayout.MaxWidth( widthTypeField ) );
                        selectedRecord.SetFieldData(
                            dataForm.Name,
                            EditorGUILayout.FloatField( selectedRecord.GetFieldData<float>( dataForm.Name ) ) );
                        break;
                    case eGameDataType.Text:
                        GUILayout.Label( "(문자열)", GUILayout.MaxWidth( widthTypeField ) );
                        selectedRecord.SetFieldData(
                            dataForm.Name,
                            EditorGUILayout.TextField( selectedRecord.GetFieldData<string>( dataForm.Name ) ) );
                        break;
                    case eGameDataType.Vector2:
                        selectedRecord.SetFieldData(
                            dataForm.Name,
                            EditorGUILayout.Vector2Field(
                                "(벡터2)",
                                selectedRecord.GetFieldData<Vector2>( dataForm.Name ) ) );
                        break;
                    case eGameDataType.Vector3:
                        selectedRecord.SetFieldData(
                            dataForm.Name,
                            EditorGUILayout.Vector3Field(
                                "(벡터3)",
                                selectedRecord.GetFieldData<Vector3>( dataForm.Name ) ) );
                        break;
                    case eGameDataType.UnityObject:
                        GUILayout.Label( "(Object)", GUILayout.MaxWidth( widthTypeField ) );

                        string assetPath = "";
                        string resourcePath = selectedRecord.GetFieldData<string>( dataForm.Name );
                        if( resourcePath != null )
                        {
                            assetPath = PathHelper.AssetPathByResourcePath( selectedRecord.GetFieldData<string>( dataForm.Name ) );
                        }
                        System.Type loadType = typeof( UnityEngine.Object );
                        UnityEngine.Object unityObject = AssetDatabase.LoadAssetAtPath( assetPath, loadType );
                        UnityEngine.Object newObject = EditorGUILayout.ObjectField( unityObject, loadType, false );
                        if( newObject != unityObject )
                        {
                            assetPath = AssetDatabase.GetAssetPath( newObject );
                            selectedRecord.SetFieldData( dataForm.Name, PathHelper.GetResourcePath( assetPath ) );
                        }
                        break;
                    case eGameDataType.Color:
                        GUILayout.Label( "(Color)", GUILayout.MaxWidth( widthTypeField ) );
                        if( selectedRecord.ExistField( dataForm.Name ) )
                        {
                            _processingColorInDataField = selectedRecord.GetFieldData<Color>( dataForm.Name );
                        }
                        else
                        {
                            Debug.LogWarning( "[GameInfoEditor]color info not found - " + dataForm.Name );
                            _processingColorInDataField = Color.white;
                        }

                        Color newColor = EditorGUILayout.ColorField( _processingColorInDataField );
                        if( newColor != _processingColorInDataField )
                        {
                            selectedRecord.SetFieldData( dataForm.Name, newColor );
                        }
                        break;
                    case eGameDataType.Enum:
                        GUILayout.Label( "(Enum)", GUILayout.MaxWidth( widthTypeField ) );

                        if( enumType != null )
                        {
                            string enumValueString = selectedRecord.GetFieldData<string>( dataForm.Name );

                            System.Enum targetEnum = default( System.Enum );
                            if( enumValueString != null && enumValueString.Length > 0 )
                            {
                                targetEnum = ( System.Enum )System.Enum.Parse( enumType, enumValueString );
                            }
                            else
                            {
                                targetEnum = ( System.Enum )System.Enum.Parse( enumType, "0" );
                            }

                            System.Enum newEnum = EditorGUILayout.EnumPopup( targetEnum );
                            if( newEnum != targetEnum )
                            {
                                selectedRecord.SetFieldData( dataForm.Name, newEnum.ToString() );
                            }
                        }
                        else
                        {
                            GUILayout.Label( string.Format( "{0} 이넘 인식 실패", dataForm.Additive ) );
                        }
                        break;
                    case eGameDataType.EnumFlag:
                        GUILayout.Label( "(Flags)", GUILayout.MaxWidth( widthTypeField ) );
                        if( null == enumType )
                        {
                            GUILayout.Label( string.Format( "{0} 이넘 인식 실패", dataForm.Additive ) );
                        }
                        else
                        {
                            System.Array enumArray = System.Enum.GetValues( enumType );
                            int rowCount = ( enumArray.Length / 2 ) + 1;
                            float areaHeight = ( rowCount - 1 ) * 20f;
                            heightBetweenRecord = areaHeight += defaultHeightBetweenRecord;
                            Rect rectToFlags = new Rect();
                            SetRectTo( ref rectToFlags, 185, 75, 280, areaHeight );
                            GUILayout.BeginArea( rectToFlags, GUI.skin.box );
                            for( int row = 0; row < rowCount; row++ )
                            {
                                for( int col = 0; col < 2; col++ )
                                {
                                    int arrayIndex = row * 2 + col;
                                    if( enumArray.Length <= arrayIndex )
                                    {
                                        break;
                                    }
                                    int flagsValue = selectedRecord.GetFieldData<int>( dataForm.Name );
                                    int targetFlagValue = ( int )enumArray.GetValue( arrayIndex );
                                    bool enabled = ( flagsValue & targetFlagValue ) != 0;

                                    SetRectTo( ref rectToFlags, col > 0 ? 155 : 5, ( row * 20 ) + 5, 20, 20 );
                                    bool newFlag = EditorGUI.Toggle( rectToFlags, enabled );
                                    selectedRecord.ToggleEnumFlag( dataForm.Name, targetFlagValue, newFlag );

                                    SetRectTo( ref rectToFlags, col > 0 ? 180 : 30, ( row * 20 ) + 5, 120, 20 );
                                    GUI.Label( rectToFlags, enumArray.GetValue( arrayIndex ).ToString() );
                                }
                            }
                            GUILayout.EndArea();
                        }
                        break;
                    case eGameDataType.Bool:
                        {
                            GUILayout.Label( "(Bool)", GUILayout.MaxWidth( widthTypeField ) );
                            selectedRecord.SetFieldData(
                                dataForm.Name,
                                EditorGUILayout.Toggle( selectedRecord.GetFieldData<bool>( dataForm.Name ) ) );
                        }
                        break;
                    default:
                        GUILayout.Label( "알 수 없는 형식의 필드" );
                        break;
                }

                GUILayout.EndHorizontal();
                GUILayout.Space( heightBetweenRecord );
                heightBetweenRecord = defaultHeightBetweenRecord;
            }
            // foreach( int fieldID in targetTableInfo.FieldIDs() )
            EditorGUILayout.EndScrollView();
        }

        void ClearCheckBoxes()
        {
            for( int i = 0; i < s_recordCountPerPage; i++ )
            {
                m_selectionFlags[ i ] = false;
            }
        }

        // 테이블 설정 정보 저장.
        void SaveTableInfoes()
        {
            System.Text.StringBuilder jsonBuilder = new System.Text.StringBuilder( 1024 );

            int tableCount = _tableInfoes.Values.Count;
            int currentTableIndex = 0;

            jsonBuilder.Append( "[\n" );
            foreach( GameDataTableInfo tableInfo in _tableInfoes.Values )
            {
                jsonBuilder.Append( "\t{\n" );

                jsonBuilder.Append( "\t\t\"Table\" : " );
                jsonBuilder.Append( "\"" );
                jsonBuilder.Append( tableInfo.TableName );
                jsonBuilder.Append( "\"\n" );

                int currentFieldIndex = 0;
                int fieldCount = tableInfo.FieldIDs().Count;
                jsonBuilder.Append( "\t\t\"Form\" : [\n" );
                foreach( int fieldID in tableInfo.FieldIDs() )
                {
                    jsonBuilder.Append( "\t\t\t{\n" );

                    GameInfoTableField dataForm = tableInfo.Field( fieldID );

                    jsonBuilder.Append( "\t\t\t\t\"Name\" : \"" );
                    jsonBuilder.Append( dataForm.Name );
                    jsonBuilder.Append( "\"\n" );

                    int typeID = ( int )dataForm.DataType;
                    jsonBuilder.Append( "\t\t\t\t\"Type\" : \"" );
                    jsonBuilder.Append( typeID.ToString() );
                    jsonBuilder.Append( "\"\n" );

                    if( dataForm.Additive.Length > 0 )
                    {
                        jsonBuilder.Append( "\t\t\t\t\"Addative\" : \"" );
                        jsonBuilder.Append( dataForm.Additive );
                        jsonBuilder.Append( "\"\n" );
                    }

                    currentFieldIndex++;
                    if( currentFieldIndex < fieldCount )
                    {
                        jsonBuilder.Append( "\t\t\t},\n" );
                    }
                    else
                    {
                        jsonBuilder.Append( "\t\t\t}\n" );
                    }

                    if( _selectedTable != null
                        && _selectedTable.TableName == tableInfo.TableName )
                    {
                        if( !_selectedTable.ExistField( dataForm.Name ) )
                        {
                            _selectedTable.AddFieldToAllRecord( dataForm );
                        }
                    }
                }
                jsonBuilder.Append( "\t\t]\n" );

                currentTableIndex++;
                if( currentTableIndex < tableCount )
                {
                    jsonBuilder.Append( "\t},\n" );
                }
                else
                {
                    jsonBuilder.Append( "\t}\n" );
                }
            }
            jsonBuilder.Append( "]\n" );

            string tableInfoPath = "Assets/Resources/GameInfoTable/table_setting.txt";
            System.IO.StreamWriter fileToWrite = new System.IO.StreamWriter(
                tableInfoPath,
                false,
                System.Text.Encoding.Unicode );
            fileToWrite.Write( jsonBuilder.ToString() );
            fileToWrite.Close();

            jsonBuilder = null;
        }

        // 테이블 설정 정보 불러오기.
        void LoadTableInfoes()
        {
            string tableInfoPath = "GameInfoTable/table_setting";
            TextAsset tableInfoJSON = Resources.Load<TextAsset>( tableInfoPath );
            if( null == tableInfoJSON )
            {
                Debug.LogError( "table info not found - " + tableInfoPath );
                return;
            }

            string tableSettingJSON = tableInfoJSON.text;
            List<object> tableInfoes =
                ( List<object> )MiniJSON.Json.Deserialize( tableSettingJSON );

            foreach( object tableInfoObject in tableInfoes )
            {
                Dictionary<string, object> tableInfo = tableInfoObject as Dictionary<string, object>;

                GameDataTableInfo dataTableInfo = new GameDataTableInfo();
                dataTableInfo.SetTableName( tableInfo[ "Table" ] as string );

                List<object> formDataList = tableInfo[ "Form" ] as List<object>;
                foreach( object formDataObject in formDataList )
                {
                    Dictionary<string, object> formData = formDataObject as Dictionary<string, object>;
                    string dataTypeString = formData[ "Type" ] as string;
                    string fieldName = formData[ "Name" ] as string;
                    string addative = "";
                    if( formData.ContainsKey( "Addative" ) )
                    {
                        addative = formData[ "Addative" ] as string;
                    }
                    dataTableInfo.AddField( ( eGameDataType )int.Parse( dataTypeString ), fieldName, addative );
                }
                _tableInfoes.Add( dataTableInfo.TableName, dataTableInfo );
            }
        }

        void HandleShortCut()
        {
            // 텍스트 박스에 키 입력중이면, 단축키 효과 OFF.
            if( GUIUtility.keyboardControl != 0 )
            {
                if( Event.current.keyCode == KeyCode.Return )
                {
                    // 텍스트 필드 안의 포커스 없애기.		
                    GUIUtility.keyboardControl = 0;
                }
                return;
            }

            switch( Event.current.type )
            {
                case EventType.KeyDown:
                    {
                        switch( Event.current.keyCode )
                        {
                            case KeyCode.Escape:// 창 닫기.
                                {
                                    if( s_instance != null && !m_bReservedToClose )
                                    {
                                        EditorPrefs.SetFloat( "GameInfoEditorX", s_instance.position.x );
                                        EditorPrefs.SetFloat( "GameInfoEditorY", s_instance.position.y );
                                    }
                                    //EditorPrefs.SetInt( "GameInfoEditorLastProcessor", ( int )m_currentInfoType );
                                    //Debug.Log( EditorPrefs.GetFloat( "GameInfoEditorX" ) );
                                    //Debug.Log( EditorPrefs.GetFloat( "GameInfoEditorY" ) );
                                    m_bReservedToClose = true;
                                }
                                break;
                        }
                        break;
                    }
                    //case EventType.ScrollWheel: // 화면 줌 인/아웃.
                    //    {
                    //        // 위/아래로 선택 항목 조정
                    //        if( Event.current.delta.y > 0 )
                    //        {
                    //            GameInfoRecord nextRecord = _selectedTable.NextRecord( _selectedRecordIndex );
                    //            if( nextRecord != null )
                    //            {
                    //                this.ProcessOnRecordSelection( nextRecord.ID );
                    //            }
                    //        }
                    //        else
                    //        {
                    //            GameInfoRecord prevRecord = _selectedTable.PreviousRecord( _selectedRecordIndex );
                    //            if( prevRecord != null )
                    //            {
                    //                this.ProcessOnRecordSelection( prevRecord.ID );                            
                    //            }
                    //            else
                    //            {
                    //                _selectedRecordIndex = _selectedTable.SetPreviousPage( s_recordCountPerPage );                            
                    //            }                        
                    //        }
                    //        this.Repaint();
                    //    }
                    //    break;
            }
        }        

        public System.Type GetTypeIncludeSearchingAssembly( string typeName )
        {
            System.Type type = System.Type.GetType( typeName );

            // 타입 획득 실패 시, 아래와 같이 재시도.
            if( null == type )
            {
                System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach( System.Reflection.Assembly assembly in assemblies )
                {
                    type = assembly.GetType( typeName );
                    if( type != null )
                    {
                        break;
                    }
                }
            }
            return type;
        }
    }
}