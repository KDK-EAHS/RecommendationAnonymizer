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
             * TODO:
             * - abstract parts of FixPronouns()
             * - fix theirs in FixPronouns()
             * - check for three-token he himself is/has/was etc.
             * - add method to look for name variants
             * - modify method to load name variants from an outside file?
             * - move tokenizing to inside of methods where applicable
             * - check if working on a few more letters
             * - clean things up & add comments
             * - enable loading of the pipeline inside of the class?
             */

            string fullName = firstName + " " + lastName;

            anonymizedLetter = letter.Replace(fullName, "the student");
            anonymizedLetter = anonymizedLetter.Replace(firstName, "the student");
            // Look for name variations somewhere here

            string test = "He is blue. You like him. This is his cat. That cat is his. He himself is king. He is there by himself.";

            List<IToken> tokens = GetTokens(anonymizedLetter);  // move this to inside the methods?
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
            string subjectPronouns = "he,she";
            string objectPronouns = "him,her"; // this has i and that's why i also gets changed
            string possessiveAdj = "his,her";
            string possessivePronouns = "his,hers";
            string reflexivePronouns = "himself,herself";

            for (int i = 0; i < tokens.Count - 1; i++)
            {
                tokens = GetTokens(fixedText);
                IToken token = tokens[i];
                IToken nextToken = tokens[i + 1];

                if(token.Value == "I") { continue; }

                if (token.POS == PartOfSpeech.PRON && nextToken.Value.ToLower() == "is" && subjectPronouns.Contains(token.Value.ToLower()))
                {
                    int index = token.Begin;
                    string start = fixedText.Substring(0, index);
                    string toFix = "they are";
                    string end = fixedText.Substring(nextToken.End + 1);

                    fixedText = start + toFix + end;
                }

                else if (token.POS == PartOfSpeech.PRON && nextToken.Value.ToLower() == "has" && subjectPronouns.Contains(token.Value.ToLower()))
                {
                    int index = token.Begin;
                    string start = fixedText.Substring(0, index);
                    string toFix = "they have";
                    string end = fixedText.Substring(nextToken.End + 1);

                    fixedText = start + toFix + end;
                }

                else if (token.POS == PartOfSpeech.PRON && nextToken.Value.ToLower() == "was" && subjectPronouns.Contains(token.Value.ToLower()))
                {
                    int index = token.Begin;
                    string start = fixedText.Substring(0, index);
                    string toFix = "they were";
                    string end = fixedText.Substring(nextToken.End + 1);

                    fixedText = start + toFix + end;
                }

                else if (token.POS == PartOfSpeech.PRON && subjectPronouns.Contains(token.Value.ToLower()))
                {
                    int index = token.Begin;
                    string start = fixedText.Substring(0, index);
                    string toFix = "they";
                    string end = fixedText.Substring(token.End + 1);

                    fixedText = start + toFix + end;
                }

                else if (token.POS == PartOfSpeech.PRON && objectPronouns.Contains(token.Value.ToLower()))
                {
                    int index = token.Begin;
                    string start = fixedText.Substring(0, index);
                    string toFix = "them";
                    string end = fixedText.Substring(token.End + 1);

                    fixedText = start + toFix + end;
                }

                else if (token.POS == PartOfSpeech.DET && possessiveAdj.Contains(token.Value.ToLower()))
                {
                    int index = token.Begin;
                    string start = fixedText.Substring(0, index);
                    string toFix = "their";
                    string end = fixedText.Substring(token.End + 1);

                    fixedText = start + toFix + end;
                }
                // THIS ONE IS NOT WORKING
                else if (token.POS == PartOfSpeech.PRON && possessivePronouns.Contains(token.Value.ToLower()))
                {
                    int index = token.Begin;
                    string start = fixedText.Substring(0, index);
                    string toFix = "theirs";
                    string end = fixedText.Substring(token.End + 1);

                    fixedText = start + toFix + end;
                }
                else if (token.POS == PartOfSpeech.PRON && reflexivePronouns.Contains(token.Value.ToLower()))
                {
                    int index = token.Begin;
                    string start = fixedText.Substring(0, index);
                    string toFix = "themself"; // Add stuff to modify verbs to allow themselves here
                    string end = fixedText.Substring(token.End + 1);

                    fixedText = start + toFix + end;
                }
            }

            Console.WriteLine(fixedText);

            return fixedText;
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
        /*
        private async Task LoadPipeline()
        {
            nlp = await Pipeline.ForAsync(Language.English);
        }
        */
    }
}
