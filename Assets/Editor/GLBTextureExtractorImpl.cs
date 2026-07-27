using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Pure C# GLB binary parser — no Unity dependencies.
/// Parses glTF 2.0 binary files to extract embedded images.
/// </summary>
public static class GLBTextureExtractorImpl
{
    public static byte[] Extract(string glbPath, int imageIndex)
    {
        using (var f = new FileStream(glbPath, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(f))
        {
            // Read 12-byte GLB header
            uint magic = reader.ReadUInt32();
            if (magic != 0x46546C67) // "glTF"
                throw new InvalidDataException($"Not a GLB file: magic=0x{magic:X8}");

            uint version = reader.ReadUInt32();
            uint length = reader.ReadUInt32();

            // Read JSON chunk
            uint jsonLen = reader.ReadUInt32();
            uint jsonType = reader.ReadUInt32();
            if (jsonType != 0x4E4F534A) // "JSON"
                throw new InvalidDataException("First chunk is not JSON");

            string jsonStr = Encoding.UTF8.GetString(reader.ReadBytes((int)jsonLen));

            // Read BIN chunk
            byte[] binData = null;
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                uint binLen = reader.ReadUInt32();
                uint binType = reader.ReadUInt32();
                if (binType == 0x004E4942) // "BIN\0"
                    binData = reader.ReadBytes((int)binLen);
            }

            if (binData == null)
                throw new InvalidDataException("No BIN chunk found in GLB");

            // Parse JSON to find image
            var json = ParseJson(jsonStr);
            var images = json.GetArray("images");
            if (imageIndex < 0 || imageIndex >= images.Count)
                return null;

            var image = images[imageIndex];
            var bufferViewIdx = image.GetInt("bufferView");
            var bufferViews = json.GetArray("bufferViews");
            var bv = bufferViews[bufferViewIdx];
            int offset = bv.GetInt("byteOffset");
            int byteLen = bv.GetInt("byteLength");

            if (offset + byteLen > binData.Length)
                throw new InvalidDataException($"Image data exceeds BIN chunk bounds: offset={offset}, len={byteLen}, binLen={binData.Length}");

            var result = new byte[byteLen];
            Array.Copy(binData, offset, result, 0, byteLen);
            return result;
        }
    }

    // Minimal JSON parser for glTF structure
    private class JsonValue
    {
        public Dictionary<string, JsonValue> obj;
        public List<JsonValue> arr;
        public string strVal;
        public int intVal;
        public bool isArray, isObject, isString, isInt;

        public List<JsonValue> GetArray(string key) => obj[key].arr;
        public int GetInt(string key) => obj[key].intVal;
    }

    private static JsonValue ParseJson(string json)
    {
        var p = new JsonParser(json);
        return p.ParseValue();
    }

    private class JsonParser
    {
        private string s;
        private int i;

        public JsonParser(string str) { s = str; i = 0; }

        public JsonValue ParseValue()
        {
            SkipWs();
            if (i >= s.Length) return null;
            char c = s[i];
            if (c == '{') return ParseObject();
            if (c == '[') return ParseArray();
            if (c == '"') return ParseString();
            if (c == '-' || char.IsDigit(c)) return ParseNumber();
            if (s.Substring(i).StartsWith("true")) { i += 4; return new JsonValue { isInt = true, intVal = 1 }; }
            if (s.Substring(i).StartsWith("false")) { i += 5; return new JsonValue { isInt = true, intVal = 0 }; }
            if (s.Substring(i).StartsWith("null")) { i += 4; return new JsonValue(); }
            throw new InvalidDataException($"Unexpected char '{c}' at {i}");
        }

        private JsonValue ParseObject()
        {
            var v = new JsonValue { isObject = true, obj = new Dictionary<string, JsonValue>() };
            i++; // skip {
            SkipWs();
            if (s[i] == '}') { i++; return v; }
            while (true)
            {
                SkipWs();
                string key = ParseString().strVal;
                SkipWs();
                i++; // skip :
                v.obj[key] = ParseValue();
                SkipWs();
                if (s[i] == ',') { i++; continue; }
                if (s[i] == '}') { i++; break; }
            }
            return v;
        }

        private JsonValue ParseArray()
        {
            var v = new JsonValue { isArray = true, arr = new List<JsonValue>() };
            i++; // skip [
            SkipWs();
            if (s[i] == ']') { i++; return v; }
            while (true)
            {
                v.arr.Add(ParseValue());
                SkipWs();
                if (s[i] == ',') { i++; continue; }
                if (s[i] == ']') { i++; break; }
            }
            return v;
        }

        private JsonValue ParseString()
        {
            i++; // skip "
            var sb = new StringBuilder();
            while (s[i] != '"')
            {
                if (s[i] == '\\') { i++; sb.Append(s[i]); i++; }
                else { sb.Append(s[i]); i++; }
            }
            i++; // skip "
            return new JsonValue { isString = true, strVal = sb.ToString() };
        }

        private JsonValue ParseNumber()
        {
            int start = i;
            if (s[i] == '-') i++;
            while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.' || s[i] == 'e' || s[i] == 'E' || s[i] == '+' || s[i] == '-'))
                i++;
            string numStr = s.Substring(start, i - start);
            return new JsonValue { isInt = true, intVal = int.Parse(numStr.Split('.')[0]) };
        }

        private void SkipWs()
        {
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        }
    }
}
