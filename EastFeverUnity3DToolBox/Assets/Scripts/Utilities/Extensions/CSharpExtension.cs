using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EastFever
{
    // C# 제공 기본 클래스 기능 확장을 위해 추가되는 확장 메서드 모음.
    public static class CSharpExtension
    {
        //  배열이 비어 있는지 String.IsNullOrEmpty스타일로 확인.
        public static bool IsNullOrEmpty( this System.Array target )
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

        // 단일 오브젝트를 배열에 담기.
        public static System.Object[] SingleObjectArray( System.Object targetObject )
        {
            System.Object[] array = new System.Object[] { targetObject };
            return array;
        }
    }
}