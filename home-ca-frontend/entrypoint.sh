#! /bin/sh

echo $SSL_CERT | tr '%' '\n' > /etc/nginx/ssl.pem 
echo $SSL_KEY | tr '%' '\n' > /etc/nginx/ssl.key
envsubst < /usr/share/nginx/html/assets/env.template.js > /usr/share/nginx/html/assets/env.js 
exec nginx -g 'daemon off;'