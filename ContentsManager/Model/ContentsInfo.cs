using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentsManager.Model
{
    class ContentsInfo
    {
        // 데이터 모델
        public List<FileInfo> FileInfo { get; set; }

        public ContentsInfo()
        {
            FileInfo = new List<FileInfo>();
        }

        public void Clear()
        {
            FileInfo.Clear();
        }
    }

    class FileInfo
    {
        /// <summary>
        /// 등록자
        /// </summary>
        public string Registrant { get; set; }

        /// <summary>
        /// 등록일(파일생성일자)
        /// </summary>
        public DateTime RegistrantDate { get; set; }
        
        public string Name { get; set; }

        public long Size { get; set; }
        
        //확장자
        public string Form { get; set; }
       
        public int Height { get; set; }

        public int Width { get; set; }

        //재생시간 (동영상만 해당됨) 
        public double? Duration { get; set; }
    }
}
