"use strict";
/* based on UDAS v0.1.0.0 */
Object.defineProperty(exports, "__esModule", { value: true });
exports.UdasConnector = exports.UdasInfoClient = void 0;
const axios_1 = require("axios");
/**
 * Provides interoperability information for the current implementation
 */
class UdasInfoClient {
    constructor(rootUrlResolver, apiTokenResolver, httpPostMethod) {
        this.rootUrlResolver = rootUrlResolver;
        this.apiTokenResolver = apiTokenResolver;
        this.httpPostMethod = httpPostMethod;
    }
    getEndpointUrl() {
        let rootUrl = this.rootUrlResolver();
        if (rootUrl.endsWith('/')) {
            return rootUrl + 'udasInfo/';
        }
        else {
            return rootUrl + '/udasInfo/';
        }
    }
    /**
     * returns the version of the specification which is implemented by this API, (this can be used for backward compatibility within inhomogeneous infrastructures)
     */
    getApiVersion() {
        let requestWrapper = {};
        let url = this.getEndpointUrl() + 'getApiVersion';
        return this.httpPostMethod(url, requestWrapper, this.apiTokenResolver()).then((r) => {
            let responseWrapper = r;
            if (responseWrapper.fault) {
                console.warn('Request to "' + url + '" faulted: ' + responseWrapper.fault);
                throw { message: responseWrapper.fault };
            }
            if (responseWrapper.return == undefined) {
                throw { message: 'response dto contains no "return" value!' };
            }
            return responseWrapper.return;
        });
    }
    /**
     * returns a list of API-features (there are several 'services' for different use cases) supported by this implementation. The following values are possible: '...tbd...',
     */
    getCapabilities() {
        let requestWrapper = {};
        let url = this.getEndpointUrl() + 'getCapabilities';
        return this.httpPostMethod(url, requestWrapper, this.apiTokenResolver()).then((r) => {
            let responseWrapper = r;
            if (responseWrapper.fault) {
                console.warn('Request to "' + url + '" faulted: ' + responseWrapper.fault);
                throw { message: responseWrapper.fault };
            }
            if (responseWrapper.return == undefined) {
                throw { message: 'response dto contains no "return" value!' };
            }
            return responseWrapper.return;
        });
    }
    /**
     * returns a list of available capabilities ("API:...") and/or data-scopes ("Tenant:FooBar") which are permitted for the CURRENT ACCESSOR and gives information about its 'authState', which can be: 0=auth needed / 1=authenticated / -1=auth expired / -2=auth invalid/disabled
     */
    getPermittedAuthScopes() {
        let requestWrapper = {};
        let url = this.getEndpointUrl() + 'getPermittedAuthScopes';
        return this.httpPostMethod(url, requestWrapper, this.apiTokenResolver()).then((r) => {
            let responseWrapper = r;
            if (responseWrapper.fault) {
                console.warn('Request to "' + url + '" faulted: ' + responseWrapper.fault);
                throw { message: responseWrapper.fault };
            }
            if (responseWrapper.return == undefined) {
                throw { message: 'response dto contains no "return" value!' };
            }
            return { authState: responseWrapper.authState, return: responseWrapper.return };
        });
    }
    /**
     * OPTIONAL: If the authentication on the current service is mapped using tokens and should provide information about the source at this point, the login URL to be called up via browser (OAuth ['CIBA-Flow'](https://openid.net/specs/openid-client-initiated-backchannel-authentication-core-1_0.html)) is returned here.
     */
    getOAuthTokenRequestUrl() {
        let requestWrapper = {};
        let url = this.getEndpointUrl() + 'getOAuthTokenRequestUrl';
        return this.httpPostMethod(url, requestWrapper, this.apiTokenResolver()).then((r) => {
            let responseWrapper = r;
            if (responseWrapper.fault) {
                console.warn('Request to "' + url + '" faulted: ' + responseWrapper.fault);
                throw { message: responseWrapper.fault };
            }
            if (responseWrapper.return == undefined) {
                throw { message: 'response dto contains no "return" value!' };
            }
            return responseWrapper.return;
        });
    }
    /**
     * GetEntitySchema
     */
    getEntitySchema() {
        let requestWrapper = {};
        let url = this.getEndpointUrl() + 'getEntitySchema';
        return this.httpPostMethod(url, requestWrapper, this.apiTokenResolver()).then((r) => {
            let responseWrapper = r;
            if (responseWrapper.fault) {
                console.warn('Request to "' + url + '" faulted: ' + responseWrapper.fault);
                throw { message: responseWrapper.fault };
            }
            if (responseWrapper.return == undefined) {
                throw { message: 'response dto contains no "return" value!' };
            }
            return responseWrapper.return;
        });
    }
}
exports.UdasInfoClient = UdasInfoClient;
class UdasConnector {
    constructor(rootUrlResolver, apiTokenResolver, httpPostMethod) {
        this.rootUrlResolver = rootUrlResolver;
        this.apiTokenResolver = apiTokenResolver;
        this.httpPostMethod = httpPostMethod;
        if (!this.httpPostMethod) {
            this.axiosHttpApi = axios_1.default.create({ baseURL: this.rootUrlResolver() });
            this.httpPostMethod = (url, requestObject, apiToken) => {
                if (!this.axiosHttpApi) {
                    this.axiosHttpApi = axios_1.default.create({ baseURL: this.rootUrlResolver() });
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
    getRootUrl() {
        let rootUrl = this.rootUrlResolver();
        if (rootUrl.endsWith('/')) {
            return rootUrl;
        }
        else {
            return rootUrl + '/';
        }
    }
    /**
     * Provides interoperability information for the current implementation
     */
    get udasInfo() { return this.udasInfoClient; }
}
exports.UdasConnector = UdasConnector;
//# sourceMappingURL=connector.js.map