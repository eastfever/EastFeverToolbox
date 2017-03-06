using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace EastFever
{
    // C# 제공 기본 클래스 기능 확장을 위해 추가되는 확장 메서드 모음.
    public static class CSharpExtension
    {
        //  배열이 비어 있는지 String.IsNullOrEmpty스타일로 확인.
        public static bool IsNullOrEmpty( this Array target )
        {
            if( null == target )
            {
                return true;
            }
            if( target.Length == 0 )
            {
                return true;
            }
            return false;
        }

        //  리스트가 비어 있는지 String.IsNullOrEmpty스타일로 확인.
        public static bool IsNullOrEmpty( this ICollection target )
        {
            if( null == target )
            {
                return true;
            }
            if( target.Count == 0 )
            {
                return true;
            }
            return false;
        }

        // 문자열이 비어 있는지 확인.
        public static bool IsNullOrEmpty( this string target )
        {
            if( null == target )
            {
                return true;
            }
            if( target.Length == 0 )
            {
                return true;
            }
            return false;
        }

        // 주어진 인덱스가 배열의 범위를 초과하는지 검사.
        public static bool IsSafeIndex( this Array target, int index )
        {
            if( 0 <= index && index < target.Length )
            {
                return true;
            }
            return false;
        }
        public static bool IsSafeIndex( this ICollection target, int index )
        {
            if( 0 <= index && index < target.Count )
            {
                return true;
            }
            return false;
        }

        public static void Shuffle<T>( this List<T> target )
        {
            // 잘 섞이라고 리스트 항목 갯수의 두 배 만큼 섞는다.
            for( int shuffleCount = 0; shuffleCount < 2; shuffleCount++ )
            {
                for( int i = 0; i < target.Count; i++ )
                {
                    int selectedIndex = UnityEngine.Random.Range( 0, target.Count );
                    T temp = target[ i ];
                    target[ i ] = target[ selectedIndex ];
                    target[ selectedIndex ] = temp;
                }
            }
        }

        // 단일 오브젝트를 배열에 담기.
        public static System.Object[] SingleObjectArray( System.Object targetObject )
        {
            System.Object[] array = new System.Object[] { targetObject };
            return array;
        }

        // 리스트 문의 for문을 간결하게 작성할 수 있도록 한다.
        public static void ForLoop<T>( this List<T> target, System.Func<T, bool> function )
        {
            int listCount = target.Count;
            for( int i = 0; i < listCount; i++ )
            {
                if( !function( target[ i ] ) )
                {
                    return;
                }
            }
        }

        // 어셈블리로부터 클래스 이름 문자열을 보내 System.Type을 얻는다.
        public static Type GetTypeFromAssemblies( string TypeName )
        {
            // null 반환 없이 Type이 얻어진다면 얻어진 그대로 반환.
            var type = Type.GetType( TypeName );
            if( type != null )
                return type;

            // 프로젝트에 분명히 포함된 클래스임에도 불구하고 System.Type이 찾아지지 않는다면,
            // 실행중인 어셈블리를 모두 탐색 하면서 그 안에 찾고자 하는 Type이 있는지 검사.
            var currentAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach( var assemblyName in referencedAssemblies )
            {
                var assembly = System.Reflection.Assembly.Load( assemblyName );
                if( assembly != null )
                {
                    // 찾았다 요놈!!!
                    type = assembly.GetType( TypeName );
                    if( type != null )
                        return type;
                }
            }

            // 못 찾았음;;; 클래스 이름이 틀렸던가, 아니면 알 수 없는 문제 때문이겠지...
            return null;
        }
    }
}