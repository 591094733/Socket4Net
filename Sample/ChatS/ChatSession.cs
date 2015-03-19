using Core.RPC;
using Core.Serialize;
using Proto;

namespace ChatS
{
    public class ChatSession : RpcSession
    {
        public override object HandleRequest(short route, byte[] param)
        {
            switch ((RpcRoute)route)
            {
                case RpcRoute.GmCmd:
                    {
                        var msg = Serializer.Deserialize<Message2Server>(param);

                        // ����Request���󣬻���ProtoBufʵ��
                        // ��ʵ�����ձ��Զ������߽���
                        return new Broadcast2Clients
                        {
                            From = Id.ToString(),
                            Message = "Gm command [" + msg.Message + "] Responsed"
                        };
                    }

                default:
                    return null;
            }
        }

        public override bool HandleNotify(short route, byte[] param)
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
                        NotifyAll((short)RpcRoute.Chat, proto);

                        // ����ֻ֪ͨ�Լ�
                        //Session.Notify(RpcRoute.Chat, proto);

                        return true;
                    }

                default:
                    return false;
            }
        }
    }
}