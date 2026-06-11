using System.Text;

namespace _2___Application._3_Utils
{
    public static class CsvImportTextReader
    {
        public const char ReplacementCharacter = '\uFFFD';

        private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);

        private static readonly char[] Windows1252Controls =
        {
            '\u20AC', '\u0081', '\u201A', '\u0192', '\u201E', '\u2026', '\u2020', '\u2021',
            '\u02C6', '\u2030', '\u0160', '\u2039', '\u0152', '\u008D', '\u017D', '\u008F',
            '\u0090', '\u2018', '\u2019', '\u201C', '\u201D', '\u2022', '\u2013', '\u2014',
            '\u02DC', '\u2122', '\u0161', '\u203A', '\u0153', '\u009D', '\u017E', '\u0178'
        };

        public static TextReader CreateReader(Stream stream)
        {
            return new StringReader(ReadAllText(stream));
        }

        public static bool ContainsReplacementCharacter(string? value)
        {
            return value?.IndexOf(ReplacementCharacter) >= 0;
        }

        public static string BuildReplacementCharacterError(string fieldName, string? reference = null)
        {
            var suffix = string.IsNullOrWhiteSpace(reference) ? string.Empty : $" Referencia: {reference}.";

            return $"O campo {fieldName} contem caractere de substituicao Unicode (U+FFFD). " +
                   $"O arquivo pode estar com codificacao incorreta ou o texto ja veio corrompido.{suffix}";
        }

        private static string ReadAllText(Stream stream)
        {
            var originalPosition = stream.CanSeek ? stream.Position : 0;

            if (stream.CanSeek)
                stream.Position = 0;

            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            var bytes = memory.ToArray();

            if (stream.CanSeek)
                stream.Position = originalPosition;

            if (bytes.Length == 0)
                return string.Empty;

            if (HasPrefix(bytes, 0xEF, 0xBB, 0xBF))
                return StrictUtf8.GetString(bytes, 3, bytes.Length - 3);

            if (HasPrefix(bytes, 0xFF, 0xFE, 0x00, 0x00))
                return Encoding.UTF32.GetString(bytes, 4, bytes.Length - 4);

            if (HasPrefix(bytes, 0x00, 0x00, 0xFE, 0xFF))
                return new UTF32Encoding(true, true).GetString(bytes, 4, bytes.Length - 4);

            if (HasPrefix(bytes, 0xFF, 0xFE))
                return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);

            if (HasPrefix(bytes, 0xFE, 0xFF))
                return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);

            if (LooksLikeUtf16LittleEndian(bytes))
                return Encoding.Unicode.GetString(bytes);

            if (LooksLikeUtf16BigEndian(bytes))
                return Encoding.BigEndianUnicode.GetString(bytes);

            try
            {
                return StrictUtf8.GetString(bytes);
            }
            catch (DecoderFallbackException)
            {
                return DecodeWindows1252(bytes);
            }
        }

        private static bool HasPrefix(byte[] bytes, params byte[] prefix)
        {
            if (bytes.Length < prefix.Length)
                return false;

            for (var i = 0; i < prefix.Length; i++)
            {
                if (bytes[i] != prefix[i])
                    return false;
            }

            return true;
        }

        private static bool LooksLikeUtf16LittleEndian(byte[] bytes)
        {
            if (bytes.Length < 4)
                return false;

            var sampleLength = Math.Min(bytes.Length, 512);
            var oddNulls = 0;

            for (var i = 1; i < sampleLength; i += 2)
            {
                if (bytes[i] == 0)
                    oddNulls++;
            }

            return oddNulls >= sampleLength / 4;
        }

        private static bool LooksLikeUtf16BigEndian(byte[] bytes)
        {
            if (bytes.Length < 4)
                return false;

            var sampleLength = Math.Min(bytes.Length, 512);
            var evenNulls = 0;

            for (var i = 0; i < sampleLength; i += 2)
            {
                if (bytes[i] == 0)
                    evenNulls++;
            }

            return evenNulls >= sampleLength / 4;
        }

        private static string DecodeWindows1252(byte[] bytes)
        {
            var chars = new char[bytes.Length];

            for (var i = 0; i < bytes.Length; i++)
            {
                var value = bytes[i];
                chars[i] = value switch
                {
                    < 0x80 => (char)value,
                    < 0xA0 => Windows1252Controls[value - 0x80],
                    _ => (char)value
                };
            }

            return new string(chars);
        }
    }
}
