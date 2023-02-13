using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Pyro.Nc.UI;

public class GCodePainter
{
    private TMP_Text InputText;

    public void Paint()
    {
        var infos = InputText.textInfo.wordInfo;
        for (int i = 0; i < infos.Length; i++)
        {
            try
            {
                var word = infos[i];
                var str = word.GetWord();
                var isParameter = Regex.IsMatch(str, @"[xXyYzZiIjJ]{1}(\d+)|(\.\d+)");
                if (isParameter)
                {
                    PaintText(word.firstCharacterIndex, word.lastCharacterIndex, new Color32(177, 3, 252, 200));
                    continue; 
                }
                var isGCommand = Regex.IsMatch(str, @"(G|g){1}\d+");
                if (isGCommand)
                {
                    PaintText(word.firstCharacterIndex, word.lastCharacterIndex, new Color32(52, 235, 152, 200));
                    continue;
                }
                var isMCommand = Regex.IsMatch(str, @"(M|m){1}\d+");
                if (isMCommand)
                {
                    PaintText(word.firstCharacterIndex, word.lastCharacterIndex, new Color32(255, 255, 0, 200));
                    continue;
                }

                var isArbCommand = Regex.IsMatch(str, @"([^\d \n]{2,}|[SsFfTtDd]{1})\d*");
                if (isArbCommand)
                {
                    PaintText(word.firstCharacterIndex, word.lastCharacterIndex, new Color32(50, 120, 200, 200));
                    continue;
                }

                PaintText(word.firstCharacterIndex, word.lastCharacterIndex, new Color32(255, 0, 0, 200));
            }
            catch
            {
                //ignore
            }
        }
    }

    private void PaintText(int start, int end, Color32 color)
    {
        var txt = InputText.textInfo;
        var characters = txt.characterInfo;
        for (int i = start; i < end; i++)
        {
            var index = characters[i].vertexIndex;
            for (int j = 0; j < 4; j++)
            {
                txt.meshInfo[characters[i].materialReferenceIndex].colors32[index + j] = color;
            }
        }
        txt.meshInfo[0].mesh.vertices = txt.meshInfo[0].vertices;
        InputText.UpdateVertexData();
    }

    private IEnumerable<WordResult> EnumerateWords()
    {
        var txt = InputText.text;
        int startIndex = 0;
        for (int i = 0; i < txt.Length; i++)
        {
            char c = txt[i];
            if (c == ' ')
            {
                yield return new WordResult(startIndex, i);
                startIndex = i + 1;
            }
        }
    }

    private struct WordResult
    {
        public int Start;
        public int End;

        public WordResult(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}