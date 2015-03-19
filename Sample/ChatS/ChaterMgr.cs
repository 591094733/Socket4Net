using System.Collections.Generic;
using Core.RPC;

namespace ChatS
{
    /// <summary>
    /// Rpcʾ���е������߹�����
    /// </summary>
    public class ChaterMgr
    {
        private readonly Dictionary<long, Chater> Chaters = new Dictionary<long, Chater>();

        public Chater Create(RpcSession session)
        {
            var chater = new Chater(session);
            chater.Boot();
            Chaters.Add(session.Id, chater);

            return chater;
        }

        public void Destroy(long id)
        {
            if (Chaters.ContainsKey(id))
            {
                var chater = Chaters[id];
                chater.Dispose();

                Chaters.Remove(id);
            }
        }
    }
}