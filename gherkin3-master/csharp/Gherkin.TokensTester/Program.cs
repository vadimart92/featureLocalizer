using System;
using System.IO;
using System.Linq;


namespace Gherkin.TokensTester
{
    class Program
    {
        static int Main(string[] args)
        {

			string featureFilePath = @"F:\DEV\sales\features\order\order_folder.feature";

            return TestTokens(featureFilePath);
        }

        private static int TestTokens(string featureFilePath)
        {
            try
            {
                return TestTokensInternal(featureFilePath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }

        private static int TestTokensInternal(string featureFilePath)
        {
            var parser = new Parser<object>();
	        using (var reader = new StreamReader(featureFilePath)) {
		       // parser.Parse(new TokenScanner(reader), new TokenMatcher(), tokenFormatterBuilder);
				var f = (Ast.Feature)parser.Parse(new TokenScanner(reader));
				
	        }

            return 0;
        }
    }
}
