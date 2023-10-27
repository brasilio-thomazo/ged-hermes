using br.dev.optimus.hermes.lib.models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace br.dev.optimus.hermes.app.binds
{
    public class DocumentTypeBind : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public DocumentType Data { get; set; } = new DocumentType();
        public string? Name
        {
            get => Data.Name;
            set
            {
                if (Data.Name != value)
                {
                    Data.Name = value;
                    OnNotify();
                }
            }
        }
        public ulong? Id
        {
            get => Data.Id;
            set
            {
                if (Data.Id != value)
                {
                    Data.Id = value;
                    OnNotify();
                }
            }
        }

        private void OnNotify([CallerMemberName] string? property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
