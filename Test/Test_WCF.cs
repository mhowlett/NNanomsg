using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;

namespace Test
{
    class Test_WCF
    {
        const bool UsePipes = true;
        public static Binding Binding = UsePipes ? (Binding)MakePipeBinding() : MakeTcpBinding();
        public static Uri ServiceUri = new Uri(UsePipes ? "net.pipe://localhost/" : "net.tcp://localhost:18180/");
        public static string Service = "helloworld";

        static byte[] _data;
        public static void Execute()
        {
            _data = new byte[TestConstants.DataSize];
            const int iter = TestConstants.Iterations;
            new Random().NextBytes(_data);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep(1000);
                try
                {
                    using (var c = new ServiceProxy())
                        while (true)
                        {

                            var sw = Stopwatch.StartNew();
                            for (int i = 0; i < iter; i++)
                                c.Call(_data);
                            sw.Stop();
                            Console.WriteLine("{0} us, {1}/s, {2} mb/s",
                                (int)(1000d * sw.Elapsed.TotalMilliseconds / iter),
                                (int)(iter / sw.Elapsed.TotalSeconds),
                                (int)(2d * _data.Length * iter / sw.Elapsed.TotalSeconds / 1024d / 1024d));
                        }
                }
                catch { }// exit when the serice is gone
            });
            var service = new Service();
            Console.WriteLine("WCF Test " + ServiceUri.ToString());
            using (var serviceHost = new ServiceHost(typeof(Service), ServiceUri))
            {
                serviceHost.AddServiceEndpoint(typeof(IService), Binding, Service);
                serviceHost.Open();

                Thread.Sleep(10 * 1000);
            }
            Console.WriteLine("End WCF Test " + ServiceUri.ToString());
        }

        public static NetNamedPipeBinding MakePipeBinding()
        {
            var b = new NetNamedPipeBinding()
            {
                MaxReceivedMessageSize = 12800 * 1024,
                Security = new NetNamedPipeSecurity() { Mode = NetNamedPipeSecurityMode.None }
            };
            b.ReaderQuotas.MaxArrayLength = (int)b.MaxReceivedMessageSize;
            return b;
        }

        public static NetTcpBinding MakeTcpBinding()
        {
            var b = new NetTcpBinding()
            {
                MaxReceivedMessageSize = 12800 * 1024,
                Security = new NetTcpSecurity() { Mode = SecurityMode.None }
            };
            b.ReaderQuotas.MaxArrayLength = (int)b.MaxReceivedMessageSize;
            return b;
        }
    }

    [ServiceContract]
    interface IService
    {
        [OperationContract]
        byte[] Call(byte[] data);
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false, InstanceContextMode = InstanceContextMode.Single)]
    class Service : IService
    {
        public byte[] Call(byte[] data)
        {
            return data;
        }
    }

    class ServiceProxy : ClientBase<IService>
    {
        public ServiceProxy()
            : base(new ServiceEndpoint(ContractDescription.GetContract(typeof(IService)),
                Test_WCF.Binding,
                new EndpointAddress(Test_WCF.ServiceUri.ToString() + Test_WCF.Service)))
        {
        }

        public byte[] Call(byte[] data)
        {
            return Channel.Call(data);
        }
    }
}
