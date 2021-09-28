using System.Threading.Tasks;

namespace NorthwindIntl.Transformer
{
    public interface ITranslator
    {
         Task<string> Translate(string sourceLanguage,string term);
    }
}