FROM node:latest as buildenv
WORKDIR /app
COPY . .
RUN npm install 
RUN npm run build --prod

FROM nginx:alpine
COPY ./nginx.conf /etc/nginx/conf.d/default.conf
COPY ./entrypoint.sh /usr/local/bin
COPY --from=buildenv /app/dist/home-ca-frontend /usr/share/nginx/html

CMD ["/bin/sh",  "-c", "entrypoint.sh"]