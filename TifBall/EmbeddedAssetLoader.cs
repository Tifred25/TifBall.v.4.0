using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TifBall;

internal static class EmbeddedAssetLoader
{
    private const string ResourcePrefix = "TifBall.Embedded.";
    private static readonly Assembly AssetAssembly = typeof(EmbeddedAssetLoader).Assembly;
    private static readonly IReadOnlyList<string> ResourceNames = AssetAssembly.GetManifestResourceNames();

    public static Stream OpenRequired(string relativePath)
    {
        string resourceName = ResolveResourceName(relativePath);
        Stream? stream = AssetAssembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException("Embedded resource stream not found.", resourceName);
        }

        return stream;
    }

    public static string[] ReadAllLines(string relativePath)
    {
        using Stream stream = OpenRequired(relativePath);
        using StreamReader reader = new(stream);
        List<string> lines = new();
        while (!reader.EndOfStream)
        {
            lines.Add(reader.ReadLine() ?? string.Empty);
        }

        return lines.ToArray();
    }

    public static Texture2D LoadTexture(GraphicsDevice graphicsDevice, string relativePath, bool transparentBlack = false)
    {
        using Stream stream = OpenRequired(relativePath);
        Texture2D texture = Texture2D.FromStream(graphicsDevice, stream);

        if (!transparentBlack)
        {
            return texture;
        }

        Color[] pixels = new Color[texture.Width * texture.Height];
        texture.GetData(pixels);
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].R == 0 && pixels[i].G == 0 && pixels[i].B == 0)
            {
                pixels[i] = Color.Transparent;
            }
        }

        texture.SetData(pixels);
        return texture;
    }

    private static string ResolveResourceName(string relativePath)
    {
        string normalizedPath = relativePath
            .Replace('\\', '.')
            .Replace('/', '.')
            .Trim('.');
        string expectedResourceName = ResourcePrefix + normalizedPath;

        if (ResourceNames.Contains(expectedResourceName, StringComparer.Ordinal))
        {
            return expectedResourceName;
        }

        string? fallbackResourceName = ResourceNames.FirstOrDefault(
            name => name.EndsWith("." + normalizedPath, StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, expectedResourceName, StringComparison.OrdinalIgnoreCase));
        if (fallbackResourceName != null)
        {
            return fallbackResourceName;
        }

        throw new FileNotFoundException(
            string.Format(
                CultureInfo.InvariantCulture,
                "Embedded resource '{0}' was not found in assembly '{1}'.",
                relativePath,
                AssetAssembly.GetName().Name));
    }
}
