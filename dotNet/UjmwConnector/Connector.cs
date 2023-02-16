/* WARNING: THIS IS GENERATED CODE - PLEASE DONT EDIT DIRECTLY - YOUR CHANGES WILL BE LOST! */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.EntitySchema;
using System.Data.UDAS;
using System.Data.UDAS.UdasInfo;
using System.Net;

namespace System.Data.UDAS {
  
  public partial class UdasConnector {
    
    public UdasConnector(string url, string apiToken) {
      
      if (!url.EndsWith("/")) {
        url = url + "/";
      }
      
      _UdasInfoClient = new UdasInfoClient(url + "udasInfo/", apiToken);
      
    }
    
    private UdasInfoClient _UdasInfoClient = null;
    /// <summary> Provides interoperability information for the current implementation </summary>
    public IUdasInfoService UdasInfo {
      get {
        return _UdasInfoClient;
      }
    }
    
  }
  
  namespace UdasInfo {
    
    /// <summary> Provides interoperability information for the current implementation </summary>
    internal partial class UdasInfoClient : IUdasInfoService {
      
      private string _Url;
      private string _ApiToken;
      
      public UdasInfoClient(string url, string apiToken) {
        _Url = url;
        _ApiToken = apiToken;
      }
      
      private WebClient CreateWebClient() {
        var wc = new WebClient();
        wc.Headers.Set("Authorization", _ApiToken);
        wc.Headers.Set("Content-Type", "application/json");
        return wc;
      }
      
      /// <summary> returns the version of the specification which is implemented by this API, (this can be used for backward compatibility within inhomogeneous infrastructures) </summary>
      public String GetApiVersion() {
        using (var webClient = this.CreateWebClient()) {
          string url = _Url + "getApiVersion";
          var requestWrapper = new GetApiVersionRequest {
          };
          string rawRequest = JsonConvert.SerializeObject(requestWrapper);
          string rawResponse = webClient.UploadString(url, rawRequest);
          var responseWrapper = JsonConvert.DeserializeObject<GetApiVersionResponse>(rawResponse);
          if(responseWrapper.fault != null){
            throw new Exception(responseWrapper.fault);
          }
          return responseWrapper.@return;
        }
      }
      
      /// <summary> returns a list of API-features (there are several 'services' for different use cases) supported by this implementation. The following values are possible: '...tbd...', </summary>
      public String[] GetCapabilities() {
        using (var webClient = this.CreateWebClient()) {
          string url = _Url + "getCapabilities";
          var requestWrapper = new GetCapabilitiesRequest {
          };
          string rawRequest = JsonConvert.SerializeObject(requestWrapper);
          string rawResponse = webClient.UploadString(url, rawRequest);
          var responseWrapper = JsonConvert.DeserializeObject<GetCapabilitiesResponse>(rawResponse);
          if(responseWrapper.fault != null){
            throw new Exception(responseWrapper.fault);
          }
          return responseWrapper.@return;
        }
      }
      
      /// <summary> returns a list of available capabilities ("API:...") and/or data-scopes ("Tenant:FooBar") which are permitted for the CURRENT ACCESSOR and gives information about its 'authState', which can be: 0=auth needed / 1=authenticated / -1=auth expired / -2=auth invalid/disabled </summary>
      /// <param name="authState">  </param>
      public String[] GetPermittedAuthScopes(out Int32 authState) {
        using (var webClient = this.CreateWebClient()) {
          string url = _Url + "getPermittedAuthScopes";
          var requestWrapper = new GetPermittedAuthScopesRequest {
          };
          string rawRequest = JsonConvert.SerializeObject(requestWrapper);
          string rawResponse = webClient.UploadString(url, rawRequest);
          var responseWrapper = JsonConvert.DeserializeObject<GetPermittedAuthScopesResponse>(rawResponse);
          authState = responseWrapper.authState;
          if(responseWrapper.fault != null){
            throw new Exception(responseWrapper.fault);
          }
          return responseWrapper.@return;
        }
      }
      
      /// <summary> OPTIONAL: If the authentication on the current service is mapped using tokens and should provide information about the source at this point, the login URL to be called up via browser (OAuth <see href="https://openid.net/specs/openid-client-initiated-backchannel-authentication-core-1_0.html">'CIBA-Flow'</see>) is returned here. </summary>
      public String GetOAuthTokenRequestUrl() {
        using (var webClient = this.CreateWebClient()) {
          string url = _Url + "getOAuthTokenRequestUrl";
          var requestWrapper = new GetOAuthTokenRequestUrlRequest {
          };
          string rawRequest = JsonConvert.SerializeObject(requestWrapper);
          string rawResponse = webClient.UploadString(url, rawRequest);
          var responseWrapper = JsonConvert.DeserializeObject<GetOAuthTokenRequestUrlResponse>(rawResponse);
          if(responseWrapper.fault != null){
            throw new Exception(responseWrapper.fault);
          }
          return responseWrapper.@return;
        }
      }
      
      /// <summary> GetEntitySchema </summary>
      public SchemaRoot GetEntitySchema() {
        using (var webClient = this.CreateWebClient()) {
          string url = _Url + "getEntitySchema";
          var requestWrapper = new GetEntitySchemaRequest {
          };
          string rawRequest = JsonConvert.SerializeObject(requestWrapper);
          string rawResponse = webClient.UploadString(url, rawRequest);
          var responseWrapper = JsonConvert.DeserializeObject<GetEntitySchemaResponse>(rawResponse);
          if(responseWrapper.fault != null){
            throw new Exception(responseWrapper.fault);
          }
          return responseWrapper.@return;
        }
      }
      
    }
    
  }
  
}
