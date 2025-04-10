import { defineBoot } from '#q-app/wrappers';
import { createAuth0 } from '@auth0/auth0-vue';

export default defineBoot(({ app }) => {
  app.use(
    createAuth0({
      domain: 'dev-oz81ytsjd1h4r1lz.uk.auth0.com',
      clientId: 'XkygJHG4uxZ2g4vF0LMYmRWsWxeIXqQa',
      authorizationParams: {
        redirect_uri: window.location.origin,
      },
    }),
  );
});
