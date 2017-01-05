using UnityEngine;
using System.Collections.Generic;

namespace EastFever
{
    // 2016.12.20 by east_fever
    // 유니티에서 제공하는 클래스들의 기능을 추가하기 위한 확장 메서드 모음.
    public static class UnityExtension
    {
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