/* based on UDAS v0.1.0.0 */

import axios, { AxiosInstance } from 'axios';

import * as DTOs from 'udas-contract';
import * as Models from 'udas-contract';
import * as Interfaces from 'udas-contract';

/**
 * Provides interoperability information for the current implementation
 */
export class UdasInfoClient {
  
  constructor(
    private rootUrlResolver: () => string,
    private apiTokenResolver: () => string,
    private httpPostMethod: (url: string, requestObject: any, apiToken: string) => Promise<any>
  ){}
  
  private getEndpointUrl(): string {
    let rootUrl = this.rootUrlResolver();
    if(rootUrl.endsWith('/')){
      return rootUrl + 'udasInfo/';
    }
    else{
      return rootUrl + '/udasInfo/';
    }
  }
  
  /**
   * returns the version of the specification which is implemented by this API, (this can be used for backward compatibility within inhomogeneous infrastructures)
   */
  public getApiVersion(): Promise<string> {
    
    let requestWrapper : DTOs.GetApiVersionRequest = {
    };
    
    let url = this.getEndpointUrl() + 'getApiVersion';
    return this.httpPostMethod(url, requestWrapper, this.apiTokenResolver()).then(
      (r) => {
        let responseWrapper = (r as DTOs.GetApiVersionResponse);
        if(responseWrapper.fault){
          console.warn('Request to "' + url + '" faulted: ' + responseWrapper.fault);
          throw {message: responseWrapper.fault};
        }
        if (responseWrapper.return == undefined){
          throw { message: 'response dto contains no "return" value!'};
        }
        return responseWrapper.return;
      }
    );
  }
  
  /**
   * returns a list of API-features (there are several 'services' for different use cases) supported by this implementation. The following values are possible: '...tbd...',
   */
  public getCapabilities(): Promise<string[]> {
    
    let requestWrapper : DTOs.GetCapabilitiesRequest = {
    };
    
    let url = this.getEndpointUrl() + 'getCapabilities';
    return this.httpPostMethod(url, requestWrapper, this.apiTokenResolver()).then(
      (r) => {
        let responseWrapper = (r as DTOs.GetCapabilitiesResponse);
        if(responseWrapper.fault){
          console.warn('Request to "' + url + '" faulted: ' + responseWrapper.fault);
          throw {message: responseWrapper.fault};
        }
        if (responseWrapper.return == undefined){
          throw { message: 'response dto contains no "return" value!'};
        }
        return responseWrapper.return;
      }
    );
  }
  
  /**
   * returns a list of available capabilities ("API:...") and/or data-scopes ("Tenant:FooBar") which are permitted for the CURRENT ACCESSOR and gives information about its 'authState', which can be: 0=auth needed / 1=authenticated / -1=auth expired / -2=auth invalid/disabled
   */
  public getPermittedAuthScopes(): Promise<{authState: number, return: string[]}> {
    
    let requestWrapper : DTOs.GetPermittedAuthScopesRequest = {
    };
    
    let url = this.getEndpointUrl() + 'getPermittedAuthScopes';
    return this.httpPostMethod(url, requestWrapper, this.apiTokenResolver()).then(
      (r) => {
        let responseWrapper = (r as DTOs.GetPermittedAuthScopesResponse);
        if(responseWrapper.fault){
          console.warn('Request to "' + url + '" faulted: ' + responseWrapper.fault);
          throw {message: responseWrapper.fault};
        }
        if (responseWrapper.return == undefined){
          throw { message: 'response dto contains no "return" value!'};
        }
        return {authState: responseWrapper.authState, return: responseWrapper.return};
      }
    );
  }
  
  /**
   * OPTIONAL: If the authentication on the current service is mapped using tokens and should provide information about the source at this point, the login URL to be called up via browser (OAuth ['CIBA-Flow'](https://openid.net/specs/openid-client-initiated-backchannel-authentication-core-1_0.html)) is returned here.
   */
  public getOAuthTokenRequestUrl(): Promise<string> {
    
    let requestWrapper : DTOs.GetOAuthTokenRequestUrlRequest = {
    };
    
    let url = this.getEndpointUrl() + 'getOAuthTokenRequestUrl';
    return this.httpPostMethod(url, requestWrapper, this.apiTokenResolver()).then(
      (r) => {
        let responseWrapper = (r as DTOs.GetOAuthTokenRequestUrlResponse);
        if(responseWrapper.fault){
          console.warn('Request to "' + url + '" faulted: ' + responseWrapper.fault);
          throw {message: responseWrapper.fault};
        }
        if (responseWrapper.return == undefined){
          throw { message: 'response dto contains no "return" value!'};
        }
        return responseWrapper.return;
      }
    );
  }
  
  /**
   * GetEntitySchema
   */
  public getEntitySchema(): Promise<Models.SchemaRoot> {
    
    let requestWrapper : DTOs.GetEntitySchemaRequest = {
    };
    
    let url = this.getEndpointUrl() + 'getEntitySchema';
    return this.httpPostMethod(url, requestWrapper, this.apiTokenResolver()).then(
      (r) => {
        let responseWrapper = (r as DTOs.GetEntitySchemaResponse);
        if(responseWrapper.fault){
          console.warn('Request to "' + url + '" faulted: ' + responseWrapper.fault);
          throw {message: responseWrapper.fault};
        }
        if (responseWrapper.return == undefined){
          throw { message: 'response dto contains no "return" value!'};
        }
        return responseWrapper.return;
      }
    );
  }
  
}

export class UdasConnector {
  
  private udasInfoClient: UdasInfoClient;
  
  private axiosHttpApi?: AxiosInstance;
  
  constructor(
    private rootUrlResolver: () => string,
    private apiTokenResolver: () => string,
    private httpPostMethod?: (url: string, requestObject: any, apiToken: string) => Promise<any>
  ){
  
    if (!this.httpPostMethod) {
      this.axiosHttpApi = axios.create({ baseURL: this.rootUrlResolver() });
      this.httpPostMethod = (url, requestObject, apiToken) => {
        if(!this.axiosHttpApi) {
          this.axiosHttpApi = axios.create({ baseURL: this.rootUrlResolver() });
        }
        return this.axiosHttpApi.post(url, requestObject, {
          headers: {
            Authorization: apiToken
          }
        });
      };
    }
    
    this.udasInfoClient = new UdasInfoClient(this.rootUrlResolver, this.apiTokenResolver, this.httpPostMethod);
    
  }
  
  private getRootUrl(): string {
    let rootUrl = this.rootUrlResolver();
    if(rootUrl.endsWith('/')){
      return rootUrl;
    }
    else{
      return rootUrl + '/';
    }
  }
  
  /**
   * Provides interoperability information for the current implementation
   */
  get udasInfo(): UdasInfoClient { return this.udasInfoClient }
  
}
