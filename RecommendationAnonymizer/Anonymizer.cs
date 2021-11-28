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
             * 
             * - check if working on a few more letters                     |
             * - clean things up & add comments                             |
             * - enable loading of the pipeline inside of the class?        |
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
            int index = 0;
            string start = "";
            string toFix = "";
            string end = "";

            for (int i = 0; i < tokens.Count - 1; i++)
            {
                tokens = GetTokens(fixedText);
                IToken token = tokens[i];
                IToken nextToken = tokens[i + 1];
                index = token.Begin;

                string word = $" {token.Value} ";

                if(fixedText != TwoWordFix(fixedText, "subject", "is", "they are", token, nextToken))
                {
                    fixedText = TwoWordFix(fixedText, "subject", "is", "they are", token, nextToken);
                }
                
                else if (fixedText != TwoWordFix(fixedText, "subject", "has", "they have", token, nextToken))
                {
                    fixedText = TwoWordFix(fixedText, "subject", "has", "they have", token, nextToken);
                }

                else if (fixedText != TwoWordFix(fixedText, "subject", "was", "they were", token, nextToken))
                {
                    fixedText = TwoWordFix(fixedText, "subject", "was", "they were", token, nextToken);
                }

                else if (fixedText != OneWordFix(fixedText, "subject", "they", token))
                {
                    fixedText = OneWordFix(fixedText, "subject", "they", token);
                }

                else if (fixedText != OneWordFix(fixedText, "object", "them", token))
                {
                    fixedText = OneWordFix(fixedText, "object", "them", token);
                }

                else if (fixedText != OneWordFix(fixedText, "possessiveAdj", "their", token))
                {
                    fixedText = OneWordFix(fixedText, "possessiveAdj", "their", token);
                }

                else if (fixedText != OneWordFix(fixedText, "reflexive", "themself", token))
                {
                    fixedText = OneWordFix(fixedText, "reflexive", "themself", token);
                }

                /*
                fixedText = TwoWordFix(fixedText, "subject", "is", "they are", token, nextToken);
                fixedText = TwoWordFix(fixedText, "subject", "has", "they have", token, nextToken);
                fixedText = TwoWordFix(fixedText, "subject", "was", "they were", token, nextToken);

                fixedText = OneWordFix(fixedText, "subject", "they", token);
                fixedText = OneWordFix(fixedText, "object", "them", token);
                fixedText = OneWordFix(fixedText, "possessiveAdj", "their", token);
                fixedText = OneWordFix(fixedText, "reflexive", "themself", token);
                */

                //tokens = GetTokens(fixedText);

                /*

                if (token.POS == PartOfSpeech.PRON && nextToken.Value.ToLower() == "is" && pronouns["subject"].Contains(word))
                {
                    start = fixedText.Substring(0, index);
                    toFix = "they are";
                    end = fixedText.Substring(nextToken.End + 1);

                    fixedText = start + toFix + end;

                }

                else if (token.POS == PartOfSpeech.PRON && nextToken.Value.ToLower() == "has" && pronouns["subject"].Contains(word))
                {
                    
                    start = fixedText.Substring(0, index);
                    toFix = "they have";
                    end = fixedText.Substring(nextToken.End + 1);

                    fixedText = start + toFix + end;
                }

                else if (token.POS == PartOfSpeech.PRON && nextToken.Value.ToLower() == "was" && pronouns["subject"].Contains(word))
                {
                    
                    start = fixedText.Substring(0, index);
                    toFix = "they were";
                    end = fixedText.Substring(nextToken.End + 1);

                    fixedText = start + toFix + end;
                }

                

                if (token.POS == PartOfSpeech.PRON && pronouns["subject"].Contains(word))
                {
                    
                    start = fixedText.Substring(0, index);
                    toFix = "they";
                    end = fixedText.Substring(token.End + 1);

                    //fixedText = start + toFix + end;
                }

                else if (token.POS == PartOfSpeech.PRON && pronouns["object"].Contains(word))
                {
                    
                    start = fixedText.Substring(0, index);
                    toFix = "them";
                    end = fixedText.Substring(token.End + 1);

                    fixedText = start + toFix + end;
                }

                else if (token.POS == PartOfSpeech.PRON && pronouns["possessiveAdj"].Contains(word))
                {
                    
                    start = fixedText.Substring(0, index);
                    toFix = "their";
                    end = fixedText.Substring(token.End + 1);

                    fixedText = start + toFix + end;
                }
                else if (token.POS == PartOfSpeech.DET && pronouns["possessiveAdj"].Contains(word))
                {

                    start = fixedText.Substring(0, index);
                    toFix = "their";
                    end = fixedText.Substring(token.End + 1);

                    fixedText = start + toFix + end;
                }
                // THIS ONE IS NOT WORKING
                /*
                else if (token.POS == PartOfSpeech.PRON && possessivePronouns.Contains(token.Value.ToLower()))
                {
                    int index = token.Begin;
                    string start = fixedText.Substring(0, index);
                    string toFix = "theirs";
                    string end = fixedText.Substring(token.End + 1);

                    fixedText = start + toFix + end;
                } 
                else if (token.POS == PartOfSpeech.PRON && pronouns["reflexive"].Contains(word))
                {
                    
                    start = fixedText.Substring(0, index);
                    toFix = "themself"; // Add stuff to modify verbs to allow themselves here
                    end = fixedText.Substring(token.End + 1);

                    fixedText = start + toFix + end;
                }
                */
                tokens = GetTokens(fixedText);
            }

            Console.WriteLine(fixedText);

            return fixedText;
        }

        private string TwoWordFix(string text, string key, string condition, string insert, IToken token, IToken nextToken)
        {
            /*
                int index = token.Begin;
                string start = text.Substring(0, index);
                string end = text.Substring(nextToken.End + 1);

                text = start + toFix + end;
            

            tokens = GetTokens(text);
            */

            if(pronouns[key].Contains($" {token.Value} ") && nextToken.Value == condition)
            {
                
                string start = text.Substring(0, token.Begin);
                string end = text.Substring(nextToken.End + 1);

                text = start + insert + end;
            }

            tokens = GetTokens(text);

            // DESIGN IDEAS
            /* 
             * he is
             * he has
             * he was
             * 
             * if token.Value is in dict[key] && if next word is condition, then insert stuff
             * 
             */

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

            // DESIGN IDEAS
            /* 
             * if it's in this set of pronouns, insert this
             * 
             */

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
