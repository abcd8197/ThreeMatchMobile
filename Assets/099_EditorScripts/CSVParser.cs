using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ThreeMatch.Editor
{
    public static class CsvParser
    {
        private const string Split = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        private const string LineSplit = @"\r\n|\n\r|\n|\r";
        private static readonly char[] Trims = { '\"' };

        /// <summary>
        /// 규칙:
        /// 1) 첫 헤더는 Key
        /// 2) List_XXX: 셀 내부 "1,2,3" 을 실제 배열로 변환하여 "XXX": [1,2,3] 로 저장 (key 동일 시 갱신X)
        /// 3) Obj_Class_Field: 같은 Key면 계속 누적, "Class": [ {Field:...}, {Field:...} ] 로 저장
        ///    - 같은 row에서 Class의 여러 Field를 모아 객체 1개를 만들고 row 끝에서 add
        /// </summary>
        public static string ReadAndConvertToListJson(string source)
        {
            var lines = Regex.Split(source, LineSplit)
                             .Where(l => !string.IsNullOrWhiteSpace(l))
                             .ToArray();

            if (lines.Length == 0) return "[]";

            var headers = Regex.Split(lines[0], Split)
                               .Select(h => h.Trim())
                               .ToArray();

            if (headers.Length == 0 || headers[0] != "Key")
                throw new Exception($"CSV header[0] must be 'Key'. Actual: '{(headers.Length > 0 ? headers[0] : "")}'");

            // key -> JObject (섹션)
            var byKey = new Dictionary<string, JObject>();
            var keyOrder = new List<string>();

            string currentKey = null;

            for (int i = 1; i < lines.Length; i++)
            {
                var values = Regex.Split(lines[i], Split);

                // Key column (0)
                var keyValue = GetCell(values, 0);
                if (!string.IsNullOrEmpty(keyValue))
                {
                    currentKey = keyValue;

                    if (!byKey.TryGetValue(currentKey, out _))
                    {
                        var root = new JObject { ["Key"] = currentKey };
                        byKey[currentKey] = root;
                        keyOrder.Add(currentKey);
                    }
                }

                if (string.IsNullOrEmpty(currentKey))
                    continue;

                var rootObj = byKey[currentKey];

                // row 단위 Obj 누적용: className -> JObject
                var rowObjByClass = new Dictionary<string, JObject>();

                // 나머지 컬럼 처리
                for (int c = 1; c < headers.Length; c++)
                {
                    var header = headers[c];
                    if (string.IsNullOrEmpty(header) || header.StartsWith("#"))
                        continue;

                    var cell = GetCell(values, c);
                    if (string.IsNullOrEmpty(cell))
                        continue;

                    if (header.StartsWith("List_"))
                    {
                        // List_XXX -> key "XXX" (갱신 금지, 첫 세팅만)
                        var listName = header.Substring("List_".Length);
                        if (string.IsNullOrEmpty(listName))
                            throw new Exception($"Invalid List header: {header}. It should be like 'List_Name'");

                        if (rootObj.ContainsKey(listName))
                            continue; // Key 동일 시 갱신하지 않음

                        var arr = ParseInlineList(cell);
                        rootObj[listName] = arr;
                    }
                    else if (header.StartsWith("Obj_"))
                    {
                        // Obj_Class_Field -> row에서 Class별로 객체 1개 구성 후 row 끝에서 add
                        var parts = header.Split('_');
                        if (parts.Length != 3)
                            throw new Exception($"Invalid Obj header: {header}. It should be like 'Obj_Class_Field'");

                        var className = parts[1];
                        var fieldName = parts[2];

                        if (!rowObjByClass.TryGetValue(className, out var obj))
                        {
                            obj = new JObject();
                            rowObjByClass[className] = obj;
                        }

                        // 같은 row 안에서 필드 누적
                        obj[fieldName] = GuessJValue(cell);
                    }
                    else if (header.StartsWith("ObjS_"))
                    {
                        // (옵션) 단일 오브젝트: ObjS_Class_Field -> "Class": {Field:...} (갱신 금지)
                        var parts = header.Split('_');
                        if (parts.Length != 3)
                            throw new Exception($"Invalid ObjS header: {header}. It should be like 'ObjS_Class_Field'");

                        var className = parts[1];
                        var fieldName = parts[2];

                        if (!rootObj.TryGetValue(className, out var token) || token.Type != JTokenType.Object)
                        {
                            token = new JObject();
                            rootObj[className] = token;
                        }

                        var obj = (JObject)token;
                        if (!obj.ContainsKey(fieldName))
                            obj[fieldName] = GuessJValue(cell); // 갱신 금지
                    }
                    else if (header.StartsWith("Dic_"))
                    {
                        // (옵션) 간단 딕셔너리: Dic_Name 셀 값 "{a,b}" 또는 "a,b" 형태
                        var dicName = header.Substring("Dic_".Length);
                        if (string.IsNullOrEmpty(dicName))
                            throw new Exception($"Invalid Dic header: {header}. It should be like 'Dic_Name'");

                        if (!rootObj.TryGetValue(dicName, out var token) || token.Type != JTokenType.Object)
                        {
                            token = new JObject();
                            rootObj[dicName] = token;
                        }

                        var dic = (JObject)token;
                        ApplySimpleDic(dic, cell);
                    }
                    else
                    {
                        // 일반 값: key 동일 시 갱신X (첫 값만)
                        if (!rootObj.ContainsKey(header))
                            rootObj[header] = GuessJValue(cell);
                    }
                }

                // row 끝: rowObjByClass를 실제 리스트로 add
                foreach (var kv in rowObjByClass)
                {
                    var className = kv.Key;
                    var rowObj = kv.Value;

                    if (!rowObj.HasValues)
                        continue;

                    if (!rootObj.TryGetValue(className, out var token) || token.Type != JTokenType.Array)
                    {
                        token = new JArray();
                        rootObj[className] = token;
                    }

                    ((JArray)token).Add(rowObj);
                }
            }

            var result = new JArray();
            foreach (var k in keyOrder)
                result.Add(byKey[k]);

            return result.ToString(Formatting.Indented);
        }

        // -----------------------
        // Helpers
        // -----------------------

        private static string GetCell(string[] values, int index)
        {
            if (index < 0 || index >= values.Length) return string.Empty;

            var value = values[index] ?? string.Empty;

            // CSV escape 처리
            value = value.Replace("\"\"", "\"").Trim();

            if (value.StartsWith("\"")) value = value.Substring(1);
            if (value.EndsWith("\"")) value = value.Substring(0, value.Length - 1);

            return value.Trim();
        }

        // "1,1,1,1,1" -> [1,1,1,1,1]
        private static JArray ParseInlineList(string cell)
        {
            var arr = new JArray();

            // 공백 제거 후 split
            var parts = cell.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                var v = p.Trim();
                if (v.Length == 0) continue;
                arr.Add(GuessJValue(v));
            }

            return arr;
        }

        private static void ApplySimpleDic(JObject dic, string cell)
        {
            // "{a,b}" , "a,b" 정도만 지원 (기존 호환)
            var s = cell.Replace("\"", string.Empty).Trim();
            s = s.Trim('{', '}');

            var parts = s.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return;

            var k = parts[0].Trim();
            var v = parts[1].Trim();

            if (k.Length == 0) return;
            if (!dic.ContainsKey(k))
                dic[k] = GuessJValue(v);
        }

        private static JValue GuessJValue(string raw)
        {
            raw = raw.Trim();
            if (raw.Length == 0) return JValue.CreateNull();

            if (bool.TryParse(raw, out var b)) return new JValue(b);

            // int / long
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                return new JValue(i);
            if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
                return new JValue(l);

            // float/double
            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                return new JValue(d);

            return new JValue(raw);
        }
    }
}
