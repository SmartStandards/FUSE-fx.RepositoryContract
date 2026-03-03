//using Jose;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.OpenApi.Models;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Web.UJMW.SelfAnnouncement;

//namespace Microsoft.AspNetCore.Builder {

//  public static class SwaggerUjmwHelperExtensions {

//    public static void AddUjmwStandardSwaggerGen(this IServiceCollection services, params string[] additionalApiGroupNames) {

//      string defaultApiGroupName = Assembly.GetCallingAssembly().GetName().Name + "-API";
//      string mainAssemblyDocumentationFile = Path.GetFileNameWithoutExtension(Assembly.GetCallingAssembly().Location) + ".xml";
//      string outDir = AppDomain.CurrentDomain.BaseDirectory;

//      services.AddSwaggerGen(c => {

//        c.ResolveConflictingActions(apiDescriptions => {
//          return apiDescriptions.First();
//        });

//        c.EnableAnnotations(true, true);

//        //c.IncludeXmlComments(Path.Combine(outDir, mainAssemblyDocumentationFile), false);

//        #region bearer

//        //https://www.thecodebuzz.com/jwt-authorization-token-swagger-open-api-asp-net-core-3-0/
//        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
//          Name = "Authorization",
//          Type = SecuritySchemeType.ApiKey,
//          Scheme = "Bearer",
//          BearerFormat = "JWT",
//          In = ParameterLocation.Header,
//          Description = "API-TOKEN"
//        });

//        c.AddSecurityRequirement(new OpenApiSecurityRequirement
//          {
//              {
//                    new OpenApiSecurityScheme
//                      {
//                          Reference = new OpenApiReference
//                          {
//                              Type = ReferenceType.SecurityScheme,
//                              Id = "Bearer"
//                          }
//                      },
//                      new string[] {}

//              }
//          });

//        #endregion

//        c.UseInlineDefinitionsForEnums();


//        string[] knownApiGroupNames = SelfAnnouncementHelper.RegisteredEndpoints.Select(ep => ep.ApiGroupName).Distinct().ToArray();
//        List<string> registeredApiNames = new List<string>();
//        foreach (string knownApiGroupName in knownApiGroupNames) {

//          string apiName = knownApiGroupName;
//          if (String.IsNullOrWhiteSpace(apiName)) {
//            apiName = defaultApiGroupName;
//          }

//          string version = "0.0.0";
//          Type firstContractType = SelfAnnouncementHelper.RegisteredEndpoints.Where(ep => ep.ApiGroupName == knownApiGroupName && ep.ContractType != null).Select(ep => ep.ContractType).FirstOrDefault();
//          if (firstContractType != null) {

//            version = firstContractType.Assembly.GetName().Version?.ToString(3) ?? "0.0.0";

//            string contractAssemblyDocumentationFile = Path.GetFileNameWithoutExtension(firstContractType.Assembly.Location) + ".xml";
//            //c.IncludeXmlComments(Path.Combine(outDir, contractAssemblyDocumentationFile), false);

//          }

//          registeredApiNames.Add(apiName);
//          c.SwaggerDoc(
//            apiName,
//            new OpenApiInfo {
//              Title = apiName,
//              Version = version,
//              Description = "" //UjmwDefaultApiInfo
//            }
//          );

//        }

//        if (additionalApiGroupNames != null) {
//          foreach (string additionalApiGroupName in additionalApiGroupNames) {
//            string apiName = additionalApiGroupName;
//            if (String.IsNullOrWhiteSpace(apiName)) {
//              apiName = defaultApiGroupName;
//            }
//            if (!registeredApiNames.Contains(apiName)) {
//              c.SwaggerDoc(
//                apiName,
//                new OpenApiInfo {
//                  Title = apiName,
//                  Version = "0.0.0",
//                  Description = "" //UjmwDefaultApiInfo
//                }
//              );
//            }

//          }
//        }

//      });

//    }

//    public const string UjmwDefaultApiInfo = "NOTE: This is not intended be a 'RESTful' api, as it is NOT located on the persistence layer and is therefore NOT focused on doing CRUD operations! This HTTP-based API uses a 'call-based' approach to known BL operations. IN-, OUT- and return-arguments are transmitted using request-/response- wrappers (see [UJMW](https://github.com/SmartStandards/UnifiedJsonMessageWrapper)), which are very lightweight and are a compromise for broad support and adaptability in REST-inspired technologies as well as soap-inspired technologies!";

//    /// <summary>
//    /// MUST BE AFTER 'UseEndpoints'/'MapControllers'
//    /// </summary>
//    /// <param name="app"></param>
//    /// <param name="config"></param>
//    public static void UseUjmwStandardSwagger(
//      this IApplicationBuilder app, IConfiguration config, params string[] additionalApiGroupNames) {

//      if (!config.GetValue<bool>("EnableSwaggerUi")) {
//        return;
//      }

//      string baseUrl = config.GetValue<string>("BaseUrl");
//      string defaultApiGroupName = Assembly.GetCallingAssembly().GetName().Name;

//      app.UseSwagger(o => {
//        //warning: needs subfolder! jsons cant be within same dir as swaggerui (below)
//        o.RouteTemplate = "docs/schema/{documentName}.{json|yaml}";
//        //o.SerializeAsV2 = true;
//      });

//      app.UseSwaggerUI(c => {


//        string[] knownApiGroupNames = SelfAnnouncementHelper.RegisteredEndpoints.Select(ep => ep.ApiGroupName).Distinct().ToArray();

//        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
//        c.DefaultModelExpandDepth(2);
//        c.DefaultModelsExpandDepth(2);
//        //c.ConfigObject.DefaultModelExpandDepth = 2;

//        c.DocumentTitle = defaultApiGroupName + " - OpenAPI Definition(s)";

//        foreach (string knownApiGroupName in knownApiGroupNames) {

//          string apiName = knownApiGroupName;
//          if (String.IsNullOrWhiteSpace(apiName)) {
//            apiName = defaultApiGroupName;
//          }

//          string version = "0.0.0";
//          Type firstContractType = SelfAnnouncementHelper.RegisteredEndpoints.Where(ep => ep.ApiGroupName == knownApiGroupName && ep.ContractType != null).Select(ep => ep.ContractType).FirstOrDefault();
//          if (firstContractType != null) {
//            version = firstContractType.Assembly.GetName().Version?.ToString(3) ?? "0.0.0";
//          }

//          string apiNameAndVersion = apiName;
//          if (version != "0.0.0") {
//            apiNameAndVersion += " - v" + version;
//          }

//          //just the combo-box entries...
//          c.SwaggerEndpoint(
//            "schema/" + apiName + ".json",
//            apiNameAndVersion
//          );

//        }

//        if (additionalApiGroupNames != null) {
//          foreach (string additionalApiGroupName in additionalApiGroupNames) {
//            string apiName = additionalApiGroupName;
//            if (String.IsNullOrWhiteSpace(apiName)) {
//              apiName = defaultApiGroupName;
//            }
//            //just the combo-box entries...
//            c.SwaggerEndpoint(
//              "schema/" + apiName + ".json",
//              apiName
//            );
//          }
//        }

//        c.RoutePrefix = "docs";

//        //requires MVC app.UseStaticFiles();
//        c.InjectStylesheet(baseUrl + "swagger-ui/custom.css");

//      });

//    }

//  }

//}
