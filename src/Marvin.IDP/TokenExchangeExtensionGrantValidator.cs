using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Marvin.IDP
{
    public class TokenExchangeExtensionGrantValidator : IExtensionGrantValidator
    {
        private readonly ITokenValidator _validator;

        public TokenExchangeExtensionGrantValidator(ITokenValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public string GrantType => "urn:ietf:params:oauth:grant-type:token-exchange";
        private string _accessTokenType => "urn:ietf:params:oauth:token-type:access_token";

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var requestedGrant = context.Request.Raw.Get("grant_type");
            if (string.IsNullOrWhiteSpace(requestedGrant) || requestedGrant != GrantType)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant,
                    "Invalid grant.");
                return;
            }

            var subjectToken = context.Request.Raw.Get("subject_token");
            if (string.IsNullOrWhiteSpace(subjectToken))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                    "Subject token missing.");
                return;
            }

            var subjectTokenType = context.Request.Raw.Get("subject_token_type");
            if (string.IsNullOrWhiteSpace(subjectTokenType))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                    "Subject token type missing.");
                return;
            }

            // use the subject token type to know how to validate it.  Must be
            // an access token in our case
            if (subjectTokenType != _accessTokenType)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                   "Subject token type invalid.");
                return;
            }

            var result = await _validator.ValidateAccessTokenAsync(subjectToken);
            if (result.IsError)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant,
                    "Subject token invalid.");
                return;
            }

            // get the subject from the access token
            var subjectClaim = result.Claims.FirstOrDefault(c => c.Type == "sub");
            if (subjectClaim == null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                    "Subject token must contain sub value.");
                return;
            }

            // we've got the subject claim and the token is valid. This is where additional rules
            // can be checked that can result in not giving access, for example: checking if the user
            // is allowed access to the API (s)he wants access to  This depends on the implementation.  
            //
            // ... additional checks
            //
            // This is also where claims transformation can happen, additional claims about the 
            // user can be returned, etc
            //
            // ... claims transformation
            // 
            // If all checks out we set the result to a GrantValidationResult, passing in 
            // the users' identifier, authentication method ("access token") 
            // and set of claims (in this example: the incoming claims)
            //
            // It's these incoming claims that will be checked (by IdSrv) against the approved
            // api scope (=> claims) rules.  Eg: if the incoming token has an email claim but the 
            // api scope that's requested doesn't offer access to that email claim, it will not be included.  
            // 
            // This also works the other way around: it's only the incoming claims passed through to the
            // grant validation result that are considered.  Eg: if an incoming token doesn't have an email claim 
            // and the email claim is thus not passed to the grant validation result, the new token
            // will not contain an email claim even if the requested api scope grants access to it.

            context.Result = new GrantValidationResult(subjectClaim.Value, "access_token", result.Claims);
            return;
        }
    }
}