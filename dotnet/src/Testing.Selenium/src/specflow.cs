using TechTalk.SpecFlow;

// this should probably be in a separate project
public class GherkinTable
{

    static public Table make(string[] header, string[] data)
    {
        var r = new Table(header);
        for (var i = 0; i < data.Count() / header.Count(); i++)
        {
            r.AddRow(data.Skip(header.Count() * i).Take(header.Count()).ToArray());
        }
        return r;
    }

}