using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EastFever
{
    public class GameInfoTableFactory
    {
        private static GameInfoTableFactory s_instance = null;
        public static GameInfoTableFactory Instance
        {
            get
            {
                if( null == s_instance )
                {
                    s_instance = new GameInfoTableFactory();
                }
                return s_instance;
            }
        }

        private Dictionary<string, GameDataTableInfo> _tableInfoes = new Dictionary<string, GameDataTableInfo>();
        private Dictionary<string, GameInfoTable> _cachedTables = new Dictionary<string, GameInfoTable>();

        private bool _initialized = false;

        public void Reset()
        {
            _initialized = false;
            _tableInfoes.Clear();
        }

        public void Initialize()
        {
            _initialized = true;

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

        public GameInfoTable CreateGameInfoTable( string tableName, bool UseCache = true )
        {
            if( !_initialized )
            {
                this.Initialize();
            }

            if( !_tableInfoes.ContainsKey( tableName ) )
            {
                Debug.LogError( "[CreateGameInfoTable]not registered table name - " + tableName );
                return null;
            }

            if( UseCache )
            {
                if( _cachedTables.ContainsKey( tableName ) )
                {
                    return _cachedTables[ tableName ];
                }
            }

            GameDataTableInfo targetTableInfo = _tableInfoes[ tableName ];
            GameInfoTable newTable = new GameInfoTable( tableName, targetTableInfo.FieldList() );
            _cachedTables.Add( tableName, newTable );

            return newTable;
        }

        public void ClearCaches()
        {
            _cachedTables.Clear();
        }
    }
}