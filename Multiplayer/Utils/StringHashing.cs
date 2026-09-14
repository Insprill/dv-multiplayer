namespace Multiplayer.Utils;

internal static class StringHashing
{
    public static uint Fnv1aHash(string text)
    {
        unchecked
        {
            const uint fnvPrime = 0x01000193;
            uint hash = 0x811C9DC5;
            foreach (char c in text)
            {
                hash ^= c;
                hash *= fnvPrime;
            }
            return hash;
        }
    }
}
