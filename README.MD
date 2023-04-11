# NeteaseMusicDownload
获取网易云无损音乐直链下载地址，没有无损音质就默认获取最高音质

## 序言
本项目仅供交流学习，尊重音乐版权，请勿私自搭建第三方接口供他人使用或用于非法盈利。

## 测试环境
Win11 + Visual Studio 2022 + .Net 6.0

## 使用方法
1.补充Cookie内容，点击运行即可。

2.请自行百度如何使用Fiddler抓包网易云音乐电脑客户端登录之后的Cookie。

3.Cookie示例内容如下：
```C#
private static readonly string Cookie = "os=pc; " +
            "deviceId=xxxxxx; " +
            "osver=xxxxxx; " +
            "NMTID=xxxxx; channel=xxxxxx; WEVNSM=xxxxxx; mode=xxxxxx; " +
            "ntes_kaola_ad=xxxxxx; __remember_me=xxxxxx; " +
            "MUSIC_U=xxxxxxx; " +
            "__csrf=xxxx; appver=xxxxxx; WNMCID=xxxxxx";
```
## 最后
1.接口有可能会失效，且行且珍惜，以后有机会随缘更新。

2.代码编写粗糙，凑合着看。

3.点到为止，json解析和下载功能各位有缘人自行实现吧。

4.再会！

## License
[The MIT License (MIT)](https://github.com/N9st/NeteaseMusicDownload/blob/master/LICENSE.txt)

