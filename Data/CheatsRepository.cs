using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EdizonCategorizer.Data
{
    public class CheatsRepository
    {
        public IEnumerable<CheatSection> DeserializeFrom(string path)
        {
            using var reader = File.OpenText(path);
            
            var results =new List<CheatSection> { new("UnCategorized", new List<Cheat>()) };
            
            var currentLine = NextLine(reader);
            while (!reader.EndOfStream && string.IsNullOrWhiteSpace(currentLine))
                currentLine = NextLine(reader);

            if (reader.EndOfStream)
                return Enumerable.Empty<CheatSection>();
            
            var nextLine = NextLine(reader);
            
            while (!reader.EndOfStream)
            {
                var section = ReadOneSection(reader, ref currentLine, ref nextLine);
                Action doStuff = section.Name.Equals("UnCategorized")
                    ? () => results[0].Cheats.AddRange(section.Cheats)
                    : () => results.Add(section);

                doStuff();
            }

            return results;
        }

        private CheatSection ReadOneSection(StreamReader reader,
                                            ref string currentLine,
                                            ref string nextLine)
        {
            var result = new CheatSection();

            (currentLine,nextLine) = TryAddSection(reader, currentLine, nextLine, result);

            while (!currentLine.StartsWith("[") || !nextLine.StartsWith("00000000"))
            {
                if (reader.EndOfStream)
                    break;
                (currentLine, nextLine) = TryAddCheat(reader, currentLine, nextLine, result);
            }


            return result;
        }

        private (string, string) TryAddCheat(StreamReader reader,
                                             string currentLine,
                                             string nextLine,
                                             CheatSection result)
        {
            if (!currentLine.StartsWith("[") || nextLine.StartsWith("00000000"))
                return (currentLine, nextLine);
            
            var name = currentLine.Trim();
            var code = nextLine.Trim();

            currentLine = NextLine(reader);
            while ( !currentLine.StartsWith("[") && !reader.EndOfStream)
            {
                code += Environment.NewLine + currentLine.Trim();
                currentLine = NextLine(reader);
            }
                
            result.Cheats.Add(new Cheat(name,code));
            return (currentLine, NextLine(reader));
        }

        private (string,string) TryAddSection(StreamReader reader, string currentLine, string nextLine, CheatSection result)
        {
            if (!currentLine.StartsWith("[") || !nextLine.StartsWith("00000000")) 
                return (currentLine,nextLine);
            
            result.Name = currentLine.Trim();
            currentLine = string.Empty;
            while (string.IsNullOrWhiteSpace(currentLine))
                currentLine = NextLine(reader);

            return (currentLine, NextLine(reader));
        }

        private string NextLine(StreamReader reader) => reader.ReadLine()?.Trim() ?? string.Empty;
    }
    
    public class CheatSection
    {
        public CheatSection()
        {
            Cheats = new List<Cheat>();
            Name = "UnCategorized";
        }
        
        public CheatSection(string name, List<Cheat> cheats)
        {
            Name = name;
            Cheats = cheats;
        }

        public string Name { get; set; }
        public List<Cheat> Cheats { get;  }
    }
    public record Cheat(string Name, string Content);
}