using System;

//TODO: rename namespace of the final versions to "System.Data.Fuse"
namespace System.Data.UDAS {

  ///// <summary> Provides interoperability information for the current implementation </summary>
  //public partial interface IUdasInfoService {

  //  /// <summary>
  //  /// returns the version of the specification which is implemented by this API,
  //  /// (this can be used for backward compatibility within inhomogeneous infrastructures)
  //  /// </summary>
  //  string GetApiVersion();

  //  /// <summary>
  //  /// returns a list of API-features (there are several 'services' for different use cases)
  //  /// supported by this implementation. The following values are possible:
  //  /// '...tbd...',
  //  /// </summary>
  //  string[] GetCapabilities();

  //  /// <summary>
  //  /// returns a list of available capabilities ("API:...") and/or
  //  /// data-scopes ("Tenant:FooBar")
  //  /// which are permitted for the CURRENT ACCESSOR and gives information about its 'authState', which can be:
  //  ///  0=auth needed /
  //  ///  1=authenticated /
  //  /// -1=auth expired /
  //  /// -2=auth invalid/disabled
  //  /// </summary>
  //  /// <param name="authState"></param>
  //  /// <returns></returns>
  //  string[] GetPermittedAuthScopes(out int authState);

  //  /// <summary>
  //  /// OPTIONAL: If the authentication on the current service is mapped
  //  /// using tokens and should provide information about the source at this point,
  //  /// the login URL to be called up via browser (OAuth <see href="https://openid.net/specs/openid-client-initiated-backchannel-authentication-core-1_0.html">'CIBA-Flow'</see>) is returned here.
  //  /// </summary>
  //  string GetOAuthTokenRequestUrl();


  //  EntitySchema.SchemaRoot GetEntitySchema();

  //}

}
