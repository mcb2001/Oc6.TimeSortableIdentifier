using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Oc6.TimeSortableIdentifier
{
    /// <summary>
    /// <para>Time Sortable Unique Identifier</para>
    /// <para>64 integer (long)</para>
    /// <para>First 42 bits is the current millis since epoch</para>
    /// <para>Next 8 bits is a counter</para>
    /// <para>Last 14 bits is randomness</para>
    /// <para>Guarenteed to generate 255 sortable unique tsids every millisecond</para>
    /// </summary>
    public static partial class Tsid
    {
        //Grouped in (2)(2)-(2)(2)-(2)(2)-(2)(2) for faster TryParse
        //Multiline flag to ensure that input is only single line
        //Compiled as it wont change during runtime
        [GeneratedRegex(
            "^" +
            "([0-9a-zA-Z]{2})([0-9a-zA-Z]{2})" +
            "-([0-9a-zA-Z]{2})([0-9a-zA-Z]{2})" +
            "-([0-9a-zA-Z]{2})([0-9a-zA-Z]{2})" +
            "-([0-9a-zA-Z]{2})([0-9a-zA-Z]{2})" +
            "$",
            RegexOptions.Multiline | RegexOptions.Compiled)]
        private static partial Regex GenerateParserRegex();

        private static byte internalCounter = 0;
        private static long previousTimeInMilliseconds = 0;
        private static readonly Regex parserRegex = GenerateParserRegex();
        private static readonly object syncRoot = new();

        /// <summary>
        /// Creates a new Time Sortable Unique Identifier as a <see cref="System.Int64"/>
        /// </summary>
        /// <returns></returns>
        public static long Create()
        {
            lock (syncRoot)
            {
                TimeSpan diff = DateTime.UtcNow - DateTime.UnixEpoch;

                long timeInMilliseconds = diff.Ticks / TimeSpan.TicksPerMillisecond;

                //counter is per millisecond, so roll back to zero on next tick
                if (timeInMilliseconds != previousTimeInMilliseconds)
                {
                    previousTimeInMilliseconds = timeInMilliseconds;
                    internalCounter = 0;
                }

                //We only need the first 42 bits, leaving behind 22 0's
                timeInMilliseconds <<= 22;

                Span<byte> randomBytes = new byte[8];

                RandomNumberGenerator.Fill(randomBytes[..2]);

                randomBytes[2] = internalCounter;

                ++internalCounter;

                long randomBits = BitConverter.ToInt64(randomBytes);

                //We only have 22 bits to play with, so drop the last two random off the end
                randomBits >>= 2;

                long tsid = timeInMilliseconds | randomBits;

                //Always have first bit 0
                return tsid & long.MaxValue;
            }
        }

        /// <summary>
        /// <para>Tries to parse a given string as a tsid</para>
        /// <para>The parser is case insensetive</para>
        /// </summary>
        /// <param name="tsid">The string containing the value to parse</param>
        /// <param name="result">The result if the parse was successful</param>
        /// <returns>true if successful, false if not successful</returns>
        public static bool TryParse(string tsid, out long result)
        {
            if (tsid.Length != 19)
            {
                result = default;
                return false;
            }

            Match match = parserRegex.Match(tsid);

            if (!match.Success)
            {
                result = default;
                return false;
            }

            byte[] bytes = new byte[8];

            for (int i = 0; i < 8; ++i)
            {
                if (!byte.TryParse(match.Groups[i + 1].ValueSpan, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
                {
                    result = default;
                    return false;
                }
                else if (i == 0 && b > 127)
                {
                    result = default;
                    return false;
                }

                //count from the end
                bytes[^(i + 1)] = b;
            }

            result = BitConverter.ToInt64(bytes);

            return true;
        }

        /// <summary>
        /// <para>Returns an uppercase string representation of this tsid</para>
        /// </summary>
        /// <param name="tsid">The value to get a string representation of</param>
        /// <returns>The string representation of the provided value</returns>
        /// <exception cref="ArgumentOutOfRangeException">If tsid is negative</exception>
        public static string ToString(long tsid)
        {
            if (tsid < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tsid), "Tsid must be positive");
            }

            string baseString = Convert.ToString(tsid, 16)
                .ToUpperInvariant()
                .PadLeft(16, '0');

            StringBuilder builder = new();
            builder.Append(baseString[0]);
            builder.Append(baseString[1]);
            builder.Append(baseString[2]);
            builder.Append(baseString[3]);
            builder.Append('-');
            builder.Append(baseString[4]);
            builder.Append(baseString[5]);
            builder.Append(baseString[6]);
            builder.Append(baseString[7]);
            builder.Append('-');
            builder.Append(baseString[8]);
            builder.Append(baseString[9]);
            builder.Append(baseString[10]);
            builder.Append(baseString[11]);
            builder.Append('-');
            builder.Append(baseString[12]);
            builder.Append(baseString[13]);
            builder.Append(baseString[14]);
            builder.Append(baseString[15]);
            return builder.ToString();
        }
    }
}