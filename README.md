# fastNZB
source for the frontend and api of https://fastnzb.com.  
designed to operate alongside the indexing software nZEDb (https://github.com/nZEDb/nZEDb), this software does not index usenet, instead if operates as an off-site frontend to nZEDb to reduce load on the public facing server

# requirements
dotnet core v2.1  
mysql  
Amazon S3 bucket for storing NZB files  
sphinxsearch (optional)  
nginx (or apache)  

# running
In one terminal
```
cd src/FastNZB
npm i
npm run start
```
In another
```
dotnet watch run
```

# configuring
copy appsettings.sample.txt to appsettings.txt in both scripts/FastNZB.Import and src/FastNZB (each has a separate format)

# installation - import
run the following queries on your nZEDb database
```mysql
ALTER TABLE releases ADD exported TINYINT(1) DEFAULT 0;
ALTER TABLE releases ADD failedexport TINYINT(1) DEFAULT 0;
```
copy the build output from scripts/FastNZB.Import to your nZEDb server  
run this console program as a cron job however often as you'd like  
the configuration value ApiKey should be the same on both the import appsettings.txt and the api appsettings, this is a hacky way to verify the import is coming from a valid source  

# installation - frontend
```
npm build:prod
```
will build a production version of the angular 2 app and copy the output to wwwroot, copy wwwroot to your production server

# installation - api
```
dotnet build -o bin/Publish
```
this will build the api and place it in bin/Publish, copy this to your production server  

to install as a service on CentOS, add the following file to /etc/systemd/system/fastnzb.service
```
[Unit]
Description=FastNZB

[Service]
WorkingDirectory=/srv/www/fastnzb.com/public_html
ExecStart=/usr/bin/dotnet /srv/www/fastnzb.com/public_html/FastNZB.dll --server.urls http://*:5021
Restart=always
SyslogIdentifier=fastnzb
User=root
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5021

[Install]
WantedBy=multi-user.target
```
run
```
sudo systemctl daemon-reload
sudo service fastnzb start
```

# appsettings.txt - api
```
MySQL - read/write connection string
MySQLRead - read-only connection string (if using read-replicas, if not copy from above)
AWS_ACCESS_KEY - AWS access for s3 bucket
AWS_SECRET_KEY - AWS access for s3 bucket
S3BucketName AWS s3 bucket name
Redis - redis server for session/caching
BaseUrl - public url
FSPath - local nzb cache location
```
