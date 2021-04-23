using Ensconce.NDjango.Core;

namespace Ensconce.NDjango.Custom.Filters
{
    public static class Loader
    {
        public static readonly Filter[] CustomFilters =
        {
            new Filter("concat", new ConcatFilter()),
            new Filter("default", new DefaultFilter()), //DO NOT USE STANDARD DEFAULT FILTER
            new Filter("exists", new ExistsFilter()),
            new Filter("empty", new EmptyFilter()),
            new Filter("contains", new ContainsFilter()),
            new Filter("startsWith", new StartsWithFilter()),
            new Filter("endsWith", new EndsWithFilter()),
            new Filter("decrypt", new DecryptFilter()),
            new Filter("encrypt", new EncryptFilter())
        };
    }
}
