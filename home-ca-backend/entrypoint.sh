#! /bin/sh

echo $SSL_CERT | tr '%' '\n' > /etc/ssl/kestrel.pem 
echo $SSL_KEY | tr '%' '\n' > /etc/ssl/kestrel.key
dotnet home-ca-backend.Api.dll
