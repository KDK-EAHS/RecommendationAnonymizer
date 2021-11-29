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
        private Dictionary<string, List<string>> nameVariants;
        private Dictionary<string, string> pronouns;
        private Pipeline nlp;
        private List<IToken> tokens;

        /*
             * TODO:
             * - abstract parts of FixPronouns()                            | STARTED
             * - fix theirs in FixPronouns()                                |
             * - check for three-token he himself is/has/was etc.           |
             * - add dictionary with pronouns & method that loads it        | DONE
             * - add method to look for name variants                       |
             * - modify method to load name variants from an outside file?  |
             * - move tokenizing to inside of methods where applicable      |
             * - make GetTokens() into RefreshTokens() ???                  |
             * 
             * - check if working on a few more letters                     |
             * - clean things up & add comments                             |
             * - enable loading of the pipeline inside of the class?        |
             * 
             * (POTENTIAL) ISSUES:
             * - What if there is a he/she that doesn't refer to the student?
             * - What if teacher's name is the same as student's?
             * - What if the student's last name is used alone for some reason?
             * 
             * - Present-tense verbs keep their s's at the end when pronouns are changed
             * - Separated he/she...is/has/was does not get fixed completely
             */

        public Anonymizer(Pipeline nlp)
        {
            nameVariants = new Dictionary<string, List<string>>();
            pronouns = new Dictionary<string, string>();
            LoadNameVariants();
            LoadPronouns();
            this.nlp = nlp;
        }

        public string Anonymize(string firstName, string lastName, string letter)
        {
            string anonymizedLetter = "";

            

            string fullName = firstName + " " + lastName;

            anonymizedLetter = letter.Replace(fullName, "the student");
            anonymizedLetter = anonymizedLetter.Replace(firstName, "the student");
            // Look for name variations somewhere here

            tokens = GetTokens(anonymizedLetter);  // move this to inside the methods?
            
            foreach(IToken token in tokens)
            {
                Console.WriteLine(token.ToString() + " " + token.POS);
            }

            anonymizedLetter = FixPronouns(tokens, anonymizedLetter);

            tokens = GetTokens(anonymizedLetter);
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
                else if (i == 0)
                {
                    string start = fixedText.Substring(0, 1).ToUpper();
                    string end = fixedText.Substring(1);

                    fixedText = start + end;
                }
            }

            return fixedText;
        }

        private string FixPronouns(List<IToken> tokens, string text)
        {
            string fixedText = text;

            for (int i = 0; i < tokens.Count - 1; i++)
            {
                tokens = GetTokens(fixedText);
                IToken token = tokens[i];
                IToken nextToken = tokens[i + 1];

                List<string> tries = GetTries(fixedText, token, nextToken);

                foreach (string t in tries)
                {
                    if (fixedText != t)
                    {
                        fixedText = t;
                        break;
                    }
                }

                // ISSUE: what if there's something like: he most certainly is...?
                // ...four word fix?
                // so then a three word fix for: he himself is/has/was/verb(s)...

                tokens = GetTokens(fixedText);
            }

            Console.WriteLine(fixedText);

            return fixedText;
        }

        private List<string> GetTries(string fixedText, IToken token, IToken nextToken)
        {
            List<string> tries = new List<string>();

            tries.Add(TwoWordFix(fixedText, "subject", "is", "they are", token, nextToken));
            tries.Add(TwoWordFix(fixedText, "subject", "has", "they have", token, nextToken));
            tries.Add(TwoWordFix(fixedText, "subject", "was", "they were", token, nextToken));

            tries.Add(OneWordFix(fixedText, "subject", "they", token));
            tries.Add(OneWordFix(fixedText, "object", "them", token));
            tries.Add(OneWordFix(fixedText, "possessiveAdj", "their", token));
            tries.Add(OneWordFix(fixedText, "possessivePron", "theirs", token));
            tries.Add(OneWordFix(fixedText, "reflexive", "themself", token));

            return tries;
        }

        private string TwoWordFix(string text, string key, string condition, string insert, IToken token, IToken nextToken)
        {
            if(pronouns[key].Contains($" {token.Value} ") && nextToken.Value == condition)
            {
                
                string start = text.Substring(0, token.Begin);
                string end = text.Substring(nextToken.End + 1);

                text = start + insert + end;
            }

            tokens = GetTokens(text);

            return text;
        }

        private string OneWordFix(string text, string key, string insert, IToken token)
        {
            if (pronouns[key].Contains($" {token.Value} "))
            {
                string start = text.Substring(0, token.Begin);
                string end = text.Substring(token.End + 1);

                text = start + insert + end;
            }

            tokens = GetTokens(text);

            return text;
        }

        private List<IToken> GetTokens(string letter)
        {
            var text = new Document(letter, Language.English);
            nlp.ProcessSingle(text);
            
            return text.ToTokenList();
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

        private void LoadPronouns()
        {
            pronouns.Add("subject", " he she He She ");
            pronouns.Add("object", " him her Him Her ");
            pronouns.Add("possessiveAdj", " his her His Her ");
            pronouns.Add("possessivePron", " his hers His Hers ");
            pronouns.Add("reflexive", " himself herself Himself Herself ");
        }
    }
}
