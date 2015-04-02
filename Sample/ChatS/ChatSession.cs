using System;
using System.Threading.Tasks;
using socket4net.RPC;
using socket4net.Serialize;
using Proto;

namespace ChatS
{
    public class ChatSession : RpcSession
    {
        public ChatSession()
        {
            ReceiveBufSize = 10 * 1024;
            PackageMaxSize = 40 * 1024;
        }

        public async override Task<Tuple<bool, byte[]>> HandleRequest(ushort route, byte[] param)
        {
            switch ((RpcRoute)route)
            {
                case RpcRoute.GmCmd:
                    {
                        var msg = Serializer.Deserialize<Message2Server>(param);

                        // ��Ӧ������
                        var proto  = new Broadcast2Clients
                        {
                            From = Id.ToString(),
                            Message = "Gm command [" + msg.Message + "] Responsed"
                        };

                        return new Tuple<bool, byte[]>(true, Serializer.Serialize(proto));
                    }

                default:
                    return null;
            }
        }

        public async override Task<bool> HandlePush(ushort route, byte[] param)
        {
            switch ((RpcRoute)route)
            {
                case RpcRoute.Chat:
                    {
                        var msg = Serializer.Deserialize<Message2Server>(param);
                        var proto = new Broadcast2Clients
                        {
                            From = Id.ToString(),
                            Message = msg.Message
                        };

                        // �㲥����������������
                        PushAll((ushort)RpcRoute.Chat, proto);

                        // ����ֻ֪ͨ�Լ�
                        //Session.Push(RpcRoute.Chat, proto);

                        return true;
                    }

                default:
                    return false;
            }
        }
    }
}