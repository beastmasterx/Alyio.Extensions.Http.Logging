// MIT License

namespace Alyio.Extensions.Http.Logging;

internal static class MimeTypeChecker
{
    private static readonly HashSet<string> s_textBasedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Generic plaintext
        "text/plain",
        "text/csv",
        "text/tab-separated-values",
        "text/markdown",
        "text/rtf",

        // Web content and scripting
        "text/html",
        "text/css",
        "text/javascript",
        "text/ecmascript",
        "text/jscript",
        "text/livescript",
        "application/javascript",

        // Structured data formats
        "application/json",
        "application/xml",
        "application/yaml",
        "application/ld+json",
        "application/vnd.api+json",
        "application/manifest+json",
        "image/svg+xml",
        "application/atom+xml",
        "application/rss+xml",
        "application/xhtml+xml",
        "application/app+yaml",
        "application/kubernetes+yaml",
        "application/ansible+yaml",

        // Calendar and contact formats
        "text/calendar",
        "text/vcard",
        "text/x-vcard",

        // Form-encoded data
        "application/x-www-form-urlencoded",

        // Other text-based formats
        "text/enriched",
        "text/h323",
        "text/prs.lines.tag",
        "text/richtext",
        "text/sgml",
        "text/uri-list",
        "text/vnd.abc",
        "text/vnd.curl",
        "text/vnd.curl.dcurl",
        "text/vnd.curl.mcurl",
        "text/vnd.curl.scurl",
        "text/vnd.fly",
        "text/vnd.fmi.flexstor",
        "text/vnd.graphviz",
        "text/vnd.in3d.3dml",
        "text/vnd.in3d.spot",
        "text/vnd.sun.j2me.app-descriptor",
        "text/vnd.wap.wml",
        "text/vnd.wap.wmlscript",
        "text/x-asm",
        "text/x-c",
        "text/x-component",
        "text/x-fortran",
        "text/x-java-source",
        "text/x-pascal",
        "text/x-script",
        "text/x-script.csh",
        "text/x-script.elisp",
        "text/x-script.ksh",
        "text/x-script.lisp",
        "text/x-script.perl",
        "text/x-script.perl-module",
        "text/x-script.phyton",
        "text/x-script.rexx",
        "text/x-script.scheme",
        "text/x-script.sh",
        "text/x-script.tcl",
        "text/x-script.tcsh",
        "text/x-script.zsh",
        "text/x-server-parsed-html",
        "text/x-setext",
        "text/x-speech",
        "text/x-uuencode",
        "text/x-vcalendar"
    };

    /// <summary>
    /// Checks if a given MIME type string represents a text-based format.
    /// </summary>
    /// <param name="mimeType">The MIME type string to check.</param>
    /// <returns>True if the MIME type is text-based, otherwise false.</returns>
    public static bool IsTextBased(string mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
        {
            return false;
        }

        var normalizedMimeType = mimeType.Split(';')[0].Trim();

        return normalizedMimeType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
            || normalizedMimeType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)
            || normalizedMimeType.EndsWith("+xml", StringComparison.OrdinalIgnoreCase)
            || normalizedMimeType.EndsWith("+yaml", StringComparison.OrdinalIgnoreCase)
            || s_textBasedMimeTypes.Contains(normalizedMimeType);
    }
}