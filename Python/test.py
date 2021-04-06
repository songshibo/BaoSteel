import numpy as np
import taichi as ti
from heatmap import Heatmap

ti.init(arch=ti.gpu, excepthook=True, kernel_profiler=True)

# for test
yh = 46.8
mint = 10
maxt = 330
p = 4
s = 10
w = 128
h = 128 * 4
yfactor = 20

seeds = np.array(np.random.rand(400, 3), dtype=np.float32)
# 对随机数据预处理，如果是从数据库中读出，则无需这一步
for i in range(seeds.shape[0]):
    seeds[i][0] *= 360  # 角度从0-1变换到0-360
    seeds[i][2] = (maxt - mint) * seeds[i][2] + mint  # 温度从0-1变换到minT到maxT

# 这里乘以yh * yfactor是由于随机生成的seeds值都在0-1之间，数据库中的数据则是在0-yh之间，因此只要乘以yfactor就行
ht = Heatmap(w, h, seeds.shape[0])
ht.assign(seeds, yh * yfactor, p, s, 1, mint, maxt)
# yh * yfactor 同理
# ht.generate()
ti.kernel_profiler_print()

# 可视化结果，实际运行直接调用ht.save(${path})即可
ht.show_gui()
