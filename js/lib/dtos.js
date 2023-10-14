"use strict";
/* based on UDAS Contract v0.1.0.0 */
Object.defineProperty(exports, "__esModule", { value: true });
exports.GetEntitySchemaRequest = exports.GetOAuthTokenRequestUrlResponse = exports.GetOAuthTokenRequestUrlRequest = exports.GetPermittedAuthScopesResponse = exports.GetPermittedAuthScopesRequest = exports.GetCapabilitiesResponse = exports.GetCapabilitiesRequest = exports.GetApiVersionResponse = exports.GetApiVersionRequest = void 0;
// import * as Models from './models';
/**
 * Contains arguments for calling 'GetApiVersion'.
 * Method: returns the version of the specification which is implemented by this API,
 * (this can be used for backward compatibility within inhomogeneous infrastructures)
 */
var GetApiVersionRequest = /** @class */ (function () {
    function GetApiVersionRequest() {
    }
    return GetApiVersionRequest;
}());
exports.GetApiVersionRequest = GetApiVersionRequest;
/**
 * Contains results from calling 'GetApiVersion'.
 * Method: returns the version of the specification which is implemented by this API,
 * (this can be used for backward compatibility within inhomogeneous infrastructures)
 */
var GetApiVersionResponse = /** @class */ (function () {
    function GetApiVersionResponse() {
    }
    return GetApiVersionResponse;
}());
exports.GetApiVersionResponse = GetApiVersionResponse;
/**
 * Contains arguments for calling 'GetCapabilities'.
 * Method: returns a list of API-features (there are several 'services' for different use cases)
 * supported by this implementation. The following values are possible:
 * '...tbd...',
 */
var GetCapabilitiesRequest = /** @class */ (function () {
    function GetCapabilitiesRequest() {
    }
    return GetCapabilitiesRequest;
}());
exports.GetCapabilitiesRequest = GetCapabilitiesRequest;
/**
 * Contains results from calling 'GetCapabilities'.
 * Method: returns a list of API-features (there are several 'services' for different use cases)
 * supported by this implementation. The following values are possible:
 * '...tbd...',
 */
var GetCapabilitiesResponse = /** @class */ (function () {
    function GetCapabilitiesResponse() {
    }
    return GetCapabilitiesResponse;
}());
exports.GetCapabilitiesResponse = GetCapabilitiesResponse;
/**
 * Contains arguments for calling 'GetPermittedAuthScopes'.
 * Method: returns a list of available capabilities ("API:...") and/or
 * data-scopes ("Tenant:FooBar")
 * which are permitted for the CURRENT ACCESSOR and gives information about its 'authState', which can be:
 * 0=auth needed /
 * 1=authenticated /
 * -1=auth expired /
 * -2=auth invalid/disabled
 */
var GetPermittedAuthScopesRequest = /** @class */ (function () {
    function GetPermittedAuthScopesRequest() {
    }
    return GetPermittedAuthScopesRequest;
}());
exports.GetPermittedAuthScopesRequest = GetPermittedAuthScopesRequest;
/**
 * Contains results from calling 'GetPermittedAuthScopes'.
 * Method: returns a list of available capabilities ("API:...") and/or
 * data-scopes ("Tenant:FooBar")
 * which are permitted for the CURRENT ACCESSOR and gives information about its 'authState', which can be:
 * 0=auth needed /
 * 1=authenticated /
 * -1=auth expired /
 * -2=auth invalid/disabled
 */
var GetPermittedAuthScopesResponse = /** @class */ (function () {
    function GetPermittedAuthScopesResponse() {
        /** Out-Argument of 'GetPermittedAuthScopes' (number) */
        this.authState = 0;
    }
    return GetPermittedAuthScopesResponse;
}());
exports.GetPermittedAuthScopesResponse = GetPermittedAuthScopesResponse;
/**
 * Contains arguments for calling 'GetOAuthTokenRequestUrl'.
 * Method: OPTIONAL: If the authentication on the current service is mapped
 * using tokens and should provide information about the source at this point,
 * the login URL to be called up via browser (OAuth ['CIBA-Flow'](https://openid.net/specs/openid-client-initiated-backchannel-authentication-core-1_0.html)) is returned here.
 */
var GetOAuthTokenRequestUrlRequest = /** @class */ (function () {
    function GetOAuthTokenRequestUrlRequest() {
    }
    return GetOAuthTokenRequestUrlRequest;
}());
exports.GetOAuthTokenRequestUrlRequest = GetOAuthTokenRequestUrlRequest;
/**
 * Contains results from calling 'GetOAuthTokenRequestUrl'.
 * Method: OPTIONAL: If the authentication on the current service is mapped
 * using tokens and should provide information about the source at this point,
 * the login URL to be called up via browser (OAuth ['CIBA-Flow'](https://openid.net/specs/openid-client-initiated-backchannel-authentication-core-1_0.html)) is returned here.
 */
var GetOAuthTokenRequestUrlResponse = /** @class */ (function () {
    function GetOAuthTokenRequestUrlResponse() {
    }
    return GetOAuthTokenRequestUrlResponse;
}());
exports.GetOAuthTokenRequestUrlResponse = GetOAuthTokenRequestUrlResponse;
/**
 * Contains arguments for calling 'GetEntitySchema'.
 */
var GetEntitySchemaRequest = /** @class */ (function () {
    function GetEntitySchemaRequest() {
    }
    return GetEntitySchemaRequest;
}());
exports.GetEntitySchemaRequest = GetEntitySchemaRequest;
/**
 * Contains results from calling 'GetEntitySchema'.
 */
// export class GetEntitySchemaResponse {
//   /** This field contains error text equivalent to an Exception message! (note that only 'fault' XOR 'return' can have a value != null) */
//   public fault?: string;
//   /** Return-Value of 'GetEntitySchema' (SchemaRoot) */
//   public return?: SchemaRoot;
// }
//# sourceMappingURL=dtos.js.map