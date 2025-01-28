public static class FastParse {
    public static int FloatAsInt(ReadOnlySpan<byte> span) {
        int value = 0;
        bool negative = false;
        for(;span.Length > 0; span = span.Slice(1)) {
            if (span[0] == (byte)'-') {
                negative = true;
                continue;
            }

            if (span[0] == (byte)'.') continue;
            value = value * 10 + (span[0] - (byte)'0');

            /* alternative, not much faster 
            if (span[0] == (byte)'.') {
                value = value * 1000 + (100 * (span[1] - (byte)'0')) + (10 * (span[2] - (byte)'0')) + (span[3] - (byte)'0');
                break;
            } else {
                value = value * 10 + (span[0] - (byte)'0');
            }
                */
        }

        return negative ? -value : value;
    }
}