﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    public abstract class Block<TKey> : IBlock<TKey>
    {
        public const int InvalidIndex = -1;

        private string _redisFeild;
        private IBlockOps _ops;
        public virtual bool Dirty { get; set; }
        public TKey Id { get; set; }
        public object Value { get; protected set; }
        public EBlockMode Mode { get; protected set; }
        public Type Type { get; protected set; }

        /// <summary>
        ///     是否需即时持久
        /// </summary>
        public bool Persistable
        {
            get { return Mode >= EBlockMode.RealtimePersistable; }
        }

        /// <summary>
        ///     是否需同步
        /// </summary>
        public bool Synchronizable
        {
            get { return Mode >= EBlockMode.Synchronizable; }
        }

        public IBlockOps Ops
        {
            get { return _ops; }
            protected set
            {
                _ops = value;
                if (_ops != null)
                    Dirty = true;
            }
        }

        public abstract EBlockType EBlockType { get; }
        public PropertyBody<TKey> Host { get; private set; }

        public void SetMode(EBlockMode mode)
        {
            Mode = mode;
        }

        public string RedisFeild
        {
            get { return _redisFeild ?? (_redisFeild = string.Format("{0}:{1}", Host.RedisFeild, Id)); }
        }

        protected Block(PropertyBody<TKey> host, TKey id, object value, Type type, EBlockMode mode)
        {
            Host = host;
            Id = id;
            Value = value;
            Mode = mode;
            Type = type;
        }
        
        public bool Is<TItem>()
        {
            return Is(typeof (TItem));
        }

        public bool Is(Type type)
        {
            return Type == type || type.IsSubclassOf(Type);
        }

        public virtual TItem As<TItem>()
        {
            return (TItem)Value;
        }

        public List<TItem> AsList<TItem>()
        {
            if (!(this is IListBlock<TKey,TItem>))
            {
                Logger.Instance.ErrorFormat("尝试Cast一个非List，Id : {0}", Id);
                return null;
            }

            var lst = (List<ListItemRepresentation<TItem>>) Value;
            return lst.Select(x => x.Item).ToList();
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", Id, Value ?? "");
        }

        public void Dispose()
        {
        }

        public abstract byte[] Serialize();
        public abstract void Deserialize(byte[] proto);
    }

    public abstract class Block<TKey, TItem> : Block<TKey>
    {
        protected Block(PropertyBody<TKey> host, TKey id, TItem value, EBlockMode mode)
            : base(host, id, value, typeof(TItem), mode)
        {
        }

        public TItem Get()
        {
            return (TItem)Value;
        }

        protected void InternalSet(TItem value)
        {
            InternalSet((object)value);
        }

        protected virtual void InternalSet(object value)
        {
            Value = value;
            Dirty = true;
        }

        public override byte[] Serialize()
        {
            return PiSerializer.SerializeValue((TItem)Value);
        }

        public override void Deserialize(byte[] bytes)
        {
            if(bytes == null) return;
            Value = PiSerializer.DeserializeValue<TItem>(bytes);
        }
    }
}