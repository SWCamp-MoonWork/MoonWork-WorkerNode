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
        public long Id { get; set; }
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
        public string ProcessStart(WorkerHostModel mod)
        {
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
                Console.WriteLine(r);
                var ob = System.Text.Json.JsonSerializer.Deserialize<List<JobUserNameModel>>(r);
                string workFlowBlob = "";
                foreach (var a in ob)
                {
                    if (a.JobId == mod.JobId)
                    {
                        workFlowBlob = a.WorkflowBlob;
                    }
                }

                Console.WriteLine(workFlowBlob);

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
                var psi = CreateProcessStartInfo(@"/usr/bin/python3", example);

                var erros = "";
                var results = "";
                DateTime start = DateTime.Now;
                long jobid = mod.JobId;
                Net.RAPI.State10(jobid);
                try
                {
                    using (var process = Process.Start(psi))
                    {
                        erros = process.StandardError.ReadToEnd();
                        results = process.StandardOutput.ReadToEnd();
                    }
                    Console.WriteLine(results);
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "10", mod.JobId, mod.HostId, mod.WorkflowName, results);
                    Console.WriteLine("����");
                }
                catch (Exception ex)
                {
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "11", mod.JobId, mod.HostId, mod.WorkflowName, erros);
                    //���� ����/���н� ���ִ� API
                    Console.WriteLine("����" + ex.ToString());
                }
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
                    //���� ����/���н� ���ִ� API
                    Console.WriteLine("����");
                }
                catch (Exception e)
                {
                    //���� ����/���н� ���ִ� API
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "11", mod.JobId, mod.HostId, mod.WorkflowName, errors);
                    Console.WriteLine("����");
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
                    //���� ����/���н� ���ִ� API
                    Console.WriteLine("����");
                }
                catch (Exception e)
                {
                    //���� ����/���н� ���ִ� API
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "11", mod.JobId, mod.HostId, mod.WorkflowName, errors);
                    Console.WriteLine("����");
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
                    //���� ����/���н� ���ִ� API
                    Console.WriteLine("����");
                }
                catch (Exception e)
                {
                    //���� ����/���н� ���ִ� API
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "11", mod.JobId, mod.HostId, mod.WorkflowName, errors);
                    Console.WriteLine("����");
                }
            }
            //blob ���� ���� .dll ���Ͻ���
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
                    //���� ����/���н� ���ִ� API
                    Console.WriteLine("����");

                }
                catch (Exception e)
                {
                    //���� ����/���н� ���ִ� API
                    Console.WriteLine("����");
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "11", mod.JobId, mod.HostId, mod.WorkflowName, erros);

                }
            }
            //.bash/.sh ���� ����
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
                        //���� ����/���н� ���ִ� API
                        Console.WriteLine("����");
                        results = process.StandardOutput.ReadToEnd();
                        erros = process.StandardError.ReadToEnd();
                    }
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "10", mod.JobId, mod.HostId, mod.WorkflowName, results);
                }
                catch (Exception e)
                {
                    //���� ����/���н� ���ִ� API
                    DateTime end = DateTime.Now;
                    Net.RAPI.SendAPI(start, end, "11", mod.JobId, mod.HostId, mod.WorkflowName, erros);
                    Console.WriteLine("����");
                }
            }
            else
            {
                Console.WriteLine("This extension is not supported");
            }
            return "����";
        }

        private ProcessStartInfo CreateProcessStartInfo(string FileName, string Arguments)
        {
            var psi = new ProcessStartInfo();
            psi.UseShellExecute = false;
            //Worker �� �θ� ���� blob ���� �ڽ��� ����ɶ� ������� ������ �θ� Ŭ������ ������ �ð��̳� �� �����ϴ� ��
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            psi.FileName = FileName;
            psi.Arguments = Arguments;

            return psi;
        }

    }
}