using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chips : MonoBehaviour
{
    [SerializeField] private List<Sprite> sprites;
    
    public Sprite GetChipsSprite(float size)
    {
        switch(size)
        {
            case float i when (i <= 1): return sprites[0];
            case float i when (i <= 5): return sprites[1];
            case float i when (i <= 10): return sprites[2];
            case float i when (i <= 20): return sprites[3];
            case float i when (i <= 50): return sprites[4];
            case float i when (i > 50): return sprites[5];
            default: return null;
        }
    }
}
