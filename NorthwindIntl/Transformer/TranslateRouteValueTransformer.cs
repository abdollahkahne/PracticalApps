using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace NorthwindIntl.Transformer
{
    public sealed class TranslateRouteValueTransformer : DynamicRouteValueTransformer
    {
        private const string _languageKey="language";
        private const string _controllerKey="controller";
        private const string _actionKey="action";
        private readonly ITranslator _translator;

        public TranslateRouteValueTransformer(ITranslator translator)
        {
            _translator = translator;
        }

        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            var language=values[_languageKey] as string;
            var controller=values[_controllerKey] as string;
            var action=values[_actionKey] as string;
            var controller_en=await _translator.Translate(language,controller)??controller;
            var action_en=await _translator.Translate(language,action)?? action;
            values[_controllerKey]=controller_en;
            values[_actionKey]=action_en;
            return values;
        }
    }
}