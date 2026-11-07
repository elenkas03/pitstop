using System.Text;

namespace PitstopWeb.Services;

public static class SlugHelper
{
    private static readonly Dictionary<char, string> CyrillicToLatin = new()
    {
        ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "g", ['д'] = "d",
        ['е'] = "e", ['ж'] = "zh", ['з'] = "z", ['и'] = "i", ['й'] = "y",
        ['к'] = "k", ['л'] = "l", ['м'] = "m", ['н'] = "n", ['о'] = "o",
        ['п'] = "p", ['р'] = "r", ['с'] = "s", ['т'] = "t", ['у'] = "u",
        ['ф'] = "f", ['х'] = "h", ['ц'] = "ts", ['ч'] = "ch", ['ш'] = "sh",
        ['щ'] = "sht", ['ъ'] = "a", ['ь'] = "y", ['ю'] = "yu", ['я'] = "ya",
    };

    public static string Slugify(string text)
    {
        var sb = new StringBuilder();
        foreach (var ch in text.ToLowerInvariant())
        {
            if (CyrillicToLatin.TryGetValue(ch, out var latin))
            {
                sb.Append(latin);
            }
            else
            {
                sb.Append(ch);
            }
        }

        var transliterated = sb.ToString();
        var slugChars = transliterated.Select(c => char.IsLetterOrDigit(c) && c < 128 ? c : '-').ToArray();
        var slug = new string(slugChars);

        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        slug = slug.Trim('-');

        return string.IsNullOrEmpty(slug) ? Guid.NewGuid().ToString("N")[..8] : slug;
    }
}
