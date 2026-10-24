# Auth0 Configuration

Menu uses an Auth0 Single Page Application (SPA) for browser sign-in and an Auth0 API for bearer-token validation. The SPA must be able to request an API token silently after the initial login and renew that token without requiring the user to consent again.

## Dashboard configuration

In the Auth0 Dashboard, configure the API and the Menu SPA application as follows:

1. Open **Applications → APIs**, select the API whose **Identifier** is `http://localhost:65273` (or the identifier used by the deployed environment), and enable **Allow Offline Access**. Save the change.
2. Open **Applications → Applications**, select the Menu SPA, and enable **Refresh Token Rotation**. Keep refresh-token reuse detection enabled and retain the tenant-approved **Rotation Overlap Period**. The overlap period allows concurrent requests to use the previous refresh token briefly; change it only if the deployment has an explicit requirement for a different value. Save the change.
3. On the SPA application, add the local Aspire UI URL to each of these fields:
   - **Allowed Callback URLs**: `http://localhost:5173`
   - **Allowed Logout URLs**: `http://localhost:5173`
   - **Allowed Web Origins**: `http://localhost:5173`

Do not commit the client secret, refresh token, access token, or exported Auth0 configuration. A browser SPA uses the client ID, not a client secret.

## Local application values

When running through Aspire, the API is exposed at `http://localhost:5002` and the UI at `http://localhost:5173`. These public ports are below Windows' dynamic TCP port range: Windows can exclude ranges containing the previous ports `65273` and `65276`, preventing Aspire from binding its proxies even while the application processes are running.

`AddViteApp` passes a separate allocated target port to Vite. Open the public UI URL shown by Aspire and use that same URL for Playwright and Auth0 callbacks. The UI's HTTP health check verifies that public endpoint before Aspire reports it healthy.

The API identifier configured in Auth0 must match the audience requested by the UI and validated by the API:

| Setting | Value |
| --- | --- |
| Auth0 API audience | `http://localhost:65273` |
| `VITE_MENU_API_URL` | `http://localhost:5002` |
| `VITE_AUTH0_AUDIENCE` | `http://localhost:65273` |
| Aspire `Auth0Audience` parameter | `http://localhost:65273` |
| Aspire `Auth0Domain` parameter | The Auth0 tenant domain, without `https://` |

The audience is the registered Auth0 API identifier and is independent of the API's network address. Auth0's [API settings documentation](https://auth0.com/docs/get-started/apis/api-settings#general-settings) explains that the identifier is fixed after API creation. The existing identifier therefore stays `http://localhost:65273` when the API moves to port `5002`; using a new identifier would require registering a new API in Auth0.

Aspire injects its `Auth0Audience` parameter into `VITE_AUTH0_AUDIENCE` and into the backend's JWT validation configuration. The checked-in `.env` supplies the same identifier for standalone Vite development. Moving the API to another port only changes `VITE_MENU_API_URL`. Changing the public UI URL requires updating the SPA's allowed callback URLs, logout URLs, and web origins.

## Verification

After saving the dashboard settings:

1. Start the full Aspire application.
2. Sign in at `http://localhost:5173`.
3. Confirm that a protected recipe page loads and that its request to `GET http://localhost:5002/api/recipe` succeeds.
4. Leave the application open until the access token would normally expire, or clear the in-memory token while retaining the Auth0 session, and confirm the page can obtain a new token without showing `consent_required`.
5. If the browser reports `consent_required`, verify **Allow Offline Access**, **Refresh Token Rotation**, the API audience, and the callback/origin URLs before changing application code.
