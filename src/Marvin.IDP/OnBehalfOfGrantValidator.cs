using IdentityServer4.Models;
using IdentityServer4.Validation;
using System.Linq;
using System.Threading.Tasks;

namespace Marvin.IDP
{
    /// <summary>
    /// Implementation of the on behalf of grant 
    /// as described at https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-protocols-oauth-on-behalf-of
    /// </summary>
    public class OnBehalfOfGrantValidator : IExtensionGrantValidator
    {
        private readonly ITokenValidator _validator;

        public OnBehalfOfGrantValidator(ITokenValidator validator)
        {
            _validator = validator;
        }

        public string GrantType => "urn:ietf:params:oauth:grant-type:jwt-bearer";

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {

            var requestedTokenUse = context.Request.Raw.Get("requested_token_use");
            if (string.IsNullOrWhiteSpace(requestedTokenUse)
                || requestedTokenUse.ToLowerInvariant() != "on_behalf_of")
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest);
                return;
            }

            var assertion = context.Request.Raw.Get("assertion");
            if (string.IsNullOrWhiteSpace(assertion))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest);
                return;
            }

            var result = await _validator.ValidateAccessTokenAsync(assertion);
            if (result.IsError)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
                return;
            }

            // get the subject from the access token
            var subjectClaim = result.Claims.FirstOrDefault(c => c.Type == "sub");
            if (subjectClaim == null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest);
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

            context.Result = new GrantValidationResult(subjectClaim.Value, "access_token", result.Claims);
            return;
        }
    }
}