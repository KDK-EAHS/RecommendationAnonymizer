using Catalyst;
using Mosaik.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommendationAnonymizer
{
    internal class Anonymizer
    {
        public Dictionary<string, List<string>> nameVariants;
        private Pipeline nlp;

        public Anonymizer(Pipeline nlp)
        {
            nameVariants = new Dictionary<string, List<string>>();
            LoadNameVariants();
            this.nlp = nlp;
            //LoadPipeline();
        }

        public string Anonymize(string firstName, string lastName, string letter)
        {
            string anonymizedLetter = "";

            /*
             * NOTES:
             * - find any full names first
             * - find first names second
             * - find variations of first names
             * - find any lone last names?
             * - fix: her, hers, herself
             * - fix capitalization last?
             */

            string fullName = firstName + " " + lastName;

            anonymizedLetter = letter.Replace(fullName, "the student");
            anonymizedLetter = anonymizedLetter.Replace(firstName, "the student");
            // Look for name variations somewhere here

            //string test = "Hello, I am a robot. i am a rabbit. i am. i am n o t.";

            List<IToken> tokens = GetTokens(anonymizedLetter);

            //FixPronouns(tokens);
            anonymizedLetter = FixCapitalization(tokens, anonymizedLetter);

            return anonymizedLetter;
        }

        private string FixCapitalization(List<IToken> tokens, string text)
        {
            string fixedText = text;

            for(int i = 0; i < tokens.Count - 1; i++)
            {
                IToken token = tokens[i];
                IToken nextToken = tokens[i + 1];

                if(token.Value == ".")
                {
                    int index = nextToken.Begin;
                    string start = fixedText.Substring(0, index);
                    string toFix = fixedText.Substring(index, 1).ToUpper();
                    string end = fixedText.Substring(index + 1);

                    //Console.WriteLine(token.Begin);

                    fixedText = start + toFix + end;
                }
            }

            return fixedText;
        }

        private void FixPronouns(List<IToken> tokens)
        {
            throw new NotImplementedException();
        }

        private List<IToken> GetTokens(string letter)
        {
            var toBeAnonymized = new Document(letter, Language.English);
            nlp.ProcessSingle(toBeAnonymized);
            
            return toBeAnonymized.ToTokenList();
        }

        private void LoadNameVariants() // FOR LATER: maybe account for if there are no variants to load?
        {
            string[] lines = File.ReadAllLines(@"C:\Users\limon\Documents\GitHub\RecommendationAnonymizer\RecommendationAnonymizer\variants.csv");
            foreach(string line in lines)
            {
                List<string> names = line.Split(new char[] { ',' }).ToList();
                string key = names[0];

                List<string> variants = names.GetRange(1, names.Count - 1);
                
                nameVariants.Add(key, variants);
            }
        }
        /*
        private async Task LoadPipeline()
        {
            nlp = await Pipeline.ForAsync(Language.English);
        }
        */
    }
}
