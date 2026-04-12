using UnityEngine;

public interface IAIProvider
{
    TextAsset GetAIAsset();
    AIAgent GetAgent(); // ⭐ 用于调试高亮
}