using System.Collections.Generic;
using Holism.Infra;
using System.Linq;

namespace Holism.Generation;

public class View
{
    public string Name { get; set; }

    public List<string> Query { get; set; }

    public string ConcatenatedQuery 
    {
        get
        {
            if (Query == null)
            {
                throw new ClientException("View should have a non-empty query");
            }
            if (Query.Any(i => i.EndsWith(" ")))
            {
                throw new ClientException("View lines should not have empty ending. Trim view lines at the end.");
            }
            var concatenatedQuery = string.Join(' ', Query);
            concatenatedQuery = concatenatedQuery.Replace("Date", "UtcDate");
            concatenatedQuery = concatenatedQuery.Replace("UtcUtc", "Utc");
            return concatenatedQuery;
        }
    }
}
