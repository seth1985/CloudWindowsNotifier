import { PublicClientApplication, type AuthenticationResult } from '@azure/msal-browser';

type EntraConfig = {
  authority: string;
  clientId: string;
  scope: string;
};

export async function loginWithEntra(config: EntraConfig): Promise<string> {
  const msal = new PublicClientApplication({
    auth: {
      clientId: config.clientId,
      authority: config.authority,
      redirectUri: window.location.origin
    },
    cache: {
      cacheLocation: 'localStorage'
    }
  });

  await msal.initialize();

  let result: AuthenticationResult;
  const accounts = msal.getAllAccounts();
  if (accounts.length > 0) {
    try {
      result = await msal.acquireTokenSilent({
        account: accounts[0],
        scopes: [config.scope]
      });
      return result.accessToken;
    } catch {
      // silent acquisition can fail when consent/session is missing.
    }
  }

  result = await msal.loginPopup({
    scopes: [config.scope]
  });

  return result.accessToken;
}

