## How To Compile
  1. 添加MSBuild环境变量
  2. 使用UpdateAssembly.bat编译
  3. 使用XXXbuild.bat编译或在开发环境中编译
  
### How To Publish
  1. 修改Wes.Desktop.App.config 中wms服务地址WMS_REST_SERVER_URL
  2. 修改Wes.Utilities.App.config 中ElasticSearch中connectionString服务地址
  3. 编译整个WES项目(无msbuild环境变量时,请在开发环境中编译)
  4. VS启动Wes.Launcher点击发布
  
  说明：在配置了环境变量的情况下，双击publish可直接发布，双击publish-full发布后客户端将执行全量更新

  
### How To Install

  1. Download Wes.Launcher.exe  from  http://172.16.4.23/wes-v2/publish/
  2. make Folder c:/wes-v2
  3. copy Wes.Launcher.exe to c:/wes-v2
  4. run (double click) Wes.Launcher.exe