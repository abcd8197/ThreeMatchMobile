using UnityEngine;

namespace ThreeMatch
{
    public static class ColorTypeExtension
    {
        public static Color GetColor(this ColorType color)
        {
            return color switch
            {
                ColorType.Red => UnityEngine.Color.red,
                ColorType.Green => UnityEngine.Color.green,
                ColorType.Blue => UnityEngine.Color.blue,
                ColorType.Yellow => UnityEngine.Color.yellow,
                ColorType.Purple => UnityEngine.Color.purple,
                _ => UnityEngine.Color.white,
            };
        }
    }
}
