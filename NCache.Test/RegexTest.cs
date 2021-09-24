using System;
using System.Text.RegularExpressions;
using Xunit;

namespace NCache.Test
{
    public class RegexTest
    {
        [Fact]
        public void TestMatch()
        {
            Regex rgx = new Regex(@"(?i)(?<=\[)(.*)(?=\])");//中括号[]
            string str = "person[0]";
            string tmp = rgx.Match(str).Value;//中括号[]
            System.Diagnostics.Debug.Write(tmp);
            Assert.Equal("0", tmp);
        }

        [Fact]
        public void TestMatchFilter()
        {
            string pattern = "person[0][1]";
            pattern = Regex.Replace(pattern, @"\[.*\]", "");//过滤[]
            System.Diagnostics.Debug.Write(pattern);
            Assert.Equal("person", pattern);
        }
    }
}
