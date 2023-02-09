using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Text;
using WorkHost2.Controllers.Net;

namespace WorkHost2.Controllers
{
    public class jobid
    {
        public long JobId { get; set; }
    }
    public class RunData
    {
        public string? WorkFlowName { get; set; }
        public DateTime? StartDT { get; set; }
        public DateTime? EndDT { get; set; }
        public string? State { get; set; }
        public long? JobId { get; set; }
        public long? HostId { get; set; }
        public string? ResultData { get; set; }
    }
    public class JobUserNameModel
    {
        public long? JobId { set; get; }
        public string? JobName { set; get; }
        public Boolean? IsUse { set; get; }
        public string? WorkflowName { set; get; }
        public string? WorkflowBlob { set; get; }
        public string? Note { set; get; }
        public DateTime? SaveDate { set; get; }
        public long? UserId { set; get; }
        public string? UserName { set; get; }
    }

    [ApiController]
    [Route("worker")]
    public class WorkerHost
    {
        [HttpPost("1")]
        public async Task<string> Main(WorkerHostModel mod)
        {
            Task Start = CompareExtension(mod);
            string results = DateTime.Now.ToString();
            return "성공시간 " + results;
        }
        public static async Task CompareExtension(WorkerHostModel mod)
        {
            //WorkFlowname을 받아와서 그 파일이 있는지 없는지 검사하는 파일
            string example = "blob/" + mod.WorkflowName;

            if (File.Exists(example))
            {
                Console.WriteLine(example + " is exists... now running");
            }
            else
            {
                Console.WriteLine(example + " is not exists... downloding and run");
                //byte[] 에서 파일로 변환
                var jobListtest = RAPI.JobList();
                string r = JsonConvert.SerializeObject(jobListtest);
                r = r.Replace("\r\n", "").Replace("\n", "").Replace(@"\", "");

                if (r.Substring(0, 2) == @"""[")
                    r = r.Substring(1, r.Length - 1);
                if (r.Substring(r.Length - 2, 2) == @"]""")
                    r = r.Substring(0, r.Length - 1);
                var ob = System.Text.Json.JsonSerializer.Deserialize<List<JobUserNameModel>>(r);
                string workFlowBlob = "";
                foreach (var a in ob)
                {
                    if (a.JobId == mod.JobId)
                    {
                        workFlowBlob = a.WorkflowBlob;
                    }
                }
                byte[] bytes = System.Convert.FromBase64String(workFlowBlob);
                Console.WriteLine(string.Join("", bytes));
                File.WriteAllBytes(example, bytes);
            }

            //파일의 경로에서 확장자 및 파일이름+확장자 추출
            string filepath = example;
            string fileExtension = System.IO.Path.GetExtension(filepath);
            string filenameOnly = System.IO.Path.GetFileNameWithoutExtension(filepath);
            string filename = System.IO.Path.GetFileName(filepath);
            //zip 파일 압축 해제 

            //확장자에 따른 runtime 실행하는 조건식
            //파이썬 실행 
            if (fileExtension == ".py")
            {
                var psi = CreateProcessStartInfo(@"/usr/bin/python3", example);
                long jobid = mod.JobId;
                ProcessStart(psi, jobid, mod.HostId, mod.WorkflowName);
            }
            //.gz 파일을 받아 압축을 풀고 dll 파일을 실행
            else if (fileExtension == ".gz")
            {

                string cmd = "tar -zxvf blob/" + filename + " -C ./blob";
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.UseShellExecute = false;
                psi.FileName = "bash";
                psi.Arguments = "-c \"" + cmd + "\"";

                try
                {
                    Process child = Process.Start(psi);
                    child.WaitForExit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                string[] path = Directory.GetFiles(@"./blob", "*.dll", SearchOption.AllDirectories);

                string dllFilename = path[0];
                string dllFileFullname = System.IO.Path.GetFileName(path[0]);
                Console.WriteLine(path[0]);
                Console.WriteLine(dllFileFullname);

                var psi2 = CreateProcessStartInfo(@"dotnet", "./blob/" + dllFileFullname);
                var errors = "";
                var results = "";

                DateTime start = DateTime.Now;
                try
                {
                    using (var process = Process.Start(psi2))
                    {
                        errors = process.StandardError.ReadToEnd();
                        results = process.StandardOutput.ReadToEnd();
                    }
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "10", mod.JobId, mod.HostId, mod.WorkflowName, results);
                    Console.WriteLine(errors);
                    Console.WriteLine(results);
                    //실행 성공/실패시 쏴주는 API
                    Console.WriteLine("성공");
                }
                catch (Exception e)
                {
                    //실행 성공/실패시 쏴주는 API
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "11", mod.JobId, mod.HostId, mod.WorkflowName, errors);
                    Console.WriteLine("실패");
                }
            }
            else if (fileExtension == ".zip")
            {
                string cmd = "unzip blob/" + filename + " -d ./blob";
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.UseShellExecute = false;
                psi.FileName = "bash";
                psi.Arguments = "-c \"" + cmd + "\"";

                try
                {
                    Process child = Process.Start(psi);
                    child.WaitForExit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                string[] path = Directory.GetFiles(@"./blob", "*.dll", SearchOption.AllDirectories);

                string dllFilename = path[0];
                string dllFileFullname = System.IO.Path.GetFileName(path[0]);
                Console.WriteLine(path[0]);
                Console.WriteLine(dllFileFullname);

                var psi2 = CreateProcessStartInfo(@"dotnet", "./blob/" + dllFileFullname);
                var errors = "";
                var results = "";

                DateTime start = DateTime.Now;
                try
                {
                    using (var process = Process.Start(psi2))
                    {
                        errors = process.StandardError.ReadToEnd();
                        results = process.StandardOutput.ReadToEnd();
                    }
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "10", mod.JobId, mod.HostId, mod.WorkflowName, results);
                    Console.WriteLine(errors);
                    Console.WriteLine(results);
                    //실행 성공/실패시 쏴주는 API
                    Console.WriteLine("성공");
                }
                catch (Exception e)
                {
                    //실행 성공/실패시 쏴주는 API
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "11", mod.JobId, mod.HostId, mod.WorkflowName, errors);
                    Console.WriteLine("실패");
                }
            }
            else if (fileExtension == ".7z")
            {
                string cmd = "7za x blob/" + filename + " -o./blob";
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.UseShellExecute = false;
                psi.FileName = "bash";
                psi.Arguments = "-c \"" + cmd + "\"";

                try
                {
                    Process child = Process.Start(psi);
                    child.WaitForExit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                string[] path = Directory.GetFiles(@"./blob", "*.dll", SearchOption.AllDirectories);

                string dllFilename = path[0];
                string dllFileFullname = System.IO.Path.GetFileName(path[0]);
                Console.WriteLine(path[0]);
                Console.WriteLine(dllFileFullname);

                var psi2 = CreateProcessStartInfo(@"dotnet", "./blob/" + dllFileFullname);
                var errors = "";
                var results = "";

                DateTime start = DateTime.Now;
                try
                {
                    using (var process = Process.Start(psi2))
                    {
                        errors = process.StandardError.ReadToEnd();
                        results = process.StandardOutput.ReadToEnd();
                    }
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "10", mod.JobId, mod.HostId, mod.WorkflowName, results);
                    Console.WriteLine(errors);
                    Console.WriteLine(results);
                    //실행 성공/실패시 쏴주는 API
                    Console.WriteLine("성공");
                }
                catch (Exception e)
                {
                    //실행 성공/실패시 쏴주는 API
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "11", mod.JobId, mod.HostId, mod.WorkflowName, errors);
                    Console.WriteLine("실패");
                }
            }
            //blob 파일 내에 .dll 파일실행 
            else if (fileExtension == ".dll")
            {
                var psi = CreateProcessStartInfo(@"dotnet", example);

                var erros = "";
                var results = "";
                DateTime start = DateTime.Now;
                try
                {
                    using (var process = Process.Start(psi))
                    {

                        erros = process.StandardError.ReadToEnd();
                        results = process.StandardOutput.ReadToEnd();
                    }
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "10", mod.JobId, mod.HostId, mod.WorkflowName, results);
                    Console.WriteLine(erros);
                    Console.WriteLine(results);
                    //실행 성공/실패시 쏴주는 API
                    Console.WriteLine("성공");

                }
                catch (Exception e)
                {
                    //실행 성공/실패시 쏴주는 API
                    Console.WriteLine("실패");
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "11", mod.JobId, mod.HostId, mod.WorkflowName, erros);

                }
            }
            //.bash/.sh 파일 실행
            else if (fileExtension == ".sh")
            {
                var psi = CreateProcessStartInfo("sh", example);

                var erros = "";
                var results = "";
                DateTime start = DateTime.Now;
                try
                {
                    using (var process = Process.Start(psi))
                    {
                        //실행 성공/실패시 쏴주는 API
                        Console.WriteLine("성공");
                        results = process.StandardOutput.ReadToEnd();
                        erros = process.StandardError.ReadToEnd();
                    }
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "10", mod.JobId, mod.HostId, mod.WorkflowName, results);
                }
                catch (Exception e)
                {
                    //실행 성공/실패시 쏴주는 API
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "11", mod.JobId, mod.HostId, mod.WorkflowName, erros);
                    Console.WriteLine("실패");
                }
            }
            else
            {
                Console.WriteLine("This extension is not supported");
            }
        }
        private static async Task ProcessStart(ProcessStartInfo psi, long jobid, long HostId, string WorkflowName)
        {
            var erros = "";
            var results = "";
            DateTime start = DateTime.Now;
            string StateON = Net.RAPI.StateON(jobid);
            try
            {
                using (var process = Process.Start(psi))
                {
                    erros = process.StandardError.ReadToEnd();
                    results = process.StandardOutput.ReadToEnd();
                }
                DateTime end = DateTime.Now;
                string StateOFF = Net.RAPI.StateOFF(jobid);
                Net.RAPI.SendAPI(start, end, "10", jobid, HostId, WorkflowName, results);
                Console.WriteLine("성공");
            }
            catch (Exception ex)
            {
                DateTime end = DateTime.Now;
                Net.RAPI.SendAPI(start, end, "11", jobid, HostId, WorkflowName, erros);
                //실행 성공/실패시 쏴주는 API
                Console.WriteLine("실패" + ex.ToString());
            }
        }
        private static ProcessStartInfo CreateProcessStartInfo(string FileName, string Arguments)
        {
            var psi = new ProcessStartInfo();
            psi.UseShellExecute = false;
            //Worker 인 부모 에서 blob 안의 자식이 실행될때 결과값과 에러를 부모 클래스로 가지고 올것이냐 를 결정하는 것
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            psi.FileName = FileName;
            psi.Arguments = Arguments;

            return psi;
        }

    }
}