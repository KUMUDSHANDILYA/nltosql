
using System.Collections.Generic;

public class NL2SQLRequest
{
    public string Question { get; set; }
}

public class NL2SQLResponse
{
    public string SqlQuery { get; set; }
    public List<Dictionary<string, object>> Result { get; set; }
}