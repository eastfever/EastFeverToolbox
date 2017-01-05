using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

namespace EastFever
{
    // ���� ��� ����� �Լ� ����
    public class PathHelper
    {
        // ���� ���� ������ Ư�� Ȯ���ڸ� ���� ������ ��� ����Ʈ�� ��´�
        // Ȯ���ڿ� null�� �ָ� Ȯ���� ���� ���� ��� ������ ��� ����Ʈ�� ��´�.
        // Ȯ���ڿ� ""�� �ָ� ����� ���� �ȵǴ� ������ ��.
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

        // ���ҽ� ��θ� �ּ� ��η� ��ȯ.
        public static string AssetPathByResourcePath( string resourcePath )
        {
            return "Assets/Resources/" + resourcePath;
        }

        public static string GetResourcePath( string originPath )
        {
            return originPath.Replace( "Assets/Resources/", "" );
        }

        // �ּ� ��θ� ���ҽ� ���� ��η� ��ȯ�ϵ� ������ Ȯ���ڴ� ����
        public static string GetResourcePathWithoutExtension( string originPath )
        {
            string[] tokens = StringHelper.GetResourcePath( originPath ).Split( '.' );
            return tokens[ 0 ];
        }


        // �־��� ���� ��ο� �ش��ϴ� ���丮�� �ִ��� ã�ƺ��� ������ �����.
        public static void CreateDirectoryIfNotExist( string path )
        {
            string directoryPath = Path.GetDirectoryName( path );
            if( !Directory.Exists( directoryPath ) )
            {
                Directory.CreateDirectory( directoryPath );
            }
        }

        // �־��� ��ο� ������ �����Ѵ�.
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

            // ��� ��� ���̱�
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