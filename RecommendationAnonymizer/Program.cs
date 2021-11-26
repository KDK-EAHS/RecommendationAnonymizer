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

namespace Catalyst.Samples.EntityRecognition
{
    public static class Program
    {
        private static async Task Main()
        {
            //Initialize the English built-in models
            Catalyst.Models.English.Register();

            Storage.Current = new DiskStorage("catalyst-models");
            var nlp = await Pipeline.ForAsync(Language.English);

            string text = File.ReadAllText(@"C:\Users\limon\Documents\GitHub\RecommendationAnonymizer\RecommendationAnonymizer\SampleLetter1.txt");
            var letter = new Document(text, Language.English);
            nlp.ProcessSingle(letter);
            List<IToken> list = new List<IToken>();
            list = letter.ToTokenList();

            foreach (IToken token in list)
            {

            }
        }
    }
}