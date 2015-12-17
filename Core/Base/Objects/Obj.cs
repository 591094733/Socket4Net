﻿using System;

namespace socket4net
{
    /// <summary>
    ///     接口仅作代码组织用
    /// </summary>
    public interface IObj
    {
        int InstanceId { get; }

        /// <summary>
        ///     名字
        /// </summary>
       string Name { get; }

        /// <summary>
        ///     调度优先级（类似Unity中的Layer）
        /// </summary>
        int Priority { get; }

        /// <summary>
        ///     比较
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        int CompareTo(IObj other);

        void Init(ObjArg arg);
        void Reset();
        void Start();
        void Destroy();

        /// <summary>
        ///     获取根
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetAncestor<T>() where T : class ,IObj;
    }

    public abstract class ObjArg
    {
        public IObj Owner { get; private set; }
        protected ObjArg(IObj owner)
        {
            Owner = owner;
        }

        public static ObjArg Empty
        {
            get { return new EmptyArg(); }
        }
    }

    /// <summary>
    ///     空参数
    /// </summary>
    public class EmptyArg : ObjArg
    {
        public EmptyArg() : base(null)
        {
        }
    }

    public abstract class Obj : IObj
    {
        /// <summary>
        ///     实例动态id
        ///     仅运行时唯一
        /// </summary>
        public int InstanceId { get; private set; }

        /// <summary>
        ///     名字
        /// </summary>
        public virtual string Name
        {
            get { return string.Format("{0}:{1}", GetType().FullName, InstanceId); }
        }

        /// <summary>
        ///     调度优先级（类似Unity中的Layer）
        /// </summary>
        public virtual int Priority
        {
            get { return 0; }
        }

        /// <summary>
        /// 附带自定义数据
        /// </summary>
        protected object UserData { get; set; }
        public T GetUserData<T>()
        {
            return (T) UserData;
        }

        /// <summary>
        /// 设置自定义数据
        /// </summary>
        /// <param name="obj"></param>
        public void SetUserData(object obj)
        {
            UserData = obj;
        }

        /// <summary>
        ///     赋予运行时实例id
        /// </summary>
        protected Obj()
        {
            InstanceId = GenInstanceId();
        }

        /// <summary>
        ///     拥有者
        /// </summary>
        public IObj Owner { get; private set; }

        /// <summary>
        ///     拥有者描述
        /// </summary>
        public string OwnerDescription
        {
            get { return Owner != null ? Owner.Name : "null"; }
        }

        /// <summary>
        ///     是否已初始化
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        ///     是否已启动
        /// </summary>
        public bool Started { get; private set; }

        /// <summary>
        ///     是否已销毁
        /// </summary>
        public bool Destroyed { get; private set; }

        /// <summary>
        ///     是否重置过
        /// </summary>
        public bool Reseted { get; private set; }

        /// <summary>
        ///     根据优先级比较两对象
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(IObj other)
        {
            return Priority.CompareTo(other.Priority);
        }

        /// <summary>
        ///     重写ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        ///     创建对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static T Create<T>(ObjArg arg) where T : IObj, new()
        {
            var ret = new T();
            ret.Init(arg);
            return ret;
        }

        /// <summary>
        ///     初始
        /// </summary>
        public void Init(ObjArg arg)
        {
            if(Initialized)
                throw new Exception("Already initialized");

            // 执行初始化
            OnInit(arg);
            Initialized = true;
        }

        protected virtual void OnInit(ObjArg arg)
        {
            Owner = arg.Owner;
        }
        
        /// <summary>
        ///     启动
        /// </summary>
        public void Start()
        {
            if(!Initialized)
                throw new Exception("Not initialized yet!");

            if (Started)
                throw new Exception("Already started");

            OnStart();
            Started = true;
        }

        protected virtual void OnStart()
        {
        }
        
        /// <summary>
        ///     销毁
        /// </summary>
        public void Destroy()
        {
            if (Destroyed)
                return;

            OnDestroy();
            Destroyed = true;
        }

        protected virtual void OnDestroy()
        {
        }

        /// <summary>
        ///     重置
        /// </summary>
        public void Reset()
        {
            if (Reseted)
                throw new Exception("Already rested");

            OnReset();
            Reseted = true;
        }

        protected virtual void OnReset()
        {
        }

        /// <summary>
        ///     获取指定类型的根（递归获取）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAncestor<T>() where T : class, IObj
        {
            if (this is T) return (this as T);
            if (Owner == null) return null;
            if (Owner is T) return (Owner as T);
            return Owner.GetAncestor<T>();
        }

        /// <summary>
        ///     实例id种子
        /// </summary>
        private static int _seed;
        protected static int GenInstanceId()
        {
            return ++_seed;
        }
    }
}