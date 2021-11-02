#pragma checksum "/home/selenium/Desktop/Learning/PracticalApps/ControllersTestSample/Src/Views/Session/index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "4ffda77ad7d6b0f55af9ff3ca931b6a124690197"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Session_index), @"mvc.1.0.view", @"/Views/Session/index.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "/home/selenium/Desktop/Learning/PracticalApps/ControllersTestSample/Src/Views/_ViewImports.cshtml"
using Src;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "/home/selenium/Desktop/Learning/PracticalApps/ControllersTestSample/Src/Views/_ViewImports.cshtml"
using Src.Models;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"4ffda77ad7d6b0f55af9ff3ca931b6a124690197", @"/Views/Session/index.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"821463483f152aaeb5f1ebb236344900e8f26368", @"/Views/_ViewImports.cshtml")]
    public class Views_Session_index : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<StormSessionViewModel>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\n");
#nullable restore
#line 3 "/home/selenium/Desktop/Learning/PracticalApps/ControllersTestSample/Src/Views/Session/index.cshtml"
  
    ViewBag.Title = "Brainstormer Session : " + Model.Name;
    Layout = "_Layout";

#line default
#line hidden
#nullable disable
            WriteLiteral("<h2>Brainstorm Session: ");
#nullable restore
#line 7 "/home/selenium/Desktop/Learning/PracticalApps/ControllersTestSample/Src/Views/Session/index.cshtml"
                   Write(Model.Name);

#line default
#line hidden
#nullable disable
            WriteLiteral("</h2>\n<div class=\"small\">");
#nullable restore
#line 8 "/home/selenium/Desktop/Learning/PracticalApps/ControllersTestSample/Src/Views/Session/index.cshtml"
              Write(Model.DateCreated);

#line default
#line hidden
#nullable disable
            WriteLiteral(@"</div>

<div class=""row"">
    <div class=""col-md-9"">
        <h3>Idea Count: <span data-bind=""text:ideas().length""></span></h3>
        <div data-bind='foreach: ideas'>
            <div class=""panel panel-default"">
                <div class=""panel-heading"" data-bind=""text:name"">
                </div>
                <div class=""panel-body"" data-bind=""text:description"">
                </div>
            </div>
        </div>
    </div>
    <div class=""col-md-3"">
        <div class=""panel panel-primary"">
            <div class=""panel-heading"">
                Add New Idea
            </div>
            <div class=""panel-body"">
                <div data-bind=""with: ideaForEditing"">
                    <fieldset class=""form-group"">
                        <label for=""ideaName"">Idea Name</label>
                        <input type=""text"" class=""form-control"" id=""ideaName"" name=""ideaName"" placeholder=""New Idea""
                            data-bind=""value:name"">
                    </fieldset>
                  ");
            WriteLiteral(@"  <fieldset class=""form-group"">
                        <label for=""ideaDescription"">Description</label>
                        <textarea class=""form-control"" id=""ideaDescription"" name=""ideaDescription""
                            data-bind=""value:description""></textarea>
                    </fieldset>
                    <input type=""submit"" value=""Save"" id=""SaveButton"" name=""SaveButton"" class=""btn btn-primary""
                        data-bind=""click: $root.addIdea"">
                </div>
            </div>
        </div>
    </div>
</div>
<div class=""row"">
    <div class=""col-md-12"">
        <a href=""/"">Return home</a>
    </div>
</div>
");
            DefineSection("Scripts", async() => {
                WriteLiteral("\n<script type=\"text/javascript\">\n    var Idea = function (id, name, description) {\n        this.id = ko.observable(id);\n        this.name = ko.observable(name);\n        this.description = ko.observable(description);\n        this.sessionId = ");
#nullable restore
#line 57 "/home/selenium/Desktop/Learning/PracticalApps/ControllersTestSample/Src/Views/Session/index.cshtml"
                    Write(Model.Id);

#line default
#line hidden
#nullable disable
                WriteLiteral(@";
    };
    var ViewModel = function () {
        var self = this;
        self.ideas = ko.observableArray([]);
        self.ideaForEditing = ko.observable(new Idea());
        self.addIdea = function (newIdea) {
            if (newIdea.name() != undefined && newIdea.description() != undefined) {
                console.log(""add idea: "" + newIdea.name() + "" desc: "" + newIdea.description());
                self.ideas.push(newIdea);
                $.ajax({
                    url: '/api/ideas/create',
                    type: 'POST',
                    data: ko.toJSON(newIdea),
                    contentType: 'application/json'
                });
                self.ideaForEditing(new Idea());
            }
        }
    };
    viewModel = new ViewModel();
    ko.applyBindings(viewModel);
    $(function () {
        $.ajax({
            url: '/api/ideas/forsession/");
#nullable restore
#line 81 "/home/selenium/Desktop/Learning/PracticalApps/ControllersTestSample/Src/Views/Session/index.cshtml"
                                   Write(Model.Id);

#line default
#line hidden
#nullable disable
                WriteLiteral("\',\n            dataType: \'json\',\n            success: function (data) {\n                if (data instanceof Array) {\n                    viewModel.ideas(data);\n                }\n            }\n        });\n    });\n</script>\n");
            }
            );
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<StormSessionViewModel> Html { get; private set; }
    }
}
#pragma warning restore 1591
