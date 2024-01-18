FROM node:latest as buildenv
WORKDIR /app
COPY . .
RUN npm install 
RUN npm run build --prod

FROM nginx:alpine
COPY ./nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=buildenv /app/dist/home-ca-frontend /usr/share/nginx/html

CMD ["/bin/sh",  "-c",  "envsubst < /usr/share/nginx/html/assets/env.template.js > /usr/share/nginx/html/assets/env.js && exec nginx -g 'daemon off;'"]