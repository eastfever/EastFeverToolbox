using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

namespace EastFever
{
    // 파일 경로 도우미 함수 모음
    public class PathHelper
    {
        // 하위 폴더 내에서 특정 확장자를 갖는 파일의 경로 리스트를 얻는다
        // 확장자에 null을 주면 확장자 구분 없이 모든 파일의 경로 리스트를 얻는다.
        // 확장자에 ""를 주면 제대로 동작 안되니 조심할 것.
        public static List<string> GetPathListByFileExtension( string path, string extension, System.Text.RegularExpressions.Regex except = null )
        {
            string extensionString = "*." + extension;
            if( null == extension )
            {
                extensionString = null;
            }

            List<string> pathList = new List<string>();
            PathHelper.TraverseToGetExtensionPaths( ref pathList, path, extensionString, except );
            return pathList;
        }

        private static void TraverseToGetExtensionPaths(
            ref List<string> list,
            string path,
            string extension,
            System.Text.RegularExpressions.Regex except )
        {
            if( !Directory.Exists( path ) )
            {
                Debug.Log( "no exist - " + path );
                return;
            }

            if( except != null &&
                except.IsMatch( path ) )
            {
                return;
            }

            string[] files = null;
            if( extension != null )
            {
                files = Directory.GetFiles( path, extension );
            }
            else
            {
                files = Directory.GetFiles( path );
            }
            foreach( string filePath in files )
            {
                string refinedPath = filePath.Replace( "\\", "/" );
                list.Add( refinedPath );
                //Debug.Log( " + - " + refinedPath );
            }

            string[] dirs = Directory.GetDirectories( path );
            foreach( string dirPath in dirs )
            {
                string refinedPath = dirPath.Replace( "\\", "/" );
                PathHelper.TraverseToGetExtensionPaths( ref list, refinedPath, extension, except );
                //DebugLogger.Log( " * - " + refinedPath );
            }
        }

        // 리소스 경로를 애셋 경로로 변환.
        public static string AssetPathByResourcePath( string resourcePath )
        {
            return "Assets/Resources/" + resourcePath;
        }

        public static string GetResourcePath( string originPath )
        {
            return originPath.Replace( "Assets/Resources/", "" );
        }

        // 애셋 경로를 리소스 기준 경로로 변환하되 마지막 확장자는 생략
        public static string GetResourcePathWithoutExtension( string originPath )
        {
            string[] tokens = StringHelper.GetResourcePath( originPath ).Split( '.' );
            return tokens[ 0 ];
        }


        // 주어진 파일 경로에 해당하는 디렉토리가 있는지 찾아보고 없으면 만든다.
        public static void CreateDirectoryIfNotExist( string path )
        {
            string directoryPath = Path.GetDirectoryName( path );
            if( !Directory.Exists( directoryPath ) )
            {
                Directory.CreateDirectory( directoryPath );
            }
        }

        // 주어진 경로에 파일을 생성한다.
        public static bool ByteArrayToFile( string _FilePath, byte[] _ByteArray )
        {
            try
            {
                System.IO.FileStream _FileStream =
                   new System.IO.FileStream( _FilePath, System.IO.FileMode.Create,
                                            System.IO.FileAccess.Write );
                _FileStream.Write( _ByteArray, 0, _ByteArray.Length );
                _FileStream.Close();

                return true;
            }
            catch( Exception _Exception )
            {
                Debug.LogError( "Exception caught in process : " + _Exception.ToString() );
            }

            return false;
        }

        public static string GetFullPersistantPath( string relativePath, string fileName )
        {
            string fullPath = Application.persistentDataPath;

            // 상대 경로 붙이기
            if( relativePath.Length > 0 )
            {
                if( relativePath[ 0 ] != '/' )
                {
                    fullPath += "/";
                }
                fullPath += relativePath;
            }

            if( fileName.Length > 0 )
            {
                if( relativePath.Length > 0 )
                {
                    if( relativePath[ relativePath.Length - 1 ] != '/' )
                    {
                        fullPath += "/";
                    }
                }
            }

            fullPath += fileName;
            return fullPath;
        }

        public static bool IsExistFile( string filePath )
        {
            System.IO.FileInfo fileInfo = new FileInfo( filePath );
            return fileInfo.Exists;
        }
    }
}