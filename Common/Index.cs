using System.Collections.Generic;

namespace Generation;

public class Index
{
    public List<string> Columns { get; set; }

    public bool IsUnique { get; set; }

    public string Filter { get; set; }
}
