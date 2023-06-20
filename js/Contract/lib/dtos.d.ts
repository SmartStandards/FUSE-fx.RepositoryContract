import * as Models from './models';
/**
 * Contains arguments for calling 'GetApiVersion'.
 * Method: returns the version of the specification which is implemented by this API,
 * (this can be used for backward compatibility within inhomogeneous infrastructures)
 */
export declare class GetApiVersionRequest {
}
/**
 * Contains results from calling 'GetApiVersion'.
 * Method: returns the version of the specification which is implemented by this API,
 * (this can be used for backward compatibility within inhomogeneous infrastructures)
 */
export declare class GetApiVersionResponse {
    /** This field contains error text equivalent to an Exception message! (note that only 'fault' XOR 'return' can have a value != null) */
    fault?: string;
    /** Return-Value of 'GetApiVersion' (String) */
    return?: string;
}
/**
 * Contains arguments for calling 'GetCapabilities'.
 * Method: returns a list of API-features (there are several 'services' for different use cases)
 * supported by this implementation. The following values are possible:
 * '...tbd...',
 */
export declare class GetCapabilitiesRequest {
}
/**
 * Contains results from calling 'GetCapabilities'.
 * Method: returns a list of API-features (there are several 'services' for different use cases)
 * supported by this implementation. The following values are possible:
 * '...tbd...',
 */
export declare class GetCapabilitiesResponse {
    /** This field contains error text equivalent to an Exception message! (note that only 'fault' XOR 'return' can have a value != null) */
    fault?: string;
    /** Return-Value of 'GetCapabilities' (String[]) */
    return?: string[];
}
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
export declare class GetPermittedAuthScopesRequest {
}
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
export declare class GetPermittedAuthScopesResponse {
    /** Out-Argument of 'GetPermittedAuthScopes' (number) */
    authState: number;
    /** This field contains error text equivalent to an Exception message! (note that only 'fault' XOR 'return' can have a value != null) */
    fault?: string;
    /** Return-Value of 'GetPermittedAuthScopes' (String[]) */
    return?: string[];
}
/**
 * Contains arguments for calling 'GetOAuthTokenRequestUrl'.
 * Method: OPTIONAL: If the authentication on the current service is mapped
 * using tokens and should provide information about the source at this point,
 * the login URL to be called up via browser (OAuth ['CIBA-Flow'](https://openid.net/specs/openid-client-initiated-backchannel-authentication-core-1_0.html)) is returned here.
 */
export declare class GetOAuthTokenRequestUrlRequest {
}
/**
 * Contains results from calling 'GetOAuthTokenRequestUrl'.
 * Method: OPTIONAL: If the authentication on the current service is mapped
 * using tokens and should provide information about the source at this point,
 * the login URL to be called up via browser (OAuth ['CIBA-Flow'](https://openid.net/specs/openid-client-initiated-backchannel-authentication-core-1_0.html)) is returned here.
 */
export declare class GetOAuthTokenRequestUrlResponse {
    /** This field contains error text equivalent to an Exception message! (note that only 'fault' XOR 'return' can have a value != null) */
    fault?: string;
    /** Return-Value of 'GetOAuthTokenRequestUrl' (String) */
    return?: string;
}
/**
 * Contains arguments for calling 'GetEntitySchema'.
 */
export declare class GetEntitySchemaRequest {
}
/**
 * Contains results from calling 'GetEntitySchema'.
 */
export declare class GetEntitySchemaResponse {
    /** This field contains error text equivalent to an Exception message! (note that only 'fault' XOR 'return' can have a value != null) */
    fault?: string;
    /** Return-Value of 'GetEntitySchema' (SchemaRoot) */
    return?: Models.SchemaRoot;
}
//# sourceMappingURL=dtos.d.ts.map