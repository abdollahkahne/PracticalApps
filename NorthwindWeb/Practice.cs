using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace NorthwindWeb
{
    public class User {
        public string FirstName {get;set;}
        public string LastName {get;set;}
        public string Email {get;set;}
    }
    public class Practice
    {
        public void Test() {

            // Instantiate Configuration Builder
            var builder=new ConfigurationBuilder();

            // set base path for file config sources (this is default to current folder)
            builder.SetBasePath(Environment.CurrentDirectory);

            // Add file config source using config provider extension method
            builder.AddJsonFile("appsettings.json"); // in the underhoood this method create an instance of provider first and then use its get/set methods

            // Add file config source using Add method directly to builder
            var jsonSrc=new JsonConfigurationSource {
                Path="appsettings.Development.json",
            };
            builder.Add(jsonSrc); // In the underhood Add method use build method of jsonSrc to build an instance of json provider and use methods of provider to get or set config value

            // Add user secrets configs (this needs userSecretsId in csproj)
            builder.AddUserSecrets(userSecretsId:"abcd");

            // Add config from Environment Vars
            builder.AddEnvironmentVariables(prefix:"ASPNETCORE_");

            // Add config from Command Line
            builder.AddCommandLine(Environment.GetCommandLineArgs().Skip(1).ToArray());

            // Add config in runtime using a dictionary
            var properties=new Dictionary<string,string> {
                {"FirstName","Ali"},
                {"LastName","Alavi"},
                {"Email","ali_alavi1999@yahoo.com"}
            };
            builder.AddInMemoryCollection(properties);

            // Generate an Config root from builder
            var configs=builder.Build();

            // Reterive a key directly or using path
            var x=configs["Levels"];
            var y=configs["A:B:C"];

            //Reterive Config from Config Section
            string z;
            var section=configs.GetSection("A");
            var exist=section.Exists();
            if (exist) {
                z=section["B"];
                
            }
            var user=section.Get<User>();
        }
    }
}