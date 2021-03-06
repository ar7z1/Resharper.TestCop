// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Util;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Helper.Mapper
{
    public class CodeProjectMapsToSingleTestProjectHeper : MappingBase
    {
        protected override Regex TestingRegEx
        {
            get
            {
                var testNameSpacePattern = Settings.SingleTestRegexTestToAssembly;
                var regEx = new Regex(testNameSpacePattern);
                return regEx;
            }
        }

        public override IList<TestCopProjectItem> GetAssociatedProject(IProject currentProject, string currentTypeNamespace)
        {
            var settings = Settings;
            const string warningMessage = "Not Supported: More than one code project has a default namespace of ";
          
            if (currentProject.IsTestProject())
            {                
                // <MyCorp.App.Tests>.API.ClassA --> <MyCorp.App.API>.ClassA
                string nameSpaceOfAssociateProject;
                RegexReplace(settings.SingleTestRegexTestToAssembly
                    , settings.SingleTestRegexTestToAssemblyProjectReplace, currentTypeNamespace,
                    out nameSpaceOfAssociateProject);

                string subNameSpace;
                RegexReplace(settings.SingleTestRegexTestToAssembly
                    , settings.SingleTestRegexTestToAssemblyProjectSubNamespaceReplace, currentTypeNamespace,
                     out subNameSpace);

                var matchedCodeProjects = currentProject.GetSolution().GetNonTestProjects().Where(
                    p => p.GetDefaultNamespace()  == nameSpaceOfAssociateProject).ToList();

                if (matchedCodeProjects.Count() > 1)
                {
                    ResharperHelper.AppendLineToOutputWindow(warningMessage + nameSpaceOfAssociateProject);
                }

                return matchedCodeProjects.Select(p => new TestCopProjectItem(p, subNameSpace)).ToList();
            }

            string subNameSpaceOfTest;
            RegexReplace(settings.SingleTestRegexCodeToTestAssembly
                , settings.SingleTestRegexCodeToTestReplace, currentTypeNamespace,
                out subNameSpaceOfTest);
            
            var matchedTestProjects = currentProject.GetSolution().GetTestProjects().ToList();
            if (matchedTestProjects.Count > 1)
            {
                ResharperHelper.AppendLineToOutputWindow("Not Supported: Expected only one test project for all code projects to use");                
            }

            return matchedTestProjects.Select(p => new TestCopProjectItem(p, subNameSpaceOfTest)).Take(1).ToList();                                        
        }
    }
}