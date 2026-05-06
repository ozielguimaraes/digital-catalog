using System.Web;

namespace MeuCatalogo.API.Infrastructure;

public static class ScalarHtml
{
    public static string Render(string openApiUrl, string title)
    {
        var safeUrl = HttpUtility.HtmlAttributeEncode(openApiUrl);
        var safeTitle = HttpUtility.HtmlEncode(title);
        return $@"<!doctype html>
<html lang=""pt-BR"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
    <title>{safeTitle}</title>
    <link rel=""icon"" href=""data:,"" />
    <style>body {{ margin: 0; }}</style>
</head>
<body>
    <script id=""api-reference"" data-url=""{safeUrl}"" data-configuration='{{""theme"":""purple"",""layout"":""modern"",""hideDownloadButton"":false,""showSidebar"":true}}'></script>
    <script src=""https://cdn.jsdelivr.net/npm/@scalar/api-reference""></script>
</body>
</html>";
    }
}
