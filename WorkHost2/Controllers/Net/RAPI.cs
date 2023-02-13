using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace WorkHost2.Controllers.Net
{
    public class RAPI
    {
        public static string JobList()
        {
            string jobList = ignore.JobList;

            string result = string.Empty;
            try
            {
                WebClient client = new WebClient();

                using (Stream data = client.OpenRead(jobList))
                {
                    using (StreamReader reader = new StreamReader(data))
                    {
                        string s = reader.ReadToEnd();
                        result = s;

                        reader.Close();
                        data.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result;

        }
        public static long SetRunTable(long jobs, long HostId)
        {
            string API = ignore.sendAPI;
            string responseText = string.Empty;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(API);
            webRequest.Method = "POST";
            webRequest.Timeout = 30 * 1000;
            webRequest.ContentType = "application/json";

            var obj = new RunAssign
            {
                JobId = jobs,
                HostId = HostId
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            //보낼 데이터를 byteArray로 바꿔준다.
            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            //요청 Data를 쓰는데 사용할 Stream 개체를 가져온다.
            Stream dataStream = webRequest.GetRequestStream();
            //전송...
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //Data를 잘 받았는지 확인하는 response 구간
            //응답 받기
            using (HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse())
            {
                HttpStatusCode status = resp.StatusCode;
                Console.WriteLine("status = " + status);      // status 가 정상일경우 OK가 입력된다.

                // 응답과 관련된 stream을 가져온다.
                Stream respStream = resp.GetResponseStream();
                using (StreamReader streamReader = new StreamReader(respStream))
                {
                    responseText = streamReader.ReadToEnd();
                }
            }
            Console.WriteLine("responseText = " + responseText);
            JObject jsonObject = JObject.Parse(responseText);
            long longValue = (long)jsonObject["RunId"];
            return longValue;
        }
        public static string StateON(long jobid)
        {
            string API = ignore.state1;
            Console.WriteLine(API);
            string responseText = string.Empty;
            Console.WriteLine("state API로 넘어온 값 : " + jobid);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(API + $"?JobId={jobid}");
            webRequest.Method = "PUT";
            webRequest.Timeout = 30 * 1000;
            webRequest.ContentType = "application/json";
           
            //Data를 잘 받았는지 확인하는 response 구간
            //응답 받기
            using (HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse())
            {
                HttpStatusCode status = resp.StatusCode;
                Console.WriteLine(status);      // status 가 정상일경우 OK가 입력된다.

                // 응답과 관련된 stream을 가져온다.
                Stream respStream = resp.GetResponseStream();
                using (StreamReader streamReader = new StreamReader(respStream))
                {
                    responseText = streamReader.ReadToEnd();
                }
            }
            return responseText;
        }
        public static string StateOFF(long jobid)
        {
            string API = ignore.state0;
            string responseText = string.Empty;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(API + $"?JobId={jobid}");
            webRequest.Method = "PUT";
            webRequest.Timeout = 30 * 1000;
            webRequest.ContentType = "application/json";
            Console.WriteLine(jobid + "jobid 넘기는거 성공");

            //Data를 잘 받았는지 확인하는 response 구간
            //응답 받기
            using (HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse())
            {
                HttpStatusCode status = resp.StatusCode;
                Console.WriteLine(status);      // status 가 정상일경우 OK가 입력된다.

                // 응답과 관련된 stream을 가져온다.
                Stream respStream = resp.GetResponseStream();
                using (StreamReader streamReader = new StreamReader(respStream))
                {
                    responseText = streamReader.ReadToEnd();
                }
            }
            return responseText;
        }

        public static string PutStartDT(long RunId, DateTime StartDT, string WorkFlowName, string state)
        {
            string API = ignore.sendAPI;
            string responseText = string.Empty;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(API+$"/{RunId}");
            webRequest.Method = "PUT";
            webRequest.Timeout = 30 * 1000;
            webRequest.ContentType = "application/json";

            var obj = new RunAssign
            {
                RunId = RunId,
                StartDT = StartDT,
                WorkFlowName = WorkFlowName,
                state= state
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            //보낼 데이터를 byteArray로 바꿔준다.
            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            //요청 Data를 쓰는데 사용할 Stream 개체를 가져온다.
            Stream dataStream = webRequest.GetRequestStream();
            //전송...
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //Data를 잘 받았는지 확인하는 response 구간
            //응답 받기
            using (HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse())
            {
                HttpStatusCode status = resp.StatusCode;
                Console.WriteLine("status = " + status);      // status 가 정상일경우 OK가 입력된다.

                // 응답과 관련된 stream을 가져온다.
                Stream respStream = resp.GetResponseStream();
                using (StreamReader streamReader = new StreamReader(respStream))
                {
                    responseText = streamReader.ReadToEnd();
                }
            }
            return "PUT StartDT Sucess";
        }
        public static string PutEndDT(long RunId, string WorkFlowName , DateTime StartDT ,DateTime EndDT, string ResultData, string state)
        {
            string API = ignore.sendAPI;
            string responseText = string.Empty;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(API + $"/{RunId}");
            webRequest.Method = "PUT";
            webRequest.Timeout = 30 * 1000;
            webRequest.ContentType = "application/json";

            var obj = new RunAssign
            {
                RunId = RunId,
                WorkFlowName = WorkFlowName,
                StartDT = StartDT,
                EndDT = EndDT,
                ResultData = ResultData,
                state = state
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            //보낼 데이터를 byteArray로 바꿔준다.
            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            //요청 Data를 쓰는데 사용할 Stream 개체를 가져온다.
            Stream dataStream = webRequest.GetRequestStream();
            //전송...
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //Data를 잘 받았는지 확인하는 response 구간
            //응답 받기
            using (HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse())
            {
                HttpStatusCode status = resp.StatusCode;
                Console.WriteLine("status = " + status);      // status 가 정상일경우 OK가 입력된다.

                // 응답과 관련된 stream을 가져온다.
                Stream respStream = resp.GetResponseStream();
                using (StreamReader streamReader = new StreamReader(respStream))
                {
                    responseText = streamReader.ReadToEnd();
                }
            }
            return "PUT EndDT Sucess";
        }
    }
}