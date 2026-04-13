using System.Collections.Generic;
using UnityEngine;
using UnityTimer;

public class PathTest : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    void Awake()
    {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(Random.value, Random.value, Random.value);
    }
}