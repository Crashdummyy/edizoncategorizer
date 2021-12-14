using System.Text.RegularExpressions;

namespace EdizonCategorizer.Data
{
    public class CheatsRepository
    {
        public IEnumerable<CheatSection> DeserializeFrom(StreamReader reader)
        {
            var results =new List<CheatSection>
            {
                new("UnCategorized", new List<Cheat>())
            };
            
            var currentLine = NextLine(reader);
            while (!reader.EndOfStream && string.IsNullOrWhiteSpace(currentLine))
                currentLine = NextLine(reader);

            if (reader.EndOfStream)
                return results;
            
            var nextLine = NextLine(reader);
            
            while (!reader.EndOfStream)
            {
                var section = ReadOneSection(reader, ref currentLine, ref nextLine);
                Action doStuff = section.Name.Equals("UnCategorized")
                    ? () => results.First().Cheats.AddRange(section.Cheats)
                    : () => results.Add(section);

                doStuff();
            }

            return results;
        }

        public IEnumerable<CheatSection> DeserializeFrom(string path)
        {
            using var reader = File.OpenText(path);
            return DeserializeFrom(reader);
        }

        private CheatSection ReadOneSection(StreamReader reader,
                                            ref string currentLine,
                                            ref string nextLine)
        {
            var result = new CheatSection();

            (currentLine, nextLine) = IgnoreBullshit(reader, currentLine, nextLine);
            (currentLine,nextLine) = TryAddSection(reader, currentLine, nextLine, result);

            while (!currentLine.StartsWith("[") || !nextLine.StartsWith("00000000"))
            {
                (currentLine, nextLine) = TryAddCheat(reader, currentLine, nextLine, result);

                // File ended with oneliner cheat
                if (reader.EndOfStream && currentLine.StartsWith("["))
                {
                    result.Cheats.Add(new Cheat(currentLine.Trim(), nextLine.Trim()));
                    break;
                }
                
                if (reader.EndOfStream)
                    break;
            }


            return result;
        }

        private (string currentLine, string nextLine) IgnoreBullshit(StreamReader reader,
                                                                     string currentLine,
                                                                     string nextLine)
        {
            if (!reader.EndOfStream && (string.IsNullOrWhiteSpace(currentLine) || currentLine.StartsWith("[")) && (string.IsNullOrWhiteSpace(nextLine) || nextLine.StartsWith("[")))
            {
                return IgnoreBullshit(reader, nextLine, NextLine(reader));
            }

            return (currentLine, nextLine);
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
            while ( !currentLine.StartsWith("["))
            {
                code += Environment.NewLine + currentLine.Trim();
                currentLine = NextLine(reader);

                if (currentLine.StartsWith("["))
                    break;
                
                if (reader.EndOfStream && string.IsNullOrWhiteSpace(currentLine))
                    break;

                if (reader.EndOfStream)
                {
                    code += Environment.NewLine + currentLine.Trim();
                    break;
                }
            }
                
            result.Cheats.Add(new Cheat(name,code));
            return (currentLine, NextLine(reader));
        }

        private (string,string) TryAddSection(StreamReader reader, string currentLine, string nextLine, CheatSection result)
        {
            if (!currentLine.StartsWith("[") || !nextLine.StartsWith("00000000")) 
                return (currentLine,nextLine);

            if (currentLine.Contains("SectionEnd:"))
            {
                NextLine(reader);
                return TryAddSection(reader, NextLine(reader), NextLine(reader), result);
            }
            
            result.Name = Regex.Replace(Regex.Replace(currentLine, 
                                                      @"\[(\-+)(?:\s+|)",
                                                      string.Empty),
                                        @"(?:\s+|)(?:\-+|)\](?:\s+|)",
                                        string.Empty)
                               .Replace("SectionStart:", string.Empty); // allow editing existing sectioned files
            
            currentLine = string.Empty;
            while (string.IsNullOrWhiteSpace(currentLine) && !reader.EndOfStream)
                currentLine = NextLine(reader);

            return (currentLine, NextLine(reader));
        }

        private string NextLine(StreamReader reader) => reader.ReadLine()?.Trim() ?? string.Empty;

        public async Task<Stream> Serialize(List<CheatSection> cheats)
        {
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.AutoFlush = true;
        
            foreach (var cheatSection in cheats.Where(x => x.Cheats.Any() && !x.Name.Equals("UnCategorized")))
            {
                await writer.WriteLineAsync($"[--SectionStart:{cheatSection.Name}--]");
                await writer.WriteLineAsync("00000000 00000000 00000000" + Environment.NewLine);
                foreach (var (name, content) in cheatSection.Cheats)
                    await writer.WriteLineAsync(name + Environment.NewLine + content + Environment.NewLine);
            
                await writer.WriteLineAsync($"[--SectionEnd:{cheatSection.Name}--]");
                await writer.WriteLineAsync("00000000 00000000 00000000" + Environment.NewLine);
            }
        
            cheats.FirstOrDefault(x => x.Name.Equals("UnCategorized"))?
                .Cheats.ForEach(x => writer.WriteLine(x.Name + Environment.NewLine + x.Content));
        
            ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }
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

    public class Cheat
    {
        public Cheat(string name, string content)
        {
            Name = name;
            Content = content;
        }

        public Cheat()
        {
            Name = string.Empty;
            Content = string.Empty;
        }

        public string Name { get; set; }
        public string Content { get; set; }

        public void Deconstruct(out string name,
                                out string content)
        {
            name = Name;
            content = Content;
        }
    }
}
