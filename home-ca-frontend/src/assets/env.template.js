(function(window) {
    window["env"] = window["env"] || {};
  
    // Environment variables
    window["env"]["AUTH0_DOMAIN"] = '${AUTH0_DOMAIN}';
    window["env"]["AUTH0_CLIENT_ID"] = '${AUTH0_CLIENT_ID}';
  })(this);