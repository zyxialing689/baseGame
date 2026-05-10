using System.Collections.Generic;
using UnityEngine;

public static class Gpu2DDepthUtility
{
    public const float YToZScale = 0.1f;
    public const float SameDepthTieZStep = 0.0000001f;
    public const float SortingOrderDepthStep = 0.000000001f;

    private static readonly Queue<int> FreeSortingOrders = new Queue<int>();
    private static int nextSortingOrder;

    public static int AcquireSortingOrder()
    {
        if (FreeSortingOrders.Count > 0)
            return FreeSortingOrders.Dequeue();

        return nextSortingOrder++;
    }

    public static void ReleaseSortingOrder(int sortingOrder)
    {
        if (sortingOrder < 0)
            return;

        FreeSortingOrders.Enqueue(sortingOrder);
    }

    public static float CalculateDepthZ(Vector3 position, int sortingOrder, int fallbackOrder, float baseZ = 0f, bool preserveSourceZ = false)
    {
        int order = sortingOrder >= 0 ? sortingOrder : fallbackOrder;
        float sourceZ = preserveSourceZ ? position.z : baseZ;
        return sourceZ + position.y * YToZScale - order * SameDepthTieZStep;
    }

    public static float CalculateSortingDepthBias(int sortingOrder, int fallbackOrder)
    {
        int order = sortingOrder >= 0 ? sortingOrder : fallbackOrder;
        return order * SortingOrderDepthStep;
    }
}
