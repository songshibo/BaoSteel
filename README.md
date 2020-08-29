# BaoSteel

## Configuration  File

开发时文件路径: Asssets/Config/

Build之后文件路径: ${ApplicationName}_Data/Config/

### 配置文件说明

- camera: 四行参数(换行)，分别表示滚轮缩放速度(1-10)/相机旋转速度(1-10)/鼠标中键拖拽速度(1-10)/相机离中心最近距离(0.1-30)

- DataServiceManagerConfig: 格式为

  ```
  ip:xxx.xxx.xxx.xxx
  port:xxxxx
  ```

- ModelManager: 程序需要实例化的模型。以 ’-‘ 开头的行代表该模型不需要读取数据库的信息。

- ShowPartModel: 每一行对应于 dropdown 中的一个 toggle ，分号前表示该 toggle 的名字，分号后表示该 toggle 所影响的模型。问号后代表属于某个高度范围的模型。

- ui: 空格隔开，每个参数为可以裁剪的角度

  ```
  ${angle0} ${angle1} ${angle2} ...
  ```

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

   模型管理器，主要负责生成 tag 、场景中所有的模型生成，以及查找场景中的模型。

7. SelectionManager : Singleton

   基于UnityFx.Outline Package的描边效果，提供当鼠标选中或者某些ui元素的mouse enter event触发时，添加或移除边缘高亮的效果，具体描边效果设置在Settings/OutlineLayerCollection。

   layerIndex参数默认为0，是热电偶选择对应的layer；dropdown按钮的mouse enter和exit事件对应layerIndex参数应为1

8. UIManager: MonoSingleton

   根据配置文件给对应的UI元素添加子项和相应的事件(目前包括裁剪的dropdown和模型分块选择的multi-select dropdown)
   
9. EnterExitOutline:Mono

   监听 DropDown 鼠标进入退出。鼠标进入某个 toggle ，则该 toggle 对应的所有模型 outline 。

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

# TODO

完成的项用~~删除线~~划掉

- TimerManager(MonoSingleton): 读取每个更新器的更新时间，维护一个全局计时器，到特定时间调用相应的更新器
- ThermocoupleUpdater(Singleton): 处理所有热电偶的更新
- TuyereUpdater(Singleton): 处理所有关于风口的更新
- HeatmapUpdater(Singleton): 处理热力图的更新 (ssb)
- 热电偶按钮的UI生成 (ss)
- SelectionManager的补全，包括鼠标点选热电偶显示对应信息(ssb)
- 风口标签显示，最好UI跟踪，或者3D Billboard效果实现，标签位置以风口水平高度对齐，会有相互遮挡
- 每个模型使用不同材质(需要裁剪的材质放到Resources/ClippingMaterials下)
- 不同区域的模型生成在同一个空父物体下 (ss)

