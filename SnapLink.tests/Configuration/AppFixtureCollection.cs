using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnapLink.Tests.Configuration;

namespace SnapLink.tests.Configuration
{
    [CollectionDefinition("App collection")]
    public class AppFixtureCollection : ICollectionFixture<AppFixture<TestProgram>>
    {
        
    }
}
