using System.Security.Cryptography;
using System.Text;

namespace NeteaseMusicDownload
{
    internal class Program
    {
        private static readonly HttpClient HttpClient = new();
        private static readonly string PostUrl = "https://interface.music.163.com/eapi/song/enhance/download/url/v1";
        private static readonly string UserAgent = @"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Safari/537.36 Chrome/91.0.4472.164 NeteaseMusicDesktop/2.10.8.200945";
        //抓包网易云电脑客户端用户登录之后的Cookie
        private static readonly string Cookie = string.Empty;
        //Aes解密的key
        private static readonly string AesKey = "e82ckenh8dichen8";
        static async Task Main(string[] args)
        {
            if(Cookie == string.Empty)
            {
                Console.WriteLine("请输入Cookie！");
                return;
            }
            //三首歌分别是起风了、哪里都是你、Something Just Like This
            //简单演示一下，写的粗糙
            Dictionary<string, string> songDict = new()
            {
                {"起风了","1330348068" },
                {"哪里都是你","488249475" },
                {"Something Just Like This","461347998"},
            };
            foreach (KeyValuePair<string, string> song in songDict)
            {
                Console.WriteLine(song.Key);
                await Start(song.Value);
            }
        }
        //开始获取
        private async static Task Start(string songId)
        { 
            //即将提交的数据
            Dictionary<string, string> postDatas = ReturnParams(songId);
            //添加请求头和cookie
            HttpClient.DefaultRequestHeaders.Add("Cookie", Cookie);
            HttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            //post提交数据，需对数据进行url编码
            HttpResponseMessage httpResponseMessage = await HttpClient.PostAsync(PostUrl, new FormUrlEncodedContent(postDatas));
            byte[] returnResult = await httpResponseMessage.Content.ReadAsByteArrayAsync();
            //解密并打印返回的内容
            Console.WriteLine(AesDecrypt(returnResult));
        }
        //构造params参数的内容，返回一个字典
        private static Dictionary<string, string> ReturnParams(string songId)
        {
            #region 随机八位整数
            Random random = new();
            char[] chars = "0123456789".ToCharArray();
            string uuid = string.Empty;
            for (int i = 0; i < 8; i++)
            {
                uuid += chars[random.Next(0, 10)];
            }
            string onlyUuid = uuid;
            //Console.WriteLine(onlyUuid);
            #endregion

            //歌曲信息（核心）
            string musicData = "{ \"id\": \"" +
                $"{songId}" +
                "\", \"level\": \"hires\", \"effects\": \"[null]\", \"e_r\": true, \"" +
                "header\": \"{\\\"os\\\":\\\"pc\\\",\\\"appver\\\":\\\"2.10.8.200945\\\",\\\"" +
                "deviceId\\\":\\\"E721AA1F0A45438E6EB7A2D87210C4A830A988886C6666888C4\\\",\\\"" +
                "requestId\\\":\\\"" +
                $"{onlyUuid}" +
                "\\\",\\\"" +
                "clientSign\\\":\\\"BC:7E:E8:29:CF:32@@@303041305F373530315F333439315F666888666E" +
                "@@@@@@d6905555-4fb7-43d5-7a7a-612478f2f80f901160524193889aa45d8d4835666888\\\",\\\"" +
                "osver\\\":\\\"Microsoft-Windows-10-Home-China-build-22666-32bit\\\",\\\"" +
                "Nm-GCore-Status\\\":\\\"1\\\"}\" }";
            //Console.WriteLine(musicData);

            //需要MD5加密的明文
            string textEncryptToMD5 = "nobody" +
                "/api/song/enhance/download/url/v1" +
                "use" + musicData + "md5forencrypt";
            //Console.WriteLine(textEncryptToMD5);

            //需要AES加密的明文
            string textEncryptToAes = "/api/song/enhance/download/url/v1" +
                "-36cd479b6b5-" +
                $"{musicData}" +
                "-36cd479b6b5-" +
                $"{MD5Encrypt(textEncryptToMD5)}";
            //Console.WriteLine(textEncryptToAes);

            Dictionary<string, string> paramsDatas = new()
            {
                {"params", AesEncrypt(textEncryptToAes)},
            };
            return paramsDatas;
        }
        //封装AES解密方法，返回UTF8编码后的字符串
        private static string AesDecrypt(byte[] byteContent)
        {
            byte[] byteKey = Encoding.UTF8.GetBytes(AesKey);
            //创建对象
            var aes = Aes.Create();
            //AES加密模式，可以修改，如ECB，默认为CBC
            aes.Mode = CipherMode.ECB;
            //赋值Key
            aes.Key = byteKey;
            //创建解密对象
            var cryptor = aes.CreateDecryptor();
            //开始转换，依次是需解密的内容、偏移量、内容的长度
            byte[] byteDecrypted = cryptor.TransformFinalBlock(byteContent, 0, byteContent.Length);
            //释放资源
            cryptor.Dispose();
            //返回UTF8编码后的字符串
            return Encoding.UTF8.GetString(byteDecrypted);
        }
        //封装AES加密方法，返回十六进制字符串
        private static string AesEncrypt(string content)
        {
            byte[] byteContent = Encoding.UTF8.GetBytes(content);
            byte[] byteKey = Encoding.UTF8.GetBytes(AesKey);
            //创建AES对象
            var aes = Aes.Create();
            //AES加密模式，可以修改，如ECB，默认为CBC
            aes.Mode = CipherMode.ECB;
            //赋值Key
            aes.Key = byteKey;
            //创建加密对象
            var crypto = aes.CreateEncryptor();
            //开始转换，依次是需加密的内容、偏移量、内容的长度
            byte[] byteEncrypted = crypto.TransformFinalBlock(byteContent, 0, byteContent.Length);
            //释放资源
            crypto.Dispose();
            //返回十六进制字符串
            return BitConverter.ToString(byteEncrypted).Replace("-", "");
        }
        //MD5加密，返回小写32位的字符串
        private static string MD5Encrypt(string password)
        {
            //保存md5加密后的结果
            string pwd = "";
            //大写MD5（X2）或者小写MD5（x2）
            string pwdFormat = "x2";
            byte[] temp = Encoding.UTF8.GetBytes(password);
            MD5 md5 = MD5.Create();
            byte[] result = md5.ComputeHash(temp);
            for (int i = 0; i < result.Length; i++)
            {
                pwd += result[i].ToString(pwdFormat);
            }
            return pwd;
        }
    }
}