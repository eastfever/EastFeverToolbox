using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if EAST_FEVER_TK2D

namespace EastFever
{
    // tk2D 기능 확장을 위해 추가되는 확장 메서드 모음.
    public static class tk2dExtension
    {
        // 런타임에 리소스 폴더의 텍스쳐를 tk2D스프라이트에 적용한다.
        public static bool ApplyTexture( this tk2dSpriteFromTexture target, string textureResourcePath )
        {
            Texture textureFromResource = Resources.Load<Texture>( textureResourcePath );
            if( null == textureFromResource )
            {
                Logger.LogError( "[tk2dExtensions/ApplyTexture]not found texture to apply - " + textureResourcePath );
                return false;
            }

            try
            {
                target.texture = textureFromResource;
                target.ForceBuild();
            }
            catch( System.Exception e )
            {
                Logger.LogError( "[tk2dExtensions/ApplyTexture]Exception occured - " + e.Message );
                return false;
            }
            return true;
        }

        public static void FadeOut( this tk2dSprite target, float time )
        {   
            float originAlpha = target.color.a;
            float newAlpha = target.color.a;        

            float elapsedTime = 0f;
            DelegationWorker.CreateJobOnUpdate(
                time,
                () =>
                {
                    elapsedTime += Time.deltaTime;
                    newAlpha = Mathf.Lerp( originAlpha, 0f, elapsedTime / time );
                    target.SetAlpha( newAlpha );
                } );
        }

        public static void SetAlpha( this tk2dSprite target, float alpha )
        {
            Color newColor = target.color;
            newColor.a = alpha;
            target.color = newColor;
        }
    }
}
#endif