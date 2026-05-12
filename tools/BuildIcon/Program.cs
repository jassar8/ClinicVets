using ImageMagick;

/// <summary>
/// Builds a multi-resolution Windows .ico from a source PNG for sharp taskbar / title bar / EXE icons.
/// Usage: BuildIcon input.png output.ico
/// </summary>
internal static class Program
{
    private static readonly int[] Sizes = [16, 20, 24, 32, 40, 48, 64, 128, 256];

    private static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: BuildIcon <input.png> <output.ico>");
            return 1;
        }

        var input = Path.GetFullPath(args[0]);
        var output = Path.GetFullPath(args[1]);

        if (!File.Exists(input))
        {
            Console.Error.WriteLine($"Input not found: {input}");
            return 2;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(output)!);

        using var collection = new MagickImageCollection();
        using (var master = new MagickImage(input))
        {
            master.Alpha(AlphaOption.Set);
            foreach (var size in Sizes)
            {
                var frame = (MagickImage)master.Clone();
                frame.FilterType = FilterType.Lanczos;
                frame.Resize(new MagickGeometry((uint)size, (uint)size) { IgnoreAspectRatio = true });
                frame.Format = MagickFormat.Png32;
                frame.Alpha(AlphaOption.Set);
                collection.Add(frame);
            }
        }

        collection.Write(output, MagickFormat.Ico);
        Console.WriteLine($"Wrote {collection.Count} frames to {output}");
        return 0;
    }
}
