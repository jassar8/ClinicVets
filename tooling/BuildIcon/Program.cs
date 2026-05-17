using ImageMagick;

/// <summary>
/// Builds branding PNGs and a multi-resolution Windows .ico from the source logo
/// without recoloring, tinting, or aggressive matte removal.
/// Usage: BuildIcon &lt;input.png&gt; &lt;output.ico&gt;
/// </summary>
internal static class Program
{
    private static readonly int[] IcoSizes = [16, 32, 48, 64, 128, 256];

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
        var brandingDir = Path.GetDirectoryName(input)!;

        using var master = PrepareFaithfulLogo(input);
        var masterPath = Path.Combine(brandingDir, "ClinicVetsLogo.png");
        WritePng(master, masterPath);
        Console.WriteLine($"Wrote faithful master {masterPath} ({master.Width}x{master.Height})");

        var previewPath = Path.Combine(Path.GetDirectoryName(output)!, "icon-preview-256.png");
        using (var preview = (MagickImage)master.Clone())
        {
            ResizeFrame(preview, 256);
            WritePng(preview, previewPath);
            Console.WriteLine($"Wrote preview {previewPath}");
        }

        WriteIco(master, output);
        ExportUiPngs(master, brandingDir);
        return 0;
    }

    /// <summary>
    /// Keeps original sRGB pixels; only removes outer white matting via corner flood-fill (low fuzz).
    /// </summary>
    private static MagickImage PrepareFaithfulLogo(string inputPath)
    {
        using var loaded = new MagickImage(inputPath);
        PreserveOriginalColors(loaded);

        loaded.Alpha(AlphaOption.Set);
        loaded.BackgroundColor = MagickColors.Transparent;

        // Remove only the exterior white canvas — never recolor logo interior pixels.
        loaded.ColorFuzz = new Percentage(5);
        var w = (int)loaded.Width - 1;
        var h = (int)loaded.Height - 1;
        foreach (var (x, y) in new[] { (0, 0), (w, 0), (0, h), (w, h) })
            loaded.FloodFill(MagickColors.Transparent, x, y);

        loaded.Trim();
        loaded.Page = new MagickGeometry(loaded.Width, loaded.Height);

        var side = Math.Max(loaded.Width, loaded.Height);
        var pad = (uint)Math.Max(4, (int)(side * 0.04));
        var canvas = new MagickImage(MagickColors.Transparent, side + pad * 2, side + pad * 2);
        PreserveOriginalColors(canvas);
        canvas.Composite(loaded, Gravity.Center, CompositeOperator.Over);
        canvas.Alpha(AlphaOption.Activate);
        canvas.Depth = 8;
        return canvas;
    }

    private static void PreserveOriginalColors(MagickImage image)
    {
        image.ColorSpace = ColorSpace.sRGB;
        image.Depth = 8;
        image.Quality = 100;
        image.Settings.SetDefine(MagickFormat.Png, "compression-level", "1");
        image.Settings.SetDefine(MagickFormat.Png, "exclude-chunk", "date,time");
        image.Settings.SetDefine(MagickFormat.Png, "preserve-iCCP", "true");
        image.Settings.SetDefine(MagickFormat.Png, "auto-gamma", "off");
    }

    private static void ResizeFrame(MagickImage frame, int size)
    {
        frame.FilterType = FilterType.Lanczos;
        frame.Settings.AntiAlias = true;
        frame.Resize((uint)size, (uint)size);
        frame.BackgroundColor = MagickColors.Transparent;
        frame.Alpha(AlphaOption.Activate);
        frame.Depth = 8;
    }

    private static void WritePng(MagickImage image, string path)
    {
        using var copy = (MagickImage)image.Clone();
        PreserveOriginalColors(copy);
        copy.Format = MagickFormat.Png32;
        copy.Alpha(AlphaOption.Activate);
        copy.Write(path, MagickFormat.Png32);
    }

    private static void WriteIco(MagickImage master, string output)
    {
        using var collection = new MagickImageCollection();
        foreach (var size in IcoSizes)
        {
            var frame = (MagickImage)master.Clone();
            PreserveOriginalColors(frame);
            ResizeFrame(frame, size);
            frame.Format = MagickFormat.Png32;
            frame.Settings.Compression = CompressionMethod.NoCompression;
            collection.Add(frame);
        }

        collection.Write(output, MagickFormat.Ico);
        Console.WriteLine($"Wrote {collection.Count} ICO frames to {output} (faithful colors)");
    }

    private static void ExportUiPngs(MagickImage master, string brandingDir)
    {
        var sizes = new (int Size, string FileName)[]
        {
            (64, "logo-64.png"),
            (128, "logo-128.png"),
            (256, "logo-256.png"),
            (512, "logo-512.png")
        };

        foreach (var (size, fileName) in sizes)
        {
            using var frame = (MagickImage)master.Clone();
            PreserveOriginalColors(frame);
            ResizeFrame(frame, size);
            var path = Path.Combine(brandingDir, fileName);
            WritePng(frame, path);
            Console.WriteLine($"Wrote UI asset {path} ({size}x{size})");
        }
    }
}
