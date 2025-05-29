using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ERad5TestGUI.Components
{
    /// <summary>
    /// 高性能 WPF 日志 Appender
    /// 功能特性：
    /// 1. 支持绑定到任何 ItemsControl（ListBox/ListView等）
    /// 2. 线程安全的异步处理
    /// 3. 自动滚动和日志数量控制
    /// 4. 内置默认日志格式（支持颜色）
    /// </summary>
    public class UiLogAppender : AppenderSkeleton
    {
        #region 配置属性
        /// <summary>
        /// 最大保留日志条数（默认2000）
        /// </summary>
        public int MaxLogItems { get; set; } = 2000;

        /// <summary>
        /// 是否自动滚动到底部（默认true）
        /// </summary>
        public bool AutoScrollToEnd { get; set; } = true;

        /// <summary>
        /// 日志格式布局（默认带颜色的模式）
        /// </summary>
        public PatternLayout Layout { get; set; } = new PatternLayout(
            "%date{HH:mm:ss.fff} [%level] %logger - %message");
        #endregion

        #region 内部字段
        private readonly BlockingCollection<LoggingEvent> _logQueue = new BlockingCollection<LoggingEvent>(5000);
        private WeakReference<ItemsControl> _logControlRef;
        private CancellationTokenSource _cts;
        private bool _isInitialized;
        #endregion

        #region 公共属性
        /// <summary>
        /// 绑定的UI控件（ListBox/ListView等）
        /// </summary>
        public ItemsControl LogControl
        {
            get => _logControlRef?.TryGetTarget(out var target) == true ? target : null;
            set
            {
                if (value != null)
                {
                    _logControlRef = new WeakReference<ItemsControl>(value);
                    InitializeItemsSource(value);
                }
            }
        }
        #endregion

        #region 核心方法

        // 在 UiLogAppender 类中添加以下方法
        private void InitializeItemsSource(ItemsControl control)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (control.ItemsSource == null)
                {
                    control.ItemsSource = new ObservableCollection<string>();
                }

                // 改为通过XAML控制虚拟化
                if (control is ListBox listBox)
                {
                    // 确保支持平滑滚动
                    ScrollViewer.SetCanContentScroll(listBox, true);
                }
            });
        }
        protected override void Append(LoggingEvent loggingEvent)
        {
            if (!_isInitialized || _logQueue.IsAddingCompleted) return;

            try
            {
                // 内存保护：队列超过90%容量时丢弃最旧日志
                if (_logQueue.Count > _logQueue.BoundedCapacity * 0.9)
                {
                    _logQueue.TryTake(out _);
                }

                _logQueue.Add(loggingEvent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UiLogAppender] Append failed: {ex.Message}");
            }
        }

        protected override void OnClose()
        {
            _cts?.Cancel();
            _logQueue.CompleteAdding();
            base.OnClose();
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            if (_isInitialized) return;

            Layout.ActivateOptions();
            _cts = new CancellationTokenSource();
            Task.Run(() => ProcessLogQueue(_cts.Token), _cts.Token);
            _isInitialized = true;
        }
        #endregion

        #region 后台处理
        private async Task ProcessLogQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var logs = new List<LoggingEvent>();
                    while (_logQueue.TryTake(out var log) && logs.Count < 50) // 每批处理50条
                    {
                        logs.Add(log);
                    }

                    if (logs.Count > 0 && LogControl != null)
                    {
                        var formattedLogs = logs.Select(Layout.Format).ToList();
                        UpdateUi(formattedLogs);
                    }

                    await Task.Delay(100, token); // 每100ms处理一次
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[UiLogAppender] Process error: {ex.Message}");
                    await Task.Delay(1000, token);
                }
            }
        }

        private void UpdateUi(List<string> logs)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var control = LogControl;
                if (control == null) return;

                var collection = control.ItemsSource as ObservableCollection<string> ??
                               new ObservableCollection<string>();

                foreach (var log in logs)
                {
                    collection.Add(log);
                    if (collection.Count > MaxLogItems)
                    {
                        collection.RemoveAt(0);
                    }
                }

                if (control.ItemsSource == null)
                {
                    control.ItemsSource = collection;
                }

                // 自动滚动
                if (AutoScrollToEnd && control is ListBox listBox && collection.Count > 0)
                {
                    listBox.ScrollIntoView(collection[collection.Count - 1]);
                }
            }), DispatcherPriority.Background);
        }
        #endregion
    }
}