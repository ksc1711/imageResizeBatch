using ContentsManager.Model;
using Newtonsoft.Json;
using NLog;
using Reservasi.CM.Common;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using DirectShowLib;

namespace ContentsManager.Biz
{
    class CMgrBiz
    {
        public string DirTree { get; set; }

        public CMgrBiz()
        {
            DirTree = string.Empty;
        }

        public void SetVideoInfo(ContentsInfo _ci, System.IO.FileInfo fi)
        {
            //미디어 정보(플레이시간/ width/ height)
            FilterGraph graphFilter = new FilterGraph();
            IGraphBuilder graphBuilder;
            IMediaPosition mediaPos;
            double length = 0.0;
            int Height, Width = 0;

            graphBuilder = (IGraphBuilder)graphFilter;
            graphBuilder.RenderFile(fi.FullName, null);
            mediaPos = (IMediaPosition)graphBuilder;
            mediaPos.get_Duration(out length);

            IVideoWindow info;
            info = (IVideoWindow)graphBuilder;
            
            info.get_Height(out Height);
            info.get_Width(out Width);
            

            _ci.FileInfo.Add(new ContentsManager.Model.FileInfo()
            {
                Name = fi.Name,
                Size = fi.Length,
                Form = fi.Extension,
                Registrant = "",
                RegistrantDate = fi.CreationTime,
                Width = Width,
                Height = Height,
                Duration = length
            });
        }

        public void SetImageInfo(StringBuilder _sb, ContentsInfo _ci, string file, System.IO.FileInfo fi)
        {
            Image ImageInfo = Image.FromFile(file);

            if (fi == null || ImageInfo == null)
            {
                // 실패로그남기기
                var logger = new Logging("Fail");
                logger.Error(_sb.AppendFormat("File Error :{0}", file).ToString());
                _sb.Clear();
            }

            _ci.FileInfo.Add(new ContentsManager.Model.FileInfo()
            {
                Name = fi.Name,
                Height = ImageInfo.Height,
                Width = ImageInfo.Width,
                Size = fi.Length,
                Form = fi.Extension,
                Registrant = "",
                RegistrantDate = fi.CreationTime
            });

            ImageSave.ImageSave.ImageResize(_sb, fi, ImageInfo);
        }

        public string CreateRedisKey(DirectoryInfo dirInfo)
        {
            DirectoryInfo ParentDir = dirInfo;
            DirTree = string.Empty;
            DirTree = dirInfo.Name;

            while (true)
            {
                if (ParentDir.Parent != null)
                {
                    DirTree = DirTree.Insert(0, ParentDir.Parent.ToString() + " | ");
                    ParentDir = ParentDir.Parent;
                }
                else
                {
                    break;
                }
            }

            return DirTree;
        }

        public void SetRedis(string _key, ContentsInfo _ci)
        {
            //레디스 셋팅(메인 폴더별로)
            var redisUrl = ConfigurationManager.AppSettings["RedisUrl"];
            var redisOn = ConfigurationManager.AppSettings["RedisOn"];
            //var lifeTime = ConfigurationManager.AppSettings["lifeTime"];
            //var objRedis = new RedisBiz(redisUrl, Convert.ToInt32(lifeTime), redisOn);
            var objRedis = new RedisBiz(redisUrl, redisOn);
            string json = JsonConvert.SerializeObject(_ci);
            objRedis.SetRedis(_key, json);
        }

        public void WalkDirectoryTree(StringBuilder _sb, DirectoryInfo _root, ContentsInfo _ci)
        {
            try
            {
                string RedisKey = string.Empty;
                bool isVideo = false;
                DirectoryInfo[] subDirs = null;

                // 현재 디랙토리의 하위 디랙토리 목록을 가져옴
                subDirs = _root.GetDirectories();

                foreach (DirectoryInfo dirInfo in subDirs)
                {
                    Console.WriteLine(dirInfo.FullName); // 출력부분

                    string SearchPatterns = ConfigurationManager.AppSettings["SearchPatterns"].ToString();

                    string[] files = SearchPatterns
                                             .Split('|')
                                             .SelectMany(searchPattern => Directory.GetFiles(dirInfo.FullName, searchPattern)).ToArray();

                    if (files.Count() != 0)
                    {
                        RedisKey = CreateRedisKey(dirInfo);
                    }

                    string[] VideoType = ConfigurationManager.AppSettings["VideoSearchPatterns"].ToString().Split('|');

                    foreach (var file in files)
                    {
                        // 파일정보
                        System.IO.FileInfo fi = new System.IO.FileInfo(file);

                        foreach (var item in VideoType)
                        {
                            if (item.Contains(fi.Extension))
                                isVideo = true;
                        }

                        if (isVideo)
                        {
                            SetVideoInfo(_ci, fi);
                            isVideo = false;
                        }
                        else
                        {
                            SetImageInfo(_sb, _ci, file, fi);
                        }
                    }

                    if (files.Count() != 0)
                    {
                        SetRedis(RedisKey, _ci);
                        _ci.Clear();
                    }

                    // 재귀 호출을 통해 하위 디랙토리의 하위디랙토리를 탐색
                    WalkDirectoryTree(_sb, dirInfo, _ci);

                }
            }
            catch (Exception e)
            {

            }
        }

        public void FileBiz()
        {
            string path = ConfigurationManager.AppSettings["path"].ToString();

            StringBuilder sb = new StringBuilder();

            // 카피해올 폴더의 정보를 넘겨서 함수 실행
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                ContentsInfo Ci = new ContentsInfo();
                WalkDirectoryTree(sb, di, Ci);
            }
        }
    }
}
