using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.Util;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Helper.Mapper
{
    public abstract class MappingBase : IProjectMappingHeper
    {
        public abstract IList<TestCopProjectItem> GetAssociatedProject(IProject currentProject, string currentNameSpace);

        public virtual bool IsTestProject(IProject project)
        {
            string currentProjectNamespace = project.GetDefaultNamespace();
            if (string.IsNullOrEmpty(currentProjectNamespace)) return false;

            return TestingRegEx.IsMatch(currentProjectNamespace);
        }

        protected virtual Regex TestingRegEx
        {
            get
            {
                var testNameSpacePattern = Settings.TestProjectToCodeProjectNameSpaceRegEx;
                var regEx = new Regex(testNameSpacePattern);
                return regEx;
            }
        }

        public void DumpDebug(ISolution solution)
        {
            var rx = TestingRegEx;
            solution.GetAllCodeProjects().ForEach(
                p => ResharperHelper.AppendLineToOutputWindow("\tProject Namespace:" + p.GetDefaultNamespace()
                                                              +
                                                              (rx.IsMatch(p.GetDefaultNamespace() ?? "")
                                                                  ? " matches "
                                                                  : " does not match ")
                                                              + rx));
        }

        protected static TestFileAnalysisSettings Settings
        {
            get { return TestCopSettingsManager.Instance.Settings; }
        }

        public static bool RegexReplace(string regexPattern, string regexReplaceText, string inputString, out string resultString)
        {
            resultString = "";
            var regex = new Regex(regexPattern);
            var match = regex.Match(inputString);

            if (match.Success && match.Groups.Count > 1)
            {
                if (regexReplaceText.IsNullOrEmpty() || regexReplaceText == "*")
                {
                    for (int i = 1; i < match.Groups.Count; i++) resultString += match.Groups[i].Value;
                    return true;
                }

                resultString = regex.Replace(inputString, regexReplaceText);
                return true;
            }
            return false;
        } 
    }
}