using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using ContentsManager.Model;
using System.Configuration;
using NLog;

using Reservasi.CM.Common;
using Newtonsoft.Json;
using System.Windows.Forms;
using ContentsManager.Biz;


namespace ContentsManager
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CMgrBiz Biz = new CMgrBiz();
                Biz.FileBiz();
            }
            catch(Exception e)
            {
                var logger = new Logging("Error");
                logger.Error(e);
            }
        }
    }
}
