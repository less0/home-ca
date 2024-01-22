# HomeCaFrontend

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 16.2.1.

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. It is required that you enter your Auth0 configuration (domain and client ID) in src/assets/env.js - don't push the changes to git. 

## Running with docker

The image built with `docker build` requires the following environment variables:

| Variable | Description |
|-|-|
| AUTH0_DOMAIN | Your Auth0 tenant domain |
| AUTH0_CLIENT_ID | Your Auth0 client ID |
| SSL_CERT | The SSL certificate for the server. All newlines have to be replaced with `%`. |
| SSL_KEY | The SSL private key for the server. All newlines have to be replaced with `%`. |

The container will listen at port 80. Assuming this port is available on your machine, you can run the frontend with

```
docker build . -t home-ca-frontend
docker run -p 80:80 --env AUTH0_DOMAIN=<your tenant domain> --env AUTH0_CLIENT_ID=<your client ID> home-ca-frontend
```