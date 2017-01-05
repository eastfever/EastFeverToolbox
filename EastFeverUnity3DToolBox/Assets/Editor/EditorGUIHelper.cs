using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace EastFever
{
    static public partial class EditorGUIHelper
    {
        private static GUIStyle s_lineStyle = null;

        public static void Label( string labelString, float labelWidth, float indentSpace = 0f )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.EndHorizontal();
        }

        public static void TitleAndLabelInOneLine( string titleString, string labelString, float labelWidth, float indentSpace = 0f )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( titleString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );
            GUILayout.Label( labelString );
            GUILayout.EndHorizontal();
        }

        public static bool ToggleField( string labelString, float labelWidth, bool oldFlag, float indentSpace = 0f )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );
            bool newFlag = EditorGUILayout.Toggle( oldFlag );
            GUILayout.EndHorizontal();

            return newFlag;
        }

        public static int IntField(
            string labelString,
            float labelWidth,
            int oldValue,
            float indentSpace = 0f,
            System.Action onButton = null,
            string buttonString = "삭제" )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );
            int newValue = EditorGUILayout.IntField( oldValue );

            if( onButton != null )
            {
                if( GUILayout.Button( buttonString, GUILayout.Width( 40f ) ) )
                {
                    onButton();
                }
            }
            GUILayout.EndHorizontal();

            return newValue;
        }

        public static void MinMaxIntField( string labelString, float labelWidth, ref int minValue, ref int maxValue, float indentSpace = 0f )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );
            minValue = EditorGUILayout.IntField( minValue );
            GUILayout.Label( " ~ ", GUILayout.Width( 20f ) );
            maxValue = EditorGUILayout.IntField( maxValue );
            GUILayout.EndHorizontal();

            if( minValue > maxValue )
            {
                int temp = maxValue;
                maxValue = minValue;
                minValue = temp;
            }
        }

        public static void MinMaxFloatField( string labelString, float labelWidth, ref float minValue, ref float maxValue, float indentSpace = 0f )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );
            minValue = EditorGUILayout.FloatField( minValue );
            GUILayout.Label( " ~ ", GUILayout.Width( 40f ) );
            maxValue = EditorGUILayout.FloatField( maxValue );
            GUILayout.EndHorizontal();

            if( minValue > maxValue )
            {
                float temp = maxValue;
                maxValue = minValue;
                minValue = temp;
            }
        }

        public static float FloatField( string labelString, float labelWidth, float oldValue, float indentSpace = 0f )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );
            float newValue = EditorGUILayout.FloatField( oldValue );
            GUILayout.EndHorizontal();

            return newValue;
        }

        public static string TextField( string labelString, float labelWidth, string oldString, float indentSpace = 0f )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );
            string newValue = EditorGUILayout.TextField( oldString );
            GUILayout.EndHorizontal();

            return newValue;
        }

        public static string TextFieldWithButton(
            string labelString,
            float labelWidth,
            string oldString,
            string buttonString,
            System.Action onButton,
            float indentSpace = 0f )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );
            string newValue = EditorGUILayout.TextField( oldString );
            if( GUILayout.Button( buttonString, GUILayout.Width( 40f ) ) )
            {
                onButton();
            }
            GUILayout.EndHorizontal();

            return newValue;
        }

        public static T ObjectField<T>(
            string labelString,
            float labelWidth,
            T targetObject,
            bool allowSceneObject,
            float indentSpace = 0f ) where T : UnityEngine.Object
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );
            T newObject = EditorGUILayout.ObjectField(
                targetObject,
                typeof( T ),
                allowSceneObject ) as T;
            GUILayout.EndHorizontal();

            return newObject;
        }

        public static T ObjectFieldWithButton<T>(
            string labelString,
            float labelWidth,
            T targetObject,
            bool allowSceneObject,
            string buttonName,
            System.Action onButton,
            float indentSpace = 0f ) where T : UnityEngine.Object
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );
            T newObject = EditorGUILayout.ObjectField(
                targetObject,
                typeof( T ),
                allowSceneObject ) as T;
            if( GUILayout.Button( buttonName, GUILayout.Width( 40f ) ) )
            {
                onButton();
            }
            GUILayout.EndHorizontal();

            return newObject;
        }

        public static T ObjectFieldWithDeleteButton<T>(
            string labelString,
            float labelWidth,
            T targetObject,
            bool allowSceneObject,
            System.Action onButton,
            float indentSpace = 0f ) where T : UnityEngine.Object
        {
            return EditorGUIHelper.ObjectFieldWithButton<T>(
                labelString,
                labelWidth,
                targetObject,
                allowSceneObject,
                "삭제",
                onButton,
                indentSpace );
        }

        public static string FolderField( string labelString, float labelWidth, string folderPath, float indentSpace = 0f )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );

            // 선택한 항목이 폴더인지 체크.
            string oldFolderPath = folderPath;
            System.IO.FileAttributes file_attr;
            try
            {
                if( oldFolderPath != "Assets" )
                {
                    string oldFolderAbsPath = Application.dataPath + "/" + folderPath.Replace( "Assets/", "" );
                    file_attr = System.IO.File.GetAttributes( oldFolderAbsPath );
                    if( ( file_attr & System.IO.FileAttributes.Directory ) != System.IO.FileAttributes.Directory )
                    {
                        oldFolderPath = "";
                    }
                }
            }
            catch
            {
                return "";
            }

            UnityEngine.Object oldFolder = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( oldFolderPath );
            UnityEngine.Object newFoloder = EditorGUILayout.ObjectField(
                oldFolder,
                typeof( UnityEngine.Object ),
                false ) as UnityEngine.Object;
            GUILayout.EndHorizontal();

            // 선택한 항목이 폴더인지 체크.
            string newFolderPath = AssetDatabase.GetAssetPath( newFoloder );
            try
            {
                if( newFolderPath != "Assets" )
                {
                    string newFolderAbsPath = Application.dataPath + "/" + newFolderPath.Replace( "Assets/", "" );
                    file_attr = System.IO.File.GetAttributes( newFolderAbsPath );
                    if( ( file_attr & System.IO.FileAttributes.Directory ) != System.IO.FileAttributes.Directory )
                    {
                        newFolderPath = "";
                    }
                }
            }
            catch
            {
                return "";
            }
            return newFolderPath;
        }

        public static System.Enum EnumField(
            string labelString,
            float labelWidth,
            System.Enum oldEnumValue,
            float indentSpace = 0f )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );
            System.Enum newEnumValue = EditorGUILayout.EnumPopup( oldEnumValue );
            GUILayout.EndHorizontal();

            return newEnumValue;
        }

        public static int StringList(
            string labelString,
            float labelWidth,
            int oldValue,
            string[] stringsInList,
            float indentSpace = 0f )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );
            int newValue = EditorGUILayout.Popup( oldValue, stringsInList );
            GUILayout.EndHorizontal();

            return newValue;
        }

        public static System.Enum EnumFieldWithDeleteButton<T>(
            string labelString,
            float labelWidth,
            System.Enum oldEnumValue,
            System.Action onButton,
            float indentSpace = 0f )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );
            System.Enum newEnumValue = EditorGUILayout.EnumPopup( oldEnumValue );
            if( GUILayout.Button( "삭제", GUILayout.Width( 40f ) ) )
            {
                onButton();
            }
            GUILayout.EndHorizontal();

            return newEnumValue;
        }

        public static Color ColorField( string labelString, float labelWidth, Color targetColor, float indentSpace = 0f )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );
            Color newColor = EditorGUILayout.ColorField( targetColor );
            GUILayout.EndHorizontal();

            return newColor;
        }

        public static void Vector3Field( string labelString, float labelWidth, ref Vector3 targetVector3, float indentSpace = 0f )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( indentSpace );
            GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            GUILayout.Space( 5f );

            GUILayout.Label( "X : ", GUILayout.Width( 30f ) );
            targetVector3.x = EditorGUILayout.FloatField( targetVector3.x, GUILayout.Width( 40f ) );
            GUILayout.Label( "Y : ", GUILayout.Width( 30f ) );
            targetVector3.y = EditorGUILayout.FloatField( targetVector3.y, GUILayout.Width( 40f ) );
            GUILayout.Label( "Z : ", GUILayout.Width( 30f ) );
            targetVector3.z = EditorGUILayout.FloatField( targetVector3.z, GUILayout.Width( 40f ) );

            GUILayout.EndHorizontal();
        }

        public static void LineSeperator( float space = 2f )
        {
            GUILayout.BeginVertical();
            GUILayout.Space( space );
            if( null == s_lineStyle )
            {
                s_lineStyle = new GUIStyle( "box" );
                s_lineStyle.border.top = s_lineStyle.border.bottom = 1;
                s_lineStyle.margin.top = s_lineStyle.margin.bottom = 1;
                s_lineStyle.padding.top = s_lineStyle.padding.bottom = 1;
            }
            GUILayout.Box( GUIContent.none, s_lineStyle, GUILayout.ExpandWidth( true ), GUILayout.Height( 1f ) );
            GUILayout.Space( space );
            GUILayout.EndVertical();
        }

        public static void BasicDataTypeListField<T>( string labelString, float labelWidth, List<T> targetList, float indentSpace = 0f )
        {
            //GUILayout.BeginHorizontal();
            //GUILayout.Space( indentSpace );
            //GUILayout.Label( labelString, GUILayout.Width( labelWidth ) );
            //if( GUILayout.Button( "항목 추가" ) )
            //{
            //	if( typeof( T ) == typeof( int ) )
            //	{
            //		List<int> intList = targetList as List<int>;
            //		intList.Add( 0 );
            //	}
            //	else if( typeof( T ) == typeof( float ) )
            //	{
            //		List<float> floatList = targetList as List<float>;
            //		floatList.Add( 0f );
            //	}
            //	else if( typeof( T ) == typeof( string ) )
            //	{
            //		List<string> stringList = targetList as List<string>;
            //		stringList.Add( "" );
            //	}			
            //}
            //GUILayout.EndHorizontal();

            //foreach( T listObject in targetList )
            //{
            //	GUILayout.BeginHorizontal();
            //	if( typeof( T ) == typeof( int ) )
            //	{

            //	}
            //	else if( typeof( T ) == typeof( float ) )
            //	{

            //	}
            //	else if( typeof( T ) == typeof( string ) )
            //	{

            //	}
            //	GUILayout.EndHorizontal();
            //}
        }


        static public void Foldout( ref bool foldout, string title, Action foldoutAction )
        {
            foldout = EditorGUILayout.Foldout( foldout, title );

            if( foldout == true )
            {
                if( null != foldoutAction )
                    foldoutAction();
            }
        }
        enum GroupTypes
        {
            Horizontal,
            Vertical
        }

        static public void GroupHorizon( Action groupAction, GUILayoutOption option = null )
        {
            group( GroupTypes.Horizontal, groupAction, option );
        }

        static public void GroupVertical( Action groupAction, GUILayoutOption option = null )
        {
            group( GroupTypes.Vertical, groupAction, option );
        }

        static public void PrefixButton( string prefix, string btnTitle, Action buttonAction )
        {
            group( GroupTypes.Horizontal, () =>
            {
                EditorGUILayout.PrefixLabel( prefix );
                if( GUILayout.Button( btnTitle ) )
                {
                    if( null != buttonAction )
                        buttonAction();
                }
            } );
        }

        static private void group( GroupTypes type, Action groupAction, GUILayoutOption option = null )
        {
            switch( type )
            {
                case GroupTypes.Horizontal:
                    {
                        if( null != option )
                            EditorGUILayout.BeginHorizontal( EditorStyles.helpBox, option );
                        else
                            EditorGUILayout.BeginHorizontal( EditorStyles.helpBox );
                        if( null != groupAction )
                            groupAction();
                        EditorGUILayout.EndHorizontal();
                    }
                    break;
                case GroupTypes.Vertical:
                    {
                        if( null != option )
                            EditorGUILayout.BeginVertical( EditorStyles.helpBox, option );
                        else
                            EditorGUILayout.BeginVertical( EditorStyles.helpBox );
                        if( null != groupAction )
                            groupAction();
                        EditorGUILayout.EndVertical();
                    }
                    break;
            }
        }

        static public string[] GetPrefabNames( string directoryPath, string searchPattern = "*.prefab" )
        {
            return Directory.GetFiles( directoryPath, searchPattern ).Select( file =>
            {
                return Path.GetFileNameWithoutExtension( file );
            } ).ToArray();
        }
    }
}