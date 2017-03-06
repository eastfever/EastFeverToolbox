using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if EAST_FEVER_TK2D

namespace EastFever
{
    // 런타임에 리소스 폴더의 텍스쳐를 tk2D스프라이트에 적용한다.
    public static bool ApplyTexture( 
        this tk2dSpriteFromTexture target,
        string textureResourcePath, 
        string defaultTexturePathOnNotFound = "" )
    {
        Texture textureFromResource = Resources.Load<Texture>( textureResourcePath );
        if( null == textureFromResource )
        {
            Logger.LogError( "[tk2dExtensions/ApplyTexture]not found texture to apply - " + textureResourcePath );
            if( !defaultTexturePathOnNotFound.IsNullOrEmpty() )
            {
                textureFromResource = Resources.Load<Texture>( defaultTexturePathOnNotFound );
                if( null == textureFromResource )
                {
                    Logger.LogError( "[tk2dExtensions/ApplyTexture]not found default texture to apply - " + defaultTexturePathOnNotFound );
                    return false;
                }
            }
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

    public static void FadeIn( this tk2dBaseSprite target, float time, float targetValue = 1.0f )
    {
        float newAlpha = 0f;
        target.SetAlpha( 0f );
        float elapsedTime = 0f;        
        DelegationWorker.CreateJobOnUpdate(
            time,
            () =>
            {
                elapsedTime += Time.deltaTime;
                newAlpha = Mathf.Lerp( 0f, targetValue, elapsedTime / time );
                newAlpha = Mathf.Min( newAlpha, 1f );
                target.SetAlpha( newAlpha );
            } );
    }

    public static void FadeOut( this tk2dBaseSprite target, float time )
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
                newAlpha = Mathf.Max( newAlpha, 0f );
                target.SetAlpha( newAlpha );
            } );
    }

    public static void SetAlpha( this tk2dBaseSprite target, float alpha )
    {
        Color newColor = target.color;
        newColor.a = alpha;
        target.color = newColor;
        target.Build();
    }

    public static void SetColor( this tk2dSprite target, Color color )
    {   
        target.color = color;
        target.Build();
    }

    public static string GetSpriteName( this tk2dBaseSprite target )
    {
        if( target.Collection.spriteDefinitions.IsSafeIndex( target.spriteId ) )
        {
            return target.Collection.spriteDefinitions[ target.spriteId ].name;
        }
        return "";
    }

    // 다른 애니메이션 개체와 동일한 상태로 맞춰서 같은 애니메이션을 플레이 한다.
    public static void PlaySync( this tk2dSpriteAnimator target, tk2dSpriteAnimator other )
    {
        tk2dBaseSprite spriteToCopy = other.Sprite as tk2dBaseSprite;
        target.SetSprite( spriteToCopy.Collection, spriteToCopy.spriteId );
        target.SetFrame( other.CurrentFrame );
        target.Play( other.CurrentClip, other.ClipTimeSeconds, other.ClipFps );
    }

    // 해당 클립의 1회 재생 동안 걸리는 시간을 반환.
    public static float RunningTime( this tk2dSpriteAnimationClip target )
    {
        if( 0f == target.fps )
        {
            return 0f;
        }
        return ( float )target.frames.Length / target.fps;
    }
}
#endif