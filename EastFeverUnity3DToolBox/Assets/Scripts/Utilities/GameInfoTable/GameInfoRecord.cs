using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EastFever
{
    // 게임 정보 테이블을 구성하는 레코드의 정의.
    public class GameInfoRecord
    {
        public static GameInfoRecordComparer RecordComparer = new GameInfoRecordComparer();

        private int _ID = 0;
        private string _Name = "";
        private GameInfoTable _parentTable = null;
        private Dictionary<string, object> _infoTable = new Dictionary<string, object>();

        public GameInfoRecord( int ID, string Name, GameInfoTable parent )
        {
            _ID = ID;
            _Name = Name;
            _parentTable = parent;
        }

        // 레코드 ID 변경에 사용. 얕은 복사이기 때문에 실제로 객체 복사 작업에 사용하는건 위험.
        public GameInfoRecord(
            int newID,
            GameInfoRecord recordToCopy,
            GameInfoTable parentTable )
        {
            _ID = newID;
            _Name = recordToCopy._Name;
            _infoTable = recordToCopy._infoTable;
            _parentTable = parentTable;
        }

        public int ID { get { return _ID; } }
        public string Name { get { return _Name; } }
        public List<string> KeyList { get { return new List<string>( _infoTable.Keys ); } }

        // 주의 : 테이블 내의 딕셔너리 관리 때문에,
        // 레코드 ID 변경은 GameInfoTable::ExchangeID를 사용할 것.
        //public void SetID( int newID ) { _ID = newID; }

        public void SetName( string newName ) { _Name = newName; }

        public void AddField( string fieldName, object fieldData )
        {
            _infoTable.Add( fieldName, fieldData );
        }

        public bool ExistField( string fieldName )
        {
            return _infoTable.ContainsKey( fieldName );
        }

        public Type GetFieldData<Type>( string fieldName )
        {
            if( !_infoTable.ContainsKey( fieldName ) )
            {
                Debug.LogError( "[GameInfoRecord]field not found - " + fieldName );
                return default( Type );
            }

            Type fieldData = default( Type );
            try
            {
                fieldData = ( Type )_infoTable[ fieldName ];
            }
            catch
            {
                Debug.LogError( "[GameInfoRecord]field type cast failed - " + fieldName );
                return default( Type );
            }
            return fieldData;
        }

        public EnumName GetEnumData<EnumName>( string fieldName )
        {
            if( !_infoTable.ContainsKey( fieldName ) )
            {
                Debug.LogError( "[GameInfoRecord]field not found - " + fieldName );
                return default( EnumName );
            }

            // 이넘 타입 처리.		
            System.Type requestedType = typeof( EnumName );
            return ( EnumName )System.Enum.Parse( requestedType, _infoTable[ fieldName ] as string );
        }

        // 이넘 플래그 타입의 값을 대상으로 주어진 값에 해당하는 플래그가 켜 있는지 확인.
        public bool IsEnableEnumFlag( string fieldName, int flagValue )
        {
            if( !_infoTable.ContainsKey( fieldName ) )
            {
                Debug.LogError( "[GameInfoRecord]field not found - " + fieldName );
                return false;
            }

            // 이넘 타입 처리.		
            int recordedValue = ( int )_infoTable[ fieldName ];
            if( ( recordedValue & flagValue ) != 0 )
            {
                return true;
            }
            return false;
        }

        public void ToggleEnumFlag( string fieldName, int flagValue, bool flag )
        {
            if( !_infoTable.ContainsKey( fieldName ) )
            {
                Debug.LogError( "[GameInfoRecord]field not found - " + fieldName );
                return;
            }

            // 이넘 타입 처리.				
            int recordedValue = ( int )_infoTable[ fieldName ];
            if( flag )
            {
                recordedValue |= flagValue;
            }
            else
            {
                recordedValue &= ~flagValue;
            }
            _infoTable[ fieldName ] = recordedValue;
        }

        public void SetFieldData( string fieldName, object fieldData )
        {
            if( !_infoTable.ContainsKey( fieldName ) )
            {
                Debug.LogError( "[GameInfoRecord]field not found - " + fieldName );
                return;
            }
            _infoTable[ fieldName ] = fieldData;
        }

        // _infoTable의 깊은 복사 실행.
        public void CopyData( GameInfoRecord source )
        {
            foreach( GameInfoTableField field in _parentTable.FieldInfoes() )
            {
                switch( field.DataType )
                {
                    case eGameDataType.Int:
                        AddField( field.Name, source.GetFieldData<int>( field.Name ) );
                        break;
                    case eGameDataType.Float:
                        AddField( field.Name, source.GetFieldData<float>( field.Name ) );
                        break;
                    case eGameDataType.Text:
                        AddField( field.Name, source.GetFieldData<string>( field.Name ) );
                        break;
                    case eGameDataType.Vector2:
                        AddField( field.Name, source.GetFieldData<Vector2>( field.Name ) );
                        break;
                    case eGameDataType.Vector3:
                        AddField( field.Name, source.GetFieldData<Vector3>( field.Name ) );
                        break;
                    case eGameDataType.UnityObject:
                        AddField( field.Name, source.GetFieldData<string>( field.Name ) );
                        break;
                    case eGameDataType.Color:
                        AddField( field.Name, source.GetFieldData<Color>( field.Name ) );
                        break;
                    case eGameDataType.Enum:
                        AddField( field.Name, source.GetFieldData<string>( field.Name ) );
                        break;
                    case eGameDataType.EnumFlag:
                        AddField( field.Name, source.GetFieldData<int>( field.Name ) );
                        break;
                    case eGameDataType.Bool:
                        AddField( field.Name, source.GetFieldData<bool>( field.Name ) );
                        break;
                }
            }
        }

        // 자신이 속해 있는 테이블 스키마를 확인해 보고 변경 사항이 있으면 적용한다.
        public void RefreshSchema()
        {
            // 사라진 스키마 필드를 찾기 위한 리스트.
            List<string> schemaNameListToDelete = new List<string>( _infoTable.Keys );

            foreach( GameInfoTableField field in _parentTable.FieldInfoes() )
            {
                if( _infoTable.ContainsKey( field.Name ) )
                {
                    // 테이블, 레코드 양쪽 다 스키마가 존재하므로 제거 대상에서 제외.
                    schemaNameListToDelete.Remove( field.Name );
                }
                else
                {
                    // 새로 추가된 스키마 적용.
                    _parentTable.AddFieldToRecord( this, field );
                }
            }

            // 이 리스트에 남은 필드들은 제거 대상.
            foreach( string fieldName in schemaNameListToDelete )
            {
                if( _infoTable.ContainsKey( fieldName ) )
                {
                    _infoTable.Remove( fieldName );
                }
            }
        }
    }

    public class GameInfoRecordComparer : IComparer<GameInfoRecord>
    {
        public int Compare( GameInfoRecord a, GameInfoRecord b )
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
        }
    }
}