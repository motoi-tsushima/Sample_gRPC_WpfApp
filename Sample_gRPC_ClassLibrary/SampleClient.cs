using System;
using Grpc.Net.Client;

namespace Sample_gRPC_ClassLibrary
{
    public class SampleClient
    {
        public SampleClient()
        {
            this.serviceUrl = "https://localhost:5001";
            this.grpcChannel = GrpcChannel.ForAddress(this.serviceUrl);
            this.greeterClient = new Greeter.GreeterClient(this.grpcChannel);
        }

        private string serviceUrl = null;
        private GrpcChannel grpcChannel = null;
        private Greeter.GreeterClient greeterClient = null;

        public GrpcChannel Channel
        {
            get { return this.grpcChannel; }
        }

        public Greeter.GreeterClient GreeterClient
        {
            get { return this.greeterClient; }
        }
    }
}
