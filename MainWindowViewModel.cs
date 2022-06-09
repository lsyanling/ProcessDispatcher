using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ProcessDispatcher
{
    public class MainWindowViewModel : ObservableObject
    {
        public MainWindowViewModel()
        {
            AddProcessCommand = new()
            {
                Action = AddProcess
            };

            DeleteProcessCommand = new()
            {
                Action = DeleteProcess
            };

            RandomInputCommand = new()
            {
                Action = RandomInput
            };

            StartRunCommand = new()
            {
                Action = StartRun
            };

            Processes = new();

            DefaultProcesses();

            ProcessDatas = new();
        }

        private void DefaultProcesses()
        {
            for (int i = 0; i < 5; i++)
                Processes.Add(new(i));
            Processes[0].ComeTime = "0";
            Processes[1].ComeTime = "2";
            Processes[2].ComeTime = "4";
            Processes[3].ComeTime = "6";
            Processes[4].ComeTime = "8";
            Processes[0].SustainTime = "3";
            Processes[1].SustainTime = "6";
            Processes[2].SustainTime = "4";
            Processes[3].SustainTime = "5";
            Processes[4].SustainTime = "2";
        }

        private int selectedIndex;

        public int SelectedIndex
        {
            get => selectedIndex;
            set => SetProperty(ref selectedIndex, value);
        }

        public MyCommand AddProcessCommand { get; set; }
        public MyCommand DeleteProcessCommand { get; set; }
        public MyCommand RandomInputCommand { get; set; }
        public MyCommand StartRunCommand { get; set; }

        public ObservableCollection<Process> Processes { get; set; }

        public ObservableCollection<ProcessData> ProcessDatas { get; set; }

        private void AddProcess(object? obj)
        {
            if (Processes.Count <= 12)
            {
                Processes.Add(new(Processes.Count));
            }
        }

        private void DeleteProcess(object? obj)
        {
            if (Processes.Count > 1)
            {
                Processes.RemoveAt(Processes.Count - 1);
            }
        }

        private void RandomInput(object? obj)
        {
            Random random = new();
            int randomRange = 30;
            foreach (var item in Processes)
            {
                item.ComeTime = random.Next(0, randomRange).ToString();
                item.SustainTime = random.Next(1, randomRange).ToString();
            }
        }

        private void StartRun(object? obj)
        {
            ProcessDatas.Clear();
            try
            {
                switch (SelectedIndex)
                {
                    case 0:
                        FCFS();
                        break;
                    case 1:
                        RR();
                        break;
                    case 2:
                        SJF();
                        break;
                    case 3:
                        HRN();
                        break;
                    default:
                        break;
                }
        }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
}

        private void HRN()
        {
            List<ProcessData> processes = new();
            foreach (var item in Processes)
            {
                processes.Add(new(item));
            }

            processes.Sort(new CompareByComeTimeSustainTime());

            SortProcessDatas sort = new();

            processes[0].StartTime = processes[0].ComeTime;
            CalculateOneTime(processes[0]);
            // 现在是第 i 次调度
            for (int i = 1; i < processes.Count; i++)
            {
                int j = i + 1;
                // 要考虑哪些进程
                for (; j < processes.Count; j++)
                    if (processes[j].ComeTime <= processes[i - 1].CompleteTime)
                    {
                        j++;
                        break;
                    }

                sort.SortByResponse(processes, i, j, processes[i - 1].CompleteTime);

                processes[i].StartTime = processes[i - 1].CompleteTime > processes[i].ComeTime
                    ? processes[i - 1].CompleteTime : processes[i].ComeTime;
                CalculateOneTime(processes[i]);
            }

            foreach (var item in processes)
            {
                ProcessDatas.Add(item);
            }
        }

        private void SJF()
        {
            List<ProcessData> processes = new();
            foreach (var item in Processes)
            {
                processes.Add(new(item));
            }

            processes.Sort(new CompareByComeTimeSustainTime());

            SortProcessDatas sort = new();

            processes[0].StartTime = processes[0].ComeTime;
            CalculateOneTime(processes[0]);
            // 现在是第 i 次调度
            for (int i = 1; i < processes.Count; i++)
            {
                int j = i;
                // 要考虑哪些进程
                for (; j < processes.Count; j++)
                    if (processes[j].ComeTime > processes[i - 1].CompleteTime)
                        break;

                sort.SortBySustainTime(processes, i, j);

                processes[i].StartTime = processes[i - 1].CompleteTime > processes[i].ComeTime
                    ? processes[i - 1].CompleteTime : processes[i].ComeTime;
                CalculateOneTime(processes[i]);
            }

            foreach (var item in processes)
            {
                ProcessDatas.Add(item);
            }
        }

        private void RR()
        {
            List<ProcessData> processes = new();
            Dictionary<ProcessData, int> runStatus = new();
            Dictionary<ProcessData, int> runTime = new();
            foreach (var item in Processes)
            {
                processes.Add(new(item));
            }

            processes.Sort(new CompareByComeTimeSustainTime());

            foreach (var item in processes)
            {
                runStatus.Add(item, 0);
                runTime.Add(item, 0);
            }

            int n = processes.Count;
            Queue<ProcessData> runningProcesses = new();

            int time = 0, allTime = 0;
            for (int i = 0; i < n; i++)
            {
                allTime += processes[i].ComeTime;
                allTime += processes[i].SustainTime;
            }

            // 0表示未到达 1表示排队 2表示运行完毕
            while (time < allTime)
            {
                foreach (var item in runStatus)
                {
                    if (item.Value == 0 && item.Key.ComeTime == time)
                    {
                        runStatus[item.Key] = 1;
                        runningProcesses.Enqueue(item.Key);
                    }
                }

                if (runningProcesses.Count > 0 &&
                    runTime[runningProcesses.First()] == runningProcesses.First().SustainTime)
                {
                    runStatus[runningProcesses.First()] = 2;
                    runningProcesses.First().CompleteTime = time;
                    CalculateRRTime(runningProcesses.First());
                    runningProcesses.Dequeue();
                }
                else if (runningProcesses.Count > 0)
                {
                    runningProcesses.Enqueue(runningProcesses.Dequeue());
                }

                if (runningProcesses.Count > 0)
                {
                    runTime[runningProcesses.First()]++;
                }

                time++;
            }

            processes.Sort(new CompareByCompleteTime());

            foreach (var item in processes)
            {
                ProcessDatas.Add(item);
            }
        }

        private void FCFS()
        {
            List<ProcessData> processes = new();
            foreach (var item in Processes)
            {
                processes.Add(new(item));
            }

            // 按到达时间排序
            processes.Sort(new CompareByComeTime());

            processes[0].StartTime = processes[0].ComeTime;
            CalculateOneTime(processes[0]);
            for (int i = 1; i < processes.Count; i++)
            {
                processes[i].StartTime = processes[i - 1].CompleteTime > processes[i].ComeTime
                    ? processes[i - 1].CompleteTime : processes[i].ComeTime;
                CalculateOneTime(processes[i]);
            }

            foreach (var item in processes)
            {
                ProcessDatas.Add(item);
            }
        }

        private void CalculateOneTime(ProcessData processData)
        {
            processData.CompleteTime = processData.StartTime + processData.SustainTime;
            CalculateRRTime(processData);
        }

        private void CalculateRRTime(ProcessData processData)
        {
            processData.strCompleteTime = processData.CompleteTime.ToString();
            processData.TurnOverTime = processData.CompleteTime - processData.ComeTime;
            processData.strTurnOverTime = processData.TurnOverTime.ToString();
            processData.WeightedTurnOverTime = (float)processData.TurnOverTime / processData.SustainTime;
            processData.strWeightedTurnOverTime = processData.WeightedTurnOverTime.ToString();
        }
    }

    class SortProcessDatas
    {
        public void SortBySustainTime(List<ProcessData> P, int begin, int end)
        {
            int min = MinSustainTime(P, begin, end);
            ProcessData temp = P[min];
            P[min] = P[begin];
            P[begin] = temp;
        }

        public int MinSustainTime(List<ProcessData> P, int begin, int end)
        {
            int min = begin;
            for (int i = begin; i < end; i++)
            {
                if (P[i].SustainTime < P[min].SustainTime)
                {
                    min = i;
                }
                else if (P[i].SustainTime == P[min].SustainTime)
                {
                    if (P[i].ComeTime<P[min].ComeTime)
                    {
                        min = i;
                    }
                }
            }
            return min;
        }

        public void SortByResponse(List<ProcessData> P, int begin, int end, int now)
        {
            if (begin == end)
                return;

            int max = MaxResponse(P, begin, end, now);
            ProcessData temp = P[max];
            P[max] = P[begin];
            P[begin] = temp;
            SortByResponse(P, begin + 1, end, now);
        }

        public int MaxResponse(List<ProcessData> P, int begin, int end, int now)
        {
            int max = begin;
            float maxResponse = 0;
            for (int i = begin; i < end; i++)
            {
                float response = (float)(P[i].SustainTime + now - P[i].ComeTime) / P[i].SustainTime;
                if (response > maxResponse)
                {
                    max = i;
                    maxResponse = response;
                }
            }
            return max;
        }
    }

    class CompareByComeTimeSustainTime : IComparer<ProcessData>
    {
        public int Compare(ProcessData? x, ProcessData? y)
        {
            if (x?.ComeTime > y?.ComeTime)
                return 1;
            else if (x?.ComeTime < y?.ComeTime)
                return -1;
            else if (x?.SustainTime > y?.SustainTime)
                return 1;
            else if (x?.SustainTime < y?.SustainTime)
                return -1;
            else if (x?.Pid > y?.Pid)
                return 1;
            else
                return -1;
        }
    }

    class CompareByCompleteTime : IComparer<ProcessData>
    {
        public int Compare(ProcessData? x, ProcessData? y)
        {
            if (x?.CompleteTime > y?.CompleteTime)
                return 1;
            else if (x?.CompleteTime < y?.CompleteTime)
                return -1;
            else
                return 0;
        }
    }

    class CompareByComeTime : IComparer<ProcessData>
    {
        public int Compare(ProcessData? x, ProcessData? y)
        {
            if (x?.ComeTime > y?.ComeTime)
                return 1;
            else if (x?.ComeTime < y?.ComeTime)
                return -1;
            else if (x?.Pid > y?.Pid)
                return 1;
            else
                return -1;
        }
    }

    public class ProcessData
    {
        public ProcessData(Process p)
        {
            strPid = p.Pid;
            Pid = int.Parse(strPid[2..]);
            ComeTime = int.Parse(p.ComeTime);
            SustainTime = int.Parse(p.SustainTime);
        }

        public int Pid;
        public string strPid { get; set; }
        public int ComeTime;
        public int SustainTime;
        public int StartTime;
        public int CompleteTime;
        public string strCompleteTime { get; set; }
        public int TurnOverTime;
        public string strTurnOverTime { get; set; }
        public float WeightedTurnOverTime;
        public string strWeightedTurnOverTime { get; set; }
    }

    public class Process : ObservableObject
    {
        public Process(int id)
        {
            Pid = "进程" + id.ToString();
        }

        private string pid;
        public string Pid
        {
            get => pid;
            set => SetProperty(ref pid, value);
        }

        private string comeTime;
        public string ComeTime
        {
            get => comeTime;
            set => SetProperty(ref comeTime, value);
        }

        private string sustainTime;
        public string SustainTime
        {
            get => sustainTime;
            set => SetProperty(ref sustainTime, value);
        }
    }

    public class MyCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Action?.Invoke(parameter);
        }

        public Action<object?>? Action { get; set; }
    }
}
