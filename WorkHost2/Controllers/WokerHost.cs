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
        //CompareExtension�� �����Ű�� �ٷ� ����
        public async Task<string> Main(WorkerHostModel mod)
        {
            string results = DateTime.Now.ToString();

            //Task Start = CompareExtension(mod);
            Task t1 = Task.Run(() => { CompareExtension(mod); });

            return "1�� Host �����ð� : " + results;
        }
        public static async void CompareExtension(WorkerHostModel mod)
        {
            //RunId�� ������ ���� POST API
            RunId = Net.RAPI.SetRunTable(mod.JobId, HostId);
            Console.WriteLine(RunId);

            //WorkFlowname�� �޾ƿͼ� �� ������ �ִ��� ������ �˻��ϴ� ����
            string example = "blob/" + mod.WorkflowName;

            if (File.Exists(example))
            {
                Console.WriteLine(example + " is exists... now running");
            }
            else
            {
                Console.WriteLine(example + " is not exists... downloding and run");
                //byte[] ���� ���Ϸ� ��ȯ
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
                //Base64�� ����Ʈ �迭�� ��ȯ�ϰ� ���Ϸ� �ٽ� ��ȯ
                byte[] bytes = System.Convert.FromBase64String(workFlowBlob);
                Console.WriteLine(string.Join("", bytes));
                File.WriteAllBytes(example, bytes);
            }

            //������ ��ο��� Ȯ���� �� �����̸�+Ȯ���� ����
            string filepath = example;
            string fileExtension = System.IO.Path.GetExtension(filepath);
            string filenameOnly = System.IO.Path.GetFileNameWithoutExtension(filepath);
            string filename = System.IO.Path.GetFileName(filepath);
            //zip ���� ���� ���� 

            //Ȯ���ڿ� ���� runtime �����ϴ� ���ǽ�
            //���̽� ���� 
            if (fileExtension == ".py")
            {
                //���� ������ �����ϴ� �޼ҵ�
                var psi = CreateProcessStartInfo(@"/usr/bin/python3", example);
                long jobid = mod.JobId;
                //�񵿱�ȭ ���μ��� start
                Task t2 = Task.Run(() => { ProcessStart(psi, jobid, HostId, mod.WorkflowName); });
            }
            //.gz ������ �޾� ������ Ǯ�� dll ������ ����
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
            //blob ���� ���� .dll ���Ͻ��� 
            else if (fileExtension == ".dll")
            {
                var psi = CreateProcessStartInfo(@"dotnet", example);

                var erros = "";
                var results = "";
                DateTime start = DateTime.Now;
                long jobid = mod.JobId;
                Task t2 = Task.Run(() => { ProcessStart(psi, jobid, HostId, mod.WorkflowName); });
            }
            //.bash/.sh ���� ����
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
            Console.WriteLine("Job�� ����Ǵ� �޼ҵ�� �Խ��ϴ�..");
            var erros = "";
            var results = "";
            DateTime start = DateTime.Now;
            //Job table�� State�� 10���� �����Ͽ� �۾��� ���ۉ����� �˸��� �޼ҵ�
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
                //Job table�� State�� 11���� �����Ͽ� �۾��� ��������� �˸��� �޼ҵ�
                string StateOFF = Net.RAPI.StateOFF(jobid);
                string PutEnd = Net.RAPI.PutEndDT(RunId, WorkflowName, start, end, results, "10");
                //������ Run table �� ���� �Է�
                Console.WriteLine("����");
            }
            catch (Exception ex)
            {
                DateTime end = DateTime.Now;
                //���� ����/���н� ���ִ� API
                //Job table�� State�� 11���� �����Ͽ� �۾��� ��������� �˸��� �޼ҵ�
                string StateOFF = Net.RAPI.StateOFF(jobid);
                string PutEnd = Net.RAPI.PutEndDT(RunId, WorkflowName, start, end, results, "10");
                //���н� Run ���̺� �Է�
                Console.WriteLine("����" + ex.ToString());
            }
        }
        private static ProcessStartInfo CreateProcessStartInfo(string FileName, string Arguments)
        {
            var psi = new ProcessStartInfo();
            psi.UseShellExecute = false;
            //Worker �� �θ� ���� blob ���� �ڽ��� ����ɶ� ������� ������ �θ� Ŭ������ ������ �ð��̳� �� �����ϴ� ��
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.FileName = FileName;
            psi.Arguments = Arguments;
            psi.CreateNoWindow = true;
            return psi;
        }
    }
}