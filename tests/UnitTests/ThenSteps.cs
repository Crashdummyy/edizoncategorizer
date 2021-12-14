using System;
using System.Linq;
using EdizonCategorizer.Data;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace UnitTests;

public sealed partial class CheatsRepositorySteps
{
    [Then(@"exists (\d+) sections")]
    public void ThenExistsDSections(int count)
    {
        currentCheats.Should()
                     .HaveCount(count);
    }

    [Then(@"the section ""(.*)"" contains (\d+) cheats")]
    public void ThenTheSectionContainsCheats(string sectionName, int count) => GetSection(sectionName).Cheats.Should().HaveCount(count);

    [Then(@"the section ""(.*)"" contains the cheats")]
    public void ThenTheSectionContainsTheCheats(string sectionName,
                                                Table table)
    {
        var section = GetSection(sectionName);

        foreach (var row in table.Rows)
            section.Cheats.Should().Contain(x => x.Name.Equals(row["Name"]) && x.Content.Replace(Environment.NewLine, " ").Trim().Equals(row["Value"].Trim()));
    }

    private CheatSection GetSection(string sectionName) 
        => currentCheats.SingleOrDefault(x => x.Name.Equals(sectionName)) 
           ?? throw new SpecFlowException($"No Section with the name {sectionName} exists. Possible sections are:" 
                                          + Environment.NewLine 
                                          + string.Join(Environment.NewLine, currentCheats.Select(x => x.Name)));
}