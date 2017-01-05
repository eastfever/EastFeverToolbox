using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace EastFever
{
    // 게임 정보의 한 단위를 이루는 레코드로 구성된 데이터 테이블.
    public class GameInfoTable
    {
        private readonly string DEFAULT_SAVE_PATH = "Assets/Resources/GameInfoTable";

        private string _tableName = "";        

        private List<GameInfoTableField> _fieldInfoes = null;
        private Dictionary<int, GameInfoRecord> _recordTable = new Dictionary<int, GameInfoRecord>();

        public GameInfoTable( string tableName, ICollection<GameInfoTableField> fieldInfoes, bool skipLoadFromJson = false )
        {
            _tableName = tableName;

            if( !skipLoadFromJson )
            {
                LoadTable( fieldInfoes );
            }

            _fieldInfoes = new List<GameInfoTableField>( fieldInfoes );
        }

        public string TableName { get { return _tableName; } }
        public void SetTableName( string newName ) { _tableName = newName; }

        public int RecordCount() { return _recordTable.Count; }
        public List<GameInfoTableField> FieldInfoes() { return _fieldInfoes; }

        public GameInfoRecord GetRecord( int ID )
        {
            if( _recordTable.ContainsKey( ID ) )
            {
                return _recordTable[ ID ];
            }

            //Debug.LogError( "[GameInfoTable]not found record - " + ID );
            return null;
        }

        // 호출될때마다 상당한 메모리 소모와 CPU연산이 발생하니 간헐적으로 사용할 것.
        public List<GameInfoRecord> SortedRecordList
        {
            get
            {
                List<GameInfoRecord> recordList = new List<GameInfoRecord>( _recordTable.Values );
                recordList.Sort( 
                    ( a, b ) =>
                    {
                        if( a.ID < b.ID )
                        {
                            return -1;
                        }
                        else if( a.ID > b.ID )
                        {
                            return 1;
                        }
                        return 0;
                    } );
                return recordList;
            }
        }

        public int AddRecord()
        {
            int newID = _recordTable.Count;
            if( newID <= 0 )
            {
                newID = 1;
            }
            while( _recordTable.ContainsKey( newID ) )
            {
                newID++;
            }

            GameInfoRecord newRecord = new GameInfoRecord( newID, "New Record", this );
            foreach( GameInfoTableField fieldInfo in _fieldInfoes )
            {
                this.AddFieldToRecord( newRecord, fieldInfo );
            }
            _recordTable.Add( newID, newRecord );

            return newID;
        }

        public int CopyRecord( int sourceID )
        {
            if( !_recordTable.ContainsKey( sourceID ) )
            {
                Debug.LogError( "[GameInfoTable]copy source id not found - " + sourceID );
                return -1;
            }

            int newID = _recordTable.Count;
            while( _recordTable.ContainsKey( newID ) )
            {
                newID++;
            }

            GameInfoRecord sourceRecord = _recordTable[ sourceID ];
            GameInfoRecord newRecord = new GameInfoRecord( newID, "New Record", this );
            newRecord.CopyData( sourceRecord );

            _recordTable.Add( newID, newRecord );
            return newID;
        }

        public void DeleteRecord( int IDToDelete )
        {
            if( !_recordTable.ContainsKey( IDToDelete ) )
            {
                Debug.LogError( "[GameInfoTable]record to delete not found - " + IDToDelete );
                return;
            }
            _recordTable.Remove( IDToDelete );
        }

        // 레코드 ID를 변경한다. 
        public bool ExchangeID( int oldID, int newID )
        {
            if( !_recordTable.ContainsKey( oldID ) )
            {
                Debug.LogError( "[ExchangeID]record not found - " + oldID );
                return false;
            }

            if( _recordTable.ContainsKey( newID ) )
            {
                Debug.LogError( "[ExchangeID]new ID already exist - " + newID );
                return false;
            }

            GameInfoRecord targetRecord = _recordTable[ oldID ];
            GameInfoRecord newRecord = new GameInfoRecord( newID, targetRecord, this );

            _recordTable.Add( newID, newRecord );
            _recordTable.Remove( oldID );
            return true;
        }

        // 주어진 이름의 필드가 레코드에 등록되어 있는지 확인한다.
        // 대표 레코드 하나에 한해서만 검사한다.
        public bool ExistField( string fieldName )
        {
            if( _recordTable.Count < 0 )
            {
                return false;
            }

            GameInfoRecord headRecord = null;
            foreach( GameInfoRecord record in _recordTable.Values )
            {
                headRecord = record;
                break;
            }

            if( null == headRecord )
            {
                return false;
            }

            return headRecord.ExistField( fieldName );
        }

        // 테이블에 속해 있는 모든 레코드에 필드 추가
        public void AddFieldToAllRecord( GameInfoTableField fieldInfo )
        {
            foreach( GameInfoRecord record in _recordTable.Values )
            {
                this.AddFieldToRecord( record, fieldInfo );
            }
        }

        // 테이블 스키마 변경. 테이블 구성 필드가 추가되거나 삭제 될 때 사용.
        public void ChangeSchema( GameDataTableInfo tableInfo )
        {
            _fieldInfoes = new List<GameInfoTableField>( tableInfo.FieldList() );
            foreach( GameInfoRecord record in _recordTable.Values )
            {
                record.RefreshSchema();
            }
        }

        // 리소스 폴더에 테이블 정보를 저장한다.
        public void SaveTable( ICollection<GameInfoTableField> fieldInfoes )
        {
            if( !Directory.Exists( DEFAULT_SAVE_PATH ) )
            {
                Directory.CreateDirectory( DEFAULT_SAVE_PATH );
            }

            string pathToSave = DEFAULT_SAVE_PATH + "/" + _tableName + ".txt";
            System.Text.StringBuilder jsonBuilder = new System.Text.StringBuilder( 4096 );

            if( null == jsonBuilder )
            {
                Debug.LogError( "Table save failed - " + pathToSave );
                return;
            }

            jsonBuilder.Append( "[\n" );

            int i = 0;
            foreach( int ID in _recordTable.Keys )
            {
                GameInfoRecord record = _recordTable[ ID ];

                jsonBuilder.Append( "\t{\n" );

                // 레코드 ID
                jsonBuilder.Append( "\t\t\"ID\" : \"" );
                jsonBuilder.Append( ID.ToString() );
                jsonBuilder.Append( "\"\n" );

                // 레코드 이름
                jsonBuilder.Append( "\t\t\"Name\" : \"" );
                jsonBuilder.Append( record.Name );
                jsonBuilder.Append( "\"\n" );

                foreach( GameInfoTableField field in fieldInfoes )
                {
                    // 필드 이름
                    jsonBuilder.Append( "\t\t\"" );
                    jsonBuilder.Append( field.Name );
                    jsonBuilder.Append( "\"" );
                    jsonBuilder.Append( " : " );

                    // 필드 내용
                    jsonBuilder.Append( "\"" );
                    switch( field.DataType )
                    {
                        case eGameDataType.Int:
                            jsonBuilder.Append( record.GetFieldData<int>( field.Name ).ToString() );
                            break;
                        case eGameDataType.Float:
                            jsonBuilder.Append( record.GetFieldData<float>( field.Name ).ToString() );
                            break;
                        case eGameDataType.Text:
                            jsonBuilder.Append( record.GetFieldData<string>( field.Name ) );
                            break;
                        case eGameDataType.Vector2:
                            jsonBuilder.Append( record.GetFieldData<Vector2>( field.Name ).ToString() );
                            break;
                        case eGameDataType.Vector3:
                            jsonBuilder.Append( record.GetFieldData<Vector3>( field.Name ).ToString() );
                            break;
                        case eGameDataType.UnityObject:
                            jsonBuilder.Append( record.GetFieldData<string>( field.Name ) );
                            break;
                        case eGameDataType.Color:
                            jsonBuilder.Append( record.GetFieldData<Color>( field.Name ).ToString() );
                            break;
                        case eGameDataType.Enum:
                            jsonBuilder.Append( record.GetFieldData<string>( field.Name ) );
                            break;
                        case eGameDataType.EnumFlag:
                            jsonBuilder.Append( record.GetFieldData<int>( field.Name ) );
                            break;
                        case eGameDataType.Bool:
                            jsonBuilder.Append( record.GetFieldData<bool>( field.Name ) );
                            break;
                    }
                    jsonBuilder.Append( "\"\n" );
                }

                i++;
                if( i < _recordTable.Count )
                {
                    jsonBuilder.Append( "\t},\n" );
                }
                else
                {
                    jsonBuilder.Append( "\t}\n" );
                }
            }
            jsonBuilder.Append( "]\n" );

            StreamWriter fileToWrite = new StreamWriter(
                pathToSave,
                false,
                System.Text.Encoding.Unicode );
            fileToWrite.Write( jsonBuilder.ToString() );
            fileToWrite.Close();
        }

        // 리소스 폴더에 있는 테이블 정보 파일을 읽는다.
        public void LoadTable( ICollection<GameInfoTableField> fieldInfoes )
        {
            _recordTable.Clear();

            string tableInfoPath = "GameInfoTable/" + _tableName;
            TextAsset tableJSON = Resources.Load<TextAsset>( tableInfoPath );
            if( null == tableJSON )
            {
                Debug.LogError( "table info not found - " + tableInfoPath );
                return;
            }

            List<object> recordList =
                ( List<object> )MiniJSON.Json.Deserialize( tableJSON.text );

            foreach( object record in recordList )
            {
                Dictionary<string, object> recordObject = record as Dictionary<string, object>;
                int ID = -1;
                if( !recordObject.ContainsKey( "ID" ) )
                {
                    Debug.LogError( "[LoadTable]record id not defined - " + _tableName );
                    continue;
                }

                ID = int.Parse( recordObject[ "ID" ] as string );
                if( ID < 0 )
                {
                    Debug.LogError( "[LoadTable]record id invalid - " + _tableName + ", " + ID );
                    continue;
                }

                string Name = "";
                if( !recordObject.ContainsKey( "Name" ) )
                {
                    Debug.LogError( "[LoadTable]record name not defined - " + _tableName );
                    continue;
                }
                Name = recordObject[ "Name" ] as string;

                GameInfoRecord newRecord = new GameInfoRecord( ID, Name, this );
                foreach( GameInfoTableField tableField in fieldInfoes )
                {
                    if( !recordObject.ContainsKey( tableField.Name ) )
                    {
                        Debug.LogWarning( "[LoadTable]uncontained field in table - " + tableField.Name );
                        this.AddFieldToRecord( newRecord, tableField );
                        continue;
                    }

                    object refinedData = null;
                    switch( tableField.DataType )
                    {
                        case eGameDataType.Int:
                            refinedData = int.Parse( recordObject[ tableField.Name ] as string );
                            break;
                        case eGameDataType.Float:
                            refinedData = float.Parse( recordObject[ tableField.Name ] as string );
                            break;
                        case eGameDataType.Vector2:
                            string vector2String = recordObject[ tableField.Name ] as string;
                            refinedData = StringHelper.Vector2FromString( vector2String );
                            break;
                        case eGameDataType.Vector3:
                            string vector3String = recordObject[ tableField.Name ] as string;
                            refinedData = StringHelper.Vector3FromString( vector3String );
                            break;
                        case eGameDataType.Color:
                            string colorString = recordObject[ tableField.Name ] as string;
                            refinedData = StringHelper.ColorFromString( colorString );
                            break;
                        case eGameDataType.EnumFlag:
                            refinedData = int.Parse( recordObject[ tableField.Name ] as string );
                            break;
                        case eGameDataType.Bool:
                            refinedData = bool.Parse( recordObject[ tableField.Name ] as string );
                            break;
                        default:
                            refinedData = recordObject[ tableField.Name ];
                            break;
                    }

                    newRecord.AddField( tableField.Name, refinedData );
                }

                _recordTable.Add( ID, newRecord );
            }
        }

        public void AddFieldToRecord( GameInfoRecord targetRecord, GameInfoTableField fieldInfo )
        {
            switch( fieldInfo.DataType )
            {
                case eGameDataType.Int:
                    targetRecord.AddField( fieldInfo.Name, 0 );
                    break;
                case eGameDataType.Float:
                    targetRecord.AddField( fieldInfo.Name, 0.0f );
                    break;
                case eGameDataType.Text:
                    targetRecord.AddField( fieldInfo.Name, "" );
                    break;
                case eGameDataType.Vector2:
                    targetRecord.AddField( fieldInfo.Name, Vector2.zero );
                    break;
                case eGameDataType.Vector3:
                    targetRecord.AddField( fieldInfo.Name, Vector3.zero );
                    break;
                case eGameDataType.UnityObject:
                    targetRecord.AddField( fieldInfo.Name, "" );
                    break;
                case eGameDataType.Color:
                    targetRecord.AddField( fieldInfo.Name, Color.white );
                    break;
                case eGameDataType.Enum:
                    {
                        System.Type enumType = System.Type.GetType( fieldInfo.Additive );

                        // 타입 획득 실패 시, 아래와 같이 재시도.
                        if( null == enumType )
                        {
                            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                            foreach( System.Reflection.Assembly assembly in assemblies )
                            {
                                enumType = assembly.GetType( fieldInfo.Additive );
                                if( enumType != null )
                                {
                                    break;
                                }
                            }
                        }

                        System.Enum defaultEnumValue = ( System.Enum )System.Enum.Parse( enumType, "0" );
                        targetRecord.AddField( fieldInfo.Name, defaultEnumValue.ToString() );
                    }
                    break;
                case eGameDataType.EnumFlag:
                    targetRecord.AddField( fieldInfo.Name, 0 );
                    break;
                case eGameDataType.Bool:
                    targetRecord.AddField( fieldInfo.Name, false );
                    break;
            }
        }
    }
}