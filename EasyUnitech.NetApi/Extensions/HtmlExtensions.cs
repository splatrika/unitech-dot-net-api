using System;
using AngleSharp;
using AngleSharp.Dom;

namespace EasyUnitech.NetApi.Extensions;

public static class HtmlExtensions
{
	public async static Task<IDocument> ParseHtmlAsync(this string content)
	{
        var configuration = Configuration.Default;
        var context = BrowsingContext.New(configuration);
        return await context.OpenAsync(response => response.Content(content));
    }
}

