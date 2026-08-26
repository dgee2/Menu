# Auth0 Configuration

Menu uses an Auth0 Single Page Application (SPA) for browser sign-in and an Auth0 API for bearer-token validation. The SPA must be able to request an API token silently after the initial login and renew that token without requiring the user to consent again.

## Dashboard configuration

In the Auth0 Dashboard, configure the API and the Menu SPA application as follows:

1. Open **Applications → APIs**, select the API whose **Identifier** is `http://localhost:65273` (or the identifier used by the deployed environment), and enable **Allow Offline Access**. Save the change.
2. Open **Applications → Applications**, select the Menu SPA, and enable **Refresh Token Rotation**. Keep refresh-token reuse detection enabled and retain the tenant-approved **Rotation Overlap Period**. The overlap period allows concurrent requests to use the previous refresh token briefly; change it only if the deployment has an explicit requirement for a different value. Save the change.
3. On the SPA application, add the local Aspire UI URL to each of these fields:
   - **Allowed Callback URLs**: `http://localhost:65276`
   - **Allowed Logout URLs**: `http://localhost:65276`
   - **Allowed Web Origins**: `http://localhost:65276`

Do not commit the client secret, refresh token, access token, or exported Auth0 configuration. A browser SPA uses the client ID, not a client secret.

## Local application values

When running through Aspire, the API is exposed at `http://localhost:65273` and the UI at `http://localhost:65276`.

The API identifier configured in Auth0 must match the audience requested by the UI and validated by the API:

| Setting | Value |
| --- | --- |
| Auth0 API audience | `http://localhost:65273` |
| `VITE_MENU_API_URL` | `http://localhost:65273` |
| Aspire `Auth0Audience` parameter | `http://localhost:65273` |
| Aspire `Auth0Domain` parameter | The Auth0 tenant domain, without `https://` |

If a different port or deployed URL is used, update the audience and all three application URLs together. The audience must be the API identifier, not the Auth0 domain.

## Verification

After saving the dashboard settings:

1. Start the full Aspire application.
2. Sign in at `http://localhost:65276`.
3. Confirm that a protected recipe page loads and that its request to `GET http://localhost:65273/api/recipe` succeeds.
4. Leave the application open until the access token would normally expire, or clear the in-memory token while retaining the Auth0 session, and confirm the page can obtain a new token without showing `consent_required`.
5. If the browser reports `consent_required`, verify **Allow Offline Access**, **Refresh Token Rotation**, the API audience, and the callback/origin URLs before changing application code.
