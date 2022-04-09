Remote Authentication
The remote authentication are 
There is two different type of Remote Authentication: 
* OAuth Authentication 
* Open ID Connect Authentication
Consider that Cookies and Bearer Tokens are not remote Authentication Handler but usually we use one of them for SignIn Authentication State in case of using Remote Authentications. 

OIDC vs OAuth
The difference of OIDC and OAuth is that Altough OAuth do Authentication but in OIDC it do Authentication through Authorization. In OIDC there is a Resource Party which is responsible for providing User Information. But to get info from resource we should do authorization using an Authorization Party. In OAuth we have ID Token which is a jwt token and contains user information as payload which returned from Identity Provider but in case of OIDC we have an access token which received from Authorization Party in response to Authorization and then we use it to get user info from Resource server. 

Token Types
Token can be defined by their applicability or their format. For example we have JWT Token which is a format of tokens. We speak about them later. We also have ID Token/ Access Token which is two token with different applicabilities (but they can have same format!)

ID Token and Access Token


OAuth 2 
The OAuth 2.0 authorization framework enables a third-party application (Our Application) to obtain limited access to an HTTP Service (For example Google User Info), either on behalf of a resource owner (Current User) by orchestrating an approval interaction between the resource owner (Current User) and the HTTP service (Gogle User Info API), or by allowing the third-party application to obtain access on its own behalf (Independent of current user). 
In the traditional client-server authentication model, the client requests an access-restricted resource (protected resource) on the server by authenticating with the server using the resource owner's   credentials.  In order to provide third-party applications access to restricted resources, the resource owner shares its credentials with the third party.  This creates several problems and limitations.
OAuth addresses these issues by introducing an authorization layer (Adding an Authorization Server) and separating the role of the client (Who require resource) from that of the resource owner (Who the resource belong to).  In OAuth, the client requests access to resources controlled by the resource owner and hosted by the resource server, and is issued a different set of credentials than those of the resource owner (By Authorization Layer).
Instead of using the resource owner's credentials to access protected resources, the client obtains an access token -- a string denoting a specific scope, lifetime, and other access attributes.  Access tokens  are issued to third-party clients by an authorization server with the approval of the resource owner.  The client uses the access token to access the protected resources hosted by the resource server.
For example, an end-user (resource owner) can grant a printing service (client) access to her protected photos stored at a photo-sharing service (resource server), without sharing her username and password with the printing service.  Instead, she authenticates directly with a server trusted by the photo-sharing service (authorization server), which issues the printing service delegation-specific credentials (access token).
OAuth defines four roles:
   1- resource owner: An entity capable of granting access to a protected resource. When the resource owner is a person, it is referred to as an end-user.
   2- resource server: The server hosting the protected resources, capable of accepting and responding to protected resource requests using access tokens.
   3- client: An application making protected resource requests on behalf of the resource owner and with its authorization.  The term "client" does not imply any particular implementation characteristics (e.g.,     whether the application executes on a server, a desktop, or other devices).
   4- authorization server: The server issuing access tokens to the client after successfully authenticating the resource owner and obtaining authorization.
   The abstract OAuth 2.0 flow describes the interaction between the four roles and includes the following steps:
   (A)  The client requests authorization from the resource owner.  The authorization request can be made directly to the resource owner (How? Grant Types 3,4), or preferably indirectly via the authorization server as an intermediary.
   (B)  The client receives an authorization grant (Result of Step 1), which is a credential representing the resource owner's authorization, expressed using one of the grant types defined later. The authorization grant type depends on the method used by the client to request authorization and the types supported by the authorization server.
   (C)  The client requests an access token by authenticating with the authorization server and presenting the authorization grant.
   (D)  The authorization server authenticates the client and validates the authorization grant, and if valid, issues an access token.
   (E)  The client requests the protected resource from the resource server and authenticates by presenting the access token.
   (F)  The resource server validates the access token, and if valid, serves the request.
There is 4 type of Authorization Grants which given to Client in Step A:
* Authorization Code: The authorization code is obtained by using an authorization server as an intermediary between the client and resource owner.  Instead of requesting authorization directly from the resource owner, the client directs the resource owner to an authorization server (via its user-agent), which in turn directs the resource owner back to the client with the authorization code. Before directing the resource owner back to the client with the authorization code, the authorization server authenticates the resource owner and obtains authorization.  Because the resource owner only authenticates with the authorization server, the resource owner's credentials are never shared with the client. The authorization code provides a few important security benefits, such as the ability to authenticate the client, as well as the transmission of the access token directly to the client without passing it through the resource owner's user-agent and potentially exposing it to others, including the resource owner.
* Implicit: The implicit grant is a simplified authorization code flow optimized for clients implemented in a browser using a scripting language such as JavaScript.  In the implicit flow, instead of issuing the client an authorization code, the client is issued an access token directly (as the result of the resource owner authorization).  The grant type is implicit, as no intermediate credentials (such as an authorization code) are issued. When issuing an access token during the implicit grant flow, the authorization server does not authenticate the client (The access token here has all rights access token in Authorization Code Grant Type but the it is less secure!).  In some cases, the client identity can be verified via the redirection URI used to deliver the access token to the client.  The access token may be exposed in Implicit Grant Type. Implicit grants improve the responsiveness and efficiency of some clients (such as a client implemented as an in-browser application), since it reduces the number of round trips required to obtain an access token.  However, this convenience should be weighed against the security implications of using implicit grants, especially when the authorization code grant type is available.
* Resource Owner Password Credentials: The resource owner password credentials (i.e., username and password)
can be used directly as an authorization grant to obtain an access token.  The credentials should only be used when there is a high degree of trust between the resource owner and the client (e.g., the client is part of the device operating system or a highly privileged application), and when other authorization grant types are not available (such as an authorization code). Even though this grant type requires direct client access to the resource owner credentials, the resource owner credentials are used for a single request and are exchanged for an access token.  This grant type can eliminate the need for the client to store the
resource owner credentials for future use, by exchanging the credentials with a long-lived access token or refresh token.
* Client Credentials: The client credentials (or other forms of client authentication) can be used as an authorization grant when the authorization scope is limited to the protected resources under the control of the client, or to protected resources previously arranged with the authorization server.  Client credentials are used as an authorization grant typically when the client is acting on its own behalf (the client is
also the resource owner) or is requesting access to protected resources based on an authorization previously arranged with the authorization server (As an example a facebook App which post on its behalf or an unexpired App token used directly in Code).

Access Token in OAuth 
Access tokens are credentials used to access protected resources.  An access token is a string representing an authorization issued to the client.  The string is usually opaque to the client.  Tokens represent specific scopes and durations of access, granted by the resource owner, and enforced by the resource server and authorization server.
The token may denote an identifier used to retrieve the authorization information or may self-contain the authorization information in a verifiable manner (i.e., a token string consisting of some data and a
signature) (For example as JWT).  Additional authentication credentials, may be required in order for the client to use a token (For example Public Key in case of Google Access Token).
The access token provides an abstraction layer, replacing different authorization constructs (e.g., username and password) with a single token understood by the resource server.  This abstraction enables issuing access tokens more restrictive than the authorization grant used to obtain them, as well as removing the resource server's need to understand a wide range of authentication methods. Access tokens can have different formats, structures, and methods of utilization (e.g., cryptographic properties) based on the resource server security requirements.
Refresh Token in OAuth
Refresh tokens are credentials used to obtain access tokens in case of expired access token. Refresh
tokens are issued to the client by the authorization server and are used to obtain a new access token when the current access token becomes invalid or expires, or to obtain additional access tokens with identical or narrower scope (second returned access tokens may have a shorter lifetime and fewer permissions than authorized by the resource owner).  Issuing a refresh token is optional at the discretion of the authorization server.  If the authorization server issues a refresh token, it is included when issuing an access token (i.e., step (D). A refresh token is a string representing the authorization granted to the client by the resource owner.  The string is usually opaque to the client.  The token denotes an identifier used to retrieve the authorization information.  Unlike access tokens, refresh tokens are intended for use only with authorization servers and are never sent to resource servers.

Client Types in OAuth
OAuth defines two client types, based on their ability to authenticate securely with the authorization server (i.e., ability to maintain the confidentiality of their client credentials):
* Public (Only required to have Client_Id to authenticate with the authorization server)
* Condifential (This uses Client_Secret too)
Note: The authorization server SHOULD NOT make assumptions about the client type.
OAuth can be used in Web Application (confidential client), User-agent-based Application (public client) and Native Applications (public client). 


The authorization process utilizes two authorization server endpoints (HTTP resources):
   o  Authorization endpoint - used by the client to obtain authorization from the resource owner via user-agent redirection.
   o  Token endpoint - used by the client to exchange an authorization grant for an access token, typically with client authentication.
As well as one client endpoint:
   o  Redirection endpoint - used by the authorization server to return responses containing authorization credentials to the client via the resource owner user-agent.
Not every authorization grant type utilizes both endpoints. Extension grant types MAY define additional endpoints as needed.

Authorization Endpoint Usage
The authorization server MUST support the use of the HTTP "GET" method for the authorization endpoint and MAY support the use of the "POST" method as well. Since requests to the authorization endpoint result in user authentication and the transmission of clear-text credentials (in the HTTP response), the authorization server MUST require the use of TLS. The following Parameters included in Requests:
   * Client_Id
   * Response Type (Required): "code" or "token"
   * Redirection Endpoint (This is required in Case of Public client type or implicit flow): The authorization server redirects the user-agent to the client's redirection endpoint previously established with the authorization server during the client registration process or when making the authorization request. The authorization server SHOULD require all clients to register their redirection endpoint prior to utilizing the authorization endpoint. Also the authorization server MUST compare and match the value received against at least one of the registered redirection URIs. The client SHOULD NOT include any third-party scripts (e.g., third-party analytics, social plug-ins, ad networks) in the redirection endpoint response.  Instead, it SHOULD extract the credentials from the URI and redirect the user-agent again to another endpoint without exposing the credentials (in the URI or elsewhere). If third-party
   scripts are included, the client MUST ensure that its own scripts (used to extract and remove the credentials from the URI) will execute first.
   * Scope
Token EndPoint Usage
The token endpoint is used by the client to obtain an access token by presenting its authorization grant or refresh token.  The token endpoint is used with every authorization grant except for the implicit grant type (since an access token is issued directly). The client MUST use the HTTP "POST" method when making access token requests and should be https. It requires followings:
   * Client_id and client_secret for Client Authentication
   * Access Token Scope: multiple scope can be send as a delimited-space for example "openid email profile". This should send in case of Authorization end point too. In response, the authorization server MUST include the "scope" response parameter to inform the client of the actual scope granted.

POST /token HTTP/1.1
     Host: server.example.com
     Content-Type: application/x-www-form-urlencoded

     grant_type=refresh_token&refresh_token=tGzv3JOkF0XG5Qx2TlKWIA
     &client_id=s6BhdRkqt3&client_secret=7Fjfp0ZBr1KtDRbnfVdmIw

Authorization Code Flow
The authorization code grant type is used to obtain both access  tokens and refresh tokens and is optimized for confidential clients. Since this is a redirection-based flow, the client must be capable of interacting with the resource owner's user-agent (typically a web browser) and capable of receiving incoming requests (via redirection) from the authorization server:
   (A)  The client initiates the flow by directing the resource owner's user-agent to the authorization endpoint.  The client includes its client identifier, requested scope, local state, and a redirection URI to which the authorization server will send the user-agent back once access is granted (or denied).
   (B)  The authorization server authenticates the resource owner (via the user-agent) and establishes whether the resource owner grants or denies the client's access request.
   (C)  Assuming the resource owner grants access, the authorization server redirects the user-agent back to the client using the redirection URI provided earlier (in the request or during client registration).  The redirection URI includes an authorization code and any local state provided by the client earlier.
   (D)  The client requests an access token from the authorization server's token endpoint by including the authorization code received in the previous step.  When making the request, the client authenticates with the authorization server.  The client includes the redirection URI used to obtain the authorization code for verification.
   (E)  The authorization server authenticates the client, validates the authorization code, and ensures that the redirection URI received matches the URI used to redirect the client in step (C).  If valid, the authorization server responds back with an access token and, optionally, a refresh token.
   Example of step A:
   GET /authorize?response_type=code&client_id=s6BhdRkqt3&state=xyz&redirect_uri=https%3A%2F%2Fclient%2Eexample%2Ecom%2Fcb
   Example of Response Received in Step C:
   HTTP/1.1 302 Found
     Location: https://client.example.com/cb?code=SplxlOBeZQQYbYS6WxSbIA&state=xyz
   Example of Step D (Authentication should do by client_id and client_secret or by Authorization Header presented here!):
   POST /token HTTP/1.1
     Host: server.example.com
     Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW
     Content-Type: application/x-www-form-urlencoded

     grant_type=authorization_code&code=SplxlOBeZQQYbYS6WxSbIA
     &redirect_uri=https%3A%2F%2Fclient%2Eexample%2Ecom%2Fcb
   An example successful response in Step E:

     HTTP/1.1 200 OK
     Content-Type: application/json;charset=UTF-8
     Cache-Control: no-store
     Pragma: no-cache

     {
       "access_token":"2YotnFZFEjr1zCsicMWpAA",
       "token_type":"example",
       "expires_in":3600,
       "refresh_token":"tGzv3JOkF0XG5Qx2TlKWIA",
       "example_parameter":"example_value"
     }

Implicit Flow
The implicit grant type is used to obtain access tokens (it does not support the issuance of refresh tokens) and is optimized for public clients known to operate a particular redirection URI.  These clients are typically implemented in a browser using a scripting language such as JavaScript.
Since this is a redirection-based flow, the client must be capable of interacting with the resource owner's user-agent (typically a web browser) and capable of receiving incoming requests (via redirection) from the authorization server. The implicit grant type does not include client authentication (It does not require client secret but it require client_id).
   (A)  The client initiates the flow by directing the resource owner's user-agent to the authorization endpoint.  The client includes its client identifier, requested scope, local state, and a redirection URI to which the authorization server will send the user-agent back once access is granted (or denied).
   (B)  The authorization server authenticates the resource owner (via the user-agent) and establishes whether the resource owner grants or denies the client's access request.
   (C)  Assuming the resource owner grants access, the authorization server redirects the user-agent back to the client using the redirection URI provided earlier.  The redirection URI includes the access token in the URI fragment.
   (D)  The user-agent follows the redirection instructions by making a request to the web-hosted client resource.
   (E)  The web-hosted client resource returns a web page (typically an HTML document with an embedded script) capable of accessing the full redirection URI including the fragment retained by the user-agent, and extracting the access token (and other parameters) contained in the fragment.
   (F)  The user-agent executes the script provided by the web-hosted client resource locally, which extracts the access token.
   (G)  The user-agent passes the access token to the client.
   Example of Step A: 
   GET /authorize?response_type=token&client_id=s6BhdRkqt3&state=xyz
        &redirect_uri=https%3A%2F%2Fclient%2Eexample%2Ecom%2Fcb
   Example of Response in Step C:
    HTTP/1.1 302 Found
     Location: http://example.com/cb#access_token=2YotnFZFEjr1zCsicMWpAA&state=xyz&token_type=example&expires_in=3600
Refreshing an Access Token
If the authorization server issued a refresh token to the client, the client makes a refresh request to the token endpoint:
POST /token HTTP/1.1
     Host: server.example.com
     Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW
     Content-Type: application/x-www-form-urlencoded

     grant_type=refresh_token&refresh_token=tGzv3JOkF0XG5Qx2TlKWIA

Open ID Connect
OpenID Connect 1.0 is a simple identity layer on top of the OAuth 2.0 protocol. It enables Clients to verify the identity of the End-User based on the authentication performed by an Authorization Server (same as OAuth), as well as to obtain basic profile information about the End-User in an interoperable and REST-like manner (Addition to OAuth). OAuth 2.0 define mechanisms to obtain and use Access Tokens to access resources but do not define standard methods to provide identity information.
OpenID Connect implements authentication as an extension to the OAuth 2.0 authorization process. Use of this extension is requested by Clients by including the "openid" scope value in the Authorization Request. Information about the authentication performed is returned in a JSON Web Token (JWT) [JWT] called an ID Token.
The OpenID Connect protocol, in abstract, follows the following steps.
   1- The RP (Client) sends a request to the OpenID Provider (OP).
   2- The OP authenticates the End-User and obtains authorization.
   3- The OP responds with an ID Token and usually an Access Token (This step may have multiple requests!).
   4- The RP can send a request with the Access Token to the UserInfo Endpoint.
   5- The UserInfo Endpoint returns Claims about the End-User.
Step 4 and 5 are where OIDC is different from OAuth2.0. Another difference is using ID token in step 3 which has authorization identity info including claims as jwt. 
ID Token
The primary extension that OpenID Connect makes to OAuth 2.0 to enable End-Users to be Authenticated is the ID Token data structure. The ID Token is a security token that contains Claims about the Authentication of an End-User by an Authorization Server when using a Client, and potentially other requested Claims. The ID Token is represented as a JSON Web Token (JWT). The following Claims are used within the ID Token for all OAuth 2.0 flows used by OpenID Connect: 
* iss (required): Issuer Identifier (case sensitive URL using the https scheme)
* sub (required): Subject Identifier (A locally unique and never reassigned identifier within the Issuer for the End-User, like 232323)
* aud (required): Audience(s) and It MUST contain the OAuth 2.0 client_id
* exp (required): Expiration time as epoch time number
* iat(required): 	Time at which the JWT was issued (epoch time)
* auth_time (optional): Time when the End-User authentication occurred.  When a max_age request is made or when auth_time is requested as an Essential Claim, then this Claim is REQUIRED.
* nonce (optional): String value used to associate a Client session with an ID Token, and to mitigate replay attacks. The value is passed through unmodified from the Authentication Request to the ID Token.
* acr (optional): It seems not useful? and The acr value is a case sensitive string.
* amr (optional): Authentication Methods References (JSON array of strings that are identifiers for authentication methods used in the authentication. Parties using this claim will need to agree upon the meanings of the values used)
* azp (optional): Authorized Party (the party to which the ID Token was issued. If present, it MUST contain the OAuth 2.0 Client ID)
{
   "iss": "https://server.example.com",
   "sub": "24400320",
   "aud": "s6BhdRkqt3",
   "nonce": "n-0S6_WzA2Mj",
   "exp": 1311281970,
   "iat": 1311280970,
   "auth_time": 1311280969,
   "acr": "urn:mace:incommon:iap:silver"
  }

Authentication to Authorization Server can follow one of three paths: the Authorization Code Flow (response_type=code), the Implicit Flow (response_type=id_token token or response_type=id_token), or the Hybrid Flow (This for example can combine both Implicit and Authorization Code Flows). The characteristics of the three flows are summarized in the following non-normative table. 
 * Authorization Code Flow
      - All tokens returned from Authorization Endpoint? No
      - All tokens returned from Token Endpoint? Y
      - Tokens not revealed to User Agent? Y
      - Client can be authenticated? Yes we send both client Id and client secret
      - Refresh Token possible? y
      - Communication in one round trip? N
      - Most communication server-to-server? Yes

* Implicit Flow
      - All tokens returned from Authorization Endpoint? y
      - All tokens returned from Token Endpoint? n
      - Tokens not revealed to User Agent? No it reveals
      - Client can be authenticated? n
      - Refresh Token possible? n
      - Communication in one round trip? y
      - Most communication server-to-server? no (the case we do verification in server considered hybrid??!)

Hybrid Flow
      - All tokens returned from Authorization Endpoint? No. some returns from Token end point
      - All tokens returned from Token Endpoint? no. some return from authorization end point
      - Tokens not revealed to User Agent? no. some revealed!
      - Client can be authenticated? Yes we send both client Id and client secret in server sider
      - Refresh Token possible? y
      - Communication in one round trip? N
      - Most communication server-to-server? both client and server do some requests

Selecting Response Type when sending request to Authorization End Point determine flow:
code (Authorization Code Flow)
id_token (Implicit Flow)
id_token token (Implicit Flow)
code id_token (Hybrid Flow)
code token  (Hybrid Flow)
code id_token token (Hybrid Flow)

Authorization Code Flow
When using the Authorization Code Flow, all tokens are returned from the Token Endpoint. The Authorization Code Flow returns an Authorization Code to the Client, which can then exchange it for an ID Token and an Access Token directly (In OAuth it just returns access token and may be refresh_token). This provides the benefit of not exposing any tokens to the User Agent and possibly other malicious applications with access to the User Agent. The Authorization Server can also authenticate the Client before exchanging the Authorization Code for an Access Token (In request to token endpoint). The Authorization Code flow is suitable for Clients that can securely maintain a Client Secret between themselves and the Authorization Server. Client Secret used for getting the tokens not the code.
The Authorization Code Flow goes through the following steps.
    0- Client prepares an Authentication Request containing the desired request parameters.
    1- Client sends the request to the Authorization Server.
    2- Authorization Server Authenticates the End-User.
    3- Authorization Server obtains End-User Consent/Authorization.
    4- Authorization Server sends the End-User back to the Client with an Authorization Code.
    5- Client requests a response using the Authorization Code at the Token Endpoint.
    6- Client receives a response that contains an ID Token and Access Token in the response body.
    7- Client validates the ID token and retrieves the End-User's Subject Identifier (Specific to OIDC).

Authentication Request which is done through steps 0 to 4 is an OAuth 2.0 Authorization Request that requests that the End-User be authenticated by the Authorization Server. It can be a get or post request.  OpenID Connect uses the following OAuth 2.0 request parameters with the Authorization Code Flow in Authentication Request: 
*  scope (required): OpenID Connect requests MUST contain the openid scope value.
* response_type (required): When using the Authorization Code Flow, this value is code. 
* client_id (required)
* redirect_uri(required)
* state (recommended)
* response_mode (optional): This is specific to OIDC. It Informs the Authorization Server of the mechanism to be used for returning parameters from the Authorization Endpoint. 
* nonce (optional)
* display (optional): string value that specifies how the Authorization Server displays the authentication and consent user interface pages to the End-User. It can be "page" (default), "popup", "touch", "wap". 
* prompt (optional): string values that specifies whether the Authorization Server prompts the End-User for reauthentication and consent.
* max_age (optional): Maximum Authentication Age. Specifies the allowable elapsed time in seconds since the last time the End-User was actively authenticated by the OP. If the elapsed time is greater than this value, the OP MUST attempt to actively re-authenticate the End-User. 
* ui_locales (optional): End-User's preferred languages and scripts for the user interface
* id_token_hint (optional): ID Token previously issued by the Authorization Server being passed as a hint about the End-User's current or past authenticated session with the Client. This is used with prompt param. 
* login_hint (optional): 	Hint to the Authorization Server about the login identifier the End-User might use to log in (if necessary). This hint can be used by an RP if it first asks the End-User for their e-mail address (or other identifier) and then wants to pass that value as a hint to the discovered authorization service. This value MAY also be a phone number in the format specified for the phone_number Claim.
* acr_values (optional): Not very useful!! I dont undrestand exactly!!!
In case of Challanging an Open ID Connect in Server side, a redirect response sent to client as below: 
  HTTP/1.1 302 Found
  Location: https://server.example.com/authorize?
    response_type=code
    &scope=openid%20profile%20email
    &client_id=s6BhdRkqt3
    &state=af0ifjsldkj
    &redirect_uri=https%3A%2F%2Fclient.example.org%2Fcb
Or directly use the following in Client side:
  GET /authorize?
    response_type=code
    &scope=openid%20profile%20email
    &client_id=s6BhdRkqt3
    &state=af0ifjsldkj
    &redirect_uri=https%3A%2F%2Fclient.example.org%2Fcb HTTP/1.1
  Host: server.example.com

Successful response is as below:
  HTTP/1.1 302 Found
  Location: https://client.example.org/cb?
    code=SplxlOBeZQQYbYS6WxSbIA
    &state=af0ifjsldkj
In Steps 5,6,7 To obtain an Access Token, an ID Token, and optionally a Refresh Token, the RP (Client) sends a Token Request to the Token Endpoint to obtain a Token Response.
A Client makes a Token Request by presenting its Authorization Grant (in the form of an Authorization Code) to the Token Endpoint using the grant_type value authorization_code. If the Client is a Confidential Client, then it MUST authenticate to the Token Endpoint using the authentication method registered for its client_id (usually a client_secret which should send with client_id in request). The Client sends the parameters to the Token Endpoint using the HTTP POST method and the Form Serialization:
 POST /token HTTP/1.1
  Host: server.example.com
  Content-Type: application/x-www-form-urlencoded
  Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW (It may be client id and secret as post inputs)

  grant_type=authorization_code&code=SplxlOBeZQQYbYS6WxSbIA
    &redirect_uri=https%3A%2F%2Fclient.example.org%2Fcb
 After receiving and validating a valid and authorized Token Request from the Client, the Authorization Server returns a successful response that includes an ID Token and an Access Token. In addition to the response parameters specified by OAuth 2.0, a parameter with name id_token should be sent.

  HTTP/1.1 200 OK
  Content-Type: application/json
  Cache-Control: no-store
  Pragma: no-cache

  {
   "access_token": "SlAV32hkKG",
   "token_type": "Bearer",
   "refresh_token": "8xLOxBtZp8",
   "expires_in": 3600,
   "id_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjFlOWdkazcifQ.ewogImlzc
     yI6ICJodHRwOi8vc2VydmVyLmV4YW1wbGUuY29tIiwKICJzdWIiOiAiMjQ4Mjg5
     NzYxMDAxIiwKICJhdWQiOiAiczZCaGRSa3F0MyIsCiAibm9uY2UiOiAibi0wUzZ
     fV3pBMk1qIiwKICJleHAiOiAxMzExMjgxOTcwLAogImlhdCI6IDEzMTEyODA5Nz
     AKfQ.ggW8hZ1EuVLuxNuuIJKX_V8a_OMXzR0EHR9R6jgdqrOOF4daGU96Sr_P6q
     Jp6IcmD3HP99Obi1PRs-cwh3LO-p146waJ8IhehcwL7F09JdijmBqkvPeB2T9CJ
     NqeGpe-gccMg4vfKjkM8FcGvnzZUN4_KSP0aAp1tOJ1zZwgjxqGByKHiOtX7Tpd
     QyHE5lcMiKPXfEIQILVq0pc_E2DzL7emopWoaoZTF_m0_N0YzFC6g6EJbOEoRoS
     K5hoDalrcvRYLSrQAZZKflyuVCyixEoV9GfNQC3_osjzw2PAithfubEEBLuVVk4
     XUVrWOLrLl0nx7RkKU8NXNHq-rvKMzqg"
  }
In addition to OAuth, there is a step to validate token response.  The Client MUST validate the Token Response as follows:
    Follow the validation rules in OAuth2.
    Follow the ID Token validation rules.
    Follow the Access Token validation rules.

In Authorization Code Flow, there is the following ID Token Validation Rule (Some are specific to jwt validation (checking jwt signature) and some are specific for OIDC):
* To ckeck JWT genuinity, You may check https signature of endpoint or check signature of id_token. To check the signature of id_token, first decode header to get algorithm (which is RS256 by recommendation) and public key used for hashing (in case of RS256 it is public key of endpoint but in case of HS256 it is client_secret) and then check the jwt payload using the algorithm an key (rs256(encode(Header)+"."+encode(payload),rsaparams)== signature of jwt).
* To check id_token contents validity check issuer (iss). audience (aud), authorized party (azp), times (iat, auth_time,exp) and nonce and at_hash if present!
We elaborate them in following:
0- (specific to Authorization Code Flow): There is an optional at_hash (access_token hash) which Its value is the base64url encoding of the left-most half of the hash of the octets of the ASCII representation of the access_token value, (where the hash algorithm used is the hash algorithm used in the alg Header Parameter of the ID Token's JOSE Header).  For instance, if the alg is RS256, hash the access_token value with SHA-256, then take the left-most 128 bits and base64url encode them. The at_hash value is a case sensitive string. 
1- Validate ID Token Encryption if applicable: if the ID Token is encrypted, decrypt it using the keys and algorithms that the Client specified during Registration, otherwise go to step 2.
2- The Issuer Identifier for the OpenID Provider (which is typically obtained during Discovery(for example "https://accounts.google.com")) MUST exactly match the value of the "iss" (issuer) Claim. 
3- The Client MUST validate that the "aud" (audience) Claim contains its "client_id".
4- If the ID Token contains multiple audiences, the Client SHOULD verify that an "azp" Claim is present. 
5- If an "azp" (authorized party) Claim is present, the Client SHOULD verify that its "client_id" is the Claim Value. 
6- If the ID Token is received via direct communication between the Client and the Token Endpoint (which it is in this flow), the TLS server validation ((https) MAY be used to validate the issuer in place of checking the token signature  (jwt signature). Otherwise The Client MUST validate the signature of ID Tokens according to JWS [JWS] using the algorithm specified in the JWT alg Header Parameter. The Client MUST use the keys provided by the Issuer ([https://datatracker.ietf.org/doc/html/draft-ietf-jose-json-web-signature]).
7- The alg value SHOULD be the default of RS256 or the algorithm sent by the Client in the id_token_signed_response_alg parameter during Registration. 
8- If the JWT alg Header Parameter uses a MAC based algorithm such as HS256, HS384, or HS512, the octets of the UTF-8 representation of the client_secret are used as the key to validate the signature. 
9- The current time MUST be before the time represented by the exp Claim
10- The iat Claim can be used to reject tokens that were issued too far away from the current time (optional)
11- If a nonce value was sent in the Authentication Request, a nonce Claim MUST be present and its value checked to verify that it is the same value as the one that was sent in the Authentication Request. 
12- If the auth_time Claim was requested, either through a specific request for this Claim or by using the max_age parameter, the Client SHOULD check the auth_time Claim value and request re-authentication if it determines too much time has elapsed since the last End-User authentication. 
13- If the acr Claim was requested, the Client SHOULD check that the asserted Claim Value is appropriate (Out of scope of standard).
To validate Access Token you can use at_hash claim from ID_Token. 

Authentication using Implicit Flow in OIDC
This section describes how to perform authentication using the Implicit Flow.
* When using the Implicit Flow, all tokens are returned from the Authorization Endpoint; the Token Endpoint is not used. 
* The Implicit Flow is mainly used by Clients implemented in a browser using a scripting language.
* The Access Token and ID Token are returned directly to the Client, which may expose them to the End-User and applications that have access to the End-User's User Agent.
* The Authorization Server does not perform Client Authentication. So it do not require client secret.  
The Implicit Flow follows the following steps:
    1- Client prepares an Authentication Request containing the desired request parameters.
    2- Client sends the request to the Authorization Server.
    3- Authorization Server Authenticates the End-User.
    4- Authorization Server obtains End-User Consent/Authorization.
    5- Authorization Server sends the End-User back to the Client with an ID Token and, if requested, an Access Token.
    6- Client validates the ID token and retrieves the End-User's Subject Identifier.
The parameters used are same in Open ID Authorization Code Flow considering followings:
* response_type: When using the Implicit Flow, this value is id_token token or id_token (No Access Token Returned). Also While OAuth 2.0 defines the "token" Response Type value for the Implicit Flow, OpenID Connect does not use this Response Type, since no ID Token would be returned. 

Note: The response of this request has a token_type field with access_token and id_token and The value MUST be "Bearer" or another token_type value. This is used when requesting from user information endpoint if we use it (I am not sure!). Bearer can be simply understood as "give access to the bearer of this token."
Validation in Implicit Flow covers following:
* Response Validation as a whole: For example state should be same in request and returned response. 
* ID Token Validation: It is same as Authorization Code flow except nonce and at_hash are required here.
* Access_Token validation (at_hash in id_token used to validate access_token): Hash the binary presentation of ASCII representation of access token using algorithm in id_token header. Then select left_most half of hashed value and encode it to base64url encoding and compare with at_hash in id_token payload

Hybrid Flow in OIDC
When using the Hybrid Flow, some tokens are returned from the Authorization Endpoint and others are returned from the Token Endpoint. The Hybrid Flow follows the following steps:
    1- Client prepares an Authentication Request containing the desired request parameters.
    2- Client sends the request to the Authorization Server (Authorization Endpoint).
    3- Authorization Server Authenticates the End-User.
    4- Authorization Server obtains End-User Consent/Authorization.
    5- Authorization Server sends the End-User back to the Client with an Authorization Code and, depending on the Response Type, one or more additional parameters.
    6- Client requests a response using the Authorization Code at the Token Endpoint.
    7- Client receives a response that contains an ID Token and Access Token in the response body (It may be also sent in Authorization endpoint previously!).
    8- Client validates the ID Token and retrieves the End-User's Subject Identifier.
Response Type value that determines the authorization processing flow to be used, including what parameters are returned from the endpoints used. When using the Hybrid Flow, this value is "code id_token", "code token", or "code id_token token". 
Note: In this flow there is a field c_hash in id_token which can be checked similar to at_hash to validate code. 
You can read the following standard if you are interested in claims:
https://openid.net/specs/openid-connect-core-1_0.html#ImplicitTokenValidation


JWT Bearer Token Authentication Scheme
In Addition To above remote authentication scheme, we may use one othe scheme for sign-in action. We have Authenticate Action we do the Authentication but we should use another scheme to persist the result of authentication. That can do through Cookies or Bearer (Authorization Header) Where cookie used for web pages and Bearer usually used for APIs. In bearer method we use a JSON Token which has claims principal as payload (or may be simply a user ID which then the Identity reterived for Identity Store). 
JWT Token has three section: 
* its header which has security key and algorithm used for hashing
* its payload which is what we sent
* its signature: To verify we should check that hash of jwtheader.jwtpayload is equal to jwtsignature (It needs details!). 
All thing it should do in Authenticate step is to verify that signature of JWT is verified. Useually we create token for example as a result of an authentication with Cookie for example in one App and then use it in another App (API usually though following configuration):
services.AddAuthentication()
    .AddJwtBearer(cfg =>
    {
      cfg.TokenValidationParameters = new TokenValidationParameters()
      {
        ValidateIssuer = true,
        ValidIssuer = _config["Security:Tokens:Issuer"],
        ValidateAudience = true,
        ValidAudience = _config["Security:Tokens:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Security:Tokens:Key"])),

      };
    });
    
As it shows, we only require the security key to do verification and then we return the claims associated with token for example in an event as:
 options.Events = new JwtBearerEvents
  {
      OnMessageReceived = (ctx) =>
      {
      }
  }
this events and configuration exist in Microsoft.AspNetCore.Authentication.JwtBearer namespace.

Useful Types in working with JWT
All we need to sign and verify jwt exist in the System.IdentityModel.Tokens.Jwt namespace. It has following types:

Security Key
You can study them in below step. They are used for different crypto work including encryption and hashing.

SigningCredentials
This type used for signing the jwt. So it require a security key and an algorithm (in case of assymetric it require one for signature (get the signature with public key to compare with digest) and one for digest (Compute the digest with private key)). it has following constructor:
public SigningCredentials (Microsoft.IdentityModel.Tokens.SecurityKey key, string algorithm);
for example we have:
var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
var signingCredentials = new SigningCredentials(securityKey, "HS256");
The Algorithm used above get from SecurityAlgorithms static class with lots of algorithms.

JWTHeader
Initializes a new instance of JwtHeader which contains JSON objects representing the cryptographic operations applied to the JWT and optionally any additional properties of the JWT. For example it have alg used for siging and verifying. It has following Constructor:
* JwtHeader(SigningCredentials): Signing Credential is an instance used for having related properties. ({ { typ, JWT }, { alg, SigningCredentials.Algorithm } })
* JwtHeader(EncryptingCredentials): Here it encrypt the payload ({ { typ, JWT }, { alg, EncryptingCredentials.Alg }, { enc, EncryptingCredentials.Enc } })
for example we have:
 var header = new JwtHeader(signingCredentials);
This also can have properties of List of header parameter names see: https://datatracker.ietf.org/doc/html/rfc7519#section-5 like Kid, Cty (Content mim type)

JwtPayload
Initializes a new instance of JwtPayload which contains JSON objects representing the claims contained in the JWT. Each claim is a JSON object of the form { Name, Value } wich both are string. Claims can be Claims from System.Security.Claims.ClaimTypes or JwtRegisteredClaimNames(List of registered claims from different sources https://datatracker.ietf.org/doc/html/rfc7519#section-4 which include claims like Exp and Nbf (Note before!), Issuer, Audience, ..). For example:
  var payload = new JwtPayload
  {
      { "aud", environmentId },
      { "iat", dateTimeOffset.ToUnixTimeSeconds() }
  };
Or
 var claims=new Claim[] {
      new Claim(ClaimTypes.Name,username),
      new Claim(JwtRegisteredClaimNames.Nbf,new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString()),
      new Claim(JwtRegisteredClaimNames.Exp,new DateTimeOffset(DateTime.UtcNow.AddDays(1)).ToUnixTimeSeconds().ToString())
  };
var jwtPayload=new JwtPayload(claims);

JwtSecurityToken
There are multiple type of token, but we only here work with jwt which is used for creating and presenting JWT Token. It has following constructor:
var token=new JwtSecurityToken(
    header:jwtHeader,payload:jwtPayload
);
Since this class has not any method to get token value we use the 
JsonSecurityTokenHandler class to work with jwt. Form example we have following which return token:
var jwt=new JwtSecurityTokenHandler().WriteToken(token);



Security Keys related Type
There is lots of different security keys which used for different security related work like Encrytion, Hashing and Siging which exist in Microsoft.IdentityModel.Tokens name space. we use them to generate keys for example from an string or any Byte Array. The purpose is to create a key which is strong enough. It uses provided value as initial value and create the key.

SecurityKey
This is an abstract base class for Other security keys including  SymmetricSecurityKey (both sign and verify with one key), AssymetricSecurityKey (sign with private key and verify with public key) and JsonWebKey which used in JWT Tokens Verifying (For example in Open ID Connect)

SymmeticSecurityKey
It has SymmetricSecurityKey(Byte[]) constructor which return a key from a Byte array. You can create Byte array from key as string. For example we have:
var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("MySecretKey"));

AssymetricSecurityKey
The following security key derived from this type depending on hashing algorithm:
* RsaSecurityKey
* X509SecurityKey
* ECDsaSecurityKey

JsonWebKey (JWK)
Represents a JSON Web Key as defined in https://datatracker.ietf.org/doc/html/rfc7517 and it has following constructure:
JsonWebKey(String) 	: Initializes an new instance of JsonWebKey from a json string.
This Key has Algorithm used for signing too which makes it different with the above ones.


