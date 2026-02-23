using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Libra.Agent.Models.Module
{
    public class FileModel : INotifyPropertyChanged
    {
        public string FileName { get; set; }
        public string ChangeDate { get; set; }
        public string Size { get; set; }
        public string Type { get; set; }


        public bool IsFolder => Type == "文件夹";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
