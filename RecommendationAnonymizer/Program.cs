using Catalyst;
using Catalyst.Models;
using Mosaik.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Version = Mosaik.Core.Version;
using P = Catalyst.PatternUnitPrototype;
using System.Text;
using Microsoft.Extensions.Logging;

namespace RecommendationAnonymizer
{
    public static class Program
    {
        /*
         * NOTES:
         * - fix capitalization last?
         * - find any full names first
         * - find first names second
         * - find variations of first names
         * - find any lone last names?
         * - fix: her, hers, herself
         * 
         * 
         */
        private static async Task Main()
        {
            //Initialize the English built-in models
            Catalyst.Models.English.Register();
            var nlp = await Pipeline.ForAsync(Language.English);

            string letterOfRec = File.ReadAllText(@"C:\Users\limon\Documents\GitHub\RecommendationAnonymizer\RecommendationAnonymizer\SampleLetter1.txt");
            
            Anonymizer anonymizer = new Anonymizer(nlp);
            string newLetter = anonymizer.Anonymize("Joe", "Bloom", letterOfRec);
            Console.WriteLine(newLetter);

           // string test = "He is blue. I am blue. You like him. This is his cat. That cat is his. You are mine. This was my fault, not hers. He himself is king. He is there by himself.";
            //string newLetter = anonymizer.Anonymize("Joe", "Bloom", test);
            Console.WriteLine(newLetter);

            Console.WriteLine("DONE");
        }
    }
}