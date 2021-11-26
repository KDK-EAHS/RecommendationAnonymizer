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
            List<IToken> list = GetTokens(letter);

            return anonymizedLetter;
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
