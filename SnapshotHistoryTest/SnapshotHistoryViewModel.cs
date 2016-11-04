using AopUtil.WpfBinding;
using Microsoft.Win32;
using SnapshotHistoryTest.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace SnapshotHistoryTest
{
    public class SnapshotHistoryViewModel : ObservableObject
    {
        [AutoNotify]
        public string FileName { get; set; } = @"C:\Users\wangys\Desktop\snapshot.history";
        [AutoNotify]
        public string ReplayInfo { get; set; }
        [AutoNotify]
        public CollectionViewSource HistorySource { get; set; }
        [AutoNotify]
        public CollectionViewSource MuxSource { get; set; }
        [AutoNotify]
        public TargetHistory SelectedHistory { get; set; }
        [AutoNotify]
        public TargetMux SelectedMux { get; set; }
        [AutoNotify]
        public CollectionViewSource SnapshotsSource { get; set; }
        [AutoNotify]
        public SnapshotDisplay SelectedSnapshot { get; set; }

        public ICommand OpenFileDialogCmd { get; set; }
        public ICommand ReadFileCmd { get; set; }
        public ICommand ClearCmd { get; set; }
        public ICommand DisplayAllCmd { get; set; }

        ObservableCollection<TargetHistory> _historys = new ObservableCollection<TargetHistory>();
        ObservableCollection<TargetMux> _muxs = new ObservableCollection<TargetMux>();
        public SnapshotHistoryViewModel()
        {
            SnapshotsSource = new CollectionViewSource();
            HistorySource = new CollectionViewSource();
            HistorySource.Source = _historys;
            HistorySource.Filter += HistorySource_Filter;
            MuxSource = new CollectionViewSource();
            MuxSource.Source = _muxs;
            OpenFileDialogCmd = new DelegateCommand(_ => openFileDialog());
            ReadFileCmd = new DelegateCommand(_ => readFile());
            ClearCmd = new DelegateCommand(_ => clear());
            initFilter();
            DisplayAllCmd = new DelegateCommand(_ => displayAll());
            PropertyChanged += onPropertyChanged;
        }
        
        private void HistorySource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = filter(e.Item);
        }

        private bool filter(object item)
        {
            var his = item as TargetHistory;
            return isAdoptFilter(his, SearchKey?.ToLower());
        }

        private void onPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SelectedHistory):
                    if (SelectedHistory != null)
                    {
                        updateSelectedMux(SelectedHistory.Id);
                        updateSelectedSnapshot(SelectedHistory.Id);
                    }
                    break;
                case nameof(SelectedSnapshot):
                    if (SelectedSnapshot != null)
                        updateSelectedHistory(SelectedSnapshot.ID);
                    break;
                case nameof(IsFilterFromTime):
                    searchKey();
                    break;
                case nameof(StartTime):
                case nameof(EndTime):
                    if(IsFilterFromTime)
                        searchKey();
                    break;
            }
        }

        private void updateSelectedHistory(string id)
        {
            SelectedHistory = _historys.FirstOrDefault(_ => _.Id.Equals(id));
        }

        private void updateSelectedMux(string id)
        {
            SelectedMux = _muxs.FirstOrDefault(_ => _.Id1.Equals(id) || _.Id2.Equals(id));
        }

        private void updateSelectedSnapshot(string id)
        {
            if (SelectedSnapshot == null || !SelectedSnapshot.ID.Equals(id))
            {
                SelectedSnapshot = (SnapshotsSource.Source as ObservableCollection<SnapshotDisplay>).FirstOrDefault(_ => _.ID.Equals(id));
            }
        }

        private void openFileDialog()
        {
            OpenFileDialog open = new OpenFileDialog();
            if (!string.IsNullOrWhiteSpace(FileName))
                open.FileName = FileName;
            open.Filter = "历史文件(*.history)|*.history|所有文件(*.*)|*.*";//设置打开文件类型
            if (open.ShowDialog().Value)
            {
                FileName = open.FileName;
            }
        }

        private void readFile()
        {
            try
            {
                HistorySource.View.MoveCurrentTo(null);
                //SelectedHistory = null;
                var hist = SnapshotHistoryJson.ReadHistory(FileName);
                foreach (var ss in hist.History)
                    add(ss);
                foreach (var mux in hist.Mux)
                    addMux(mux);
                HistorySource.View.Refresh();
                updateReplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void add(SnapshotInfo ssInfo)
        {
            var first = _historys.FirstOrDefault(_ => _.Id == ssInfo.Ship.Id);
            if (first == null)
                _historys.Add(new TargetHistory(ssInfo));
            else
                first.Add(ssInfo);
        }

        private void addMux(TargetMux mux)
        {
            if (!_muxs.Any(_ => _.Id1 == mux.Id1 && _.Id2 == mux.Id2 || _.Id1 == mux.Id2 && _.Id2 == mux.Id1))
                _muxs.Add(mux);
        }

        private void clear()
        {
            HistorySource.View.MoveCurrentTo(null);
            //SelectedHistory = null;
            MuxSource.View.MoveCurrentTo(null);
            _historys.Clear();
            _muxs.Clear();
            updateReplay();
            //ReplayInfo = null;
        }
        private void displayAll()
        {
            HistorySource.View.MoveCurrentTo(null);
            SelectedHistory = null;
            updateReplay();
        }

        private void updateReplay(string key = null)
        {
            key = key?.ToLower();
            List<Point> pts = new List<Point>();
            List<SnapshotInfo> infos = new List<SnapshotInfo>();
            foreach (var his in _historys)
            {
                if (isAdoptKeyFilter(his, key))
                    infos.AddRange(his.SnapshotInfos().ToList().FindAll(info=> isAdoptTimeFilter(info)));
            }
            SnapshotsSource.Source = new ObservableCollection<SnapshotDisplay>(infos.Select(_ => new SnapshotDisplay(_)).ToList());
        }

        private bool isAdoptFilter(TargetHistory his, string key)
        {
            return isAdoptKeyFilter(his, key) && his.SnapshotInfos().Any(info => isAdoptTimeFilter(info));
        }

        private bool isAdoptKeyFilter(TargetHistory his, string key)
        {
            if (his == null)
                return false;
            if (string.IsNullOrWhiteSpace(key))
                return true;
            if ((his.Id.ToLower().Contains(key) || his.名称.ToLower().Contains(key)))
                return true;
            return false;
        }
        
        private bool isAdoptTimeFilter(SnapshotInfo his)
        {
            if (!IsFilterFromTime)
                return true;
            if (his.SnapshotTime >= StartTime && his.SnapshotTime < EndTime)
                return true;
            return false;
        }
        #region Search
        [AutoNotify]
        public string SearchKey { get; set; }
        public ICommand SearchCmd { get; set; }

        void initFilter()
        {
            SearchCmd = new DelegateCommand(_ => searchKey());
        }

        private void searchKey()
        {
            HistorySource.View.Refresh();
            updateReplay(SearchKey);
        }

        [AutoNotify]
        public DateTime StartTime { get; set; } = new DateTime(2016, 1, 1);

        [AutoNotify]
        public DateTime EndTime { get; set; } = DateTime.Now.Date.AddDays(1);

        [AutoNotify]
        public bool IsFilterFromTime { get; set; }

        #endregion Search

    }
}
