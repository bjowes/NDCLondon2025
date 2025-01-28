public static class FastHash {
    public static ulong HashBytes(ReadOnlySpan<byte> span) {
        ulong hash = 0;

        unchecked {
            foreach (var b in span) {
                hash += (hash * 97) + b;
            }
        }

        return hash;
    }
}