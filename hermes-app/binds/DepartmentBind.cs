using br.dev.optimus.hermes.lib.models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace br.dev.optimus.hermes.app.binds
{
    public class DepartmentBind : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public Department Data { get; set; } = new Department();

        public ulong? Id
        {
            get => Data.Id; set
            {
                if (value != Data.Id)
                {
                    Data.Id = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string? Name
        {
            get => Data.Name;
            set
            {
                if (value != Data.Name)
                {
                    Data.Name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string? property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

    }
}
