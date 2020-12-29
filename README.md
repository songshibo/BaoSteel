[toc]

# BaoSteel

## Configuration  File

开发时文件路径: Asssets/Config/

Build之后文件路径: {ApplicationName}_Data/Config/

### 配置文件说明

- camera: 四行参数(换行)，分别表示滚轮缩放速度(1-10)/相机旋转速度(1-10)/鼠标中键拖拽速度(1-10)/相机离中心最近距离(0.1-30)

- DataServiceManagerConfig: 格式为

  ```
  ip:xxx.xxx.xxx.xxx
  port:xxxxx
  ```

- ModelManager: 分号前表示 toggle 的名字，分号后表示该 toggle 所控制的模型。以 '-' 开始的模型表示本地模型。'?' 表示后面为高度数据，'<' 表示最低高度到最高高度。

- ui: 空格隔开，每个参数为可以裁剪的角度

  ```
  ${angle0} ${angle1} ${angle2} ...
  ```

- timing: 分号前表示计时器的名字，分号后表示计时器触发时间

  ```
  name:number # such as thermocouple_timing:10
  ```

- Tag：格式ModelManager一致，不过更具体到小的编号，例如cooling_wall7，以生成tag。

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

   层管理器，选中的物体在highlight层，未选中的在ambient层，当ambient层有物体时，会打开SRP的Custom Blit Render Feature给背景层相机添加蒙版(默认关闭是权重为0，打开权重为0.7)。

6. ModelManager: MonoSingleton

   模型管理器，主要负责生成 tag 、场景中所有的模型生成，以及查找场景中的模型。

7. SelectionManager : Singleton

   基于UnityFx.Outline Package的描边效果，提供当鼠标选中或者某些ui元素的mouse enter event触发时，添加或移除边缘高亮的效果，具体描边效果设置在Settings/OutlineLayerCollection。

   layerIndex参数默认为0，是热电偶选择对应的layer；dropdown按钮的mouse enter和exit事件对应layerIndex参数应为1。

8. UIManager: MonoSingleton

   根据配置文件给对应的UI元素添加子项和相应的事件(目前包括裁剪的dropdown和模型分块选择的multi-select dropdown)。
   
9. HeatLoadManager: MonoSingleton

   处理热负荷数据。

10. BatchManager: MonoSingleton

    处理料层数据。

## Util 全局静态方法(Util.${MethodName}调用)

- ```ReadConfigFile``` 读取路径下的配置文件，全部内容返回为单个string(自主用正则或者分割解析)
- ```FindChildren```找到物体的所有子物体gameobject
- ```RemoveComments```读取配置文件时，去掉注释
- ```FindDistinctObjectsWithTags``` 查找多个tag下所有的物体(去重)
- ```WrapAngle``` 把0\~360的角度映射到-180\~180
- ```FindObjectsInLayer``` 查找特定layer下所有的物体
- ```IsAnyObjectsInLayer``` 查找特定layer下是否有物体
- ```MergeThermocoupleName```得到热电偶的真正编号，TE4560A-TE4560B_1 to TE4560，TI4370-TI4379_1 to TI4370-TI4379，TE4360_1 to TE4360
- ```GenerateGradient```生成gradient贴图，输入是gradient的一系列颜色~~(注意按照a通道，从低到高排序)~~自动排序，a通道代表的意思即是这个颜色key所处在整个gradient贴图里的百分比。

## Others

Resources/ClippingMaterals为需要裁剪的材质

Resources/Prefabs为动态生成的模型的prefab

其他需要动态加载的部分也应当放在Resources目录下

# TODO

完成的项用~~删除线~~划掉

- ~~TimerManager(MonoSingleton): 读取每个更新器的更新时间，维护一个全局计时器，到特定时间调用相应的更新器~~
- ThermocoupleUpdater(Singleton): 处理所有热电偶的更新
- TuyereUpdater(Singleton): 处理所有关于风口的更新
- ~~HeatmapUpdater(Singleton): 处理热力图的更新 (ssb)~~
- ~~热电偶按钮的UI生成 (ss)~~
- ~~SelectionManager的补全，包括鼠标点选热电偶显示对应信息(ssb)~~(需要UI对接，数据库对接)
- 风口标签显示，最好UI跟踪，或者3D Billboard效果实现，标签位置以风口水平高度对齐，会有相互遮挡(目前用UI制作，需要调整canvas大小以及在模型background层时的显示问题)(ss)
- ~~每个模型使用不同材质(需要裁剪的材质放到Resources/ClippingMaterials下)(ssb)~~
- ~~不同区域的模型生成在同一个空父物体下 (ss)~~
- 展示单个热电偶信息的浮动UI以及获取相应UI数据的接口(基本完成，还需对接数据库)
- **ModelManager里生成tag的部分时通过UnityEditor实现的，因此存在无法build的问题，如果没有解决办法则把这个部分写成一个单独继承UnityEditor的脚本，在build之前固定Tag**
- 料层图片的生成（qjl）
- ~~根据温度采样颜色API（ssb）完成~~
- 热负荷数据（ss）

## DEBUG功能

导出HeatMap热力图等

在Unity的工具栏中选择```Tools/Export Texture```。在弹出的窗口中点击Export则会把贴图导出到指定目录下 (注意：需要在运行时使用，即有热力图生成后再导出)