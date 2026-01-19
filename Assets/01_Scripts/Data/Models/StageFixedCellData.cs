using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ThreeMatch
{
    [Serializable]
    public sealed class StageFixedCellSetData
    {
        public string Key;
        public List<StageFixedCellData> Cells;
    }

    [Serializable]
    [JsonConverter(typeof(StageFixedCellDataJsonConverter))]
    public sealed class StageFixedCellData
    {
        public string Key;
        public CellCoordinate Coord;
        public CellType CellType;
        public ColorType Color;
        public PieceType PieceType;
        public int HP;
        public CellCoordinate PortalOutCoord;

        public StageFixedCellData(string key, byte x, byte y, CellType cellType, ColorType colorType, PieceType pieceType, int hp, byte portalOutX, byte portalOutY)
        {
            this.Key = key;
            this.Coord = new CellCoordinate(x, y);
            this.CellType = cellType;
            this.Color = colorType;
            this.PieceType = pieceType;
            this.HP = hp;
            this.PortalOutCoord = new CellCoordinate(portalOutX, portalOutY);
        }
    }

    public sealed class StageFixedCellDataJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(StageFixedCellData);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var jo = JObject.Load(reader);

            byte x = (byte)(jo.Value<int?>("X") ?? 0);
            byte y = (byte)(jo.Value<int?>("Y") ?? 0);
            byte portalOutX = (byte)(jo.Value<int?>("PortalOutX") ?? 0);
            byte portalOutY = (byte)(jo.Value<int?>("PortalOutY") ?? 0);

            var cellType = ParseEnumOrDefault<CellType>(jo["CellType"], default);
            var data = new StageFixedCellData(
                jo.Value<string>("Key"),
                x,y,
                ParseEnumOrDefault(jo["CellType"], CellType.Normal),
                ParseEnumOrDefault(jo["Color"], ColorType.None),
                ParseEnumOrDefault(jo["PieceType"], PieceType.None),
                jo.Value<int?>("HP") ?? 0,
                portalOutX,
                portalOutY
            );
            return data;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is not StageFixedCellData d)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            // Key는 있을 때만
            if (!string.IsNullOrEmpty(d.Key))
            {
                writer.WritePropertyName("Key");
                writer.WriteValue(d.Key);
            }

            writer.WritePropertyName("X");
            writer.WriteValue(d.Coord.X);

            writer.WritePropertyName("Y");
            writer.WriteValue(d.Coord.Y);

            writer.WritePropertyName("CellType");
            writer.WriteValue(d.CellType.ToString());

            writer.WritePropertyName("Color");
            writer.WriteValue(d.Color.ToString());

            writer.WritePropertyName("PieceType");
            writer.WriteValue(d.PieceType.ToString());

            writer.WritePropertyName("HP");
            writer.WriteValue(d.HP);
            writer.WritePropertyName("PortalOutX");
            writer.WriteValue(d.PortalOutCoord.X);
            writer.WritePropertyName("PortalOutY");
            writer.WriteValue(d.PortalOutCoord.Y);

            writer.WriteEndObject();
        }

        private static CellCoordinate ReadPortalOutCoord(JObject jo, JsonSerializer serializer)
        {
            if (jo.TryGetValue("PortalOutX", out var xTok) || jo.TryGetValue("PortalOutY", out var yTok))
            {
                byte x = (byte)(jo.Value<int?>("PortalOutX") ?? 0);
                byte y = (byte)(jo.Value<int?>("PortalOutY") ?? 0);
                return new CellCoordinate(x, y);
            }

            return default;
        }

        private static TEnum ParseEnumOrDefault<TEnum>(JToken token, TEnum defaultValue) where TEnum : struct
        {
            if (token == null) return defaultValue;
            var s = token.Type == JTokenType.String ? token.Value<string>() : token.ToString();
            if (string.IsNullOrWhiteSpace(s)) return defaultValue;
            return Enum.TryParse<TEnum>(s, true, out var e) ? e : defaultValue;
        }
    }
}
