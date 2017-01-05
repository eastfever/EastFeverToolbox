using UnityEngine;
using System.Collections;

namespace EastFever
{
    // 전처리기 디파인 모음.
    // 아래 문자열들은 DefineEditorWindow에서 사용 여부 체크 후,
    // 유니티 PlayerSetting의 Scripting Define Symbols에 선별되서 들어감
    public class DefineStrings
    {
        public static string[] DEFINES = new string[]
        {
            "EAST_FEVER_TK2D",   // tk2d 관련 내용을 활성화 한다.
        };

        public static int DefineCount() { return DEFINES.Length; }
    }
}