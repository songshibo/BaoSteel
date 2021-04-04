import numpy as np
import taichi as ti

ti.init(arch=ti.gpu)


@ti.data_oriented
class Heatmap:
    # xRes/yRes: 贴图分辨率
    # raw_data: 从数据库获得的数据 [[angle, height, temperature], [], [] ...]
    # yFactor: 高度缩放系数
    # minT,maxT: 温度上下限
    def __init__(self, xRes, yRes, raw_data, yFactor):
        self.w = xRes
        self.h = yRes
        self.size = raw_data.shape[0]
        self.yFactor = yFactor
        self.pixels = ti.field(ti.f32, (self.w, self.h))
        # parameter: 每个向量中分量的数目，数据类型，向量的形状
        self.data = ti.Vector.field(
            raw_data.shape[1], ti.f32, raw_data.shape[0])
        for i in range(raw_data.shape[0]):
            raw_data[i][1] *= yFactor
        self.data.from_numpy(raw_data)
        print("resolution:[", self.w, ",", self.h, "]")
        print("thermocouple Number:", self.size)
        print("height scale factor:", self.yFactor)

    # power/smoothin : IDW插值的参数
    # scaledHeight: 最大的实际高度 * 缩放系数
    # yFactor: 高度缩放系数
    # minT, maxT: 最小/大温度
    @ti.kernel
    def generate(self, power: ti.f32, smoothin: ti.f32, scaledHeight: ti.f32, minT: ti.f32, maxT: ti.f32):
        for idx, idy in self.pixels:
            x = idx * 360.0 / self.w
            y = idy * scaledHeight / self.h

            nominator = 0.0
            denominator = 0.0

            for i in range(self.size):
                xdiff = ti.abs(x - self.data[i].x)
                xdiff = 360 - xdiff if xdiff > 180 else xdiff
                ydiff = y - self.data[i].y
                inv_dist = 1 / pow(ti.sqrt(xdiff * xdiff + ydiff *
                                           ydiff + smoothin * smoothin), power)
                nominator += self.data[i].z * inv_dist
                denominator += 1 * inv_dist

            self.pixels[idx, idy] = (
                nominator / denominator - minT) / (maxT - minT)

    # Save result to path
    def save(self, path):
        ti.imwrite(self.pixels, path)
        print("Image saved!")

    # debug data & result with gui
    def show_gui(self):
        gui = ti.GUI("heatmap", (self.w, self.h))
        while gui.running:
            for e in gui.get_events(ti.GUI.PRESS):
                if e.key == ti.GUI.SPACE:
                    self.save("./result.png")

            gui.set_image(self.pixels)
            gui.circles(self.data.to_numpy()[..., 0:2] / [360, self.yFactor],
                        color=0xffaa77, radius=2)
            gui.show()
