﻿#region MIT
//  /*The MIT License (MIT)
// 
//  Copyright 2016 lizs lizs4ever@163.com
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//   * */
#endregion
using System;

namespace socket4net
{
    /// <summary>
    ///     launcher arguments
    /// </summary>
    public class LauncherArg : ObjArg
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="monitorEnabled"></param>
        /// <param name="logger"></param>
        /// <param name="id"></param>
        /// <param name="passiveLogicServiceEnabled"></param>
        public LauncherArg(bool monitorEnabled = true, ILog logger = null, Guid? id = null, bool passiveLogicServiceEnabled = false)
            : base(null)
        {
            Logger = logger;
            PassiveLogicServiceEnabled = passiveLogicServiceEnabled;
            Id = id ?? new Guid();
            MonitorEnabled = monitorEnabled;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool PassiveLogicServiceEnabled { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public ILog Logger { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public static LauncherArg Default => new LauncherArg();

        /// <summary>
        ///     performance monitor enabled
        /// </summary>
        public bool MonitorEnabled { get; private set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Launcher : Obj
    {
        /// <summary>
        ///     get launcher singlton instance
        /// </summary>
        public static Launcher Ins { get; private set; }

        /// <summary>
        ///     get logic service
        /// </summary>
        public ILogicService LogicService => GlobalVarPool.Ins.LogicService;

        /// <summary>
        ///     get network service
        /// </summary>
        public INetService NetService => GlobalVarPool.Ins.NetService;

        /// <summary>
        ///    internal called when an Obj is initialized
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            if (Ins != null)
                throw new Exception("Launcher already instantiated!");
            Ins = this;

            var more = arg.As<LauncherArg>();

            // logger
            GlobalVarPool.Ins.Set(GlobalVarPool.NameOfLogger, more.Logger ?? new DefaultLogger());

            // logic service
            var serviceArg = new ServiceArg(this, 10000, 10);
            var logicService = more.PassiveLogicServiceEnabled
                ? New<PassiveLogicService>(serviceArg)
                : (ILogicService)New<AutoLogicService>(serviceArg);
            GlobalVarPool.Ins.Set(GlobalVarPool.NameOfLogicService, logicService);

            // net service
            var netService = New<NetService>(serviceArg);
            GlobalVarPool.Ins.Set(GlobalVarPool.NameOfNetService, netService);

            if (more.MonitorEnabled)
            {
                var monitor = New<PerformanceMonitor>(ObjArg.Empty);
                GlobalVarPool.Ins.Set(GlobalVarPool.NameOfMonitor, monitor);
            }
        }

        /// <summary>
        ///     Invoked when obj started
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            NetService.Start();
            LogicService.Start();

            if (PerformanceMonitor.Ins != null)
                PerformanceMonitor.Ins.Start();
        }

        /// <summary>
        ///    internal called when an Obj is to be destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            NetService.Destroy();
            LogicService.Destroy();
            Logger.Ins.Shutdown();

            if(PerformanceMonitor.Ins != null)
                PerformanceMonitor.Ins.Destroy();

            Ins = null;
        }
    }
}
