import { createAuth0 as auth0 } from '@auth0/auth0-vue';

export const createAuth0 = () =>
  auth0({
    domain: 'dev-oz81ytsjd1h4r1lz.uk.auth0.com',
    clientId: 'XkygJHG4uxZ2g4vF0LMYmRWsWxeIXqQa',
    authorizationParams: {
      redirect_uri: window.location.origin,
    },
  });
