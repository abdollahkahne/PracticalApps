using System.Threading.Tasks;

namespace NorthwindIntl.Transformer
{
    public class Translator : ITranslator
    {
        public Task<string> Translate(string sourceLanguage, string term)
        {
            string translatedTo=sourceLanguage switch {
                "en" => term switch {
                    "Home"=>"Home",
                    "Index"=>"Index",
                    "Privacy"=>"Privacy",
                    "Calculate"=>"Calculate",
                    _=>null,
                },
                "fa"=>term switch {
                    "خانه"=>"Home",
                    "فهرست"=>"Index",
                    "حریم خصوصی"=>"Privacy",
                    "ماشین حساب"=>"Calculate",
                    _=>null,
                },

                _=>null,
            };
            return Task.FromResult<string>(translatedTo);
        }
    }
}