using System.IO;
using System.Linq;
using TechTalk.SpecFlow;

namespace UnitTests;

public sealed partial class CheatsRepositorySteps
{
    [When(@"I deserialize a file with the content")]
    public void WhenIDeserializeAFileWithTheContent(string cheatFileContent)
    {
        using var memStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memStream);
        using var reader = new StreamReader(memStream);
        
        streamWriter.Write(cheatFileContent);
        streamWriter.Flush();
        memStream.Seek(0, SeekOrigin.Begin);

        currentCheats = repository.DeserializeFrom(reader).ToList();
    }
}