using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomGradient
{
    public enum BlendMode { Linear, Discrete };
    public BlendMode blendMode;
    public bool randomizeColour;

    [SerializeField]
    List<ColourKey> keys = new List<ColourKey>();

    public CustomGradient()
    {
        AddKey(Color.white, 0);
        AddKey(Color.black, 1);
    }

    public Color Evaluate(float time, int segement = 1)
    {
        ColourKey keyLeft = keys[0];
        ColourKey keyRight = keys[keys.Count - 1];

        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i].Time < time)
            {
                keyLeft = keys[i];
            }
            if (keys[i].Time > time)
            {
                keyRight = keys[i];
                break;
            }
        }

        if (blendMode == BlendMode.Linear)
        {
            float blendTime = Mathf.InverseLerp(keyLeft.Time, keyRight.Time, time);
            return Color.Lerp(keyLeft.Colour, keyRight.Colour, blendTime);
        }
        else
        {
            if (segement > 1)
            {
                float step = (keyRight.Time - keyLeft.Time) / segement;
                for (int i = 1; i <= segement; i++)
                {
                    float step_time = keyLeft.Time + step;
                    if (time > step_time)
                    {
                        Color leftColor = Color.Lerp(keyLeft.Colour, keyRight.Colour, Mathf.InverseLerp(keyLeft.Time, keyRight.Time, step_time));
                        keyLeft = new ColourKey(leftColor, step_time);
                    }
                    else
                    {
                        Color rightColor = Color.Lerp(keyLeft.Colour, keyRight.Colour, Mathf.InverseLerp(keyLeft.Time, keyRight.Time, step_time));
                        keyRight = new ColourKey(rightColor, step_time);
                        break;
                    }
                }
            }
            return keyRight.Colour;
        }
    }

    public int AddKey(Color colour, float time)
    {
        ColourKey newKey = new ColourKey(colour, time);
        for (int i = 0; i < keys.Count; i++)
        {
            if (newKey.Time < keys[i].Time)
            {
                keys.Insert(i, newKey);
                return i;
            }
        }

        keys.Add(newKey);
        return keys.Count - 1;
    }

    public void RemoveKey(int index)
    {
        if (keys.Count >= 2)
        {
            keys.RemoveAt(index);
        }
    }

    public int UpdateKeyTime(int index, float time)
    {
        Color col = keys[index].Colour;
        RemoveKey(index);
        return AddKey(col, time);
    }

    public void UpdateKeyColour(int index, Color col)
    {
        keys[index] = new ColourKey(col, keys[index].Time);
    }

    public int NumKeys
    {
        get
        {
            return keys.Count;
        }
    }

    public ColourKey GetKey(int i)
    {
        return keys[i];
    }

    public Texture2D GetTexture(int width, int segment, bool isLinear = false)
    {
        Texture2D texture = new Texture2D(width, 1, TextureFormat.ARGB32, false, isLinear)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point
        };
        Color[] colours = new Color[width];
        for (int i = 0; i < width; i++)
        {
            colours[i] = Evaluate((float)i / (width - 1), segment);
        }
        texture.SetPixels(colours);
        texture.Apply();
        return texture;
    }

    [System.Serializable]
    public struct ColourKey
    {
        [SerializeField]
        Color colour;
        [SerializeField]
        float time;

        public ColourKey(Color colour, float time)
        {
            this.colour = colour;
            this.time = time;
        }

        public Color Colour
        {
            get
            {
                return colour;
            }
        }

        public float Time
        {
            get
            {
                return time;
            }
        }
    }

}
