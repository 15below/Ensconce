using Ensconce.NDjango.Core.Filters.HtmlFilters;
using Ensconce.NDjango.Core.Filters.List;
using Ensconce.NDjango.Core.Filters.StringFilters;

namespace Ensconce.NDjango.Core.Filters
{
    public static class Loader
    {
        public static readonly Filter[] StringFilters =
        {
            new Filter("add", new AddFilter()),
            new Filter("get_digit", new GetDigit()),
            new Filter("divisibleby", new DivisibleByFilter()),
            new Filter("addslashes", new AddSlashesFilter()),
            new Filter("capfirst", new CapFirstFilter()),
            new Filter("escapejs", new EscapeJSFilter()),
            new Filter("fix_ampersands", new FixAmpersandsFilter()),
            new Filter("floatformat", new FloatFormatFilter()),
            new Filter("linenumbers", new LineNumbersFilter()),
            new Filter("lower", new LowerFilter()),
            new Filter("upper", new UpperFilter()),
            new Filter("make_list", new MakeListFilter()),
            new Filter("wordcount", new WordCountFilter()),
            new Filter("ljust", new LJustFilter()),
            new Filter("rjust", new RJustFilter()),
            new Filter("center", new CenterFilter()),
            new Filter("cut", new CutFilter()),
            new Filter("title", new TitleFilter())
        };

        public static readonly Filter[] HtmlFilters =
        {
            new Filter("removetags", new RemoveTagsFilter())
        };

        public static readonly Filter[] ListFilters =
        {
            new Filter("first", new FirstFilter()),
            new Filter("last", new LastFilter()),
            new Filter("length", new LengthFilter()),
            new Filter("length_is", new LengthIsFilter()),
            new Filter("random", new RandomFilter()),
            new Filter("slice", new SliceFilter()),
        };
    }
}
