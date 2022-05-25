namespace Akade.IndexedSet.Utils;

internal static class LevenshteinDistance
{
    /// <summary>
    /// Returns true if the strings have a levenshein distance smaller than <paramref name="maxDistance"/>.
    /// Does not calculate the entire distance if the minimum distance is already bigger.
    /// </summary>
    public static bool FuzzyMatch(ReadOnlySpan<char> a, ReadOnlySpan<char> b, int maxDistance)
    {
        int wordLength = a.Length;

        if (a.Length == 0 && b.Length == 0)
        {
            return true;
        }

        if (Math.Abs(a.Length - b.Length) > maxDistance)
        {
            return false;
        }

        int[] currentRow = new int[wordLength + 1];
        for (int i = 0; i < currentRow.Length; i++)
        {
            currentRow[i] = i;
        }

        for (int j = 0; j < b.Length; j++)
        {
            int[] lastRow = currentRow;
            currentRow = new int[wordLength + 1];
            currentRow[0] = lastRow[0] + 1;

            int minDistance = currentRow[0];

            for (int i = 1; i < currentRow.Length; i++)
            {
                int insertOrDeletion = Math.Min(currentRow[i - 1] + 1, lastRow[i] + 1);
                int replacement = a[i - 1] == b[j] ? lastRow[i - 1] : lastRow[i - 1] + 1;
                currentRow[i] = Math.Min(insertOrDeletion, replacement);
                minDistance = Math.Min(minDistance, currentRow[i]);
            }

            if (minDistance > maxDistance)
            {
                return false;
            }
        }

        return true;
    }
}
