# BaoSteel

## Configuration  File

开发时文件路径: Asssets/Config/

Build之后文件路径: ${ApplicationName}_Data/Config/

### 配置文件说明

- camera: 四行参数(换行)，分别表示滚轮缩放速度(1-10)/相机旋转速度(1-10)/鼠标中键拖拽速度(1-10)/相机离中心最近距离(0.1-30)
- DataServiceManagerConfig: 格式为ip: xxx.xxx.xxx.xxx \n port: xxxxx
- ModelManager: ==TODO==
- ShowPartModel: ==TODO==
- ui: 空格隔开，每个参数为可以裁剪的角度

## Manager Scripts

1. ConfigurationManager : Mono

   读取配置文件,调用其他的Manager相应的初始化函数(所有的初始化应当在这里进行，避免时序问题)
   
2. CoroutineHandler: MonoSingleton

   协程管理，主要负责数据库部分读取的协程处理

3. CullingController : Singleton

   裁剪控制器，所有需要裁剪的材质放在Resources/ClippingMaterials下，且必须是Shader Graph/standard着色器的材质。提供特定角度/特定高度区间的裁剪和关闭所有裁剪

4. DataServiceManager: Singleton

   ==TODO==

5. LayerManager: Singleton

   层管理器，选中的物体在highlight层，未选中的在ambient层，当ambient层有物体时，会打开SRP的Custom Blit Render Feature给背景层相机添加蒙版(默认关闭是权重为0，打开权重为0.7)

6. ModelManager: MonoSingleton

   ==TODO==

7. SelectionManager : Singleton

   基于UnityFx.Outline Package的描边效果，提供当鼠标选中或者某些ui元素的mouse enter event触发时，添加或移除边缘高亮的效果，具体描边效果设置在Settings/OutlineLayerCollection

8. UIManager: MonoSingleton

   根据配置文件给对应的UI元素添加子项和相应的事件(目前包括裁剪的dropdown和模型分块选择的multi-select dropdown)

## Util 全局静态方法(Util.${MethodName}调用)

- ```ReadConfigFile``` 读取路径下的配置文件，全部内容返回为单个string(自主用正则或者分割解析)
- ```FindDistinctObjectsWithTags``` 查找多个tag下所有的物体(去重)
- ```WrapAngle``` 把0\~360的角度映射到-180\~180
- ```FindObjectsInLayer``` 查找特定layer下所有的物体
- ```IsAnyObjectsInLayer``` 查找特定layer下是否有物体

## Others

Resources/ClippingMaterals为需要裁剪的材质

Resouces/Prefabs为动态生成的模型的prefab

其他需要动态加载的部分也应当放在Resources目录下