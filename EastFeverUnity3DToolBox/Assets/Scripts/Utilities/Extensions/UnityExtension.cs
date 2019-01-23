using UnityEngine;
using System.Collections.Generic;

namespace EastFever
{
    // 2016.12.20 by east_fever
    // 유니티에서 제공하는 클래스들의 기능을 추가하기 위한 확장 메서드 모음.
    public static class UnityExtension
    {
        // 오브젝트의 x축 위치값을 바꾼다.
        public static void SetPositionX( this GameObject target, float x )
        {
            Vector3 targetPosition = target.transform.position;
            targetPosition.x = x;
            target.transform.position = targetPosition;
        }

        // 오브젝트의 높이값을 바꾼다.
        public static void SetPositionY( this GameObject target, float y )
        {
            Vector3 targetPosition = target.transform.position;
            targetPosition.y = y;
            target.transform.position = targetPosition;
        }

        // 주어진 오브젝트 하위의 child오브젝트들을 자신을 포함하여 켜거나 끈다.
        public static void SetChildActiveState( this GameObject target, bool flag )
        {
            _setChildActiveState( target, flag );
        }        

        // 주어진 오브젝트의 Hierarchy경로를 반환한다.
        public static string GetHierarchyPath( this GameObject target )
        {
            GameObject currentObject = target;

            string path = "/" + currentObject.name;
            while( currentObject.transform.parent != null )
            {
                currentObject = currentObject.transform.parent.gameObject;
                path = "/" + currentObject.name + path;
            }
            return path;
        }

        // 현재 씬에 존재하는 모든 오브젝트 반환.
        public static GameObject[] GetAllObjectsInScene( this UnityEngine.SceneManagement.Scene target )
        {
            List<GameObject> objectListToReturn = new List<GameObject>();
            GameObject[] rootObjectsInScenes = target.GetRootGameObjects();
            foreach( GameObject rootObject in rootObjectsInScenes )
            {
                PushChildObjectsToList( rootObject, objectListToReturn );
            }
            return objectListToReturn.ToArray();
        }

        //// EaseBack연출 부여와 함께 원래 크기로 되돌린다.
        //public static void ScaleToOneWithEaseBackTween( this GameObject target, float time )
        //{
        //    target.transform.localScale = new Vector3( 0.01f, 0.01f, 0.01f );
        //    LeanTween
        //        .scale( target, Vector3.one, time )
        //        .setEase( LeanTweenType.easeInOutBack );
        //}

        // 좀 더 정확한 소수점 표현으로 벡터 문자열을 표시한다.
        public static string ToStringWithoutCutting( this Vector2 target )
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder( 128 );
            stringBuilder.Append( "(" );
            stringBuilder.Append( target.x.ToString() );
            stringBuilder.Append( ", " );
            stringBuilder.Append( target.y.ToString() );
            stringBuilder.Append( ")" );
            return stringBuilder.ToString();
        }

        // 좀 더 정확한 소수점 표현으로 벡터 문자열을 표시한다.
        public static string ToStringWithoutCutting( this Vector3 target )
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder( 128 );
            stringBuilder.Append( "(" );
            stringBuilder.Append( target.x.ToString() );
            stringBuilder.Append( ", " );
            stringBuilder.Append( target.y.ToString() );
            stringBuilder.Append( ", " );
            stringBuilder.Append( target.z.ToString() );
            stringBuilder.Append( ")" );
            return stringBuilder.ToString();
        }

        private static void PushChildObjectsToList( GameObject targetObject, List<GameObject> targetList )
        {
            targetList.Add( targetObject );
            foreach( Transform child in targetObject.transform )
            {
                PushChildObjectsToList( child.gameObject, targetList );
            }
        }

        private static void _setChildActiveState( GameObject target, bool flag )
        {
            int childCount = target.transform.childCount;
            for( int i = 0; i < childCount; i++ )
            {
                GameObject childObject = target.transform.GetChild( i ).gameObject;
                childObject.SetActive( flag );
                _setChildActiveState( childObject, flag );
            }
            target.SetActive( flag );
        }
    }
}

