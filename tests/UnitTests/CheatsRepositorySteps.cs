using System.Collections.Generic;
using EdizonCategorizer.Data;
using TechTalk.SpecFlow;

namespace UnitTests
{
    [Binding]
    public sealed partial class  CheatsRepositorySteps
    {
        private readonly CheatsRepository repository;
        private List<CheatSection> currentCheats;

        public CheatsRepositorySteps(ScenarioContext context)
        {
            currentCheats = new List<CheatSection>();
            repository = new CheatsRepository();
        }
    }
}