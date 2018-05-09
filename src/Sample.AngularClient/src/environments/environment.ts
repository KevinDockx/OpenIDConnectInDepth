// The file contents for the current environment will overwrite these during build.
// The build system defaults to the dev environment which uses `environment.ts`, but if you do
// `ng build --env=prod` then `environment.prod.ts` will be used instead.
// The list of which env maps to which file can be found in `.angular-cli.json`.

export const environment = {
  production: false,
  apiUrl: 'https://localhost:44392/api',
  openIdConnectSettings: {
    authority: 'https://localhost:44379',
    client_id: 'sampleuseragentclient',
    redirect_uri: 'https://localhost:4200/signin-oidc',
    post_logout_redirect_uri: 'https://localhost:4200/',
    response_type: 'id_token token',
    scope: 'openid profile sampleapi',
    automaticSilentRenew: true,
    silent_redirect_uri: 'https://localhost:4200/redirect-silentrenew',
    loadUserInfo: true,
    revokeAccessTokenOnSignout: true
    }
};
 