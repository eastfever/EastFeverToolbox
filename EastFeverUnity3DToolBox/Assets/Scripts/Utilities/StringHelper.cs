using UnityEngine;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace EastFever
{
    public static class StringHelper
    {
        // 문자열 안에서 숫자만 뽑아서 반환.
        public static string FilterNumbers( string unformattedNumber )
        {
            string numberExtractorExpression = @"(\d+\.?\d*|\.\d+)";

            MatchCollection formattedNumber = Regex.Matches( unformattedNumber, numberExtractorExpression );

            StringBuilder numbers = new StringBuilder();

            for( int i = 0; i != formattedNumber.Count; ++i )
            {
                numbers.Append( formattedNumber[ i ].Value );
            }

            return numbers.ToString();
        }

        // 문자열 안에서 숫자 제거.
        public static string RemoveNumbers( string targetString )
        {
            return Regex.Replace( targetString, @"\d", "" );
        }

        // 애셋 경로를 리소스 기준 경로로 변환.
        public static string GetResourcePath( string originPath )
        {
            return originPath.Replace( "Assets/Resources/", "" );
        }

        // 애셋 경로를 리소스 기준 경로로 변환. 파일 확장자 제거.
        public static string GetResourcePathWithoutExtension( string originPath )
        {
            string[] tokens = StringHelper.GetResourcePath( originPath ).Split( '.' );
            return tokens[ 0 ];
        }

        // 숫자를 받아서 세 자리 단위마다 콤마를 삽입한 문자열을 반환.
        /// <summary>
        /// 숫자를 받아서 세 자리 단위마다 콤마를 삽입한 문자열을 반환. <para/>=> <see cref="int"/> 의 확장 메서드에서 NumberString 메서드 사용 추천
        /// </summary>
        [System.Obsolete]
        public static string GetNumberStringWithMonetaryExpression( int targetNumber )
        {
            return string.Format( "{0:N0}", targetNumber );
        }

        // 밀리초를 받아서 mm:ss:ff 형식의 문자열로 반환.
        public static string GetTimeStringByMilliSeconds( long milliSecends )
        {
            long minutes = milliSecends / 60000;
            long seconds = ( milliSecends - ( 60000 * minutes ) ) / 1000;
            long millies = ( milliSecends - ( 60000 * minutes ) - ( 1000 * seconds ) ) / 10;

            return string.Format( "{0:00}:{1:00}:{2:00}", minutes, seconds, millies );
        }

        public static string GetTimeStringBySeconds( int seconds )
        {
            int hours = seconds / 3600;
            int minutes = ( seconds - ( hours * 3600 ) ) / 60;
            int remainSeconds = seconds % 60;

            return string.Format( "{0:00}:{1:00}:{2:00}", hours, minutes, remainSeconds );
        }

        // 공백 제거.
        public static string DeleteSpace( string _str )
        {
            int num = 0;
            string tmp = _str;
            while( tmp.IndexOf( " " ) > 0 )
            {
                num = tmp.IndexOf( " " );
                string tmp1 = tmp.Substring( 0, num );
                tmp1 += tmp.Substring( num + 1 );
                tmp = tmp1;
            }
            return tmp;
        }

        // 벡터 문자열을 벡터로 교체.
        public static Vector2 Vector2FromString( string vectorString )
        {
            vectorString = vectorString.Replace( "(", "" );
            vectorString = vectorString.Replace( ")", "" );
            string[] tokens = vectorString.Split( ',' );

            Vector2 newVector = Vector2.zero;
            newVector.x = float.Parse( tokens[ 0 ] );
            newVector.y = float.Parse( tokens[ 1 ] );

            return newVector;
        }

        // 벡터 문자열을 벡터로 교체.
        public static Vector3 Vector3FromString( string vectorString )
        {
            vectorString = vectorString.Replace( "(", "" );
            vectorString = vectorString.Replace( ")", "" );
            string[] tokens = vectorString.Split( ',' );

            Vector3 newVector = Vector3.zero;
            try
            {
                newVector.x = float.Parse( tokens[ 0 ] );
                newVector.y = float.Parse( tokens[ 1 ] );
                newVector.z = float.Parse( tokens[ 2 ] );
            }
            catch
            {
                Debug.LogError( "[TileMapEditor]Vector3 string parsing error - " + vectorString );
            }
            return newVector;
        }        

        // 컬러 문자열을 컬러로 교체.
        public static Color ColorFromString( string colorString )
        {
            colorString = colorString.Replace( "RGBA(", "" );
            colorString = colorString.Replace( ")", "" );
            string[] tokens = colorString.Split( ',' );

            Color newColor = new Color();
            try
            {
                newColor.r = float.Parse( tokens[ 0 ] );
                newColor.g = float.Parse( tokens[ 1 ] );
                newColor.b = float.Parse( tokens[ 2 ] );
                newColor.a = float.Parse( tokens[ 3 ] );
            }
            catch
            {
                Debug.LogError( "[TileMapEditor]Color string parsing error - " + colorString );
            }
            return newColor;
        }

        public static string ColorToHex( Color32 color )
        {
            string hex = color.r.ToString( "X2" ) + color.g.ToString( "X2" ) + color.b.ToString( "X2" );
            return hex;
        }

        public static Color HexToColor( string hex )
        {
            try
            {
                hex = hex.Replace( "0x", "" );//in case the string is formatted 0xFFFFFF
                hex = hex.Replace( "#", "" );//in case the string is formatted #FFFFFF
                byte a = 255;//assume fully visible unless specified in hex
                byte r = byte.Parse( hex.Substring( 0, 2 ), System.Globalization.NumberStyles.HexNumber );
                byte g = byte.Parse( hex.Substring( 2, 2 ), System.Globalization.NumberStyles.HexNumber );
                byte b = byte.Parse( hex.Substring( 4, 2 ), System.Globalization.NumberStyles.HexNumber );
                //Only use alpha if the string has enough characters
                if( hex.Length == 8 )
                {
                    a = byte.Parse( hex.Substring( 4, 2 ), System.Globalization.NumberStyles.HexNumber );
                }
                return new Color32( r, g, b, a );
            }
            catch( System.FormatException e )
            {
                Debug.Log( e.ToString() );
            }
            return new Color( 1f, 1f, 1f );
        }

        // BASE64 생성.
        public static string EncodeBase64( string source )
        {
            System.Text.Encoding encoding = new System.Text.UTF8Encoding();
            byte[] bytes = encoding.GetBytes( source );
            return System.Convert.ToBase64String( bytes );
        }

        // BASE64해석.
        public static string DecodeBase64( string encoded )
        {
            System.Text.Encoding encoding = new System.Text.UTF8Encoding();
            byte[] bytes = System.Convert.FromBase64String( encoded );
            return encoding.GetString( bytes );
        }
    }
}


