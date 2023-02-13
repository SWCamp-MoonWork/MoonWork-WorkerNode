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
    public class RunAssign
    {
        public string? WorkFlowName { get; set; }
        public long? JobId { get; set; }
        public long? HostId { get; set; }
        public long? RunId { get; set; }
        public DateTime? StartDT { get; set; }
        public DateTime? EndDT { get; set; }
        public string? state { get; set; }
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
        static long HostId = 2;
        static long RunId = 0;
        [HttpPost("2")]
        //CompareExtension을 실행시키고 바로 리턴
        public async Task<string> Main(WorkerHostModel mod)
        {
            string results = DateTime.Now.ToString();

            //Task Start = CompareExtension(mod);
            Task t1 = Task.Run(() => { CompareExtension(mod); });

            return "1번 Host 성공시간 : " + results;
        }
        public static async void CompareExtension(WorkerHostModel mod)
        {
            //RunId를 가지고 오는 POST API
            RunId = Net.RAPI.SetRunTable(mod.JobId, HostId);
            Console.WriteLine(RunId);

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
                //Base64로 바이트 배열로 변환하고 파일로 다시 변환
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
                //실행 정보를 저장하는 메소드
                var psi = CreateProcessStartInfo(@"/usr/bin/python3", example);
                long jobid = mod.JobId;
                //비동기화 프로세스 start
                Task t2 = Task.Run(() => { ProcessStart(psi, jobid, HostId, mod.WorkflowName); });
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
                long jobid = mod.JobId;
                Task t2 = Task.Run(() => { ProcessStart(psi2, jobid, HostId, mod.WorkflowName); });
            }
            else if (fileExtension == ".zip")
            {
                string cmd = "unzip -o blob/" + filename + " -d ./blob";
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
                long jobid = mod.JobId;
                Task t2 = Task.Run(() => { ProcessStart(psi2, jobid, HostId, mod.WorkflowName); });
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
                long jobid = mod.JobId;
                Task t2 = Task.Run(() => { ProcessStart(psi2, jobid, HostId, mod.WorkflowName); });
            }
            //blob 파일 내에 .dll 파일실행 
            else if (fileExtension == ".dll")
            {
                var psi = CreateProcessStartInfo(@"dotnet", example);

                var erros = "";
                var results = "";
                DateTime start = DateTime.Now;
                long jobid = mod.JobId;
                Task t2 = Task.Run(() => { ProcessStart(psi, jobid, HostId, mod.WorkflowName); });
            }
            //.bash/.sh 파일 실행
            else if (fileExtension == ".sh")
            {
                var psi = CreateProcessStartInfo(@"sh", example);
                long jobid = mod.JobId;
                Task t2 = Task.Run(() => { ProcessStart(psi, jobid, HostId, mod.WorkflowName); });
            }
            else
            {
                Console.WriteLine("This extension is not supported");
            }
        }
        private static async void ProcessStart(ProcessStartInfo psi, long jobid, long HostId, string WorkflowName)
        {
            Console.WriteLine("Job이 실행되는 메소드로 왔습니다..");
            var erros = "";
            var results = "";
            DateTime start = DateTime.Now;
            //Job table의 State를 10으로 수정하여 작업이 시작됬음을 알리는 메소드
            string StateON = Net.RAPI.StateON(jobid);
            string PutStart = Net.RAPI.PutStartDT(RunId, start, WorkflowName, "01");
            try
            {
                using (var process = Process.Start(psi))
                {
                    erros = process.StandardError.ReadToEnd();
                    results = process.StandardOutput.ReadToEnd();
                }
                DateTime end = DateTime.Now;
                Console.WriteLine(results);
                //Job table의 State를 11으로 수정하여 작업이 종료됬음을 알리는 메소드
                string StateOFF = Net.RAPI.StateOFF(jobid);
                string PutEnd = Net.RAPI.PutEndDT(RunId, WorkflowName, start, end, results, "10");
                //성공시 Run table 에 정보 입력
                Console.WriteLine("성공");
            }
            catch (Exception ex)
            {
                DateTime end = DateTime.Now;
                //실행 성공/실패시 쏴주는 API
                //Job table의 State를 11으로 수정하여 작업이 종료됬음을 알리는 메소드
                string StateOFF = Net.RAPI.StateOFF(jobid);
                string PutEnd = Net.RAPI.PutEndDT(RunId, WorkflowName, start, end, results, "10");
                //실패시 Run 테이블에 입력
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
            psi.FileName = FileName;
            psi.Arguments = Arguments;
            psi.CreateNoWindow = true;
            return psi;
        }
    }
}