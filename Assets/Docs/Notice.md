### 配置文件

配置都放在Application.datapath下的/Config/文件

在build之前是Assets/Config/

build之后根据平台不同不同，webgl不可通过这种方式读配置

### 脚本目录

所有控制器脚本放在Scripts/Managers/下

所有单例类以及工具静态类放在Scripts/SingleInstance&Utilities下

### 动态加载的材质以及prefab

放在Resources/的对应目录下

无需裁剪的材质放在Materials里

### 读取配置文件

调用ExternalConfigReader.Instance().ReadConfigFile()将配置文件全部内容读取为一个string

仿照ConfigurationManager里的InitializeCamera()的方式进行配置初始化

* 文件名写死
* 文件解析的过程写死(类似于给txt规定了一个语法)
* 将解析后的Debug输出一下，方便快速查看内容
* 通过FindObjectofType<>直接调用对应的子管理器或者脚本的设置函数传递解析后的配置

#### 裁剪控制器

单例的控制器，使用单例的模式调用，场景中通过继承monobehaviour的测试脚本测试功能，后期使用UI绑定时间

### **数据服务管理器**

1.接口:DataServiceManager.Instance().GetTemperature(funcName)

- 作用：获得所有热电偶的温度数据 需传入处理函数名funcName

- 注意点：

  - funcName为数据后续处理函数。函数定义需要一个string类型参数（与数据类型一致） 返回值为bool类型   

    eg：bool funcName(string temperatureData){ do dataOperation}

  - 接口为协程 需要通过StartCoroutine()启动  ，所在脚本需继承MonoBehaviour

    eg：StartCoroutine(DataServiceManager.Instance().GetTemperature(funcName));

​	