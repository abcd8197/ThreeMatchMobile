using UnityEngine;

namespace ThreeMatch
{
    public static class ColorTypeExtension
    {
        public static Color GetColor(this ColorType color)
        {
            return color switch
            {
                ColorType.Red => Color.red,
                ColorType.Green => Color.green,
                ColorType.Blue => Color.blue,
                ColorType.Yellow => Color.yellow,
                ColorType.Purple => Color.purple,
                _ => Color.white,
            };
        }
    }
}
