# 1 基本结构

- server: nodeJs后台，文档协作的主web服务，用于连接数据库、redis、rabbitmq等。
- sdkjs: 前端项目，负责读写与后台交互的文档输入输出流，提供了可供editor便捷调用的api接口。
- web-apps: 前端项目，editor的前端代码。
- editor-server: nodeJs后台，负责与日事清集成，提供显示editor的iframe页面。

note: 以上工程为前端和集成的工程。在具体开发使用过程中需要先搭建并启动document server服务。

# 2 安装步骤

## 搭建document server服务
centOS参考：[document server for centOS](http://helpcenter.onlyoffice.com/server/linux/document/linux-installation-centos.aspx)
其他平常参考：[document server全平台](http://helpcenter.onlyoffice.com/server/document.aspx)

## git clone source code
`git clone git@github.com:rishiqing/cooffice.git`

## npm install
`cd server/Common && npm install`  
`cd server/DocService && npm install`  
`cd sdkjs/build && npm install`  
`cd web-apps/build && npm install`  
`cd editor-server && npm install`

# 3 开发步骤
## 修改document server中nginx的配置，反向代理到本地开发机器
`vim /etc/nginx/conf.d/onlyoffice-documentserver.conf`  
示例：  

    # add development web-apps
    location ~ ^(\/2017-12-07-11-28)?(\/web-apps\/.*) {
    proxy_pass http://[local_ip]:[local_port]$2;
    }
    # add development sdkjs
    location ~ ^(\/2017-12-07-11-28)?(\/sdkjs\/.*) {
      proxy_pass http://[local_ip]:[local_port]$2;
    }
    location ~ ^(\/2017-12-07-11-28)?(\/fonts\/.*) {
      proxy_pass http://[local_ip]:[local_port]$2;
    }

其中[local_ip]和[local_port]分别表示本地的ip和端口号

## start local server
`cd server/DocService/sources && node server.js`

## start local editor server
`cd editor-server && node bin/www`

## 浏览器访问
浏览器打开： `http://[local_ip]:3000`访问editor-server

# 4 打包及部署步骤
## grunt compile frontend source code
`cd sdkjs/build && grunt --level=ADVANCED`  
等待sdkjs编译结束后，执行  
`cd web-apps/build && grunt --level=ADVANCED`  
编译成功，会在`web-apps/deploy/`目录下生成打包文件

## replace the code of document server
覆盖document server中的相关文件，例如：  
`/var/www/onlyoffice/documentserver/sdkjs`  
和  
`/var/www/onlyoffice/documentserver/web-apps`

## change folder auth and regenerate font files
document server中，修改目录权限  
`chown -R onlyoffice:onlyoffice /var/www/onlyoffice/documentserver/sdkjs`  
`chown -R onlyoffice:onlyoffice /var/www/onlyoffice/documentserver/web-apps`  
生成字体文件  
`/usr/bin/documentserver-generate-allfonts.sh`

## change relative configurations
`cd /etc/onlyoffice/documentserver`  
基本的配置文件包括：  

- redis路径
- rabbitmq路径

# 5 配置文件
## server config
path: `server/Common/config`  
一般需要修改rabbitmq/redis等配置

## sdkjs config
path: `sdkjs/build/configs`
grunt构建的配置，一般不需要修改

## web-apps config
path: 
`web-apps/build/common.json`  
`web-apps/build/documenteditor.json`  
`web-apps/build/presentationeditor.json`  
`web-apps/build/spreadsheeteditor.json`
grunt构建配置，一般需要修改version版本号

## editor-server config
path: `editor-server/config`
需要修改`siteUrl`为document server的访问路径  

# 6 常见问题
## build版本问题
web-apps/build/common.json  
web-apps/build/documenteditor.json  
web-apps/build/presentationeditor.json  
web-apps/build/spreadsheeteditor.json  
以上配置文件中`version`字段需要与DocumentSever的后台version版本一致，例如`5.0.6`

## 密钥问题
`cd server/DocService/sources && node rsqLicense.js`  
生成密钥文件

powered by [onlyoffice](https://www.onlyoffice.com/)