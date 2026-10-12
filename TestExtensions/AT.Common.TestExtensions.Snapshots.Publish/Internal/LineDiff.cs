using System.Text;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Internal;

/// <summary>Minimal unified line diff based on the longest common subsequence.</summary>
internal static class LineDiff
{
    private const int Context = 3;
    private const int MaxLines = 200;
    private const int MaxCells = 4_000_000;

    public static string Unified(
        string expected,
        string actual,
        string expectedName,
        string actualName
    )
    {
        var a = expected.Split('\n');
        var b = actual.Split('\n');
        var output = new StringBuilder();
        output.Append("--- ").Append(expectedName).Append('\n');
        output.Append("+++ ").Append(actualName).Append('\n');

        if ((long)a.Length * b.Length > MaxCells)
        {
            output.Append("(diff omitted: files are too large)\n");
            return output.ToString();
        }

        var ops = Compute(a, b);
        var written = 0;
        var i = 0;
        while (i < ops.Count)
        {
            if (ops[i].Kind == ' ')
            {
                i++;
                continue;
            }

            var start = Math.Max(0, i - Context);
            var end = HunkEnd(ops, i);
            var first = ops[start];
            var oldCount = Count(ops, start, end, '+');
            var newCount = Count(ops, start, end, '-');

            output.Append(
                $"@@ -{first.OldLine + 1},{oldCount} +{first.NewLine + 1},{newCount} @@\n"
            );
            for (var k = start; k < end; k++)
            {
                if (written++ >= MaxLines)
                {
                    output.Append("... (diff truncated)\n");
                    return output.ToString();
                }

                output.Append(ops[k].Kind).Append(ops[k].Text).Append('\n');
            }

            i = end;
        }

        return output.ToString();
    }

    private static int HunkEnd(List<Op> ops, int firstChange)
    {
        var lastChange = firstChange;
        for (var k = firstChange; k < ops.Count && k - lastChange <= Context * 2; k++)
        {
            if (ops[k].Kind != ' ')
            {
                lastChange = k;
            }
        }

        return Math.Min(ops.Count, lastChange + Context + 1);
    }

    private static int Count(List<Op> ops, int start, int end, char excludedKind)
    {
        var count = 0;
        for (var k = start; k < end; k++)
        {
            if (ops[k].Kind != excludedKind)
            {
                count++;
            }
        }

        return count;
    }

    private readonly record struct Op(char Kind, string Text, int OldLine, int NewLine);

    private static List<Op> Compute(string[] a, string[] b)
    {
        var lengths = new int[a.Length + 1, b.Length + 1];
        for (var i = a.Length - 1; i >= 0; i--)
        {
            for (var j = b.Length - 1; j >= 0; j--)
            {
                lengths[i, j] =
                    a[i] == b[j]
                        ? lengths[i + 1, j + 1] + 1
                        : Math.Max(lengths[i + 1, j], lengths[i, j + 1]);
            }
        }

        var ops = new List<Op>();
        int x = 0,
            y = 0;
        while (x < a.Length || y < b.Length)
        {
            if (x < a.Length && y < b.Length && a[x] == b[y])
            {
                ops.Add(new Op(' ', a[x], x, y));
                x++;
                y++;
            }
            else if (y < b.Length && (x == a.Length || lengths[x, y + 1] >= lengths[x + 1, y]))
            {
                ops.Add(new Op('+', b[y], x, y));
                y++;
            }
            else
            {
                ops.Add(new Op('-', a[x], x, y));
                x++;
            }
        }

        return ops;
    }
}
