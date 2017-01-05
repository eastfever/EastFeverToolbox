using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EastFever
{
    // eGameDataType에 타입 추가 하기.
    // 1. eGameDataType에 신규 타입 추가
    // 2. GameInfoEditor::ProcessTableRecordGUI()에 EditorGUILayout추가.   
    // 3. GameInfoRecord::CopyData()에 AddField추가
    // 4. GameInfoTable::SaveTable()에 jsonBuilder.Append추가.
    // 5. GameInfoTable::LoadTable()에 refinedData 추가.
    // 6. GameInfoTable::AddFieldToRecord()에 AddField 추가.
    public enum eGameDataType
    {
        Int = 0,
        Float = 1,
        Text = 2,
        Vector2 = 3,
        Vector3 = 4,
        UnityObject = 5,
        Color = 6,
        Enum = 7,
        EnumFlag = 8,
        Bool = 9,
    }

    public class GameInfoTableField
    {
        public int DataID = 0;
        public eGameDataType DataType = eGameDataType.Int;
        public string Name = "";
        public string Additive = ""; // 추가 정보.
    }

    // 게임 정보 테이블의 구성 방식( 테이블들은 어떤 필드로 이루어져 있는가 )을 기록/반환 한다.
    public class GameDataTableInfo
    {
        private static int s_newFieldID = 1;

        private string _tableName = "";

        // 테이블을 구성하는 데이터 형식
        private Dictionary<int, GameInfoTableField> _dataFormTable = new Dictionary<int, GameInfoTableField>();

        public string TableName { get { return _tableName; } }
        public void SetTableName( string tableName ) { _tableName = tableName; }

        public ICollection<int> FieldIDs()
        {
            return _dataFormTable.Keys;
        }

        public ICollection<GameInfoTableField> FieldList()
        {
            return _dataFormTable.Values;
        }

        public GameInfoTableField Field( int fieldID )
        {
            if( _dataFormTable.ContainsKey( fieldID ) )
            {
                return _dataFormTable[ fieldID ];
            }
            return null;
        }

        public void AddField( eGameDataType fieldType, string fieldName, string addative )
        {
            GameInfoTableField newDataForm = new GameInfoTableField();
            newDataForm.DataID = s_newFieldID++;
            newDataForm.DataType = fieldType;
            newDataForm.Name = fieldName;
            newDataForm.Additive = addative;
            _dataFormTable.Add( newDataForm.DataID, newDataForm );
        }

        public void RemoveField( int fieldID )
        {
            if( _dataFormTable.ContainsKey( fieldID ) )
            {
                _dataFormTable.Remove( fieldID );
            }
        }
    }
}