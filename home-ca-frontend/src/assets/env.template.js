(function(window) {
    window["env"] = window["env"] || {};
  
    // Environment variables
    window["env"]["AUTH0_DOMAIN"] = '${AUTH0_DOMAIN}';
    window["env"]["AUTH0_CLIENT_ID"] = '${AUTH0_CLIENT_ID}';
    window["env"]["BACKEND_URI"] = 'https://${BACKEND_HOSTNAME}:${BACKEND_PORT}'
  })(this);